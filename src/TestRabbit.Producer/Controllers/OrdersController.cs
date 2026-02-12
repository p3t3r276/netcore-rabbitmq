using Microsoft.AspNetCore.Mvc;
using TestRabbit.Shared.Models;
using TestRabbit.Producers.Services;

namespace TestRabbit.Producers.Controllers;

[Route("api/[controller]")]
public class OrdersController(RabbitMqProducer producer, ILogger<OrdersController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
    {
        logger.LogInformation(
            "Received order creation request for customer {CustomerId}",
            request.CustomerId
        );
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.CustomerId))
            {
                return BadRequest(new { error = "CustomerId is required" });
            }

            if (request.Items is null || request.Items.Count == 0)
            {
                return BadRequest(new { error = "Order must contain at least one item" });
            }

            // Create order
            var order = new Order
            {
                CustomerId = request.CustomerId,
                CustomerEmail = request.CustomerEmail,
                Items = request.Items,
                Status = OrderStatus.Pending
            };

            logger.LogInformation(
                "Received order request from customer {CustomerId} with {ItemCount} items",
                order.CustomerId,
                order.Items.Count
            );

            // Publish to RabbitMQ
            await producer.PublishOrderAsync(order);

            return Ok(new
            {
                orderId = order.OrderId,
                status = order.Status.ToString(),
                totalAmount = order.TotalAmount,
                message = "Order received and queued for processing"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing order request");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "OrderProcessing.Producer" });
    }
}

public class OrderRequest
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = [];
}
