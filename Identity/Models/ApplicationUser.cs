using Microsoft.AspNetCore.Identity;
using System;

namespace Identity.Models
{
    public class ApplicationUser : IdentityUser
    {

        public ApplicationUser()
        {
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
    }
}
