using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Core.Application.Services;
using BasketService.Api.Core.Domain.Models;
using BasketService.Api.IntegrationEvents.Events;
using EventBus.Base.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BasketService.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BasketsController(IBasketRepository basketRepository, IIdentityService identityService, IEventBus eventBus, ILogger<BasketsController> logger) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        // Get info whether the app is running or not
        return Ok("Basket Service is Up and Running");
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<CustomerBasket>> GetBasketByIdAsync(string id) // id = UserName
    {
        var basket = await basketRepository.GetBasketAsync(id);

        return Ok(basket ?? new CustomerBasket(id));
    }

    [HttpPut]
    [Route("update")]
    [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<CustomerBasket>> UpdateBasketAsync([FromBody] CustomerBasket basket)
    {
        return Ok(await basketRepository.UpdateBasketAsync(basket));
    }

    [HttpPost]
    [Route("additem")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> AddItemToBasketAsync([FromBody] BasketItem basketItem)
    {
        var userName = identityService.GetUserName();

        var basket = await basketRepository.GetBasketAsync(userName) ?? new CustomerBasket(userName);

        basket.BasketItems.Add(basketItem);

        await basketRepository.UpdateBasketAsync(basket);

        return Ok();
    }

    [HttpPost]
    [Route("checkout")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CheckoutAsync([FromBody] BasketCheckout checkout)
    {
        var userName = identityService.GetUserName();

        if (userName != checkout.UserName)
            return BadRequest();

        var basket = await basketRepository.GetBasketAsync(userName);

        if (basket is null)
            return BadRequest();


        var eventMessage = new OrderCreatedIntegrationEvent(userName, checkout.Country, checkout.State, checkout.City, checkout.Street, checkout.ZipCode, checkout.CardNumber, checkout.CardHolderName, checkout.CardExpiration, checkout.CardSecurityNumber, checkout.CardTypeId, basket);

        try
        {
            // EventBus'a pushlanan bu event OrderService tarafından dinlenecek ve order oluşturma işlemleri başlayacaktır.

            // Aynı zamanda BasketService tarafıdan pushlanan bu event yine basketService tarafından dinlenecek ve ardından müşterinin basket'i temizlenecek.
            // Event'in ilgili EventBus'a ulaştığından emin olunduktan sonra basket silinmeli.

            eventBus.Publish(eventMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[ ERROR ] Publishing integration event : {IntegrationEventId} from BasketService.Api", eventMessage.Id);

            throw;
        }

        return Accepted();
    }

    // DELETE api/baskets/denek -> userName'i denek olan müşterinin sepeti silinecek.
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<bool>> DeleteBasketByIdAsync(string id) // id = userName
    {
        return Ok(await basketRepository.DeleteBasketAsync(id));
    }
}
