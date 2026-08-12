using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Location
{
    public sealed class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;

        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public Task<IReadOnlyList<LocationDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return _locationRepository.GetAllAsync(cancellationToken);
        }

        public Task<LocationDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "El identificador de la ubicación debe ser mayor que cero.");
            }

            return _locationRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<LocationDto> CreateAsync(
            CreateLocationDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (string.IsNullOrWhiteSpace(dto.Address))
            {
                throw new ArgumentException("La dirección de la ubicación es obligatoria.", nameof(dto));
            }

            if (dto.JurisdictionId <= 0)
            {
                throw new ArgumentException("La jurisdicción asociada debe ser mayor que cero.", nameof(dto));
            }

            dto.Address = dto.Address.Trim();
            dto.Reference = Normalize(dto.Reference);

            if (dto.Address.Length > 250)
            {
                throw new ArgumentException("La dirección no puede superar 250 caracteres.", nameof(dto));
            }

            if (dto.Reference?.Length > 250)
            {
                throw new ArgumentException("La referencia no puede superar 250 caracteres.", nameof(dto));
            }

            var newId = await _locationRepository.CreateAsync(dto, cancellationToken);

            var created = await _locationRepository.GetByIdAsync(newId, cancellationToken);

            return created
                ?? throw new InvalidOperationException("La ubicación fue creada pero no se pudo recuperar.");
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
