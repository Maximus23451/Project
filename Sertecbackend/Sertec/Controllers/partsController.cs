using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using Sertec.Models;
using System.Diagnostics.CodeAnalysis;




// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{

    public class partsPostDTO
    {
        public string name { get; set; }
        public string serialNumber { get; set; }
    }

    public class partsGetDTO
    {
        public string name { get; set; }
        public string serialNumber { get; set; }
    }

    public class partsFilterDTO
    {
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
        public IActionResult Get()
        {
            try
            {
                var result = ctx.parts
               .Select(x => new partsGetDTO
               {
                   name = x.name,
                   serialNumber = x.serialNumber
               })
               .ToList();

                if(result==null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

           
        }

        // GET api/<partsController>/5
        [HttpGet("/api/parts/Filter")]
        public IActionResult Get([FromBody] partsFilterDTO value)
        {
            try
            {
                var result = ctx.parts
                    .Where(x => x.serialNumber == value.serialNumber)
                    .Select(x => new partsGetDTO
                    {
                        name = x.name,
                        serialNumber = x.serialNumber
                    })
                    .FirstOrDefault();

                if (result == null) return NotFound();
                return Ok(result);


            }
            catch (Exception ex) {

                return BadRequest();
            
            }
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

        [HttpPatch("{id}")]
        public IActionResult Patch(int id,[FromBody] partsPostDTO value)
        {

            try
            {
                var result = ctx.parts
                    .Where(x => x.pid == id)
                    .FirstOrDefault();

                if (result != null)
                {

                    if(value.serialNumber!=null) result.serialNumber=value.serialNumber;
                    if (value.name != null) result.name = value.name;

                    ctx.SaveChanges();

                    return Ok();


                }

                return NotFound();
            }
            catch (Exception ex) {

                return BadRequest(ex.Message);
            
            }

        }


        // DELETE api/<partsController>/5
        [HttpDelete]
        public IActionResult Delete([FromBody] partsFilterDTO value)
        {

            try
            {
                var result = ctx.parts
                .Where(x => x.serialNumber == value.serialNumber)
                .FirstOrDefault();

                if (result != null)
                {
                    ctx.parts.Remove(result);

                    ctx.SaveChanges();

                    return Ok();

                }

                return NotFound();

            }
            catch (Exception ex) {

                return BadRequest(ex.Message);
            }






        }
    }
}
