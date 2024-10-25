using API.Controllers;
using Application.Account;
using Application.Auth.Account;
using Domain.Auth.Account;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static Application.Account.validateJwtTokenBL;



namespace Api.Controllers.Auth
{
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        [Authorize]

        [HttpPost]
        public async Task<ActionResult<string>> Register([FromBody] UserModel request)
        {
           var result = await Mediator.Send(new Register.Command { param = request });
           return Ok(result);
        }
        [Authorize]

        [HttpPost]
        public async Task<ActionResult<string>> OtpValidation([FromBody] OtpValidationModel request)
        {
            var result = await Mediator.Send(new OtpValidation.Command { Param = request });
            return Ok(result);
        }
        

        [HttpPost]

        public async Task<ActionResult<string>> UserLogin([FromBody] UserLoginModel request)
        {
            var result = await Mediator.Send(new UserLogin.Command { Param = request});
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<string>> validateJwtToken(ValidateJwtTokenParam p)
        {
            return await Mediator.Send(new validateJwtTokenBL.Command { Param = p });
        }
    }


}
