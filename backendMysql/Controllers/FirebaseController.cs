using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using backendTest.Infrastructure.Models;
using backendTest.Infrastructure.Services;

namespace backendTest.Controllers
{
    [Route("api/[controller]")]    
    [ApiController]
    public class FirebaseController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly DataContext _context;
        private readonly ILogger<FirebaseController> _logger;

        public FirebaseController(IFirebaseService firebaseService, DataContext context, ILogger<FirebaseController> logger)
        {
            _firebaseService = firebaseService;
            _context = context;
            _logger = logger;
        }

        [HttpGet("SignInAnonymously")]
        public async Task<ActionResult<FirebaseUserToken>> SignInAnonymously()
        {
            try
            {
                var result = await _firebaseService.SignInAnonymously();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during anonymous sign-in.");
                return StatusCode(500, "An error occurred during sign-in.");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            try
            {
                // Input validation
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if the user already exists
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return Conflict("User already exists.");
                }

                // Register the user with Firebase
                var firebaseResult = await _firebaseService.SignUpWithEmailAndPassword(request.Email, request.Password);

                // Create and save user in the database
                var user = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    
                    IdToken = firebaseResult.idToken
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok("User successfully created!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration.");
                return StatusCode(500, "An error occurred during registration.");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<FirebaseUserToken>> Login([FromBody] UserLoginRequest request)
        {
            try
            {
                var result = await _firebaseService.SignInWithEmailAndPassword(request.Email, request.Password);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login.");
                return StatusCode(500, "An error occurred during login.");
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult> ResetPasswordByEmail(string email)
        {
            try
            {
                await _firebaseService.ResetPasswordByEmail(email);
                return Ok("Check your email for password reset instructions.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset.");
                return StatusCode(500, "An error occurred during password reset.");
            }
        }

        [HttpPost("SignInWithOTP")]
        public async Task<ActionResult> SignInWithOTP(string number,string otp,string email)
        {
            try
            {
                 FirebaseUserToken res =await _firebaseService.SignInWithOTP(number,otp);
                if ()
                {
                    var user = _context.Users.FirstOrDefault(u => u.Email == email);

                    if (user != null)
                    {
                        // Update user data with new values from the form
                        user.phoneNumber = number;
                        // Save changes to the database
                       // await _context.SaveChanges();

                        // Provide feedback to the user
                    }
                    return Ok("sucess");
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset.");
                return StatusCode(500, "An error occurred during is not your number");
            }
        }

        [HttpGet("deletacount")]
        public async Task<ActionResult> DeleteAccount(string email, string password)
        {
            try
            {
                var userToDelete = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (userToDelete != null)
                {
                    // Remove the user from Firebase
                    await _firebaseService.DeleteAccount(email, password);

                    // Remove the user from the database
                    _context.Users.Remove(userToDelete);
                    await _context.SaveChangesAsync();

                    return Ok("Your account has been deleted.");
                }
                else
                {
                    return NotFound("User not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during account deletion.");
                return StatusCode(500, "An error occurred during account deletion.");
            }
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            // Specify the authentication scheme and additional parameters
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleLoginCallback)),
            };

            return Challenge(authenticationProperties, "Google");
        }

        // Callback endpoint for Google authentication
        [HttpGet("google-login-callback")]
        public IActionResult GoogleLoginCallback()
        {
            // Check if the authentication was successful
            if (User.Identity.IsAuthenticated)
            {
                // User is authenticated, you can access their information here
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
                // Perform any user management or other actions here

                return Ok($"Logged in as {userEmail}");
            }
            else
            {
                // Authentication failed
                return BadRequest("Google authentication failed");
            }
        }

        // Endpoint for logging the user out
        [Authorize] // Require authentication to access this endpoint
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            // Sign out the user
            HttpContext.SignOutAsync();

            return Ok("Logged out successfully");
        }

        [HttpGet("SignInWithGoogleAccessToken")]
        public async Task<ActionResult<FirebaseOAuthUserToken>> SignInWithGoogleAccessToken(string googleIdToken)
        {
            try
            {
                var result = await _firebaseService.SignInWithGoogleAccessToken(googleIdToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google access token sign-in.");
                return StatusCode(500, "An error occurred during Google access token sign-in.");
            }
        }

        [HttpGet("SignInWithFacebookAccessToken")]
        public async Task<ActionResult<FirebaseOAuthUserToken>> SignInWithFacebookAccessToken(string facebookAccessToken)
        {
            try
            {
                var result = await _firebaseService.SignInWithFacebookAccessToken(facebookAccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google access token sign-in.");
                return StatusCode(500, "An error occurred during Google access token sign-in.");
            }
        }
        //[Authorize]
        //[HttpGet("GetDataFromMyToken")]
        //public async Task<ActionResult<FirebaseToken>> GetDataFromFirebaseToken()
        //{
        //   try
        //   {
        //       // Retrieve Firebase data using the user's token
        //      var userToken = HttpContext.Items["user"] as FirebaseToken;
        //       // Process data as needed

        //        return Ok(userToken);
        //    }
        //    catch (Exception ex)
        //   {
        //      _logger.LogError(ex, "Error during data retrieval.");
        //        return StatusCode(500, "An error occurred during data retrieval.");
        //    }
        //  }
    }
}
