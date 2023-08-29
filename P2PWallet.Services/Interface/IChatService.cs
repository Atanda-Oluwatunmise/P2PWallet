using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interface
{
    public interface IChatService
    {
        Task SendMessageToAdmin(ChatDto chatDto);
        Task SendMessageFromAdmin(ChatDto chatDto);
        Task<ServiceResponse<List<ChatView>>> RetrieveMessagesForAdmin(GetMessagesDto getMessagesDto);
        Task<ServiceResponse<List<ChatView>>> RetrieveMessagesForUser(GetMessagesDto getMessagesDto);
        Task<ServiceResponse<SearchUserViewmodel>> FindUser(SearchDto userObj);
        Task<ServiceResponse<List<ChatView>>> StartChatting(GetMessagesDto getMessagesDto);
        Task<ServiceResponse<List<SearchUserViewmodel>>> ListOfStartedChats(GetMessagesDto getMessagesDto);
        Task<ServiceResponse<string>> VerifyUserEmail(EmailDto emailDto);
        Task<ServiceResponse<string>> VerifyOtp(OtpDto otpDto);



    }
}
