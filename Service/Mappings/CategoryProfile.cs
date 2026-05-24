using DataContext.DTO;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Service.Mappings

{
    public class CategoryProfile : Profile    
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName));

            CreateMap<CategoryDTO, Category>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore()); // אומרים ל-Mapper: אל תיגע ב-ID, ה-DB יטפל בזה
        }
    }
}

