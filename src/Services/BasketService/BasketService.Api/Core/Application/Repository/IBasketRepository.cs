using BasketService.Api.Core.Domain.Models;

namespace BasketService.Api.Core.Application.Repository;

public interface IBasketRepository
{
    // UserName'ler unique olacak.
    Task<CustomerBasket> GetBasketAsync(string userName); // Mevcut kullanıcının sepetini getirecek.
    IEnumerable<string> GetUsers(); // Redis içerisindeki bütün kullanıcıları getirecek.
    Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket); // Mevcut kullanıcının sepeti güncellenecek.
    Task<bool> DeleteBasketAsync(string userName); // Mevcut kullanıcının sepeti boşaltılacak.
}
