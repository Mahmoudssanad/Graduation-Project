using GreenEye.CustomValidation;
using GreenEye.Data.Seeder;
using GreenEye.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


#region Add services to container

// Disable for auto ModelState validation
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);

// register DbContext
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// register Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(x => 
    x.User.AllowedUserNameCharacters = null!)
    .AddUserValidator<UserValidator>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// register HttpContextAccessor => Services ÃÊ« «· session and claims ⁄·‘«‰ «ﬁœ— « ⁄«„· „⁄ «· 
builder.Services.AddHttpContextAccessor();

// Create Logger
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();


// Register Authentication => Session by default ⁄·‘«‰ »Ì⁄ „œ ⁄·Ì «· 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = ""
    };
});


// register core(define who can be use me)
builder.Services.AddCors(builder =>
{
    builder.AddDefaultPolicy(options =>
    {
        options.AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowed(origins => true)
        .AllowCredentials();
    });
});

// register redis cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

// register services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ISimulationService, SimulationService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ICropDiseaseService, CropDiseaseService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<SimulationService>();

builder.Host.UseSerilog();


#endregion


var app = builder.Build();


try
{
    app.UseCors();

    // Ì ﬁ›· »„Ã—œ Œ—ÊÃ «· „‰Â« Scoped ⁄·‘«‰ «· using «” Œœ„‰« 
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedRole.SeedRolesAsync(roleManager);
    }

    using (var scope = app.Services.CreateAsyncScope())
    {
        var user = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await SeedAdmin.SeedAdminAsync(user);
    }

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseMiddleware<GlobalExceptionHandler>();
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}catch(Exception ex)
{
    Log.Error(ex.Message);
}

