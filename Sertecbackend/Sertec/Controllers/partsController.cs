using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using Sertec.Models;




// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{

    public class partsPostDTO
    {
        public string name { get; set; }
        public string serialNumber { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class partsController : ControllerBase
    {
        private readonly Appdbcontext ctx;
        public partsController(Appdbcontext context)
        {
            ctx = context;
        }


        // GET: api/<partsController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<partsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<partsController>
        [HttpPost]
        public IActionResult Post([FromBody] partsPostDTO value)
        {
            try
            {
                var sn = ctx.parts
                .Where(x => x.serialNumber == value.serialNumber)
                .FirstOrDefault();


                if (sn != null)
                {
                    return Conflict("Serial number already exists");
                }
                else
                {
                    ctx.parts.Add(new Parts
                    {
                        name = value.name,
                        serialNumber = value.serialNumber
                    });

                    ctx.SaveChanges();

                    return Created();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }




        }

        // PUT api/<partsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            

        }

        // DELETE api/<partsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
