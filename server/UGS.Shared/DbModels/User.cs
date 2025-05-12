using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace UGS.Shared.DbModels;

[Index(nameof(UserName), IsUnique = true)]
public class User
{
    public int Id { get; set; }
    [Required]
    public required string UserName { get; set; }
    [Required]
    public required string HashedPassword { get; set; }
    [Required]
    public required bool IsAdmin { get; set; }
}