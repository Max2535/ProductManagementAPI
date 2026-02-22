#!/bin/bash

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
NAMESPACE="productmanagement"
APP_NAME="productmanagement"
ARGOCD_NAMESPACE="argocd"
IMAGE_NAME="productmanagement-api"
IMAGE_TAG="latest"

# Functions
print_header() {
    echo -e "\n${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}\n"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}ℹ $1${NC}"
}

check_command() {
    if ! command -v $1 &> /dev/null; then
        print_error "$1 is not installed"
        exit 1
    fi
    print_success "$1 is installed"
}

wait_for_pods() {
    local namespace=$1
    local label=$2
    local timeout=${3:-300}

    print_info "Waiting for pods with label $label in namespace $namespace..."
    kubectl wait --for=condition=ready pod -l $label -n $namespace --timeout=${timeout}s || {
        print_error "Timeout waiting for pods"
        kubectl get pods -n $namespace
        exit 1
    }
    print_success "Pods are ready"
}

# Start deployment
print_header "ProductManagement Local Deployment"

# Step 1: Check prerequisites
print_header "Step 1: Checking Prerequisites"
check_command "kubectl"
check_command "docker"

# Check if Kubernetes is running
if ! kubectl cluster-info &> /dev/null; then
    print_error "Kubernetes cluster is not running"
    print_info "Please start Docker Desktop Kubernetes or Minikube"
    exit 1
fi
print_success "Kubernetes cluster is running"

# Detect cluster type
if kubectl get nodes | grep -q "minikube"; then
    CLUSTER_TYPE="minikube"
    print_info "Detected Minikube cluster"
    # Use Minikube's Docker daemon
    eval $(minikube docker-env)
elif kubectl get nodes | grep -q "docker-desktop"; then
    CLUSTER_TYPE="docker-desktop"
    print_info "Detected Docker Desktop cluster"
else
    CLUSTER_TYPE="unknown"
    print_info "Detected unknown cluster type"
fi

# Step 2: Install ArgoCD
print_header "Step 2: Installing ArgoCD"

if kubectl get namespace $ARGOCD_NAMESPACE &> /dev/null; then
    print_info "ArgoCD namespace already exists"
else
    kubectl create namespace $ARGOCD_NAMESPACE
    print_success "Created ArgoCD namespace"
fi

if kubectl get deployment argocd-server -n $ARGOCD_NAMESPACE &> /dev/null; then
    print_info "ArgoCD is already installed"
else
    print_info "Installing ArgoCD..."
    kubectl apply -n $ARGOCD_NAMESPACE -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml

    print_info "Waiting for ArgoCD to be ready (this may take a few minutes)..."
    kubectl wait --for=condition=available --timeout=600s deployment/argocd-server -n $ARGOCD_NAMESPACE
    print_success "ArgoCD installed successfully"
fi

# Step 3: Build Docker Image
print_header "Step 3: Building Docker Image"

print_info "Building $IMAGE_NAME:$IMAGE_TAG..."
docker build -t $IMAGE_NAME:$IMAGE_TAG -f src/ProductManagement.API/Dockerfile .

if [ $CLUSTER_TYPE == "minikube" ]; then
    print_info "Loading image to Minikube..."
    minikube image load $IMAGE_NAME:$IMAGE_TAG
fi

print_success "Docker image built and loaded"

# Step 4: Create Namespace
print_header "Step 4: Creating Application Namespace"

if kubectl get namespace $NAMESPACE &> /dev/null; then
    print_info "Namespace $NAMESPACE already exists"
else
    kubectl create namespace $NAMESPACE
    print_success "Created namespace $NAMESPACE"
fi

# Step 5: Deploy Infrastructure
print_header "Step 5: Deploying Infrastructure (SQL Server & Redis)"

