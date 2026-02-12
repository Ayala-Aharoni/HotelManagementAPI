using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IRequestRepository:IRepository<Request>
    {
        Task<bool> TryAssignRequestAsync(int requestId, int employeeId);

    }
}
