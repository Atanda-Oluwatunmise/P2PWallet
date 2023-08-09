using Aspose.Cells;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NPOI.POIFS.FileSystem;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;
using System;
using System.Net;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using NPOI.HPSF;
using Grpc.Core;
using FileMode = System.IO.FileMode;
using Aspose.Pdf;
using Microsoft.AspNetCore.SignalR;
using P2PWallet.Services.Hubs;
using SixLabors.ImageSharp;

namespace P2PWallet.Services.Services
{
    public class KycProcessService : IKycProcessService
    {
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMailService _mailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<KycProcessService> _logger;
        private readonly IAdminService _adminService;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly INotificationService _notificationService;

        public KycProcessService(DataContext dataContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment, ILogger<KycProcessService> logger, IAdminService adminService, IHubContext<NotificationHub> hub, INotificationService notificationService)
        {
            _dataContext = dataContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _adminService = adminService;
            _hub = hub;
            _notificationService = notificationService;
        }

        //add documents to documentListTable
        public async Task<ServiceResponse<string>> AddToKycDocumentList(KycDocumentsView kycDocumentsdto)
        {
            var response = new ServiceResponse<string>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduser = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                    if (loggeduser.ToLower() != "admin")
                        throw new Exception("User Unauthorized");

                    var data = new KycDocument()
                    {
                        DocumentName = kycDocumentsdto.Name.ToLower(),
                    };

                    await _dataContext.KycDocuments.AddAsync(data);
                    await _dataContext.SaveChangesAsync();

                    response.Data = "New KYC requirement added successfully";
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;

                _logger.LogError($"An error just happend {ex.Message}");
            }
            return response;
        }
        //get list of documents to upload for users
        public async Task<ServiceResponse<List<KycDocumentsView>>> GetDocumentsRequired()
        {
            var response = new ServiceResponse<List<KycDocumentsView>>();
            var documentList = new List<KycDocumentsView>();
            try
            {
                var loggeduser = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var useraccount = await _dataContext.Users.Where(x => x.Id == loggeduser).FirstOrDefaultAsync();
                if (useraccount == null)
                    throw new Exception("User does not exist");

                var documents = await _dataContext.KycDocuments.ToListAsync();
                foreach (var document in documents)
                {
                    var data = new KycDocumentsView()
                    {
                        Name = document.DocumentName
                    };
                    documentList.Add(data);
                }
                response.Data = documentList;
            }

            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An error just occurred {ex.Message}");

            }
            return response;
        }
        //user upload documents
        public async Task<ServiceResponse<string>> UploadDocuments(KycDto kycDto)
        {
            /*
             1- Pending
             2- Accepted
             3- Rejected
             */
            var response = new ServiceResponse<string>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduser = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var useraccount = await _dataContext.Users.Where(x => x.Id == loggeduser).FirstOrDefaultAsync();
                    if (useraccount == null)
                        throw new Exception("User does not exist");


                    //fetch the fullpath from appsettings
                    var folderpath = _configuration.GetSection("FolderPath:KycFolderPath").Value!;
                    //string wwwPath = _webHostEnvironment.WebRootPath;
                    //string path = Path.Combine(wwwPath, folderpath);
                    var userUniqueNo = $"User_{useraccount.Username}";
                    string userfolderPath = Path.Combine(folderpath, userUniqueNo);
                    if (!Directory.Exists(folderpath))
                        Directory.CreateDirectory(folderpath);

                    //create a folder unique to each user
                    if (Directory.Exists(folderpath))
                        Directory.CreateDirectory(userfolderPath);

                    //save the files to the folder
                    if (Directory.Exists(userfolderPath))
                        foreach (var item in kycDto.UploadedImage)
                        {
                            var documentList = await _dataContext.KycDocuments.Where(x => x.DocumentName.ToLower() == Path.GetFileNameWithoutExtension(item.FileName).ToLower()).FirstOrDefaultAsync();
                            if (documentList == null)
                                throw new Exception("Wrong file name");

                            var filepath = Path.Combine(userfolderPath, item.FileName);
                            using (FileStream fs = new FileStream(filepath, FileMode.Create))
                            {
                                await item.CopyToAsync(fs);
                                fs.Close();
                            }
                            //save the details to the db
                            var data = new KycDocumentUpload()
                            {
                                DocumentId = documentList.Id,
                                UserId = useraccount.Id,
                                FileName = item.FileName,
                                Status = 1,
                            };

                            await _dataContext.KycDocumentUploads.AddAsync(data);
                            await _dataContext.SaveChangesAsync();
                        }
                    var pendinguser = new PendingUser()
                    {
                        FullName = useraccount.Username,
                        Pending = true
                    };
                    await _dataContext.PendingUsers.AddAsync(pendinguser);
                    await _dataContext.SaveChangesAsync();

                    response.Data = @"Verification documents uploaded successfully,
                                     It will be treated by an Admin. ";
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An error just occurred {ex.Message}");
            }
            return response;
        }

        //admin checks the documents; accepts or rejects
        public async Task<ServiceResponse<List<PendingUsersView>>> GetListOfPendingUsers()
        {
            var response = new ServiceResponse<List<PendingUsersView>>();
            var userslist = new List<PendingUsersView>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduser = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                    if (loggeduser.ToLower() != "admin")
                        throw new Exception("User Unauthorized");

                    var pendingusers = await _dataContext.PendingUsers.Where(x => x.Pending == true).ToListAsync();

                    foreach (var a in pendingusers)
                    {
                        var data = new PendingUsersView()
                        {
                            Username = a.FullName
                        };
                        userslist.Add(data);
                    }
                    response.Data = userslist;
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An error just occurred {ex.Message}");
            }
            return response;
        }

        public async Task<ServiceResponse<List<KycUserDetails>>> GetKycDetailsForUser(KycDocumentsView kycDocumentsView)
        {
            var response = new ServiceResponse<List<KycUserDetails>>();
            var responseList = new List<KycUserDetails>();
            int counter = -1;

            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduser = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                    if (loggeduser.ToLower() != "admin")
                        throw new Exception("User Unauthorized");
                    var user = await _dataContext.Users.Where(x => x.Username == kycDocumentsView.Name).FirstOrDefaultAsync();
                    if (user == null)
                        throw new Exception("User does not exist");

                    var userkycDetails = await _dataContext.KycDocumentUploads.Where(x => x.UserId == user.Id && x.Status == 1 || x.UserId == user.Id && x.Status == 3).ToListAsync();

                    //var userkycDoc = await _dataContext.KycDocumentUploads.Where(x => x.UserId == user.Id).ToListAsync();
                    //var details = userkycDetails[counter];
                    if (userkycDetails == null)
                        throw new Exception("User has no documents uploaded");

                    var noOfDocuments = await _dataContext.KycDocuments.ToListAsync();

                    if (noOfDocuments.Count == userkycDetails.Count)
                    {
                        string folderpath = _configuration.GetSection("FolderPath:KycFolderPath").Value!;
                        string[] dir = Directory.GetDirectories(folderpath);
                        foreach (string dirpath in dir)
                        {
                            if (dirpath.Contains(user.Username))
                            {
                                string[] filePaths = Directory.GetFiles(dirpath);
                                    foreach (string filePath in filePaths)
                                    {
                                    var n = counter += 1;


                                    var textBytes = Encoding.UTF8.GetBytes(filePath);
                                    //var uri = new Uri("government id.jpg");
                                            var data = new KycUserDetails()
                                            {
                                                ImagePath = File.ReadAllBytes(filePath),
                                                ImageName = userkycDetails[n].FileName
                                            };
                                            responseList.Add(data);
                                    }
                            }
                        }
                        response.Data = responseList;
                    }

                    if (userkycDetails.Count < noOfDocuments.Count)
                    {
                        string folderpath = _configuration.GetSection("FolderPath:KycFolderPath").Value!;
                        string[] dir = Directory.GetDirectories(folderpath);
                        foreach (string dirpath in dir)
                        {
                            if (dirpath.Contains(user.Username))
                            {
                                string[] filePaths = Directory.GetFiles(dirpath, userkycDetails[0].FileName);
                                foreach (string filePath in filePaths)
                                {
                                    var textBytes = Encoding.UTF8.GetBytes(filePath);
                                    var uri = new Uri(filePath);
                                    var data = new KycUserDetails()
                                    {
                                        ImagePath = File.ReadAllBytes(filePath),
                                        ImageName = userkycDetails[0].FileName
                                    };
                                    responseList.Add(data);
                                    response.Data = responseList;

                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An error just occurred {ex.Message}");
            }
            return response;
        }

        //admin accepts document
        public async Task<ServiceResponse<string>> ApproveDocument(KycDocumentsView kycDocumentsView)
        {
            var response = new ServiceResponse<string>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduser = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                    if (loggeduser.ToLower() != "admin")
                        throw new Exception("User Unauthorized");

                    var imagedetail = await _dataContext.KycDocumentUploads.Where(x => x.FileName.ToLower() == kycDocumentsView.Name.ToLower()).FirstOrDefaultAsync();
                    if (imagedetail == null)
                        throw new Exception("File name does not exist");

                    imagedetail.Status = 2;
                    await _dataContext.SaveChangesAsync();

                    response.Data = "File Approved";
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An error just occurred {ex.Message}");

            }
            return response;
        }

        //admin rejects document
        public async Task<ServiceResponse<string>> RejectDocument(KycDocumentsView kycDocumentsView)
        {
            var response = new ServiceResponse<string>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduser = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                    if (loggeduser.ToLower() != "admin")
                        throw new Exception("User Unauthorized");

                    var imagedetail = await _dataContext.KycDocumentUploads.Where(x => x.FileName.ToLower() == kycDocumentsView.Name.ToLower()).FirstOrDefaultAsync();
                    var useraccount = await _dataContext.Users.Where(x => x.Id == imagedetail.UserId).FirstOrDefaultAsync();
                    if (imagedetail == null)
                        throw new Exception("File name does not exist");
                    if (useraccount == null)
                        throw new Exception("User does not exist");

                    imagedetail.Status = 3;
                    await _dataContext.SaveChangesAsync();

                    response.Data = "File Rejected";

                    //send a notification message, send name of file and username of user
                    await _notificationService.SendKycNotifications(useraccount.Username, imagedetail.FileName);
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An error just occurred {ex.Message}");

            }
            return response;
        }

        //reupload document

        //set verification status
        public async Task<ServiceResponse<string>> UpgradeUserAccount()
        {
            var response = new ServiceResponse<string>();
            bool status = false; 
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduser = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var useraccount = await _dataContext.Users.Where(x => x.Id == loggeduser).FirstOrDefaultAsync();
                    var pendinguser = await _dataContext.PendingUsers.Where(x => x.FullName == useraccount.Username).FirstOrDefaultAsync();

                    if (useraccount == null) throw new Exception("User does not exist");

                    //check if the documents count is the same as the table doc count;
                    var documentList = await _dataContext.KycDocuments.ToListAsync();
                    var userdocuments = await _dataContext.KycDocumentUploads.Where(x => x.UserId == useraccount.Id).ToListAsync();

                    if (userdocuments.Count != documentList.Count) throw new Exception("Incomplete documents required");

                    //check if all docs are set to approved, compare with the table as well
                    var docStatusCode = await _dataContext.DocumentStatusCodes.Where(x => x.StatusMessage.ToLower() == "approved").FirstOrDefaultAsync();

                    foreach(var userdoc in userdocuments)
                    {
                        if(userdoc.Status == docStatusCode.StatusCode)
                        {
                            status = true;
                        }
                        if(userdoc.Status != docStatusCode.StatusCode)
                        {
                            status = false;
                        }
                    }

                    //set if true, and send notification to user
                    if (status == true)
                    {
                        useraccount.KycVerified = true;
                        await _dataContext.SaveChangesAsync();
                        _dataContext.Remove(pendinguser);
                        await _dataContext.SaveChangesAsync();
                        response.Data = "User is Verified";
                    }
                    //send notification
                    //await _notificationService.SendKycNotifications(useraccount.Username, response.Data);


                    //other wise set to false and do not send notifcation
                    if (status == false)
                    {
                        useraccount.KycVerified = false;
                        await _dataContext.SaveChangesAsync();
                        response.Status = false;
                        response.Data = "Approval is pending";
                    }
                        //send notification, send response.data and username of user
                        //await _notificationService.SendKycNotifications(useraccount.Username, response.Data);
                }

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An error just occurred {ex.Message}");
            }
            return response;
        }
    }
}
