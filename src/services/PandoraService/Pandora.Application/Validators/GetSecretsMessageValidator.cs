using CoreSolution.Shared.Application.Validation;
using Pandora.Application.Features.Secrets.GetSecrets;

namespace Pandora.Application.Validators;

public sealed class GetSecretsMessageValidator : PagedQueryValidator<GetSecretsMessage>
{
}
