using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;

namespace VerichatCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Get all products with optional filtering by category or low-stock status.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "System Administrator,Sales Representative,Procurement Manager,Client User")]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts(
            [FromQuery] string? category = null,
            [FromQuery] bool? lowStockOnly = null)
        {
            var products = await _productService.GetProductsAsync(category, lowStockOnly);
            return Ok(products);
        }

        /// <summary>
        /// Get a single product by SKU.
        /// </summary>
        [HttpGet("{sku}")]
        [Authorize(Roles = "System Administrator,Sales Representative,Procurement Manager,Client User")]
        public async Task<ActionResult<ProductResponse>> GetProduct(string sku)
        {
            var product = await _productService.GetProductBySkuAsync(sku);
            if (product == null) return NotFound(new { message = $"Product with SKU {sku} not found." });
            return Ok(product);
        }

        /// <summary>
        /// Create a new product (Admin only).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "System Administrator")]
        public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
        {
            var product = await _productService.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetProduct), new { sku = product.SKU }, product);
        }

        /// <summary>
        /// Edit a product's details, including its price. Sales representatives
        /// and administrators may do this; procurement and clients may not.
        /// </summary>
        [HttpPut("{sku}")]
        [Authorize(Roles = "System Administrator,Sales Representative")]
        public async Task<ActionResult<ProductResponse>> UpdateProduct(string sku, [FromBody] UpdateProductRequest request)
        {
            var product = await _productService.UpdateProductAsync(sku, request);
            if (product == null) return NotFound(new { message = $"Product with SKU {sku} not found." });
            return Ok(product);
        }

        /// <summary>
        /// Update product stock quantity.
        /// </summary>
        [HttpPatch("{sku}/stock")]
        [Authorize(Roles = "System Administrator,Procurement Manager")]
        public async Task<ActionResult<ProductResponse>> UpdateStock(string sku, [FromBody] UpdateStockRequest request)
        {
            var product = await _productService.UpdateStockAsync(sku, request);
            if (product == null) return NotFound(new { message = $"Product with SKU {sku} not found." });
            return Ok(product);
        }
    }
}
