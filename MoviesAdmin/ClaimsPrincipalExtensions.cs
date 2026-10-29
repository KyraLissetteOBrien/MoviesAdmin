using System.Security.Claims;

namespace MoviesAdmin
{
    public static class ClaimsPrincipalExtensions //static - we dont have to intialize it, we can call it right away
    {
        public static string GetUserId(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User is not authenticated");
    }
}
