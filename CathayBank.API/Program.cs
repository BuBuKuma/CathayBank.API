using CathayBank.API.Data;
using CathayBank.API.Handlers;
using CathayBank.API.Middlewares;
using CathayBank.API.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using NLog.Web;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<CryptoSettings>(builder.Configuration.GetSection("CryptoSettings"));
builder.Services.AddSingleton<ICryptoService, CryptoService>();

builder.Services.AddTransient<LoggingDelegatingHandler>();
builder.Services.AddHttpClient("CoinDeskClient")
    .AddHttpMessageHandler<LoggingDelegatingHandler>();
builder.Services.AddScoped<ICoinDeskService, CoinDeskService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();

var app = builder.Build();

app.UseUnifiedDiagnostics();
var supportedCultures = new[] { new CultureInfo("zh-TW"), new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("zh-TW"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CathayBank Exam API V1");
    });
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();