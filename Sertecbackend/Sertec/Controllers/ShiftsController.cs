using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using Sertec.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{




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
            var result=ctx.shifts.ToList();

            return Ok(result);
        }

        // GET api/<ShiftsController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var result=ctx.shifts
                .Where(x => x.sId == id)
                .FirstOrDefault();

            return Ok(result);
        }

        // POST api/<ShiftsController>
        [HttpPost]
        public IActionResult Post([FromBody] Shift value)
        {

            ctx.shifts.Add(new Shift
            {
                sId=value.sId,
                planned=value.planned,
                uph=value.uph,
                waste=value.waste,
                ups=value.ups,
                units=value.units,
                unPlanned=value.unPlanned


            });

            ctx.SaveChanges();

            return Created();

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
