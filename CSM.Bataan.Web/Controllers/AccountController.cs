using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using CSM.Bataan.Web.Infrastructure.Data.Enums;
using CSM.Bataan.Web.Infrastructure.Data.Helpers;
using CSM.Bataan.Web.Infrastructure.Data.Models;
using CSM.Bataan.Web.Infrastructure.Security;
using CSM.Bataan.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CSM.Bataan.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly DefaultDbContext _context;
        protected readonly IConfiguration _config;
        private string emailUserName;
        private string emailPassword;

        public AccountController(DefaultDbContext context, IConfiguration iConfiguration)
        {
            _context = context;
            this._config = iConfiguration;
            var emailConfig = this._config.GetSection("Email");
            emailUserName = (emailConfig["Username"]).ToString();
            emailPassword = (emailConfig["Password"]).ToString();
        }

        [HttpGet, Route("account/register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost, Route("account/register")]
        public IActionResult Register(RegisterViewModel model)
        {
            if(model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Password and Confirmation does not match.");
                return View();
            }

            var duplicate = this._context.Users.FirstOrDefault(u => u.EmailAddress.ToLower() == model.EmailAddress.ToLower());

            if(duplicate == null)
            {
                var registrationCode = RandomString(6);

                User user = new User()
                {
                    Id = Guid.NewGuid(),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress.ToLower(),
                    Gender = model.Gender,
                    LoginStatus = Infrastructure.Data.Enums.LoginStatus.NewRegister,
                    RegistrationCode = registrationCode,
                    Password = BCrypt.BCryptHelper.HashPassword(model.Password, BCrypt.BCryptHelper.GenerateSalt(8))
                };

                this._context.Users.Add(user);
                this._context.SaveChanges();

                this.SendNow(
                            "Hi " + model.FirstName + " " + model.LastName + @",
                             Welcome to CSM Bataan Website. Please use the following registration code to activate your account: " + registrationCode + @".
                             Regards,
                             CSM Bataan Website",
                            model.EmailAddress,
                            model.FirstName + " " + model.LastName,
                            "Welcome to CSM Bataan Website!!!"
                );
            }


            return View();

        }

        [HttpGet, Route("account/verify")]
        public IActionResult Verify()
        {
            return View();
        }

        [HttpPost, Route("account/verify")]
        public IActionResult Verify(VerifyViewModel model)
        {
            var user = this._context.Users.FirstOrDefault(u => u.EmailAddress.ToLower() == model.EmailAddress.ToLower() && u.RegistrationCode == model.RegistrationCode);

            if(user != null)
            {
                user.LoginStatus = Infrastructure.Data.Enums.LoginStatus.Active;
                user.LoginTrials = 0;
                this._context.Users.Update(user);
                this._context.SaveChanges();

                return RedirectToAction("login");
            }

            return View();
        }

        [HttpGet, Route("account/login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost, Route("account/login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var user = this._context.Users.FirstOrDefault(u => 
                u.EmailAddress.ToLower() == model.EmailAddress.ToLower());

            if (user != null)
            {
                if (BCrypt.BCryptHelper.CheckPassword(model.Password, user.Password))
                {
                    if (user.LoginStatus == Infrastructure.Data.Enums.LoginStatus.Locked)
                    {
                        ModelState.AddModelError("", "Your account has been locked please contact an Administrator.");
                        return View();
                    }
                    else if (user.LoginStatus == Infrastructure.Data.Enums.LoginStatus.NewRegister)
                    {
                        ModelState.AddModelError("", "Please verify your account first.");
                        return View();
                    }
                    else if (user.LoginStatus == Infrastructure.Data.Enums.LoginStatus.NeedsToChangePassword)
                    {
                        user.LoginTrials = 0;
                        user.LoginStatus = Infrastructure.Data.Enums.LoginStatus.Active;
                        this._context.Users.Update(user);
                        this._context.SaveChanges();

                        WebUser.SetUser(user, GetRoles(user.Id));
                        await this.SignIn();

                        return RedirectToAction("change-password");
                    }
                    else if (user.LoginStatus == Infrastructure.Data.Enums.LoginStatus.Active)
                    {
                        user.LoginTrials = 0;
                        user.LoginStatus = Infrastructure.Data.Enums.LoginStatus.Active;
                        this._context.Users.Update(user);
                        this._context.SaveChanges();

                        WebUser.SetUser(user, GetRoles(user.Id));
                        await this.SignIn();

                        return RedirectPermanent("/posts/index");
                    }
                }
                else
                {
                    user.LoginTrials = user.LoginTrials + 1;

                    if(user.LoginTrials >= 3)
                    {
                        ModelState.AddModelError("", "Your account has been locked please contact an Administrator.");
                        user.LoginStatus = Infrastructure.Data.Enums.LoginStatus.Locked;
                    }

                    this._context.Users.Update(user);
                    this._context.SaveChanges();

                    ModelState.AddModelError("", "Invalid Login.");
                    return View();
                }
            }

            ModelState.AddModelError("", "Invalid Login.");
            return View();

        }

        private List<Role> GetRoles(Guid? userId)
        {
            List<Role> roles = new List<Role>();
            var userRoles = this._context.UserRoles.Where(r => r.UserId == userId);

            foreach(UserRole userRole in userRoles)
            {
                roles.Add(userRole.Role);
            }

            return roles;
        }

        [HttpGet, Route("account/forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost, Route("account/forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = this._context.Users.FirstOrDefault(u =>
                    u.EmailAddress.ToLower() == model.EmailAddress.ToLower());

            if (user != null)
            {
                var newPassword = RandomString(6);
                user.Password = BCrypt.BCryptHelper.HashPassword(newPassword, BCrypt.BCryptHelper.GenerateSalt(8));
                user.LoginStatus = Infrastructure.Data.Enums.LoginStatus.NeedsToChangePassword;

                this._context.Users.Update(user);
                this._context.SaveChanges();

                this.SendNow(
                            "Hi " + user.FirstName + " " + user.LastName + @",
                             You forgot your password. Please use this new password: " + newPassword + @".
                             Regards,
                             CSM Bataan Website",
                            user.EmailAddress,
                            user.FirstName + " " + user.LastName,
                            "CSM Bataan Website - Forgot Password"
                );
            }

            return View();
        }

        [Authorize(Policy = "SignedIn")]
        [HttpGet, Route("account/change-password")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize(Policy = "SignedIn")]
        [HttpPost, Route("account/change-password")]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (model.NewPassword != model.ConfirmNewPassword)
            {
                ModelState.AddModelError("", "New Password does not match Confirm New Password");
                return View();
            }


            var user = this._context.Users.FirstOrDefault(u =>
                    u.Id == WebUser.UserId);

            if (user != null)
            {
                if (BCrypt.BCryptHelper.CheckPassword(model.OldPassword, user.Password) == false)
                {
                    ModelState.AddModelError("", "Incorrect old Password.");
                    return View();
                }

                user.Password = BCrypt.BCryptHelper.HashPassword(model.NewPassword, BCrypt.BCryptHelper.GenerateSalt(8));
                user.LoginStatus = Infrastructure.Data.Enums.LoginStatus.Active;

                this._context.Users.Update(user);
                this._context.SaveChanges();

                return RedirectPermanent("/home/index");
            }

            return View();
        }



        /// <summary>
        /// ////////////////////////////////////////
        /// </summary>
        /// 
        private async Task SignIn()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, WebUser.UserId.ToString())
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.UtcNow
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        }

        private async Task SignOut()
        {
            await HttpContext.SignOutAsync();

            WebUser.EmailAddress = string.Empty;
            WebUser.FirstName = string.Empty;
            WebUser.LastName = string.Empty;
            WebUser.UserId = null;
            WebUser.Roles = null;

            HttpContext.Session.Clear();
        }


        private Random random = new Random();
        private string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void SendNow(string message, string messageTo, string messageName, string emailSubject)
        {
            var fromAddress = new MailAddress(emailUserName, "CSM Bataan Apps");
            string body = message;

            ///https://support.google.com/accounts/answer/6010255?hl=en
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, emailPassword),
                Timeout = 20000
            };

            var toAddress = new MailAddress(messageTo, messageName);

            smtp.Send(new MailMessage(fromAddress, toAddress)
            {
                Subject = emailSubject,
                Body = body,
                IsBodyHtml = true
            });
        }
    }
}