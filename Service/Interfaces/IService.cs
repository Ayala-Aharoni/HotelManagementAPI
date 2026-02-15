using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IService<T>
    {
        Task<IEnumerable<T>> GetAll();

        
        Task<T> GetById(int id);

        Task<T> Add(T entity);

        Task Update(int id, T entity);

        Task Delete(int id);



    }
}
