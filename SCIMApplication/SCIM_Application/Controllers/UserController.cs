using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCIM_Application.Data;
using SCIM_Application.Models;
using SCIM_Application.ViewModels;
using SCIM_Application.Services;
using Newtonsoft.Json;

namespace SCIM_Application.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<UserController> _logger;
        private readonly IScimService _scim;

        public UserController(ApplicationDbContext db, ILogger<UserController> logger, IScimService scim)
        {
            _db = db;
            _logger = logger;
            _scim = scim;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _db.Users
                .Include(u => u.UserApplications)
                .ThenInclude(ua => ua.Application)
                .OrderBy(u => u.UserName)
                .ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Create()
        {
            var apps = await _db.Applications.Where(a => a.IsActive).ToListAsync();
            var vm = new UserFormViewModel
            {
                AvailableApplications = apps.Select(a => new SelectableApplication{ Id = a.Id, Name = a.Name }).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AvailableApplications = (await _db.Applications.Where(a => a.IsActive).ToListAsync())
                    .Select(a => new SelectableApplication{ Id = a.Id, Name = a.Name, Selected = vm.SelectedApplicationIds.Contains(a.Id) }).ToList();
                return View(vm);
            }

            if (await _db.Users.AnyAsync(u => u.UserName == vm.UserName))
                ModelState.AddModelError(nameof(vm.UserName), "Bu kullanıcı adı zaten kullanılıyor.");
            if (await _db.Users.AnyAsync(u => u.Email == vm.Email))
                ModelState.AddModelError(nameof(vm.Email), "Bu e-posta zaten kullanılıyor.");

            if (!ModelState.IsValid)
            {
                vm.AvailableApplications = (await _db.Applications.Where(a => a.IsActive).ToListAsync())
                    .Select(a => new SelectableApplication{ Id = a.Id, Name = a.Name, Selected = vm.SelectedApplicationIds.Contains(a.Id) }).ToList();
                return View(vm);
            }

            var user = new User
            {
                UserName = vm.UserName,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                IsActive = vm.IsActive
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            if (vm.SelectedApplicationIds.Any())
            {
                foreach (var appId in vm.SelectedApplicationIds.Distinct())
                {
                    _db.UserApplications.Add(new UserApplication
                    {
                        UserId = user.Id,
                        ApplicationId = appId,
                        IsActive = true
                    });
                }
                await _db.SaveChangesAsync();

               
                var apps = await _db.Applications.Where(a => vm.SelectedApplicationIds.Contains(a.Id)).ToListAsync();
                foreach (var app in apps)
                {
                    await _scim.CreateUserAsync(user, app); 
                }

                var updatedUser = await _db.Users.FindAsync(user.Id);
                if (updatedUser != null)
                {
                    user.ScimId = updatedUser.ScimId;
                    user.ExternalId = updatedUser.ExternalId;
                }
            }

        
            _db.ScimLogs.Add(new ScimLog
            {
                UserId = user.Id,
                ApplicationId = null,
                Operation = "UserCreate",
                Status = "Success",
                RequestData = JsonConvert.SerializeObject(new
                {
                    user.UserName,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    SelectedApplications = vm.SelectedApplicationIds
                }),
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Kullanıcı oluşturuldu";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _db.Users.Include(u => u.UserApplications).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var activeAppIds = user.UserApplications.Where(ua => ua.IsActive).Select(ua => ua.ApplicationId).ToHashSet();
            var apps = await _db.Applications.Where(a => a.IsActive).ToListAsync();

            var vm = new UserFormViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                SelectedApplicationIds = activeAppIds.ToList(),
                AvailableApplications = apps.Select(a => new SelectableApplication
                {
                    Id = a.Id,
                    Name = a.Name,
                    Selected = activeAppIds.Contains(a.Id)
                }).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserFormViewModel vm)
        {
            if (id != vm.Id) return NotFound();
            var user = await _db.Users.Include(u => u.UserApplications).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.AvailableApplications = (await _db.Applications.Where(a => a.IsActive).ToListAsync())
                    .Select(a => new SelectableApplication{ Id = a.Id, Name = a.Name, Selected = vm.SelectedApplicationIds.Contains(a.Id) }).ToList();
                return View(vm);
            }

            if (await _db.Users.AnyAsync(u => u.UserName == vm.UserName && u.Id != id))
                ModelState.AddModelError(nameof(vm.UserName), "Bu kullanıcı adı zaten kullanılıyor.");
            if (await _db.Users.AnyAsync(u => u.Email == vm.Email && u.Id != id))
                ModelState.AddModelError(nameof(vm.Email), "Bu e-posta zaten kullanılıyor.");

            if (!ModelState.IsValid)
            {
                vm.AvailableApplications = (await _db.Applications.Where(a => a.IsActive).ToListAsync())
                    .Select(a => new SelectableApplication{ Id = a.Id, Name = a.Name, Selected = vm.SelectedApplicationIds.Contains(a.Id) }).ToList();
                return View(vm);
            }

            user.UserName = vm.UserName;
            user.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.Email = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;
            user.IsActive = vm.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            var current = user.UserApplications.Where(ua => ua.IsActive).Select(ua => ua.ApplicationId).ToHashSet();
            var desired = vm.SelectedApplicationIds.Distinct().ToHashSet();

            var toRemove = current.Except(desired).ToList();
            foreach (var appId in toRemove)
            {
                var link = user.UserApplications.First(ua => ua.ApplicationId == appId && ua.IsActive);
                link.IsActive = false;
                link.UpdatedAt = DateTime.UtcNow;
            }

            var toAdd = desired.Except(current).ToList();
            foreach (var appId in toAdd)
            {
                _db.UserApplications.Add(new UserApplication
                {
                    UserId = user.Id,
                    ApplicationId = appId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            
            var appsForUpdate = await _db.Applications.Where(a => desired.Contains(a.Id)).ToListAsync();
            foreach (var app in appsForUpdate)
            {
                await _scim.UpdateUserAsync(user, app); 
            }

          
            var updatedUser = await _db.Users.FindAsync(user.Id);
            if (updatedUser != null)
            {
                user.ScimId = updatedUser.ScimId;
                user.ExternalId = updatedUser.ExternalId;
            }

           
            _db.ScimLogs.Add(new ScimLog
            {
                UserId = user.Id,
                ApplicationId = null,
                Operation = "UserUpdate",
                Status = "Success",
                RequestData = JsonConvert.SerializeObject(new
                {
                    user.UserName,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    SelectedApplications = vm.SelectedApplicationIds
                }),
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Kullanıcı güncellendi";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            // SCIM tarafında da sil
            var apps = await _db.UserApplications.Where(ua => ua.UserId == id && ua.IsActive).Select(ua => ua.Application).ToListAsync();
            foreach (var app in apps)
            {
                _ = _scim.DeleteUserAsync(user, app);
            }

            
            _db.ScimLogs.Add(new ScimLog
            {
                UserId = user.Id, 
                ApplicationId = null,
                Operation = "UserDelete",
                Status = "Success",
                RequestData = JsonConvert.SerializeObject(new { user.UserName, user.Email }),
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            // User'ı sil
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Kullanıcı silindi";
            return RedirectToAction(nameof(Index));
        }
    }
}
