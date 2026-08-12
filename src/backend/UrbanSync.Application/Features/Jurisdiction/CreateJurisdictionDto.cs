
namespace UrbanSync.Application.Features.Jurisdiction
{
    public sealed class CreateJurisdictionDto
    {
        public string Name { get; set; } = string.Empty;

        public string Level { get; set; } = string.Empty;

        public int? ParentJurisdictionId { get; set; }
    }
}
