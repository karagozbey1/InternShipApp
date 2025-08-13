using Core.Entity;

namespace WebUI.Models
{
    public class InternshipOffersViewModel
    {
        public List<InternshipOffer>? ActiveOffers { get; set; }
        public string? Keyword { get; set; }
        public string? City { get; set; }
        public string? Field { get; set; }
    }
}
