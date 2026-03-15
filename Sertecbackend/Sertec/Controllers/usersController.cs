using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using Sertec.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{

    public class userGetDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public int roleid { get; set; }
    }



    [Route("api/[controller]")]
    [ApiController]
    public class usersController : ControllerBase
    {
        private readonly Appdbcontext ctx;
        public usersController(Appdbcontext context)
        {
            ctx = context;
        }


        // GET: api/<ValuesController>
        [HttpGet]
        public IActionResult Get()
        {
            var result = ctx.users
                .Select(x => new userGetDTO
                {
                    id = x.uid,
                    name = x.Username,
                    email = x.Email,
                    roleid = x.roleid.Rid
                });


            return Ok(result);
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ValuesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
