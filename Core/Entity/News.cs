using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime PublishDate { get; set; } = DateTime.UtcNow;
        public string Category { get; set; } = null!; // Staj, Burs, Genel
        public string? Link { get; set; } // Dış ya da detay linki
        public string? SourceName { get; set; }
        public string? SourceLogoUrl { get; set; }
    }
}
