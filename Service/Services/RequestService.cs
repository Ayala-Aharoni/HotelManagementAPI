using Common.DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using Repository.Repositories;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Service.Services
{
    public  class RequestService //:IRequestService
    {
        private readonly IRequestRepository requestRepository;
        private readonly IAlgorithmcs _algorithmics;
        private readonly Icontext ctx;
        public RequestService(IRequestRepository requestRepository, Icontext ctx)
        {
            requestRepository = requestRepository;
            this.ctx = ctx;
        }


        //פפה אני מזמנת את כל האלגוריתמים או לא?? לשאול ??
        //כאילו מבחינתי זה אמור ליצור BEW REQUEST עם קטגוריה שתחזור לי מכל הפונקציות שאזמו
        public Task CreateRequest(RequestDTO requestDto)
        {
            

        }


        // הוסיפי את ה-ID כפרמטר לפונקציה (הוא יגיע מה-Controller)
        public async Task<bool> TakeRequest(int requestId, int employeeId)
        {
            // קריאה לפונקציה החדשה והיעילה שיצרנו ברפוסיטורי
            // היא מחזירה true אם העדכון הצליח (כלומר אף אחד לא תפס את זה לפנינו)
            bool success = await requestRepository.TryAssignRequestAsync(
                requestId,
                employeeId  
            );
            if (success)
            {
                // כאן תוכלי להוסיף את הלוגיקה של SignalR בהמשך
                // await _hubContext.Clients.All.SendAsync("RequestTaken", requestId);!!!!!!!!!!
            }

            return success;
        }


        public async Task CompleteRequest(int requestId, int employeeId)
        {
            var rowsAffected = await ctx.Requests
                .Where(r => r.RequestId == requestId
                         && r.Status == RequestStatus.InProgress
                         && r.EmployeeId == employeeId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.Status, RequestStatus.Completed));

            if (rowsAffected == 0)
            {
                // ה-Middleware שלך יתפוס את זה וישלח הודעה יפה ל-Frontend
                throw new Exception("לא ניתן להשלים את הבקשה. וודא שהיא בטיפולך ושטרם הושלמה.");
            }
        }


    }
}
