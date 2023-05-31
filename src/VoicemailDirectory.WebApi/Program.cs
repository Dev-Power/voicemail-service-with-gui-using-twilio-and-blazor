using Microsoft.AspNetCore.HttpOverrides;
using VoicemailDirectory.WebApi;
using VoicemailDirectory.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.AddTransient<FileService>();

builder.Services.Configure<VoicemailOptions>(builder.Configuration.GetSection("Voicemail"));
// For load balancers, reverse proxies, and tunnels like ngrok and VS dev tunnels
// Follow guidance to secure here: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer
builder.Services.Configure<ForwardedHeadersOptions>(
    options => options.ForwardedHeaders = ForwardedHeaders.All
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();