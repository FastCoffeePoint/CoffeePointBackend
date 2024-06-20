using Cpb.Api.AspNetCore;
using Cpb.Application;
using Cpb.Application.Configurations;
using Cpb.Application.Services;
using Cpb.Common;
using Cpb.Common.Kafka;
using Cpb.Database;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


//Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<IngredientsService>();
builder.Services.AddScoped<CoffeeRecipesService>();
builder.Services.AddScoped<CoffeeMachinesService>();
builder.Services.AddScoped<OrdersService>();

// Kafka
var kafkaOptions = GetConfigurationOnRun<KafkaOptions>();
builder.Services
    .AddConsumer(kafkaOptions.OrderEventsConsumer)
    .AddEvent<CoffeeStartedBrewingEvent, CoffeeStartedBrewingEventHandler>()
    .AddEvent<CoffeeIsReadyToBeGottenEvent, CoffeeIsReadyToBeGottenEventHandler>()
    .AddEvent<OrderHasBeenCompletedEvent, OrderHasBeenCompletedEventHandler>()
    .AddEvent<OrderHasBeenFailedEvent, OrderHasBeenFailedEventHandler>();

builder.Services.AddProducer(kafkaOptions.OrderEventsProducer);

// Options
builder.Services.Configure<DefaultAdminOptions>(builder.Configuration.GetSection(DefaultAdminOptions.Name));
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.Name));
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection(KafkaOptions.Name));
builder.Services.Configure<BusinessOptions>(builder.Configuration.GetSection(BusinessOptions.Name));


// Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(u => u.UseNpgsqlConnection(builder.Configuration.GetConnectionString("HangfireConnection"))));
builder.Services.AddHangfireServer();


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddDbContext<DbCoffeePointContext>(u => 
    u.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCustomAuthentication(GetConfigurationOnRun<AuthOptions>());
builder.Services.AddAuthorization();

builder.Services.AddHttpClient();
builder.Services.AddCors(options => options.AddDefaultPolicy(u => u.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(u => 
{
    u.AddSecurityDefinition("Bearer", GetOpenApiSecurityScheme());
    u.AddSecurityRequirement(GetOpenApiSecurityRequirement());
});

var app = builder.Build();



await InitializeDb(app);

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHangfireDashboard();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();


#region Helpers

T GetConfigurationOnRun<T>() where T : IOptions
{
    var authOptions = builder.Configuration.GetSection(T.Name).Get<T>();
    if (authOptions is null)
        throw new ArgumentNullException($"Cannot get {nameof(T)}");
    return authOptions;
}

async Task InitializeDb(IHost host)
{
    using var scope = host.Services.CreateScope();
    var authService = scope.ServiceProvider.GetRequiredService<AuthService>();

    await authService.CreateDefaultAdmin();
}

OpenApiSecurityScheme GetOpenApiSecurityScheme() => new()
{
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JSON Web Token based security",
};

OpenApiSecurityRequirement GetOpenApiSecurityRequirement() => new()
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    }
};

#endregion