using Identity.Extensions;
using Identity.Models;
using Identity.Services;
using Identity.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : MainController
    {
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

        [HttpPost("ConfirmEmail")]
        public async Task<ActionResult> ConfirmEmail()
        {
            return Ok();
        }
    }
}
