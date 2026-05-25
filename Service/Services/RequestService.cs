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
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IAlgorithmcs _algorithmics;
        private readonly IEmployeeRepository _employeeRepository;
        //   private readonly Icontext ctx;
        private readonly IRoomRepository _roomRepository;
        //    private readonly INaiveBase _naiveBase; //עשיתי זאת רק לבדיקה!!!!!!!!!!!!!!!!!!!!! למחוק אחרי שווידאתי שזה עובד!!
        private readonly IHubContext<RequestHub> _hubContext;
        private readonly IMapper _mapper;
        private static readonly ConcurrentDictionary<int, List<string>> _analyzedWordsCache = new();

        public RequestService(IRequestRepository requestRepository, Icontext ctx, IAlgorithmcs algorithmcs,/* INaiveBase naiveBase */ IHubContext<RequestHub> hubContext, IMapper mapper, IRepository<Category> categoryRepository, IRoomRepository roomRepository, IEmployeeRepository employeeRepository)
        {
            this._requestRepository = requestRepository;
            //   this.ctx = ctx;
            this._algorithmics = algorithmcs;
            //_naiveBase = naiveBase;
            _categoryRepository = categoryRepository;
            _hubContext = hubContext;
            _mapper = mapper;
            _roomRepository = roomRepository;
            _employeeRepository = employeeRepository;
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
        public async Task<IEnumerable<RequestResponseDTO>> GetRequestsByEmployee(int employeeId)
        {
            var employeeExists = await _employeeRepository.GetById(employeeId);
            if (employeeExists == null)
            {
                throw new EntityNotFoundException("עובד", employeeId);
            }
            var requests = await _requestRepository.GetByEmployeeIdAsync(employeeId);
            return _mapper.Map<IEnumerable<RequestResponseDTO>>(requests);
        }
        public async Task<IEnumerable<RequestResponseDTO>> GetAvailableRequestsByCategory(int categoryId)
        {
            // 1. נשתמש ב-CategoryRepository כדי לבדוק שהקטגוריה קיימת
            var category = await _categoryRepository.GetById(categoryId);
            if (category == null)
            {
                throw new EntityNotFoundException("קטגוריה", categoryId);
            }

            // 2. נשתמש ב-RequestRepository כדי לשלוף את הבקשות
            var requests = await _requestRepository.GetAvailableByCategoryAsync(categoryId);

            // 3. נמיר את התוצאה ל-DTO
            return _mapper.Map<IEnumerable<RequestResponseDTO>>(requests);
        }
        public async Task Delete(int id)
        {
            var request = await _requestRepository.GetById(id);
            if (request == null)
            {
                throw new EntityNotFoundException("בקשה", id);
            }
            await _requestRepository.DeleteItem(id);
        }

        public async Task CreateRequest(RequestDTO RequestDTO, int roomId)
        {
            var room = await _roomRepository.GetById(roomId);
            if (room == null)
            {
                throw new EntityNotFoundException("חדר", roomId);
            }
            string roomNumber = room.RoomNumber;

            var result = await _algorithmics.AnalisisRequest(RequestDTO.Description);
            Console.WriteLine($"Sending to Predict: {result.Count} words.");
            var category = await _algorithmics.ClassifyText(result);
            Console.WriteLine("********************************");
            Console.WriteLine($"THE PREDICTED CATEGORY ID IS: {category}");
            Console.WriteLine("********************************");

            Request newRequest = _mapper.Map<Request>(RequestDTO);
            newRequest.CategoryId = category;
            newRequest.Status = RequestStatus.New;
            newRequest.RoomId = roomId;
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
            // 4. מיפוי ל-DTO ושליחה בזמן אמת
            var notification = _mapper.Map<NotificationDTO>(newRequest);
            notification.RoomNumber = roomNumber ?? "Unknown"; // הצבה ידנית של מה ששלפנו
            notification.Title = $"בקשה חדשה מחדר {roomNumber}";

            // 5. הקסם של SignalR: שליחת ההודעה בזמן אמת
            // אנחנו שולחים את זה לקבוצה שהשם שלה הוא ה-ID של הקטגוריה
            await _hubContext.Clients.Group(category.ToString())
                .SendAsync("ReceiveNotification", notification);
            Console.WriteLine($"Notification sent to group {category}");
        }

        public async Task ReassignToReception(int requestId)
        {
            var request = await _requestRepository.GetById(requestId);
            if (request == null) throw new EntityNotFoundException("בקשה", requestId);
            int wrongCategoryId = request.CategoryId;

            var allCategories = await _categoryRepository.GetAll();
            var receptionCategory = allCategories.FirstOrDefault(c => c.CategoryName == "Reception");
            if (receptionCategory == null)
            {
                throw new EntityNotFoundException("קטגוריה", "Reception");
            }
            int receptionId = receptionCategory.CategoryId;
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
            bool success = await _requestRepository.TryAssignRequestAsync(requestId, employeeId);
            if (!success)
            {
                // כאן אנחנו זורקים את השגיאה המיוחדת שיצרנו (409 Conflict)
                throw new RequestExceptions.RequestAlreadyAssigned();
            }
            // פקודה שקטה לכולם: "תמחקו את בקשה מספר X מהתצוגה"
            await _hubContext.Clients.All.SendAsync("RemoveRequestFromUI", requestId);
            var finalRequest = await _requestRepository.GetById(requestId);

            //זה בעצם ללמידה שקורית רק עכשיו שמשהו לקח זה אומר שאכן הקטגוריה מתאימה
            // מנסים לשלוף את המילים מהזיכרון הזמני (העבודה הרגילה)
            if (_analyzedWordsCache.TryRemove(requestId, out var words))
            {
                // המילים נמצאו בזיכרון! מעדכנים את מסד הנתונים
                await _algorithmics.InsertWordsIntoWordTable(words, finalRequest.CategoryId);
            }
            else
            {
                // רשת ביטחון: הזיכרון נמחק או שהבקשה הוכנסה ידנית מ-SQL!
                // ניקח את הטקסט המקורי של הבקשה ונפרק אותו למילים עכשיו
                string text = finalRequest.Description;
                // פירוק המשפט למילים בודדות (לפי רווחים)
                List<string> backupWords = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                // שליחה של המילים לעדכון בטבלה, בדיוק כמו מקודם
                await _algorithmics.InsertWordsIntoWordTable(backupWords, finalRequest.CategoryId);
            }
            return true;
        }
        public async Task CompleteRequest(int requestId, int employeeId)
        {
            // פנייה אחת בלבד ל-DB - סופר מהירה, בלי שליפת נתונים מיותרת ובלי Include
            bool success = await _requestRepository.TryCompleteRequestAsync(requestId, employeeId);

            if (!success)
            {
                throw new AppException("לא ניתן להשלים את הבקשה. וודאו שהבקשה קיימת, משויכת אליכם ובסטטוס 'בעבודה'.", System.Net.HttpStatusCode.BadRequest);
            }

            Console.WriteLine($"[SUCCESS] Request {requestId} completed successfully.");
        }
        public async Task TransferRequestToCorrectCategory(int requestId, int correctCategoryId)
        {
            // 1. שליפת הבקשה הנוכחית (ה-GetById כבר מביא איתו את נתוני החדר בזכות ה-Include!)
            var request = await _requestRepository.GetById(requestId);
            if (request == null)
            {
                throw new EntityNotFoundException("Request", requestId);
            }

            // 2. מציאת קטגוריית הקבלה בצורה ממוקדת (שאילתה ישירה ב-DB)
            var allCategories = await _categoryRepository.GetAll();
            var reception = allCategories.FirstOrDefault(c => c.CategoryName == "Reception");
            if (reception == null)
            {
                throw new EntityNotFoundException("קטגוריה", 0); // או ליצור קונסטרקטור לטקסט חופשי, אבל הכי טוב זה להשתמש ב-AppException אם אין לך:
                                                                 // throw new AppException("מחלקת קבלה לא הוגדרה במערכת.", System.Net.HttpStatusCode.NotFound);
            }

            // 3. עדכון הנתונים והכנת האובייקט
            request.CategoryId = correctCategoryId;
            request.Status = RequestStatus.New;
            request.EmployeeId = null; // שחרור העובד הקודם

            // שמירה סופית ב-DB
            await _requestRepository.UpdateItem(requestId, request);

            // =========================================================================
            // 🔥 שדרוג הביצועים: חולצים את החדר ישירות מהאובייקט שכבר בזיכרון!
            // =========================================================================
            string roomNumber = request.Room?.RoomNumber ?? "לא ידוע";

            // 4. SignalR - עדכון הממשק

            // הסרה ממסך הקבלה
            await _hubContext.Clients.Group(reception.CategoryId.ToString())
                .SendAsync("RemoveRequestFromUI", requestId);

            // בניית ה-DTO למחלקה החדשה
            var notification = _mapper.Map<NotificationDTO>(request);
            notification.RoomNumber = roomNumber;
            notification.Title = $"בקשה חדשה מחדר {roomNumber}";

            // שליחה למחלקה החדשה
            await _hubContext.Clients.Group(correctCategoryId.ToString())
                .SendAsync("ReceiveNotification", notification);

            Console.WriteLine($"[FLOW] Request {requestId} was transferred by Reception to Category {correctCategoryId}");
        }
    }
}
