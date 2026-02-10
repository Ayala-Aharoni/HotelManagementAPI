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
    public  static class ServiceDI
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IEmployeeService<AuthResponseDTO>, EmployeeService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<WordService>();
            services.AddScoped<RequestService>();
            services.AddScoped<IRepository<Request>, RequestRepository>();
            services.AddScoped<IRepository<Word>, WordRepository>();
            services.AddScoped<IRepository<Category>, CategoryRepository>();
            services.AddScoped<ICategoryWordRepository, CategoryWordRepository>();

            //זה למחלקת ניתוח המילים
            var password = configuration["TextAnalyzerSettings:Password"];
            services.AddScoped<ITextAnalyzer>(provider =>
                new TextAnalyzer("myPassword"));
            return services;
        }
    }
}
