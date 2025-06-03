using Cwiczenia_12_API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Rejestracja Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Rejestracja kontrolerów
builder.Services.AddControllers();

// 3. Rejestracja DbContext 
builder.Services.AddDbContext<FinalContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// 4. Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// 5. Mapowanie kontrolerów
app.MapControllers();

app.Run();