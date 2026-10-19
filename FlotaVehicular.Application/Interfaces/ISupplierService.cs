using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;

namespace FlotaVehicular.Application.Interfaces;

public interface ISupplierService
{
    Task<Result<IEnumerable<SupplierListDto>>> GetAllAsync(SupplierQueryDto query);
    Task<Result<IEnumerable<SupplierListDto>>> GetPagedAsync(SupplierQueryDto query);
    Task<Result<SupplierDetailDto>> GetByIdAsync(int id);
    Task<Result<SupplierDetailDto>> AddAsync(SupplierCreateDto supplier);
    Task<Result<SupplierDetailDto>> UpdateAsync(int id, SupplierUpdateDto supplier);

    Task<Result<SupplierDetailDto>> DeactivateAsync(int id);
    Task<Result<SupplierDetailDto>> ActivateAsync(int id);

    Task<Result<int>> DeleteAsync(int id);
}
