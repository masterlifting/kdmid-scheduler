using System.Collections.Concurrent;

namespace Telegram.ApAzureBot.Core.Services.Telegram;

public sealed class TelegramMemoryCache
{
    private readonly ConcurrentDictionary<long, Dictionary<string, string>> _storage = new();

    public void AddOrUpdate(long chatId, string key, string value)
    {
        if (_storage.ContainsKey(chatId))
        {
            _storage[chatId][key] = value;
        }
        else
        {
            _storage.TryAdd(chatId, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { key, value } });
        }
    }
    public bool TryGetValue(long chatId, string key, out string? value)
    {
        if (_storage.TryGetValue(chatId, out var chatStorage))
        {
            return chatStorage.TryGetValue(key, out value);
        }

        value = null;
        return false;
    }
    public void Clear(long chatId)
    {
        _storage.TryRemove(chatId, out _);
    }
}
