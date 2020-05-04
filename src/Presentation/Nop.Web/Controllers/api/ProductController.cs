using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Controllers.api
{
    [Route("api/[controller]")]
    public class ProductController : BaseApiController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public ProductController(IProductService productService,
            CatalogSettings catalogSettings,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            IUrlRecordService urlRecordService,
            ShoppingCartSettings shoppingCartSettings,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IProductModelFactory productModelFactory,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ICustomerService customerService)
        {
            _productService = productService;
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _permissionService = permissionService;
            _urlRecordService = urlRecordService;
            _shoppingCartSettings = shoppingCartSettings;
            _workContext = workContext;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _productModelFactory = productModelFactory;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        [HttpGet]
        [Route("{productId}/{updatecartitemid?}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProductDetailsModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ProductDetailsModel>> ModelBySystemNameAsync(int productId, int updatecartitemid = 0)
        {
            var result = new ProductDetailsModel();
            await Task.Run(() =>
            {
                var product = _productService.GetProductById(productId);
                if (product == null || product.Deleted)
                    BadRequest("Not exists");

                var notAvailable =
                    //published?
                    (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
                    //ACL (access control list) 
                    !_aclService.Authorize(product) ||
                    //Store mapping
                    !_storeMappingService.Authorize(product) ||
                    //availability dates
                    !_productService.ProductIsAvailable(product);
                //Check whether the current user has a "Manage products" permission (usually a store owner)
                //We should allows him (her) to use "Preview" functionality
                var hasAdminAccess = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageProducts);
                if (notAvailable && !hasAdminAccess)
                    BadRequest("Not available");

                //visible individually?
                if (!product.VisibleIndividually)
                {
                    //is this one an associated products?
                    var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                    if (parentGroupedProduct == null)
                        BadRequest("Not individual");

                    //todo
                    BadRequest("Not individual");
                }

                //update existing shopping cart or wishlist  item?
                ShoppingCartItem updatecartitem = null;
                if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
                {
                    var customer = _workContext.CurrentCustomer;
                    var cart = _shoppingCartService.GetShoppingCart(customer, storeId: _storeContext.CurrentStore.Id);
                    updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                    //not found?
                    if (updatecartitem == null)
                    {
                        BadRequest("To do");
                    }
                    //is it this product?
                    if (product.Id != updatecartitem.ProductId)
                    {
                        BadRequest("To do");
                    }
                }

                //save as recently viewed
                _recentlyViewedProductsService.AddProductToRecentlyViewedList(product.Id);

                //activity log
                _customerActivityService.InsertActivity("PublicStore.ViewProduct",
                    string.Format(_localizationService.GetResource("ActivityLog.PublicStore.ViewProduct"), product.Name), product);

                //model
                result = _productModelFactory.PrepareProductDetailsModel(product, updatecartitem, false);
            });
            return result;
        }

        #endregion
    }
}