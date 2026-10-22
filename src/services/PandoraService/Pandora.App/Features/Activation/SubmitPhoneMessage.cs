using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Activation;

public record SubmitPhoneMessage(long TelegramUserId, int MessageId, string Phone) : IRequest<Result<TelegramMessageContent>>, IActivatingBotRequest;
