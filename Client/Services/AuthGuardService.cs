using Microsoft.AspNetCore.Components.Authorization;
namespace Client.Services
{
    public class AuthGuardService
    {
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthGuardService(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> IsAuthorizedAsync(string requiredRole)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            return user.Identity?.IsAuthenticated == true && user.IsInRole(requiredRole);
        }
    }
}