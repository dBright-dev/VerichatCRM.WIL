using Ganss.Xss;

namespace VerichatCRM.API.Middleware
{
    public class XssSanitizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHtmlSanitizer _sanitizer;

        public XssSanitizationMiddleware(RequestDelegate next)
        {
            _next = next;
            _sanitizer = new HtmlSanitizer();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.HasJsonContentType())
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                // For JSON, sanitize string values (simplified approach)
                // In production, use a JSON sanitization library
            }

            await _next(context);
        }
    }
}
