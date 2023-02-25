using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Services;
using P2PWallet.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Api.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class P2PWalletController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IP2PWalletServices _p2PWalletServices;
        public P2PWalletController(DataContext dataContext, IP2PWalletServices p2PWalletServices) {
            _dataContext = dataContext;
            _p2PWalletServices = p2PWalletServices;

        }

        [Route("Users")]
        [HttpPost]
        public async Task<ActionResult<List<UserViewModel>>> Register(UserDto user)
        {
            if (await _p2PWalletServices.UserAlreadyExists(user.Username) || await _p2PWalletServices.EmailAlreadyExists(user.Email))
                return BadRequest("User already exists");

            var result = await _p2PWalletServices.Register(user);
            return StatusCode(201);
        }

    }
}
