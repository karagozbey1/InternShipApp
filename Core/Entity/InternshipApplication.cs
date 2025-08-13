using Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity;

   

    public class InternshipApplication
    {
        public int Id { get; set; }
        public string Status { get; set; } = "Pending";
        public string? StudentNote { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



        public int InternshipOfferId { get; set; }
        public InternshipOffer InternshipOffer { get; set; } = null!;

        public int StudentId { get; set; }
        public User Student { get; set; } = null!;

        public int? AdvisorId { get; set; }
        public User? Advisor { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

       
        public ICollection<InternshipDocument>? InternshipDocuments { get; set; }
        public ICollection<InternshipComment>? InternshipComments { get; set; }
    }
