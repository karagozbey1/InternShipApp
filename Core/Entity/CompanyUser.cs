using Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    // Þirket kullanýcý iliþkisi
    public class CompanyUser
    {
        // Kimlik numarasý
        public int Id { get; set; }

        // Þirket kimliði
        public int CompanyId { get; set; }
        
        // Þirket
        public Company Company { get; set; } = null!;

        // Kullanýcý kimliði
        public int UserId { get; set; }
        
        // Kullanýcý
        public User User { get; set; } = null!;

        // Þirketteki görevi
        public string? RoleInCompany { get; set; }
    }
}

