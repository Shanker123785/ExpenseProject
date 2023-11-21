using Expenses.Core;
using Expenses.DB;
using ExpensesCore;
using Google;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(o =>
o.UseSqlServer(builder.Configuration.GetConnectionString("DB_CONNECTION_STRING")));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options=>
{
    options.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme,
        securityScheme: new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter the Bearer Authorization: `Bearer Generated-JWT-Token` ",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
        new OpenApiSecurityScheme
        {
            Reference= new OpenApiReference
            {
                Type=ReferenceType.SecurityScheme,
                Id=JwtBearerDefaults.AuthenticationScheme
            }
        },new string[]{ }
        }
    });
});
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddTransient<IExpensesServices, ExpensesServices>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.ToString());
});                                                    
builder.Services.AddCors(options =>
{
    options.AddPolicy("ExpensesPolicy",
        builder =>
        {
            builder.WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});
var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");

builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opts =>
{
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidateAudience = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret))
    };
});

var app = builder.Build();

//var service = (IServiceProviderFactory<AppDbContext>)app.Services.GetService(typeof(IServiceProviderFactory<AppDbContext>));
//using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
//{
//    var context = serviceScope.ServiceProvider.GetService<AppDbContext>();
//    //context.Database.EnsureCreated();
//    serviceScope.Database
//}

var service = app.Services.GetService<IServiceScopeFactory>();
using(var db = service.CreateScope().ServiceProvider.GetService<AppDbContext>())
{
    db.Database.Migrate();
}



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("ExpensesPolicy");
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
