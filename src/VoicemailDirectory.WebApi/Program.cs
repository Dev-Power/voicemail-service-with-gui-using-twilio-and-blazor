using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using VoicemailDirectory.WebApi;
using VoicemailDirectory.WebApi.Data;
using VoicemailDirectory.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var folder = Environment.SpecialFolder.LocalApplicationData;
var path = Environment.GetFolderPath(folder);
var dbPath = Path.Join(path, "recordings.db");
builder.Services.AddDbContext<RecordingContext>(options => options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddCors(policy =>
{
    policy.AddPolicy("CorsPolicy", opt => opt
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.AddTransient<FileService>();
builder.Services.AddTransient<IRecordingRepository, RecordingRepository>();

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

app.UseCors("CorsPolicy");

app.Run();