# Apply all infrastructure manifests
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/sqlserver-pvc.yaml
kubectl apply -f k8s/redis-pvc.yaml
kubectl apply -f k8s/sqlserver-deployment.yaml
kubectl apply -f k8s/sqlserver-service.yaml
kubectl apply -f k8s/redis-deployment.yaml
kubectl apply -f k8s/redis-service.yaml

print_info "Waiting for SQL Server to be ready..."
wait_for_pods $NAMESPACE "app=sqlserver" 300

print_info "Waiting for Redis to be ready..."
wait_for_pods $NAMESPACE "app=redis" 180

print_success "Infrastructure deployed successfully"

# Step 6: Deploy API
print_header "Step 6: Deploying API"

kubectl apply -f k8s/api-deployment.yaml
kubectl apply -f k8s/api-service.yaml

print_info "Waiting for API to be ready..."
wait_for_pods $NAMESPACE "app=productmanagement-api" 300

print_success "API deployed successfully"

# Step 7: Setup ArgoCD Application (Optional)
print_header "Step 7: Registering with ArgoCD"

# Get ArgoCD admin password
ARGOCD_PASSWORD=$(kubectl -n $ARGOCD_NAMESPACE get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d)

print_info "Creating ArgoCD Application manifest..."

# Create ArgoCD application if it doesn't exist
if kubectl get application $APP_NAME -n $ARGOCD_NAMESPACE &> /dev/null; then
    print_info "ArgoCD application already exists"
else
    # Create in-cluster application
    cat <<EOF | kubectl apply -f -
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: $APP_NAME
  namespace: $ARGOCD_NAMESPACE
spec:
  project: default
  source:
    repoURL: https://github.com/Max2535/productmanagement.git
    targetRevision: HEAD
    path: k8s/base
  destination:
    server: https://kubernetes.default.svc
    namespace: $NAMESPACE
  syncPolicy:
    automated:
      prune: false
      selfHeal: true
    syncOptions:
    - CreateNamespace=true
EOF
    print_success "ArgoCD application created"
fi

# Step 8: Expose Services
print_header "Step 8: Exposing Services"
read -p "How do you want to expose services? (1=Ingress, 2=NodePort, 3=Both, 4=Skip): " expose_choice

case $expose_choice in
    1)
        print_info "Setting up Ingress..."
        bash scripts/install-ingress.sh
        kubectl apply -f k8s/ingress.yaml
        kubectl apply -f k8s/ingress-argocd.yaml
        USE_INGRESS=true
        ;;
    2)
        print_info "Setting up NodePort..."
        kubectl apply -f k8s/api-service-nodeport.yaml
        USE_NODEPORT=true
        ;;
    3)
        print_info "Setting up both Ingress and NodePort..."
        bash scripts/install-ingress.sh
        kubectl apply -f k8s/ingress.yaml
        kubectl apply -f k8s/ingress-argocd.yaml
        kubectl apply -f k8s/api-service-nodeport.yaml
        USE_INGRESS=true
        USE_NODEPORT=true
        ;;
    *)
        print_info "Skipping service exposure setup"
        ;;
esac

# Step 9: Port Forwarding (fallback)
print_header "Step 9: Setting up Port Forwarding"

# ... existing port forwarding code ...

# Step 10: Display all access methods
print_header "All Access Methods"

echo -e "${BLUE}════════════════════════════════════════════════════════${NC}"

if [ "$USE_INGRESS" = true ]; then
    echo -e "${GREEN}Via Ingress (Recommended):${NC}"
    echo -e "  📱 API Swagger:    ${YELLOW}http://api.productmanagement.local/swagger${NC}"
    echo -e "  🏥 Health Check:   ${YELLOW}http://api.productmanagement.local/health${NC}"
    echo -e "  🚀 ArgoCD:         ${YELLOW}http://argocd.productmanagement.local${NC}"
    echo ""
fi

if [ "$USE_NODEPORT" = true ]; then
    if kubectl get nodes | grep -q "minikube"; then
        NODE_IP=$(minikube ip)
    else
        NODE_IP="localhost"
    fi
    echo -e "${GREEN}Via NodePort:${NC}"
    echo -e "  📱 API:            ${YELLOW}http://${NODE_IP}:30080${NC}"
    echo ""
