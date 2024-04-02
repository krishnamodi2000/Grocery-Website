using Besos.Data;
using Besos.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Besos.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase //why we use controller base?
    {
        private static AppDbContext _context;
        public UserController(AppDbContext context) 
        {
            _context = context;
        
        }

        //why we use async and await ?
        [HttpGet]
        public async Task<IActionResult> Get() {

            var users=await  _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id:int}")]

        public  async Task<IActionResult> Get(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id==id);
            if (user == null)
                return BadRequest("Invalid Id");
            return Ok(user);

        }

        [HttpPost]
        public async Task<IActionResult> Post(User user)
        {
           await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

           return CreatedAtAction("Get", user.Id, user);
        }

        //put is used for entire object update, patch is used for a single update
        [HttpPatch]
        public async Task<IActionResult> Patch(int id, string country)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return BadRequest("Invalid Id");

            user.Country = country;

            await _context.SaveChangesAsync();


            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return BadRequest("Invalid Id");

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
