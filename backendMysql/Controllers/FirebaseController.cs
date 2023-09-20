using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using backendTest.Infrastructure.Attributes;
using backendTest.Infrastructure.Models;
using backendTest.Infrastructure.Services;

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
