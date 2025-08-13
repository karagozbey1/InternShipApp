using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity;

    public class InternshipPeriod
    {
    public int PeriodId { get; set; }
    public string PeriodName { get; set; } = null!;
    public DateTime StartDate { get; set; }= DateTime.Now;
    public DateTime EndDate { get; set; }


    public ICollection<InternshipOffer>? InternshipOffers { get; set; }
    public ICollection<InternshipApplication>? InternshipApplications { get; set; }
}
