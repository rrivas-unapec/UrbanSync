using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;

namespace UrbanSync.Application.Features.Evidence
{
    public sealed class EvidenceService : IEvidenceService
    {
        private readonly IEvidenceRepository _evidenceRepository;

        public EvidenceService(IEvidenceRepository evidenceRepository)
        {
            _evidenceRepository = evidenceRepository;
        }

        public Task<IReadOnlyList<EvidenceDto>> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default)
        {
            if (incidentId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(incidentId),
                    "El identificador de la incidencia debe ser mayor que cero.");
            }

            return _evidenceRepository.GetByIncidentIdAsync(incidentId, cancellationToken);
        }

        public Task<EvidenceDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "El identificador de la evidencia debe ser mayor que cero.");
            }

            return _evidenceRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<EvidenceDto> CreateAsync(
            CreateEvidenceDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.IncidentId <= 0)
            {
                throw new ArgumentException("El identificador de la incidencia asociada debe ser mayor que cero.", nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.EvidenceType))
            {
                throw new ArgumentException("El tipo de evidencia es obligatorio.", nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.FilePath))
            {
                throw new ArgumentException("La ruta del archivo es obligatoria.", nameof(dto));
            }

            if (dto.UploadedByUserId <= 0)
            {
                throw new ArgumentException("El usuario que sube el archivo debe ser válido.", nameof(dto));
            }

            dto.EvidenceType = dto.EvidenceType.Trim();
            dto.FilePath = dto.FilePath.Trim();
            dto.Description = Normalize(dto.Description);

            if (dto.EvidenceType.Length > 20)
            {
                throw new ArgumentException("El tipo de evidencia no puede superar los 20 caracteres.", nameof(dto));
            }

            if (dto.FilePath.Length > 400)
            {
                throw new ArgumentException("La ruta del archivo no puede superar los 400 caracteres.", nameof(dto));
            }

            if (dto.Description?.Length > 300)
            {
                throw new ArgumentException("La descripción no puede superar los 300 caracteres.", nameof(dto));
            }

            var newId = await _evidenceRepository.CreateAsync(dto, cancellationToken);

            var created = await _evidenceRepository.GetByIdAsync(newId, cancellationToken);

            return created
                ?? throw new InvalidOperationException("La evidencia fue creada pero no se pudo recuperar.");
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
