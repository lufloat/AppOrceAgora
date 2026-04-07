namespace OrceAgora.Domain.Entities;

public class LoginAttempt
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; }
}