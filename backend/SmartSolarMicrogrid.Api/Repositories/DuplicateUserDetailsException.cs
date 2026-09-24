// Identifies MongoDB unique-index conflicts encountered while persisting user accounts.
namespace SmartSolarMicrogrid.Api.Repositories;

public enum DuplicateUserField
{
    Email,
    Nic,
    Unknown
}

public sealed class DuplicateUserDetailsException(
    DuplicateUserField field,
    Exception innerException) : Exception("A unique UserDetails field already exists.", innerException)
{
    public DuplicateUserField Field { get; } = field;
}
