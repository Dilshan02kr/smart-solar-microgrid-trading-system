using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Repositories;

public sealed class UserDetailsRepository : IUserDetailsRepository
{
    public const string CollectionName = "UserDetails";

    private readonly IMongoCollection<UserDetails> _users;

    public UserDetailsRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<UserDetails>(CollectionName);
    }

    public async Task<UserDetails?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        return await _users
            .Find(user => user.Id == objectId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserDetails?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);

        return await _users
            .Find(user => user.Email == normalizedEmail)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserDetails?> GetByNicAsync(
        string nic,
        CancellationToken cancellationToken = default)
    {
        return await _users
            .Find(user => user.Nic == nic.Trim())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        return await _users
            .Find(user => user.Role == role)
            .Limit(1)
            .AnyAsync(cancellationToken);
    }

    public async Task CreateAsync(
        UserDetails user,
        CancellationToken cancellationToken = default)
    {
        user.Email = NormalizeEmail(user.Email);
        user.Nic = string.IsNullOrWhiteSpace(user.Nic) ? null : user.Nic.Trim();

        var now = DateTime.UtcNow;
        user.CreatedAt = now;
        user.UpdatedAt = now;

        try
        {
            await _users.InsertOneAsync(user, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException exception)
            when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            var field = exception.Message.Contains(
                "ux_userdetails_nic_when_present",
                StringComparison.Ordinal)
                ? DuplicateUserField.Nic
                : exception.Message.Contains("ux_userdetails_email", StringComparison.Ordinal)
                    ? DuplicateUserField.Email
                    : DuplicateUserField.Unknown;

            throw new DuplicateUserDetailsException(field, exception);
        }
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        var emailIndex = new CreateIndexModel<UserDetails>(
            Builders<UserDetails>.IndexKeys.Ascending(user => user.Email),
            new CreateIndexOptions
            {
                Name = "ux_userdetails_email",
                Unique = true
            });

        // A partial index excludes users whose role does not require a NIC.
        var nicIndex = new CreateIndexModel<UserDetails>(
            Builders<UserDetails>.IndexKeys.Ascending(user => user.Nic),
            new CreateIndexOptions<UserDetails>
            {
                Name = "ux_userdetails_nic_when_present",
                Unique = true,
                PartialFilterExpression = new BsonDocument("nic", new BsonDocument("$type", "string"))
            });

        await _users.Indexes.CreateManyAsync(
            [emailIndex, nicIndex],
            cancellationToken: cancellationToken);
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();
}
