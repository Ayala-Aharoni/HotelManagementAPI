using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IAlgorithmcs
    {
        void GetRequest(string description);

       
        string[] ParseAndFilterWords();

        
        string DetermineTopCategory();

        void SendNotification(string category);
    }
}
