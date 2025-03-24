using Microsoft.AspNetCore.Components;
using Synaplic.BlazorJwtApp.Shared;

namespace Synaplic.BlazorJwtApp.Client.Authentication
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly AuthService _authService;
        private readonly NavigationManager _navigation;

        public AuthHeaderHandler(AuthService authService, NavigationManager navigation)
        {
            _authService = authService;
            _navigation = navigation;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine(" ### AuthHeaderHandler send start");

            // skip login endpoints
            if (request.RequestUri?.AbsolutePath.Contains("/login") is  true)
            {
                return await base.SendAsync(request, cancellationToken);
            }
            
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(AuthenticationConstants.AuthScheme, token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await _authService.RefreshToken();
                token = await _authService.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(AuthenticationConstants.AuthScheme, token);
                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    _navigation.NavigateTo("/login");
                }
            }

            Console.WriteLine(" ### AuthHeaderHandler send end");

            return response;
        }
    }

    
}
