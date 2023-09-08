using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController:ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IChatService _chatService;
        public ChatController(DataContext dataContext, IChatService chatService) 
        {
            _dataContext = dataContext;
            _chatService = chatService;
        }

        [HttpPost("postusermessage"), Authorize]
        public async Task SendMessageToAdmin(UserChatDto chatDto)
        {
           await _chatService.SendMessageToAdmin(chatDto);
        }

        [HttpPost("usertousermessage"), Authorize]
        public async Task UsertoUserChat(UserChatDto chatDto)
        {
            await _chatService.UsertoUserChat(chatDto);

        }


        [HttpPost("postadminmessage"), Authorize]
        public async Task SendMessageFromAdmin(UserChatDto chatDto)
        {
            await _chatService.SendMessageFromAdmin(chatDto);
        }

        [HttpPost("getadminmessages"), Authorize]
        public async Task<ServiceResponse<List<ChatView>>> RetrieveMessagesForAdmin(GetMessagesDto getMessagesDto)
        {
            var response = await _chatService.RetrieveMessagesForAdmin(getMessagesDto);
            return response;
        }

        [HttpPost("getusermessages")]
        public async Task<ServiceResponse<List<ChatView>>> RetrieveMessagesForUser(GetMessagesDto getMessagesDto)
        {
            var response = await _chatService.RetrieveMessagesForUser(getMessagesDto);
            return response;
        }

        [HttpPost("getlockedusermessages")]
        public async Task<ServiceResponse<List<ChatView>>> RetrieveMessagesForUserOutside(GetMessagesDto getMessagesDto)
        {
            var response = await _chatService.RetrieveMessagesForUserOutside(getMessagesDto);
            return response;
        }

        [HttpPost("finduser"), Authorize]
        public async Task<ServiceResponse<SearchUserViewmodel>> FindUser(SearchDto userObj)
        {
            var response = await _chatService.FindUser(userObj);
            return response;
        }


        [HttpPost("startchatting"), Authorize]
        public async Task<ServiceResponse<List<ChatView>>> StartChatting(GetMessagesDto getMessagesDto)
        {
            var result = await _chatService.StartChatting(getMessagesDto);
            return result;
        }

        [HttpPost("listofstartedchats"), Authorize]
        public async Task<ServiceResponse<List<SearchUserViewmodel>>> ListOfStartedChats(GetMessagesDto getMessagesDto)
        {
            var response = await _chatService.ListOfStartedChats(getMessagesDto);
            return response;
        }

        [HttpPost("verifyuseremail")]
        public async Task<ServiceResponse<string>> VerifyUserEmail(EmailDto emailDto)
        {
            var response = await _chatService.VerifyUserEmail(emailDto);
            return response;
        }

        [HttpPost("verifyuserotp")]
        public async Task<ServiceResponse<string>> VerifyOtp(OtpDto otpDto)
        {
            var response = await _chatService.VerifyOtp(otpDto);
            return response;
        }

        [HttpPost("startedchatsforadmin")]
        public async Task<ServiceResponse<List<SearchUserViewmodel>>> ListOfStartedChatsforAdmin(GetMessagesDto getMessagesDto)
        {
            var response = await _chatService.ListOfStartedChatsforAdmin(getMessagesDto);
            return response;
        }

        [HttpPost("getreveiverdetails"), Authorize]
        public async Task<ServiceResponse<chatUserDetails>> GetReceipientDetails(GetMessagesDto getMessagesDto)
        {
            var response = await _chatService.GetReceipientDetails(getMessagesDto);
            return response;
        }

    }
}
