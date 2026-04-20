using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IRoomService
    {
        Task<string> SetupRoomAsync(RoomDTO R);
    }
}
