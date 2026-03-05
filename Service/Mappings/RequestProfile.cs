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

            // השורה החדשה: מאפשרת להמיר בקשה מה-DB לאובייקט של הודעה
            CreateMap<Request, NotificationDTO>();
        }
     

      
    }
}

