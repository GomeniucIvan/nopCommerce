using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CatalogController : BaseApiController
    {
        #region Fields

        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IWorkContext _workContext;
        private readonly ICategoryService _categoryService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public CatalogController(ICatalogModelFactory catalogModelFactory,
            IProductService productService,
            IProductModelFactory productModelFactory,
            IWorkContext workContext,
            ICategoryService categoryService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            IGenericAttributeService genericAttributeService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWebHelper webHelper) 
        {
            _catalogModelFactory = catalogModelFactory;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _workContext = workContext;
            _categoryService = categoryService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _permissionService = permissionService;
            _genericAttributeService = genericAttributeService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        [HttpGet]
        [Route("categories")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<CategoryModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CategoryModel>>> HomePageCategoriesAsync()
        {
            var result = new List<CategoryModel>();
            await Task.Run(() => result = _catalogModelFactory.PrepareHomepageCategoryModels());
            return result;
        }

        [HttpGet]
        [Route("products")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<ProductOverviewModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<ProductOverviewModel>>> HomePageProductsAsync()
        {
            var result = new List<ProductOverviewModel>();
            var products = _productService.GetAllProductsDisplayedOnHomepage();

            //TODO 
            //ACL and store mapping
            //products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

            products = products.Where(p => p.VisibleIndividually).ToList();

            await Task.Run(() => result = _productModelFactory.PrepareProductOverviewModels(products, true, true, null).ToList());
            return result;
        }

        [HttpGet]
        [Route("categories/{categoryId}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(CategoryModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CategoryModel>> Category(int categoryId)
        {
            var result = new CategoryModel();
            await Task.Run(() =>
            {
                var category = _categoryService.GetCategoryById(categoryId);
                if (category == null || category.Deleted)
                    BadRequest("Category not exists");

                var notAvailable =
                    //published?
                    !category.Published ||
                    //ACL (access control list) 
                    !_aclService.Authorize(category) ||
                    //Store mapping
                    !_storeMappingService.Authorize(category);
                //Check whether the current user has a "Manage categories" permission (usually a store owner)
                //We should allows him (her) to use "Preview" functionality
                var hasAdminAccess = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageCategories);
                if (notAvailable && !hasAdminAccess)
                    BadRequest("Admin");

                //'Continue shopping' URL
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    NopCustomerDefaults.LastContinueShoppingPageAttribute,
                    _webHelper.GetThisPageUrl(false),
                    _storeContext.CurrentStore.Id);

                //activity log
                _customerActivityService.InsertActivity("PublicStore.ViewCategory",
                    string.Format(_localizationService.GetResource("ActivityLog.PublicStore.ViewCategory"), category.Name), category);

                //model
                result = _catalogModelFactory.PrepareCategoryModel(category, new CatalogPagingFilteringModel());
            });
            return result;
        }

        [HttpGet]
        [Route("navigation")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(CategoryNavigationModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CategoryNavigationModel>> PrepareCategoryNavigationModel(int currentCategoryId = 0,int currentProductId = 0)
        {
            var result = new CategoryNavigationModel();
            await Task.Run(() =>
            {
                //model
                result = _catalogModelFactory.PrepareCategoryNavigationModel(currentCategoryId, currentProductId);
            });
            return result;
        }

        #endregion
    }
}