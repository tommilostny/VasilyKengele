namespace VasilyKengele.Repositories;

/// <summary>
/// Class that is used to store a collection of user entites.
/// </summary>
public class VKBotUsersRepository
{
    private const string _usersFileName = "users.json";
    private const string _compressedFileName = $"{_usersFileName}.gz";
    private readonly IList<VKBotUserEntity> _usersCollection;

    /// <summary>
    /// Tries to load user entities collection serialized in the compressed JSON file.
    /// If it does not exist, the collection is initialized as empty and JSON file with empty list is created.
    /// </summary>
    public VKBotUsersRepository()
    {
        CheckForUncompressedJson();

        if (System.IO.File.Exists(_compressedFileName))
        {
            using var compressedFileStream = System.IO.File.Open(_compressedFileName, FileMode.Open);
            using var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);
            using var memoryStream = new MemoryStream();
            decompressor.CopyTo(memoryStream);

            var jsonStr = Encoding.UTF8.GetString(memoryStream.GetBuffer());
            var stored = JsonConvert.DeserializeObject<List<VKBotUserEntity>>(jsonStr);
            if (stored is not null)
            {
                _usersCollection = stored;
                return;
            }
        }
        _usersCollection = new List<VKBotUserEntity>();
    }

    /// <summary>
    /// Adds given user entity to the repository.
    /// </summary>
    /// <param name="user">User entity to store.</param>
    public async Task AddAsync(VKBotUserEntity user)
    {
        var existingUser = _usersCollection.SingleOrDefault(u => u.ChatId == user.ChatId);
        if (existingUser is not null && existingUser != user)
        {
            user.UtcDifference = existingUser.UtcDifference;
            user.TimeZoneSet = existingUser.TimeZoneSet;
            user.ReceiveWakeUps = existingUser.ReceiveWakeUps;
            user.Email = existingUser.Email;
            _usersCollection.Remove(existingUser);
        }
        _usersCollection.Add(user);
        await SaveJsonAsync();
    }

    /// <summary>
    /// Removes given user entity from the repository.
    /// </summary>
    /// <param name="chatId">Chat ID of the user entity to delete.</param>
    /// <returns>true if user was removed successfully, otherwise false.</returns>
    public async Task<bool> RemoveAsync(long chatId)
    {
        var existingUser = _usersCollection.SingleOrDefault(u => u.ChatId == chatId);
        if (existingUser is null)
        {
            return false;
        }
        var result = _usersCollection.Remove(existingUser);
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
        var existingUser = _usersCollection.SingleOrDefault(u => u.ChatId == user.ChatId);
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
    /// <param name="fullname"><seealso cref="VKBotUserEntity.Name"/></param>
    /// <param name="username"><seealso cref="VKBotUserEntity.Username"/></param>
    /// <returns>
    /// Tuple that represents the user entity and its existence in the repository.
    /// If user entity is not already stored its existence is set to false and set to true otherwise.
    /// If it does not exists, new user entity is created with given chat ID and name.
    /// </returns>
    public async Task<(VKBotUserEntity, bool)> GetAsync(long chatId, string fullname, string? username)
    {
        var existingUser = _usersCollection.SingleOrDefault(u => u.ChatId == chatId);

        if (existingUser is not null) // User exists in the collection.
        {
            // Existing user matches one stored in the repository. Return it.
            if (existingUser!.Name == fullname && existingUser.Username == username)
            {
                return (existingUser, true);
            }
            // Existing user doesn't match. Create a new updated copy, save and return it.
            var user = new VKBotUserEntity(chatId, fullname, username ?? string.Empty)
            {
                UtcDifference = existingUser.UtcDifference,
                TimeZoneSet = existingUser.TimeZoneSet,
                ReceiveWakeUps = existingUser.ReceiveWakeUps,
                Email = existingUser.Email
            };
            _usersCollection.Remove(existingUser);
            _usersCollection.Add(user);
            await SaveJsonAsync();
            return (user, true);
        }
        // User doesn't yet exist in the repository. Handle by outside command.
        return (new(chatId, fullname, username ?? string.Empty), false);
    }

    /// <summary>
    /// Serializes user entities collection and writes it to the compressed JSON file.
    /// </summary>
    private async Task SaveJsonAsync()
    {
        var jsonStr = JsonConvert.SerializeObject(_usersCollection);
        if (jsonStr is not null)
        {
            using var compressedFileStream = System.IO.File.Create(_compressedFileName);
            using var compressor = new GZipStream(compressedFileStream, CompressionMode.Compress);
            
            await compressor.WriteAsync(Encoding.UTF8.GetBytes(jsonStr));
        }
    }

    /// <summary>
    /// Compress JSON file used in previous versions of Vasily Kengele and delete it.
    /// </summary>
    private static void CheckForUncompressedJson()
    {
        if (!System.IO.File.Exists(_usersFileName))
        {
            return;
        }
        var jsonBytes = System.IO.File.ReadAllBytes(_usersFileName);
        using var compressedFileStream = System.IO.File.Create(_compressedFileName);
        using var compressor = new GZipStream(compressedFileStream, CompressionMode.Compress);

        compressor.Write(jsonBytes);

        System.IO.File.Delete(_usersFileName);
    }
}
