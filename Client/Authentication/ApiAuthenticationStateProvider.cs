using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Synaplic.BlazorJwtApp.Shared;

namespace Synaplic.BlazorJwtApp.Client.Authentication
{
 
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
  

        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _httpClient;
        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

        public ApiAuthenticationStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
        {
            _localStorage = localStorage;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsStringAsync(AuthenticationConstants.AuthTokenKey);
           //  Console.WriteLine("Token: " + token);
            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( AuthenticationConstants.AuthScheme, token);
            var user = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), AuthenticationConstants.AuthType));
            return new AuthenticationState(user);
        }
        public async Task<ClaimsPrincipal> GetAuthStateUserAsync()
        {
            var state = await this.GetAuthenticationStateAsync();
            var authenticationStateProviderUser = state.User;
            return authenticationStateProviderUser;
        }

        private async Task SetToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                await _localStorage.RemoveItemAsync(AuthenticationConstants.AuthTokenKey);
                _httpClient.DefaultRequestHeaders.Authorization = null;
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            }
            else
            {
                await _localStorage.SetItemAsync(AuthenticationConstants.AuthTokenKey, token);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationConstants.AuthScheme, token);
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), AuthenticationConstants.AuthType));
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }

        public async Task MarkUserAsAuthenticated(string token)
        {
            await SetToken(token);
        }

        public async Task MarkUserAsLoggedOut()
        {
            await SetToken(null);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var parts = jwt.Split('.');
            if (parts.Length != 3)
                throw new ArgumentException("Invalid JWT token format");

            var payload = parts[1]; // Get the payload part

            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);

            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            var claims = new List<Claim>();
            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Value is JsonElement element)
                {
                    if (element.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in element.EnumerateArray())
                        {
                            claims.Add(new Claim(kvp.Key, item.GetString() ?? string.Empty));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(kvp.Key, element.ToString()));
                    }
                }
            }

            return claims;
        }


      

    }
}

