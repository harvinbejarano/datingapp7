using DatingApp.Extensions;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Entities
{
    public class AppUser
    {
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public DateOnly DateOfBirth  { get; set; }
        public string KnownAs { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime lastActive { get; set; } = DateTime.UtcNow;
        public string Gender { get; set; } = string.Empty;
        public string Introduction { get; set; } = string.Empty;
        public string LookingFor { get; set; } = string.Empty;
        public string Interests { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public List<Photo> Photos { get; set; } = new();
        public List<UserLike> LikedByUsers { get; set; }
        public List<UserLike> LikedUsers { get; set; }

    }
}
