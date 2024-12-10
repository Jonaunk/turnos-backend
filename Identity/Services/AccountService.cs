using Application.Common.Enums;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mailing;
using Application.Common.Wrappers;
using Application.Features.Authenticate.Commands.RegisterCommand;
using Application.Features.Authenticate.User;
using Domain.Settings;
using Identity.Common;
using Identity.Context;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JWTSettings _jwtSettings;
        private readonly IDateTimeService _dateTimeService;
        private readonly IdentityContext _identityContext;
        private readonly IEmailTemplateService _templateService;
        private readonly SecuritySettings _securitySettings;
        private readonly CurrentUser _user;

        public AccountService(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<JWTSettings> jwtSettings,
            IDateTimeService dateTimeService,
            IdentityContext identityContext,
            IOptions<SecuritySettings> securitySettings,
            ICurrentUserService currentUserService,
            IEmailTemplateService templateService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _dateTimeService = dateTimeService;
            _identityContext = identityContext;
            _templateService = templateService;
            _securitySettings = securitySettings.Value;
            _user = currentUserService.User;
        }


        public async Task<Response<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request, string ipAddress)
        {
            var usuario = await _userManager.FindByEmailAsync(request.Email);
            if (usuario == null)
            {
                throw new ApiException($"No hay cuenta registrada con el Email {request.Email}");
            }

            var result = await _signInManager.PasswordSignInAsync(usuario.UserName, request.Password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                throw new ApiException($"Las credenciales del usuario no son validas");
            }

            JwtSecurityToken jwtSecurityToken = await GenerateJwyToken(usuario);
            AuthenticationResponse response = new AuthenticationResponse();
            response.Id = usuario.Id;
            response.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.Email = usuario.Email;
            response.UserName = usuario.UserName;
            response.Apellido = usuario.Apellido;
            response.Nombre = usuario.Nombre;
            response.UrlImage = "";

            var rolesList = await _userManager.GetRolesAsync(usuario).ConfigureAwait(false);
            response.Roles = rolesList.ToList();
            response.IsVerified = usuario.EmailConfirmed;

            var newRT = await GenerateRefreshToken(ipAddress, usuario.Id);

            response.RefreshToken = newRT.Token;
            response.RefreshTokenExpiration = newRT.Expires;
            //response.RefreshToken = await GenerateRefreshToken(ipAddress, usuario.Id);
            return new Response<AuthenticationResponse>(response, $"Usuario {usuario.UserName} autenticado");
        }

        public async Task<Response<AuthenticationResponse>> GetUser()
        {

            var user = await _userManager.Users
            .AsNoTracking()
            .Where(u => u.Id == _user.Id)
            .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new ApiException($"Usuario no encontrado");
            }

            JwtSecurityToken jwtSecurityToken = await GenerateJwyToken(user);
            AuthenticationResponse response = new AuthenticationResponse();
            response.Id = user.Id;
            response.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.Email = user.Email;
            response.UserName = user.UserName;
            response.Apellido = user.Apellido;
            response.Nombre = user.Nombre;
            response.UrlImage = "";

            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();
            response.IsVerified = user.EmailConfirmed;

            return new Response<AuthenticationResponse>(response);

        }

        public async Task<Response<string>> RegisterAsync(RegisterRequest request, string origin)
        {
            var usuarioConElMismoUserName = await _userManager.FindByNameAsync(request.UserName);
            if (usuarioConElMismoUserName != null)
                throw new ApiException($"El nombre de usuario {request.UserName} ya fue registrado previamente");

            var usuario = new ApplicationUser
            {
                Email = request.Email,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                UserName = request.UserName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var usuarioConElMismoCorreo = await _userManager.FindByEmailAsync(request.Email);
            if (usuarioConElMismoCorreo != null)
            {
                throw new ApiException($"El mail {request.Email} ya fue registrado previamente");
            }



            if (_securitySettings.RequireConfirmedAccount && !string.IsNullOrEmpty(usuario.Email))
            {
                // send verification email
                string emailVerificationUri = await GetEmailVerificationUriAsync(usuario, origin);
                RegisterUserEmail eMailModel = new RegisterUserEmail()
                {
                    Email = usuario.Email,
                    UserName = usuario.UserName,
                    Url = emailVerificationUri
                };

                var mailRequest = new MailRequest(
                    new List<string> { usuario.Email },
                    "Confirm Registration",
                    _templateService.GenerateEmailTemplate("email-confirmation", eMailModel)
                 );
                usuario.AddDomainEvent(new RegisterCommandEvent(mailRequest));
                //_jobService.Enqueue(() => _mailService.SendAsync(mailRequest, CancellationToken.None));
            }


            var result = await _userManager.CreateAsync(usuario, request.Password);
            if (!result.Succeeded)
                throw new ApiException($"{result.Errors}");

            await _userManager.AddToRoleAsync(usuario, Roles.Basic.ToString());

            return new Response<string>(usuario.Id, message: $"Usuario {usuario.UserName} registrado correctamente. Por favor chequear en {usuario.Email} para verificar tu cuenta!");
        }
        public async Task<Response<AuthenticationResponse>> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var oldToken = await _identityContext.RefreshTokens.FirstOrDefaultAsync(q => q.Token == refreshToken);

            // Refresh token no existe, expiró o fue revocado manualmente
            // (Pensando que el usuario puede dar click en "Cerrar Sesión en todos lados" o similar)
            if (oldToken is null || oldToken.Expires <= DateTime.UtcNow)
            {
                throw new ApiException("RefreshToken inactivo");
            }

            // Se está intentando usar un Refresh Token que ya fue usado anteriormente,
            // puede significar que este refresh token fue robado.
            if (!oldToken.IsActive)
            {
                //_logger.LogWarning("El refresh token del {UserId} ya fue usado. RT={RefreshToken}", refreshToken.UserId, refreshToken.RefreshTokenValue);

                var refreshTokens = await _identityContext.RefreshTokens
                    .Where(q => q.IsActive && q.UserId == oldToken.UserId).ToListAsync();

                foreach (var rt in refreshTokens)
                {
                    rt.Revoked = DateTime.Now;
                }

                await _identityContext.SaveChangesAsync();

                throw new ApiException("Se ha intentado usar un RefreshToken inactivo");
            }

            // TODO: Podríamos validar que el Access Token sí corresponde al mismo usuario
            oldToken.Revoked = DateTime.Now;

            var user = await _identityContext.Users.FindAsync(oldToken.UserId);

            if (user is null)
            {
                throw new ApiException("El usuario no corresponde a ningun RefreshToken");
            }

            JwtSecurityToken jwtSecurityToken = await GenerateJwyToken(user);
            AuthenticationResponse response = new AuthenticationResponse();
            response.Id = user.Id;
            response.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.Email = user.Email;
            response.UserName = user.UserName;

            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();
            response.IsVerified = user.EmailConfirmed;

            var newRT = await GenerateRefreshToken(ipAddress, user.Id);

            response.RefreshToken = newRT.Token;
            response.RefreshTokenExpiration = newRT.Expires;

            return new Response<AuthenticationResponse>(response, $"Usuario {user.UserName} autenticado");
        }

        public async Task<Response<bool>> RevokeTokenAsync(string refreshToken, string ipAddress)
        {

            var oldToken = await _identityContext.RefreshTokens.FirstOrDefaultAsync(q => q.Token == refreshToken);

            // Refresh token no existe, expiró o fue revocado manualmente
            // (Pensando que el usuario puede dar click en "Cerrar Sesión en todos lados" o similar)
            if (oldToken is null || oldToken.Expires <= DateTime.UtcNow)
            {
                throw new ApiException("RefreshToken inactivo");
            }

            // Se está intentando usar un Refresh Token que ya fue usado anteriormente,
            // puede significar que este refresh token fue robado.
            if (!oldToken.IsActive)
            {
                //_logger.LogWarning("El refresh token del {UserId} ya fue usado. RT={RefreshToken}", refreshToken.UserId, refreshToken.RefreshTokenValue);

                var refreshTokens = await _identityContext.RefreshTokens
                    .Where(q => q.IsActive && q.UserId == oldToken.UserId).ToListAsync();

                foreach (var rt in refreshTokens)
                {
                    rt.Revoked = DateTime.Now;
                }

                await _identityContext.SaveChangesAsync();

                throw new ApiException("Se ha intentado usar un RefreshToken inactivo");
            }

            // TODO: Podríamos validar que el Access Token sí corresponde al mismo usuario
            oldToken.Revoked = DateTime.Now;

            var res = await _identityContext.SaveChangesAsync();


            return new Response<bool>(true);

        }

        private async Task<string> GetEmailVerificationUriAsync(ApplicationUser user, string origin)
        {
            //EnsureValidTenant();

            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            const string route = "api/users/confirm-email/";
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            string verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), QueryStringKeys.UserId, user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, QueryStringKeys.Code, code);
            //verificationUri = QueryHelpers.AddQueryString(verificationUri, MultitenancyConstants.TenantIdName, _currentTenant.Id!);
            return verificationUri;
        }

        private async Task<JwtSecurityToken> GenerateJwyToken(ApplicationUser usuario)
        {
            var userClaims = await _userManager.GetClaimsAsync(usuario);
            var userRoles = await _userManager.GetRolesAsync(usuario);

            var roleClaims = new List<Claim>();

            for (int i = 0; i < userRoles.Count; i++)
            {
                roleClaims.Add(new Claim("roles", userRoles[i]));
            }

            //string ipAddress = IpHelper.GetIpAddres();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim("uid", usuario.Id)
                //,new Claim("ip", ipAddress)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signinCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecutiryToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signinCredentials
                );

            return jwtSecutiryToken;
        }

        private async Task<RefreshToken> GenerateRefreshToken(string ipAddress, string idUser)
        {

            var newRefreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString("N"),
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                Created = DateTime.Now,
                CreatedByIp = ipAddress,
                UserId = idUser
            };

            _identityContext.RefreshTokens.Add(newRefreshToken);

            await _identityContext.SaveChangesAsync();

            return newRefreshToken;
        }
    }
}
