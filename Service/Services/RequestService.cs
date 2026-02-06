using Repository.Entities;
using Repository.Interfaces;
using Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Service.Interfaces;
namespace Service.Services
{
    public  class RequestService //: IAlgorithmc
    {
        private readonly IRepository<Request> requestRepository;
        public RequestService(IRepository<Request> requestRepository)
        {
            requestRepository = requestRepository;
        }

    }
}
