using Core.Entity;
using DataAccess.GenericRepository.Ýnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Internship
{
    public interface IOfferFavoriteRepository : IRepository<OfferFavorite>
    {
        Task<OfferFavorite?> GetByUserAndOfferAsync(int studentId, int offerId);
        Task<List<OfferFavorite>> GetByUserAsync(int studentId);
        Task RemoveByUserAndOfferAsync(int studentId, int offerId);
    }
}
