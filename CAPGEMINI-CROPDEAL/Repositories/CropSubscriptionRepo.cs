using CAPGEMINI_CROPDEAL.Interfaces;
using CAPGEMINI_CROPDEAL.Models;
using CAPGEMINI_CROPDEAL.Data;
public class CropSubscriptionRepo : ICropSubscriptionRepo
{
    private readonly CropDealDbContext _context;
    public CropSubscriptionRepo(CropDealDbContext context)
    {
        _context = context;

    }

    public async Task AddSubscription(CropSubscription subscription)
    {
        _context.CropSubscriptions.Add(subscription);

        await _context.SaveChangesAsync();
    }
}