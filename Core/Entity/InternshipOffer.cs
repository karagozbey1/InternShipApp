using Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    public class InternshipOffer
    {
      
        public int Id { get; set; }
  
        public string Title { get; set; } = null!;
     
        public string? Description { get; set; }
    
        public string? Requirements { get; set; }
        public string City { get; set; }=null!;
        public string Field { get; set; } = null!;

        public int AvailablePositions { get; set; } 
  
        public bool IsActive { get; set; } = true;
    
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ApplicationDeadline { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        public int PeriodId { get; set; }
        public InternshipPeriod Period { get; set; } = null!;
        
        
        public ICollection<InternshipApplication>? InternshipApplications { get; set; }
    }
}
