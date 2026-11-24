using CoreSolution.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Pandora.Api.Hubs;

[Authorize(Policy = nameof(UserRole.PandoraAdmin))]
public sealed class AuditHub : Hub
{
    public const string Path = "/hubs/audit";
    public const string ReceiveAuditEntryMethod = "ReceiveAuditEntry";
}
