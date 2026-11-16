using CoreSolution.Shared.Application.Validation;
using Pandora.Application.Features.Audit;

namespace Pandora.Application.Validators;

public sealed class GetAuditEntriesMessageValidator : PagedQueryValidator<GetAuditEntriesMessage>
{
}
