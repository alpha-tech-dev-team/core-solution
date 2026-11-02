using Atlas.App.Common;
using Atlas.Core.Constants.Localization;
using Atlas.Core.Enums;
using Atlas.Core.Interfaces;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.App.Features.Menu;

public sealed class ShowMenuHandler : IRequestHandler<ShowMenuMessage, Result<TelegramMessageContent>>
{
    private readonly ILocalizer _localizer;

    public ShowMenuHandler(ILocalizer localizer)
    {
        _localizer = localizer;
    }

    public Task<Result<TelegramMessageContent>> Handle(ShowMenuMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<TelegramMessageContent>.Success(CreateMenu()));
    }

    private TelegramMessageContent CreateMenu()
    {
        return new TelegramMessageContent
        {
            Text = _localizer.GetPhrase(
                WorkflowStep.ProblemCategorySelection,
                LocalizationKeysConstants.ProblemCategorySelection.Text),
            ButtonRows =
            [
                new TelegramButtonRow(
                    new TelegramButton(
                        _localizer.GetPhrase(
                            WorkflowStep.ProblemCategorySelection,
                            LocalizationKeysConstants.ProblemCategorySelection.Problems),
                        CallbackConstants.MenuProblems),
                    new TelegramButton(
                        _localizer.GetPhrase(
                            WorkflowStep.ProblemCategorySelection,
                            LocalizationKeysConstants.ProblemCategorySelection.Profile),
                        CallbackConstants.MenuProfile))
            ]
        };
    }
}
