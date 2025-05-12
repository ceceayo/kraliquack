using Microsoft.EntityFrameworkCore;

namespace UGS.Shared.DbModels;

[Index(nameof(Key), IsUnique = true)]
public class ConfigKey
{
    public int Id { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
}