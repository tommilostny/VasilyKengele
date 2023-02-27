namespace VasilyKengele.Repositories;

/// <summary>
/// Class that is used to store a collection of user entites.
/// </summary>
public class VKBotUsersRepository
{
#if DEBUG
    private const string _usersFileName = "users.Development.json";
#else
    private const string _usersFileName = "users.json";
#endif
    private const string _compressedFileName = $"{_usersFileName}.gz";
    private readonly IDictionary<long, VKBotUserEntity> _users;

    /// <summary>
    /// Tries to load user entities collection serialized in the compressed JSON file.
    /// If it does not exist, the collection is initialized as empty and JSON file with empty list is created.
    /// </summary>
    public VKBotUsersRepository()
    {
        if (System.IO.File.Exists(_compressedFileName))
        {
            using var compressedFileStream = System.IO.File.Open(_compressedFileName, FileMode.Open);
            using var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);
            using var memoryStream = new MemoryStream();
            decompressor.CopyTo(memoryStream);

            var jsonStr = Encoding.UTF8.GetString(memoryStream.GetBuffer());
            var stored = JsonConvert.DeserializeObject<Dictionary<long, VKBotUserEntity>>(jsonStr);
            if (stored is not null)
            {
                _users = stored;
                return;
            }
        }
        _users = new Dictionary<long, VKBotUserEntity>();
    }

    /// <summary>
    /// Adds given user entity to the repository.
    /// </summary>
    /// <param name="user">User entity to store.</param>
    public async Task AddAsync(VKBotUserEntity user)
    {
        var exists = _users.TryGetValue(user.ChatId, out var existingUser);
        if (exists && existingUser is not null && existingUser != user)
        {
            user.UtcDifference = existingUser.UtcDifference;
            user.TimeZoneSet = existingUser.TimeZoneSet;
            user.ReceiveWakeUps = existingUser.ReceiveWakeUps;
            user.Email = existingUser.Email;
        }
        _users[user.ChatId] = user;
        await SaveJsonAsync();
    }

    /// <summary>
    /// Removes given user entity from the repository.
    /// </summary>
    /// <param name="chatId">Chat ID of the user entity to delete.</param>
    /// <returns>true if user was removed successfully, otherwise false.</returns>
    public async Task<bool> RemoveAsync(long chatId)
    {
        if (_users.ContainsKey(chatId))
        {
            var result = _users.Remove(chatId);
            await SaveJsonAsync();
            return result;
        }
        return false;
    }

    /// <summary>
    /// Updates user entity stored in the repository with provided data.
    /// This happens only if the user entity already exists (it's searched for by <seealso cref="VKBotUserEntity.ChatId"/>).
    /// </summary>
    /// <param name="user">User entity to update.</param>
    public async Task UpdateAsync(VKBotUserEntity user)
    {
        _users[user.ChatId] = user;
        await SaveJsonAsync();
    }

    /// <summary>
    /// Iterates over the user entities stored in the repository.
    /// </summary>
    public IReadOnlyCollection<VKBotUserEntity> GetAll()
    {
        return new ReadOnlyCollection<VKBotUserEntity>(_users.Values.ToList());
    }

    /// <summary>
    /// Gets user entity stored in the repository.
    /// </summary>
    /// <param name="chatId"><seealso cref="VKBotUserEntity.ChatId"/></param>
    /// <param name="fullname"><seealso cref="VKBotUserEntity.Name"/></param>
    /// <param name="username"><seealso cref="VKBotUserEntity.Username"/></param>
    /// <returns>
    /// Tuple that represents the user entity and its existence in the repository.
    /// If user entity is not already stored its existence is set to false and set to true otherwise.
    /// If it does not exists, new user entity is created with given chat ID and name.
    /// </returns>
    public async Task<(VKBotUserEntity, bool)> GetAsync(long chatId, string fullname, string? username)
    {
        var exists = _users.TryGetValue(chatId, out var existingUser);

        if (exists && existingUser is not null) // User exists in the repo.
        {
            // Existing user matches one stored in the repository. Return it.
            if (existingUser.Name == fullname && existingUser.Username == username)
            {
                return (existingUser, true);
            }
            // Existing user doesn't match. Create a new updated copy, save and return it.
            var updatedUser = new VKBotUserEntity(chatId, fullname, username ?? string.Empty)
            {
                UtcDifference = existingUser.UtcDifference,
                TimeZoneSet = existingUser.TimeZoneSet,
                ReceiveWakeUps = existingUser.ReceiveWakeUps,
                Email = existingUser.Email
            };
            _users[chatId] = updatedUser;
            await SaveJsonAsync();
            return (updatedUser, true);
        }
        // User doesn't yet exist in the repository. Handle by outside command.
        return (new(chatId, fullname, username ?? string.Empty), false);
    }

    public VKBotUserEntity? GetById(long chatId)
    {
        _users.TryGetValue(chatId, out var existingUser);
        return existingUser;
    }

    /// <summary>
    /// Serializes user entities collection and writes it to the compressed JSON file.
    /// </summary>
    private async Task SaveJsonAsync()
    {
        var jsonStr = JsonConvert.SerializeObject(_users);
        if (jsonStr is not null)
        {
            using var compressedFileStream = System.IO.File.Create(_compressedFileName);
            using var compressor = new GZipStream(compressedFileStream, CompressionMode.Compress);
            
            await compressor.WriteAsync(Encoding.UTF8.GetBytes(jsonStr));
        }
    }
}
