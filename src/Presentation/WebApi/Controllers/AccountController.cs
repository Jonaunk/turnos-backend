using Application.Features.Authenticate.Commands.AuthenticateCommand;
using Application.Features.Authenticate.Commands.RefreshTokenCommand;
using Application.Features.Authenticate.Commands.RegisterCommand;
using Application.Features.Authenticate.User;
using Microsoft.AspNetCore.Mvc;


namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseApiController
    {
        [HttpPost("Authenticate")]
        public async Task<IActionResult> AuthenticateAsync(AuthenticationRequest request)
        {
            return Ok(await Mediator.Send(new AuthenticateCommand
            {
                Email = request.Email,
                Password = request.Password
            }));
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        {
            return Ok(await Mediator.Send(new RegisterCommand
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword,
                UserName = request.UserName,
                Origin = GetOriginFromRequest()
                //Origin = Request.Headers["origin"]
            }));
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            return Ok(await Mediator.Send(new RefreshTokenCommand
            {
                AccessToken = request.AccessToken,
                RefreshToken = request.RefreshToken
                //,IpAddress = GenerateIpAddress()
            }));
        }

        private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
    }
}