using Aspose.Pdf;
using Aspose.Pdf.Operators;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace P2PWallet.Services.Services
{
    public class UserServices : IUserServices
    {
        public static User user = new User();
        public static Account account = new Account();
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserServices(DataContext dataContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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
                        AccountNumber = userAccounNumber,
                        Balance = account.Balance
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
                new Claim("AccountNumber", user.UserAccount.AccountNumber),
                new Claim(ClaimTypes.Role, "Admin")
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

        public async Task<ServiceResponse<LoginView>> Login(LoginDto loginreq)
        {
            var response = new ServiceResponse<LoginView>();
            try
            {
                var user = await _dataContext.Users.Include("UserAccount").FirstOrDefaultAsync(x => x.Username == loginreq.Username);

                if (user == null)
                {
                    throw new Exception("Username/Password is Incorrect");
                }

                if (!VerifyPasswordHash(loginreq.Password, user.PasswordKey, user.Password))
                {
                    throw new Exception("Username/Password is Incorrect");
                }

                var logindata = new LoginView()
                {
                    Name = loginreq.Username,
                    Token = CreateJWT(user)
                };
                response.Data = logindata;

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }

            return response;
        }


        public async Task<ServiceResponse<List<AccountDetails>>> GetMyAccountNumber()
        {
            var response = new ServiceResponse<List<AccountDetails>>();
            List <AccountDetails> accountDetails = new List<AccountDetails>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var userId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

                    var userAccount = await _dataContext.Accounts.Include("User").Where(x => x.UserId == userId).FirstOrDefaultAsync();

                    if (userAccount != null)
                    {
                        var data = new AccountDetails()
                        {
                            AccountName = userAccount.User.FirstName + " " + userAccount.User.LastName,
                            AccountNumber = userAccount.AccountNumber,
                            Balance = userAccount.Balance
                        };
                        accountDetails.Add(data);
                    }
                    response.Data = accountDetails;
                }

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }  
            return response;
        }   
        
        public async Task<ServiceResponse<List<SearchAccountDetails>>> GetUserDetails(UserSearchDto userSearch)
        {
            var response = new ServiceResponse<List<SearchAccountDetails>>();
            List<SearchAccountDetails> userDetails = new List<SearchAccountDetails>();
            try
            {
                var searchUser = await _dataContext.Users.FirstOrDefaultAsync(x => x.UserAccount.AccountNumber == userSearch.AccountSearch
                                                                                   || x.Username == userSearch.AccountSearch
                                                                                   || x.Email == userSearch.AccountSearch);

                if (searchUser == null)
                {
                    throw new Exception("Account does not exist");
                }
                    var userId = await _dataContext.Users.Include("UserAccount")
                                .Where(x => x.UserAccount.UserId == searchUser.Id)
                                .FirstOrDefaultAsync();

                    if (userId != null)
                    {
                        var data = new SearchAccountDetails()
                        {
                            AccountName = userId.FirstName + " " + userId.LastName,
                            AccountNumber = userId.UserAccount.AccountNumber
                        };
                        userDetails.Add(data);
                    }
                    response.Data = userDetails;

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
        }
    }
}

