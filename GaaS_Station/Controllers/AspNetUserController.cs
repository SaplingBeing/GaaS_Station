
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GaaS_Station.Models;

public class AspNetUserController : Controller
{
    private readonly FS2026_Group1_ProjectContext _context;

    public AspNetUserController(FS2026_Group1_ProjectContext context)
    {
        _context = context;
    }

    // GET: ASPNETUSERS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.AspNetUsers.ToListAsync());
    }

    // GET: ASPNETUSERS/Details/5
    public async Task<IActionResult> Details(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var aspnetuser = await _context.AspNetUsers
            .FirstOrDefaultAsync(m => m.Id == id);
        if (aspnetuser == null)
        {
            return NotFound();
        }

        return View(aspnetuser);
    }

    // GET: ASPNETUSERS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ASPNETUSERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount,FirstName,LastName,AspNetUserClaims,AspNetUserLogins,AspNetUserTokens,Roles")] AspNetUser aspnetuser)
    {
        if (ModelState.IsValid)
        {
            _context.Add(aspnetuser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(aspnetuser);
    }

    // GET: ASPNETUSERS/Edit/5
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var aspnetuser = await _context.AspNetUsers.FindAsync(id);
        if (aspnetuser == null)
        {
            return NotFound();
        }
        return View(aspnetuser);
    }

    // POST: ASPNETUSERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string? id, [Bind("Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount,FirstName,LastName,AspNetUserClaims,AspNetUserLogins,AspNetUserTokens,Roles")] AspNetUser aspnetuser)
    {
        if (id != aspnetuser.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(aspnetuser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AspNetUserExists(aspnetuser.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(aspnetuser);
    }

    // GET: ASPNETUSERS/Delete/5
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var aspnetuser = await _context.AspNetUsers
            .FirstOrDefaultAsync(m => m.Id == id);
        if (aspnetuser == null)
        {
            return NotFound();
        }

        return View(aspnetuser);
    }

    // POST: ASPNETUSERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string? id)
    {
        var aspnetuser = await _context.AspNetUsers.FindAsync(id);
        if (aspnetuser != null)
        {
            _context.AspNetUsers.Remove(aspnetuser);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AspNetUserExists(string? id)
    {
        return _context.AspNetUsers.Any(e => e.Id == id);
    }
}
