using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TesteMuralisApi.Data;
using TesteMuralisApi.Mapper;
using TesteMuralisApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("PostgreSqlConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddHttpClient<ViaCepService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();