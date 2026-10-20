using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Interfaces;
using MapsterMapper;

namespace FlotaVehicular.Application.Implementation;

public class SpecialtyService : ISpecialtyService
{
    public const int MaxPageSize = 50;

    private readonly ISpecialtyRepository _repository;
    private readonly IMapper _mapper;

    public SpecialtyService(ISpecialtyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<SpecialtyListDto>>> GetAllAsync(SpecialtyQueryDto query)
    {
        var specialties = await _repository.GetAllAsync(query);

        var data = _mapper.Map<IEnumerable<SpecialtyListDto>>(specialties);

        return Result<IEnumerable<SpecialtyListDto>>.Success(data);
    }

    public async Task<Result<IEnumerable<SpecialtyListDto>>> GetPagedAsync(SpecialtyQueryDto query)
    {
        var pagingError = ValidatePaging(query.PageNumber, query.PageSize);
        if (pagingError is not null)
            return Result<IEnumerable<SpecialtyListDto>>.Failure(400, "Bad Request", pagingError);

        var specialties = await _repository.GetPagedAsync(query);
        var totalRecords = await _repository.CountAsync(query);

        var data = _mapper.Map<IEnumerable<SpecialtyListDto>>(specialties);

        return Result<IEnumerable<SpecialtyListDto>>.Paged(data, query.PageNumber, query.PageSize, totalRecords);
    }

    public async Task<Result<SpecialtyDetailDto>> GetByIdAsync(int id)
    {
        var specialty = await _repository.GetByIdAsync(id);

        if (specialty is null)
            return Result<SpecialtyDetailDto>.Failure(404, "Not Found", $"La especialidad {id} no existe.");

        return Result<SpecialtyDetailDto>.Success(_mapper.Map<SpecialtyDetailDto>(specialty));
    }

    public async Task<Result<SpecialtyDetailDto>> AddAsync(SpecialtyCreateDto specialty)
    {
        var name = specialty.Name!.Trim();

        if (await _repository.ExistsByNameAsync(name))
            return Result<SpecialtyDetailDto>.Failure(409, "Conflict",
                $"Ya existe una especialidad con el nombre '{name}'.");

        var entity = _mapper.Map<Specialty>(specialty);
        entity.Name = name;
        entity.Description = specialty.Description?.Trim();

        await _repository.AddAsync(entity);

        var data = _mapper.Map<SpecialtyDetailDto>(entity);

        return Result<SpecialtyDetailDto>.Success(data, 201, "Created", "Registro creado.");
    }

    public async Task<Result<SpecialtyDetailDto>> UpdateAsync(int id, SpecialtyUpdateDto specialty)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return Result<SpecialtyDetailDto>.Failure(404, "Not Found", $"La especialidad {id} no existe.");

        var name = specialty.Name!.Trim();

        if (await _repository.ExistsByNameAsync(name, id))
            return Result<SpecialtyDetailDto>.Failure(409, "Conflict",
                $"Ya existe una especialidad con el nombre '{name}'.");

        entity.Name = name;
        entity.Description = specialty.Description?.Trim();

        await _repository.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<SpecialtyDetailDto>(updated!);

        return Result<SpecialtyDetailDto>.Success(data, 200, "OK", "Registro actualizado.");
    }

    public async Task<Result<int>> DeleteAsync(int id)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return Result<int>.Failure(404, "Not Found", $"La especialidad {id} no existe.");

        // Una especialidad asociada a técnicos no se puede eliminar.
        if (await _repository.HasTechniciansAsync(id))
            return Result<int>.Failure(409, "Conflict",
                $"La especialidad {id} está asociada a uno o más técnicos; desasóciela antes de eliminarla.");

        _repository.Remove(entity);
        await _repository.SaveChangesAsync();

        return Result<int>.Success(id, 200, "OK", "Registro eliminado.");
    }

    private static string? ValidatePaging(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return $"pageNumber debe ser 1 o mayor y pageSize debe estar entre 1 y {MaxPageSize}.";

        return null;
    }
}
