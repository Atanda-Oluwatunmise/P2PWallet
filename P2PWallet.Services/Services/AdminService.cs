using Aspose.Cells;
using Aspose.Pdf.Operators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Octokit;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace P2PWallet.Services.Services
{
    public class AdminService : IAdminService
    {
        public static SuperAdmin superAdmin = new SuperAdmin();
        private readonly IConfiguration _configuration;
        private readonly DataContext _dataContext;
        private readonly ILogger<UserServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMailService _mailService;
        private readonly INotificationService _notificationService;

        public AdminService(DataContext dataContext, IConfiguration configuration, ILogger<UserServices> logger, IHttpContextAccessor httpContextAccessor, IMailService mailService, INotificationService notificationService)
        {
            _configuration = configuration;
            _dataContext = dataContext;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mailService = mailService;
            _notificationService = notificationService;
        }
        private string CreateJWT(SuperAdmin superAdmin, Admin? admin)
        {
            List<Claim> claims = new List<Claim>();
            if (superAdmin == null && admin == null)
            {
                return null;
            }
            if (superAdmin == null)
            {

                claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                    new Claim(ClaimTypes.Name, admin.Username),
                    //new Claim("AccountNumber", user.),
                    new Claim(ClaimTypes.Role, "Admin")
                };
            }
            if (admin == null)
            {
                claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, superAdmin.Id.ToString()),
                    new Claim(ClaimTypes.Name, superAdmin.Name),
                    //new Claim("AccountNumber", user.),
                    new Claim(ClaimTypes.Role, "Admin")
                };
            }
          
            // defining a symmetric security key that creates the web token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));

            //signing credentials
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //generate token
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
                );


            //write the token
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
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
        public async Task<ServiceResponse<LoginView>> Login(LoginDto loginreq)
        {
            var response = new ServiceResponse<LoginView>();
            try
            {
                var superadmin = await _dataContext.SuperAdmins.FirstOrDefaultAsync(x => x.Name == loginreq.Username);
                var admin = await _dataContext.Admins.FirstOrDefaultAsync(x => x.Username == loginreq.Username);
                if (superadmin == null && admin == null)
                {
                    throw new Exception("Username/Password is Incorrect");
                }
                if (superadmin != null)
                {
                    if (!VerifyPasswordHash(loginreq.Password, superadmin.PasswordKey, superadmin.Password)) 
                    { 
                        throw new Exception("Username/Password is Incorrect");
                    }
                }  
                if (admin != null)
                {
                    if (!VerifyPasswordHash(loginreq.Password, admin.PasswordKey, admin.Password))
                    {
                        throw new Exception("Username/Password is Incorrect");
                    }
                }

                if (superadmin != null)
                {
                    superadmin.UserToken = CreateJWT(superadmin, null);
                    await _dataContext.SaveChangesAsync();
                }
                if (admin != null)
                {
                    admin.UserToken = CreateJWT(null, admin);
                    await _dataContext.SaveChangesAsync();
                    if (admin.PasswordChanged == false)
                        response.StatusMessage = "Admin first login";
                }

                if (superadmin != null)
                {
                    var logindata = new LoginView()
                    {
                        Name = loginreq.Username,
                        Token = superadmin.UserToken,
                    };
                    response.Data = logindata;
                }


                if (admin != null)
                {
                    var adminview = new LoginView()
                    {
                        Name = loginreq.Username,
                        Token = admin.UserToken,
                    };
                    response.Data = adminview;
                }
                

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"AN ERROR OCCURRED....{ex.Message}");
            }

            return response;
        }

        public async Task<ServiceResponse<AdminAccount>> GetAdminAccountDetail()
        {
            var response = new ServiceResponse<AdminAccount>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var userId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

                    var userAccount = await _dataContext.Admins.Where(x => x.Id == userId).FirstOrDefaultAsync();
                    var adminAccount = await _dataContext.SuperAdmins.Where(x => x.Id == userId).FirstOrDefaultAsync();

                    if (userAccount != null)
                    {
                        var data = new AdminAccount()
                        {
                            Username = userAccount.Username
                        };
                        response.Data = data;
                    }
                    if (adminAccount != null)
                    {
                        var data = new AdminAccount()
                        {
                            Username = adminAccount.Name
                        };
                        response.Data = data;
                    }
                    if (userAccount == null && adminAccount == null)
                    {
                        response.Data = null;
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
        public string GenerateGlNumber(string currency)
        {
            int totalCharLength = 15;
            string allowedChars = "0123456789";
            int allowedCharCount = totalCharLength - currency.Length;
            Random randNum = new Random();
            char[] chars = new char[totalCharLength];

                for(int j = 0; j < allowedCharCount; j++)
                {
                    chars[j] = allowedChars[(int)((allowedChars.Length) * randNum.NextDouble())];
                    if (j == 5)
                    {
                        foreach (char c in currency)
                        {
                        chars[j] += c;
                        }
                    }

                }
            
            return new string(chars);

        }

        //Autogenerate Password
        public string AutogenerateAdminPassword(int passwordLength)
        {
            string allowedChars = "0123456789";
            Random randNum = new Random();
            char[] chars = new char[passwordLength];
            int allowedCharCount = allowedChars.Length;
            for (int i = 0; i < passwordLength; i++)
            {
                chars[i] = allowedChars[(int)((allowedChars.Length) * randNum.NextDouble())];
            }
            return new string(chars);
        }

        //SuperAdmin Should create new admins
        public async Task<ServiceResponse<string>> CreateNewAdmins(EmailDto emailDto)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var nameofAdmin = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
                if (nameofAdmin.ToLower() != "admin")
                {
                    throw new Exception("This admin is not authorized to create an admin");
                }

                if (await UserAlreadyExists(emailDto.Email) || await EmailAlreadyExists(emailDto.Email))
                {
                    throw new Exception("User Already Exists");
                }
                int passwordLength = 8;
                var generatedPassword = AutogenerateAdminPassword(passwordLength);
                byte[] passwordKey = null;
                byte[] passwordHash = null;
                using (var hmac = new HMACSHA512())
                {
                    passwordKey = hmac.Key;
                    passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(generatedPassword));
                }

                var admin = new Admin()
                {
                    Username = emailDto.Email,
                    Email = emailDto.Email,
                    Password = passwordHash,
                    PasswordKey = passwordKey
                };
                await _dataContext.Admins.AddAsync(admin);
                await _dataContext.SaveChangesAsync();
                string subject = "LOGIN DETAILS FOR YOUR ACCOUNT";
                string MailBody = "<!DOCKTYPE html>" +
                                        "<html>" +
                                            "<body>" +
                                            $"<h3>Dear admin {emailDto.Email},</h3>" +
                                            $"<h5>Your user name same as you email is {emailDto.Email} and your Password is {generatedPassword}." +
                                            $"Please go ahead to change your password on first login.</h5>" +
                                            $"<br>" +
                                            $"<h5>Warm regards.</h5>" +
                                            "</body>" +
                                        "</html>";

                var sendmail = await _mailService.SendLoginDetails(emailDto.Email, subject, MailBody);
                    
            }
            catch(Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"AN ERROR OCCURRED....{ex.Message}");
            }
            return response;
        }

        public async Task<bool> EmailAlreadyExists(string emailName)
        {
            return await _dataContext.Admins.AnyAsync(x => x.Email == emailName);
        }


        public async Task<bool> UserAlreadyExists(string userName)
        {
            return await _dataContext.Admins.AnyAsync(x => x.Username == userName);
        }

        public async Task <ServiceResponse<string>> ChangeAdminPassword(ResetPasswordDto resetPasswordDto)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var adminacc = await _dataContext.Admins.Where(x => x.Username == resetPasswordDto.Username).FirstOrDefaultAsync();
                if (adminacc == null)
                {
                    throw new Exception("Admin Account does not exist");
                }

                if (VerifyPasswordHash(resetPasswordDto.Password, adminacc.PasswordKey, adminacc.Password))
                {
                    throw new Exception("Cannot use old password");
                }

                byte[] passwordKey = null;
                byte[] passwordHash = null;
                using (var hmac = new HMACSHA512())
                {
                    passwordKey = hmac.Key;
                    passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(resetPasswordDto.Password));
                }
                adminacc.Password = passwordHash;
                adminacc.PasswordKey = passwordKey;
                adminacc.PasswordChanged = true;
                await _dataContext.SaveChangesAsync();

                response.Data = "Password Changed Successfully";
            }
            catch ( Exception ex ) {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"AN ERROR OCCURRED....{ex.Message}");
            }
            return response;

        }
        public async Task<ServiceResponse<string>> LockUserAccount(LockingUserDto lockingUserDto)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var roleofAdmin = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (roleofAdmin.ToLower() != "admin")
                {
                    throw new Exception("Not authorized to lock an account");
                }
                var userDetail = _dataContext.Users.Include("UserAccount").Where(x => x.Email == lockingUserDto.Email).FirstOrDefault();
                if (userDetail == null)
                {
                    throw new Exception("User does not exist");
                }

                userDetail.IsLocked = true;
                await _dataContext.SaveChangesAsync();

                var lockedUser = new LockedUser()
                {
                    UserId = userDetail.Id,
                    Name = $"{userDetail.FirstName} {userDetail.LastName}",
                    Reason = lockingUserDto.Reason,
                    No_of_Accounts = userDetail.UserAccount.Count.ToString(),
                    LockingDate = DateTime.Now,
                    Email = lockingUserDto.Email,
                };
                await _dataContext.LockedUsers.AddAsync(lockedUser);
                await _dataContext.SaveChangesAsync();
                response.Data = "User account locked successfully";
                await _notificationService.CreateLockedUserNotification(userDetail.Id);

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"AN ERROR JUST OCCURED......{ex.Message}");
            }

            return response;
        }  
        public async Task<ServiceResponse<string>> UnlockUserAccount(EmailDto emaildto)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var roleofAdmin = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (roleofAdmin.ToLower() != "admin")
                {
                    throw new Exception("Not authorized to unlock an account");
                }
                var userDetail = _dataContext.Users.Where(x => x.Email == emaildto.Email).FirstOrDefault();
                if (userDetail == null)
                {
                    throw new Exception("User does not exist");
                }
                var lockeduser = await _dataContext.LockedUsers.Where(x => x.UserId == userDetail.Id).FirstOrDefaultAsync();

                if (lockeduser == null)
                {
                    throw new Exception("Account is not locked");
                }

                userDetail.IsLocked = false;
                await _dataContext.SaveChangesAsync();
                 _dataContext.LockedUsers.Remove(lockeduser);


                response.Data = "User account unlocked successfully";
                await _dataContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"AN ERROR JUST OCCURED......{ex.Message}");
            }

            return response;
        }

        public async Task<ServiceResponse<GLAccountView>> CreateGlAccount(GLAccountDTO gLAccount)
        {
            var response = new ServiceResponse<GLAccountView>();
            try
            {
                //authenticate the user creating the GL
                var roleofAdmin = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (roleofAdmin.ToLower() != "admin")
                {
                    throw new Exception("Not authorized to create a GLaccount");
                }
                //get the parameters from the argument
                //add the parameters to the gl Table

                string number = DateTime.Now.ToString("yyMMddHHmmssfff");

                var data = new GLAccount()
                {
                    GLName = gLAccount.GLName.ToUpper(),
                    Currency = gLAccount.Currency.ToUpper(),
                    GLNumber = number,
                    Balance = 0
                };

                //save the changes
                await _dataContext.GLAccounts.AddAsync(data);
                await _dataContext.SaveChangesAsync();
                 //response.Data = "GL Account created successfully"
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"Error occured.....{ex.Message}");
            }
            //return the response 
            return response;

        }
        public async Task<ServiceResponse<List<ListOfLockedUsers>>> GetListOfLockedUsers()
        {
            var response = new ServiceResponse<List<ListOfLockedUsers>>();
            var usersList = new List<ListOfLockedUsers>();
            try
            {
                var roleofAdmin = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (roleofAdmin.ToLower() != "admin")
                {
                    throw new Exception("User is not authorized to view");
                }
                var users = _dataContext.LockedUsers.ToList();
                foreach ( var user in users)
                {
                    var data = new ListOfLockedUsers()
                    {
                        Name = user.Name,
                        Email = user.Email,
                        Reason = user.Reason,
                        Date = user.LockingDate
                    };
                    usersList.Add(data);
                }

                response.Data = usersList;
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"Error occured.....{ex.Message}");
            }
            return response;
        }

        public async Task<ServiceResponse<string>> SetWalletCharge(ChargeorRateDTo chargeorRateDTo)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var roleofAdmin = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (roleofAdmin.ToLower() != "admin")
                {
                    throw new Exception("Cannot perform operation");
                }
                var chargeAccount = await _dataContext.CurrenciesWallets.Where(x => x.Currencies == chargeorRateDTo.Currency).FirstOrDefaultAsync();
                if (chargeAccount != null)
                {
                    chargeAccount.ChargeAmount = chargeorRateDTo.Amount;
                    await _dataContext.SaveChangesAsync();
                }
                response.Data = "Wallet charge update succesful";

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"Error occured.....{ex.Message}");
            }
            return response;
        }
        public async Task<ServiceResponse<string>> SetWalletRate(ChargeorRateDTo chargeorRateDTo)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var roleofAdmin = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (roleofAdmin.ToLower() != "admin")
                {
                    throw new Exception("Cannot perform operation");
                }
                var chargeAccount = await _dataContext.CurrenciesWallets.Where(x => x.Currencies.ToLower() == chargeorRateDTo.Currency.ToLower()).FirstOrDefaultAsync();
                if (chargeAccount != null)
                {
                    chargeAccount.Rate = chargeorRateDTo.Amount;
                    await _dataContext.SaveChangesAsync();
                }
                response.Data = "Wallet rate update succesful";

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"Error occured.....{ex.Message}");
            }
            return response;
        }

        public async Task<ServiceResponse<List<ListOfGLAccounts>>> GetAllGlAccounts()
        {
            var response = new ServiceResponse<List<ListOfGLAccounts>>();
            var glList = new List<ListOfGLAccounts>();
            try
            {
                var roleofAdmin = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (roleofAdmin.ToLower() != "admin")
                {
                    throw new Exception("User is not authorized to view");
                }
                var gls = _dataContext.GLAccounts.ToList();
                foreach (var gl in gls)
                {
                    var data = new ListOfGLAccounts()
                    {
                        Name = gl.GLName,
                        GLAccount = gl.GLNumber,
                        Currency = gl.Currency,
                        Balance = gl.Balance
                    };
                    glList.Add(data);
                }

                response.Data = glList;
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"Error occured.....{ex.Message}");
            }
            return response;
        }
        public async Task<ServiceResponse<List<GLTransactionHistory>>> GetAllGlTransactionsHistory()
        {
            var response = new ServiceResponse<List<GLTransactionHistory>>();
            var glList = new List<GLTransactionHistory>();
            try
            {
                var roleofAdmin = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (roleofAdmin.ToLower() != "admin")
                {
                    throw new Exception("User is not authorized to view");
                }
                var gls = await _dataContext.GLTransactions.ToListAsync();
                foreach (var gl in gls)
                {
                    var data = new GLTransactionHistory()
                    {
                        GLAccount = gl.GlAccount,
                        Narration = gl.Narration,
                        Type = gl.Type,
                        Currency = gl.Currency,
                        Amount = (decimal)gl.Amount,
                        Reference = gl.Reference,
                        Date = (DateTime)gl.Date
                    };
                    glList.Add(data);
                }

                response.Data = glList;
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"Error occured.....{ex.Message}");
            }
            return response;
        }

        public async Task<ServiceResponse<List<GLTransactionHistory>>> GetWalletGlTransactionsHistory(CurrencyObj currencyobj)
        {
            var response = new ServiceResponse<List<GLTransactionHistory>>();
            var glList = new List<GLTransactionHistory>();
            try
            {
                var roleofAdmin = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (roleofAdmin.ToLower() != "admin")
                {
                    throw new Exception("User is not authorized to view");
                }
                var gls = await _dataContext.GLTransactions.Where(x => x.Currency == currencyobj.Currency).ToListAsync();
                foreach (var gl in gls)
                {
                    var data = new GLTransactionHistory()
                    {
                        GLAccount = gl.GlAccount,
                        Narration = gl.Narration,
                        Type = gl.Type,
                        Reference = gl.Reference,
                        Date = (DateTime)gl.Date
                    };
                    glList.Add(data);
                }

                response.Data = glList;
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"Error occured.....{ex.Message}");
            }
            return response;
        }



        public async Task <ServiceResponse<string>> ResetAdminCredentials(ResetAdminDto resetAdminDto)
        {
            var response = new ServiceResponse<string>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedUser = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);

                    if (loggedUser.ToLower() != "admin")
                    {
                        throw new Exception("This admin is not authorized to reset an admin credential");
                    }
                    var adminaccount = await _dataContext.Admins.Where(x => x.Email == resetAdminDto.Email).FirstOrDefaultAsync();
                    if (adminaccount == null)
                    {
                        throw new Exception("Admin account does not exist");
                    }

                    if (VerifyPasswordHash(resetAdminDto.NewPassword, adminaccount.PasswordKey, adminaccount.Password))
                    {
                        throw new Exception("Cannot use old password");
                    }

                    byte[] passwordKey = null;
                    byte[] passwordHash = null;
                    using (var hmac = new HMACSHA512())
                    {
                        passwordKey = hmac.Key;
                        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(resetAdminDto.NewPassword));
                    }
                    adminaccount.Password = passwordHash;
                    adminaccount.PasswordKey = passwordKey;
                    adminaccount.PasswordChanged = false;
                    await _dataContext.SaveChangesAsync();

                    response.Data = $"{resetAdminDto.Email} credentials has been reset!";
                    _logger.LogInformation($"{response.Data}........");
                
                    string subject = "LOGIN CREDENTIALS RESET";
                    string MailBody = "<!DOCKTYPE html>" +
                                            "<html>" +
                                                "<body>" +
                                                $"<h3>Dear admin {adminaccount.Email},</h3>" +
                                                $"<h5>Your Password has been reset, new password is {resetAdminDto.NewPassword}." +
                                                $"Please go ahead to change your password on first login.</h5>" +
                                                $"<h5>Warm regards.</h5>" +
                                                "</body>" +
                                            "</html>";

                  await _mailService.SendLoginDetails(adminaccount.Email, subject, MailBody);
                }

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An Error Occurred...{ex.Message}");
            }
            return response;
            
        }

        public async Task<ServiceResponse<string>> DisableAdminAccount(DisableAdminDto disableAdminDto)
        {
            var response = new ServiceResponse<string>();
            try
            {
                if(_httpContextAccessor.HttpContext != null)
                {
                    var loggedUser = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);

                    if (loggedUser.ToLower() != "admin")
                    {
                        throw new Exception("This admin is not authorized to disables an admin");
                        
                    }
                    var adminaccount = await _dataContext.Admins.Where(x => x.Username == disableAdminDto.Email).FirstOrDefaultAsync();
                    if(adminaccount == null)
                    {
                        throw new Exception("Admin account does not exist");
                    }
                    adminaccount.Disabled = true;
                    await _dataContext.SaveChangesAsync();
                    response.Data = $"{disableAdminDto.Email} account is disabled!";
                }

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An Error Occurred...{ex.Message}");
            }
            return response;
        }
        //View kyc validations of users
    }
}
