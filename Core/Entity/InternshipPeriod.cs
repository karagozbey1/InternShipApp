using System;
using System.Collections.Generic;

namespace Core.Entity;

public class InternshipPeriod
{
    public int PeriodId { get; set; }
    public string PeriodName { get; set; } = null!;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }

    // keep legacy optional UserId (mapped to DB column UserId) to avoid insert errors
    public int? UserId { get; set; }

    // removed single StudentId; use join entity for many-to-many
    public ICollection<InternshipPeriodStudent>? PeriodStudents { get; set; }
    public ICollection<InternshipOffer>? InternshipOffers { get; set; }
    public ICollection<InternshipApplication>? InternshipApplications { get; set; }
}
