using Telegram.Bot.Types;

namespace CoreSolution.Shared.TgBot.Abstractions;

public sealed record TelegramSendOptions(bool ProtectContent = false, IReadOnlyList<MessageEntity>? Entities = null);
