namespace VasilyKengele.Repositories;

public class VKTelegramChatIdsRepository
{
    private const string _chatIdsFile = "chatIds.json";

    private readonly ICollection<long> _chatIdsCollection;

    public VKTelegramChatIdsRepository()
    {
        if (System.IO.File.Exists(_chatIdsFile))
        {
            var jsonStr = System.IO.File.ReadAllText(_chatIdsFile);
            var stored = JsonConvert.DeserializeObject<List<long>>(jsonStr);
            if (stored is not null)
            {
                _chatIdsCollection = stored;
                return;
            }
        }
        _chatIdsCollection = new List<long>();
        System.IO.File.WriteAllText(_chatIdsFile, "[]");
    }

    public async Task<bool> AddAsync(long chatId)
    {
        if (_chatIdsCollection.Contains(chatId))
        {
            return false;
        }
        _chatIdsCollection.Add(chatId);
        await SaveJsonAsync();
        return true;
    }

    public IEnumerable<long> GetAll()
    {
        foreach (var chatId in _chatIdsCollection)
        {
            yield return chatId;
        }
    }

    public async Task<bool> RemoveAsync(long chatId)
    {
        var result = _chatIdsCollection.Remove(chatId);
        await SaveJsonAsync();
        return result;
    }

    private async Task SaveJsonAsync()
    {
        var jsonStr = JsonConvert.SerializeObject(_chatIdsCollection);
        if (jsonStr is not null)
        {
            await System.IO.File.WriteAllTextAsync(_chatIdsFile, jsonStr);
        }
    }
}
