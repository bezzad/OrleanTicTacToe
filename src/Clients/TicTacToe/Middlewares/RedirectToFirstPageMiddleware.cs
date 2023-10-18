namespace TicTacToe.Middlewares
{
    public class RedirectToFirstPageMiddleware
    {
        public RedirectToFirstPageMiddleware(RequestDelegate next)
        { }

        public Task Invoke(HttpContext context)
        {
            context.Response.Redirect("/index.html");
            return Task.CompletedTask;
        }
    }
}
