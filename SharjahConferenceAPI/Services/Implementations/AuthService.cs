namespace SharjahConferenceAPI.Services.Implementations;

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities;
using Data.Enums;
using Data.UnitOfWork;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Services.Interfaces;

public class AuthService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService
) : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await unitOfWork.Users.SingleOrDefaultAsync(
            u => u.Email.ToLower() == normalizedEmail,
            cancellationToken,
            u => u.Publisher!,
            u => u.Participant!
        );

        if (user == null || !passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var (token, expiration) = tokenService.GenerateToken(user);
        return new AuthResponseDto(
            token,
            expiration,
            user.Role.ToString(),
            user.Email,
            user.Id,
            user.PublisherId,
            user.ParticipantId
        );
    }

    public async Task<AuthResponseDto> RegisterParticipantAsync(RegisterParticipantDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var existingUser = await unitOfWork.Users.SingleOrDefaultAsync(
            u => u.Email.ToLower() == normalizedEmail,
            cancellationToken
        );

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var participant = new Participant
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = normalizedEmail,
            Phone = dto.Phone.Trim(),
            Title = dto.Title.Trim()
        };

        await unitOfWork.Participants.AddAsync(participant, cancellationToken);

        var passwordHash = passwordHasher.HashPassword(dto.Password);
        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Role = UserRole.Participant,
            ParticipantId = participant.Id,
            Participant = participant
        };

        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        var (token, expiration) = tokenService.GenerateToken(user);
        return new AuthResponseDto(
            token,
            expiration,
            user.Role.ToString(),
            user.Email,
            user.Id,
            user.PublisherId,
            user.ParticipantId
        );
    }

    public async Task<AuthResponseDto> RegisterPublisherAsync(RegisterPublisherDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var existingUser = await unitOfWork.Users.SingleOrDefaultAsync(
            u => u.Email.ToLower() == normalizedEmail,
            cancellationToken
        );

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var publisher = new Publisher
        {
            NameEn = dto.NameEn.Trim(),
            NameAr = dto.NameAr.Trim(),
            CountryCode = dto.CountryCode.Trim().ToUpperInvariant()
        };

        await unitOfWork.Publishers.AddAsync(publisher, cancellationToken);

        var passwordHash = passwordHasher.HashPassword(dto.Password);
        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Role = UserRole.Publisher,
            PublisherId = publisher.Id,
            Publisher = publisher
        };

        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        var (token, expiration) = tokenService.GenerateToken(user);
        return new AuthResponseDto(
            token,
            expiration,
            user.Role.ToString(),
            user.Email,
            user.Id,
            user.PublisherId,
            user.ParticipantId
        );
    }

    public async Task<AuthResponseDto> RegisterAdminAsync(RegisterAdminDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var existingUser = await unitOfWork.Users.SingleOrDefaultAsync(
            u => u.Email.ToLower() == normalizedEmail,
            cancellationToken
        );

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var passwordHash = passwordHasher.HashPassword(dto.Password);
        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Role = UserRole.Admin
        };

        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        var (token, expiration) = tokenService.GenerateToken(user);
        return new AuthResponseDto(
            token,
            expiration,
            user.Role.ToString(),
            user.Email,
            user.Id,
            user.PublisherId,
            user.ParticipantId
        );
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        var user = await unitOfWork.Users.GetByIdAsync(
            userId,
            cancellationToken,
            u => u.Publisher!,
            u => u.Participant!
        );

        if (user == null)
        {
            return null;
        }

        string? displayName = user.Role switch
        {
            UserRole.Publisher => user.Publisher?.NameEn,
            UserRole.Participant => $"{user.Participant?.FirstName} {user.Participant?.LastName}".Trim(),
            UserRole.Admin => "Administrator",
            _ => null
        };

        return new CurrentUserDto(
            user.Id,
            user.Email,
            user.Role.ToString(),
            user.PublisherId,
            user.ParticipantId,
            displayName
        );
    }
}
