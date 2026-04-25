using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlataformaCreditos.Data;
using PlataformaCreditos.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
});

builder.Services.AddSession();

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Analista"))
    {
        await roleManager.CreateAsync(new IdentityRole("Analista"));
    }

    var email = "analista@demo.com";
    var user = await userManager.FindByEmailAsync(email);

    if (user == null)
    {
        user = new IdentityUser
        {
            UserName = email,
            Email = email
        };

        await userManager.CreateAsync(user, "Admin123!");
    }

    if (!await userManager.IsInRoleAsync(user, "Analista"))
    {
        await userManager.AddToRoleAsync(user, "Analista");
    }

    if (!context.Clientes.Any())
    {
        var cliente1 = new Cliente
        {
            UsuarioId = user.Id,
            IngresosMensuales = 2000,
            Activo = true
        };

        var cliente2 = new Cliente
        {
            UsuarioId = user.Id,
            IngresosMensuales = 3000,
            Activo = true
        };

        context.Clientes.AddRange(cliente1, cliente2);
        await context.SaveChangesAsync();

        context.Solicitudes.AddRange(
            new SolicitudCredito
            {
                ClienteId = cliente1.Id,
                MontoSolicitado = 1000,
                FechaSolicitud = DateTime.Now,
                Estado = "Pendiente"
            },
            new SolicitudCredito
            {
                ClienteId = cliente2.Id,
                MontoSolicitado = 2000,
                FechaSolicitud = DateTime.Now,
                Estado = "Aprobado"
            }
        );

        await context.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();