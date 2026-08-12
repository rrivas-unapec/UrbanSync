
namespace UrbanSync.Application.Features.Departament
{
    public sealed class DepartmentDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int? JurisdictionId { get; set; }

        public string? JurisdictionName { get; set; }

        public bool IsActive { get; set; }
    }
}
