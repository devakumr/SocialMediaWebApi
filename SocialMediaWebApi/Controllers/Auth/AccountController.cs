using API.Controllers;
using Application.Auth.Account;
using Domain.Auth.Account;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Controllers.Auth
{
    [AllowAnonymous]
    public class AccountController : BaseController
    {

        [HttpPost]
        public async Task<ActionResult<string>> Register([FromBody] UserModel request)
        {
           var result = await Mediator.Send(new Register.Command { param = request });
           return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<string>> OtpValidation([FromBody] OtpValidationModel request)
        {
            var result = await Mediator.Send(new OtpValidation.Command { param = request });
            return Ok(result);
        }

        [HttpPost]
        
        public async Task<ActionResult<String>> UserLogin([FromBody] UserLoginModel  request)
        {
            var result= await Mediator.Send(new UserLoginInfo().Command { Param = request});
            return Ok(result);
        }
    }

     
}
