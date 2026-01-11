using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskManager.Areas.Identity.Data;
using TaskManager.Data;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'TaskManagerContextConnection' not found.");;

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4))));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.Configure<TaskManager.Settings.AiSettings>(builder.Configuration.GetSection("AiSettings"));

// Register OpenAiService as a typed HTTP client and map IAiService to the concrete OpenAiService implementation.
builder.Services.AddHttpClient<TaskManager.Services.OpenAiService>();
builder.Services.AddScoped<TaskManager.Services.IAiService>(sp => sp.GetRequiredService<TaskManager.Services.OpenAiService>());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity.IsAuthenticated)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<TaskManager.Areas.Identity.Data.ApplicationUser>>();
        var signInManager = context.RequestServices.GetRequiredService<SignInManager<TaskManager.Areas.Identity.Data.ApplicationUser>>();

        var user = await userManager.GetUserAsync(context.User);

        if (user != null && user.LockoutEnd > DateTimeOffset.Now)
        {
            if (!context.Request.Path.Value.Contains("/Home/Banned"))
            {
                await signInManager.SignOutAsync();

                context.Response.Redirect("/Home/Banned");
                return; 
            }
        }
    }

    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();

