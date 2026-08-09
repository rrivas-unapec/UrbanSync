
using UrbanSync.Application.Common.Interfaces.Persistence;

namespace UrbanSync.Application.Features.Asset
{
    public sealed class AssetService : IAssetService
    {
        private readonly IAssetRepository _assetRepository;

        public AssetService(IAssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
        }

        public Task<IReadOnlyList<AssetDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return _assetRepository.GetAllAsync(cancellationToken);
        }

        public Task<AssetDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "El identificador del activo debe ser mayor que cero.");
            }

            return _assetRepository.GetByIdAsync(id, cancellationToken);
        }

        public Task<AssetHistoryDto?> GetHistoryByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "El identificador del activo debe ser mayor que cero.");
            }

            return _assetRepository.GetHistoryByIdAsync(id, cancellationToken);
        }

        public async Task<AssetDto> CreateAsync(
            CreateAssetDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (string.IsNullOrWhiteSpace(dto.Code))
            {
                throw new ArgumentException("El código del activo es obligatorio.", nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("El nombre del activo es obligatorio.", nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.Type))
            {
                throw new ArgumentException("El tipo de activo es obligatorio.", nameof(dto));
            }

            if (dto.JurisdictionId <= 0)
            {
                throw new ArgumentException("La jurisdicción asociada debe ser mayor que cero.", nameof(dto));
            }

            dto.Code = dto.Code.Trim();
            dto.Name = dto.Name.Trim();
            dto.Type = dto.Type.Trim();
            dto.Status = Normalize(dto.Status) ?? "Operativo";

            if (dto.Code.Length > 50)
            {
                throw new ArgumentException("El código no puede superar 50 caracteres.", nameof(dto));
            }

            if (dto.Name.Length > 100)
            {
                throw new ArgumentException("El nombre no puede superar 100 caracteres.", nameof(dto));
            }

            if (dto.Type.Length > 50)
            {
                throw new ArgumentException("El tipo no puede superar 50 caracteres.", nameof(dto));
            }

            var newAssetId = await _assetRepository.CreateAsync(dto, cancellationToken);

            var createdAsset = await _assetRepository.GetByIdAsync(newAssetId, cancellationToken);

            return createdAsset
                ?? throw new InvalidOperationException("El activo fue creado pero no se pudo recuperar.");
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
