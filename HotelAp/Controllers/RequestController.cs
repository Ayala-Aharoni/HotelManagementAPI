using Common.DTO;
using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Repository.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelAp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IRepository<Request> repository;
        //פה אני חייבתתת עוד תכונה שזה הסרביס!



        // GET: api/<RequestController>
        [HttpGet]
        public async Task<IEnumerable<Request>> Get()
        {
            return await repository.GetAll();
        }

        // GET api/<RequestController>/5
        [HttpGet("{id}")]
        public async Task<Request> Get(int id)
        {
            return await repository.GetById(id);
        }

        // POST api/<RequestController>
        [HttpPost]
        public void Post([FromBody] RequestDTO Req)
        {
            //פה אני צריכה לעשות!!! 

        }

        // PUT api/<RequestController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<RequestController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
