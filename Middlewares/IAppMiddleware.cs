public interface IAppMiddleware
{
    Task InvokeAsync(HttpContext context);
}
