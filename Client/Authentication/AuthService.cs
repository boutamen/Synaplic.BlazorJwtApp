using System.Net.Http.Json;
using Blazored.LocalStorage;
using Synaplic.BlazorJwtApp.Shared;

namespace Synaplic.BlazorJwtApp.Client.Authentication
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly ApiAuthenticationStateProvider _stateProvider;

        public AuthService(HttpClient httpClient, ApiAuthenticationStateProvider stateProvider, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _stateProvider = stateProvider;
        }

        public async Task<string> GetTokenAsync()
        {
            return await _localStorage.GetItemAsync<string>(AuthenticationConstants.AuthTokenKey);
        }
        public async Task RefreshToken()
        {
            var requestDTO = new RefreshTokenRequestDTO();
            requestDTO.Token = await _localStorage.GetItemAsync<string>(AuthenticationConstants.AuthTokenKey);
            requestDTO.RefreshToken = await _localStorage.GetItemAsync<string>(AuthenticationConstants.RefreshTokenKey);
        
            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", requestDTO);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();
                await _localStorage.SetItemAsync(AuthenticationConstants.AuthTokenKey, result.Token);
                await _localStorage.SetItemAsync(AuthenticationConstants.RefreshTokenKey, result.RefreshToken);
            }
            else
            {
                await _stateProvider.MarkUserAsLoggedOut();
            }
        }
    }
}
