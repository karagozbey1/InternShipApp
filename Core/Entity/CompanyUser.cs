using Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{


    public class CompanyUser
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string? RoleInCompany { get; set; }
    }

}

