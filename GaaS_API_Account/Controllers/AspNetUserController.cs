using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GaaS_API_Account.Models;

[Route("api/[controller]")]
[ApiController]
public class AspNetUserController : ControllerBase
{
    private readonly FS2026_Group1_ProjectContext _context;
    public AspNetUserController(FS2026_Group1_ProjectContext context)
    {
        _context = context;
    }

    // GET: api/AspNetUser
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AspNetUser>>> GetAspNetUser()
    {
        return await _context.AspNetUsers.Include(x => x.Roles).ToListAsync();
    }

    // GET: api/AspNetUser/5
    [HttpGet("{id}")]
    public async Task<ActionResult<AspNetUser>> GetAspNetUser(string id)
    {
        var aspnetuser = await _context.AspNetUsers.Include(x => x.Roles).FirstOrDefaultAsync(u => u.Id == id);

        if (aspnetuser == null)
        {
            return NotFound();
        }

        return aspnetuser;
    }

    // PUT: api/AspNetUser/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAspNetUser(string? id, AspNetUser aspnetuser)
    {
        if (id != aspnetuser.Id)
        {
            return BadRequest();
        }

        _context.Entry(aspnetuser).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AspNetUserExists(id))
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

    // POST: api/AspNetUser
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<AspNetUser>> PostAspNetUser(AspNetUser aspnetuser)
    {
        _context.AspNetUsers.Add(aspnetuser);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetAspNetUser", new { id = aspnetuser.Id }, aspnetuser);
    }

    // DELETE: api/AspNetUser/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAspNetUser(string? id)
    {
        var aspnetuser = await _context.AspNetUsers.FindAsync(id);
        if (aspnetuser == null)
        {
            return NotFound();
        }

        _context.AspNetUsers.Remove(aspnetuser);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool AspNetUserExists(string? id)
    {
        return _context.AspNetUsers.Any(e => e.Id == id);
    }
}
