using System;

namespace Core.Entity
{
    public class OfferFavorite
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public User? Student { get; set; }
        public int OfferId { get; set; }
        public InternshipOffer? Offer { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
