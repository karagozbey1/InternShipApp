using Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    public enum OfferDurationType
    {
        [Display(Name = "Kısa")]
        Kisa = 1,
        [Display(Name = "Uzun")]
        Uzun = 2,
        [Display(Name = "Yarı zamanlı")]
        YariZamanli = 3,
        [Display(Name = "Tam zamanlı")]
        TamZamanli = 4
    }

    public enum OfferJobType
    {
        Internship = 1,
        Job = 2,
        Both = 3
    }

    public enum CurrencyType
    {
        [Display(Name = "₺ - TL")]
        TRY = 1,
        [Display(Name = "$ - USD")]
        USD = 2,
        [Display(Name = "€ - EUR")]
        EUR = 3
    }

    public enum DateSelectionMode
    {
        // Company selects a named period (student cannot change dates)
        Period = 1,
        // Company provided specific fixed start/end dates (student cannot change)
        FixedRange = 2,
        // Company did not restrict dates - student can enter any dates
        Open = 3
    }

    public class InternshipOffer
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;

        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public string City { get; set; } = null!;
        public string Field { get; set; } = null!;
     
        public int AvailablePositions { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApplicationDeadline { get; set; }
        public int CompanyId { get; set; }
        public Company? Company { get; set; }

        // Link to InternshipPeriod instead of storing start/end directly
        public int PeriodId { get; set; }
        public InternshipPeriod? Period { get; set; }

        public OfferDurationType Duration { get; set; } = OfferDurationType.Kisa;

        public OfferJobType JobType { get; set; } = OfferJobType.Internship;

        public decimal? MonthlySalary { get; set; }

        public CurrencyType MonthlySalaryCurrency { get; set; } = CurrencyType.TRY;

        public ICollection<InternshipApplication>? InternshipApplications { get; set; }

        // New: how dates are selected for this offer
        public DateSelectionMode DateMode { get; set; } = DateSelectionMode.Period;
    }
}
