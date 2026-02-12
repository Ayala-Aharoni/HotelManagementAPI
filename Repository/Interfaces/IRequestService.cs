using Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Entities;  

namespace Repository.Interfaces
{
    public interface IRequestService
    {
        Task<IEnumerable<Request>> GetAllRequests();
        Task<Request> GetRequestById(int id);
        Task CreateRequest(RequestDTO requestDto);
        Task<bool> TakeRequest(int requestId, int employeeId);

        Task CompleteRequest(int requestId, int employeeId);
    }
}
