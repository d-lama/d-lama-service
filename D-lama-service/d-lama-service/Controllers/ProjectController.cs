using d_lama_service.Repositories;
using Microsoft.AspNetCore.Mvc;
using Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace d_lama_service.Controllers
{
    /// <summary>

    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {

        private readonly IUnitOfWork _unitOfWork;

        public ProjectController(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/<ProjectController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _unitOfWork.ProjectRepository.GetAllAsync()); // Example of how to use unit of work 
        }

        // GET api/<ProjectController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            Project? project = await _unitOfWork.ProjectRepository.GetAsync(id);
            if (project != null) 
            {
                return Ok(project);
            }
            return NotFound();
        }

        // POST api/<ProjectController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ProjectController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ProjectController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
