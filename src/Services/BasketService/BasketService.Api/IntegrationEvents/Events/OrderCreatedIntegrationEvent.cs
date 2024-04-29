using BasketService.Api.Core.Domain.Models;
using EventBus.Base.Events;

namespace BasketService.Api.IntegrationEvents.Events;

public class OrderCreatedIntegrationEvent : IntegrationEvent
{
    // Kim?
    public string UserName { get; set; } // Buyer

    // Nerede?
    public string Country { get; set; }
    public string State { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string ZipCode { get; set; }

    // Ne ile?
    public string CardNumber { get; set; }
    public string CardHolderName { get; set; }
    public DateTime CardExpiration { get; set; }
    public string CardSecurityNumber { get; set; }
    public int CardTypeId { get; set; }

    // Ne Aldı?
    public CustomerBasket Basket { get; set; }

    public OrderCreatedIntegrationEvent(string userName, string country, string state, string city, string street, string zipCode, string cardNumber, string cardHolderName, DateTime cardExpiration, string cardSecurityNumber, int cardTypeId, CustomerBasket basket)
    {
        UserName = userName;
        Country = country;
        State = state;
        City = city;
        Street = street;
        ZipCode = zipCode;
        CardNumber = cardNumber;
        CardHolderName = cardHolderName;
        CardExpiration = cardExpiration;
        CardSecurityNumber = cardSecurityNumber;
        CardTypeId = cardTypeId;
        Basket = basket;
    }
}
