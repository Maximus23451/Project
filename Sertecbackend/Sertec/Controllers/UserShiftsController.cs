using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using Sertec.Models;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{


    public class userShiftGetDTO
    {
        public int shiftId { get; set; }
        public List<userDTO> users { get; set; }
    }

    public class userDTO
    {
        public string name { get; set; }
        public int uid { get; set; }
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
            try
            {
                var result = ctx.shifts
              .Select(shift => new userShiftGetDTO
              {
                  shiftId = shift.sId,
                  users = ctx.userShifts
                      .Where(us => us.shiftId == shift.sId)
                      .Select(us =>new userDTO
                      {
                          uid=us.userId,
                          name=us.users.Username
                      })
                      .ToList()
              })
              .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return NoContent();
            }

        }

        // GET api/<UserShiftsController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
               var result = ctx.shifts
              .Select(shift => new userShiftGetDTO
              {
                  shiftId = shift.sId,
                  users = ctx.userShifts
                      .Where(us => us.shiftId == shift.sId)
                      .Select(us => new userDTO
                      {
                          uid = us.userId,
                          name = us.users.Username
                      })
                      .ToList()
              })
              .Where(x=>x.shiftId==id)
              .FirstOrDefault();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        // POST api/<UserShiftsController>
        [HttpPost]
        public IActionResult Post(int userId)
        {

            try
            {
                var shift = ctx.shifts
                .Select(x => x.sId)
                .OrderBy(x=>x)
                .Last();

                ctx.userShifts.Add(new UserShifts
                {
                    userId = userId,
                    shiftId = shift
                });


                ctx.SaveChanges();

                return Created();
            }
            catch (Exception ex)
            {
                
                return BadRequest(ex.Message);


            }




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
