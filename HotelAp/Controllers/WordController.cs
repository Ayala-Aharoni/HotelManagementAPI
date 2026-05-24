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
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost("AddWord")]
        public async Task<IActionResult> AddWord([FromBody] string dto)
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
    }
    
}
