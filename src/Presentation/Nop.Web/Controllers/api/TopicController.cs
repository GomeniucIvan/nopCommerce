using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Models.Topics;

namespace Nop.Web.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TopicController : BaseApiController
    {
        #region Fields

        private readonly ITopicModelFactory _topicModelFactory;

        #endregion

        #region Ctor

        public TopicController(
            ITopicModelFactory topicModelFactory)
        {
            _topicModelFactory = topicModelFactory;
        }

        #endregion

        #region Methods

        [HttpGet]
        [Route("{systemname}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(TopicModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<TopicModel>> ModelBySystemNameAsync(string systemName)
        {
            var result = new TopicModel();
            await Task.Run(() => result = _topicModelFactory.PrepareTopicModelBySystemName(systemName));
            return result;
        }

        #endregion
    }
}