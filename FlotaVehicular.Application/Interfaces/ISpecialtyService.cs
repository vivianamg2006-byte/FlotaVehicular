using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;

namespace FlotaVehicular.Application.Interfaces;

public interface ISpecialtyService
{
    Task<Result<IEnumerable<SpecialtyListDto>>> GetAllAsync(SpecialtyQueryDto query);
    Task<Result<IEnumerable<SpecialtyListDto>>> GetPagedAsync(SpecialtyQueryDto query);
    Task<Result<SpecialtyDetailDto>> GetByIdAsync(int id);
    Task<Result<SpecialtyDetailDto>> AddAsync(SpecialtyCreateDto specialty);
    Task<Result<SpecialtyDetailDto>> UpdateAsync(int id, SpecialtyUpdateDto specialty);
    Task<Result<int>> DeleteAsync(int id);
}
