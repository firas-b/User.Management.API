using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using User.Management.API.Models;
using User.Management.API.Models.Authentication.SignUp;

namespace User.Management.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        public AuthenticationController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;


        }



        [HttpPost]
        public async Task<IActionResult> Register([FromBody]RegisterUser registerUser,string role) { 
            // check user exists 
            var userExists=await _userManager.FindByEmailAsync(registerUser.Email);
            if(userExists != null) {
                return StatusCode(StatusCodes.Status403Forbidden,
                new Response { Status="Error",MessageProcessingHandler="User already exists"});
            }
            // add the user to the db 
            IdentityUser user = new()
            {
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUser.UserName,


            };
            if( await _roleManager.RoleExistsAsync(role))
            {
                var result = await _userManager.CreateAsync(user, registerUser.Password);
                if (!result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Fail", MessageProcessingHandler = "User creation failure " });
                }

                // Add role to the user
                await _userManager.AddToRoleAsync(user,role);
                return StatusCode(StatusCodes.Status200OK,
                    new Response { Status = "Success", MessageProcessingHandler = "User created successfully " });
            }
            else 
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                       new Response { Status = "Error", MessageProcessingHandler = "Role does not exist " });
            }
            
            


        }
    }
}
