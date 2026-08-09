using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniWallet.Api.Data;
using MiniWallet.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("WalletDatabase")));
builder.Services.AddScoped<IWalletService, WalletService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("UnhandledException");
    logger.LogError(exception, "Unhandled error for {Method} {Path}", context.Request.Method, context.Request.Path);
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new ProblemDetails
    {
        Status = StatusCodes.Status500InternalServerError,
        Title = "An unexpected error occurred."
    });
}));

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<WalletDbContext>().Database.MigrateAsync();

app.MapControllers();
app.Run();

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public partial class Program { }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
