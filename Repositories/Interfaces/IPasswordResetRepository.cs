using KDomBackend.Models.Entities;

public interface IPasswordResetRepository
{
    Task CreateAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task MarkAsUsedAsync(int id);
}
