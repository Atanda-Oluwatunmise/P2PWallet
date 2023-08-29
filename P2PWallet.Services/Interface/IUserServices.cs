using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interface
{
    public interface IUserServices
    {
        public Task<ServiceResponse<UserViewModel>> Register(UserDto user);
        public Task<ServiceResponse<LoginView>> Login(LoginDto loginreq);
        public Task<bool> UserAlreadyExists(string userName);
        public Task<bool> EmailAlreadyExists(string emailName);
        public string GenerateEmailToken();
        public bool VerifyPasswordHash(string password, byte[] passwordKey, byte[] passwordHash);
        public Task<ServiceResponse<List<AccountDetails>>> GetMyAccountNumber();
        public Task<ServiceResponse<List<SearchAccountDetails>>> GetUserDetails(UserSearchDto userSearch);
        public Task<ServiceResponse<List<SearchAccountDetails>>> GetUserForeignDetails(ForeignUserSearchDto userSearch);

        public void CreatePasswordHash(string password, out byte[] passwordKey, out byte[] passwordHash);
        public Task<ServiceResponse<List<EditViewModel>>> EditUserInfo(EditViewModel request);
        public Task<ServiceResponse<string>> SaveImage([FromBody] ImageViewmodel imageview);
        public Task<DisplayViewmodel> DisplayImage();
        public Task<ServiceResponse<string>> DeleteImage();
        public Task<bool> VerifyImageStatus();
        public Task<ServiceResponse<TokenApiDto>> Refresh(TokenApiDto tokenApiDto);
        public Task<ServiceResponse<UsersCount>> LockedandUnlockedUsers();




    }
}
