using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ASC.Web.Services;
using ASC.Web.Areas.Accounts.Models;
using ASC.Model.BaseTypes;
using ASC.Utilities;
namespace ASC.Web.Areas.Accounts.Controllers
{
    [Authorize]
    [Area("Accounts")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<IdentityUser> _signInManager;
        public AccountController(UserManager<IdentityUser> userManager, IEmailSender emailSender, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ServiceEngineers()
        {
            var serviceEngineers = await _userManager.GetUsersInRoleAsync(Roles.Engineer.ToString());
            HttpContext.Session.SetSession("ServiceEngineers", serviceEngineers);
            return View(new ServiceEngineerViewModel
            {
                ServiceEngineers = serviceEngineers == null ? null : serviceEngineers.ToList(),
                Registration = new ServiceEngineerRegistrationViewModelcs() { IsEdit = false },
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServiceEngineers(ServiceEngineerViewModel serviceEngineer)
        {
            serviceEngineer.ServiceEngineers = HttpContext.Session.GetSession<List<IdentityUser>>("ServiceEngineers");
            if (!ModelState.IsValid)
            {
                return View(serviceEngineer);
            }
            if (serviceEngineer.Registration.IsEdit)
            {
                // Update User
                var user = await _userManager.FindByEmailAsync(serviceEngineer.Registration.Email);
                user.UserName = serviceEngineer.Registration.UserName;
                IdentityResult result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    result.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                    return View(serviceEngineer);
                }
                // Update Password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                IdentityResult passwordResult = await _userManager.ResetPasswordAsync(user, token, serviceEngineer.Registration.Password);
                if (!passwordResult.Succeeded)
                {
                    passwordResult.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                    return View(serviceEngineer);
                }
                // Update claims
                user = await _userManager.FindByEmailAsync(serviceEngineer.Registration.Email);
                var identity = await _userManager.GetClaimsAsync(user);
                var isActiveClaim = identity.SingleOrDefault(p => p.Type == "IsActive");
                var removeClaimResult = await _userManager.RemoveClaimAsync(user, new System.Security.Claims.Claim(isActiveClaim.Type, isActiveClaim.Value));
                var addClaimResult = await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(isActiveClaim.Type, serviceEngineer.Registration.IsActive.ToString()));
            }
            else
            {
                // Create User
                IdentityUser user = new IdentityUser
                {
                    UserName = serviceEngineer.Registration.UserName,
                    Email = serviceEngineer.Registration.Email,
                    EmailConfirmed = true
                };

                IdentityResult result = await _userManager.CreateAsync(user, serviceEngineer.Registration.Password);
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", serviceEngineer.Registration.Email));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", serviceEngineer.Registration.IsActive.ToString()));

                if (!result.Succeeded)
                {
                    result.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                    return View(serviceEngineer);
                }

                // Assign user to Engineer Role
                var roleResult = await _userManager.AddToRoleAsync(user, Roles.Engineer.ToString());
                if (!roleResult.Succeeded)
                {
                    roleResult.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                    return View(serviceEngineer);
                }
            }

                if (serviceEngineer.Registration.IsActive)
                {
                    await _emailSender.SendEmailAsync(serviceEngineer.Registration.Email, "Account Created/Modified", $"Email: {serviceEngineer.Registration.Email} \n Password: {serviceEngineer.Registration.Password}");
                }
                else
                {
                    await _emailSender.SendEmailAsync(serviceEngineer.Registration.Email, "Account Deactivated", $"Your account has been deactivated.");
                }
            return RedirectToAction("ServiceEngineers");
        }
        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            var customers = await _userManager.GetUsersInRoleAsync(Roles.User.ToString());
            HttpContext.Session.SetSession("Customers", customers);
            return View(new CustomerViewModel
            {
                Customers = customers == null ? null : customers.ToList(),
                Registration = new CustomerRegistrationViewModel() { IsEdit = false }
            });
        }
        //xóa service engineer
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteServiceEngineer(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ServiceEngineers");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    TempData["Error"] = "Failed to delete the user.";
                }
            }

            return RedirectToAction("ServiceEngineers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Customers(CustomerViewModel customer)
        {
            customer.Customers = HttpContext.Session.GetSession<List<IdentityUser>>("Customers");
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            if (customer.Registration.IsEdit)
            {
                // Update User
                // Update claims IsActive
                var user = await _userManager.FindByEmailAsync(customer.Registration.Email);
                var identity = await _userManager.GetClaimsAsync(user);
                var isActiveClaim = identity.SingleOrDefault(p => p.Type == "IsActive");
                var removeClaimResult = await _userManager.RemoveClaimAsync(user, new System.Security.Claims.Claim(isActiveClaim.Type, isActiveClaim.Value));
                var addClaimResult = await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(isActiveClaim.Type, customer.Registration.IsActive.ToString()));

                if (customer.Registration.IsActive)
                {
                    await _emailSender.SendEmailAsync(customer.Registration.Email, "Account Modified", $"Your account has been activated, email : {customer.Registration.Email}");
                }
                else
                {
                    await _emailSender.SendEmailAsync(customer.Registration.Email, "Account Deactivated", $"Your account has been deactivated.");
                }
            }

            return RedirectToAction("Customers");
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var user = HttpContext.User.GetCurrentUserDetails();
            return View(new ProfileModel() { UserName = user.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileModel profile)
        {
            if (!ModelState.IsValid)
            {
                return View(profile);
            }
            var user = await _userManager.FindByEmailAsync(HttpContext.User.GetCurrentUserDetails().Email);
            user.UserName = profile.UserName;
            IdentityResult result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                return View(profile);
            }
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Dashboard", "Dashboard", new { area = "ServiceRequests" });
        }

    }
}
