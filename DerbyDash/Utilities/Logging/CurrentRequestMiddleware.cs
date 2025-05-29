namespace DerbyDash.Utilities.Logging {
    public class CurrentRequestMiddleware {
        private readonly RequestDelegate _next;

        public CurrentRequestMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, CurrentRequestDTO dto) {
            context.Items["CurrentRequestDTO"] = dto;
            await _next(context);
        }
    }
}
