using System;

namespace Core.Entity
{
    // Favori ilan
    public class OfferFavorite
    {
        // Kimlik numarasý
        public int Id { get; set; }
        
        // Öðrenci kimliði
        public int StudentId { get; set; }
        
        // Öðrenci
        public User? Student { get; set; }
        
        // Ýlan kimliði
        public int OfferId { get; set; }
        
        // Ýlan
        public InternshipOffer? Offer { get; set; }
        
        // Oluþturulma zamaný
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
