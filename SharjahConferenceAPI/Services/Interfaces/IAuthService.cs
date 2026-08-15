namespace SharjahConferenceAPI.Services.Interfaces;

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RegisterParticipantAsync(RegisterParticipantDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RegisterPublisherAsync(RegisterPublisherDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RegisterAdminAsync(RegisterAdminDto dto, CancellationToken cancellationToken = default);
    Task<CurrentUserDto?> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}
