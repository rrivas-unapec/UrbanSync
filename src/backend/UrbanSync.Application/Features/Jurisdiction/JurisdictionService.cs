using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;

namespace UrbanSync.Application.Features.Jurisdiction
{
    public sealed class JurisdictionService : IJurisdictionService
    {
        private readonly IJurisdictionRepository _jurisdictionRepository;

        public JurisdictionService(IJurisdictionRepository jurisdictionRepository)
        {
            _jurisdictionRepository = jurisdictionRepository;
        }

        public Task<IReadOnlyList<JurisdictionDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return _jurisdictionRepository.GetAllAsync(cancellationToken);
        }

        public Task<JurisdictionDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "El identificador de la jurisdicción debe ser mayor que cero.");
            }

            return _jurisdictionRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<JurisdictionDto> CreateAsync(
            CreateJurisdictionDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("El nombre de la jurisdicción es obligatorio.", nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.Level))
            {
                throw new ArgumentException("El nivel de la jurisdicción es obligatorio.", nameof(dto));
            }

            if (dto.ParentJurisdictionId.HasValue && dto.ParentJurisdictionId.Value <= 0)
            {
                throw new ArgumentException("El identificador de la jurisdicción padre debe ser mayor que cero.", nameof(dto));
            }

            dto.Name = dto.Name.Trim();
            dto.Level = dto.Level.Trim();

            if (dto.Name.Length > 100)
            {
                throw new ArgumentException("El nombre no puede superar 100 caracteres.", nameof(dto));
            }

            if (dto.Level.Length > 30)
            {
                throw new ArgumentException("El nivel no puede superar 30 caracteres.", nameof(dto));
            }

            var newId = await _jurisdictionRepository.CreateAsync(dto, cancellationToken);

            var created = await _jurisdictionRepository.GetByIdAsync(newId, cancellationToken);

            return created
                ?? throw new InvalidOperationException("La jurisdicción fue creada pero no se pudo recuperar.");
        }
    }
}
