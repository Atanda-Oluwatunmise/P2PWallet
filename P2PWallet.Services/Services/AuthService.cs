using Azure.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;


namespace P2PWallet.Services.Services
{
    public class AuthService:IAuthService
    {
        public static User user = new User();
        public static Account account = new Account();
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserServices _userServices;
        private readonly IMailService _mailService;
        public AuthService(DataContext dataContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IUserServices userServices, IMailService mailService) 
        {
            _dataContext = dataContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _userServices = userServices;
            _mailService = mailService;
        }

        private void CreatePinHash(string pin, out byte[] pinKey, out byte[] pinHash)
        {
            using (var hmac = new HMACSHA512())
            {
                pinKey = hmac.Key;
                pinHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(pin));
            }
        }

            public async Task<ServiceResponse<string>> CreatePin(PinDto pin)
            {
                var response = new ServiceResponse<string>();
            //string resData = "Pin created successfully";

            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var pinAccount = await _dataContext.Users.Include("Userpin").Where(x => x.Id == loggedUserId).FirstOrDefaultAsync();

                    if (pinAccount != null && pinAccount.Userpin == null)
                    //{
                    //    throw new Exception("Already created a Pin!");
                    //}

                    //if (pinAccount != null)
                    {
                        CreatePinHash(pin.UserPin,
                            out byte[] pinKey, out byte[] pinHash);

                        //instantiating the User constructor
                        var newpin = new Pin()
                        {
                            PinId = pinAccount.Id,
                            UserPin = pinHash,
                            PinKey = pinKey
                        };
                        response.Data = "Pin created successfully";

                        await _dataContext.Pin.AddAsync(newpin);
                        await _dataContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
        }

        public bool VerifyPinHash(string pin, byte[] pinkey, byte[] pinhash)
        {
            try
            {
                using (var hmac = new HMACSHA512(pinkey))
                {
                    //computing passwordhash
                    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(pin));

                    //check if computedhash and passwordhash are the same
                    return computedHash.SequenceEqual(pinhash);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot Verify Password");
            }
        }

        public async Task<ServiceResponse<string>> VerifyPin(PinDto pin)
        {
            var response = new ServiceResponse<string>();

            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var userId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var loggedInUser = await _dataContext.Pin.Include("User").Where(x => x.PinId == userId).FirstOrDefaultAsync();

                    if (loggedInUser != null)
                    {
                        if (!VerifyPinHash(pin.UserPin, loggedInUser.PinKey, loggedInUser.UserPin))
                        {
                            throw new Exception("Pin is Incorrect");
                        }
                        response.Data = "Pin is correct";
                    }
                }
            }
            catch (Exception ex) {
                response.Status = false; 
                response.Data = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<string>> ValidateUser()
        {
            var response = new ServiceResponse<string>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedinId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var loggedinUser = await _dataContext.Users.Include("Userpin").Where(x => x.Id == loggedinId).FirstOrDefaultAsync();

                    if (loggedinUser.Userpin == null)
                    {
                        response.Data = "Never created a Pin";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<string>> ForgotPassword(string email)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var user = await _dataContext.Users.Include("UserAccount").Where(x => x.Email == email).FirstOrDefaultAsync();
                if (user == null)
                {
                    throw new Exception("Email cannot be found");
                }
                var token = _userServices.CreateJWT(user);
                var encodedToken = Encoding.UTF8.GetBytes(token);
                var validToken = WebEncoders.Base64UrlEncode(encodedToken);

                string url = $"{_configuration.GetSection("ResetPassword:Reseturl").Value!}?email={email}&token={validToken}";
                await _mailService.ResetPasswordEmailAsync(email, "Reset Password", "<h1>Follow the instruction to reset your password</h1>",
                    $"<p>To reset your password <a href={url}>Click Here</a></p>");

                response.Data = "Password Link has been sent to Email";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<string>> ChangePassword(ChangePasswordDto changepassword)
        {
            var response = new ServiceResponse<string>();


            try
            {
                if (_httpContextAccessor.HttpContext!= null)
                {
                    var loggedinId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var loggedinUser = await _dataContext.Users.Where(x => x.Id == loggedinId).FirstOrDefaultAsync();

                    var user = _dataContext.Users.Where(x => x.Username == changepassword.Username).FirstOrDefault();

                    if (loggedinUser != user)
                    {
                        throw new Exception("Cannot Change Password");
                    }
                    if (loggedinUser != null)
                    {
                        var userPassword = _userServices.VerifyPasswordHash(changepassword.CurrentPassword, loggedinUser.PasswordKey, loggedinUser.Password);
                        if (!userPassword)
                        {
                            throw new Exception("Current Password does not exist");
                        }
                    }
                    if (string.Compare(changepassword.NewPassword, changepassword.ConfirmPassword) != 0)
                    {
                        throw new Exception("Password does not match");
                    }

                    _userServices.CreatePasswordHash(changepassword.ConfirmPassword, out byte[] passwordKey, out byte[] passwordHash);

                    loggedinUser.Password = passwordHash;
                    loggedinUser.PasswordKey = passwordKey;

                    _dataContext.SaveChanges();

                    response.Data = "Password successfully changed";
                }
            }
            catch(Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
        }
        public async Task<ServiceResponse<string>> ChangePin(ChangePinDto changepin)
        {
            var response = new ServiceResponse<string>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedinId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var loggedinUser = await _dataContext.Users.Include("Userpin").Where(x => x.Id == loggedinId).FirstOrDefaultAsync();

                    if (loggedinUser != null)
                    {
                        var userpin = VerifyPinHash(changepin.CurrentPin, loggedinUser.Userpin.PinKey, loggedinUser.Userpin.UserPin);
                        if (!userpin)
                        {
                            throw new Exception("Current Pin does not exist");
                        }
                    }
                    if (string.Compare(changepin.NewPin, changepin.ConfirmPin) != 0)
                    {
                        throw new Exception("Pin does not match");
                    }

                    CreatePinHash(changepin.ConfirmPin, out byte[] pinkey, out byte[] pinHash);

                    loggedinUser.Userpin.UserPin = pinHash;
                    loggedinUser.Userpin.PinKey = pinkey;

                     _dataContext.SaveChanges();

                    response.Data = "Pin successfully changed";

                }
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
