namespace PropertyManagement.MVC.Services
{
    public class TokenService
    {
        private const string TokenCookieName = "jwtToken";
        private const string RoleCookieName = "userRole";
        private const string EmailCookieName = "userEmail";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SaveToken(string token, string email, IList<string> roles)
        {
            var context = _httpContextAccessor.HttpContext!;
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.Now.AddHours(8)
            };

            context.Response.Cookies.Append(TokenCookieName, token, options);
            context.Response.Cookies.Append(EmailCookieName, email, options);
            context.Response.Cookies.Append(
                RoleCookieName,
                string.Join(",", roles),
                options);
        }

        public string? GetToken()
        {
            return _httpContextAccessor.HttpContext?
                .Request.Cookies[TokenCookieName];
        }

        public string? GetEmail()
        {
            return _httpContextAccessor.HttpContext?
                .Request.Cookies[EmailCookieName];
        }

        public string? GetRole()
        {
            return _httpContextAccessor.HttpContext?
                .Request.Cookies[RoleCookieName];
        }

        public void ClearToken()
        {
            var context = _httpContextAccessor.HttpContext!;
            context.Response.Cookies.Delete(TokenCookieName);
            context.Response.Cookies.Delete(EmailCookieName);
            context.Response.Cookies.Delete(RoleCookieName);
        }

        public bool HasToken()
        {
            var token = GetToken();
            return !string.IsNullOrEmpty(token);
        }
    }
}