using Aspose.Pdf;
using Aspose.Pdf.Operators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace P2PWallet.Services.Services
{
    public class P2PWalletServices : IP2PWalletServices
    {
        public static User user = new User();
        public static Account account = new Account();
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public P2PWalletServices(DataContext dataContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _dataContext = dataContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<bool> EmailAlreadyExists(string emailName)
        {
            return await _dataContext.Users.AnyAsync(x => x.Email == emailName);
        }

        private void CreatePasswordHash(string password, out byte[] passwordKey, out byte[] passwordHash)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordKey = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
        public async Task<ServiceResponse<UserViewModel>> Register(UserDto user)
        {
            //Initializing a collection
            var response = new ServiceResponse<UserViewModel>();

            try
            {
                if (await UserAlreadyExists(user.Username) || await EmailAlreadyExists(user.Email))
                {
                    throw new Exception("User Already Exists");
                }

                CreatePasswordHash(user.Password,
                    out byte[] passwordKey, out byte[] passwordHash);

                //instantiating the User constructor
                var newuser = new User()
                {
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Password = passwordHash,
                    PasswordKey = passwordKey
                };

                //Adding the new instance in the databse
                await _dataContext.Users.AddAsync(newuser);
                await _dataContext.SaveChangesAsync();
                //response.Data = new UserViewModel();

                string userAccounNumber = string.Empty;
                string startWith = "1000";
                Random generator = new Random();
                string r = generator.Next(0, 999999).ToString("D6");
                userAccounNumber = startWith + r;

                if (newuser != null && await UserAlreadyExists(user.Username))
                {
                    var newaccount = new Account()
                    {
                        UserId = newuser.Id,
                        AccountNumber = userAccounNumber
                    };
                    await _dataContext.Accounts.AddAsync(newaccount);
                    await _dataContext.SaveChangesAsync();

                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }

            return response;

        }

        public async Task<bool> UserAlreadyExists(string userName)
        {
            return await _dataContext.Users.AnyAsync(x => x.Username == userName);
        }

        public bool VerifyPasswordHash(string password, byte[] passwordKey, byte[] passwordHash)
        {
            using (var hmac = new HMACSHA512(passwordKey))
            {
                //computing passwordhash
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                //check if computedhash and passwordhash are the same
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateJWT(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "Admin"),
            };
            // defining a symmetric security key that creates the web token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));

            //signing credentials
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //generate token
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(3),
                signingCredentials: credentials
                );

            //write the token
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        public async Task<ServiceResponse<string>> Login(LoginDto loginreq)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Username == loginreq.Username);

                if (user == null)
                {
                    throw new Exception("Username/Password is Incorrect");
                }

                if (!VerifyPasswordHash(loginreq.Password, user.PasswordKey, user.Password))
                {
                    throw new Exception("Username/Password is Incorrect");
                }

                string token = CreateJWT(user);
                response.Data = token;

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }

            return response;
        }

        public string GetMyAccountNumber()
        {

            var result = string.Empty;
            if (_httpContextAccessor.HttpContext != null)
            {
                var userId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

                var userAccount = _dataContext.Accounts.Where(x => x.UserId == userId).FirstOrDefault();

                if (userAccount != null)
                {
                    result = userAccount.AccountNumber.ToString();
                }
            }
        
            return result;

        }
            
    }
}

