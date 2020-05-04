using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using LinqToDB.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Localization;
using NUglify.Helpers;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Nop.Web.Controllers.api.Filters
{
    public class AuthorizeCustomerAttribute : TypeFilterAttribute
    {
        public AuthorizeCustomerAttribute() : base(typeof(AuthorizeCustomerFilter))
        {

        }

        private class AuthorizeCustomerFilter : IAuthorizationFilter
        {
            #region Fields

            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly IWorkContext _workContext;
            private readonly ICustomerService _customerService;
            private readonly ILanguageService _languageService;

            #endregion

            #region Ctor

            public AuthorizeCustomerFilter(
                IHttpContextAccessor httpContextAccessor,
                IWorkContext workContext,
                ICustomerService customerService,
                ILanguageService languageService)
            {
                _httpContextAccessor = httpContextAccessor;
                _workContext = workContext;
                _customerService = customerService;
                _languageService = languageService;
            }

            #endregion

            #region Methods

            void IAuthorizationFilter.OnAuthorization(AuthorizationFilterContext context)
            {
                if (context?.HttpContext?.Request?.Headers == null)
                    return;

                // customer
                var accessToken = context.HttpContext.Request.Headers["Authorization"].ToString();

                if (accessToken.IsNullOrWhiteSpace())
                    return;

                var jwt = accessToken.Replace($"{JwtBearerDefaults.AuthenticationScheme} ", string.Empty);

                var handler = new JwtSecurityTokenHandler();
                var tokenS = handler.ReadToken(jwt) as JwtSecurityToken;

                var customerGuidString = tokenS?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)?.Value;

                //change to cached customer TODO
                if (Guid.TryParse(customerGuidString, out var newGuid))
                    _workContext.CurrentCustomer = _customerService.GetCustomerByGuid(newGuid);

                // customer
                var languageIdString = context.HttpContext.Request.Headers["LanguageId"].ToString();
                if (!languageIdString.IsNullOrEmpty())
                {
                    var validLanguageId = int.TryParse(languageIdString, out var languageId);
                    if (validLanguageId)
                    {
                        _workContext.WorkingLanguage = _languageService.GetLanguageById(languageId);
                    }
                }
            }

            #endregion
        }

    }
}