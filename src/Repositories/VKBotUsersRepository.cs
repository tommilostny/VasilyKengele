namespace VasilyKengele.Repositories;

/// <summary>
/// Class that is used to store a collection of user entites.
/// </summary>
public class VKBotUsersRepository
{
    private const string _usersJsonFile = "users.json";

    private readonly IList<VKBotUserEntity> _usersCollection;

    /// <summary>
    /// Tries to load user entities collection serialized in the JSON file.
    /// If it does not exist, the collection is initialized as empty and JSON file with empty list is created.
    /// </summary>
    public VKBotUsersRepository()
    {
        if (System.IO.File.Exists(_usersJsonFile))
        {
            var jsonStr = System.IO.File.ReadAllText(_usersJsonFile);
            var stored = JsonConvert.DeserializeObject<List<VKBotUserEntity>>(jsonStr);
            if (stored is not null)
            {
                _usersCollection = stored;
                return;
            }
        }
        _usersCollection = new List<VKBotUserEntity>();
        System.IO.File.WriteAllText(_usersJsonFile, "[]");
    }

    /// <summary>
    /// Adds given user entity to the repository.
    /// </summary>
    /// <param name="user">User entity to store.</param>
    public async Task AddAsync(VKBotUserEntity user)
    {
        if (_usersCollection.Contains(user))
        {
            return;
        }
        _usersCollection.Add(user);
        await SaveJsonAsync();
    }

    /// <summary>
    /// Removes given user entity from the repository.
    /// </summary>
    /// <param name="user">User entity to delete.</param>
    /// <returns>true if user was removed successfully, otherwise false.</returns>
    public async Task<bool> RemoveAsync(VKBotUserEntity user)
    {
        var result = _usersCollection.Remove(user);
        await SaveJsonAsync();
        return result;
    }

    /// <summary>
    /// Updates user entity stored in the repository with provided data.
    /// This happens only if the user entity already exists (it's searched for by <seealso cref="VKBotUserEntity.ChatId"/>).
    /// </summary>
    /// <param name="user">User entity to update.</param>
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

    /// <summary>
    /// Iterates over the user entities stored in the repository.
    /// </summary>
    public IReadOnlyCollection<VKBotUserEntity> GetAll()
    {
        return new ReadOnlyCollection<VKBotUserEntity>(_usersCollection);
    }

    /// <summary>
    /// Gets user entity stored in the repository.
    /// </summary>
    /// <param name="chatId"><seealso cref="VKBotUserEntity.ChatId"/></param>
    /// <param name="username"><seealso cref="VKBotUserEntity.Name"/></param>
    /// <returns>
    /// Tuple that represents the user entity and its existence in the repository.
    /// If user entity is not already stored its existence is set to false and set to true otherwise.
    /// If it does not exists, new user entity is created with given chat ID and name.
    /// </returns>
    public (VKBotUserEntity, bool) Get(long chatId, string username)
    {
        var stored = _usersCollection.SingleOrDefault(u => u.ChatId == chatId);
        if (stored is not null)
        {
            return (stored, true);
        }
        return (new(chatId, username), false);
    }

    /// <summary>
    /// Serializes user entities collection and writes it to the JSON file.
    /// </summary>
    private async Task SaveJsonAsync()
    {
        var jsonStr = JsonConvert.SerializeObject(_usersCollection);
        if (jsonStr is not null)
        {
            await System.IO.File.WriteAllTextAsync(_usersJsonFile, jsonStr);
        }
    }
}
