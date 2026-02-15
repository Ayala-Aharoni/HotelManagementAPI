using Common.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.Entities;
using Repository.Interfaces;
using Repository.Repositories;
using Service.Interfaces;
using Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public static class ServiceDI
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // שירותים כלליים / Repositories
            services.AddScoped<IRepository<Request>, RequestRepository>();
            services.AddScoped<IRepository<Word>, WordRepository>();
            services.AddScoped<IRepository<Category>, CategoryRepository>();
            services.AddScoped<ICategoryWordRepository, CategoryWordRepository>();

            // שירותי Business
            services.AddScoped<IRequestService, RequestService>();
            services.AddScoped<IAlgorithmcs, Algorithmics>(); // תלוי בארבעה השירותים למטה
            services.AddScoped<INaiveBase, NaiveBase>();

            // TextAnalyzer עם Factory שמושך סיסמה מה־appsettings
            services.AddScoped<ITextAnalyzer>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var password = config["TextAnalyzerSettings:Password"];

                if (string.IsNullOrWhiteSpace(password))
                    throw new Exception("TextAnalyzer password is missing in configuration!");

                return new TextAnalyzer(password);
            });

            // שירותים נוספים אם יש לך
            services.AddScoped<IEmployeeService<AuthResponseDTO>, EmployeeService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<WordService>();

            return services;
        }
    }
}