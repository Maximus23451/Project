using Microsoft.AspNetCore.Mvc;
using SertecDashboard.Api.Data;
using SertecDashboard.Api.Services;
using System.Reflection.Metadata.Ecma335;


namespace SertecDashboard.Api.Controllers
{
    public class partPostDTO
    {
        public string Name { get; set; }
        public string serialNumber { get; set; }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class PartsController : ControllerBase
    {


        private readonly AppDbContext _db;
        private readonly SseService _sse;

        public PartsController(AppDbContext db, SseService sse)
        {
            _db = db;
            _sse = sse;
        }



        // GET: api/<PartsController>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(_db.Parts.ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Alkatrészek lekérése sikertelen", error = ex.Message });
            }
        }
        // GET api/<PartsController>/5
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            try
            {
                var part = _db.Parts
                    .Where(x=>x.serialNumber==id);
                if (part == null)
                {
                    return NotFound(new { message = "Alkatrész nem található" });
                }
                return Ok(part);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Alkatrész lekérése sikertelen", error = ex.Message });
            }
        }

        // POST api/<PartsController>
        [HttpPost]
        public IActionResult Post([FromBody] partPostDTO value)
        {

            try
            {

                var result = _db.Parts
                    .Where(x => x.serialNumber == value.serialNumber)
                    .FirstOrDefault();

                if (result == null)
                {

                    _db.Parts.Add(new Models.Parts
                    {
                        Name = value.Name,
                        serialNumber = value.serialNumber
                    });
                    _db.SaveChanges();

                    return Ok(new { message = "Alkatrész sikeresen hozzáadva" });

                }

                return BadRequest(new { message = "Ugyanazzal a sorozatszámmal rendelkező alkatrész már létezik" });

            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = "Alkatrész hozzáadása sikertelen", error = ex.Message });
            }


        }

        // PUT api/<PartsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PartsController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {

                var machineparts = _db.MachineParts
                    .Where(x => x.PartId == id)
                    .ToList();

                foreach (var item in machineparts)
                {
                    _db.MachineParts.Remove(item);
                }
                _db.SaveChanges();

                var part = _db.Parts.Find(id);
                if (part == null)
                {
                    return NotFound(new { message = "Alkatrész nem található" });
                }
                _db.Parts.Remove(part);
                _db.SaveChanges();

                return Ok(new { message = "Alkatrész sikeresen törölve" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Alkatrész törlése sikertelen", error = ex.Message });
            }


        }
    }
}
