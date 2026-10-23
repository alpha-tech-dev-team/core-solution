using Atlas.Core.Interfaces;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.App.Features.Start;

public sealed class StartRequestHandler : IRequestHandler<StartRequest, Result<TelegramMessageContent>>
{
    private readonly IAccessService _access;

    public StartRequestHandler(IAccessService access) => _access = access;
    public async Task<Result<TelegramMessageContent>> Handle(StartRequest request, CancellationToken ctn)
    {
if (!await _access.IsAllowedAsync(request.ChatId, ctn))
        {
            return Result.Failure<TelegramMessageContent>(
                "AccessDenied", ["У вас нет доступа. Обратитесь к администратору."]);
        }

        return Result<TelegramMessageContent>.Success(new TelegramMessageContent
        {
            Text = "Главное меню:",
            ButtonRows = [new TelegramButtonRow(
                new TelegramButton("Задачи", "menu:tasks"),
                new TelegramButton("Профиль", "menu:profile"))]
        });
            }
}
