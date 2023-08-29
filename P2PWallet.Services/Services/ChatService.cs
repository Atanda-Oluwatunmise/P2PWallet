using Aspose.Cells;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Octokit;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Hubs;
using P2PWallet.Services.Interface;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TableDependency.SqlClient.Base.Messages;
using User = P2PWallet.Models.Models.Entities.User;

namespace P2PWallet.Services.Services
{
    public class ChatService:IChatService
    {
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMailService _mailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ChatService> _logger;
        private readonly IAdminService _adminService;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly INotificationService _notificationService;
        public ChatService(DataContext dataContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<ChatService> logger, IHubContext<NotificationHub> hub) 
        {
            _dataContext = dataContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _hub = hub;
            _logger = logger;
        }

        public async Task SendMessageToAdmin(ChatDto chatDto)
        {
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduser = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var useraccount = await _dataContext.Users.Where(x => x.Id == loggeduser && x.Username.ToLower() == chatDto.Username.ToLower()).FirstOrDefaultAsync();

                    if (useraccount == null)
                        _logger.LogInformation("User does not exist");

                    var chatdate = DateTime.Now;
                    string time = chatdate.ToString("hh:mm tt");
                    await _hub.Clients.All.SendAsync("ReceiveMessage", chatDto.Username, chatDto.Message, time);

                    if (useraccount != null)
                    {
                        var data = new Chat()
                        {
                            SenderUserId = useraccount.Id,
                            SenderUsername = useraccount.Username,
                            ReceiverUsername = "admin",
                            Message = chatDto.Message,
                            DateofChat = DateTime.Now,
                        };
                        _dataContext.Chats.Add(data);
                        await _dataContext.SaveChangesAsync();
                    }                     
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex.StackTrace}....{ex.Message}");
            }
        }  
        
        public async Task SendMessageFromAdmin(ChatDto chatDto)
        {
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedadmin = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                    if (loggedadmin.ToLower() != "admin")
                        _logger.LogInformation("User not authorized");

                    var chatdate = DateTime.Now;
                    string time = chatdate.ToString("hh:mm tt");
                    await _hub.Clients.All.SendAsync("ReceiveAdminMessage", chatDto.Username, chatDto.Message, time);
                    
                    if (loggedadmin != null)
                    {
                        var data = new Chat()
                        {
                            //i should get the userame of the user
                            //ReceiverUserId = useraccount.Id,
                            SenderUsername = chatDto.Username,
                            ReceiverUsername = "user",
                            Message = chatDto.Message,
                            DateofChat = DateTime.Now,
                        };
                        _dataContext.Chats.Add(data);
                        await _dataContext.SaveChangesAsync();
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex.StackTrace}....{ex.Message}");
            }
        }

        //retrieve old chats for the admin
        public async Task<ServiceResponse<List<ChatView>>> RetrieveMessagesForAdmin(GetMessagesDto getMessagesDto)
        {
            var response = new ServiceResponse<List<ChatView>>();
            var chatlist = new List<ChatView>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                        var loggedadmin = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                        if (loggedadmin.ToLower() == getMessagesDto.SenderUserName.ToLower())
                        {
                            var chatslists = await _dataContext.Chats.Where(x => x.SenderUsername.ToLower() == getMessagesDto.SenderUserName.ToLower() && x.ReceiverUsername .ToLower() == getMessagesDto.ReceiverUserName.ToLower()).ToListAsync();

                            var secondchatlist = await _dataContext.Chats.Where(x => x.ReceiverUsername.ToLower() == getMessagesDto.SenderUserName.ToLower() && x.SenderUsername.ToLower() == getMessagesDto.ReceiverUserName.ToLower()).ToListAsync();

                            foreach (var chat in chatslists) {
                                var data = new ChatView()
                                {
                                    SenderUserName = chat.SenderUsername,
                                    ReceiverUserName = chat.ReceiverUsername,
                                    Chat = chat.Message,
                                    ChatsDate = chat.DateofChat.ToString("hh:mm tt"),
                                    Date = chat.DateofChat

                                };
                                chatlist.Add(data);
                            } 
                            
                            foreach (var chat in secondchatlist) {
                                var data = new ChatView()
                                {
                                    SenderUserName = chat.SenderUsername,
                                    ReceiverUserName = chat.ReceiverUsername,
                                    Chat = chat.Message,
                                    ChatsDate = chat.DateofChat.ToString("hh:mm tt"),
                                    Date= chat.DateofChat
                                };
                                chatlist.Add(data);
                            }
                            response.Data = chatlist.OrderBy(x => x.Date).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"{ex.StackTrace}....{ex.Message}");
            }
            return response;
        }


        //retrieve old chats for the user
        public async Task<ServiceResponse<List<ChatView>>> RetrieveMessagesForUser(GetMessagesDto getMessagesDto)
        {
            var response = new ServiceResponse<List<ChatView>>();
            var chatlist = new List<ChatView>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedinuser = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var useracc = await _dataContext.Users.Where(x => x.Id == loggedinuser).FirstOrDefaultAsync();

                    if (useracc.Username.ToLower() == getMessagesDto.SenderUserName.ToLower())
                    {
                        var chatslists = await _dataContext.Chats.Where(x => x.SenderUsername.ToLower() == getMessagesDto.SenderUserName.ToLower() && x.ReceiverUsername.ToLower() == getMessagesDto.ReceiverUserName.ToLower()).ToListAsync();

                        var secondchatlist = await _dataContext.Chats.Where(x => x.ReceiverUsername.ToLower() == getMessagesDto.SenderUserName.ToLower() && x.SenderUsername.ToLower() == getMessagesDto.ReceiverUserName.ToLower()).ToListAsync();

                        foreach (var chat in chatslists)
                        {
                            var data = new ChatView()
                            {
                                SenderUserName = chat.SenderUsername,
                                ReceiverUserName = chat.ReceiverUsername,
                                Chat = chat.Message,
                                ChatsDate = chat.DateofChat.ToString("hh:mm tt"),
                                Date = chat.DateofChat

                            };
                            chatlist.Add(data);
                        }

                        foreach (var chat in secondchatlist)
                        {
                            var data = new ChatView()
                            {
                                SenderUserName = chat.SenderUsername,
                                ReceiverUserName = chat.ReceiverUsername,
                                Chat = chat.Message,
                                ChatsDate = chat.DateofChat.ToString("hh:mm tt"),
                                Date = chat.DateofChat
                            };
                            chatlist.Add(data);
                        }
                        response.Data = chatlist.OrderBy(x => x.Date).ToList();

                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"{ex.StackTrace}....{ex.Message}");
            }
            return response;
        }

        //user to user chat
        //find the user
        public async Task<ServiceResponse<SearchUserViewmodel>> FindUser(SearchDto userObj)
        {
            var response = new ServiceResponse<SearchUserViewmodel>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedinuser = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var useracc = await _dataContext.Users.Where(x => x.Id == loggedinuser).FirstOrDefaultAsync();
                    var usertofind = await _dataContext.Users.Where(x => x.PhoneNumber == userObj.UserSearch
                                                                            || x.Username == userObj.UserSearch
                                                                            || x.Email == userObj.UserSearch).FirstOrDefaultAsync();
                    if (useracc == null)
                        throw new Exception("User is not Authorized");

                    if (usertofind == null)
                        throw new Exception("User does not exist");
                    //display the username and picture

                    var founduserImage = await _dataContext.ImageDetails.Where(x => x.ImageUserId == usertofind.Id).FirstOrDefaultAsync();

                    if (founduserImage != null)
                    {

                        byte[] image = founduserImage.Image;
                        var userview = new SearchUserViewmodel()
                        {
                            Username = usertofind.Username,
                            ImagePath = image
                        };
                        response.Data = userview;
                    }

                    if (founduserImage == null)
                    {
                        var userview = new SearchUserViewmodel()
                        {
                            Username = usertofind.Username,
                            ImagePath = null
                        };
                        response.Data = userview;
                    }

                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"{ex.StackTrace}....{ex.Message}");
            }
            return response;
        }

        //on click, open the chat box, with empty for new chat or chatbox filled with messages
        public async Task<ServiceResponse<List<ChatView>>> StartChatting(GetMessagesDto getMessagesDto)
        {
            var startchatted = await _dataContext.StartedChats.Where(x => x.StartedChatWith.ToLower() == getMessagesDto.ReceiverUserName).FirstOrDefaultAsync();

            var response = await RetrieveMessagesForUser(getMessagesDto);
            if (startchatted == null)
            {
                var data = new StartedChat()
                {
                    StartedChatWith = getMessagesDto.ReceiverUserName,
                    ReceivedChatFrom = getMessagesDto.SenderUserName,
                    HasStarted = true,
                };
                await _dataContext.StartedChats.AddAsync(data);
                await _dataContext.SaveChangesAsync();

            }
                return response; 
        }
        //once clicked before, remember it and leave it at the side
        public async Task<ServiceResponse<List<SearchUserViewmodel>>> ListOfStartedChats(GetMessagesDto getMessagesDto)
        {
            var response = new ServiceResponse<List<SearchUserViewmodel>>();
            var startedchats = new List<SearchUserViewmodel>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedinuser = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var useracc = await _dataContext.Users.Where(x => x.Id == loggedinuser).FirstOrDefaultAsync();

                    if (useracc == null)
                        throw new Exception("User is not authorized");

                    var userschatted = await _dataContext.StartedChats.Where(x => x.ReceivedChatFrom.ToLower() == getMessagesDto.SenderUserName).ToListAsync();


                    foreach(var user in userschatted)
                    {
                        var chatteduseracc = await _dataContext.Users.Where(x => x.Username.ToLower() == user.StartedChatWith.ToLower()).FirstOrDefaultAsync();
                        if (chatteduseracc != null)
                        {
                            var founduserImage = await _dataContext.ImageDetails.Where(x => x.ImageUserId == chatteduseracc.Id).FirstOrDefaultAsync();

                            if (chatteduseracc != null && founduserImage != null)
                            {

                                byte[] image = founduserImage.Image;
                                var data = new SearchUserViewmodel()
                                {
                                    Username = chatteduseracc.Username,
                                    ImagePath = image
                                };
                                startedchats.Add(data);

                            }

                            if (chatteduseracc != null && founduserImage == null)
                            {
                                var data = new SearchUserViewmodel()
                                {
                                    Username = chatteduseracc.Username,
                                    ImagePath = null
                                };
                                startedchats.Add(data);
                            }
                        }
                        var adminacc = await _dataContext.SuperAdmins.Where(x => x.Name == user.StartedChatWith).FirstOrDefaultAsync();
                        
                        if (chatteduseracc == null && adminacc != null)
                        {
                            var data = new SearchUserViewmodel()
                            {
                                Username = adminacc.Name,
                                ImagePath = null
                            };
                            startedchats.Add(data);
                        }
                    }
                    response.Data = startedchats;

                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"{ex.StackTrace}....{ex.Message}");
            }
            return response;
        }

        public async Task<bool> EmailAlreadyExists(string emailName)
        {
            return await _dataContext.Users.AnyAsync(x => x.Email == emailName);
        }
        public async Task<bool> UserAlreadyExists(string userName)
        {
            return await _dataContext.Users.AnyAsync(x => x.Username == userName);
        }
        

        public string AutogeneratePassword(int passwordLength)
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
        public async Task<ServiceResponse<string>> VerifyUserEmail(EmailDto emailDto)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var useracc = await _dataContext.Users.Where(x => x.Email.ToLower() == emailDto.Email.ToLower()).FirstOrDefaultAsync();
                if (!await UserAlreadyExists(emailDto.Email) || !await EmailAlreadyExists(emailDto.Email))
                {
                    throw new Exception("User does not exist");
                }
                int passwordLength = 6;
                var otp = AutogeneratePassword(passwordLength);
                //save the otp somewhere
                useracc.UserOtp = otp;
                await _dataContext.SaveChangesAsync();


                string subject = "OTP DETAILS";
                string MailBody = "<!DOCKTYPE html>" +
                                        "<html>" +
                                            "<body>" +
                                            $"<h3>Dear {useracc.Username},</h3>" +
                                            $"<h5>Your verification code is: {otp}." +
                                            $"<br>" +
                                            $"<h5>Warm regards.</h5>" +
                                            "</body>" +
                                        "</html>";

                var sendmail = await _mailService.SendLoginDetails(emailDto.Email, subject, MailBody);

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"AN ERROR OCCURRED....{ex.Message}");
            }
            return response;
        }

        public async Task<ServiceResponse<string>> VerifyOtp(OtpDto otpDto)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var useracc = await _dataContext.Users.Where(x => x.Email.ToLower() == otpDto.Email.ToLower()).FirstOrDefaultAsync();
                if (useracc == null)
                    throw new Exception("User does not exist");
                if(useracc.UserOtp == otpDto.Otp)
                {
                    response.StatusMessage = "Otp Confirmed";
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

    }
}
