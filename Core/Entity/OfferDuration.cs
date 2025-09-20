using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    // Ýlan süresi
    public class OfferDuration
    {
        // Kimlik numarasý
        public int Id { get; set; }
        
        // Süre adý
        public string Name { get; set; } = null!; // e.g. "Kýsa", "Uzun", "Yarý zamanlý", "Tam zamanlý"
    }
}