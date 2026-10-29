using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;

namespace FlotaVehicular.Application.Interfaces;

// Casos de uso de la gestión de usuarios.
public interface IUserService
{
    Task<Result<IEnumerable<UserListDto>>> GetPagedAsync(UserQueryDto query);
    Task<Result<UserDetailDto>> GetByIdAsync(int id);
    Task<Result<UserDetailDto>> AddAsync(UserCreateDto user);
    Task<Result<UserDetailDto>> UpdateAsync(int id, UserUpdateDto user);
    Task<Result<UserDetailDto>> ChangePasswordAsync(int id, UserPasswordUpdateDto user);
    Task<Result<UserDetailDto>> ChangeRoleAsync(int id, UserRoleUpdateDto user);
    Task<Result<UserDetailDto>> DeactivateAsync(int id);
    Task<Result<UserDetailDto>> ActivateAsync(int id);
}
