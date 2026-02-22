#!/bin/bash

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
NAMESPACE="productmanagement"
APP_NAME="productmanagement"
ARGOCD_NAMESPACE="argocd"
IMAGE_NAME="productmanagement-api"
IMAGE_TAG="latest"
USE_INGRESS=true
USE_TLS=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --no-ingress)
            USE_INGRESS=false
            shift
            ;;
        --with-tls)
            USE_TLS=true
            shift
            ;;
        *)
            shift
            ;;
    esac
done

# Include all previous functions...
# (Copy from deploy-local.sh)

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

# ... (copy all other functions from deploy-local.sh)

# After Step 6 (Deploy API), add:

# Step 7: Setup Ingress
if [ "$USE_INGRESS" = true ]; then
    print_header "Step 7: Setting up Ingress Controller"

    # Check if Ingress Controller is installed
    if ! kubectl get namespace ingress-nginx &> /dev/null; then
        print_info "Installing Nginx Ingress Controller..."
        ./scripts/install-ingress.sh
    else
        print_info "Ingress Controller already installed"
    fi

    # Create TLS certificates if requested
    if [ "$USE_TLS" = true ]; then
        print_info "Creating SSL certificates..."
        ./scripts/create-ssl-cert.sh

        print_info "Applying Ingress with TLS..."
        kubectl apply -f k8s/ingress-tls.yaml
        kubectl apply -f k8s/ingress-argocd.yaml
    else
        print_info "Applying Ingress..."
        kubectl apply -f k8s/ingress.yaml
        kubectl apply -f k8s/ingress-argocd.yaml
    fi

    print_success "Ingress configured successfully"

    # Get Ingress IP
    if kubectl get nodes | grep -q "minikube"; then
        INGRESS_IP=$(minikube ip)
    else
        INGRESS_IP="localhost"
    fi

    # Display access URLs
    print_header "Access Information"

    echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
    echo -e "${GREEN}Via Ingress:${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"

    if [ "$USE_TLS" = true ]; then
        echo -e "  📱 Main Site:          ${YELLOW}https://productmanagement.local${NC}"
        echo -e "  📱 API:                ${YELLOW}https://api.productmanagement.local${NC}"
        echo -e "  📱 Swagger:            ${YELLOW}https://api.productmanagement.local/swagger${NC}"
        echo -e "  🏥 Health:             ${YELLOW}https://api.productmanagement.local/health${NC}"
        echo -e "  🚀 ArgoCD:             ${YELLOW}https://argocd.productmanagement.local${NC}"
    else
        echo -e "  📱 Main Site:          ${YELLOW}http://productmanagement.local${NC}"
        echo -e "  📱 API:                ${YELLOW}http://api.productmanagement.local${NC}"
        echo -e "  📱 Swagger:            ${YELLOW}http://api.productmanagement.local/swagger${NC}"
        echo -e "  🏥 Health:             ${YELLOW}http://api.productmanagement.local/health${NC}"
        echo -e "  🚀 ArgoCD:             ${YELLOW}http://argocd.productmanagement.local${NC}"
    fi

    echo ""
    echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
    echo -e "${GREEN}Direct Access (NodePort):${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
    echo -e "  📱 API:                ${YELLOW}http://${INGRESS_IP}:30080${NC}"
    echo ""

else
    # Use port forwarding (original method)
    print_header "Step 7: Setting up Port Forwarding"

    # ... (original port forwarding code)
fi

# Add hosts file instructions
if [ "$USE_INGRESS" = true ]; then
    echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
    echo -e "${GREEN}Hosts File Configuration:${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
    echo -e "${YELLOW}Add these entries to /etc/hosts:${NC}"
    echo ""
    echo -e "  ${INGRESS_IP} productmanagement.local"
    echo -e "  ${INGRESS_IP} api.productmanagement.local"
    echo -e "  ${INGRESS_IP} argocd.productmanagement.local"
    echo ""
fi

print_header "Deployment Complete! 🎉"