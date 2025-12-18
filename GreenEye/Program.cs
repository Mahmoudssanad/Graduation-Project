using GreenEye.Data.Seeder;
using GreenEye.Middleware;

var builder = WebApplication.CreateBuilder(args);


#region Add services to container

// Disable for auto ModelState validation
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);

// register DbContext
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// register Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(x => 
    x.User.AllowedUserNameCharacters = "AZXCVBNMLKJHGFDSAQWERTYUIOPazxcvbnmsdfghjklpoiuytrewq _1234567890")
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// register HttpContextAccessor => Services ÃÊ« «· session and claims ⁄·‘«‰ «ﬁœ— « ⁄«„· „⁄ «· 
builder.Services.AddHttpContextAccessor();

// register core(define who can be use me)
builder.Services.AddCors(builder =>
{
    builder.AddDefaultPolicy(options =>
    {
        options.AllowAnyHeader()
        .AllowAnyOrigin()
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<SimulationService>();

#endregion


var app = builder.Build();
//app.UseCors();

// Ì ﬁ›· »„Ã—œ Œ—ÊÃ «· „‰Â« Scoped ⁄·‘«‰ «· using «” Œœ„‰« 
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedRole.SeedRolesAsync(roleManager);
}

using(var scope = app.Services.CreateAsyncScope())
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
