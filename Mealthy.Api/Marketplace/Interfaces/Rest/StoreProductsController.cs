using Mealthy.Api.Marketplace.Application.Services;
using Mealthy.Api.Marketplace.Interfaces.Rest.Mappings;
using Mealthy.Api.Marketplace.Interfaces.Rest.Resources;
using Mealthy.Api.Security.Infraestructure.Auth;
using Mealthy.Api.Shared.Application;
using Mealthy.Api.Shared.Interfaces.Extensions;
using Mealthy.Api.Shared.Interfaces.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mealthy.Api.Marketplace.Interfaces.Rest;

[ApiController]
[Route("api/v1/stores/{storeId:int}/products")]
[Authorize(Policy = AuthPolicies.StoreOwnerOnly)]
[Produces("application/json")]
public class StoreProductsController(ProductService products) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PagedResult<ProductResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<ProductResponse>>> List(
        int storeId, [FromQuery] PageQuery query, [FromQuery] string? category, [FromQuery] string? search,
        CancellationToken ct)
    {
        var page = await products.ListByStoreAsync(storeId, category, search, query.SafePage, query.SafePageSize, ct);
        return Ok(new PagedResult<ProductResponse>(
            page.Items.Select(p => p.ToResponse()).ToList(), page.Page, page.PageSize, page.TotalItems));
    }

    [HttpPost]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Create(int storeId, ProductRequest req, CancellationToken ct)
    {
        var product = await products.CreateAsync(storeId, User.GetUserId(), req.ToCommand(), ct);
        // Location apunta a GET /api/v1/products/{id}
        return CreatedAtAction(nameof(ProductsController.GetById), "Products", new { id = product.Id }, product.ToResponse());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Update(int storeId, int id, ProductRequest req, CancellationToken ct)
        => Ok((await products.UpdateAsync(storeId, id, User.GetUserId(), req.ToCommand(), ct)).ToResponse());

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int storeId, int id, CancellationToken ct)
    {
        await products.DeleteAsync(storeId, id, User.GetUserId(), ct);
        return NoContent();
    }
}
