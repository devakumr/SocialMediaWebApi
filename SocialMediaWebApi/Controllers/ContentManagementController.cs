using API.Controllers;
using Application.Auth.Account;
using Application.ContentManagementBl;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static Application.ContentManagementBl.ArticleFormBl;


namespace Api.Controllers
{
    public class ContentManagementController : BaseController
    {
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<JObject>> ArticleForm([FromBody] ArticleFormParam p)
        {
            var result = await Mediator.Send(new ArticleFormBl.Command { Param = p });
            return Ok(result);
        }


    }
}
