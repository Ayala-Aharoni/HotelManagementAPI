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
            CreateMap<CategoryDTO, Category>().ReverseMap();
        }
    }
}
