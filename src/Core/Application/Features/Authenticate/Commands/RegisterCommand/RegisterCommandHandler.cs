using Application.Common.Interfaces;
using Application.Common.Wrappers;
using Application.Features.Authenticate.User;
using MediatR;

namespace Application.Features.Authenticate.Commands.RegisterCommand
{
     public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Response<string>>
    {
        private readonly IAccountService _accountService;

        public RegisterCommandHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }
        public async Task<Response<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var user = await _accountService.RegisterAsync(new RegisterRequest
            {
                Email = request.Email,
                UserName = request.UserName,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
            }, request.Origin);

            return user;

        }
    }
}