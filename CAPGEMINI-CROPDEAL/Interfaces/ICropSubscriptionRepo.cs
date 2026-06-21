using Microsoft.VisualBasic;
using CAPGEMINI_CROPDEAL.Models;
namespace CAPGEMINI_CROPDEAL.Interfaces
{
    public interface ICropSubscriptionRepo
    {
        public Task AddSubscription(CropSubscription obj);
    }
    
}