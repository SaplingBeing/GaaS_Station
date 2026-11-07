using GaaS_Station.Data;
using GaaS_Station.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using X.PagedList.Extensions;

namespace GaaS_Station.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.FirstName.Contains(searchString.Trim()) || s.LastName.Contains(searchString.Trim()) || (s.FirstName + " " + s.LastName).Contains(searchString.Trim()));
            }
            var users = await query.ToListAsync();

            var viewModelList = new List<AdminVM>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                viewModelList.Add(new AdminVM
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    UserRoles = userRoles
                });
            }
            int pageSize = 6;
            int pageNumber = page ?? 1;
            var pagedList = viewModelList.ToPagedList(pageNumber, pageSize);
            return View(pagedList);
        }

        [HttpGet]
        public string IndexAJAX(string searchString)
        {
            searchString ??= string.Empty;

            string sql = "SELECT * FROM AspNetUsers WHERE FirstName LIKE @p0 OR LastName LIKE @p0";
            string wrapString = "%" + searchString + "%";

            List<ApplicationUser> users = _context.Users.FromSqlRaw(sql, wrapString).ToList();

            string json = JsonConvert.SerializeObject(users);
            return json;
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var viewModel = new AdminVM
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                AllRoles = allRoles!,
                UserRoles = userRoles
            };

            return View(viewModel);
        }

        // EDIT user
        [HttpPost]
        public async Task<IActionResult> Edit(AdminVM viewModel, List<string> selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(viewModel.Id);
            if (user == null) return NotFound();

            user.Email = viewModel.Email;
            user.UserName = viewModel.UserName;
            user.FirstName = viewModel.FirstName;
            user.LastName = viewModel.LastName;
            await _userManager.UpdateAsync(user);

            // Sync user roles
            var currentRoles = await _userManager.GetRolesAsync(user);

            selectedRoles ??= new List<string>();

            var rolesToAdd = selectedRoles.Except(currentRoles);
            var rolesToRemove = currentRoles.Except(selectedRoles);

            await _userManager.AddToRolesAsync(user, rolesToAdd);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            return RedirectToAction(nameof(Index));
        }

        // DELETE user
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                return RedirectToAction(nameof(Index));

            }
            return NotFound("User not found");
        }
    }
}