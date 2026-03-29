using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Sertec.Data;
using Sertec.Models;
using System.Reflection.PortableExecutable;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{

    public class machinePostDTO
    {
        public int machineId { get; set; }
        public string machineName { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class MachinesController : ControllerBase
    {

        private readonly Appdbcontext ctx;
        public MachinesController(Appdbcontext context)
        {
            ctx = context;
        }


        // GET: api/<MachinesController>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var result = ctx.machines.ToList();

                return Ok(result);
            }
            catch
            {
                return BadRequest();

            }




        }

        // GET api/<MachinesController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {

            try
            {
                var result = ctx.machines
                .Where(x => x.machineId == id)
                .FirstOrDefault();

                if(result==null) return NotFound();

                return Ok(result);
            }
            catch(Exception ex)
            {

                return BadRequest(ex.Message);

            }







        }

        // POST api/<MachinesController>
        [HttpPost]
        public IActionResult Post([FromBody] machinePostDTO value)
        {
            try
            {

                ctx.machines.Add(new Machines
                {
                    machineId = value.machineId,
                    name=value.machineName
                });

                ctx.SaveChanges();

                return Created();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }
        // PUT api/<MachinesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<MachinesController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {

                var result = ctx.machines
                    .Where(x => x.machineId == id)
                    .FirstOrDefault();

                ctx.machines.Remove(result);

                ctx.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {

                return NotFound();

            }







        }
    }
}
