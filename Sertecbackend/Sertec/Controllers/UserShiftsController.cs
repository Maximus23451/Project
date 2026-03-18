using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using Sertec.Models;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{


    public class userShiftGetDTO
    {
        public int shiftId { get; set; }
        public List<Users> users { get; set; }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class UserShiftsController : ControllerBase
    {
        private readonly Appdbcontext ctx;
        public UserShiftsController(Appdbcontext context)
        {
            ctx = context;
        }


        // GET: api/<UserShiftsController>
        [HttpGet]
        public IActionResult Get()
        {
            var result = ctx.userShifts
                .Select(x => new userShiftGetDTO
                {
                    shiftId = x.shiftId,
                    users=ctx.users.Where(u => u.uid == x.userId).ToList()
                })
                .ToList();

            return Ok(result);
        }

        // GET api/<UserShiftsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<UserShiftsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<UserShiftsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UserShiftsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
