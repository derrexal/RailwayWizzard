using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace RzdHack.Robot.Core;

[Table("AppUsers")]
public class User: Entity
{
    [Required]
    public long IdTg { get; set; }

    [MaxLength(32)]
    public string? Username { get; set; } // тг может прятать ники...

}