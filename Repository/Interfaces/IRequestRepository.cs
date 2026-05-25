using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Entities;  

namespace Repository.Interfaces
{
    public interface IRequestRepository:IRepository<Request>
    {
        Task<bool> TryAssignRequestAsync(int requestId, int employeeId);
        Task<bool> TryCompleteRequestAsync(int requestId, int employeeId);

        Task<IEnumerable<Request>> GetByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<Request>> GetAvailableByCategoryAsync(int categoryId);


    }
}
