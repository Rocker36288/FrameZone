using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AAContext _context;

        public AccountController(AAContext context)
        {
            _context = context;
        }

        // GET: api/Account
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserSession>>> GetUserSessions()
        {
            return _context.UserSessions;
        }

        // GET: api/Account/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserSession>> GetUserSession(long id)
        {
            var userSession = await _context.UserSessions.FindAsync(id);

            if (userSession == null)
            {
                return NotFound();
            }

            return userSession;
        }

        // PUT: api/Account/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserSession(long id, UserSession userSession)
        {
            if (id != userSession.SessionId)
            {
                return BadRequest();
            }

            _context.Entry(userSession).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserSessionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Account
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserSession>> PostUserSession(UserSession userSession)
        {
            _context.UserSessions.Add(userSession);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserSession", new { id = userSession.SessionId }, userSession);
        }

        // DELETE: api/Account/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserSession(long id)
        {
            var userSession = await _context.UserSessions.FindAsync(id);
            if (userSession == null)
            {
                return NotFound();
            }

            _context.UserSessions.Remove(userSession);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserSessionExists(long id)
        {
            return _context.UserSessions.Any(e => e.SessionId == id);
        }
    }
}
