using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using INSCmapaFerias.Data;
using INSCmapaFerias.Areas.Identity.Data;
using System.Net.Mail;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// conexão BD.
//appsetting.json
var connectionString = builder.Configuration
    .GetConnectionString("ApplicationDbContextConnection") ?? 
    throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

//conexão com o contexto da app.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();;
app.UseAuthorization();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
