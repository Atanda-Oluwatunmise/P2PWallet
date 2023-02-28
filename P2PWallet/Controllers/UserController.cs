using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services;
using P2PWallet.Services.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Api.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    { 

        private readonly DataContext _dataContext;
        private readonly IP2PWalletServices _p2PWalletServices;
        public UserController(DataContext dataContext, IP2PWalletServices p2PWalletServices)
        {
            _dataContext = dataContext;
            _p2PWalletServices = p2PWalletServices;

        }

        [HttpPost("Register")]
        public async Task<ActionResult<ServiceResponse<List<UserViewModel>>>> Register(UserDto user)
        {
            if (await _p2PWalletServices.UserAlreadyExists(user.Username) || await _p2PWalletServices.EmailAlreadyExists(user.Email))
              return BadRequest ("User Already Exists");

            await _p2PWalletServices.Register(user);
            return Ok("User Successfully created");
        }

        [HttpPost("Login")]
        public async Task<ServiceResponse<string>> Login(LoginDto loginreq)
        {
             var result = await _p2PWalletServices.Login(loginreq);
             return result;
        }




    }
}
