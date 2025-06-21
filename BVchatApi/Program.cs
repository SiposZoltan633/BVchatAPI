using Microsoft.EntityFrameworkCore;
using BVchatApi.Data;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL kapcsolat
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// MVC + API controller támogatás
builder.Services.AddControllers();

// CORS engedélyezése minden irányból – ha mobilról vagy frontendről hívod
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger dokumentáció
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// CORS middleware
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
