﻿namespace IP.Project.Contracts
{
    public class SambaResponse
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string IPv4Address { get; set; } = string.Empty;
    }
}