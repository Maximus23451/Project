using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using Sertec.Models;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{
    public class shiftPartGetDTO
    {
        public int shiftId { get; set; }
        public List<partsPostDTO> parts { get; set; }
    }

    public class shiftPartPostDTO
    {
        public string serialNumber { get; set; }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class ShiftPartsController : ControllerBase
    {

        private readonly Appdbcontext ctx;
        public ShiftPartsController(Appdbcontext context)
        {
            ctx = context;
        }

        // GET: api/<ShiftPartsController>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var result = ctx.shifts
              .Select(shift => new shiftPartGetDTO
              {
                  shiftId = shift.sId,
                  parts = ctx.shiftParts
                      .Where(us => us.shiftId == shift.sId)
                      .Select(us => new partsPostDTO
                      {
                          name = us.parts.name,
                          serialNumber = us.parts.serialNumber
                      })
                      .ToList()
              })
              .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        // GET api/<ShiftPartsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ShiftPartsController>
        [HttpPost]
        public IActionResult Post([FromBody] shiftPartPostDTO value)
        {

            try
            {
                var shift=ctx.shifts
                    .Select(x=>x.sId)
                    .OrderBy(x => x)
                    .LastOrDefault();

                var part=ctx.parts
                    .Where(x => x.serialNumber == value.serialNumber)
                    .FirstOrDefault();

                ctx.shiftParts.Add(new ShiftParts
                {
                    shiftId = shift,
                    partId = part.pid
                });

                ctx.SaveChanges();

                if(shift==null || part==null) return NotFound();

                return Created();

            }
            catch (Exception ex) { 
            
                return BadRequest();

            }


        }

        // PUT api/<ShiftPartsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ShiftPartsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
