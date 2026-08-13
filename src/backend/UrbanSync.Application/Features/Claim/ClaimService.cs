using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;

namespace UrbanSync.Application.Features.Claim
{
    public sealed class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepository;

        public ClaimService(IClaimRepository claimRepository)
        {
            _claimRepository = claimRepository;
        }

        public Task<IReadOnlyList<ClaimDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return _claimRepository.GetAllAsync(cancellationToken);
        }

        public Task<IReadOnlyList<ClaimDto>> GetByCitizenIdAsync(
            int citizenUserId,
            CancellationToken cancellationToken = default)
        {
            if (citizenUserId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(citizenUserId),
                    "El identificador del ciudadano debe ser mayor que cero.");
            }

            return _claimRepository.GetByCitizenIdAsync(citizenUserId, cancellationToken);
        }

        public Task<ClaimDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "El identificador de la reclamación debe ser mayor que cero.");
            }

            return _claimRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<ClaimDto> CreateAsync(
            CreateClaimDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.CitizenUserId <= 0)
            {
                throw new ArgumentException("El usuario ciudadano debe ser válido.", nameof(dto));
            }

            if (dto.LocationId <= 0)
            {
                throw new ArgumentException("La ubicación asociada debe ser válida.", nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.Category))
            {
                throw new ArgumentException("La categoría es obligatoria.", nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                throw new ArgumentException("El título es obligatorio.", nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.Description))
            {
                throw new ArgumentException("La descripción es obligatoria.", nameof(dto));
            }

            dto.Category = dto.Category.Trim();
            dto.Title = dto.Title.Trim();
            dto.Description = dto.Description.Trim();

            if (dto.Category.Length > 50)
            {
                throw new ArgumentException("La categoría no puede superar 50 caracteres.", nameof(dto));
            }

            if (dto.Title.Length > 150)
            {
                throw new ArgumentException("El título no puede superar 150 caracteres.", nameof(dto));
            }

            if (dto.Description.Length > 1000)
            {
                throw new ArgumentException("La descripción no puede superar 1000 caracteres.", nameof(dto));
            }

            var newId = await _claimRepository.CreateAsync(dto, cancellationToken);

            var created = await _claimRepository.GetByIdAsync(newId, cancellationToken);

            return created
                ?? throw new InvalidOperationException("La reclamación fue creada pero no se pudo recuperar.");
        }

        public async Task<ClaimDto?> UpdateStatusAsync(
            UpdateClaimStatusDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.Id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(dto.Id),
                    "El identificador de la reclamación debe ser mayor que cero.");
            }

            if (string.IsNullOrWhiteSpace(dto.Status))
            {
                throw new ArgumentException("El estado es obligatorio.", nameof(dto));
            }

            dto.Status = dto.Status.Trim();

            if (dto.Status.Length > 30)
            {
                throw new ArgumentException("El estado no puede superar 30 caracteres.", nameof(dto));
            }

            var updated = await _claimRepository.UpdateStatusAsync(dto, cancellationToken);
            if (!updated)
            {
                return null;
            }

            return await _claimRepository.GetByIdAsync(dto.Id, cancellationToken);
        }
    }
}
