using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Authenticate.User
{
    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        //public string? IpAddress { get; set; }
    }
}
