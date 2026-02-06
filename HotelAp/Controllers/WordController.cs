using DataContext.DTO;
using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Repository.Interfaces;
using Service.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelAp.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class WordController : ControllerBase
    {

        private readonly IRepository<Word> repository;
        private readonly WordService _wordService;
        public WordController(IRepository<Word> repo, WordService wordService)
        {
            repository = repo;
            _wordService = wordService;
        }
        // GET: api/<WordController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<WordController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<WordController>
        [HttpPost("AddWord")]
        public async Task<IActionResult> AddWord([FromBody] WordDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var word = await _wordService.AddWordAsync(dto);
                return Ok(word);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // PUT api/<WordController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<WordController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
