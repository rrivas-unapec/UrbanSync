using System.Net;
using UrbanSync.Application.Common.Interfaces.Notifications;
using UrbanSync.Application.Common.Interfaces.Persistence;

namespace UrbanSync.Application.Features.Incidents;

public sealed class IncidentNotificationService
    : IIncidentNotificationService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailSender _emailSender;

    public IncidentNotificationService(
        IUsuarioRepository usuarioRepository,
        IEmailSender emailSender)
    {
        _usuarioRepository = usuarioRepository;
        _emailSender = emailSender;
    }

    public async Task NotifyStatusChangedAsync(
        IncidentDto previousIncident,
        IncidentDto currentIncident,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(previousIncident);
        ArgumentNullException.ThrowIfNull(currentIncident);

        if (string.Equals(
            previousIncident.Estado,
            currentIncident.Estado,
            StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (currentIncident.UsuarioReportaId <= 0)
        {
            return;
        }

        var user =
            await _usuarioRepository.GetByIdAsync(
                currentIncident.UsuarioReportaId);

        if (user is null ||
            !user.Activo ||
            string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        var recipientName =
            string.IsNullOrWhiteSpace(user.NombreCompleto)
                ? user.NombreUsuario
                : user.NombreCompleto;

        var subject =
            $"UrbanSync | Actualización de la incidencia {currentIncident.CodigoCaso}";

        var htmlBody = BuildStatusChangedEmail(
            recipientName,
            previousIncident,
            currentIncident);

        await _emailSender.SendAsync(
            user.Email,
            recipientName,
            subject,
            htmlBody,
            cancellationToken);
    }

    private static string BuildStatusChangedEmail(
        string recipientName,
        IncidentDto previousIncident,
        IncidentDto currentIncident)
    {
        var safeName =
            WebUtility.HtmlEncode(recipientName);

        var safeCaseCode =
            WebUtility.HtmlEncode(
                currentIncident.CodigoCaso);

        var safePreviousStatus =
            WebUtility.HtmlEncode(
                FormatStatus(previousIncident.Estado));

        var safeCurrentStatus =
            WebUtility.HtmlEncode(
                FormatStatus(currentIncident.Estado));

        var safeDescription =
            WebUtility.HtmlEncode(
                currentIncident.Descripcion);

        var safeIncidentType =
            WebUtility.HtmlEncode(
                currentIncident.TipoIncidencia);

        var safeAddress =
            WebUtility.HtmlEncode(
                currentIncident.Direccion);

        var safeInstitution =
            WebUtility.HtmlEncode(
                string.IsNullOrWhiteSpace(
                    currentIncident.InstitucionAsignada)
                    ? "Aún no asignada"
                    : currentIncident.InstitucionAsignada);

        return $$"""
        <!DOCTYPE html>
        <html lang="es">
        <head>
            <meta charset="utf-8">
            <meta name="viewport"
                  content="width=device-width, initial-scale=1">
        </head>

        <body style="
            margin:0;
            padding:0;
            background:#f4f6f8;
            font-family:Arial,Helvetica,sans-serif;
            color:#172033;">

            <table
                role="presentation"
                width="100%"
                cellspacing="0"
                cellpadding="0"
                style="
                    width:100%;
                    background:#f4f6f8;
                    padding:32px 16px;">

                <tr>
                    <td align="center">

                        <table
                            role="presentation"
                            width="600"
                            cellspacing="0"
                            cellpadding="0"
                            style="
                                width:100%;
                                max-width:600px;
                                background:#ffffff;
                                border-radius:14px;
                                overflow:hidden;
                                border:1px solid #e3e8ef;">

                            <tr>
                                <td style="
                                    padding:24px 28px;
                                    background:#0d2340;
                                    color:#ffffff;">

                                    <div style="
                                        font-size:21px;
                                        font-weight:700;">
                                        UrbanSync
                                    </div>

                                    <div style="
                                        margin-top:4px;
                                        font-size:13px;
                                        color:#b9c9dd;">
                                        Plataforma de Gestión Urbana
                                    </div>

                                </td>
                            </tr>

                            <tr>
                                <td style="padding:30px 28px;">

                                    <p style="
                                        margin:0 0 18px;
                                        font-size:16px;">
                                        Hola <strong>{{safeName}}</strong>,
                                    </p>

                                    <p style="
                                        margin:0 0 22px;
                                        line-height:1.6;
                                        color:#4c596b;">
                                        Te informamos que una incidencia
                                        relacionada con tu cuenta ha cambiado
                                        de estado.
                                    </p>

                                    <div style="
                                        margin-bottom:22px;
                                        padding:18px;
                                        background:#f7f9fc;
                                        border:1px solid #e3e8ef;
                                        border-radius:10px;">

                                        <div style="
                                            margin-bottom:6px;
                                            font-size:12px;
                                            font-weight:700;
                                            text-transform:uppercase;
                                            color:#718096;">
                                            Código de incidencia
                                        </div>

                                        <div style="
                                            font-size:18px;
                                            font-weight:700;
                                            color:#0b5cab;">
                                            {{safeCaseCode}}
                                        </div>

                                    </div>

                                    <table
                                        role="presentation"
                                        width="100%"
                                        cellspacing="0"
                                        cellpadding="0"
                                        style="margin-bottom:24px;">

                                        <tr>
                                            <td
                                                width="48%"
                                                style="
                                                    padding:14px;
                                                    background:#f7f9fc;
                                                    border-radius:8px;">

                                                <div style="
                                                    font-size:11px;
                                                    font-weight:700;
                                                    color:#718096;">
                                                    ESTADO ANTERIOR
                                                </div>

                                                <div style="
                                                    margin-top:6px;
                                                    font-size:15px;">
                                                    {{safePreviousStatus}}
                                                </div>

                                            </td>

                                            <td width="4%"></td>

                                            <td
                                                width="48%"
                                                style="
                                                    padding:14px;
                                                    background:#eaf4ff;
                                                    border-radius:8px;">

                                                <div style="
                                                    font-size:11px;
                                                    font-weight:700;
                                                    color:#0b5cab;">
                                                    NUEVO ESTADO
                                                </div>

                                                <div style="
                                                    margin-top:6px;
                                                    font-size:15px;
                                                    font-weight:700;
                                                    color:#0b5cab;">
                                                    {{safeCurrentStatus}}
                                                </div>

                                            </td>
                                        </tr>

                                    </table>

                                    <table
                                        role="presentation"
                                        width="100%"
                                        cellspacing="0"
                                        cellpadding="0"
                                        style="
                                            margin-bottom:24px;
                                            font-size:14px;">

                                        <tr>
                                            <td style="
                                                padding:8px 0;
                                                color:#718096;">
                                                Tipo
                                            </td>

                                            <td
                                                align="right"
                                                style="
                                                    padding:8px 0;
                                                    font-weight:600;">
                                                {{safeIncidentType}}
                                            </td>
                                        </tr>

                                        <tr>
                                            <td style="
                                                padding:8px 0;
                                                color:#718096;">
                                                Dirección
                                            </td>

                                            <td
                                                align="right"
                                                style="
                                                    padding:8px 0;
                                                    font-weight:600;">
                                                {{safeAddress}}
                                            </td>
                                        </tr>

                                        <tr>
                                            <td style="
                                                padding:8px 0;
                                                color:#718096;">
                                                Institución asignada
                                            </td>

                                            <td
                                                align="right"
                                                style="
                                                    padding:8px 0;
                                                    font-weight:600;">
                                                {{safeInstitution}}
                                            </td>
                                        </tr>

                                    </table>

                                    <div style="
                                        margin-bottom:22px;
                                        padding:16px;
                                        background:#f7f9fc;
                                        border-left:4px solid #0b5cab;
                                        border-radius:6px;">

                                        <div style="
                                            margin-bottom:5px;
                                            font-size:12px;
                                            font-weight:700;
                                            color:#718096;">
                                            DESCRIPCIÓN
                                        </div>

                                        <div style="
                                            font-size:14px;
                                            line-height:1.6;">
                                            {{safeDescription}}
                                        </div>

                                    </div>

                                    <p style="
                                        margin:0;
                                        font-size:13px;
                                        line-height:1.6;
                                        color:#718096;">
                                        Este mensaje fue generado
                                        automáticamente por UrbanSync.
                                        No es necesario responder a este correo.
                                    </p>

                                </td>
                            </tr>

                            <tr>
                                <td style="
                                    padding:18px 28px;
                                    background:#f7f9fc;
                                    border-top:1px solid #e3e8ef;
                                    text-align:center;
                                    font-size:12px;
                                    color:#718096;">

                                    © 2026 UrbanSync ·
                                    Ayuntamiento del Distrito Nacional

                                </td>
                            </tr>

                        </table>

                    </td>
                </tr>

            </table>

        </body>
        </html>
        """;
    }

    private static string FormatStatus(
        string status)
    {
        return status switch
        {
            "EnAnalisis" => "En análisis",
            "EnProceso" => "En proceso",
            _ => status
        };
    }
}