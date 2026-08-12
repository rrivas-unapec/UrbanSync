using UrbanSync.Application.Common.Interfaces.Persistence;

namespace UrbanSync.Application.Features.Departament
{
    public sealed class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentService(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public Task<IReadOnlyList<DepartmentDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return _departmentRepository.GetAllAsync(cancellationToken);
        }

        public Task<DepartmentDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "El identificador del departamento debe ser mayor que cero.");
            }

            return _departmentRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<DepartmentDto> CreateAsync(
            CreateDepartmentDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("El nombre del departamento es obligatorio.", nameof(dto));
            }

            if (dto.JurisdictionId.HasValue && dto.JurisdictionId.Value <= 0)
            {
                throw new ArgumentException("El identificador de la jurisdicción debe ser mayor que cero.", nameof(dto));
            }

            dto.Name = dto.Name.Trim();

            if (dto.Name.Length > 100)
            {
                throw new ArgumentException("El nombre no puede superar 100 caracteres.", nameof(dto));
            }

            var newId = await _departmentRepository.CreateAsync(dto, cancellationToken);

            var created = await _departmentRepository.GetByIdAsync(newId, cancellationToken);

            return created
                ?? throw new InvalidOperationException("El departamento fue creado pero no se pudo recuperar.");
        }
    }
}
