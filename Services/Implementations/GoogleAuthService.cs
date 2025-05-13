using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using KDomBackend.Helpers;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Models.Entities;
using Microsoft.Extensions.Options;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly GoogleOAuthSettings _settings;
        private readonly IUserRepository _userRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly HttpClient _http;

        public GoogleAuthService(
            IOptions<GoogleOAuthSettings> settings,
            IUserRepository userRepository,
            JwtHelper jwtHelper)
        {
            _settings = settings.Value;
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _http = new HttpClient();
        }

        public async Task<string> HandleGoogleLoginAsync(string code)
        {
            // Get access token from Google
            var tokenResponse = await _http.PostAsync("https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
            { "code", code },
            { "client_id", _settings.ClientId },
            { "client_secret", _settings.ClientSecret },
            { "redirect_uri", _settings.RedirectUri },
            { "grant_type", "authorization_code" }
                }));

            tokenResponse.EnsureSuccessStatusCode();

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            using var tokenDoc = JsonDocument.Parse(tokenJson);
            var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();

            // Get user profile
            var profileRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
            profileRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var profileResponse = await _http.SendAsync(profileRequest);
            profileResponse.EnsureSuccessStatusCode();

            var profileJson = await profileResponse.Content.ReadAsStringAsync();
            using var profileDoc = JsonDocument.Parse(profileJson);

            var email = profileDoc.RootElement.GetProperty("email").GetString();
            var googleId = profileDoc.RootElement.GetProperty("id").GetString();
            var name = profileDoc.RootElement.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "user";

            // Find or create user
            var user = await _userRepository.GetByProviderIdAsync("google", googleId!);
            if (user == null)
            {
                user = new User
                {
                    Username = email!,
                    Email = email!,
                    PasswordHash = null,
                    Provider = "google",
                    ProviderId = googleId!,
                    RoleId = 1,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _userRepository.CreateAsync(user);
            }

            //  Return JWT
            return _jwtHelper.GenerateToken(user);
        }

    }
}
