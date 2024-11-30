
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoordinatorsAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        // GET: api/<HomeController>
        private readonly IConfiguration _configuration;

        // Wstrzykujemy BaseManager i IConfiguration do kontrolera
        public HomeController( IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/<HomeController>
        [HttpGet]
        public ActionResult<string> Get()
        {
            // Możemy zwrócić wynik w formacie JSON
            return Ok(new
            {
                Hello = "A"
            });
        }

        // GET api/<HomeController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<HomeController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<HomeController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<HomeController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
