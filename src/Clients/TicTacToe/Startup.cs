using TicTacToe.Hubs;
using TicTacToe.Middlewares;

namespace TicTacToe;

public class Startup
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseStaticFiles(); // For the wwwroot folder
        app.UseDefaultFiles();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            //endpoints.MapGet("/", context =>
            //{
            //    context.Response.Redirect("/index.html");
            //    return Task.CompletedTask;
            //});
            endpoints.MapControllers();
            endpoints.MapHub<GameHub>("/gameHub");
        });
        app.MapWhen(ctx => ctx.Request.Path == "/" || ctx.Request.Path == "/index.html", app =>
        {
            app.UseMiddleware<RedirectToFirstPageMiddleware>();
        });
    }
}
