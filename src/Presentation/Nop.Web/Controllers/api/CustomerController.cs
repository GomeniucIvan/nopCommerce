using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tax;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Web.Controllers.api.Models;
using Nop.Web.Factories;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using IAuthenticationService = Nop.Services.Authentication.IAuthenticationService;

namespace Nop.Web.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CustomerController : BaseApiController
    {
        #region Fields

        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IWorkContext _workContext;
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ITaxService _taxService;
        private readonly TaxSettings _taxSettings;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly GdprSettings _gdprSettings;
        private readonly IGdprService _gdprService;
        private readonly IAddressService _addressService;
        private readonly ICommonModelFactory _commonModelFactory;

        #endregion

        #region Ctor

        public CustomerController(
            ICustomerModelFactory customerModelFactory,
            IWorkContext workContext,
            CustomerSettings customerSettings,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IShoppingCartService shoppingCartService,
            IAuthenticationService authenticationService,
            IEventPublisher eventPublisher,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            DateTimeSettings dateTimeSettings,
            ITaxService taxService,
            TaxSettings taxSettings,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            GdprSettings gdprSettings,
            IGdprService gdprService,
            IAddressService addressService,
            ICommonModelFactory commonModelFactory)
        {
            _customerModelFactory = customerModelFactory;
            _workContext = workContext;
            _customerSettings = customerSettings;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _shoppingCartService = shoppingCartService;
            _authenticationService = authenticationService;
            _eventPublisher = eventPublisher;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _dateTimeSettings = dateTimeSettings;
            _taxService = taxService;
            _taxSettings = taxSettings;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _gdprSettings = gdprSettings;
            _gdprService = gdprService;
            _addressService = addressService;
            _commonModelFactory = commonModelFactory;
        }

        #endregion

        #region Methods

        [HttpGet]
        [Route("login")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiGenericModel<LoginModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiGenericModel<LoginModel>>> GetLoginAsync()
        {
            var result = new ApiGenericModel<LoginModel>();
            await Task.Run(() => result.Data = _customerModelFactory.PrepareLoginModel(null));
            return result;
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiGenericModel<Guid>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiGenericModel<Guid>>> Login([FromBody] ApiLoginModel model)
        {
            if (_customerSettings.UsernamesEnabled && model.Username != null)
                model.Username = model.Username.Trim();

            var loginResult = _customerRegistrationService.ValidateCustomer(_customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password);
             var result = new ApiGenericModel<Guid>();
            switch (loginResult)
            {
                case CustomerLoginResults.Successful:
                    {
                        var customer = _customerSettings.UsernamesEnabled
                            ? _customerService.GetCustomerByUsername(model.Username)
                            : _customerService.GetCustomerByEmail(model.Email);

                        //migrate shopping cart
                        _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer, true);

                        //sign in new customer
                        _authenticationService.SignIn(customer, model.RememberMe);

                        //raise event       
                        _eventPublisher.Publish(new CustomerLoggedinEvent(customer));

                        //activity log
                        _customerActivityService.InsertActivity(customer, "PublicStore.Login",
                            _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);

                        result.Data = customer.CustomerGuid;
                        return result;
                    }
                case CustomerLoginResults.CustomerNotExist:
                    return BadRequest(_localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist"));
                case CustomerLoginResults.Deleted:
                    return BadRequest(_localizationService.GetResource("Account.Login.WrongCredentials.Deleted"));
                case CustomerLoginResults.NotActive:
                    return BadRequest(_localizationService.GetResource("Account.Login.WrongCredentials.NotActive"));
                case CustomerLoginResults.NotRegistered:
                    return BadRequest(_localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered"));
                case CustomerLoginResults.LockedOut:
                    return BadRequest(_localizationService.GetResource("Account.Login.WrongCredentials.LockedOut"));
                case CustomerLoginResults.WrongPassword:
                default:
                    return BadRequest(_localizationService.GetResource("Account.Login.WrongCredentials"));
            }
        }

        [HttpGet]
        [Route("register")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiGenericModel<RegisterModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiGenericModel<RegisterModel>>> RegisterAsync()
        {
            //check whether registration is allowed
            //if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
            //    return Response;

            var result = new ApiGenericModel<RegisterModel>();
            await Task.Run(() => result.Data = _customerModelFactory.PrepareRegisterModel(new RegisterModel(), false, setDefaultValues: true));
            return result;
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiGenericModel<Guid>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiGenericModel<Guid>>> Register([FromBody] ApiRegisterModel model)
        {
            var result = new ApiGenericModel<Guid>();

            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return BadRequest("Disabled");

            if (_customerService.IsRegistered(_workContext.CurrentCustomer))
            {
                //Already registered customer. 
                _authenticationService.SignOut();

                //raise logged out event       
                _eventPublisher.Publish(new CustomerLoggedOutEvent(_workContext.CurrentCustomer));

                //Save a new record
                _workContext.CurrentCustomer = _customerService.InsertGuestCustomer();
            }
            var customer = _workContext.CurrentCustomer;
            customer.RegisteredInStoreId = _storeContext.CurrentStore.Id;

            if (_customerSettings.UsernamesEnabled && model.Username != null)
            {
                model.Username = model.Username.Trim();
            }

            var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
            var registrationRequest = new CustomerRegistrationRequest(customer,
                model.Email,
                _customerSettings.UsernamesEnabled ? model.Username : model.Email,
                model.Password,
                _customerSettings.DefaultPasswordFormat,
                _storeContext.CurrentStore.Id,
                isApproved);
            var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
            if (registrationResult.Success)
            {
                //properties
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                {
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.TimeZoneIdAttribute, model.TimeZoneId);
                }
                //VAT number
                if (_taxSettings.EuVatEnabled)
                {
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberAttribute, model.VatNumber);

                    var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out string _, out string vatAddress);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberStatusIdAttribute, (int)vatNumberStatus);
                    //send VAT number admin notification
                    if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                        _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                }

                //form fields
                if (_customerSettings.GenderEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.GenderAttribute, model.Gender);
                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);
                if (_customerSettings.DateOfBirthEnabled)
                {
                    var dateOfBirth = model.ParseDateOfBirth();
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                }
                if (_customerSettings.CompanyEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CompanyAttribute, model.Company);
                if (_customerSettings.StreetAddressEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddressAttribute, model.StreetAddress);
                if (_customerSettings.StreetAddress2Enabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddress2Attribute, model.StreetAddress2);
                if (_customerSettings.ZipPostalCodeEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
                if (_customerSettings.CityEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CityAttribute, model.City);
                if (_customerSettings.CountyEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountyAttribute, model.County);
                if (_customerSettings.CountryEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountryIdAttribute, model.CountryId);
                if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StateProvinceIdAttribute,
                        model.StateProvinceId);
                if (_customerSettings.PhoneEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);
                if (_customerSettings.FaxEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FaxAttribute, model.Fax);

                //newsletter
                if (_customerSettings.NewsletterEnabled)
                {
                    //save newsletter value
                    var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(model.Email, _storeContext.CurrentStore.Id);
                    if (newsletter != null)
                    {
                        if (model.Newsletter)
                        {
                            newsletter.Active = true;
                            _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);

                            //GDPR
                            if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                            {
                                _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.Newsletter"));
                            }
                        }
                        //else
                        //{
                        //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                        //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                        //}
                    }
                    else
                    {
                        if (model.Newsletter)
                        {
                            _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                            {
                                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                Email = model.Email,
                                Active = true,
                                StoreId = _storeContext.CurrentStore.Id,
                                CreatedOnUtc = DateTime.UtcNow
                            });

                            //GDPR
                            if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                            {
                                _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.Newsletter"));
                            }
                        }
                    }
                }

                if (_customerSettings.AcceptPrivacyPolicyEnabled)
                {
                    //privacy policy is required
                    //GDPR
                    if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                    {
                        _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.PrivacyPolicy"));
                    }
                }

                //GDPR
                //if (_gdprSettings.GdprEnabled)
                //{
                //    var consents = _gdprService.GetAllConsents().Where(consent => consent.DisplayDuringRegistration).ToList();
                //    foreach (var consent in consents)
                //    {
                //        var controlId = $"consent{consent.Id}";
                //        var cbConsent = form[controlId];
                //        if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                //        {
                //            //agree
                //            _gdprService.InsertLog(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                //        }
                //        else
                //        {
                //            //disagree
                //            _gdprService.InsertLog(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                //        }
                //    }
                //}

                //save customer attributes
                //_genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CustomCustomerAttributes, customerAttributesXml);

                //login customer now
                if (isApproved)
                    _authenticationService.SignIn(customer, true);

                //insert default address (if possible)
                var defaultAddress = new Address
                {
                    FirstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute),
                    LastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute),
                    Email = customer.Email,
                    Company = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CompanyAttribute),
                    CountryId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.CountryIdAttribute) > 0
                        ? (int?)_genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.CountryIdAttribute)
                        : null,
                    StateProvinceId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute) > 0
                        ? (int?)_genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute)
                        : null,
                    County = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CountyAttribute),
                    City = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CityAttribute),
                    Address1 = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.StreetAddressAttribute),
                    Address2 = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.StreetAddress2Attribute),
                    ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute),
                    PhoneNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute),
                    FaxNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FaxAttribute),
                    CreatedOnUtc = customer.CreatedOnUtc
                };
                if (_addressService.IsAddressValid(defaultAddress))
                {
                    //some validation
                    if (defaultAddress.CountryId == 0)
                        defaultAddress.CountryId = null;
                    if (defaultAddress.StateProvinceId == 0)
                        defaultAddress.StateProvinceId = null;
                    //set default address
                    //customer.Addresses.Add(defaultAddress);
                    _customerService.InsertCustomerAddress(customer, defaultAddress);

                    customer.BillingAddressId = defaultAddress.Id;
                    customer.ShippingAddressId = defaultAddress.Id;

                    _customerService.UpdateCustomer(customer);
                }

                //notifications
                if (_customerSettings.NotifyNewCustomerRegistration)
                    _workflowMessageService.SendCustomerRegisteredNotificationMessage(customer,
                        _localizationSettings.DefaultAdminLanguageId);

                //raise event       
                _eventPublisher.Publish(new CustomerRegisteredEvent(customer));

                //switch (_customerSettings.UserRegistrationType)
                //{
                //    case UserRegistrationType.EmailValidation:
                //        {
                //            //email validation message
                //            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                //            _workflowMessageService.SendCustomerEmailValidationMessage(customer, _workContext.WorkingLanguage.Id);

                //            //result
                //            return RedirectToRoute("RegisterResult",
                //                new { resultId = (int)UserRegistrationType.EmailValidation });
                //        }
                //    case UserRegistrationType.AdminApproval:
                //        {
                //            return RedirectToRoute("RegisterResult",
                //                new { resultId = (int)UserRegistrationType.AdminApproval });
                //        }
                //    case UserRegistrationType.Standard:
                //        {
                //            //send customer welcome message
                //            _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

                //            var redirectUrl = Url.RouteUrl("RegisterResult",
                //                new { resultId = (int)UserRegistrationType.Standard, returnUrl }, _webHelper.CurrentRequestProtocol);
                //            return Redirect(redirectUrl);
                //        }
                //    default:
                //        {
                //            return RedirectToRoute("Homepage");
                //        }
                //}
            }

            //errors
            foreach (var error in registrationResult.Errors)
                ModelState.AddModelError("", error);

            return result;
        }

        [HttpGet]
        [Route("currentcustomer")]
        [AllowAnonymous]
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

        [HttpPost]
        [Route("logout")]
        public void Logout(ApiCustomer model)
        {
            var customer = _customerService.GetCustomerByEmail(model.Email);
            if (customer != null && customer.CustomerGuid == model.CustomerGuid)
            {
                //activity log
                _customerActivityService.InsertActivity(customer, "PublicStore.Logout",
                    _localizationService.GetResource("ActivityLog.PublicStore.Logout"), customer);

                //standard logout 
                _authenticationService.SignOut();

                //raise logged out event       
                _eventPublisher.Publish(new CustomerLoggedOutEvent(customer));
            }
        }

        [HttpGet]
        [Route("languages")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(IList<LanguageModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IList<LanguageModel>>> GetLanguages()
        {
            var result = new List<LanguageModel>();
            await Task.Run(() => result = _commonModelFactory.PrepareLanguageSelectorModel().AvailableLanguages.ToList());
            return result;
        }

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

        #endregion 
    }
}