using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace AgencyFlow.Models
{
    public class User : IdentityUser
    {
        // Navigation Properties
        public virtual UserProfile? Profile { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
