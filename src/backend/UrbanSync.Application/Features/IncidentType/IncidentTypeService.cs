using System;
using System.Collections.Generic;
using UrbanSync.Application.Common.Interfaces.Persistence;

namespace UrbanSync.Application.Features.IncidentType
{
    public sealed class IncidentTypeService : IIncidentTypeService
    {
        private readonly IIncidentTypeRepository _incidentTypeRepository;

        public IncidentTypeService(IIncidentTypeRepository incidentTypeRepository)
        {
            _incidentTypeRepository = incidentTypeRepository;
        }

        public Task<IReadOnlyList<IncidentTypeDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return _incidentTypeRepository.GetAllAsync(cancellationToken);
        }

        public Task<IncidentTypeDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "El identificador del tipo de incidencia debe ser mayor que cero.");
            }

            return _incidentTypeRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IncidentTypeDto> CreateAsync(
            CreateIncidentTypeDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("El nombre del tipo de incidencia es obligatorio.", nameof(dto));
            }

            if (dto.InstitutionId <= 0)
            {
                throw new ArgumentException("La institución asociada debe ser mayor que cero.", nameof(dto));
            }

            dto.Name = dto.Name.Trim();
            dto.Description = Normalize(dto.Description);

            if (dto.Name.Length > 100)
            {
                throw new ArgumentException("El nombre no puede superar los 100 caracteres.", nameof(dto));
            }

            if (dto.Description?.Length > 250)
            {
                throw new ArgumentException("La descripción no puede superar los 250 caracteres.", nameof(dto));
            }

            var newId = await _incidentTypeRepository.CreateAsync(dto, cancellationToken);

            var created = await _incidentTypeRepository.GetByIdAsync(newId, cancellationToken);

            return created
                ?? throw new InvalidOperationException("El tipo de incidencia fue creado pero no se pudo recuperar.");
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
