using Common.DTO;
using Microsoft.Extensions.DependencyInjection;
using Repository.Entities;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public  static class ServiceDI
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IEmployeeService<AuthResponseDTO>, EmployeeService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<WordService>();
            services.AddScoped<RequestService>();

            return services;
        }
    }
}
