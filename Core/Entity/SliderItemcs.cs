using Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    public class SliderItem
    {
        public int Id { get; set; }
        public int SliderId { get; set; }
        public Slider Slider { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Subtitle { get; set; }
        public string? ImageUrl { get; set; }
        public string? Link { get; set; }
        public string? TargetType { get; set; } // "News", "InternshipOffer",Scholership, "External"
        public int? TargetId { get; set; }
        public DateTime? StartDisplay { get; set; }
        public DateTime? EndDisplay { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}
