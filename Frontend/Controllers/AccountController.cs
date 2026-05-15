using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AMS.Models;
using AMS.Services;
using AMS.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _api;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(ApiService api, IWebHostEnvironment webHostEnvironment)
        {
            _api = api;
            _webHostEnvironment = webHostEnvironment;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                var role = HttpContext.Session.GetString("Role");
                if (role == "Admin") return RedirectToAction("Dashboard", "Admin");
                if (role == "Teacher") return RedirectToAction("Dashboard", "Teacher");
                if (role == "Student") return RedirectToAction("Dashboard", "Student");
            }
            ViewBag.IsWakingUp = false;
            ViewBag.Error = "";
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Username and password are required";
                return View();
            }

            try
            {
                var result = await _api.LoginAsync(username, password);
                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    await HttpContext.SignOutAsync();
                    HttpContext.Session.SetString("JwtToken", result.Token);
                    HttpContext.Session.SetString("Role", result.Role);
                    if (!string.IsNullOrEmpty(result.RefreshToken))
                        HttpContext.Session.SetString("RefreshToken", result.RefreshToken);

                    if (result.Role == "Admin")
                    {
                        var displayName = result.FullName ?? result.Username;
                        HttpContext.Session.SetString("Username", displayName);
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, displayName),
                            new Claim(ClaimTypes.Role, "Admin")
                        };
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                    }
                    else if (result.Role == "Teacher")
                    {
                        int teacherId = result.TeacherId ?? 0;
                        string teacherDisplayName = result.FullName ?? result.Username;
                        string teacherNo = result.TeacherNo ?? result.Username;

                        HttpContext.Session.SetString("Username", teacherDisplayName);
                        HttpContext.Session.SetString("TeacherName", teacherDisplayName);
                        HttpContext.Session.SetString("TeacherId", teacherId.ToString());
                        HttpContext.Session.SetString("TeacherNo", teacherNo);

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, teacherDisplayName),
                            new Claim(ClaimTypes.Role, "Teacher")
                        };
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                    }
                    else if (result.Role == "Student")
                    {
                        var studentName = result.FullName ?? result.Username;
                        HttpContext.Session.SetString("Username", studentName);
                        HttpContext.Session.SetString("StudentName", studentName);
                        HttpContext.Session.SetString("StudentId", result.StudentId?.ToString() ?? "");
                        HttpContext.Session.SetString("StudentNo", result.StudentNo ?? "");
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, studentName),
                            new Claim(ClaimTypes.Role, "Student")
                        };
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                    }

                    // Load profile photo from backend after login
                    var profile = await _api.GetUserProfileAsync();
                    if (profile != null && !string.IsNullOrEmpty(profile.ProfilePhotoUrl))
                    {
                        var fullPhotoUrl = profile.ProfilePhotoUrl.StartsWith("http")
                            ? profile.ProfilePhotoUrl
                            : $"https://triotagapayo-ams-dbtc.onrender.com{profile.ProfilePhotoUrl}";
                        HttpContext.Session.SetString("ProfilePicture", fullPhotoUrl);
                    }

                    if (result.Role == "Admin") return RedirectToAction("Dashboard", "Admin");
                    if (result.Role == "Teacher") return RedirectToAction("Dashboard", "Teacher");
                    if (result.Role == "Student") return RedirectToAction("Dashboard", "Student");
                }

                ViewBag.Error = "Invalid username or password";
                return View();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Connection refused") || ex.Message.Contains("timed out") || ex.Message.Contains("502"))
                {
                    ViewBag.IsWakingUp = true;
                    ViewBag.Error = "Server is waking up from cold start. Please wait...";
                }
                else
                {
                    ViewBag.Error = $"Login failed: {ex.Message}";
                }
                return View();
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult StudentLogin() => View();

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StudentLogin(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Student ID and password are required";
                return View();
            }

            try
            {
                var result = await _api.LoginAsync(username, password);
                if (result != null && result.Role == "Student")
                {
                    await HttpContext.SignOutAsync();
                    HttpContext.Session.SetString("JwtToken", result.Token);
                    HttpContext.Session.SetString("Role", result.Role);
                    var studentName = result.FullName ?? result.Username;
                    HttpContext.Session.SetString("Username", studentName);
                    HttpContext.Session.SetString("StudentName", studentName);
                    HttpContext.Session.SetString("StudentId", result.StudentId?.ToString() ?? "");
                    HttpContext.Session.SetString("StudentNo", result.StudentNo ?? "");
                    if (!string.IsNullOrEmpty(result.RefreshToken))
                        HttpContext.Session.SetString("RefreshToken", result.RefreshToken);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, studentName),
                        new Claim(ClaimTypes.Role, "Student")
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    var profile = await _api.GetUserProfileAsync();
                    if (profile != null && !string.IsNullOrEmpty(profile.ProfilePhotoUrl))
                    {
                        var fullPhotoUrl = profile.ProfilePhotoUrl.StartsWith("http")
                            ? profile.ProfilePhotoUrl
                            : $"https://triotagapayo-ams-dbtc.onrender.com{profile.ProfilePhotoUrl}";
                        HttpContext.Session.SetString("ProfilePicture", fullPhotoUrl);
                    }

                    return RedirectToAction("Dashboard", "Student");
                }
                ViewBag.Error = "Invalid student credentials";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Login failed: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _api.LogoutAsync();
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            var profile = await _api.GetUserProfileAsync();
            if (profile != null && !string.IsNullOrEmpty(profile.ProfilePhotoUrl))
            {
                var fullPhotoUrl = profile.ProfilePhotoUrl.StartsWith("http")
                    ? profile.ProfilePhotoUrl
                    : $"https://triotagapayo-ams-dbtc.onrender.com{profile.ProfilePhotoUrl}";
                HttpContext.Session.SetString("ProfilePicture", fullPhotoUrl);
            }

            var role = HttpContext.Session.GetString("Role");
            var userName = HttpContext.Session.GetString("Username") ?? "User";
            var profilePhotoUrl = HttpContext.Session.GetString("ProfilePicture");
            var model = new ProfileViewModel
            {
                FullName = userName,
                Email = "",
                Role = role ?? "Admin",
                ProfilePhotoUrl = profilePhotoUrl
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([FromForm] string fullName, [FromForm] string email,
            [FromForm] string currentPassword, [FromForm] string newPassword, [FromForm] string confirmPassword)
        {
            try
            {
                if (!string.IsNullOrEmpty(newPassword) && newPassword != confirmPassword)
                    return Json(new { success = false, message = "New passwords do not match" });
                if (!string.IsNullOrEmpty(newPassword) && newPassword.Length < 6)
                    return Json(new { success = false, message = "Password must be at least 6 characters" });

                var result = await _api.PutAsync("/api/User/profile", new { fullName, email, currentPassword, newPassword });
                if (result.Success)
                {
                    if (!string.IsNullOrEmpty(fullName))
                        HttpContext.Session.SetString("Username", fullName);
                    return Json(new { success = true, message = "Profile updated successfully!" });
                }
                return Json(new { success = false, message = result.Error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfilePhoto(IFormFile profilePhoto)
        {
            try
            {
                if (profilePhoto == null || profilePhoto.Length == 0)
                    return Json(new { success = false, message = "No file selected" });

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(profilePhoto.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    return Json(new { success = false, message = "Only JPG, PNG, GIF, or WEBP images are allowed" });
                if (profilePhoto.Length > 5 * 1024 * 1024)
                    return Json(new { success = false, message = "File size must be less than 5MB" });

                using var memoryStream = new MemoryStream();
                await profilePhoto.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var (success, photoUrl, error) = await _api.UpdateProfilePhotoAsync(memoryStream, profilePhoto.FileName);

                string finalPhotoUrl = null;

                if (success && !string.IsNullOrEmpty(photoUrl))
                {
                    finalPhotoUrl = photoUrl.StartsWith("http")
                        ? photoUrl
                        : $"https://triotagapayo-ams-dbtc.onrender.com{photoUrl}";
                }
                else
                {
                    string localUploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                    if (!Directory.Exists(localUploadsFolder))
                        Directory.CreateDirectory(localUploadsFolder);

                    string uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    string localFilePath = Path.Combine(localUploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                    {
                        await profilePhoto.CopyToAsync(fileStream);
                    }
                    finalPhotoUrl = $"/uploads/profiles/{uniqueFileName}";
                }

                HttpContext.Session.SetString("ProfilePicture", finalPhotoUrl);
                return Json(new { success = true, message = "Profile photo updated!", photoUrl = finalPhotoUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}