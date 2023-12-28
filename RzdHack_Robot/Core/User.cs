using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace RzdHack_Robot.Core;

[Table("Users")]
public class User: Entity
{
    public long IdTg { get; set; }

    [MaxLength(32)]
    public string? Username { get; set; } // тг может прятать ники...

    public User(long idTg, string? username)
    {
        IdTg = idTg;
        Username = username;
    }
}