using TestRabbit.API.Models;

namespace TestRabbit.API.Services;

public class OrderProcessor(ILogger<OrderProcessor> logger)
{
    public async Task ProcessOrderAsync(Order order)
    {
        logger.LogInformation(
            "Starting to process order {OrderId} for customer {CustomerId}",
            order.OrderId,
            order.CustomerId
        );

        try
        {
            // Step 1: Validate inventory
            order.Status = OrderStatus.Processing;
            logger.LogInformation("Order {OrderId}: Checking inventory...", order.OrderId);
            await CheckInventoryAsync(order);

            // Step 2: Process payment
            order.Status = OrderStatus.PaymentProcessed;
            logger.LogInformation(
                "Order {OrderId}: Processing payment of ${Amount:F2}...",
                order.OrderId,
                order.TotalAmount
            );
            await ProcessPaymentAsync(order);

            // Step 3: Arrange shipping
            order.Status = OrderStatus.Shipped;
            logger.LogInformation("Order {OrderId}: Arranging shipping...", order.OrderId);
            await ArrangeShippingAsync(order);

            // Step 4: Complete order
            order.Status = OrderStatus.Completed;
            logger.LogInformation(
                "Order {OrderId}: Successfully completed! Customer: {Email}",
                order.OrderId,
                order.CustomerEmail
            );

            await SendConfirmationEmailAsync(order);
        }
        catch (Exception ex)
        {
            order.Status = OrderStatus.Failed;
            logger.LogError(
                ex,
                "Order {OrderId} failed during processing. Status: {Status}",
                order.OrderId,
                order.Status
            );
            throw;
        }
    }

    private async Task CheckInventoryAsync(Order order)
    {
        // Simulate inventory check
        await Task.Delay(500);

        foreach (var item in order.Items)
        {
            logger.LogDebug(
                "  ✓ {ProductName} (x{Quantity}) - In stock",
                item.ProductName,
                item.Quantity
            );
        }

        logger.LogInformation("  All items are in stock");
    }

    private async Task ProcessPaymentAsync(Order order)
    {
        // Simulate payment processing
        await Task.Delay(1000);

        logger.LogInformation(
            "  Payment of ${Amount:F2} processed successfully",
            order.TotalAmount
        );
    }

    private async Task ArrangeShippingAsync(Order order)
    {
        // Simulate shipping arrangement
        await Task.Delay(500);

        var estimatedDelivery = DateTime.UtcNow.AddDays(3);
        logger.LogInformation(
            "  Shipping label generated. Estimated delivery: {DeliveryDate:yyyy-MM-dd}",
            estimatedDelivery
        );
    }

    private async Task SendConfirmationEmailAsync(Order order)
    {
        // Simulate email sending
        await Task.Delay(200);

        logger.LogInformation(
            "  Confirmation email sent to {Email}",
            order.CustomerEmail
        );
    }
}
