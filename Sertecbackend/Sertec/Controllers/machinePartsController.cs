using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using System.Diagnostics.CodeAnalysis;
using Sertec.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{
    public class machinePartGetDTO
    {
        public int mid { get; set; }
        public int pid { get; set; }
        public string partName { get; set; }
    }

    public class machinePartFilterDTO
    {
        public int mid { get; set; }
        public List<partsPostDTO> parts { get; set; }
    }

    public class machinePartPostDTO
    {
        public int machineId { get; set; }
        public string serialNumber { get; set; }

    }


    public class machinePartDeleteDTO
    {
        public int machineId { get; set; }
        public string serialNumber { get; set; }

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
                var machines = ctx.machineParts
                    .GroupBy(x => x.MachineId)
                     .Select(x => new machinePartFilterDTO
                     {
                         mid = x.Key,
                         parts = ctx.machineParts
                         .Where(mp => mp.MachineId == x.Key)
                         .Select(mp => new partsPostDTO
                         {
                             name = mp.Parts.name,
                             serialNumber = mp.Parts.serialNumber
                         }).ToList()
                     }).ToList();


                if (machines == null) return NotFound();
                return Ok(machines);


            }
            catch (Exception ex)
            {
                return BadRequest();
            }


        }

        // GET api/<machinePartsController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var machines = ctx.machineParts
                     .Where(x => x.MachineId == id)
                     .Select(x => new machinePartFilterDTO
                     {
                         mid = x.Machines.machineId,
                         parts = ctx.machineParts
                         .Where(mp => mp.MachineId == x.MachineId)
                         .Select(mp => new partsPostDTO
                         {
                             name = mp.Parts.name,
                             serialNumber = mp.Parts.serialNumber
                         }).ToList()
                     }).FirstOrDefault();


                if (machines == null) return NotFound();
                return Ok(machines);


            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        // POST api/<machinePartsController>
        [HttpPost]
        public IActionResult Post([FromBody] machinePartPostDTO value)
        {

            try
            {
                var part = ctx.parts
                    .Where(x => x.serialNumber == value.serialNumber)
                    .Select(x => x.pid)
                    .FirstOrDefault();


                if (part != null)
                {
                    ctx.machineParts.Add(new MachineParts
                    {
                        MachineId = value.machineId,
                        PartId = part
                    });

                    ctx.SaveChanges();



                    return Created();
                }

                return NotFound();


            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }

        // PUT api/<machinePartsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<machinePartsController>/5
        [HttpDelete]
        public IActionResult Delete([FromBody] machinePartDeleteDTO value )
        {
            try
            {
                var part = ctx.parts
                    .Where(x => x.serialNumber == value.serialNumber)
                    .Select(x=>x.pid)
                    .FirstOrDefault();

                var item = ctx.machineParts
                    .Where(x => x.MachineId == value.machineId && x.PartId == part)
                    .FirstOrDefault();

                if (item == null) return NotFound();

                ctx.machineParts.Remove(item);
                ctx.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();


            }


        }
    }
}
