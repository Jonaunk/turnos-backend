using Microsoft.AspNetCore.Identity;
using Domain.Entities.Users;

namespace Domain.Entities.Users
{
    public class User : IdentityUser
    {
       public string? FirstName { get; set; }
       public string? LastName { get; set;}
       public Gender Gender{ get; set; }
       public int ProjectSupported { get; set; }
       public decimal Amount { get; set; }

        public Guid CountryId { get; set; }

        public virtual Country? Country{ get; set; }

    }
} 