using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;

namespace FlotaVehicular.Application.Interfaces;

// Casos de uso de la gestión de técnicos.
public interface ITechnicianService
{
    Task<Result<IEnumerable<TechnicianListDto>>> GetAllAsync(TechnicianQueryDto query);
    Task<Result<IEnumerable<TechnicianListDto>>> GetPagedAsync(TechnicianQueryDto query);
    Task<Result<TechnicianDetailDto>> GetByIdAsync(int id);
    Task<Result<TechnicianDetailDto>> AddAsync(TechnicianCreateDto technician);
    Task<Result<TechnicianDetailDto>> UpdateAsync(int id, TechnicianUpdateDto technician);
    Task<Result<TechnicianDetailDto>> DeactivateAsync(int id);
    Task<Result<TechnicianDetailDto>> ActivateAsync(int id);
    Task<Result<IEnumerable<TechnicianListDto>>> GetAvailableBySpecialtyAsync(int idSpecialty);
}