fi

echo -e "${GREEN}Via Port Forward:${NC}"
echo -e "  📱 API:            ${YELLOW}http://localhost:8080${NC}"
echo -e "  🚀 ArgoCD:         ${YELLOW}https://localhost:8081${NC}"
echo ""
echo -e "${BLUE}════════════════════════════════════════════════════════${NC}"

# Start port forwarding in background
print_info "Starting port forwarding..."

# Kill existing port-forwards
pkill -f "port-forward.*argocd-server" 2>/dev/null || true
pkill -f "port-forward.*productmanagement-api" 2>/dev/null || true

# ArgoCD UI
kubectl port-forward svc/argocd-server -n $ARGOCD_NAMESPACE 8081:443 > /dev/null 2>&1 &
ARGOCD_PF_PID=$!
print_success "ArgoCD UI: https://localhost:8081"

sleep 2

# API
kubectl port-forward svc/productmanagement-api-service -n $NAMESPACE 8080:80 > /dev/null 2>&1 &
API_PF_PID=$!
print_success "API: http://localhost:8080"

# Step 10: Display Summary
print_header "Deployment Complete! 🎉"

echo -e "${GREEN}"
cat << "EOF"
    ____                 __                             __
   / __ \___  ____  / /___  __  ______ ___  ___  ____  / /_
  / / / / _ \/ __ \/ / __ \/ / / / __ `__ \/ _ \/ __ \/ __/
 / /_/ /  __/ /_/ / / /_/ / /_/ / / / / / /  __/ /_/ / /_
/_____/\___/ .___/_/\____/\__, /_/ /_/ /_/\___/\____/\__/
          /_/            /____/
EOF
echo -e "${NC}"

echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}Access URLs:${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "  📱 API Swagger:        ${YELLOW}http://localhost:8080/swagger${NC}"
echo -e "  🏥 Health Check:       ${YELLOW}http://localhost:8080/health${NC}"
echo -e "  🚀 ArgoCD UI:          ${YELLOW}https://localhost:8081${NC}"
echo -e "  📊 Hangfire Dashboard: ${YELLOW}http://localhost:8080/hangfire${NC}"
echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}ArgoCD Credentials:${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "  Username: ${YELLOW}admin${NC}"
echo -e "  Password: ${YELLOW}$ARGOCD_PASSWORD${NC}"
echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}Useful Commands:${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "  View pods:           ${YELLOW}kubectl get pods -n $NAMESPACE${NC}"
echo -e "  View logs:           ${YELLOW}kubectl logs -f deployment/productmanagement-api -n $NAMESPACE${NC}"
echo -e "  Restart API:         ${YELLOW}kubectl rollout restart deployment/productmanagement-api -n $NAMESPACE${NC}"
echo -e "  Delete all:          ${YELLOW}./scripts/cleanup.sh${NC}"
echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo ""

# Save credentials to file
cat > .deployment-info << EOF
ARGOCD_URL=https://localhost:8081
ARGOCD_USERNAME=admin
ARGOCD_PASSWORD=$ARGOCD_PASSWORD
API_URL=http://localhost:8080
NAMESPACE=$NAMESPACE
ARGOCD_PF_PID=$ARGOCD_PF_PID
API_PF_PID=$API_PF_PID
EOF

print_success "Deployment information saved to .deployment-info"
print_info "Port forwarding processes started in background"
print_info "To stop port forwarding, run: ./scripts/stop-portforward.sh"

echo ""
print_info "Testing API health endpoint..."
sleep 5

if curl -s http://localhost:8080/health > /dev/null; then
    print_success "API is responding!"
else
    print_error "API is not responding yet. It may still be starting up."
    print_info "Check logs: kubectl logs -f deployment/productmanagement-api -n $NAMESPACE"
fi

echo ""