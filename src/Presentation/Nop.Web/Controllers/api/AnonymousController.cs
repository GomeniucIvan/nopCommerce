using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Localization;
using Nop.Web.Controllers.api.Models;

namespace Nop.Web.Controllers.api
{
    //https://docs.microsoft.com/en-us/dotnet/core/compatibility/2.2-3.1#authorization-iallowanonymous-removed-from-authorizationfiltercontextfilters
    [Route("api/[controller]")]
    public class AnonymousController : Controller
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILanguageService _languageService;
        private readonly ILanguageModelFactory _languageModelFactory;

        #endregion

        #region Ctor

        public AnonymousController(IWorkContext workContext,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IShoppingCartService shoppingCartService,
            ILanguageService languageService,
            ILanguageModelFactory languageModelFactory)
        {
            _workContext = workContext;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _shoppingCartService = shoppingCartService;
            _languageService = languageService;
            _languageModelFactory = languageModelFactory;
        }

        #endregion

        #region Methods

        [HttpPost]
        [Route("token")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiGenericModel<string>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiGenericModel<string>>> TokenCreate([FromBody] GenerateTokenFilter model)
        {
            var result = new ApiGenericModel<string>();
            await Task.Run(()=>
            {
                if (!model.CustomerGuid.HasValue)
                    return;

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sid, model.CustomerGuid.ToString()),
                };

                var stringBytes = Encoding.UTF8.GetBytes(NopAuthenticationDefaults.JwtSecurityTokenKey);

                var signingCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(stringBytes),
                    SecurityAlgorithms.HmacSha256Signature);

                var token = new JwtSecurityToken(
                    NopAuthenticationDefaults.AuthenticationScheme,
                    NopAuthenticationDefaults.AuthenticationScheme,
                    claims,
                    notBefore: DateTime.Now,
                    expires: DateTime.UtcNow.AddYears(1),
                    signingCredentials
                );

                result.Data = new JwtSecurityTokenHandler().WriteToken(token);
            });
            return result;
        }

        [HttpGet]
        [Route("currentcustomer")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiCustomer), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiCustomer>> GetCurrentCustomerModelAsync()
        {
            var result = new ApiCustomer();
            await Task.Run(() =>
            {
                var customer = _workContext.CurrentCustomer;
                result = new ApiCustomer()
                {
                    Email = customer.Email,
                    Username = customer.Username,
                    HasShoppingCartItems = customer.HasShoppingCartItems,
                    RequireReLogin = customer.RequireReLogin,
                    CustomerRoles = _customerService.GetCustomerRoles(customer).Select(v => new CustomerRoleModel
                    {
                        Name = v.Name,
                        SystemName = v.SystemName,
                        Active = v.Active,
                        IsSystemRole = v.IsSystemRole
                    }).ToList(),
                    FirstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute),
                    LastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute),
                    ShoppingCartItems = _shoppingCartService.GetShoppingCart(customer).Select(v => new ShoppingCartItemModel()
                    {
                        ProductId = v.ProductId,
                        ShoppingCartTypeId = v.ShoppingCartTypeId,
                        Quantity = v.Quantity
                    }).ToList(),
                    CustomerGuid = customer.CustomerGuid
                };
            });
            return result;
        }

        [HttpGet]
        [Route("localeresources/{languageId}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<LocaleResourceModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<LocaleResourceModel>>> GetLocaleResourceByIdAsync(int languageId)
        {
            var result = new List<LocaleResourceModel>();
            await Task.Run(() =>
            {
                var language = _languageService.GetLanguageById(languageId);
                result = _languageModelFactory
                    .PrepareLocaleResourceListModel(new LocaleResourceSearchModel() {Length = int.MaxValue}, language)
                    .Data.Where(v => v.ResourceName.Contains("mobile")).ToList();
            });

            return result;
        }

        #endregion
    }
}