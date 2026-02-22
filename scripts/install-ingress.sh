#!/bin/bash

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_header() {
    echo -e "\n${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}\n"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}ℹ $1${NC}"
}

print_header "Installing Nginx Ingress Controller"

# Detect cluster type
if kubectl get nodes | grep -q "minikube"; then
    CLUSTER_TYPE="minikube"
    print_info "Detected Minikube cluster"

    # Enable ingress addon for Minikube
    minikube addons enable ingress

elif kubectl get nodes | grep -q "docker-desktop"; then
    CLUSTER_TYPE="docker-desktop"
    print_info "Detected Docker Desktop cluster"

    # Install Nginx Ingress for Docker Desktop
    kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.2/deploy/static/provider/cloud/deploy.yaml

else
    CLUSTER_TYPE="unknown"
    print_info "Detected unknown cluster - using standard installation"

    # Standard installation
    kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.2/deploy/static/provider/cloud/deploy.yaml
fi

# Wait for ingress controller to be ready
print_info "Waiting for Ingress Controller to be ready..."
kubectl wait --namespace ingress-nginx \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/component=controller \
  --timeout=300s

print_success "Nginx Ingress Controller installed successfully"

# Get LoadBalancer IP/Hostname
print_info "Getting Ingress Controller endpoint..."

if [ "$CLUSTER_TYPE" == "minikube" ]; then
    INGRESS_IP=$(minikube ip)
    print_info "Minikube IP: $INGRESS_IP"
else
    # Wait for LoadBalancer IP
    sleep 10
    INGRESS_IP=$(kubectl get svc ingress-nginx-controller -n ingress-nginx -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "localhost")

    if [ "$INGRESS_IP" == "localhost" ] || [ -z "$INGRESS_IP" ]; then
        print_info "LoadBalancer IP not assigned. Using localhost."
        print_info "You may need to run 'minikube tunnel' or use port forwarding"
    fi
fi

echo ""
echo -e "${GREEN}Ingress Controller Details:${NC}"
echo -e "  IP: ${YELLOW}${INGRESS_IP}${NC}"
echo -e "  HTTP Port: ${YELLOW}80${NC}"
echo -e "  HTTPS Port: ${YELLOW}443${NC}"
echo ""

# Update /etc/hosts
print_header "Updating /etc/hosts"

HOSTS_ENTRIES=(
    "productmanagement.local"
    "api.productmanagement.local"
    "argocd.productmanagement.local"
)

print_info "Adding the following entries to /etc/hosts:"
for entry in "${HOSTS_ENTRIES[@]}"; do
    echo "  $INGRESS_IP $entry"
done

echo ""
read -p "Do you want to update /etc/hosts automatically? (requires sudo) (y/N) " -n 1 -r
echo

if [[ $REPLY =~ ^[Yy]$ ]]; then
    # Backup hosts file
    sudo cp /etc/hosts /etc/hosts.backup.$(date +%Y%m%d_%H%M%S)

    # Remove old entries
    for entry in "${HOSTS_ENTRIES[@]}"; do
        sudo sed -i.bak "/$entry/d" /etc/hosts
    done

    # Add new entries
    echo "" | sudo tee -a /etc/hosts > /dev/null
    echo "# ProductManagement Local Development" | sudo tee -a /etc/hosts > /dev/null
    for entry in "${HOSTS_ENTRIES[@]}"; do
        echo "$INGRESS_IP $entry" | sudo tee -a /etc/hosts > /dev/null
    done

    print_success "/etc/hosts updated successfully"
else
    print_info "Skipped /etc/hosts update"
    echo ""
    echo -e "${YELLOW}Please manually add these entries to your /etc/hosts file:${NC}"
    for entry in "${HOSTS_ENTRIES[@]}"; do
        echo "$INGRESS_IP $entry"
    done
fi

print_header "Ingress Controller Setup Complete! 🎉"