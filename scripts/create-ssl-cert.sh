#!/bin/bash

set -e

NAMESPACE="productmanagement"
CERT_DIR="./certs"

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${YELLOW}Creating self-signed SSL certificate...${NC}"

# Create cert directory
mkdir -p $CERT_DIR

# Generate private key
openssl genrsa -out $CERT_DIR/tls.key 2048

# Generate certificate signing request
cat > $CERT_DIR/csr.conf << EOF
[req]
default_bits = 2048
prompt = no
default_md = sha256
distinguished_name = dn
req_extensions = v3_req

[dn]
C = TH
ST = Bangkok
L = Bangkok
O = ProductManagement
OU = Development
CN = productmanagement.local

[v3_req]
subjectAltName = @alt_names

[alt_names]
DNS.1 = productmanagement.local
DNS.2 = api.productmanagement.local
DNS.3 = argocd.productmanagement.local
DNS.4 = *.productmanagement.local
EOF

# Generate CSR
openssl req -new -key $CERT_DIR/tls.key -out $CERT_DIR/tls.csr -config $CERT_DIR/csr.conf

# Generate self-signed certificate
openssl x509 -req -in $CERT_DIR/tls.csr -signkey $CERT_DIR/tls.key -out $CERT_DIR/tls.crt -days 365 -extensions v3_req -extfile $CERT_DIR/csr.conf

echo -e "${GREEN}✓ SSL certificate created${NC}"

# Create Kubernetes secret
kubectl create secret tls productmanagement-tls \
  --cert=$CERT_DIR/tls.crt \
  --key=$CERT_DIR/tls.key \
  -n $NAMESPACE \
  --dry-run=client -o yaml | kubectl apply -f -

echo -e "${GREEN}✓ TLS secret created in namespace $NAMESPACE${NC}"

# Create secret for ArgoCD
kubectl create secret tls argocd-tls \
  --cert=$CERT_DIR/tls.crt \
  --key=$CERT_DIR/tls.key \
  -n argocd \
  --dry-run=client -o yaml | kubectl apply -f -

echo -e "${GREEN}✓ TLS secret created in namespace argocd${NC}"

echo ""
echo -e "${YELLOW}Note: This is a self-signed certificate for development only.${NC}"
echo -e "${YELLOW}Your browser will show a security warning. You can safely proceed.${NC}"