using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using Sertec.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{

    public class shiftPostDTO
    {
        public int planned { get; set; }
        public int uph { get; set; }
        public int waste { get; set; }
        public int ups { get; set; }
        public int units { get; set; }
        public int unPlanned { get; set; }
    }



    [Route("api/[controller]")]
    [ApiController]
    public class ShiftsController : ControllerBase
    {
        private readonly Appdbcontext ctx;
        public ShiftsController(Appdbcontext context)
        {
            ctx = context;
        }


        // GET: api/<ShiftsController>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var result = ctx.shifts.ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return NoContent();
            }

        }

        // GET api/<ShiftsController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var result = ctx.shifts
                .Where(x => x.sId == id)
                .FirstOrDefault();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound();
            }

        }

        // POST api/<ShiftsController>
        [HttpPost]
        public IActionResult Post([FromBody] shiftPostDTO value)
        {

            try
            {

                ctx.shifts.Add(new Shift
                {
                    planned = value.planned,
                    uph = value.uph,
                    waste = value.waste,
                    ups = value.ups,
                    units = value.units,
                    unPlanned = value.unPlanned


                });

                ctx.SaveChanges();

                return Created();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }



        }

        // PUT api/<ShiftsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ShiftsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
