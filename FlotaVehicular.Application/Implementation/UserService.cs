using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Domain.Exceptions;
using FlotaVehicular.Infrastructure.Interfaces;
using MapsterMapper;

namespace FlotaVehicular.Application.Implementation;

// Casos de uso de la gestión de usuarios.
public class UserService : IUserService
{
    public const int MaxPageSize = 50;

    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository repository,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    // Lista usuarios con paginación y filtros opcionales por rol y estado.
    public async Task<Result<IEnumerable<UserListDto>>> GetPagedAsync(UserQueryDto query)
    {
        ValidatePaging(query.PageNumber, query.PageSize);

        var users = await _repository.GetPagedAsync(query);
        var totalRecords = await _repository.CountAsync(query);

        var data = _mapper.Map<IEnumerable<UserListDto>>(users);

        return Result<IEnumerable<UserListDto>>.Paged(
            data, query.PageNumber, query.PageSize, totalRecords);
    }

    // Devuelve el detalle de un usuario.
    public async Task<Result<UserDetailDto>> GetByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);

        if (user is null)
            throw new NotFoundException($"El usuario {id} no existe.");

        return Result<UserDetailDto>.Success(_mapper.Map<UserDetailDto>(user));
    }

    // Registra un usuario nuevo y almacena únicamente el hash de la contraseña.
    public async Task<Result<UserDetailDto>> AddAsync(UserCreateDto user)
    {
        if (user.Role is null)
            throw new BusinessRuleException("El rol es obligatorio.");

        var fullName = user.FullName!.Trim();
        var username = user.Username!.Trim();
        var email = user.Email!.Trim();

        if (await _repository.ExistsByUsernameAsync(username))
            throw new ConflictException($"Ya existe un usuario con el nombre de usuario '{username}'.");

        if (await _repository.ExistsByEmailAsync(email))
            throw new ConflictException($"Ya existe un usuario con el correo '{email}'.");

        var entity = new User
        {
            FullName = fullName,
            Username = username,
            Email = email,
            PasswordHash = _passwordHasher.Hash(user.Password!),
            Role = user.Role.Value,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastAccessAt = null
        };

        await _repository.AddAsync(entity);

        return await BuildDetailResultAsync(
            entity.IdUser, 201, "Created", "Registro creado.");
    }

    // Actualiza los datos públicos del usuario; no modifica contraseña ni rol.
    public async Task<Result<UserDetailDto>> UpdateAsync(int id, UserUpdateDto user)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            throw new NotFoundException($"El usuario {id} no existe.");

        var fullName = user.FullName!.Trim();
        var username = user.Username!.Trim();
        var email = user.Email!.Trim();

        if (await _repository.ExistsByUsernameAsync(username, id))
            throw new ConflictException($"Ya existe un usuario con el nombre de usuario '{username}'.");

        if (await _repository.ExistsByEmailAsync(email, id))
            throw new ConflictException($"Ya existe un usuario con el correo '{email}'.");

        entity.FullName = fullName;
        entity.Username = username;
        entity.Email = email;

        await _repository.SaveChangesAsync();

        return await BuildDetailResultAsync(id, 200, "OK", "Registro actualizado.");
    }

    // Restablecimiento por administrador: no se solicita CurrentPassword porque
    // el login y la autenticación están fuera de alcance hasta el Avance 2.
    public async Task<Result<UserDetailDto>> ChangePasswordAsync(int id, UserPasswordUpdateDto user)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            throw new NotFoundException($"El usuario {id} no existe.");

        entity.PasswordHash = _passwordHasher.Hash(user.NewPassword!);

        await _repository.SaveChangesAsync();

        return await BuildDetailResultAsync(id, 200, "OK", "Contraseña actualizada.");
    }

    // Asigna o modifica el rol sin permitir que el sistema se quede sin administradores.
    public async Task<Result<UserDetailDto>> ChangeRoleAsync(int id, UserRoleUpdateDto user)
    {
        if (user.Role is null)
            throw new BusinessRuleException("El rol es obligatorio.");

        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            throw new NotFoundException($"El usuario {id} no existe.");

        var newRole = user.Role.Value;

        if (entity.IsActive
            && entity.Role == UserRole.Administrador
            && newRole != UserRole.Administrador
            && await _repository.CountActiveAdministratorsAsync() <= 1)
        {
            throw new BusinessRuleException(
                "No se puede cambiar el rol del último administrador activo.");
        }

        entity.Role = newRole;

        await _repository.SaveChangesAsync();

        return await BuildDetailResultAsync(id, 200, "OK", "Rol actualizado.");
    }

    // Aplica una baja lógica y conserva la fila del usuario.
    public async Task<Result<UserDetailDto>> DeactivateAsync(int id)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            throw new NotFoundException($"El usuario {id} no existe.");

        if (!entity.IsActive)
        {
            var inactive = await _repository.GetByIdAsync(id);
            return Result<UserDetailDto>.Success(
                _mapper.Map<UserDetailDto>(inactive!),
                200, "OK", "El usuario ya estaba desactivado.");
        }

        if (entity.Role == UserRole.Administrador
            && await _repository.CountActiveAdministratorsAsync() <= 1)
        {
            throw new BusinessRuleException(
                "No se puede desactivar al último administrador activo.");
        }

        entity.IsActive = false;

        await _repository.SaveChangesAsync();

        return await BuildDetailResultAsync(id, 200, "OK", "Usuario desactivado.");
    }

    // Reactiva un usuario desactivado.
    public async Task<Result<UserDetailDto>> ActivateAsync(int id)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            throw new NotFoundException($"El usuario {id} no existe.");

        if (entity.IsActive)
        {
            var active = await _repository.GetByIdAsync(id);
            return Result<UserDetailDto>.Success(
                _mapper.Map<UserDetailDto>(active!),
                200, "OK", "El usuario ya estaba activo.");
        }

        entity.IsActive = true;

        await _repository.SaveChangesAsync();

        return await BuildDetailResultAsync(id, 200, "OK", "Usuario activado.");
    }

    private async Task<Result<UserDetailDto>> BuildDetailResultAsync(
        int id, int status, string title, string detail)
    {
        var updated = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<UserDetailDto>(updated!);

        return Result<UserDetailDto>.Success(data, status, title, detail);
    }

    private static void ValidatePaging(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            throw new BusinessRuleException(
                $"pageNumber debe ser 1 o mayor y pageSize debe estar entre 1 y {MaxPageSize}.");
    }
}
