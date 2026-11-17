using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Interfaces;

namespace FlotaVehicular.Application.Implementation;

// Valida credenciales, actualiza el último acceso y entrega el token de sesión.
public sealed class AuthService : IAuthService
{
    private const string InvalidCredentialsMessage = "Usuario o contraseña incorrectos.";
    private const string InvalidSessionMessage = "La sesión ya no es válida. Inicie sesión nuevamente.";

    // Hash PBKDF2-SHA256 válido de relleno; mantiene el tiempo de respuesta cuando no hay usuario.
    private const string DummyPasswordHash =
        "PBKDF2-SHA256$600000$AQIDBAUGBwgJCgsMDQ4PEA==$LF+HSArhzaKhhPJoYYfZRfuzvpHJ8VSM0oPkFxUm/iQ=";

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    // Autentica por nombre de usuario o correo con el mismo mensaje para cualquier fallo.
    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var login = request.Username!.Trim();
        var user = await _userRepository.GetByLoginForUpdateAsync(login);

        // Siempre se ejecuta la verificación, exista o no el usuario.
        var passwordMatches = _passwordHasher.Verify(
            request.Password!, user?.PasswordHash ?? DummyPasswordHash);

        if (user is null || !passwordMatches)
            return Result<LoginResponseDto>.Failure(401, "Unauthorized", InvalidCredentialsMessage);

        if (!user.IsActive)
            return Result<LoginResponseDto>.Failure(403, "Forbidden",
                "La cuenta está desactivada. Comuníquese con un administrador.");

        user.LastAccessAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync();

        var accessToken = _tokenGenerator.Generate(user);

        var response = new LoginResponseDto
        {
            Token = accessToken.Value,
            TokenType = "Bearer",
            ExpiresAt = accessToken.ExpiresAtUtc,
            ExpiresInSeconds = Math.Max(0, (int)(accessToken.ExpiresAtUtc - user.LastAccessAt.Value).TotalSeconds),
            User = ToAuthUserDto(user)
        };

        return Result<LoginResponseDto>.Success(response, 200, "OK", "Sesión iniciada.");
    }

    // Comprueba que el usuario del token siga existiendo y activo.
    public async Task<Result<AuthUserDto>> GetCurrentUserAsync(int idUser)
    {
        var user = await _userRepository.GetByIdAsync(idUser);

        if (user is null || !user.IsActive)
            return Result<AuthUserDto>.Failure(401, "Unauthorized", InvalidSessionMessage);

        return Result<AuthUserDto>.Success(ToAuthUserDto(user));
    }

    // Expone únicamente los datos públicos del usuario.
    private static AuthUserDto ToAuthUserDto(User user) => new()
    {
        IdUser = user.IdUser,
        FullName = user.FullName,
        Username = user.Username,
        Email = user.Email,
        Role = user.Role.ToString()
    };
}
