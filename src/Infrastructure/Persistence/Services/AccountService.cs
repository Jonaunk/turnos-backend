using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Application.Common.Enums;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Wrappers;
using Application.Features.Authenticate.User;
using Domain.Entities.Users;
using Domain.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Persistence.Contexts;

namespace Persistence.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JWTSettings _jwtSettings;
        private readonly IDateTimeService _dateTimeService;
        private readonly ApplicationDbContext _context;

        public AccountService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager, IOptions<JWTSettings> jwtSettings, IDateTimeService dateTimeService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _dateTimeService = dateTimeService;
            _context = context;
        }



        public async Task<Response<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email!);
            if (user is null) throw new ApiException($"No hay cuenta registrada con el email {request.Email}");
            var result = await _signInManager.PasswordSignInAsync(user.UserName!, request.Password!, false, lockoutOnFailure: false);

            if (!result.Succeeded) throw new ApiException($"Las credenciales del usuario no son validas");

            JwtSecurityToken jwtSecurityToken = await GenerateJwtSecurityToken(user);
            AuthenticationResponse authenticationResponse = new AuthenticationResponse();
            authenticationResponse.Id = user.Id;
            authenticationResponse.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authenticationResponse.FirstName = user.FirstName;
            authenticationResponse.LastName = user.LastName;
            authenticationResponse.Country = Guid.NewGuid().ToString();
            authenticationResponse.Email = user.Email;
            authenticationResponse.UserName = user.UserName;

            var roleList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            authenticationResponse.Roles = roleList.ToList();
            authenticationResponse.IsVerified = user.EmailConfirmed;


            //var refreshToken = GenerateRefreshToken(ipAddress, usuario.Id);
            //response.RefreshToken = refreshToken.Token;
            authenticationResponse.RefreshToken = await GenerateRefreshToken(user.Id);
            return new Response<AuthenticationResponse>(authenticationResponse, $"Usuario {user.UserName} autenticado");

        }

        public async Task<Response<string>> RegisterAsync(RegisterRequest request, string origin)
        {
            var userWithTheSameUserName = await _userManager.FindByNameAsync(request.UserName!);
            if(userWithTheSameUserName is not null) throw new ApiException($"El nombre de usuario {request.UserName} ya fue registrado previamente");

          

            var user = new User {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CountryId = Guid.NewGuid(),
                UserName = request.UserName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var userWithTheSameMail = await _userManager.FindByEmailAsync(request.Email!);

            if(userWithTheSameMail is not null) throw new ApiException($"El mail {request.Email} ya fue registrado previamente");

            var result = await _userManager.CreateAsync(user, request.Password!);
            if(result.Succeeded)    {
                await _userManager.AddToRoleAsync(user, Roles.Basic.ToString());       
                return new Response<string>(user.Id, message: $"Usuario {request.UserName} registrado correctamente.");
            }
            else throw new ApiException($"{result.Errors}");

        }

           public async Task<Response<AuthenticationResponse>> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var oldToken = await _context.RefreshTokens.FirstOrDefaultAsync(q => q.Token == refreshToken);

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

                var refreshTokens = await _context.RefreshTokens
                    .Where(q => q.IsActive && q.UserId == oldToken.UserId).ToListAsync();

                foreach (var rt in refreshTokens)
                {
                    rt.Revoked = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                throw new ApiException("Se ha intentado usar un RefreshToken inactivo");
            }

            // TODO: Podríamos validar que el Access Token sí corresponde al mismo usuario
            oldToken.Revoked = DateTime.Now;

            var user = await _context.Users.FindAsync(oldToken.UserId);

            if (user is null)
            {
                throw new ApiException("El usuario no corresponde a ningun RefreshToken");
            }

            JwtSecurityToken jwtSecurityToken = await GenerateJwtSecurityToken(user);
            AuthenticationResponse response = new AuthenticationResponse();
            response.Id = user.Id;
            response.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.Email = user.Email;
            response.UserName = user.UserName;

            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();
            response.IsVerified = user.EmailConfirmed;

            response.RefreshToken = await GenerateRefreshToken( user.Id);
            return new Response<AuthenticationResponse>(response, $"Usuario {user.UserName} autenticado");
        }

        private async Task<JwtSecurityToken> GenerateJwtSecurityToken(User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();

            foreach (var userRole in userRoles)
            {
                roleClaims.Add(new Claim("roles", userRole));
            }



            var claims = new[]{
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("uid", user.Id)
                //, claim ip
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key!));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials
            );

            return jwtSecurityToken;
        }





        private async Task<string> GenerateRefreshToken( string idUser)
        {

            var newAccessToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString("N"),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.Now,
                UserId = idUser
            };

            _context.RefreshTokens.Add(newAccessToken);

            await _context.SaveChangesAsync();

            return newAccessToken.Token;
        }


    }
}