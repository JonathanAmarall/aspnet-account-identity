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
    public class AuthController : ControllerBase
    {
        [HttpPost("Authenticate")]
        public async Task<ActionResult> Authenticate(
            AuthenticateUserViewModel model,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromServices] TokenService tokenService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(e => e.Errors));

            if (model.ConfirmPassword != model.Password) return BadRequest("Password and ConfirmPassowrd does not match.");
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null) return BadRequest("Email ou senha inválidos.");
            var result = await signInManager.PasswordSignInAsync(user, model.Password, false, true);

            if (result.Succeeded)
            {
                var token = await tokenService.GenerateTokenAsync(user);
                await signInManager.SignInAsync(user, false);
                return Ok(new
                {
                    Token = token,
                    UserName = user.UserName,
                    Email = user.Email,
                });
            }

            return BadRequest("Email ou senha inválidos.");
        }

        [HttpPost("Register")]
        public async Task<ActionResult> Register([FromServices] UserManager<ApplicationUser> userManager)
        {
            var newUser = new ApplicationUser
            {
                UserName = "host@mail.com",
                Email = "host@mail.com",
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(newUser, "123Qwe!");

            return Ok();
        }

        [HttpPost("ConfirmEmail")]
        public async Task<ActionResult> ConfirmEmail()
        {
            return Ok();
        }
    }
}
