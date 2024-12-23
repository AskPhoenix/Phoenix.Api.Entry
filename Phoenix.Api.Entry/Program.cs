using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Phoenix.DataHandle.Senders;
using System.Text;
using static Phoenix.DataHandle.Api.DocumentationHelper;

var builder = WebApplication.CreateBuilder(args);

// Configure Web Host Defaults
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// Add services to the container.
Action<DbContextOptionsBuilder> buildDbContextOptions = o => o
    .UseLazyLoadingProxies()
    .UseSqlServer(builder.Configuration.GetConnectionString("PhoenixConnection"));

builder.Services.AddDbContext<ApplicationContext>(buildDbContextOptions);
builder.Services.AddDbContext<PhoenixContext>(buildDbContextOptions);

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddRoles<ApplicationRole>()
    .AddUserStore<ApplicationStore>()
    .AddUserManager<ApplicationUserManager>()
    .AddEntityFrameworkStores<ApplicationContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddScoped(_ =>
    new EmailSender(builder.Configuration["SendGrid:Key"]));

builder.Services.AddApplicationInsightsTelemetry(
    o => o.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]);

builder.Services.AddControllers()
    .AddNewtonsoftJson();
builder.Services.AddHttpsRedirection(o => o.HttpsPort = 443);
builder.Services.AddRouting(o => o.LowercaseUrls = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenNewtonsoftSupport();
builder.Services.AddSwaggerGen(o =>
{
    o.EnableAnnotations();

    // SwaggerDoc name refers to the name of the documention and is included in the endpoint path
    o.SwaggerDoc("v3", new OpenApiInfo()
    {
        Title = "Pavo API",
        Description = "A Rest API for the school data entry.",
        Version = "3.0"
    });

    o.AddSecurityDefinition(JWTSecurityScheme.Reference.Id, JWTSecurityScheme);

    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { JWTSecurityScheme, Array.Empty<string>() }
    });

    o.TagActionsBy(api =>
    {
        string tag = string.Empty;

        if (!string.IsNullOrEmpty(api.GroupName))
            tag = $"{api.GroupName} \u2013 ";

        if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            tag += controllerActionDescriptor.ControllerName;

        return new string[] { tag };
    });

    o.DocInclusionPredicate((name, api) => true);
});

// Configure Logging
// TODO: Create File Logging & on app insights
builder.Logging.ClearProviders()
    .AddConfiguration(builder.Configuration.GetSection("Logging"))
    .SetMinimumLevel(LogLevel.Trace)
    .AddSimpleConsole()
    .AddDebug();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // app.UseDatabaseErrorPage();
}
else
{
    app.UseHsts();
}

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
app.UseSwagger(o => o.RouteTemplate = "doc/{documentname}/swagger.json");
app.UseSwaggerUI(
    o =>
    {
        o.SwaggerEndpoint("/doc/v3/swagger.json", "Pavo v3");
        o.RoutePrefix = "doc";
        //o.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
