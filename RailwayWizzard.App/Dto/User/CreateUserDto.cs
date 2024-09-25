using System.ComponentModel.DataAnnotations;

namespace RailwayWizzard.App.Dto.User
{
    public class CreateUserDto
    {
        public int Id { get; set; }

        [Required]
        public long IdTg { get; set; }

        [MaxLength(32)]
        public string? Username { get; set; } // тг может прятать ники...
    }
}
