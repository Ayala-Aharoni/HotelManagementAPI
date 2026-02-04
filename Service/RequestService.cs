using Repository.Entities;
using Repository.Interfaces;
using Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public  class RequestService
    {
        private readonly IRepository<Request> _requestRepository;
        public RequestService(IRepository<Request> requestRepository)
        {
            _requestRepository = requestRepository;
        }
    }
}
