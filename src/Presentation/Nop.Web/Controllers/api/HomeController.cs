using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.News;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.News;

namespace Nop.Web.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class HomeController : BaseApiController
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly NewsSettings _newsSettings;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IOrderReportService _orderReportService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly INewsModelFactory _newsModelFactory;

        #endregion

        #region Ctor

        public HomeController(
            CatalogSettings catalogSettings,
            NewsSettings newsSettings,
            ICatalogModelFactory catalogModelFactory,
            IProductService productService,
            IProductModelFactory productModelFactory,
            ICacheManager cacheManager,
            IStoreContext storeContext,
            IOrderReportService orderReportService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            INewsModelFactory newsModelFactory
            )
        {
            _catalogSettings = catalogSettings;
            _newsSettings = newsSettings;
            _catalogModelFactory = catalogModelFactory;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _cacheManager = cacheManager;
            _storeContext = storeContext;
            _orderReportService = orderReportService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _newsModelFactory = newsModelFactory;
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
            await Task.Run(() =>
            {
                var products = _productService.GetAllProductsDisplayedOnHomepage();

                //ACL and store mapping
                products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

                //availability dates
                products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

                products = products.Where(p => p.VisibleIndividually).ToList();

                result = _productModelFactory.PrepareProductOverviewModels(products, true, true, null).ToList();
                return result;
            });
            return result;
        }

        [HttpGet]
        [Route("bestsellers")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<ProductOverviewModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<ProductOverviewModel>>> HomeBestSellersAsync()
        {
            var result = new List<ProductOverviewModel>();
            await Task.Run(() =>
            {
                if (!_catalogSettings.ShowBestsellersOnHomepage || _catalogSettings.NumberOfBestsellersOnHomepage == 0)
                    return result;

                //load and cache report
                var report = _cacheManager.Get(NopModelCacheDefaults.HomepageBestsellersIdsKey.FillCacheKey(_storeContext.CurrentStore.Id),
                    () => _orderReportService.BestSellersReport(
                            storeId: _storeContext.CurrentStore.Id,
                            pageSize: _catalogSettings.NumberOfBestsellersOnHomepage)
                        .ToList());

                //load products
                var products = _productService.GetProductsByIds(report.Select(x => x.ProductId).ToArray());
                //ACL and store mapping
                products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
                //availability dates
                products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

                if (!products.Any())
                    return result;

                //prepare model
                result = _productModelFactory.PrepareProductOverviewModels(products, true, true, null).ToList();

                return result;
            });
            return result;
        }

        [HttpGet]
        [Route("news")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<NewsItemModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<NewsItemModel>>> HomeNewsAsync()
        {
            var result = new List<NewsItemModel>();
            await Task.Run(() =>
            {
                if (!_newsSettings.Enabled || !_newsSettings.ShowNewsOnMainPage)
                    return result;

                result = _newsModelFactory.PrepareHomepageNewsItemsModel().NewsItems.ToList();
                return result;
            });
            return result;
        }

        #endregion
    }
}