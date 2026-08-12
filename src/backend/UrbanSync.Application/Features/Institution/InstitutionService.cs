using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;

namespace UrbanSync.Application.Features.Institution
{
    public sealed class InstitutionService : IInstitutionService
    {
        private readonly IInstitutionRepository _institutionRepository;

        public InstitutionService(IInstitutionRepository institutionRepository)
        {
            _institutionRepository = institutionRepository;
        }

        public Task<IReadOnlyList<InstitutionDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return _institutionRepository.GetAllAsync(cancellationToken);
        }

        public Task<InstitutionDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "El identificador de la institución debe ser mayor que cero.");
            }

            return _institutionRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<InstitutionDto> CreateAsync(
            CreateInstitutionDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("El nombre de la institución es obligatorio.", nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.InstitutionType))
            {
                throw new ArgumentException("El tipo de institución es obligatorio.", nameof(dto));
            }

            dto.Name = dto.Name.Trim();
            dto.InstitutionType = dto.InstitutionType.Trim();
            dto.ContactEmail = Normalize(dto.ContactEmail);
            dto.ContactPhone = Normalize(dto.ContactPhone);

            if (dto.Name.Length > 150)
            {
                throw new ArgumentException("El nombre no puede superar 150 caracteres.", nameof(dto));
            }

            if (dto.InstitutionType.Length > 50)
            {
                throw new ArgumentException("El tipo de institución no puede superar 50 caracteres.", nameof(dto));
            }

            if (dto.ContactEmail?.Length > 150)
            {
                throw new ArgumentException("El correo de contacto no puede superar 150 caracteres.", nameof(dto));
            }

            if (dto.ContactPhone?.Length > 30)
            {
                throw new ArgumentException("El teléfono de contacto no puede superar 30 caracteres.", nameof(dto));
            }

            var newId = await _institutionRepository.CreateAsync(dto, cancellationToken);

            var created = await _institutionRepository.GetByIdAsync(newId, cancellationToken);

            return created
                ?? throw new InvalidOperationException("La institución fue creada pero no se pudo recuperar.");
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
