using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        CurrentUser User { get; }

        bool IsInRole(string roleName);
    }

    public record CurrentUser(string Id, string UserName, bool IsAuthenticated);
}
