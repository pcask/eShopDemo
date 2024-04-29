namespace BasketService.Api.Core.Domain.Models;

public class CustomerBasket
{
    public string BuyerId { get; set; } // Customer userName

    public List<BasketItem> BasketItems { get; set; }

    public CustomerBasket()
    {
        BasketItems = [];
    }

    public CustomerBasket(string customerId) // UserName
    {
        BuyerId = customerId;
        BasketItems = [];
    }
}