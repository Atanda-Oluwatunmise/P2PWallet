﻿using Aspose.Pdf;
using Aspose.Pdf.Operators;
using Azure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using PdfSharpCore;
using PdfSharpCore.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using PageSize = PdfSharpCore.PageSize;

namespace P2PWallet.Services.Services
{
    public class UserServices : IUserServices
    {
        public static User user = new User();
        public static Account account = new Account();
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMailService _mailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<UserServices> _logger;

        public UserServices(DataContext dataContext,ILogger<UserServices> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IMailService mailService, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _mailService = mailService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }


        public string GenerateEmailToken()
        {
            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return token;
        }

        public async Task<bool> EmailAlreadyExists(string emailName)
        {
            return await _dataContext.Users.AnyAsync(x => x.Email == emailName);
        }

        public void CreatePasswordHash(string password, out byte[] passwordKey, out byte[] passwordHash)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordKey = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }


        public async Task<ServiceResponse<string>> VerifyEmail(string verifyemail)
        {
            var response = new ServiceResponse<string>();
            var emailuser = await _dataContext.Users.FirstOrDefaultAsync(x => x.Email == verifyemail);
            try
            {
                var token = GenerateEmailToken();
                var encodedToken = Encoding.UTF8.GetBytes(token);
                var validToken = WebEncoders.Base64UrlEncode(encodedToken);

                string url = $"{_configuration.GetSection("ResetPassword:LoginUrl").Value!}?token={validToken}";
                await _mailService.ResetPasswordEmailAsync(verifyemail, "Email Verification", "<h1>Your email has been verified</h1>",
                    $"<p><a href={url}>Proceed to log in </a>" +
                    $"or paste {url} in your web browser</p>");

                emailuser.VerificationToken = validToken;
                emailuser.UserVerifiedAt = DateTime.Now;

                await _dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
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

                //Adding the new instance in the database
                await _dataContext.Users.AddAsync(newuser);
                await _dataContext.SaveChangesAsync();

                var verifyemail = await VerifyEmail(user.Email);
                if (verifyemail != null)
                {
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
                expires: DateTime.Now.AddMinutes(60),
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

                if (user.VerificationToken == null)
                {
                    throw new Exception("Email not Verified!");
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
                _logger.LogError($"AN ERROR OCCURRED....{ex.Message}");
            }

            return response;
        }


        public async Task<ServiceResponse<List<AccountDetails>>> GetMyAccountNumber()
        {
            var response = new ServiceResponse<List<AccountDetails>>();
            List<AccountDetails> accountDetails = new List<AccountDetails>();
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
                            Username = userAccount.User.Username,
                            FirstName = userAccount.User.FirstName,
                            LastName = userAccount.User.LastName,
                            AccountName = userAccount.User.FirstName + " " + userAccount.User.LastName,
                            AccountNumber = userAccount.AccountNumber,
                            Phonenumber = userAccount.User.PhoneNumber,
                            Email = userAccount.User.Email,
                            Address = userAccount.User.Address,
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

        public async Task<ServiceResponse<List<EditViewModel>>> EditUserInfo(EditViewModel request)
        {
            var response = new ServiceResponse<List<EditViewModel>>();
            List<EditViewModel> updateInfo = new List<EditViewModel>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedinId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var loggedinUser = await _dataContext.Users.Where(x => x.Id == loggedinId).FirstOrDefaultAsync();
                    var verifypinuser = await _dataContext.ImageDetails.Include("UserImageDetail").Where(x => x.ImageUserId == loggedinId).FirstOrDefaultAsync();


                    if (loggedinUser != null)
                    {
                        if (request.ImageFile.FileName != null || request.ImageFile.Length != 0)
                        {
                            var path = Path.Combine(_webHostEnvironment.WebRootPath, "Images/", request.ImageFile.FileName);
                            byte[] FileBytes = File.ReadAllBytes(path);
                            verifypinuser.ImageName = request.ImageFile.FileName;
                            verifypinuser.Image = FileBytes;
                        }

                        loggedinUser.FirstName = request.FirstName;
                        loggedinUser.LastName = request.LastName;
                        loggedinUser.PhoneNumber = request.Phonenumber;
                        loggedinUser.Address = request.Address;

                        _dataContext.Users.Update(loggedinUser);
                        _dataContext.ImageDetails.Update(verifypinuser);
                        await _dataContext.SaveChangesAsync();

                        response.Data = updateInfo;
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

        public async Task<ServiceResponse<string>> SaveImage(ImageViewmodel imageview)
        {
            var response = new ServiceResponse<string>()
;            try
              {
                    var loggedinId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var loggedinUser = await _dataContext.Users.FindAsync(loggedinId);
                    //var dbimageid = _dataContext.ImageDetails.Where( x => x.ImageId == loggedinId);

                    //if (dbimageid != null)
                    //{
                    //    throw new Exception("Cannot save image");
                    //}

                    if (loggedinUser != null) { 

                        foreach (var item in imageview.ImagePath)
                        {
                            if (item.FileName == null || item.FileName.Length == 0)
                            {
                                throw new Exception("Image cannot be empty");
                            }
                            var path = Path.Combine(_webHostEnvironment.WebRootPath, "Images/", item.FileName);

                            using (FileStream stream = new FileStream(path, FileMode.Create))
                            {
                                await item.CopyToAsync(stream);
                                stream.Close();
                            }
                            byte[] FileBytes = File.ReadAllBytes(path);

                            var imagedetails = new ImageDetail
                            {
                                ImageUserId = loggedinUser.Id,
                                ImageName = item.FileName,
                                Image = FileBytes

                            };

                            await _dataContext.ImageDetails.AddAsync(imagedetails);
                            await _dataContext.SaveChangesAsync();

                            response.Data = "Image saved successfully";
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

        public async Task<DisplayViewmodel> DisplayImage()
        {
            DisplayViewmodel imageview = new DisplayViewmodel();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var userid = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var loggedinuser = await _dataContext.ImageDetails.Where(x => x.ImageUserId == userid).FirstOrDefaultAsync();

                    if (loggedinuser != null)
                    {
                        byte[] image = loggedinuser.Image;
                        imageview = new DisplayViewmodel
                        {
                            ImagePath = image
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return imageview;
        }

        public async Task<bool> VerifyImageStatus()
        {
            try
            {
                if(_httpContextAccessor.HttpContext != null)
                {
                    var userId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var userImg = await _dataContext.ImageDetails.Where(x => x.ImageUserId == userId).FirstOrDefaultAsync();
                    if (userImg != null) { return true; }
                    else { return false; }
                }
            }
            catch (Exception ex)
            {
            }
            return true;
        }

        public async Task<ServiceResponse<string>> DeleteImage()
        {
            var response = new ServiceResponse<string>();
            ImageDetail imagedetails = new ImageDetail();
            try
            {
                if(_httpContextAccessor.HttpContext != null)
                {
                    var loggedinuserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var loggedinuser = await _dataContext.ImageDetails.AsNoTracking().Where(x => x.ImageUserId == loggedinuserId).FirstOrDefaultAsync();

                    if (loggedinuser == null)
                    {
                        throw new Exception("User is not authorized");
                    }

                    imagedetails = new ImageDetail()
                    {
                        Id = loggedinuser.Id,
                        ImageUserId = loggedinuser.ImageUserId,
                        ImageName = loggedinuser.ImageName,
                        Image = loggedinuser.Image
                    };

                    _dataContext.ImageDetails.Remove(imagedetails);
                    await _dataContext.SaveChangesAsync();

                    response.Data = "Image Successfully deleted";
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
        }

        public async Task<ActionResult> GeneratePdf(ControllerBase controller)
        {
            var document = new PdfDocument();
            string htmlcontent = "<div style='width:100%;>";
            htmlcontent += "<img style='height:60px' src='"  + "'   />";
            htmlcontent += "<h2 style= 'display: flex'>Transactions History </h2>";
            htmlcontent += "<h2>Welcome to Nihira Techiees</h2>";

            htmlcontent += "<table style ='width:100%; border: 1px solid #000'>";
            htmlcontent += "<thead style='font-weight:bold'>";
            htmlcontent += "<tr>";
            htmlcontent += "<td style='border:1px solid #000'> Product Code </td>";
            htmlcontent += "<td style='border:1px solid #000'> Description </td>";
            htmlcontent += "<td style='border:1px solid #000'>Qty</td>";
            htmlcontent += "<td style='border:1px solid #000'>Price</td >";
            htmlcontent += "<td style='border:1px solid #000'>Total</td>";
            htmlcontent += "</tr>";
            htmlcontent += "</thead >";


            htmlcontent += "<tbody>";

            htmlcontent += "</tbody>";

            htmlcontent += "</table>";
            htmlcontent += "</div>";

            htmlcontent += "<div style='text-align:right'>";
            htmlcontent += "<h1> Summary Info </h1>";
            htmlcontent += "<table style='border:1px solid #000;float:right' >";
            htmlcontent += "<tr>";
            htmlcontent += "<td style='border:1px solid #000'> Summary Total </td>";
            htmlcontent += "<td style='border:1px solid #000'> Summary Tax </td>";
            htmlcontent += "<td style='border:1px solid #000'> Summary NetTotal </td>";
            htmlcontent += "</tr>";

            htmlcontent += "</table>";
            htmlcontent += "</div>";

            htmlcontent += "</div>";


            //generate pdf content
            PdfGenerator.AddPdfPages(document, htmlcontent, PageSize.A4);
            byte[]? response = null;
           

            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms);
                response = ms.ToArray();
            }   

            string FileName = "Transaction Statement .pdf";

            return controller.File(response, "application/pdf", FileName);
        }
    }
}

