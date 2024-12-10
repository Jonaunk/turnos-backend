using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string? Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.Now >= Expires;
        public DateTime Created { get; set; }
        public string? CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? replaceByToken { get; set; }
        public bool IsActive => Revoked == null && !IsExpired;
        public ApplicationUser User { get; set; }
        public string UserId { get; set; }

    }
}
