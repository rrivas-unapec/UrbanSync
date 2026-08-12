using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;

namespace UrbanSync.Application.Features.TechnicalAnalysis
{
    public sealed class TechnicalAnalysisService : ITechnicalAnalysisService
    {
        private readonly ITechnicalAnalysisRepository _technicalAnalysisRepository;

        public TechnicalAnalysisService(ITechnicalAnalysisRepository technicalAnalysisRepository)
        {
            _technicalAnalysisRepository = technicalAnalysisRepository;
        }

        public Task<TechnicalAnalysisDto?> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default)
        {
            if (incidentId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(incidentId),
                    "El identificador de la incidencia debe ser mayor que cero.");
            }

            return _technicalAnalysisRepository.GetByIncidentIdAsync(incidentId, cancellationToken);
        }

        public Task<TechnicalAnalysisDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "El identificador del análisis técnico debe ser mayor que cero.");
            }

            return _technicalAnalysisRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<TechnicalAnalysisDto> CreateAsync(
            CreateTechnicalAnalysisDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.IncidentId <= 0)
            {
                throw new ArgumentException("El identificador de la incidencia asociada debe ser mayor que cero.", nameof(dto));
            }

            if (dto.TechnicalUserId <= 0)
            {
                throw new ArgumentException("El identificador del usuario técnico debe ser válido.", nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.Diagnosis))
            {
                throw new ArgumentException("El diagnóstico es obligatorio.", nameof(dto));
            }

            dto.Diagnosis = dto.Diagnosis.Trim();
            dto.RecommendedActions = Normalize(dto.RecommendedActions);

            if (dto.Diagnosis.Length > 1000)
            {
                throw new ArgumentException("El diagnóstico no puede superar los 1000 caracteres.", nameof(dto));
            }

            if (dto.RecommendedActions?.Length > 1000)
            {
                throw new ArgumentException("Las acciones recomendadas no pueden superar los 1000 caracteres.", nameof(dto));
            }

            var newId = await _technicalAnalysisRepository.CreateAsync(dto, cancellationToken);

            var created = await _technicalAnalysisRepository.GetByIdAsync(newId, cancellationToken);

            return created
                ?? throw new InvalidOperationException("El análisis técnico fue creado pero no se pudo recuperar.");
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
