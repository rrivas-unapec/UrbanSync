using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;

namespace UrbanSync.Application.Features.Report
{
    public sealed class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public Task<IReadOnlyList<ReportDto>> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default)
        {
            if (incidentId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(incidentId),
                    "El identificador de la incidencia debe ser mayor que cero.");
            }

            return _reportRepository.GetByIncidentIdAsync(incidentId, cancellationToken);
        }

        public Task<ReportDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "El identificador del reporte debe ser mayor que cero.");
            }

            return _reportRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<ReportDto> CreateAsync(
            CreateReportDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.IncidentId <= 0)
            {
                throw new ArgumentException("La incidencia asociada debe ser válida.", nameof(dto));
            }

            if (dto.JobId.HasValue && dto.JobId.Value <= 0)
            {
                throw new ArgumentException("El trabajo asociado debe ser mayor que cero.", nameof(dto));
            }

            if (dto.GeneratedByUserId <= 0)
            {
                throw new ArgumentException("El usuario generador debe ser válido.", nameof(dto));
            }

            dto.Content = Normalize(dto.Content);
            dto.FilePath = Normalize(dto.FilePath);

            if (string.IsNullOrWhiteSpace(dto.Content) && string.IsNullOrWhiteSpace(dto.FilePath))
            {
                throw new ArgumentException("Debe proporcionar al menos el contenido en texto o la ruta del archivo del reporte.", nameof(dto));
            }

            if (dto.FilePath?.Length > 400)
            {
                throw new ArgumentException("La ruta del archivo no puede superar 400 caracteres.", nameof(dto));
            }

            var newId = await _reportRepository.CreateAsync(dto, cancellationToken);

            var created = await _reportRepository.GetByIdAsync(newId, cancellationToken);

            return created
                ?? throw new InvalidOperationException("El reporte fue creado pero no se pudo recuperar.");
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
