﻿namespace PersonalFinancialManager.Application.DTOs.User;

public class LoginDTO
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}
