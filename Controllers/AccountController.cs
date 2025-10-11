using Ade_Farming.Models;
using Ade_Farming.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Ade_Farming.Services;
//using Microsoft.AspNetCore.Identity.UI.Services;


namespace Ade_Farming.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;


        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;

        }

        // GET: Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    IsSeller = model.IsSeller,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    Address = model.Address,
                    Contact = model.Contact,
                    Gender = model.Gender,
                    Question1 = model.Question1,
                    Question2 = model.Question2
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }


        // GET: Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("/Account/Login"))
                returnUrl = null;

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user);

                        if (user.IsSeller)
                        {
                            var identity = (System.Security.Claims.ClaimsIdentity)claimsPrincipal.Identity;
                            if (!identity.HasClaim(c => c.Type == "IsSeller"))
                                identity.AddClaim(new System.Security.Claims.Claim("IsSeller", "True"));
                        }

                        await _signInManager.SignInAsync(user, model.RememberMe);

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);

                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }

            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        
        [HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ForgotPassword(
    ForgotPasswordViewModel model, 
    [FromServices] IEmailSender emailSender,
    [FromServices] ILogger<AccountController> logger)
{
    if (!ModelState.IsValid)
        return View(model);

    logger.LogInformation("ForgotPassword called for: {Email}", model.Email);

    var user = await _userManager.FindByEmailAsync(model.Email);
    if (user == null)
    {
        logger.LogWarning("User not found for email: {Email}", model.Email);
        TempData["Message"] = "If the email exists, a password reset link has been sent.";
        return RedirectToAction("ForgotPasswordConfirmation");
    }

    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
    logger.LogInformation("Reset token generated for user: {Email}", model.Email);

    var resetLink = Url.Action("ResetPassword", "Account", new { email = user.Email, token }, Request.Scheme);

    string emailBody = $"<p>Hi {user.FullName},</p>" +
                       $"<p>You requested to reset your password. Click below to reset it:</p>" +
                       $"<p><a href='{resetLink}'>Reset Password</a></p>" +
                       "<p>If you did not request this, ignore this email.</p>";

    try
    {
        await emailSender.SendEmailAsync(user.Email, "Password Reset - Ade Farming", emailBody);
        logger.LogInformation("Password reset email sent successfully to {Email}", user.Email);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error sending password reset email to {Email}", user.Email);
        TempData["ErrorMessage"] = "An error occurred while sending the reset email.";
        return View(model);
    }

    return RedirectToAction("ForgotPasswordConfirmation");
}
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return BadRequest("A token and email must be supplied.");

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction("ResetPasswordConfirmation");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public IActionResult AccessDenied() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
