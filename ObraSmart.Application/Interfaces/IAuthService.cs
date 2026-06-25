using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs;

namespace ObraSmart.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result> RegistrarAsync(RegistroUsuarioDto dto);
        Task<Result<string>> LoginAsync(LoginDto dto);
    }
}
