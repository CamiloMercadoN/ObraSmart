using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ObraSmart.IntegrationTests.Helpers
{
    public class TestAuthHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        // ID del usuario "dueño" de los datos en las pruebas
        public const string TestUsuarioId =
            "11111111-1111-1111-1111-111111111111";

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult>
            HandleAuthenticateAsync()
        {
            /*
             * Una solicitud solo representa a un usuario autenticado
             * cuando incluye explícitamente el esquema de prueba.
             */
            if (!Request.Headers.TryGetValue(
                    "Authorization",
                    out var authorizationHeader) ||
                !string.Equals(
                    authorizationHeader.ToString(),
                    "TestScheme",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(
                    AuthenticateResult.NoResult());
            }

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    TestUsuarioId)
            };

            var identity = new ClaimsIdentity(
                claims,
                "TestScheme");

            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(
                principal,
                "TestScheme");

            return Task.FromResult(
                AuthenticateResult.Success(ticket));
        }
    }
}