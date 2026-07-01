using System.ComponentModel.DataAnnotations;
using Lumen.Application.Cart;
using Lumen.Application.Customers;
using Lumen.Infrastructure.Identity;
using Lumen.Storefront.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lumen.Storefront.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ICustomerService _customerService;
    private readonly ICartService _cartService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ICustomerService customerService,
        ICartService cartService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _customerService = customerService;
        _cartService = cartService;
    }

    [HttpGet("/account/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost("/account/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is not null)
        {
            var customer = await _customerService.EnsureCustomerForUserAsync(
                user.Id,
                user.Email ?? model.Email,
                user.UserName,
                cancellationToken);

            await HttpContext.Session.LoadAsync(cancellationToken);
            var sessionKey = HttpCartContextProvider.GetSessionKeyFromHttpContext(HttpContext);
            if (!string.IsNullOrWhiteSpace(sessionKey))
            {
                await _cartService.MergeAnonymousCartOnLoginAsync(sessionKey, customer.Id, cancellationToken);
                HttpContext.Session.Remove(HttpCartContextProvider.SessionCartKey);
                await HttpContext.Session.CommitAsync(cancellationToken);
            }
        }

        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpGet("/account/register")]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new RegisterViewModel());
    }

    [HttpPost("/account/register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _customerService.EnsureCustomerForUserAsync(
            user.Id,
            user.Email!,
            model.DisplayName,
            cancellationToken);

        await _signInManager.SignInAsync(user, isPersistent: false);

        await HttpContext.Session.LoadAsync(cancellationToken);
        var sessionKey = HttpCartContextProvider.GetSessionKeyFromHttpContext(HttpContext);
        if (!string.IsNullOrWhiteSpace(sessionKey))
        {
            var customer = await _customerService.GetByUserIdAsync(user.Id, cancellationToken);
            if (customer is not null)
            {
                await _cartService.MergeAnonymousCartOnLoginAsync(sessionKey, customer.Id, cancellationToken);
                HttpContext.Session.Remove(HttpCartContextProvider.SessionCartKey);
                await HttpContext.Session.CommitAsync(cancellationToken);
            }
        }

        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpPost("/account/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect("/");
    }

    public sealed class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public sealed class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string? DisplayName { get; set; }

        [Required, StringLength(100, MinimumLength = 6), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password)), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}