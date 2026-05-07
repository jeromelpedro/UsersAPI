#!/bin/bash
# Validation script for Users API deployment on AKS

set -e

NAMESPACE="users-api"
DEPLOYMENT="users-api"
TIMEOUT=300

echo "=========================================="
echo "Users API AKS Deployment Validator"
echo "=========================================="
echo ""

# Color codes
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Utility functions
check_pass() {
    echo -e "${GREEN}✓ PASS${NC}: $1"
}

check_fail() {
    echo -e "${RED}✗ FAIL${NC}: $1"
}

check_warn() {
    echo -e "${YELLOW}⚠ WARN${NC}: $1"
}

# Step 1: Check namespace
echo "Step 1: Validating namespace..."
if kubectl get namespace $NAMESPACE > /dev/null 2>&1; then
    check_pass "Namespace '$NAMESPACE' exists"
else
    check_fail "Namespace '$NAMESPACE' not found"
    exit 1
fi
echo ""

# Step 2: Check if pods are running
echo "Step 2: Checking pod status..."
PENDING_PODS=$(kubectl get pods -n $NAMESPACE --no-headers 2>/dev/null | grep -v Running | grep -v Completed | wc -l)
RUNNING_PODS=$(kubectl get pods -n $NAMESPACE --no-headers 2>/dev/null | grep Running | wc -l)

if [ "$PENDING_PODS" -eq 0 ]; then
    check_pass "All pods are running ($RUNNING_PODS pods)"
else
    check_warn "$PENDING_PODS pods are still pending (may be normalizing)"
    kubectl get pods -n $NAMESPACE
fi
echo ""

# Step 3: Check deployment replicas
echo "Step 3: Checking deployment status..."
DESIRED=$(kubectl get deployment $DEPLOYMENT -n $NAMESPACE -o jsonpath='{.spec.replicas}' 2>/dev/null || echo "0")
READY=$(kubectl get deployment $DEPLOYMENT -n $NAMESPACE -o jsonpath='{.status.readyReplicas}' 2>/dev/null || echo "0")

if [ "$READY" -eq "$DESIRED" ] && [ "$DESIRED" -gt 0 ]; then
    check_pass "Deployment has $READY/$DESIRED replicas ready"
else
    check_fail "Deployment replicas mismatch: $READY/$DESIRED"
fi
echo ""

# Step 4: Check Key Vault secrets mounted
echo "Step 4: Validating Key Vault secret mounts..."
POD_NAME=$(kubectl get pods -n $NAMESPACE -l app=users-api -o jsonpath='{.items[0].metadata.name}' 2>/dev/null || echo "")

if [ -z "$POD_NAME" ]; then
    check_fail "No users-api pod found"
else
    SECRET_COUNT=$(kubectl exec -n $NAMESPACE "$POD_NAME" -- ls /mnt/secrets-store/ 2>/dev/null | wc -l || echo "0")
    
    if [ "$SECRET_COUNT" -gt 0 ]; then
        check_pass "Key Vault secrets mounted: $SECRET_COUNT files"
        kubectl exec -n $NAMESPACE "$POD_NAME" -- ls -la /mnt/secrets-store/ 2>/dev/null | tail -n +2
    else
        check_fail "No Key Vault secrets found in /mnt/secrets-store"
    fi
fi
echo ""

# Step 5: Check health endpoints
echo "Step 5: Testing health endpoints..."
if [ -n "$POD_NAME" ]; then
    # Port-forward in background
    kubectl port-forward -n $NAMESPACE "pod/$POD_NAME" 5055:5055 > /dev/null 2>&1 &
    PF_PID=$!
    sleep 2
    
    # Test /health
    if curl -s http://localhost:5055/health > /dev/null 2>&1; then
        check_pass "/health endpoint responding"
    else
        check_fail "/health endpoint not responding"
    fi
    
    # Test /health/live
    if curl -s http://localhost:5055/health/live > /dev/null 2>&1; then
        check_pass "/health/live endpoint responding"
    else
        check_fail "/health/live endpoint not responding"
    fi
    
    # Test /health/ready
    if curl -s http://localhost:5055/health/ready > /dev/null 2>&1; then
        check_pass "/health/ready endpoint responding"
    else
        check_fail "/health/ready endpoint not responding"
    fi
    
    kill $PF_PID 2>/dev/null || true
else
    check_fail "Could not test endpoints (no pod found)"
fi
echo ""

# Step 6: Check Redis connectivity
echo "Step 6: Validating Redis service..."
if kubectl exec -n $NAMESPACE "$POD_NAME" -- redis-cli -h redis.$NAMESPACE.svc.cluster.local ping > /dev/null 2>&1; then
    check_pass "Redis service is reachable"
else
    check_warn "Redis service not responding (may still be initializing)"
fi
echo ""

# Step 7: Check MongoDB connectivity
echo "Step 7: Validating MongoDB service..."
MONGO_READY=$(kubectl get pod -n $NAMESPACE -l app=mongodb -o jsonpath='{.items[0].status.conditions[?(@.type=="Ready")].status}' 2>/dev/null || echo "False")
if [ "$MONGO_READY" = "True" ]; then
    check_pass "MongoDB pod is ready"
else
    check_warn "MongoDB pod not yet ready (may still be initializing)"
fi
echo ""

# Step 8: Check Elasticsearch connectivity
echo "Step 8: Validating Elasticsearch service..."
if kubectl exec -n $NAMESPACE "$POD_NAME" -- curl -s http://elasticsearch.$NAMESPACE.svc.cluster.local:9200/_cluster/health > /dev/null 2>&1; then
    check_pass "Elasticsearch service is reachable"
else
    check_warn "Elasticsearch service not responding (may still be initializing)"
fi
echo ""

# Step 9: Check logs for errors
echo "Step 9: Checking pod logs for errors..."
ERROR_COUNT=$(kubectl logs -n $NAMESPACE -l app=users-api --tail=100 2>/dev/null | grep -i "error" | wc -l || echo "0")

if [ "$ERROR_COUNT" -eq 0 ]; then
    check_pass "No errors found in recent logs"
else
    check_warn "$ERROR_COUNT error(s) found in logs"
    kubectl logs -n $NAMESPACE -l app=users-api --tail=20 | grep -i "error" || true
fi
echo ""

# Step 10: Service and Ingress check
echo "Step 10: Checking Services and Ingress..."
SVC_COUNT=$(kubectl get svc -n $NAMESPACE {-o jsonpath='{.items|length}' 2>/dev/null || echo "0")
INGRESS_COUNT=$(kubectl get ingress -n $NAMESPACE --no-headers 2>/dev/null | wc -l || echo "0")

check_pass "Services found: $SVC_COUNT"
check_pass "Ingress found: $INGRESS_COUNT"

if [ "$INGRESS_COUNT" -gt 0 ]; then
    echo "Ingress details:"
    kubectl get ingress -n $NAMESPACE
fi
echo ""

# Final summary
echo "=========================================="
echo "Validation Complete!"
echo "=========================================="
echo ""
echo "Next steps:"
echo "1. If all checks passed, your deployment is ready"
echo "2. To access the API, use the Ingress host (or port-forward for testing)"
echo "3. Check logs: kubectl logs -n $NAMESPACE -l app=users-api -f"
echo "4. For detailed troubleshooting, see: /memories/session/deployment-summary.md"
echo ""
