using System.ComponentModel.DataAnnotations;

namespace RailwayWizzard.Core;

/// <summary>
/// Базовый класс сущности.
/// </summary>
public class Entity
{
    [Required]
    public int Id { get; set; }
}