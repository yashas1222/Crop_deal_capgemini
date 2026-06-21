using CAPGEMINI_CROPDEAL.Data;
using CAPGEMINI_CROPDEAL.Models;
using CAPGEMINI_CROPDEAL.Interfaces;
public class CropSubscriptionService : ICropSubscriptionService
{
    private readonly ICropSubscriptionRepo _repo;

    public CropSubscriptionService(ICropSubscriptionRepo repo)
    {
        _repo = repo;
    }

    public async Task Subscribe(int buyerId, string cropName)
    {
        var subscription = new CropSubscription
        {
            BuyerId = buyerId,
            CropName = cropName
        };
        await _repo.AddSubscription(subscription);

        
    }
}