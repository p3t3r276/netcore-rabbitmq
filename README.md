# Netcore Rabbitmq example

# E-Commerce Order Processing System with RabbitMQ

## Overview
This .NET 9 application demonstrates a practical RabbitMQ implementation for processing e-commerce orders asynchronously.

## Architecture

### Components:
1. **OrderProcessing.Shared** - Shared models and contracts
2. **OrderProcessing.Producer** - ASP.NET Core Web API (Order submission)
3. **OrderProcessing.Consumer** - Background Worker Service (Order processing)

### Data Flow:
```
Customer → Web API (Producer) → RabbitMQ Queue → Worker Service (Consumer) → Processing
```

## Scenario
1. Customer places an order via REST API
2. API validates and publishes order to RabbitMQ queue
3. Background worker picks up the order
4. Worker processes the order (inventory check, payment, shipping)
5. Order status is updated

## Queue Configuration
- **Queue Name**: `orders_queue`
- **Exchange**: `orders_exchange` (direct)
- **Routing Key**: `order.new`

## Running the Application

### Prerequisites
- .NET 9 SDK
- Docker (for RabbitMQ)

### Start RabbitMQ
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### Run Producer (API)
```bash
cd OrderProcessing.Producer
dotnet run
```
API will be available at: http://localhost:5000

### Run Consumer (Worker)
```bash
cd OrderProcessing.Consumer
dotnet run
```

### Test the System
```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "CUST001",
    "customerEmail": "john@example.com",
    "items": [
      {"productId": "PROD001", "productName": "Laptop", "quantity": 1, "price": 999.99},
      {"productId": "PROD002", "productName": "Mouse", "quantity": 2, "price": 29.99}
    ]
  }'
```

## RabbitMQ Management
Access the management UI at: http://localhost:15672
- Username: `guest`
- Password: `guest`
