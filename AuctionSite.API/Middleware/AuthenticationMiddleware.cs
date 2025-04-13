namespace AuctionSite.API.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                _logger.LogInformation("Middleware processing request for {Path}", context.Request.Path);

                if (context.Request.Path.StartsWithSegments("/api/auth") ||
                    context.Request.Method == "OPTIONS" ||
                    context.Request.Path.StartsWithSegments("/swagger"))
                {
                    _logger.LogInformation("Skipping authentication for {Path}", context.Request.Path);
                    await _next(context);
                    return;
                }

                if (context.User?.Identity?.IsAuthenticated != true)
                {
                    _logger.LogWarning("Unauthorized access attempt to {Path}", context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Unauthorized" });
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in authentication middleware for {Path}", context.Request.Path);

                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsJsonAsync(new { message = "An error occurred" });
                }
                else
                {
                    throw;
                }
            }
        }
    }
}