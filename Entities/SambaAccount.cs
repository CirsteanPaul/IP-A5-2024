namespace IP.Project.Entities;

public sealed class SambaAccount
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public string IPv4Address { get; set; } = string.Empty;
}