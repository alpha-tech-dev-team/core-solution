using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CoreSolution.Shared.TgBot.Internal.Notifications;

internal sealed record TelegramTextChunk(string Text, IReadOnlyList<MessageEntity>? Entities);

internal static class TelegramTextSplitter
{
    public static IReadOnlyList<TelegramTextChunk> Split(
        string text,
        IReadOnlyList<MessageEntity>? entities = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [new TelegramTextChunk(string.Empty, entities)];
        }

        if (text.Length <= TelegramConstants.MaxMessageLength)
        {
            return [new TelegramTextChunk(text, entities)];
        }

        var parts = new List<TelegramTextChunk>();
        var index = 0;

        while (index < text.Length)
        {
            var length = Math.Min(TelegramConstants.MaxMessageLength, text.Length - index);

            if (index + length < text.Length)
            {
                var breakAt = text.LastIndexOf('\n', index + length - 1, length);

                if (breakAt > index)
                {
                    length = breakAt - index + 1;
                }

                length = AvoidEntityCut(entities, index, length);
            }

            parts.Add(new TelegramTextChunk(text.Substring(index, length), SliceEntities(entities, index, length)));
            index += length;
        }

        return parts;
    }

    private static int AvoidEntityCut(IReadOnlyList<MessageEntity>? entities, int start, int length)
    {
        if (entities is null)
        {
            return length;
        }

        var end = start + length;

        foreach (var entity in entities)
        {
            var entityEnd = entity.Offset + entity.Length;

            if (entity.Offset < end && entityEnd > end && entity.Offset > start)
            {
                return entity.Offset - start;
            }
        }

        return length;
    }

    private static IReadOnlyList<MessageEntity>? SliceEntities(
        IReadOnlyList<MessageEntity>? entities,
        int start,
        int length)
    {
        if (entities is null)
        {
            return null;
        }

        var end = start + length;
        var sliced = new List<MessageEntity>();

        foreach (var entity in entities)
        {
            var entityEnd = entity.Offset + entity.Length;
            var overlapStart = Math.Max(entity.Offset, start);
            var overlapEnd = Math.Min(entityEnd, end);

            if (overlapEnd > overlapStart)
            {
                sliced.Add(new MessageEntity
                {
                    Type = entity.Type,
                    Offset = overlapStart - start,
                    Length = overlapEnd - overlapStart
                });
            }
        }

        return sliced;
    }
}
