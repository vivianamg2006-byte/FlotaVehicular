using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Interfaces;
using MapsterMapper;

namespace FlotaVehicular.Application.Implementation;

// Casos de uso de las asociaciones entre técnicos y especialidades.
public class TechnicianSpecialtyService : ITechnicianSpecialtyService
{
    public const int MaxPageSize = 50;

    private readonly ITechnicianSpecialtyRepository _repository;
    private readonly IMapper _mapper;

    public TechnicianSpecialtyService(ITechnicianSpecialtyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    // Lista las asociaciones que cumplen el filtro indicado.
    public async Task<Result<IEnumerable<TechnicianSpecialtyListDto>>> GetAllAsync(TechnicianSpecialtyQueryDto query)
    {
        var links = await _repository.GetAllAsync(query);

        var data = _mapper.Map<IEnumerable<TechnicianSpecialtyListDto>>(links);

        return Result<IEnumerable<TechnicianSpecialtyListDto>>.Success(data);
    }

    // Lista asociaciones con paginación y filtros.
    public async Task<Result<IEnumerable<TechnicianSpecialtyListDto>>> GetPagedAsync(TechnicianSpecialtyQueryDto query)
    {
        var pagingError = ValidatePaging(query.PageNumber, query.PageSize);
        if (pagingError is not null)
            return Result<IEnumerable<TechnicianSpecialtyListDto>>.Failure(400, "Bad Request", pagingError);

        var links = await _repository.GetPagedAsync(query);
        var totalRecords = await _repository.CountAsync(query);

        var data = _mapper.Map<IEnumerable<TechnicianSpecialtyListDto>>(links);

        return Result<IEnumerable<TechnicianSpecialtyListDto>>.Paged(data, query.PageNumber, query.PageSize, totalRecords);
    }

    // Devuelve el detalle de una asociación.
    public async Task<Result<TechnicianSpecialtyListDto>> GetByIdAsync(int idTechnician, int idSpecialty)
    {
        var link = await _repository.GetByIdAsync(idTechnician, idSpecialty);

        if (link is null)
            return Result<TechnicianSpecialtyListDto>.Failure(404, "Not Found",
                $"La asociación técnico {idTechnician} – especialidad {idSpecialty} no existe.");

        return Result<TechnicianSpecialtyListDto>.Success(_mapper.Map<TechnicianSpecialtyListDto>(link));
    }

    // Asocia un técnico con una especialidad.
    public async Task<Result<TechnicianSpecialtyListDto>> AddAsync(TechnicianSpecialtyCreateDto link)
    {
        var referenceError = await ValidateReferencesAsync(link.IdTechnician, link.IdSpecialty);
        if (referenceError is not null)
            return Result<TechnicianSpecialtyListDto>.Failure(referenceError.Value.Code,
                referenceError.Value.Title, referenceError.Value.Detail);

        // Un técnico no puede certificar dos veces la misma especialidad.
        if (await _repository.ExistsAsync(link.IdTechnician, link.IdSpecialty))
            return Result<TechnicianSpecialtyListDto>.Failure(409, "Conflict",
                $"El técnico {link.IdTechnician} ya tiene asociada la especialidad {link.IdSpecialty}.");

        var entity = _mapper.Map<TechnicianSpecialty>(link);
        entity.CertificationLevel = link.CertificationLevel!.Trim();

        await _repository.AddAsync(entity);

        var created = await _repository.GetByIdAsync(entity.IdTechnician, entity.IdSpecialty);
        var data = _mapper.Map<TechnicianSpecialtyListDto>(created!);

        return Result<TechnicianSpecialtyListDto>.Success(data, 201, "Created", "Asociación creada.");
    }

    // Actualiza la certificación de la asociación.
    public async Task<Result<TechnicianSpecialtyListDto>> UpdateAsync(int idTechnician, int idSpecialty, TechnicianSpecialtyUpdateDto link)
    {
        var entity = await _repository.GetForUpdateAsync(idTechnician, idSpecialty);

        if (entity is null)
            return Result<TechnicianSpecialtyListDto>.Failure(404, "Not Found",
                $"La asociación técnico {idTechnician} – especialidad {idSpecialty} no existe.");

        entity.CertificationLevel = link.CertificationLevel!.Trim();
        entity.SinceDate = link.SinceDate;

        await _repository.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(idTechnician, idSpecialty);
        var data = _mapper.Map<TechnicianSpecialtyListDto>(updated!);

        return Result<TechnicianSpecialtyListDto>.Success(data, 200, "OK", "Certificación actualizada.");
    }

    // Desasocia una especialidad del técnico.
    public async Task<Result<int>> DeleteAsync(int idTechnician, int idSpecialty)
    {
        var entity = await _repository.GetForUpdateAsync(idTechnician, idSpecialty);

        if (entity is null)
            return Result<int>.Failure(404, "Not Found",
                $"La asociación técnico {idTechnician} – especialidad {idSpecialty} no existe.");

        _repository.Remove(entity);
        await _repository.SaveChangesAsync();

        return Result<int>.Success(idTechnician, 200, "OK",
            $"Especialidad {idSpecialty} desasociada del técnico {idTechnician}.");
    }

    private async Task<(int Code, string Title, string Detail)?> ValidateReferencesAsync(int idTechnician, int idSpecialty)
    {
        var technician = await _repository.GetTechnicianAsync(idTechnician);

        if (technician is null)
            return (404, "Not Found", $"El técnico {idTechnician} no existe.");

        if (!technician.IsActive)
            return (400, "Bad Request",
                $"El técnico {idTechnician} está desactivado; reactívelo antes de asociarle especialidades.");

        if (await _repository.GetSpecialtyAsync(idSpecialty) is null)
            return (404, "Not Found", $"La especialidad {idSpecialty} no existe.");

        return null;
    }

    private static string? ValidatePaging(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return $"pageNumber debe ser 1 o mayor y pageSize debe estar entre 1 y {MaxPageSize}.";

        return null;
    }
}
