using Common.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR; 
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Exception;
using Repository.Interfaces;
using Repository.Repositories;
using Service.Hubs; 
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Service.Services
{
    public  class RequestService :IRequestService
    {
        private readonly IRequestRepository requestRepository;
        private readonly IAlgorithmcs _algorithmics;
        private readonly Icontext ctx;
        private readonly INaiveBase _naiveBase; //עשיתי זאת רק לבדיקה!!!!!!!!!!!!!!!!!!!!! למחוק אחרי שווידאתי שזה עובד!!
        private readonly IHubContext<RequestHub> _hubContext;
        private readonly IMapper _mapper;

        public RequestService(IRequestRepository requestRepository, Icontext ctx,IAlgorithmcs algorithmcs, INaiveBase naiveBase , IHubContext<RequestHub> hubContext , IMapper mapper)
        {
            this.requestRepository = requestRepository;
            this.ctx = ctx;
            this._algorithmics = algorithmcs;
            _naiveBase = naiveBase;
            _hubContext = hubContext;   
            _mapper = mapper;   
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
        public async Task CreateRequest(RequestDTO RequestDTO)
        {
            var result = await _algorithmics.AnalisisRequest(RequestDTO.Description);
            Console.WriteLine($"Sending to Predict: {result.Count} words.");
            await _naiveBase.LoadModel();//למחוקקקק את זה מכאן אחר כךךךך זה לא אמור להיות כל בקשה רק וידאי שהכל עובד פה!!!!!!!!!!
            var category = await _algorithmics.ClassifyText(result);

            Console.WriteLine("********************************");
            Console.WriteLine($"THE PREDICTED CATEGORY ID IS: {category}");
            Console.WriteLine("********************************");


            Request newRequest = _mapper.Map<Request>(RequestDTO);
            newRequest.CategoryId = category;
            newRequest.Status = RequestStatus.New; // סטטוס התחלתי

            // 3. שמירה בבסיס הנתונים
            await requestRepository.AddItem(newRequest); 

            // 4. מפר: הפיכת הישות (Request) ל-NotificationDTO (ההודעה לעובד)
            // עכשיו ל-newRequest יש כבר מזהה (ID) מה-DB
            var notification = _mapper.Map<NotificationDTO>(newRequest);

            // 5. הקסם של SignalR: שליחת ההודעה בזמן אמת
            // אנחנו שולחים את זה לקבוצה שהשם שלה הוא ה-ID של הקטגוריה
            await _hubContext.Clients.Group(category.ToString())
                .SendAsync("ReceiveNotification", notification);

            Console.WriteLine($"Notification sent to group {category}");


            //TODOOOOOOOOOOOOOO
            //כאן עלי ליצור בקשה חדשה באמת עם הקטגוריה המתאימה שהאלגוריתם החזיר לי ולשמור אותה בבסיס הנתונים    
            //כאן גם אמור להיות הלוגיקה של ה-SignalR שידווח ל-Frontend שיש בקשה חדשה (ככה שהעובדים יוכלו לראות את זה בזמן אמת)!!!!!!!!!!
            // צריך להיות גם טיפול בשגיאות 
            //

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
