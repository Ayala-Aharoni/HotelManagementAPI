using AutoMapper;
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public  class RequestService :IRequestService
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IAlgorithmcs _algorithmics;
        private readonly Icontext ctx;
        private readonly INaiveBase _naiveBase; //עשיתי זאת רק לבדיקה!!!!!!!!!!!!!!!!!!!!! למחוק אחרי שווידאתי שזה עובד!!
        private readonly IHubContext<RequestHub> _hubContext;
        private readonly IMapper _mapper;

      
        private static readonly ConcurrentDictionary<int, List<string>> _analyzedWordsCache = new();

        public RequestService(IRequestRepository requestRepository, Icontext ctx,IAlgorithmcs algorithmcs, INaiveBase naiveBase , IHubContext<RequestHub> hubContext , IMapper mapper, IRepository<Category> categoryRepository)
        {
            this._requestRepository = requestRepository;
            this.ctx = ctx;
            this._algorithmics = algorithmcs;
            _naiveBase = naiveBase;
            _categoryRepository = categoryRepository;
            _hubContext = hubContext;   
            _mapper = mapper;   
        }
        public async Task<IEnumerable<RequestResponseDTO>> GetAll()
        {

            var requests = await _requestRepository.GetAll();

            return _mapper.Map<IEnumerable<RequestResponseDTO>>(requests);
        }

        public async Task<RequestResponseDTO> GetById(int id)
        {
            var request = await _requestRepository.GetById(id);

            if (request == null)
                throw new EntityNotFoundException("בקשה", id);  
            return _mapper.Map<RequestResponseDTO>(request);
        }

        //public async Task<IEnumerable<RequestResponseDTO>> GetRequestsByEmployee(int employeeId)
        //{
        //    var employeeExists = await ctx.Employees.AnyAsync(e => e.EmployeeId == employeeId);

        //    if (!employeeExists)
        //    {
        //        throw new EntityNotFoundException("עובד", employeeId);
        //    }

        //    var requests = await ctx.Requests
        //        .Where(r => r.EmployeeId == employeeId)
        //        .ToListAsync();

        //    return _mapper.Map<IEnumerable<RequestResponseDTO>>(requests);
        //}


        //זה לשאוללל את המורה !!!!
        //public async Task<IEnumerable<RequestResponseDTO>> GetMyInProgressTasks(int employeeId)
        //{
        //    // סינון ישירות בשאילתה מול ה-DB - הכי מהיר שיש!
        //    var tasks = await ctx.Requests
        //        .Where(r => r.EmployeeId == employeeId && r.Status == RequestStatus.InProgress)
        //        .ToListAsync();

        //    return _mapper.Map<IEnumerable<RequestResponseDTO>>(tasks);
        //}

        //public async Task<IEnumerable<RequestResponseDTO>> GetRequestsByEmployee(int employeeId)
        //{
        //    var employeeExists = await ctx.Employees.AnyAsync(e => e.EmployeeId == employeeId);

        //    if (!employeeExists)
        //    {
        //        throw new EntityNotFoundException("עובד", employeeId);
        //    }

        //    var requests = await ctx.Requests
        //        .Where(r => r.EmployeeId == employeeId)
        //        .ToListAsync();

        //    return _mapper.Map<IEnumerable<RequestResponseDTO>>(requests);
        //}
        public async Task<IEnumerable<RequestResponseDTO>> GetRequestsByEmployee(int employeeId)
        {
            var employeeExists = await ctx.Employees.AnyAsync(e => e.EmployeeId == employeeId);

            if (!employeeExists)
            {
                throw new EntityNotFoundException("עובד", employeeId);
            }

            var requests = await ctx.Requests
                .Include(r => r.Room) // <--- הוספת השורה הזו
                .Where(r => r.EmployeeId == employeeId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RequestResponseDTO>>(requests);
        }

        //public async Task<IEnumerable<RequestResponseDTO>> GetAvailableRequestsByCategory(int categoryId)
        //{
        //    // 1. בדיקה אופציונלית אם הקטגוריה קיימת (דומה לבדיקת העובד שלך)
        //    var categoryExists = await ctx.Categories.AnyAsync(c => c.CategoryId == categoryId);
        //    if (!categoryExists)
        //    {
        //        throw new EntityNotFoundException("קטגוריה", categoryId);
        //    }

        //    // 2. שליפת הבקשות: סטטוס NEW וגם שייכות לקטגוריה
        //    var requests = await ctx.Requests
        //        .Where(r => r.Status == RequestStatus.New && r.CategoryId == categoryId)
        //        .ToListAsync();

        //    // 3. מיפוי ל-DTO והחזרה
        //    return _mapper.Map<IEnumerable<RequestResponseDTO>>(requests);
        //}
        public async Task<IEnumerable<RequestResponseDTO>> GetAvailableRequestsByCategory(int categoryId)
        {
            var categoryExists = await ctx.Categories.AnyAsync(c => c.CategoryId == categoryId);
            if (!categoryExists)
            {
                throw new EntityNotFoundException("קטגוריה", categoryId);
            }

            var requests = await ctx.Requests
                .Include(r => r.Room) // <--- הוספת השורה הזו
                .Where(r => r.Status == RequestStatus.New && r.CategoryId == categoryId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RequestResponseDTO>>(requests);
        }


        public async Task Delete(int id)
        {
            // אפשר להוסיף כאן בדיקה: למשל, האם מותר למחוק בקשה שכבר הושלמה?
            var request = await _requestRepository.GetById(id);
            if (request == null)
            {
                throw new EntityNotFoundException("בקשה", id);
            }

            await _requestRepository.DeleteItem(id);
        }

       


        //פפה אני מזמנת את כל האלגוריתמים או לא?? לשאול ??
        //כאילו מבחינתי זה אמור ליצור BEW REQUEST עם קטגוריה שתחזור לי מכל הפונקציות שאזמו
        //createRequest
        public async Task CreateRequest(RequestDTO RequestDTO, int roomId)
        {
            var result = await _algorithmics.AnalisisRequest(RequestDTO.Description);
            Console.WriteLine($"Sending to Predict: {result.Count} words.");
            var category = await _algorithmics.ClassifyText(result);

            Console.WriteLine("********************************");
            Console.WriteLine($"THE PREDICTED CATEGORY ID IS: {category}");
            Console.WriteLine("********************************");


            Request newRequest = _mapper.Map<Request>(RequestDTO);
            newRequest.CategoryId = category;
            newRequest.Status = RequestStatus.New; // סטטוס התחלתי
            newRequest.RoomId = roomId;

            // 3. שמירה בבסיס הנתונים
            await _requestRepository.AddItem(newRequest);

            Console.WriteLine($"!!! FINAL CHECK BEFORE DB SAVE !!!");
            Console.WriteLine($"Request Description: {newRequest.Description}");
            Console.WriteLine($"Category ID to be saved: {newRequest.CategoryId}");
            Console.WriteLine($"Status: {newRequest.Status}");
            Console.WriteLine($"Room ID: {newRequest.RoomId}");

            //זה בשביל ההמילים שאעשה אותם בהמשך רק בלקיחת!
            _analyzedWordsCache[newRequest.RequestId] = result;

            // 4. מפר: הפיכת הישות (Request) ל-NotificationDTO (ההודעה לעובד)
            // עכשיו ל-newRequest יש כבר מזהה (ID) מה-DB
            var roomNumber = await ctx.Rooms
          .Where(r => r.Id == roomId)
          .Select(r => r.RoomNumber)
          .FirstOrDefaultAsync();

            // 4. מיפוי ל-DTO ושליחה בזמן אמת
            var notification = _mapper.Map<NotificationDTO>(newRequest);
            notification.RoomNumber = roomNumber ?? "Unknown"; // הצבה ידנית של מה ששלפנו
            notification.Title = $"בקשה חדשה מחדר {roomNumber}";

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



        //זו הפונקציה שאם זה לא יסתווג בצורה נכונה זה עובר לקבלה 
        //!!!!!!!!!!!!!!! לשנות את זה לגבי השליפה זה מיותר לשלוף את כל הקטגוריות ואז רק שם לעשות שליפה של בי שם 
        public async Task ReassignToReception(int requestId)
        {
            // 1. שליפת הבקשה
            var request = await _requestRepository.GetById(requestId);
            if (request == null) throw new Exception("Request not found");

            // שומרים את ה-ID הישן כדי שנדע את מי "להעניש" אחר כך
            int wrongCategoryId = request.CategoryId;

            // 2. שליפה של קטגוריית "קבלה"
            var allCategories = await _categoryRepository.GetAll();
            var receptionCategory = allCategories.FirstOrDefault(c => c.CategoryName == "Reception");

            if (receptionCategory == null)
            {
                throw new Exception("שגיאת מערכת: קטגוריית קבלה לא נמצאה");
            }

            int receptionId = receptionCategory.CategoryId;

            // 3. עדכון הבקשה ב-DB
            request.CategoryId = receptionId;
            request.EmployeeId = null;
            request.Status = RequestStatus.New;

            await _requestRepository.UpdateItem(requestId, request);

            // 4. SignalR - עדכון בזמן אמת לקבלה
            var notification = _mapper.Map<NotificationDTO>(request);
            await _hubContext.Clients.Group(receptionId.ToString())
                .SendAsync("ReceiveNotification", notification);

            // 5. ענישה למודל - Sherlock Mode: Correction
            // אנחנו שולפים מהקאש את המילים שגרמו לסיווג המוטעה
            if (_analyzedWordsCache.TryGetValue(requestId, out var words))
            {
                // קוראים לפונקציה שכתבנו שמורידה את ה-Frequency של המילים בקטגוריה הטועה
                await _algorithmics.DecreaseWordsFrequency(words, wrongCategoryId);

                Console.WriteLine($"[AI-PUNISH] Reduced weight for {words.Count} words in Category {wrongCategoryId}");
            }

            Console.WriteLine($"Request {requestId} was redirected to Reception");
        }
        public async Task<bool> TakeRequest(int requestId, int employeeId)
        {

            //await _naiveBase.LoadModel();//למחוקקקק את זה מכאן אחר כךךךך זה לא אמור להיות כל בקשה רק וידאי שהכל עובד פה!!!!!!!!!!

            // קריאה לפונקציה החדשה והיעילה שיצרנו ברפוסיטורי
            // היא מחזירה true אם העדכון הצליח (כלומר אף אחד לא תפס את זה לפנינו)
            bool success = await _requestRepository.TryAssignRequestAsync(requestId, employeeId);

            // 3. טיפול בכשלון: אם success הוא false, מישהו כבר תפס את זה
            if (!success)
            {
                // כאן אנחנו זורקים את השגיאה המיוחדת שיצרנו (409 Conflict)
                throw new RequestExceptions.RequestAlreadyAssigned();
            }
                // פקודה שקטה לכולם: "תמחקו את בקשה מספר X מהתצוגה"
                await _hubContext.Clients.All.SendAsync("RemoveRequestFromUI", requestId);

                var finalRequest = await _requestRepository.GetById(requestId);

                //זה בעצם ללמידה שקורית רק עכשיו שמשהו לקח זה אומר שאכן הקטגוריה מתאימה
                if (_analyzedWordsCache.TryRemove(requestId, out var words))
                {
                   
                    await _algorithmics.InsertWordsIntoWordTable(words, finalRequest.CategoryId);
                }
            return true;
        }


        public async Task CompleteRequest(int requestId, int employeeId)
        {
            // 1. נשלוף את המשימה מה-DB רק בשביל הדיבג כדי לראות מה המצב שלה
            var debugReq = await ctx.Requests.FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (debugReq == null)
            {
                Console.WriteLine($"[DEBUG] Request {requestId} NOT FOUND in DB.");
            }
            else
            {
                Console.WriteLine($"[DEBUG] Found Request: ID={debugReq.RequestId}, Status={debugReq.Status}, EmployeeId={debugReq.EmployeeId}");
                Console.WriteLine($"[DEBUG] Trying to complete with: InputRequestId={requestId}, InputEmployeeId={employeeId}");
            }

            // 2. ננסה לבצע את העדכון
            var rowsAffected = await ctx.Requests
                .Where(r => r.RequestId == requestId
                         && r.Status == RequestStatus.InProgress
                         && r.EmployeeId == employeeId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.Status, RequestStatus.Completed));

            Console.WriteLine($"[DEBUG] Rows affected: {rowsAffected}");

            if (rowsAffected == 0)
            {
                // כאן אנחנו בודקים ספציפית למה זה נכשל לפי הנתונים שראינו למעלה
                if (debugReq != null && debugReq.Status != RequestStatus.InProgress)
                {
                    throw new Exception($"לא ניתן להשלים: המשימה בסטטוס {debugReq.Status} ולא ב-InProgress.");
                }
                if (debugReq != null && debugReq.EmployeeId != employeeId)
                {
                    throw new Exception($"לא ניתן להשלים: המשימה רשומה על עובד {debugReq.EmployeeId} אבל את/ה עובד {employeeId}.");
                }

                throw new Exception("לא ניתן להשלים את הבקשה. וודאו שהנתונים תקינים.");
            }
        }

        //פונקציה שפקיד בקבלה מסווג באופן ידני את הבקשה למקום המתאים 
        public async Task TransferRequestToCorrectCategory(int requestId, int correctCategoryId)
        {
            // 1. שליפת הבקשה הנוכחית
            var request = await _requestRepository.GetById(requestId);
            if (request == null) throw new Exception("Request not found");

            // 2. מציאת ה-ID של הקבלה באופן דינמי (בשביל ה-SignalR)
            var allCategories = await _categoryRepository.GetAll();
            var reception = allCategories.FirstOrDefault(c => c.CategoryName == "Reception");
            if (reception == null) throw new Exception("Reception category not found");

            // 3. עדכון הנתונים - אנחנו מכינים את האובייקט לעדכון
            request.CategoryId = correctCategoryId;
            request.Status = RequestStatus.New;
            request.EmployeeId = null; // משחררים עובד קודם אם היה

            // קריאה לפונקציית ה-UpdateItem שלך - כאן השינוי נשמר סופית ב-DB
            await _requestRepository.UpdateItem(requestId, request);

            // 4. SignalR - עדכון הממשק

            // מעיפים מהמסך של הקבלה (הקבוצה הדינמית)
            await _hubContext.Clients.Group(reception.CategoryId.ToString())
                .SendAsync("RemoveRequestFromUI", requestId);

            // שולחים למסך של המחלקה הנכונה
            var notification = _mapper.Map<NotificationDTO>(request);
            await _hubContext.Clients.Group(correctCategoryId.ToString())
                .SendAsync("ReceiveNotification", notification);

            Console.WriteLine($"[FLOW] Request {requestId} was transferred by Reception to Category {correctCategoryId}");
        }
        public async Task Update(int id, RequestDTO requestDto)
        {
           //TODOOOOOOO!!
        }

    }
}
