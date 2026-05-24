using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Service.Hubs
{
    public class RequestHub : Hub   
    {
        public async Task JoinCategoryGroup(int categoryId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, categoryId.ToString());
            Console.WriteLine($"Employee connected to category group: {categoryId}");
        }
    }
}
