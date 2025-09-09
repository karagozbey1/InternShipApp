using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    public class OfferDuration
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // e.g. "Kýsa", "Uzun", "Yarý zamanlý", "Tam zamanlý"
    }
}