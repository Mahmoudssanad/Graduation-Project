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

// register HttpContextAccessor => Services ÌæÇ Çá session and claims ÚáÔÇä ÇÞÏÑ ÇÊÚÇãá ãÚ Çá 
builder.Services.AddHttpContextAccessor();

// register cache for session
builder.Services.AddDistributedMemoryCache();
// register Session
builder.Services.AddSession();

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

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedRolesAsync(roleManager);
}

// save roles enum to AspNetRole table in database
async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    var roles = Enum.GetNames(typeof(Roles));

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
