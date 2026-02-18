using Common.DTO;
using Repository.Entities; 
using Repository.Exception;
using Repository.Repositories;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IRequestService
    {
         Task<IEnumerable<Request>> GetAll();
         Task<Request> GetById(int id);
         Task Delete(int id);

        Task CreateRequest (RequestDTO request);    
        Task<bool> TakeRequest(int requestId, int employeeId);

        Task CompleteRequest(int requestId, int employeeId);
    }
}
