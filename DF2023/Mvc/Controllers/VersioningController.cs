using DF2023.Core.Helpers;
using DF2023.Mvc.Models;
using System;
using System.Web.Http;

namespace DF2023.Mvc.Controllers
{
    [Authorize]
    public class VersioningController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetFullHistory(string itemType, Guid id)
        {
            if (!itemType.StartsWith("Telerik.Sitefinity.DynamicTypes.Model"))
            {
                itemType = $"Telerik.Sitefinity.DynamicTypes.Model.{itemType}";
            }
            ApiResult apiResult = null;
            try
            {
                var history = VersioningHelper.GetFullHistory(itemType, id);

                apiResult = new ApiResult("Guest Convention Details", true, history);
            }
            catch (Exception ex)
            {
                apiResult = new ApiResult(ex.Message, false, null);
            }

            return this.Ok(apiResult);
        }
    }
}