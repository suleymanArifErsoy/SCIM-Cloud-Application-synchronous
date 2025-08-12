using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCIM_Application.Data;
using SCIM_Application.Models;
using Newtonsoft.Json;

namespace SCIM_Application.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ApplicationController> _logger;

        public ApplicationController(ApplicationDbContext db, ILogger<ApplicationController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var apps = await _db.Applications.OrderBy(a => a.Name).ToListAsync();
            return View(apps);
        }

        public IActionResult Create()
        {
            return View(new Application());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Application model)
        {
            if (!ModelState.IsValid) return View(model);
            if (await _db.Applications.AnyAsync(a => a.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "Bu uygulama adı zaten kullanılıyor.");
                return View(model);
            }
            model.CreatedAt = DateTime.UtcNow;
            _db.Applications.Add(model);
            await _db.SaveChangesAsync();

            // Log: AppCreate
            _db.ScimLogs.Add(new ScimLog
            {
                UserId = null,
                ApplicationId = model.Id,
                Operation = "AppCreate",
                Status = "Success",
                RequestData = JsonConvert.SerializeObject(new { model.Name, model.Provider, model.ScimEndpoint }),
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Uygulama oluşturuldu";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var app = await _db.Applications.FindAsync(id);
            if (app == null) return NotFound();
            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Application model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            if (await _db.Applications.AnyAsync(a => a.Name == model.Name && a.Id != id))
            {
                ModelState.AddModelError(nameof(model.Name), "Bu uygulama adı zaten kullanılıyor.");
                return View(model);
            }

            var app = await _db.Applications.FindAsync(id);
            if (app == null) return NotFound();

            app.Name = model.Name;
            app.Description = model.Description;
            app.Provider = model.Provider;
            app.ScimEndpoint = model.ScimEndpoint;
            app.ApiKey = model.ApiKey;
            app.BearerToken = model.BearerToken;
            app.IsActive = model.IsActive;
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Log: AppUpdate
            _db.ScimLogs.Add(new ScimLog
            {
                UserId = null,
                ApplicationId = app.Id,
                Operation = "AppUpdate",
                Status = "Success",
                RequestData = JsonConvert.SerializeObject(new { app.Name, app.Provider, app.ScimEndpoint }),
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Uygulama güncellendi";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var app = await _db.Applications.FindAsync(id);
            if (app == null) return NotFound();
            return View(app);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var app = await _db.Applications.FindAsync(id);
            if (app == null) return NotFound();

            var hasActiveLinks = await _db.UserApplications.AnyAsync(ua => ua.ApplicationId == id && ua.IsActive);
            if (hasActiveLinks)
            {
                TempData["Error"] = "Bu uygulamaya bağlı aktif kullanıcılar var. Önce bağlantıları kaldırın.";
                return RedirectToAction(nameof(Index));
            }

            
            _db.ScimLogs.Add(new ScimLog
            {
                UserId = null,
                ApplicationId = app.Id, 
                Operation = "AppDelete",
                Status = "Success",
                RequestData = JsonConvert.SerializeObject(new { app.Name, app.Provider }),
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            // Application'ı sil
            _db.Applications.Remove(app);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Uygulama silindi";
            return RedirectToAction(nameof(Index));
        }
    }
}
