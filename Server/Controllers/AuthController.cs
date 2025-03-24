using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Synaplic.BlazorJwtApp.Server.Authentication;
using Synaplic.BlazorJwtApp.Server.Model;
using Synaplic.BlazorJwtApp.Shared;
using System.IdentityModel.Tokens.Jwt;

namespace Synaplic.BlazorJwtApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly ApplicationDbContext _context;

        public AuthController(UserManager<IdentityUser> userManager, TokenService tokenService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDTO request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null )
            {
                return Unauthorized(new { Message = "Invalid username" });
            }
            Console.WriteLine("🔹 User found");
            Console.WriteLine("🔹 password : " + request.Password);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }

            var token = await _tokenService.GenerateJwtToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Store refresh token securely
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDTO
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiryInMinutes = 60,
                UserName = user.UserName
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDTO request)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken == null || storedToken.ExpiryDate < DateTime.UtcNow)
            {
                return Unauthorized(new { Message = "Invalid or expired refresh token" });
            }

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            if (user == null)
            {
                return Unauthorized(new { Message = "User not found" });
            }

            // Validate expired token and get user claims
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.Token);
            if (principal == null || principal.Identity?.Name != user.UserName)
            {
                return Unauthorized(new { Message = "Invalid token" });
            }

            // Generate new JWT token and refresh token
            var newJwtToken = await _tokenService.GenerateJwtToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Update refresh token in the database
            storedToken.Token = newRefreshToken;
            storedToken.ExpiryDate = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDTO
            {
                Token = newJwtToken,
                RefreshToken = newRefreshToken,
                ExpiryInMinutes = 60,
                UserName = user.UserName
            });
        }
    }
}
