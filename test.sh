#!/bin/bash

# Test script for Order Processing System

echo "=========================================="
echo "Order Processing System - Test Script"
echo "=========================================="
echo ""

# Test 1: Single order
echo "Test 1: Submitting a single order..."
curl -X POST http://localhost:5176/api/orders \
    -H "Content-Type: application/json" \
    -d '{
        "customerId": "CUST001",
        "customerEmail": "alice@example.com",
        "items": [
            {"productId": "PROD001", "productName": "Laptop", "quantity": 1, "price": 999.99},
            {"productId": "PROD002", "productName": "Mouse", "quantity": 2, "price": 29.99}
        ]
    }'
echo -e "\n"

sleep 1

# Test 2: Multiple items order
echo "Test 2: Submitting an order with multiple items..."
curl -X POST http://localhost:5176/api/orders \
    -H "Content-Type: application/json" \
    -d '{
        "customerId": "CUST002",
        "customerEmail": "bob@example.com",
        "items": [
            {"productId": "PROD003", "productName": "Keyboard", "quantity": 1, "price": 79.99},
            {"productId": "PROD004", "productName": "Monitor", "quantity": 1, "price": 299.99},
            {"productId": "PROD005", "productName": "USB Cable", "quantity": 3, "price": 9.99}
        ]
    }'
echo -e "\n"

sleep 3

# Test 3: High-value order
echo "Test 3: Submitting a high-value order..."
curl -X POST http://localhost:5176/api/orders \
    -H "Content-Type: application/json" \
    -d '{
        "customerId": "CUST003",
        "customerEmail": "charlie@example.com",
        "items": [
            {"productId": "PROD006", "productName": "Gaming PC", "quantity": 1, "price": 2499.99},
            {"productId": "PROD007", "productName": "4K Monitor", "quantity": 2, "price": 599.99}
        ]
    }'
echo -e "\n"

echo "=========================================="
echo "All test orders submitted!"
echo "Check the Consumer logs to see processing"
echo "=========================================="
