using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using Sertec.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class rolesController : ControllerBase
    {
        private readonly Appdbcontext ctx;
        public rolesController(Appdbcontext context)
        {
             ctx = context;
        }


        // GET: api/<rolesController>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var result = ctx.roles.ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return NoContent();
            }

        }

        // GET api/<rolesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<rolesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<rolesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<rolesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
