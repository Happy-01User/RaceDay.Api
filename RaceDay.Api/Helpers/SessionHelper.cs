


namespace RaceDay.Api.Helpers
{
    public static class SessionHelper
    {
        public static int? GetUserId(HttpContext context)
        {
            return context.Session.GetInt32("UserId");
        }

        public static string? GetRole(HttpContext context)
        {
            return context.Session.GetString("Role");
        }

        public static bool IsLoggedIn(HttpContext context)
        {
            return GetUserId(context) != null;
        }

        public static bool HasRole(HttpContext context, string role)
        {
            var currentRole = GetRole(context);
            return currentRole != null &&
                   currentRole.Equals(role, StringComparison.OrdinalIgnoreCase);
        }
    }
}
