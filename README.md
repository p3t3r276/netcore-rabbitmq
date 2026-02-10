# netcore-rabbitmq

## Overview

This project demonstrates how to integrate RabbitMQ with .NET Core for message queuing. It includes a producer application that sends messages and a consumer application that receives and processes them.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [RabbitMQ Server](https://www.rabbitmq.com/download.html)

## Installation

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd netcore-rabbitmq
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

## Configuration

Update the `appsettings.json` files in both the producer and consumer projects with your RabbitMQ connection details if needed.

## Usage

### Start the Consumer

Open a terminal and run the consumer application:

```bash
cd src/Consumer
dotnet run
```

### Start the Producer

Open another terminal and run the producer application:

```bash
cd src/Producer
dotnet run
```

Messages will be sent from the producer to the consumer via RabbitMQ.

## Project Structure

- `src/Producer`: The .NET Core application that sends messages to RabbitMQ.
- `src/Consumer`: The .NET Core application that receives messages from RabbitMQ.
- `src/Shared`: Shared code or models used by both applications.

## License

This project is licensed under the terms of the MIT license.
