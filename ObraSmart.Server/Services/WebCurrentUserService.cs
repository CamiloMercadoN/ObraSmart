using ObraSmart.Application.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ObraSmart.Server.Services
{
    public class WebCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public Guid? GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return null;
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ??
                              user.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null; // Retorna null si no hay usuario autenticado o no hay contexto HTTP
        }
    }
}
