using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;

namespace FlotaVehicular.Application.Interfaces;

public interface IVehicleImageService
{
    Task<Result<IEnumerable<VehicleImageDto>>> GetImagesAsync(int vehicleId);
    Task<Result<VehicleImageDto>> UploadAsync(int vehicleId, VehicleImageUploadDto image);

    Task<Result<VehicleImageDto>> ReplaceAsync(int vehicleId, int imageId, VehicleImageUploadDto image);
    Task<Result<int>> DeleteAsync(int vehicleId, int imageId);

    Task<Result<VehicleImageFileDto>> GetFileAsync(int vehicleId, int imageId);
}
