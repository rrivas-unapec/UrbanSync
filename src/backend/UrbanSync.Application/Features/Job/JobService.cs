using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;

namespace UrbanSync.Application.Features.Job
{
    public sealed class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;

        public JobService(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public Task<IReadOnlyList<JobDto>> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default)
        {
            if (incidentId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(incidentId),
                    "El identificador de la incidencia debe ser mayor que cero.");
            }

            return _jobRepository.GetByIncidentIdAsync(incidentId, cancellationToken);
        }

        public Task<JobDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "El identificador del trabajo debe ser mayor que cero.");
            }

            return _jobRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<JobDto> CreateAsync(
            CreateJobDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.IncidentId <= 0)
            {
                throw new ArgumentException("El identificador de la incidencia asociada debe ser mayor que cero.", nameof(dto));
            }

            if (dto.AssignedUserId <= 0)
            {
                throw new ArgumentException("El usuario asignado debe ser válido.", nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.JobDescription))
            {
                throw new ArgumentException("La descripción del trabajo es obligatoria.", nameof(dto));
            }

            dto.JobDescription = dto.JobDescription.Trim();
            dto.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pendiente" : dto.Status.Trim();
            dto.Result = Normalize(dto.Result);

            if (dto.JobDescription.Length > 1000)
            {
                throw new ArgumentException("La descripción del trabajo no puede superar 1000 caracteres.", nameof(dto));
            }

            if (dto.Status.Length > 30)
            {
                throw new ArgumentException("El estado no puede superar 30 caracteres.", nameof(dto));
            }

            if (dto.Result?.Length > 1000)
            {
                throw new ArgumentException("El resultado no puede superar 1000 caracteres.", nameof(dto));
            }

            var newId = await _jobRepository.CreateAsync(dto, cancellationToken);

            var created = await _jobRepository.GetByIdAsync(newId, cancellationToken);

            return created
                ?? throw new InvalidOperationException("El trabajo fue creado pero no se pudo recuperar.");
        }

        public async Task<JobDto?> UpdateAsync(
    UpdateJobDto dto,
    CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.Id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(dto.Id),
                    "El identificador del trabajo debe ser mayor que cero.");
            }

            if (string.IsNullOrWhiteSpace(dto.Status))
            {
                throw new ArgumentException("El estado del trabajo es obligatorio.", nameof(dto));
            }

            dto.Status = dto.Status.Trim();
            dto.Result = Normalize(dto.Result);

            if (dto.Status.Length > 30)
            {
                throw new ArgumentException("El estado no puede superar 30 caracteres.", nameof(dto));
            }

            if (dto.Result?.Length > 1000)
            {
                throw new ArgumentException("El resultado no puede superar 1000 caracteres.", nameof(dto));
            }

            var updated = await _jobRepository.UpdateAsync(dto, cancellationToken);
            if (!updated)
            {
                return null;
            }

            return await _jobRepository.GetByIdAsync(dto.Id, cancellationToken);
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
