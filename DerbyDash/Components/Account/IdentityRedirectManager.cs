using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace DerbyDash.Components.Account {
    internal sealed class IdentityRedirectManager(NavigationManager NavManager, ILogger<IdentityRedirectManager> Logger) {
        public const string StatusCookieName = "Identity.StatusMessage";

        private static readonly CookieBuilder StatusCookieBuilder = new() {
            SameSite = SameSiteMode.Strict,
            HttpOnly = true,
            IsEssential = true,
            MaxAge = TimeSpan.FromSeconds(5),
        };

        [DoesNotReturn]
        public void RedirectTo(string? uri, bool forceLoad = false) {
            uri ??= "";

            // Prevent open redirects.
            if (!Uri.IsWellFormedUriString(uri, UriKind.Relative)) {
                uri = NavManager.ToBaseRelativePath(uri);
            }
            Logger.LogInformation($"Redirecting to {uri}");
            // During static rendering, NavigateTo throws a NavigationException which is handled by the framework as a redirect.
            // So as long as this is called from a statically rendered Identity component, the InvalidOperationException is never thrown.
            NavManager.NavigateTo(uri, forceLoad);
            throw new InvalidOperationException($"{nameof(IdentityRedirectManager)} can only be used during static rendering.");
        }

        [DoesNotReturn]
        public void RedirectToWParams(string uri, Dictionary<string, object?> queryParameters) {
            var uriWithoutQuery = NavManager.ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
            var newUri = NavManager.GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
            RedirectTo(newUri);
        }

        [DoesNotReturn]
        public void RedirectToWithStatus(string uri, string message, HttpContext context) {
            context.Response.Cookies.Append(StatusCookieName, message, StatusCookieBuilder.Build(context));
            RedirectTo(uri);
        }

        private string CurrentPath => NavManager.ToAbsoluteUri(NavManager.Uri).GetLeftPart(UriPartial.Path);

        [DoesNotReturn]
        public void RedirectToCurrentPage() => RedirectTo(CurrentPath);

        [DoesNotReturn]
        public void RedirectToCurrentPageWithStatus(string message, HttpContext context)
            => RedirectToWithStatus(CurrentPath, message, context);
    }
}
