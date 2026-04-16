using Common.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.Entities;
using Repository.Interfaces;
using Repository.Repositories;
using Service.Interfaces;
using Service.Services;
using System;

namespace Service
{
    public static class ServiceDI
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // --- Repositories (נשארים Scoped כי הם עובדים עם ה-DB) ---
            services.AddScoped<IRepository<Request>, RequestRepository>();
            services.AddScoped<IRepository<Word>, WordRepository>();
            services.AddScoped<IRepository<Category>, CategoryRepository>();
            services.AddScoped<ICategoryWordRepository, CategoryWordRepository>();

            // --- שירותי Business ---
            services.AddScoped<IRequestService, RequestService>();
            services.AddScoped<IAlgorithmcs, Algorithmics>();

            // --- המודל של ה-AI: הופך ל-Singleton (חי לנצח בזיכרון) ---
            services.AddSingleton<INaiveBase, NaiveBase>();

            // TextAnalyzer עם Factory שמושך סיסמה מה־appsettings
            services.AddScoped<ITextAnalyzer>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var password = config["TextAnalyzerSettings:Password"];

                if (string.IsNullOrWhiteSpace(password))
                    throw new Exception("TextAnalyzer password is missing in configuration!");

                return new TextAnalyzer(password);
            });

            // שירותים נוספים
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<WordService>();

            services.AddHttpClient<SimiliarWordsService>();
            services.AddScoped<ISimiliarWord, SimiliarWordsService>();

            return services;
        }
    }
}