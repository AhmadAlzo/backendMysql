using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using backendTest.Infrastructure.Attributes;
using backendTest.Infrastructure.Models;
using backendTest.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace backendTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FirebaseController : ControllerBase
    {
        private readonly IFirebaseService _service;
        private readonly DataContext _context;

        public FirebaseController(IFirebaseService service, DataContext context) : base()
        {
            _service = service;
            _context = context;
        }

        [HttpGet("SignInAnonymously")]
        public async Task<ActionResult<FirebaseUserToken>> SignInAnonymously()
        {
            return await _service.SignInAnonymously();
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserRegisterRequest request)
        {
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest("User already exists.");
            }
            var res = await _service.SignUpWithEmailAndPassword(request.Email, request.Password);

            var user = new User
            {
                Name = request.Name,  
                Email = request.Email,
                IdToken = res.idToken
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User successfully created!");

        }

        [HttpPost("login")]
        public async Task<ActionResult<FirebaseUserToken>> Login(UserLoginRequest request)
        {
            return await _service.SignInWithEmailAndPassword(request.Email, request.Password);
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult> ResetPasswordByEmail(string email)
        { 
             await _service.ResetPasswordByEmail(email);
            return Ok("chek your gmail");
        }

        [HttpGet("deletacount")]
        public async Task<ActionResult> Logout(string email,string password)
        {

            var userToDelete = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (userToDelete != null)
            {
                // Remove the user from the firebase
                await _service.DeleteAccount(email, password);

                // Remove the user from the context
                _context.Users.Remove(userToDelete);
                await _context.SaveChangesAsync();

                return Ok("your acount de");
            }
            else
            {
                // Handle the case where the user with the specified email is not found.
                // You can throw an exception or return an appropriate response.
                // For example:
                // throw new NotFoundException("User not found");
                // or
                return NotFound("User not found");

            }
        }


        [HttpGet("SignInWithGoogle")]
        public async Task GoogleLogin()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }

        public async Task GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities
                .FirstOrDefault().Claims.Select(claim => new
                {
                    claim.Issuer,
                    claim.OriginalIssuer,
                    claim.Type,
                    claim.Value
                });
            
        }

        [HttpGet("logoutWithGoogle")]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync();
            
        }



        [HttpGet("SignInWithGoogleAccessToken")]
        public async Task<ActionResult<FirebaseOAuthUserToken>> SignInWithGoogleAccessToken(string googleIdToken)
        {
            return await _service.SignInWithGoogleAccessToken(googleIdToken);
        }

        /// <summary>
        /// You have to be authorized in swagger to be able get data from firebase token.
        /// </summary>
        [Authorize]
        [HttpGet("GetDataFromMyToken")]
        public async Task<ActionResult<FirebaseToken>> GetDataFromFirebaseToken()
        {
            return await Task.FromResult((FirebaseToken)HttpContext.Items["user"]);
        }
    }
}
