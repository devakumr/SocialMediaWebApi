using Application.CmnCls;
using Infrastructure.Security;
using MediatR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Application.ContentManagementBl
{
    public class ArticleFormBl
    {
        // Command class for article submission
        public class Command : IRequest<JObject>
        {
            public ArticleFormParam Param { get; set; }
        }

        // Handler for processing the article submission
        public class Handler : IRequestHandler<Command, JObject>
        {
            private readonly IUserAccessor _userAccessor;

            public Handler(IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor;
            }

            public async Task<JObject> Handle(Command request, CancellationToken cancellationToken)
            {
                // Simulate an asynchronous delay (not recommended in production)
                await Task.Delay(1);

                // Retrieve user claims
                ClaimsPrincipal claimsPrincipal = _userAccessor.GetClaim();
                string userId = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                // Create a SQL command to execute the stored procedure
                using (SqlCommand cmd = new SqlCommand("ArticleFormSP"))
                {
                    // Add parameters to the SQL command
                    cmd.Parameters.AddWithValue("@ArticleTitle", request.Param.ArticleTitle);
                    cmd.Parameters.AddWithValue("@ArticleContent", request.Param.ArticleContent);
                    cmd.Parameters.AddWithValue("@ArticleFeedContent", request.Param.ArticleFeedContent);
                    cmd.Parameters.AddWithValue("@ArticleImage", request.Param.ArticleImage);
                    cmd.Parameters.AddWithValue("@Category", request.Param.Category);
                    cmd.Parameters.AddWithValue("@Permalink", request.Param.Permalink);
                    cmd.Parameters.AddWithValue("@PublicationDate", request.Param.PublicationDate.HasValue ? (object)request.Param.PublicationDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SeoMetaDescription", request.Param.SeoMetaDescription);
                    cmd.Parameters.AddWithValue("@SeoMetaTitle", request.Param.SeoMetaTitle);
                    cmd.Parameters.AddWithValue("@Status", request.Param.Status);
                    cmd.Parameters.AddWithValue("@Summary", request.Param.Summary);
                    cmd.Parameters.AddWithValue("@AllowComments", request.Param.AllowComments);

                    // Execute the command and retrieve results
                    DataSet ds = ConnCls.ret_ds_Cmd(cmd, ConnCls.Con);

                    // Return success response
                    return JObject.FromObject(new { success = true });
                }
            }
        }

        // Parameters class for article form data
        public class ArticleFormParam
        {
            public string ArticleTitle { get; set; }
            public string ArticleContent { get; set; }
            public string ArticleFeedContent { get; set; }
            public string ArticleImage { get; set; }
            public string Category { get; set; }
            public string Permalink { get; set; }
            public DateTime? PublicationDate { get; set; }
            public string SeoMetaDescription { get; set; }
            public string SeoMetaTitle { get; set; }
            public string Status { get; set; }
            public string Summary { get; set; }
            public List<string> Tags { get; set; } // Add tags support
            public bool AllowComments { get; set; }
        }
    }
}
