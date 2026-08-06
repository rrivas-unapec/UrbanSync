using Moq;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Audit;
using UrbanSync.Application.Features.Incidents;

namespace UrbanSync.Application.UnitTests.Features.Incidents;

public sealed class IncidentServiceAuditTests
{
    private const int ActingUserId = 9;

    private readonly Mock<IIncidentRepository>
        _incidentRepositoryMock;

    private readonly Mock<IAuditService>
        _auditServiceMock;

    private readonly IncidentService _service;

    public IncidentServiceAuditTests()
    {
        _incidentRepositoryMock =
            new Mock<IIncidentRepository>();

        _auditServiceMock =
            new Mock<IAuditService>();

        _auditServiceMock
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateAuditDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuditDto());

        _service = new IncidentService(
            _incidentRepositoryMock.Object,
            _auditServiceMock.Object);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldRecordAudit_WithStatusTransition()
    {
        SetupIncident(
            before: Incident(estado: "Asignada"),
            after: Incident(estado: "EnProceso"));

        _incidentRepositoryMock
            .Setup(repository => repository.UpdateStatusAsync(
                1,
                "EnProceso",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _service.UpdateStatusAsync(
            1,
            new UpdateIncidentStatusDto { Estado = "EnProceso" },
            ActingUserId);

        var audit = CapturedAudit();

        Assert.Equal(ActingUserId, audit.UserId);
        Assert.Equal("Cambio de estado", audit.Action);
        Assert.Equal("Incidencias", audit.Entity);
        Assert.Equal(1, audit.EntityId);
        Assert.Contains(
            "Estado: Asignada → EnProceso",
            audit.Detail);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldOmitFields_ThatDidNotChange()
    {
        SetupIncident(
            before: Incident(estado: "Asignada"),
            after: Incident(estado: "EnProceso"));

        _incidentRepositoryMock
            .Setup(repository => repository.UpdateStatusAsync(
                1,
                "EnProceso",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _service.UpdateStatusAsync(
            1,
            new UpdateIncidentStatusDto { Estado = "EnProceso" },
            ActingUserId);

        Assert.DoesNotContain(
            "Institución asignada",
            CapturedAudit().Detail);
    }

    [Fact]
    public async Task TriageAsync_ShouldRecordAudit_WithEveryChangedField()
    {
        SetupIncident(
            before: Incident(
                estado: "Registrada",
                prioridad: "Media"),
            after: Incident(
                estado: "Asignada",
                prioridad: "Alta"));

        _incidentRepositoryMock
            .Setup(repository => repository.TriageAsync(
                1,
                It.IsAny<TriageIncidentDto>(),
                "Asignada",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _service.TriageAsync(
            1,
            new TriageIncidentDto
            {
                Accion = "asignar",
                Prioridad = "Alta"
            },
            ActingUserId);

        var detail = CapturedAudit().Detail;

        Assert.Contains("Estado: Registrada → Asignada", detail);
        Assert.Contains("Prioridad: Media → Alta", detail);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldNotRecordAudit_WhenIncidentIsMissing()
    {
        _incidentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IncidentDto?)null);

        var result = await _service.UpdateStatusAsync(
            1,
            new UpdateIncidentStatusDto { Estado = "EnProceso" },
            ActingUserId);

        Assert.Null(result);

        _auditServiceMock.Verify(
            service => service.CreateAsync(
                It.IsAny<CreateAuditDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupIncident(
        IncidentDto before,
        IncidentDto after)
    {
        _incidentRepositoryMock
            .SetupSequence(repository => repository.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(before)
            .ReturnsAsync(after);
    }

    private CreateAuditDto CapturedAudit()
    {
        CreateAuditDto? captured = null;

        _auditServiceMock.Verify(
            service => service.CreateAsync(
                It.Is<CreateAuditDto>(audit =>
                    Capture(audit, out captured)),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.NotNull(captured);

        return captured;
    }

    private static bool Capture(
        CreateAuditDto audit,
        out CreateAuditDto? captured)
    {
        captured = audit;
        return true;
    }

    private static IncidentDto Incident(
        string estado = "Registrada",
        string prioridad = "Media",
        string? institucion = null)
    {
        return new IncidentDto
        {
            Id = 1,
            CodigoCaso = "INC-20260806-ABCDEF0123",
            Estado = estado,
            Prioridad = prioridad,
            InstitucionAsignada = institucion,
            TipoIncidencia = "Infraestructura Fisica",
            Jurisdiccion = "Distrito Nacional"
        };
    }
}
