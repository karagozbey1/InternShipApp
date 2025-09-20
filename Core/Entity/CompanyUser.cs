using Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    // Þirket-kullanýcý iliþki entity'si - many-to-many junction table
    public class CompanyUser
    {
        // Primary key - iliþki benzersiz tanýmlayýcýsý
        public int Id { get; set; }

        // Foreign key - þirketin ID'si
        public int CompanyId { get; set; }
        
        // Navigation property - þirket
        public Company Company { get; set; } = null!;

        // Foreign key - kullanýcýnýn ID'si
        public int UserId { get; set; }
        
        // Navigation property - kullanýcý
        public User User { get; set; } = null!;

        // Kullanýcýnýn þirketteki rolü - opsiyonel (örn: "ÝK Uzmaný", "Staj Koordinatörü")
        public string? RoleInCompany { get; set; }
    }
}

