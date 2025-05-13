namespace KDomBackend.Services.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<string> HandleGoogleLoginAsync(string code);
    }
}
