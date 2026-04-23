//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using SCICHRPortal.API.Extensions;
using SCICHRPortal.Repository;
using SCICHRPortal.Utility.Cryptography;
using SCICHRPortal.Utility.Cryptography.Interfaces;
using SCICHRPortal.Utility.HttpContext.Implementations;
using SCICHRPortal.Utility.HttpContext.Interfaces;
using SCICHRPortal.Utility.Settings;
using System.Text;
using System.Text.Json.Serialization;
using SCICHRPortal.Repository.Implementations;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);
OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("Manuel A. Rivas Jr.");
// Add services to the container.
builder.Services.AddDerivedClassesServices();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var tokenKey = builder.Configuration.GetValue<string>("JWTSecretKey");
var key = Encoding.ASCII.GetBytes(tokenKey);


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);

var mailSettingsSection = builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailSettingsSection);


builder.Services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();
builder.Services.AddSingleton<IJsonWebTokenGenerator>(sp => new JsonWebTokenGenerator(tokenKey, sp.GetService<IRefreshTokenGenerator>()!));

builder.Services.AddScoped<IHttpContextService, HttpContextService>();

builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();


//builder.Services.AddDbContext<ApplicationContext>(options =>
//               options.UseSqlServer(
//                   builder.Configuration.GetConnectionString("DefaultConnection"),
//                   options => options.MigrationsAssembly("WebEnrolmentSystem.Repository")));

string mySqlConnectionStr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseNpgsql(mySqlConnectionStr,
    //ServerVersion.AutoDetect(mySqlConnectionStr),
     options => options.MigrationsAssembly("SCICHRPortal.Repository")));


builder.Services.AddCors(options =>
           options.AddDefaultPolicy(
               b =>
               {
                   //builder.WithOrigins(Configuration["WebBaseUrl"])
                   // .AllowAnyHeader()
                   // .AllowAnyMethod();

                   b.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
               }
           ));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

