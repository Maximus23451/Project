using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using Sertec.Models;
using System.Net;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{


    public class questionPostDTO
    {
        public string question { get; set; }
        public string serialNumber { get; set; }
        public string role { get; set; }
    }
    public class questionGetDTO
    {
        public string question { get; set; }
        public string role { get; set; }
        public string part { get; set; }
    }




    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {

        private readonly Appdbcontext ctx;
        public QuestionsController(Appdbcontext context)
        {
            ctx = context;
        }


        // GET: api/<QuestionsController>
        [HttpGet]
        public IActionResult Get()
        {

            var questions = ctx.questions
                .Select(x => new questionGetDTO
                {
                    question = x.question,
                    role = x.roles.Name,
                    part = x.parts.name
                })
                .ToList();

            return Ok(questions);
        }

        // GET api/<QuestionsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<QuestionsController>
        [HttpPost]
        public IActionResult Post([FromBody] questionPostDTO value)
        {
            var role = ctx.roles
                .Where(x => x.Name == value.role)
                .Select(x => x.Rid)
                .FirstOrDefault();

            var part = ctx.parts
                .Where(x=>x.serialNumber==value.serialNumber)
                .Select(x=>x.pid)
                .FirstOrDefault();

            ctx.questions.Add(new Questions
            {
                question = value.question,
                partsId = part,
                roleId = role
            });

            ctx.SaveChanges();

            return Created();



        }

        // PUT api/<QuestionsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<QuestionsController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var question=ctx.questions
                .Where(x=>x.qid==id)
                .FirstOrDefault();

            if (question != null)
            {
                ctx.questions.Remove(question);
                ctx.SaveChanges();

                return Ok();
            }
            else
            {
                return NotFound();
            }



        }
    }
}
