using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IAlgorithmcs
    {
        //  void GetRequest(string description);


        //זה כבר עשיתי!
        //מחזיר רשימת מילים רלוונטיות מתוך התיאור של הבקשה!
        List<string> AnalisisRequest(string content);
    }
        


        
}
