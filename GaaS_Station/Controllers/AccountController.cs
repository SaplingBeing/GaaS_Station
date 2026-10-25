using GaaS_Station.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;

namespace GaaS_Station.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly HttpClient _client;
        public AccountController(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("GaaSApi");
        }
        public async Task<IActionResult> Index()
        {
            var respone = await _client.GetFromJsonAsync<List<AspNetUser>>("api/users");
            return View(respone);
        }
        public async Task<IActionResult> Details(string id)
        {
            var response = await _client.GetAsync($"api/users/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound();
            if (!response.IsSuccessStatusCode)
                return NotFound();
            return View(await response.Content.ReadFromJsonAsync<AspNetUser>());
        }
        public async Task<IActionResult> Edit(string id)
        {
            var response = await _client.GetAsync($"api/users/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound();
            if (!response.IsSuccessStatusCode)
                return NotFound();
            var user = await response.Content.ReadFromJsonAsync<AspNetUser>();
            if (user == null)
                return NotFound();
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(string id, AspNetUser user)
        {
            if (id != user.Id)
                return BadRequest();
            var response = await _client.PutAsJsonAsync($"api/users/{id}", user);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");
            return View(user);
        }
    }
}
