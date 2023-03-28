using d_lama_service.Repositories;
using Microsoft.AspNetCore.Mvc;
using Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace d_lama_service.Controllers
{
    /// <summary>
    /// Example Controller... Will be deleted...
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ExampleController : ControllerBase
    {

        private readonly IUnitOfWork _unitOfWork;

        public ExampleController(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }
        
        // GET: api/<ExampleController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _unitOfWork.ExampleRepository.GetAllAsync()); // Example of how to use unit of work 
        }

        // GET api/<ExampleController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            Example? example = await _unitOfWork.ExampleRepository.GetAsync(id);
            if (example != null) 
            {
                return Ok(example);
            }
            return NotFound();
        }

        // POST api/<ExampleController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ExampleController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ExampleController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
