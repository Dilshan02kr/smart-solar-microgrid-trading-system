/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: UserDetailsRepository.cs
 * Component: Web User and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Provides normalized, uniqueness-aware MongoDB persistence for user accounts.
 */
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
        // Resolves the established user collection from the shared database.
        _users = database.GetCollection<UserDetails>(CollectionName);
    }

    public async Task<UserDetails?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        // Validates an ObjectId string and retrieves the matching user account.
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
        // Normalizes an email address and retrieves its system-wide account owner.
        var normalizedEmail = NormalizeEmail(email);

        return await _users
            .Find(user => user.Email == normalizedEmail)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserDetails?> GetByNicAsync(
        string nic,
        CancellationToken cancellationToken = default)
    {
        // Trims a NIC and retrieves the matching Prosumer account when present.
        return await _users
            .Find(user => user.Nic == nic.Trim())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        // Determines whether at least one account exists for the supplied role.
        return await _users
            .Find(user => user.Role == role)
            .Limit(1)
            .AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserDetails>> GetByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        // Returns accounts for one role ordered by newest creation time.
        return await _users
            .Find(user => user.Role == role)
            .SortByDescending(user => user.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserDetails>> GetByRoleAndStatusAsync(
        UserRole role,
        AccountStatus accountStatus,
        CancellationToken cancellationToken = default)
    {
        // Returns accounts matching one role and lifecycle status.
        return await _users
            .Find(user => user.Role == role && user.AccountStatus == accountStatus)
            .SortByDescending(user => user.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserDetails>> GetWebUsersAsync(
        CancellationToken cancellationToken = default)
    {
        // Return only Backoffice and Grid Operator accounts for Web administration.
        var filter = Builders<UserDetails>.Filter.In(
            user => user.Role,
            new[] { UserRole.BACKOFFICE, UserRole.GRID_OPERATOR });
        return await _users
            .Find(filter)
            .SortByDescending(user => user.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDetails?> UpdateWebUserAsync(
        string id,
        UserRole expectedRole,
        string firstName,
        string lastName,
        string email,
        string phone,
        ObjectId? assignedMicrogridNodeId,
        CancellationToken cancellationToken = default)
    {
        // Atomically update editable Web-user fields while preserving role and account state.
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        var filter = Builders<UserDetails>.Filter.Eq(user => user.Id, objectId) &
                     Builders<UserDetails>.Filter.Eq(user => user.Role, expectedRole);
        var update = Builders<UserDetails>.Update
            .Set(user => user.FirstName, firstName)
            .Set(user => user.LastName, lastName)
            .Set(user => user.Email, NormalizeEmail(email))
            .Set(user => user.Phone, phone)
            .Set(user => user.AssignedMicrogridNodeId, assignedMicrogridNodeId)
            .Set(user => user.UpdatedAt, DateTime.UtcNow);

        return await UpdateAndMapDuplicateAsync(filter, update, cancellationToken);
    }

    public async Task<UserDetails?> UpdateProsumerProfileAsync(
        string id,
        string firstName,
        string lastName,
        string email,
        string phone,
        CancellationToken cancellationToken = default)
    {
        // Atomically update only self-service profile fields on an active Prosumer account.
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        var filter = Builders<UserDetails>.Filter.Eq(user => user.Id, objectId) &
                     Builders<UserDetails>.Filter.Eq(user => user.Role, UserRole.PROSUMER) &
                     Builders<UserDetails>.Filter.Eq(user => user.AccountStatus, AccountStatus.ACTIVE);
        var update = Builders<UserDetails>.Update
            .Set(user => user.FirstName, firstName)
            .Set(user => user.LastName, lastName)
            .Set(user => user.Email, NormalizeEmail(email))
            .Set(user => user.Phone, phone)
            .Set(user => user.UpdatedAt, DateTime.UtcNow);

        return await UpdateAndMapDuplicateAsync(filter, update, cancellationToken);
    }

    public async Task<UserDetails?> TryTransitionAccountStatusAsync(
        string id,
        UserRole role,
        AccountStatus expectedStatus,
        AccountStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        // Atomically changes account status only when role and expected state still match.
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        var filter = Builders<UserDetails>.Filter.And(
            Builders<UserDetails>.Filter.Eq(user => user.Id, objectId),
            Builders<UserDetails>.Filter.Eq(user => user.Role, role),
            Builders<UserDetails>.Filter.Eq(user => user.AccountStatus, expectedStatus));
        var update = Builders<UserDetails>.Update
            .Set(user => user.AccountStatus, newStatus)
            .Set(user => user.UpdatedAt, DateTime.UtcNow);

        return await _users.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<UserDetails>
            {
                ReturnDocument = ReturnDocument.After
            },
            cancellationToken);
    }

    public async Task CreateAsync(
        UserDetails user,
        CancellationToken cancellationToken = default)
    {
        // Normalizes unique fields, applies server timestamps, and inserts an account.
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
        // Creates unique email and partial unique NIC indexes required by account rules.
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

    private async Task<UserDetails?> UpdateAndMapDuplicateAsync(
        FilterDefinition<UserDetails> filter,
        UpdateDefinition<UserDetails> update,
        CancellationToken cancellationToken)
    {
        // Return the updated document and translate MongoDB unique-index failures consistently.
        try
        {
            return await _users.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<UserDetails>
                {
                    ReturnDocument = ReturnDocument.After
                },
                cancellationToken);
        }
        catch (MongoCommandException exception)
            when (exception.Code == 11000)
        {
            throw new DuplicateUserDetailsException(DuplicateUserField.Email, exception);
        }
        catch (MongoWriteException exception)
            when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new DuplicateUserDetailsException(DuplicateUserField.Email, exception);
        }
    }

    // Produces the canonical trimmed lowercase email representation.
    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();
}
