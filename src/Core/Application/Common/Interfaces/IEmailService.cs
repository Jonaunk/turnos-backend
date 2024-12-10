using Application.Common.Mailing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IEmailService
    {
        //Task SendAsync(EmailDTO request);
        Task SendAsync(MailRequest request, CancellationToken cancellationToken);
    }
}
