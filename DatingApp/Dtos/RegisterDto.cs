﻿using System.ComponentModel.DataAnnotations;

namespace DatingApp.Dtos
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;


        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
