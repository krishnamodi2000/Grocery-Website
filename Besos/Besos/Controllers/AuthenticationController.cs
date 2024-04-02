using Besos.Configurations;
using Besos.Models;
using Besos.Models.DTOs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Besos.Controllers
{
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;

        //private readonly JwtConfig _jwtConfig;

        private readonly IConfiguration _configuration;


        public AuthenticationController(
            UserManager<IdentityUser> userManager,
           IConfiguration configuration
            //JwtConfig jwtConfig
            )
        {
            //_jwtConfig = jwtConfig;
            _configuration = configuration;
            _userManager = userManager;

        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto userRegistrationRequestDto)
        {
            //validate the incoming request
            if(ModelState.IsValid)
            {
                //Check if email already exsists
                var user_exsits = await _userManager.FindByEmailAsync(userRegistrationRequestDto.Email);

                if(user_exsits != null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Email exists already. Please login"
                        }
                    });
                }

                //create a user
                var new_user = new IdentityUser()
                {
                    Email= userRegistrationRequestDto.Email,
                    UserName = userRegistrationRequestDto.Name,
                };

                //create a user with a specific password
                var is_created = await _userManager.CreateAsync(new_user, userRegistrationRequestDto.Password);

                if (is_created.Succeeded) 
                {
                    //Generate Token
                    var token= GenerateJwtToken(new_user);

                    return Ok(new AuthResult()
                    {
                        Result = true,
                        Token = token
                    });
                }

                return BadRequest(new AuthResult()
                {
                    Errors= new List<string>()
                    {
                        "Server Error"
                    },
                    Result=false
                });

            }
            
            return BadRequest();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginRequestDto)
        {
            if (ModelState.IsValid)
            {
                //check is user exists
                var existing_user = await _userManager.FindByEmailAsync(loginRequestDto.Email);

                if (existing_user == null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid Paylod"
                        },
                        Result = false
                    });
                }

                var isCorrect = await _userManager.CheckPasswordAsync(existing_user, loginRequestDto.Password);

                if (!isCorrect)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid Paylod"
                        },
                        Result = false
                    });
                }

                var jwtToken = GenerateJwtToken(existing_user);

                return Ok(new AuthResult()
                {
                    Result = true,
                    Token = jwtToken
                });

            }

            return BadRequest(new AuthResult()
            {
                Errors = new List<string>()
                {
                    "Invalid Paylod"
                },
                Result= false
            });
        }
        private string GenerateJwtToken(IdentityUser identityUser)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:Secret").Value);

            //Token descriptor

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", identityUser.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, identityUser.Email),
                    new Claim(JwtRegisteredClaimNames.Email, identityUser.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
                }),

                Expires = DateTime.UtcNow.AddHours(1),
                NotBefore= DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }

    }
}
