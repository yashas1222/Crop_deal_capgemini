using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CAPGEMINI_CROPDEAL.Events;
using CAPGEMINI_CROPDEAL.Data;
using CAPGEMINI_CROPDEAL.Services;


public class CropNotificationConsumer : BackgroundService
{
    private readonly RabbitMQService _rabbitService;
    private readonly IServiceScopeFactory _scopeFactory;

    public CropNotificationConsumer(
        RabbitMQService rabbitService,
        IServiceScopeFactory scopeFactory)
    {
        _rabbitService = rabbitService;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _rabbitService.GetChannel();

        var consumer = new EventingBasicConsumer(channel);
        //consumer is the class that raises the event when it receives the message.
        // consumer.Received is the Event that is raised, and async(model,ea) is subscibed to it.
        // ea contains all the information about the message was published into the queue.
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            var cropEvent = JsonSerializer.Deserialize<CropPublishedEvent>(json);

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CropDealDbContext>();

            if (cropEvent == null)return;
            var buyers = await context.CropSubscriptions
                .Where(s => s.CropName == cropEvent.CropName)
                .Include(s => s.Buyer)
                .ToListAsync();

            foreach (var sub in buyers)
            {
                Console.WriteLine($"Notify Buyer {sub.Buyer?.BuyerName}: New {cropEvent?.CropName} crop available!");
                context.CropSubscriptions.Remove(sub);
            }
            await context.SaveChangesAsync();
        };

        channel.BasicConsume(
            queue: "crop_notifications",
            autoAck: true,
            consumer: consumer);

        return Task.CompletedTask;
    }
}