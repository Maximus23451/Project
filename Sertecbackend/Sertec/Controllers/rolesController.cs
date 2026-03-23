using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using Sertec.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{

    public class rolesPostDTO
    {
        public string name { get; set; }
    }


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

                if (result == null) return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
        public IActionResult Post([FromBody] rolesPostDTO value)
        {
            try
            {
                var role = ctx.roles
                    .Where(x => x.Name == value.name)
                    .FirstOrDefault();

                if (role == null)
                {

                    ctx.roles.Add(new Roles
                    {
                        Name = value.name
                    });

                    return Created();

                }

                return Conflict("This role already exists");




            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

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
