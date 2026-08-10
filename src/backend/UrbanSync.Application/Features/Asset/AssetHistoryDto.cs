

namespace UrbanSync.Application.Features.Asset
{
    public sealed class AssetHistoryDto
    {
        public int IncidentId { get; set; }
        public string CaseCode { get; set; } = string.Empty;
        public string IncidentType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
    }
}
