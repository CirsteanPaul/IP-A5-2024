using Carter;
using FluentValidation;
using IP.Project.Contracts.Interfaces;
using IP.Project.Database;
using IP.Project.Extensions;
using IP.Project.Infrastucture;
using IP.Project.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// We need to add cors policy so other HOSTS, PORTS can connect to our application. Without it the integration tests
// will not work and neither the React app.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Open", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("Location"));
});
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddInfrastrutureIdentityToDI(builder.Configuration);
builder.Services.AddDbContext<ApplicationDBContext>(db => 
db.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));
builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddCarter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
}
app.MapCarter();
app.UseHttpsRedirection();
app.UseCors("Open");
app.Run();


// Added for integration tests.
public partial class Program
{

}
