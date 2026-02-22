#!/bin/bash

set -e

# Colors
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

print_header "ProductManagement - Expose Services"

echo "Select exposure method:"
echo "1) Ingress (Recommended - with domain names)"
echo "2) Ingress with TLS/SSL"
echo "3) NodePort (Simple - port-based access)"
echo "4) Port Forwarding (Current sessions only)"
echo "5) All methods"
read -p "Choice (1-5): " choice

case $choice in
    1)
        print_info "Setting up Ingress..."
        ./scripts/install-ingress.sh
        kubectl apply -f k8s/ingress.yaml
        kubectl apply -f k8s/ingress-argocd.yaml
        ;;
    2)
        print_info "Setting up Ingress with TLS..."
        ./scripts/install-ingress.sh
        ./scripts/create-ssl-cert.sh
        kubectl apply -f k8s/ingress-tls.yaml
        kubectl apply -f k8s/ingress-argocd.yaml
        ;;
    3)
        print_info "Setting up NodePort..."
        kubectl apply -f k8s/api-service-nodeport.yaml
        ;;
    4)
        print_info "Starting Port Forwarding..."
        pkill -f "port-forward.*argocd-server" 2>/dev/null || true
        pkill -f "port-forward.*productmanagement-api" 2>/dev/null || true

        kubectl port-forward svc/argocd-server -n argocd 8081:443 > /dev/null 2>&1 &
        kubectl port-forward svc/productmanagement-api-service -n productmanagement 8080:80 > /dev/null 2>&1 &
        ;;
    5)
        print_info "Setting up all methods..."
        ./scripts/install-ingress.sh
        ./scripts/create-ssl-cert.sh
        kubectl apply -f k8s/ingress-tls.yaml
        kubectl apply -f k8s/ingress-argocd.yaml
        kubectl apply -f k8s/api-service-nodeport.yaml

        kubectl port-forward svc/argocd-server -n argocd 8081:443 > /dev/null 2>&1 &
        kubectl port-forward svc/productmanagement-api-service -n productmanagement 8080:80 > /dev/null 2>&1 &
        ;;
    *)
        echo "Invalid choice"
        exit 1
        ;;
esac

print_success "Services exposed!"

# Display access information
print_header "Access Information"

# Get IP
if kubectl get nodes | grep -q "minikube"; then
    IP=$(minikube ip)
else
    IP="localhost"
fi

echo -e "${BLUE}════════════════════════════════════════${NC}"
echo -e "${GREEN}Ingress (Domain-based):${NC}"
echo -e "${BLUE}════════════════════════════════════════${NC}"
echo -e "  API:    http://api.productmanagement.local"
echo -e "  ArgoCD: http://argocd.productmanagement.local"
echo ""
echo -e "${BLUE}════════════════════════════════════════${NC}"
echo -e "${GREEN}NodePort (Direct):${NC}"
echo -e "${BLUE}════════════════════════════════════════${NC}"
echo -e "  API:    http://${IP}:30080"
echo ""
echo -e "${BLUE}════════════════════════════════════════${NC}"
echo -e "${GREEN}Port Forward:${NC}"
echo -e "${BLUE}════════════════════════════════════════${NC}"
echo -e "  API:    http://localhost:8080"
echo -e "  ArgoCD: https://localhost:8081"
echo ""

if [ "$choice" == "1" ] || [ "$choice" == "2" ] || [ "$choice" == "5" ]; then
    echo -e "${YELLOW}Don't forget to update /etc/hosts:${NC}"
    echo "  ${IP} productmanagement.local"
    echo "  ${IP} api.productmanagement.local"
    echo "  ${IP} argocd.productmanagement.local"
fi