using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sertec.Data;
using Sertec.Models;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{


    public class reportGetDTO
    {

        public int reportId { get; set; }
        public string question { get; set; }
        public string report { get; set; }
        public DateTime created { get; set; }
        public string user { get; set; }
        public string explanation { get; set; }

    }

    public class reportFilteredDTO
    {
        public string report { get; set; }
        public DateTime created { get; set; }
        public string user { get; set; }
        public string explanation { get; set; }

    }


    public class reportPostDTO
    {
        public int questionId { get; set; }
        public short report { get; set; }
        public int userId { get; set; }
        public string explanation { get; set; }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {



        private readonly Appdbcontext ctx;
        public ReportsController(Appdbcontext context)
        {
            ctx = context;
        }


        // GET: api/<ReportsController>
        [HttpGet]
        public IActionResult Get()
        {

            try
            {
                var result = ctx.reports
                .Include(x=>x.users)
                .Include(x=>x.questions)
                .AsEnumerable()
                .Select(x => new reportGetDTO
                {
                    reportId = x.reportId,
                    question = x.questions.question,
                    report = x.report switch
                    {
                        0 => "No",
                        1 => "Yes",
                        2 => "Both",
                        3 => "Not needed",
                        _ => "Unknown"
                    },
                    created = x.reportCreated,
                    user = x.users.Username,
                    explanation = x.explanation


                })
                .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return NoContent();
            }



        }

        // GET api/<ReportsController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {

                var result = ctx.reports
                .Include(x => x.users)
                .Include(x => x.questions)
                .AsEnumerable()
               .Where(x => x.qId == id)
               .Select(x => new reportFilteredDTO
               {
                   report = x.report switch
                   {
                       0 => "No",
                       1 => "Yes",
                       2 => "Both",
                       3 => "Not needed",
                       _ => "Unknown"
                   },
                   created = x.reportCreated,
                   user = x.users.Username,
                   explanation = x.explanation


               })
               .ToList();

                return Ok(result);


            }
            catch (Exception ex)
            {
                return NoContent();
            }


           


        }

        // POST api/<ReportsController>
        [HttpPost]
        public IActionResult Post([FromBody] reportPostDTO value)
        {

            try
            {
                ctx.reports.Add(new Reports
                {
                    qId = value.questionId,
                    report = value.report,
                    uId = value.userId,
                    reportCreated = DateTime.Now,
                    explanation= value.explanation
                });


                ctx.SaveChanges();

                return Created();

            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
        // PUT api/<ReportsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ReportsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
