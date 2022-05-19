namespace VasilyKengele.Repositories;

public class VKBotUsersRepository
{
    private const string _chatIdsFile = "users.json";

    private readonly ICollection<VKBotUserEntity> _usersCollection;

    public VKBotUsersRepository()
    {
        if (System.IO.File.Exists(_chatIdsFile))
        {
            var jsonStr = System.IO.File.ReadAllText(_chatIdsFile);
            var stored = JsonConvert.DeserializeObject<List<VKBotUserEntity>>(jsonStr);
            if (stored is not null)
            {
                _usersCollection = stored;
                return;
            }
        }
        _usersCollection = new List<VKBotUserEntity>();
        System.IO.File.WriteAllText(_chatIdsFile, "[]");
    }

    public async Task AddAsync(VKBotUserEntity user)
    {
        if (_usersCollection.Contains(user))
        {
            return;
        }
        _usersCollection.Add(user);
        await SaveJsonAsync();
    }

    public async Task<bool> RemoveAsync(VKBotUserEntity user)
    {
        var result = _usersCollection.Remove(user);
        await SaveJsonAsync();
        return result;
    }

    public async Task UpdateAsync(VKBotUserEntity user)
    {
        var existingUser = _usersCollection.SingleOrDefault(existing => existing.ChatId == user.ChatId);
        if (existingUser is null)
        {
            return;
        }
        _usersCollection.Remove(existingUser);
        _usersCollection.Add(user);
        await SaveJsonAsync();
    }

    public IEnumerable<VKBotUserEntity> GetAll()
    {
        foreach (var user in _usersCollection)
        {
            yield return user;
        }
    }

    public (VKBotUserEntity, bool) Get(long chatId, string username)
    {
        var stored = _usersCollection.SingleOrDefault(u => u.ChatId == chatId);
        if (stored is not null)
        {
            return (stored, true);
        }
        return (new(chatId, username), false);
    }

    private async Task SaveJsonAsync()
    {
        var jsonStr = JsonConvert.SerializeObject(_usersCollection);
        if (jsonStr is not null)
        {
            await System.IO.File.WriteAllTextAsync(_chatIdsFile, jsonStr);
        }
    }
}
