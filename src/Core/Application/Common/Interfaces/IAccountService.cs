using Application.Common.Wrappers;
using Application.Features.Authenticate.User;

namespace Application.Common.Interfaces{
    public interface IAccountService
    {
        Task<Response<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request, string ipAddress);
        Task<Response<string>> RegisterAsync(RegisterRequest request, string origin);
        Task<Response<AuthenticationResponse>> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task<Response<bool>> RevokeTokenAsync(string refreshToken, string ipAddress);
        Task<Response<AuthenticationResponse>> GetUser();
    }
}