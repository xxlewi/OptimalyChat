using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OptimalyChat.DataLayer.Entities;
using OptimalyChat.ServiceLayer.Interfaces;
using OptimalyChat.PresentationLayer.ViewModels;

namespace OptimalyChat.PresentationLayer.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<User> userManager, 
        SignInManager<User> signInManager,
        IUserService userService,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        var model = new LoginViewModel { ReturnUrl = returnUrl };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            _logger.LogInformation("Attempting login for user: {Email}", model.Email);
            
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                model.RememberMe, 
                lockoutOnFailure: false);

            _logger.LogInformation("Login result for {Email}: Succeeded={Succeeded}, IsLockedOut={IsLockedOut}, RequiresTwoFactor={RequiresTwoFactor}", 
                model.Email, result.Succeeded, result.IsLockedOut, result.RequiresTwoFactor);

            if (result.Succeeded)
            {
                // Update last login through service layer
                try
                {
                    await _userService.UpdateLastLoginAsync(model.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update last login for user {Email}", model.Email);
                    // Don't fail login if last login update fails
                }
                
                _logger.LogInformation("User {Email} successfully logged in", model.Email);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning("Failed login attempt for user: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Neplatné přihlašovací údaje.");
        }
        else
        {
            _logger.LogWarning("ModelState is invalid for login attempt: {Email}", model.Email);
            foreach (var error in ModelState)
            {
                _logger.LogWarning("ModelState error for {Key}: {Errors}", error.Key, string.Join(", ", error.Value?.Errors?.Select(e => e.ErrorMessage) ?? new string[0]));
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Custom validation for terms agreement
        if (!model.AgreeToTerms)
        {
            ModelState.AddModelError(nameof(model.AgreeToTerms), "Musíte souhlasit s podmínkami");
        }

        if (ModelState.IsValid)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName ?? string.Empty,
                LastName = model.LastName ?? string.Empty,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("New user {Email} registered successfully", user.Email);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var model = new ProfileViewModel
        {
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}