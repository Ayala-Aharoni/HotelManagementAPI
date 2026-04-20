using HotelAp.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;
using Repository.Entities;
using Repository.Interfaces;
using Repository.Repositories;
using Service;
using Service.Interfaces;
using Service.Mappings; 
using Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Service.Hubs;

var builder = WebApplication.CreateBuilder(args);

//זה בשביל שהוא ימיר אינמים ממספרים למחרוזת
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
       
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "securityLessonWebApi", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Please enter your bearer token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
});
//בעת האימות
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(option =>
             option.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidateIssuer = true,
                 ValidateAudience = true,
                 ValidateLifetime = true,
                 ValidateIssuerSigningKey = true,
                 ValidIssuer = builder.Configuration["Jwt:Issuer"],
                 ValidAudience = builder.Configuration["Jwt:Audience"],
                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
             });




//מכאן כל מה שתוך ירוק ארוך זה שלי :)
/////////////////////////////////////////
///בכל בקשה יוצר מופע-עבור גולש
//כל פעם שמשהו מבקש מופע של איקונטס וזה קורה לי הרי בתוך הרפוסיטורי אני ישלח לו את הדטהביס - הוטל!!
builder.Services.AddScoped<Icontext,HotelDbContext>();
// רישום הממשק הגנרי והמימוש שלו


builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IRoomService, RoomService>();    


// Repositories
builder.Services.AddRepositories();

// Services
builder.Services.AddServices(builder.Configuration);


//MAPPEER
builder.Services.AddAutoMapper(typeof(CategoryProfile));
////////////////////////////////////////
///////////////////////////////////////




// רישום השירות שלך והגדרת ה-HttpClient שלו
builder.Services.AddHttpClient<TextAnalysisService>();//זה בשביל לתקשר עם פיתון
builder.Services.AddScoped<TextAnalysisService>();


//זה השורה של ההאזנה לSINGLER בשביל שליחת הודעות לעובדים
builder.Services.AddSignalR();


builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowSpecificOrigins",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost") // מאשר כל פורט על לוקלהוסט!
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});


//!
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var naiveBase = scope.ServiceProvider.GetRequiredService<INaiveBase>();
    // קורא ל-LoadModel שכתבנו, שמושך הכל מה-DB לזיכרון
    await naiveBase.LoadModel();
}
//
app.UseMiddleware<ExceptionMiddleware>();//זה בשביל השגיאותתת
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "securityLessonWebApi V1");
    c.RoutePrefix = string.Empty; // פותח את ה-UI ישר ב-root
});


app.UseCors("MyAllowSpecificOrigins");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//זה בשביל שהסינגלר יתחבר לשרת ויוכל לקבל הודעות    
app.MapHub<RequestHub>("/requestHub"); // זה הנתיב שהריאקט יתחבר אליו

app.Run();
 

