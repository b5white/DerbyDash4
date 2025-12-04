namespace DerbyDash.Services {
    public class CurrentSessionMiddleware {
        private readonly RequestDelegate _next;

        public CurrentSessionMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, SessionData dto) {
            context.Items["SessionData"] = dto;
            await _next(context);
        }
    }
}
