using API.Controllers;
using Application.Account;
using Application.Auth.Account;
using Domain.DtoClass;
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
        public async Task<ActionResult<string>> Register([FromBody] RegisterRequestDto request)
        {
           var result = await Mediator.Send(new RegisterBL.Command { param = request });
           return Ok(result);
        }

        [Authorize]

        [HttpPost]
        public async Task<ActionResult<string>> OtpValidation([FromBody] OtpValidationDto request)
        {
            var result = await Mediator.Send(new OtpValidationBL.Command { Param = request });
            return Ok(result);
        }
        

        [HttpPost]

        public async Task<ActionResult<string>> UserLogin([FromBody] UserLoginDto request)
        {
            var result = await Mediator.Send(new UserLoginBL.Command { Param = request});
            return Ok(result);
        }

        [HttpPost]

        public async Task<ActionResult<string>> ForgotPassword(ForgotPasswordParam request)
        {
            var result = await Mediator.Send(new ForgotPasswordBL.Command { Param = request });
            return Ok(result);
        }


    }


}
