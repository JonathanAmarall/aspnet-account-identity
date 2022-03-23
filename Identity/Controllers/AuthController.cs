using Identity.Extensions;
using Identity.Models;
using Identity.Services;
using Identity.Services.AspEmail.Services;
using Identity.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Identity.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : MainController
    {
        private readonly IEmailService _emailService;

        public AuthController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("Authenticate")]
        public async Task<ActionResult> Authenticate(
            AuthenticateUserViewModel model,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromServices] TokenService tokenService)
        {
            if (!ModelState.IsValid) return CustomReponse(ModelState);

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null) AddProcessingError("Email or password invalid.");

            if (!user.EmailConfirmed) AddProcessingError("Email is not confirmed.");

            var result = await signInManager.PasswordSignInAsync(user, model.Password, false, true);

            if (result.Succeeded)
            {
                var token = await tokenService.GenerateTokenJwtAsync(user);
                await signInManager.SignInAsync(user, false);
                return CustomReponse(new
                {
                    Token = token,
                    UserName = user.UserName,
                    Email = user.Email,
                });
            }
            AddProcessingError("Email or password invalid.");
            return CustomReponse();
        }

        [HttpPost("Register")]
        public async Task<ActionResult> Register(
            RegisterUserViewModel model,
            [FromServices] TokenService tokenService,
            [FromServices] UserManager<ApplicationUser> userManager)
        {
            if (model.ConfirmPassword != model.Password)
            {
                AddProcessingError("Password and ConfirmPassowrd does not match.");
                return CustomReponse();
            }

            var newUser = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = false,
            };

            var result = await userManager.CreateAsync(newUser, model.Password);

            if (result.Succeeded)
            {
                var tokenJwt = await tokenService.GenerateTokenJwtAsync(newUser);
                var tokenOfConfirmationEmail = await userManager.GenerateEmailConfirmationTokenAsync(newUser);
                await SendEmailConfirmationTokenAsync(tokenOfConfirmationEmail, newUser);

                return CustomReponse(new
                {
                    Token = tokenJwt,
                    UserName = newUser.UserName,
                    Email = newUser.Email,
                });
            }

            foreach (var error in result.Errors)
            {
                AddProcessingError(error.Description);
            }

            return CustomReponse();
        }

        [HttpGet("ConfirmEmail")]
        public async Task<ActionResult> ConfirmEmail(string token, string email, [FromServices] UserManager<ApplicationUser> userManager)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                AddProcessingError("There was an error confirming email.");
                return CustomReponse();
            }

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    AddProcessingError(error.Description);
                }

                return CustomReponse();
            }

            return CustomReponse("Email successfully confirmed!");
        }

        [HttpPost("RecoveryPassword")]
        public async Task<ActionResult> RecoveryPassword(
            RequestRecoveryPasswordViewModel model,
            [FromServices] UserManager<ApplicationUser> userManager
        )
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null) return BadRequest();

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            await SendEmailForRequestRecoveryPasswordAsync(token, user);

            return CustomReponse();
        }


        [HttpPost("ResetPasword")]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model, [FromServices] UserManager<ApplicationUser> userManager)
        {
            if (model.Password != model.ConfirmPassword)
            {
                AddProcessingError("Password and ConfirmPassword does not match.");
                return CustomReponse();
            }

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                AddProcessingError("User not found.");
                return CustomReponse();
            }

            var resetPassResult = await userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (!resetPassResult.Succeeded)
            {

                foreach (var error in resetPassResult.Errors)
                {
                    AddProcessingError(error.Description);
                }
            }

            return CustomReponse();
        }

        private async Task SendEmailForRequestRecoveryPasswordAsync(string token, ApplicationUser user)
        {
            string tokenHtmlVersion = HttpUtility.UrlEncode(token);

            var emailData = new EmailRequest
            {
                ToEmail = user.Email,
                Subject = $"Recovery Password at {user.UserName}",
                Body = $@"<h2>Hello {user.UserName}, we received a request for recovery password.</h2>
                        <p>If it wasn't you, please disregard.</p>
                        <a href='Send-this-your-frontend-url?token={tokenHtmlVersion}&email={user.Email}'>Recovery Password example</a>"
            };

            await _emailService.SendEmailAsync(emailData);
        }

        private async Task SendEmailConfirmationTokenAsync(string tokenOfConfirmationEmail, ApplicationUser user)
        {
            string tokenHtmlVersion = HttpUtility.UrlEncode(tokenOfConfirmationEmail);

            var emailData = new EmailRequest
            {
                ToEmail = user.Email,
                Subject = $"Welcome {user.UserName}",
                Body = $@"<h2>Ol&aacute; {user.UserName}, obrigado por se cadastrar.</h2>
                        <p>Por segurança, precisamos que você clique no link a baixo para confirmar o seu email.</p>
                        <a href='{GetBaseUrl()}/api/v1/Auth/ConfirmEmail?token={tokenHtmlVersion}&email={user.Email}'> Confirmar E-mail </a>"
            };

            await _emailService.SendEmailAsync(emailData);
        }

        private string GetBaseUrl()
        {
            var request = HttpContext.Request;
            var _baseURL = $"{request.Scheme}://{request.Host}"; // http://localhost:5000
            return _baseURL;
        }


    }
}
