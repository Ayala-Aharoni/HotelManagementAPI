using AutoMapper;
using Common.DTO;
using DataContext.DTO;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Service.Mappings
{
    public class RequestProfile : Profile
    {
        public RequestProfile()
        {
            CreateMap<Request, RequestDTO>().ReverseMap();
            CreateMap<Request, NotificationDTO>();


            CreateMap<Request, RequestResponseDTO>()
     .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : "ללא קטגוריה"))
     .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Fullname : "טרם שובץ"))
     .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
     .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room != null ? src.Room.RoomNumber : "ללא חדר"));
        }




     

      
    }
}

