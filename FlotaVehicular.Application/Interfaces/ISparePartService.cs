using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;

namespace FlotaVehicular.Application.Interfaces;

// Casos de uso de la gestión del catálogo de repuestos.
public interface ISparePartService
{
    Task<Result<IEnumerable<SparePartListDto>>> GetAllAsync(SparePartQueryDto query);
    Task<Result<IEnumerable<SparePartListDto>>> GetPagedAsync(SparePartQueryDto query);

    Task<Result<IEnumerable<SparePartListDto>>> GetBelowMinimumStockAsync(int pageNumber, int pageSize);
    Task<Result<SparePartDetailDto>> GetByIdAsync(int id);
    Task<Result<SparePartDetailDto>> AddAsync(SparePartCreateDto sparePart);
    Task<Result<SparePartDetailDto>> UpdateAsync(int id, SparePartUpdateDto sparePart);
    Task<Result<int>> DeleteAsync(int id);
}
