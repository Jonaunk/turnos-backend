using Application.Common.Wrappers;
using Application.Features.Authenticate.User;

namespace Application.Common.Interfaces{
    public interface IAccountService{

        Task<Response<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request);
        Task<Response<string>> RegisterAsync(RegisterRequest request, string origin);
        Task<Response<AuthenticationResponse>> RefreshTokenAsync(string accessToken, string refreshToken);
    }
}