using System.ComponentModel.DataAnnotations;
using Cpb.Domain;

namespace Cpb.Database;

public class DbUser
{
    [Key]
    public Guid Id { get; init; }
    
    [MaxLength(64)]
    public required string FirstName { get; init; }
    
    [MaxLength(64)]
    public required string LastName { get; init; }
    
    [MaxLength(320)]
    public required string Email { get; init; }
    
    public required string HashedPassword { get; init; }
    
    [MaxLength(32)]
    public Roles Role { get; init; }
}