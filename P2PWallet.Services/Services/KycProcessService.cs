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

        public async Task<ServiceResponse<List<KycDocumentsView>>> ListsofUnUploadedandRejectedDocuments()
        {
            var response = new ServiceResponse<List<KycDocumentsView>>();
            var tempdocs = new List<KycDocs>();
            var tempdocumentList = new List<KycDocs>();
            var documentList = new List<KycDocumentsView>();
            try
            {
                var loggeduser = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var useraccount = await _dataContext.Users.Where(x => x.Id == loggeduser).FirstOrDefaultAsync();
                if (useraccount == null)
                    throw new Exception("User does not exist");

                var requiredDocs = await _dataContext.KycDocuments.ToListAsync();
                //var userdocs = await _dataContext.KycDocumentUploads.Where(x => x.UserId == useraccount.Id && x.FileName.Contains()).ToListAsync();

                //compare the documents

                foreach (var doc in requiredDocs)
                {
                    var userdocs = await _dataContext.KycDocumentUploads.Where(x => x.UserId == useraccount.Id && x.FileName.Contains(doc.DocumentName)).FirstOrDefaultAsync();
                    if (userdocs != null)
                    {
                        var data = new KycDocs()
                        {
                            Name = doc.DocumentName,
                            Status = userdocs.Status
                        };
                        tempdocumentList.Add(data);
                    }

                    var usertwodocs = await _dataContext.KycDocumentUploads.Where(x => x.UserId == useraccount.Id && (x.FileName.Contains(doc.DocumentName) && x.Status==3)).FirstOrDefaultAsync();
                    if (usertwodocs != null) {
                        if (userdocs != usertwodocs)
                        {
                            var newdata = new KycDocs()
                            {
                                Name = doc.DocumentName,
                                Status = usertwodocs.Status,
                            };
                        tempdocumentList.Add(newdata);
                        }
                    }
                }
                foreach( var doc in tempdocumentList)
                {
                    if(doc.Status != 1 && doc.Status != 2)
                    {
                        var data = new KycDocumentsView()
                        {
                            Name = doc.Name
                        };
                        documentList.Add(data);
                    }
                }

                foreach(var doc in requiredDocs)
                {
                    var userdocs = await _dataContext.KycDocumentUploads.Where(x => x.UserId == useraccount.Id && x.FileName.Contains(doc.DocumentName)).FirstOrDefaultAsync();
                    if (userdocs == null)
                    {
                        var data = new KycDocumentsView()
                        {
                            Name = doc.DocumentName
                        };
                        documentList.Add(data);
                    }
                }
                if (documentList.Count() == 0)
                {
                    response.Status = false;
                    response.StatusMessage = "Empty List";
                }
                response.Data = documentList;
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An error occurred {ex.Message}");
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

                    var userUniqueNo = $"User_{useraccount.Username}";
                    string userfolderPath = Path.Combine(folderpath, userUniqueNo);
                    if (!Directory.Exists(folderpath))
                        Directory.CreateDirectory(folderpath);

                    //create a folder unique to each user
                    if (Directory.Exists(folderpath))
                        Directory.CreateDirectory(userfolderPath);

                    //save the files to the folder
                    if (Directory.Exists(userfolderPath))
                    {
                        foreach (var item in kycDto.UploadedImage)
                        {
                            var documentList = await _dataContext.KycDocuments.Where(x => x.DocumentName.ToLower() == Path.GetFileNameWithoutExtension(item.FileName).ToLower()).FirstOrDefaultAsync();
                            if (documentList == null)
                                throw new Exception("Wrong file name");

                            var duplicatedoc = await _dataContext.KycDocumentUploads.Where(x => x.UserId == useraccount.Id && x.FileName == item.FileName).FirstOrDefaultAsync();
                            if (duplicatedoc != null)
                            {
                                string[] filetodelete = Directory.GetFiles(userfolderPath);
                                foreach (var deletefile in filetodelete)
                                {
                                    var filepath = Path.Combine(userfolderPath, item.FileName);

                                    if (deletefile == filepath)
                                    {
                                        File.Delete(deletefile);
                                    }
                                }

                                var newfilepath = Path.Combine(userfolderPath, item.FileName);
                                using (FileStream fs = new FileStream(newfilepath, FileMode.Create))
                                {
                                    await item.CopyToAsync(fs);
                                    fs.Close();
                                }
                                duplicatedoc.Status = 1;
                                duplicatedoc.ReasonForRejection = "";
                                _dataContext.KycDocumentUploads.Update(duplicatedoc);
                                await _dataContext.SaveChangesAsync();
                            }

                            else
                            {
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

                                var useralreadyPending = await _dataContext.PendingUsers.Where(x => x.FullName.ToLower() == useraccount.Username.ToLower()).FirstOrDefaultAsync();

                                if (useralreadyPending == null)
                                {
                                    var pendinguser = new PendingUser()
                                    {
                                        FullName = useraccount.Username,
                                        Pending = true
                                    };
                                    await _dataContext.PendingUsers.AddAsync(pendinguser);
                                    await _dataContext.SaveChangesAsync();
                                }
                            }

                            response.Data = @"Verification documents uploaded successfully,
                                     It will be treated by an Admin. ";
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

                    if(pendingusers.Count == 0)
                    {
                        response.Status = false;
                        response.StatusMessage = "Empty list";
                    }

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

        public async Task<ServiceResponse<List<KycUserDetails>>> GetKycDetailsForUser(NewKycDocumentsView kycDocumentsView)
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
            var tempList = new List<string>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduser = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                    if (loggeduser.ToLower() != "admin")
                        throw new Exception("User Unauthorized");

                    var useracc = await _dataContext.Users.Where(x => x.Username.ToLower() == kycDocumentsView.Username).FirstOrDefaultAsync();

                    var imagedetail = await _dataContext.KycDocumentUploads.Where(x => x.UserId == useracc.Id && x.FileName.ToLower() == kycDocumentsView.Name.ToLower()).FirstOrDefaultAsync();
                    if (imagedetail == null)
                        throw new Exception("File name does not exist");

                    imagedetail.Status = 2;
                    imagedetail.ReasonForRejection = "";
                    await _dataContext.SaveChangesAsync();

                    response.Data = "File Approved";
                    var userdocs = await _dataContext.KycDocumentUploads.Where(x => x.UserId == useracc.Id).ToListAsync();
                    foreach( var item in userdocs)
                    {
                        if(item.Status == 2)
                        {
                            tempList.Add("doc");
                        }
                    }
                    if (userdocs.Count() == tempList.Count())
                    {
                        var pendinguser = await _dataContext.PendingUsers.Where(x => x.FullName == useracc.Username).FirstOrDefaultAsync();
                        _dataContext.PendingUsers.Remove(pendinguser);
                        await _dataContext.SaveChangesAsync();
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

        //admin rejects document
        public async Task<ServiceResponse<string>> RejectDocument(RejectDocsDto rejectDocsDto)
        {
            var response = new ServiceResponse<string>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduser = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                    if (loggeduser.ToLower() != "admin")
                        throw new Exception("User Unauthorized");
                    var useracc = await _dataContext.Users.Where(x => x.Username.ToLower() == rejectDocsDto.Username).FirstOrDefaultAsync();

                    var imagedetail = await _dataContext.KycDocumentUploads.Where(x => x.UserId == useracc.Id && x.FileName.ToLower() == rejectDocsDto.Name.ToLower()).FirstOrDefaultAsync();
                    var useraccount = await _dataContext.Users.Where(x => x.Id == imagedetail.UserId).FirstOrDefaultAsync();
                    if (imagedetail == null)
                        throw new Exception("File name does not exist");
                    if (useraccount == null)
                        throw new Exception("User does not exist");

                    imagedetail.Status = 3;
                    imagedetail.ReasonForRejection = rejectDocsDto.Reason.ToLower();
                    await _dataContext.SaveChangesAsync();

                    response.Data = "File Rejected";

                    //send a notification message, send name of file and username of user
                    await _notificationService.CreateKycNotification(useraccount.Id, useraccount.Username, imagedetail.FileName, rejectDocsDto.Reason.ToLower());
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
                        //_dataContext.Remove(pendinguser);
                        //await _dataContext.SaveChangesAsync();
                        response.Data = "User is Kyc Verified";
                        await _notificationService.CreateVerifiedUserKycNotification(useraccount.Id, useraccount.Username);
                    }


                    //other wise set to false and do not send notifcation
                    if (status == false)
                    {
                        useraccount.KycVerified = false;
                        await _dataContext.SaveChangesAsync();
                        response.Status = false;
                        response.Data = "Approval is pending";
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
    }
}
