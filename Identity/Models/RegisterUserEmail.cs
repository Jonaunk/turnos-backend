using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Models
{
    public class RegisterUserEmail
    {
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Url { get; set; } = default!;
    }
}
