using Mealthy.Api.Marketplace.Application.Services;
using Mealthy.Api.Marketplace.Interfaces.Rest.Mappings;
using Mealthy.Api.Marketplace.Interfaces.Rest.Resources;
using Mealthy.Api.Shared.Application;
using Mealthy.Api.Shared.Infraestructure.Web;
using Mealthy.Api.Shared.Interfaces.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Mealthy.Api.Marketplace.Interfaces.Rest;

/// <summary>Catálogo global de solo lectura: búsqueda de productos entre todas las tiendas.</summary>
[ApiController]
[Route("api/v1/products")]
[AllowAnonymous]
[Produces("application/json")]
public class ProductsController(ProductService products) : ControllerBase
{
    [HttpGet]
    [OutputCache(PolicyName = CachePolicies.Products)]
    [ProducesResponseType<PagedResult<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductResponse>>> Search(
        [FromQuery] PageQuery query, [FromQuery] string? category, [FromQuery] string? search, CancellationToken ct)
    {
        var page = await products.SearchAsync(category, search, query.SafePage, query.SafePageSize, ct);
        return Ok(new PagedResult<ProductResponse>(
            page.Items.Select(p => p.ToResponse()).ToList(), page.Page, page.PageSize, page.TotalItems));
    }

    [HttpGet("{id:int}")]
    [OutputCache(PolicyName = CachePolicies.Products)]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken ct)
        => Ok((await products.GetAsync(id, ct)).ToResponse());
}
