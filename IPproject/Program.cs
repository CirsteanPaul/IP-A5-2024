using Carter;
using FluentValidation;
using IP.Project.Database;
using IP.Project.Extensions;
using IP.Project.LDAP;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// We need to add cors policy so other HOSTS, PORTS can connect to our application. Without it the integration tests
// will not work and neither the React app.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Open", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddDbContext<ApplicationDBContext>(db => 
db.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));
builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddCarter();

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
builder.Services.Configure<ConfigurationAD>
(
    c =>
    {
        c.Port = configuration.GetSection("AD:port").Get<int>();

        c.Zone = configuration.GetSection("AD:zone").Value;
        c.Domain = configuration.GetSection("AD:domain").Value;
        c.Subdomain = configuration.GetSection("AD:subdomain").Value;

        c.Username = configuration.GetSection("AD:username").Value;
        c.Password = configuration.GetSection("AD:password").Value;

        // connection string with port doesn't work on GNU/Linux and Mac OS
        //c.LDAPserver = $"{c.Subdomain}.{c.Domain}.{c.Zone}:{c.Port}";
        c.LDAPserver = $"{c.Subdomain}.{c.Domain}.{c.Zone}";
        // that depends on how it is in your LDAP server
        //c.LDAPQueryBase = $"DC={c.Subdomain},DC={c.Domain},DC={c.Zone}";
        c.LDAPQueryBase = $"DC={c.Domain},DC={c.Zone}";

        c.Crew = new StringBuilder()
            .Append($"CN={configuration.GetSection("AD:crew").Value},")
            // check which CN (Users or Groups) your LDAP server has the groups in
            .Append($"CN=Users,{c.LDAPQueryBase}")
            .ToString();
        c.Managers = new StringBuilder()
            .Append($"CN={configuration.GetSection("AD:managers").Value},")
            // check which CN (Users or Groups) your LDAP server has the groups in
            .Append($"CN=Users,{c.LDAPQueryBase}")
            .ToString();
    }
);
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ISignInManager, SignInManager>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(
        options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromDays(14);

            options.LoginPath = "/accounts/login";
            options.AccessDeniedPath = "/accounts/access-denied";
        }
    );

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddControllersWithViews(
    options =>
    {
        options.Filters.Add(new AuthorizeFilter());
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
}

app.UseRouting();

// these two come between UseRouting() and MapControllerRoute()
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapCarter();
app.UseHttpsRedirection();
app.UseCors("Open");

//LDAPInstance.Main();
string username = "tom";
string password = "pass123";
LDAPManager.CreateUser(username, password);
Console.WriteLine(LDAPManager.VerifyUser(username, password));
Console.WriteLine(LDAPManager.VerifyUser(username+"1", password));
Console.WriteLine(LDAPManager.VerifyUser(username, password+"1"));

app.Run();




// Added for integration tests.
public partial class Program
{

}
