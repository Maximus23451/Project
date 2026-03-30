using Microsoft.AspNetCore.Mvc;
using Sertec.Data;
using Sertec.Models;
using Sertec.UserManager;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Sertec.Controllers
{

    public class userGetDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public int roleid { get; set; }
        public DateTime? lastPwChange { get; set; }
        public string displayName { get; set; }

    }

    public class userPostDTO
    {
        public string username { get; set; }
        public string password { get; set; }
        public string role { get; set; }
        public string? rfid { get; set; }
        public string email { get; set; }
        public string displayName { get; set; }
    }


    public class userPatchDTO
    {
        public string? username { get; set; }
        public string? password { get; set; }
        public string? role { get; set; }
        public string? email { get; set; }
        public string? displayName { get; set; }
    }

    public class userLogin
    {
        public string? userName { get; set; }
        public string? password { get; set; }
        public string? rfid { get; set; }

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
            try
            {
                var result = ctx.users
                .Select(x => new userGetDTO
                {
                    id = x.uid,
                    name = x.Username,
                    email = x.Email,
                    roleid = x.roleid,
                    lastPwChange = x.LastPwChange,
                    displayName = x.displayName
                });


                return Ok(result);
            }
            catch (Exception ex)
            {
                return NoContent();
            }
            
        }

        // GET api/<ValuesController>/5
        [Route("/api/users/login")]
        [HttpPost]
        public IActionResult Post([FromBody] userLogin value)
        {

            try
            {
                if (value.userName != null)
                {

                    var user = ctx.users
                        .Where(x => x.Username == value.userName)
                        .FirstOrDefault();

                    var pw = PasswordManager.VerifyPasswordHash(value.password, user.PasswordHash, user.PasswordSalt);

                    if (pw) return Ok(user);
                    else return Unauthorized("Invalid password");

                }

                if (value.rfid != null)
                {

                    var user = ctx.users
                        .Where(x => x.rfid == value.rfid && x.Username == value.userName)
                        .FirstOrDefault();

                    if (user != null) return Ok(user);
                    else return Unauthorized("Invalid password");


                }

                return BadRequest("Username or rfid must be provided");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<ValuesController>
        [HttpPost]
        public IActionResult Post([FromBody] userPostDTO value)
        {
            try
            {
                var user = ctx.users
                .Where(x => x.Username == value.username)
                .FirstOrDefault();

                if (user != null)
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
                        roleid = role.Rid,
                        LastPwChange = DateTime.Today,
                        displayName = value.displayName

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

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }


        [HttpPatch("{id}")]
        public IActionResult Patch(string uname, userPatchDTO value)
        {
            try
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
                        user.LastPwChange = DateTime.Today;
                    }
                    if (value.role != null)
                    {
                        var r = ctx.roles
                            .Where(x => x.Name == value.role)
                            .Select(x => x.Rid)
                            .FirstOrDefault();

                        user.roleid = r;

                    }

                    if (value.displayName != null)
                    {
                        user.displayName= value.displayName;
                    }
                    ctx.SaveChanges();

                    return Ok("User updated successfully");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            



        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {





        }
    }
}
