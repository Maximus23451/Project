using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using Sertec.Models;
using Sertec.UserManager;
using System.Diagnostics.CodeAnalysis;
using System.Net;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sertec.Controllers
{

    public class userGetDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public int roleid { get; set; }

    }

    public class userPostDTO
    {
        public string username { get; set; }
        public string password { get; set; }
        public string role { get; set; }
        public string? rfid { get; set; }
        public string email { get; set; }

    }


    public class userPatchDTO
    {
        public string? username { get; set; }
        public string? password { get; set; }
        public string? role { get; set; }
        public string? email { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class usersController : ControllerBase
    {
        private readonly Appdbcontext ctx;
        public usersController(Appdbcontext context)
        {
            ctx = context;
        }


        // GET: api/<ValuesController>
        [HttpGet]
        public IActionResult Get()
        {
            var result = ctx.users
                .Select(x => new userGetDTO
                {
                    id = x.uid,
                    name = x.Username,
                    email = x.Email,
                    roleid = x.roleid
                });


            return Ok(result);
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ValuesController>
        [HttpPost]
        public IActionResult Post([FromBody] userPostDTO value)
        {
            var user = ctx.users
                .Where(x => x.Username == value.username)
                .FirstOrDefault();

            if (user!=null)
            {
                return Conflict("Username already exists");
            }
            else
            {
                var role = ctx.roles
                    .Where(x => x.Name == value.role)
                    .FirstOrDefault();

                PasswordManager.CreatePasswordHash(value.password, out byte[] passwordHash, out byte[] passwordSalt);
                ctx.users.Add(new Users
                {
                    Username = value.username,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Email = value.email,
                    rfid = value.rfid,
                    roleid = role.Rid

                });


                ctx.SaveChanges();
                return Created();
            }




        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }


        [HttpPatch("{id}")]
        public IActionResult Patch(string uname, userPatchDTO value)
        {

            var user = ctx.users
                .Where(x => x.Username == uname)
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound("User not found");
            }
            else
            {
                if (value.username != null) user.Username = value.username;
                
                if (value.email != null) user.Email = value.email;
                if (value.password != null)
                {
                    PasswordManager.CreatePasswordHash(value.password, out byte[] passwordHash, out byte[] passwordSalt);

                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                }
                if (value.role != null)
                {
                    var r = ctx.roles
                        .Where(x => x.Name == value.role)
                        .Select(x => x.Rid)
                        .FirstOrDefault();

                    user.roleid = r;

                }
                ctx.SaveChanges();

                return Ok("User updated successfully");
            }



        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {





        }
    }
}
