using Common.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Exception;
using Repository.Interfaces;
using Repository.Repositories;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Service.Services
{
    public  class RequestService :IRequestService
    {
        private readonly IRequestRepository requestRepository;
        private readonly IAlgorithmcs _algorithmics;
        private readonly Icontext ctx;
        public RequestService(IRequestRepository requestRepository, Icontext ctx,IAlgorithmcs algorithmcs)
        {
           this.requestRepository = requestRepository;
            this.ctx = ctx;
            this._algorithmics = algorithmcs;
         }
        public async Task<IEnumerable<Request>> GetAll()
        {

            return await requestRepository.GetAll();
        }

        public async Task<Request> GetById(int id)
        {
            var request = await requestRepository.GetById(id);
            if (request == null)
            {
                throw new EntityNotFoundException("בקשה", id);
            }
            return request;
        }
        public async Task Delete(int id)
        {
            // אפשר להוסיף כאן בדיקה: למשל, האם מותר למחוק בקשה שכבר הושלמה?
            var request = await requestRepository.GetById(id);
            if (request == null)
            {
                throw new EntityNotFoundException("בקשה", id);
            }

            await requestRepository.DeleteItem(id);
        }



        //פפה אני מזמנת את כל האלגוריתמים או לא?? לשאול ??
        //כאילו מבחינתי זה אמור ליצור BEW REQUEST עם קטגוריה שתחזור לי מכל הפונקציות שאזמו
        //createRequest
        public Task<List<string>> CreateRequest(RequestDTO Request)
        {
            //TODOOOOOOOOOOOOO
            var result = _algorithmics.AnalisisRequest(Request.Description);
            return Task.FromResult(result); // מחזיר ל-Controller
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


        public async Task Update(int id, RequestDTO requestDto)
        {
           //TODOOOOOOO!!
        }

    }
}
