using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using System.Reflection.Metadata;
using Sertec.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{
    public class PasswordRequestDTO
    {
        public string username { get; set; }
    }

    public class PasswordRequestGetDTO
    {
        public string userName { get; set; }
        public DateTime requestedAt { get; set; }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class PasswordRequestController : ControllerBase
    {

        private readonly Appdbcontext ctx;
        public PasswordRequestController(Appdbcontext context)
        {
            ctx = context;
        }


        // GET: api/<ValuesController>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var requests = ctx.PasswordRequests
                    .Select(x=>new PasswordRequestGetDTO
                    {
                        userName = ctx.users.Where(u => u.uid == x.userId).Select(u => u.Username).FirstOrDefault(),
                        requestedAt = x.requestedAt
                    })
                    .ToList();
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
           
            
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ValuesController>
        [HttpPost]
        public IActionResult Post([FromBody] PasswordRequestDTO value)
        {
            try
            {
                var users = ctx.users
                .Where(x => x.Username == value.username)
                .FirstOrDefault();

                var res=ctx.PasswordRequests
                    .Where(x => x.userId == users.uid)
                    .FirstOrDefault();


                if (res == null)
                {
                    ctx.PasswordRequests.Add(new PasswordRequest
                    {
                        userId = users.uid,
                        requestedAt = DateTime.Now
                    });

                    ctx.SaveChanges();

                    return Created();


                }

                return Conflict("A password reset request already exists for this user.");
            }
            catch (Exception ex) { 
            
                return BadRequest($"An error occurred while processing the request: {ex.Message}");


            }


                




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
