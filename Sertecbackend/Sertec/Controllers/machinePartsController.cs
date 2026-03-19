using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Sertec.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{
    public class machinePartGetDTO
    {
        public int mid { get; set; }
        public int pid { get; set; }
        public string partName { get; set; }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class machinePartsController : ControllerBase
    {

        private readonly Appdbcontext ctx;
        public machinePartsController(Appdbcontext context)
        {
            ctx = context;
        }

        // GET: api/<machinePartsController>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var result = ctx.machineParts
                .Select(x => new machinePartGetDTO
                {
                    mid = x.Machines.machineId,
                    pid = x.Parts.pid,
                    partName = x.Parts.name
                })
                .ToList();


                return Ok(result);
            }
            catch (Exception ex)
            {
                return NoContent();
            }


        }

        // GET api/<machinePartsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<machinePartsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<machinePartsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<machinePartsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
