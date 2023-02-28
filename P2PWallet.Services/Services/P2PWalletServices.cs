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
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;

        public P2PWalletServices(DataContext dataContext, IConfiguration configuration)
        {
            _dataContext = dataContext;
            _configuration= configuration;
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
            var serviceResponse = new ServiceResponse<UserViewModel>();

            try
            {
                   
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
                serviceResponse.Data = new UserViewModel();
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.StatusMessage = ex.Message;
            }
            return serviceResponse;
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
                new Claim(ClaimTypes.Name, user.Username)

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
    }
}
