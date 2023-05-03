using Aspose.Pdf.Operators;
using Azure;
using Azure.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Crypto.Macs;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.DataObjects.WebHook;
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

            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var userAccount = await _dataContext.Users.Include("Userpin").Where(x => x.Id == loggedUserId).FirstOrDefaultAsync();
                    var pinAccount = await _dataContext.Pin.Where(x => x.UserId == loggedUserId).FirstOrDefaultAsync();


                    if (userAccount != null && pinAccount == null)

                    {
                        CreatePinHash(pin.UserPin,
                            out byte[] pinKey, out byte[] pinHash);

                        //instantiating the User constructor
                        var newpin = new Pin()
                        {
                            UserId = userAccount.Id,
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
                    var loggedInUser = await _dataContext.Pin.Include("PinUser").Where(x => x.UserId == userId).FirstOrDefaultAsync();

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
                    var loggedinUser = await _dataContext.Pin.Include("PinUser").Where(x => x.UserId == loggedinId).FirstOrDefaultAsync();

                    if (loggedinUser == null)
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

        public async Task<ServiceResponse<string>> ForgotPassword(EmailDto emaill)
        {

            var response = new ServiceResponse<string>();
            try
            {
                var user = await _dataContext.Users.Include("UserAccount").Where(x => x.Email == emaill.Email).FirstOrDefaultAsync();
                if (user == null)
                {
                    throw new Exception("Email cannot be found");
                }
                if (user.VerificationToken == null)
                {
                    throw new Exception("User  cannot change password");
                }
                var token = _userServices.GenerateEmailToken();
                var encodedToken = Encoding.UTF8.GetBytes(token);
                var validToken = WebEncoders.Base64UrlEncode(encodedToken);

                string url = $"{_configuration.GetSection("ResetPassword:Reseturl").Value!}?token={validToken}";
                var sendmail = await _mailService.ResetPasswordEmailAsync(emaill.Email, "Reset Password", "<h1>Follow the instruction to reset your password</h1>",
                    $"<p>To reset your password <a href={url}>Click Here</a></p>" +
                    $"<p>Or paste {url} in your web browser</p>");


                if (sendmail == true)
                {
                    var resetdetails = new ResetPassword
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        Token = validToken,
                        TokenExpires = DateTime.Now.AddDays(1)
                    };
                    await _dataContext.ResetPasswords.AddAsync(resetdetails);
                    await _dataContext.SaveChangesAsync();

                    response.Data = "Reset password link has been sent to the email successfully, go and verify";
                }
                if (sendmail != true)
                {
                    response.Status = false;
                    response.Data = null;
                    response.StatusMessage = "Password link cannot be sent";
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<string>> ForgotPin(EmailDto emaill)
        {

            var response = new ServiceResponse<string>();
            try
            {
                var user = await _dataContext.Users.Include("UserAccount").Where(x => x.Email == emaill.Email).FirstOrDefaultAsync();
                if (user == null)
                {
                    throw new Exception("Email cannot be found");
                }
                var token = _userServices.GenerateEmailToken();
                var encodedToken = Encoding.UTF8.GetBytes(token);
                var validToken = WebEncoders.Base64UrlEncode(encodedToken);

                string url = $"{_configuration.GetSection("ResetPassword:ResetPin").Value!}?token={validToken}";
                var sendmail = await _mailService.ResetPasswordEmailAsync(emaill.Email, "Reset Pin", "<h1>Follow the instruction to reset your pin</h1>",
                    $"<p>To reset your pin, <a href={url}>click here</a></p>" +
                    $"<p>or paste {url} in your web browser</p>");

                if (sendmail == true)
                {
                    var resetdetails = new ResetPin
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        PinToken = validToken,
                        PinTokenExpires = DateTime.Now.AddDays(1)
                    };
                    await _dataContext.ResetPins.AddAsync(resetdetails);
                    await _dataContext.SaveChangesAsync();

                    response.Data = "Reset pin link has been sent to the email successfully, go and verify";
                }
                if (sendmail != true)
                {
                    response.Status = false;
                    response.Data = null;
                    response.StatusMessage = "Pin link cannot be sent";
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
        }


        public async Task<ServiceResponse<string>> ResetPassword(ResetPasswordRequest request)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var user = await _dataContext.ResetPasswords.Include("ResetUser").Where(x => x.Token == request.Token).FirstOrDefaultAsync();
                if (user == null || user.TokenExpires < DateTime.Now)
                {
                    throw new Exception("Invalid token");
                }
                if (string.Compare(request.Password, request.ConfirmPassword) != 0)
                {
                    throw new Exception("Password does not match");
                }
                _userServices.CreatePasswordHash(request.ConfirmPassword, out byte[] passwordKey, out byte[] passwordHash);

                user.ResetUser.Password = passwordHash;
                user.ResetUser.PasswordKey = passwordKey;

                _dataContext.SaveChanges();

                response.Data = "Password successfully changed";
            }
            catch(Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
        }
        
        public async Task<ServiceResponse<string>> ResetPin(ResetPinRequest request)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var user = await _dataContext.ResetPins.Include("ResetUserPin").Where(x => x.PinToken == request.Token).FirstOrDefaultAsync();
                var userdetail = await _dataContext.Pin.Include("PinUser").Where(x => x.UserId == user.UserId).FirstOrDefaultAsync();
                if (user == null || user.PinTokenExpires < DateTime.Now)
                {
                    throw new Exception("Invalid token");
                }
                if (string.Compare(request.Pin, request.ConfirmPin) != 0)
                {
                    throw new Exception("Pin does not match");
                }
                _userServices.CreatePasswordHash(request.ConfirmPin, out byte[] pinkey, out byte[] pinHash);

                userdetail.UserPin = pinHash;
                userdetail.PinKey = pinkey;
                //user.ResetUserPin.Userpin. .UserPin = pinHash;
                //user.ResetUserPin.Userpin.PinKey = pinkey;

                _dataContext.SaveChanges();

                response.Data = "Pin reset successful";
            }
            catch(Exception ex)
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

                    if (loggedinUser != null)
                    {
                        if(changepassword.CurrentPassword == changepassword.ConfirmPassword)
                        {
                            throw new Exception("Cannot use old password");
                        }

                        var userPassword = _userServices.VerifyPasswordHash(changepassword.CurrentPassword, loggedinUser.PasswordKey, loggedinUser.Password);
                        if (!userPassword)
                        {
                            throw new Exception("Current Password does not exist");
                        }
                    }

                    var verifyquestion = await _dataContext.SecurityQuestions.Include("UserSecurity").Where(x => x.UserId == loggedinId).FirstOrDefaultAsync();

                    if (changepassword.Answer != verifyquestion.Answer)
                    {
                        throw new Exception("Answer does not match");

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
                    var loggedinUser = await _dataContext.Users.Include("Userpin").Include("UserSecurityQuestion").Where(x => x.Id == loggedinId).FirstOrDefaultAsync();
                    var verifypinuser = await _dataContext.Pin.Include("PinUser").Where(x => x.UserId == loggedinId).FirstOrDefaultAsync();

                    if (loggedinUser != null)
                    {
                        if(changepin.CurrentPin == changepin.ConfirmPin)
                        {
                            throw new Exception("Cannot use old pin");
                        }
                        

                        var userpin = VerifyPinHash(changepin.CurrentPin, verifypinuser.PinKey, verifypinuser.UserPin);
                        if (!userpin)
                        {
                            throw new Exception("Current Pin does not exist");
                        }
                    }
                    var verifyquestion = await _dataContext.SecurityQuestions.Include("UserSecurity").Where(x => x.UserId == loggedinId).FirstOrDefaultAsync();
                 
                    
                    if(changepin.Answer != verifyquestion.Answer)
                    {
                        throw new Exception("Answer does not match");

                    }

                    if (string.Compare(changepin.NewPin, changepin.ConfirmPin) != 0)
                    {
                        throw new Exception("Pin does not match");
                    }
                    CreatePinHash(changepin.ConfirmPin, out byte[] pinkey, out byte[] pinHash);

                    verifypinuser.UserPin = pinHash;
                    verifypinuser.PinKey = pinkey;

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

        public async Task<ServiceResponse<string>> AddSecurityDetails(SecurityQuestionDto details)
        {
            var response = new ServiceResponse<string>();

            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var securityUser = await _dataContext.Users.Where(x => x.Id == loggedUserId).FirstOrDefaultAsync();

                    if (securityUser != null)

                    {
                        var securityDetails = new SecurityQuestion()
                        {
                            UserId = securityUser.Id,
                            Question = details.Question,
                            Answer = details.Answer
                        };

                        response.Data = "Security details saved successfully";

                        await _dataContext.SecurityQuestions.AddAsync(securityDetails);
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

        public async Task<ServiceResponse<SecurityViewModel>> GetSecuritydetail()
        {
            var response = new ServiceResponse<SecurityViewModel>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedinId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var loggedinUser = await _dataContext.SecurityQuestions.Where(x => x.UserId == loggedinId).FirstOrDefaultAsync();

                    if (loggedinUser != null)
                    {

                        var question = new SecurityViewModel
                        {
                            Question = loggedinUser.Question
                        };
                        response.Data = question;
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
    }
}
