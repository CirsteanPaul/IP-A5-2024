﻿namespace IP.Project.Contracts
{
    public class CreateAccountRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Matricol { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
