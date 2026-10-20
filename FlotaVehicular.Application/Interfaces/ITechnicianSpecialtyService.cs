using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;

namespace FlotaVehicular.Application.Interfaces;

public interface ITechnicianSpecialtyService
{
    Task<Result<IEnumerable<TechnicianSpecialtyListDto>>> GetAllAsync(TechnicianSpecialtyQueryDto query);
    Task<Result<IEnumerable<TechnicianSpecialtyListDto>>> GetPagedAsync(TechnicianSpecialtyQueryDto query);
    Task<Result<TechnicianSpecialtyListDto>> GetByIdAsync(int idTechnician, int idSpecialty);
    Task<Result<TechnicianSpecialtyListDto>> AddAsync(TechnicianSpecialtyCreateDto link);
    Task<Result<TechnicianSpecialtyListDto>> UpdateAsync(int idTechnician, int idSpecialty, TechnicianSpecialtyUpdateDto link);
    Task<Result<int>> DeleteAsync(int idTechnician, int idSpecialty);
}
