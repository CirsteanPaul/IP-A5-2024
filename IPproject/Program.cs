using Carter;
using FluentValidation;
using IP.Project.Database;
using IP.Project.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();
// We need to add cors policy so other HOSTS, PORTS can connect to our application. Without it the integration tests
// will not work and neither the React app.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Open", b => b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("Location"));
});
builder.Services.AddDbContext<ApplicationDBContext>(db => 
db.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));
builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddCarter();
builder.Services.AddIdentity(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("Open");
app.UseAuthorization();

app.MapCarter();

app.Run();

// Added for integration tests.
public partial class Program
{

}
