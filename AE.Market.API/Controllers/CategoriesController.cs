using AE.Market.API.Authentication;
using AE.Market.API.Helpers;
using AE.Market.Application.Features.Catalog.Commands.AssignAttributeToGroup;
using AE.Market.Application.Features.Catalog.Commands.CreateAttributeGroup;
using AE.Market.Application.Features.Catalog.Commands.CreateCategory;
using AE.Market.Application.Features.Catalog.Commands.CreateCategoryAttribute;
using AE.Market.Application.Features.Catalog.Commands.DeleteAttributeGroup;
using AE.Market.Application.Features.Catalog.Commands.DeleteCategory;
using AE.Market.Application.Features.Catalog.Commands.DeleteCategoryAttribute;
using AE.Market.Application.Features.Catalog.Commands.UpdateAttributeGroup;
using AE.Market.Application.Features.Catalog.Commands.UpdateCategory;
using AE.Market.Application.Features.Catalog.Commands.UpdateCategoryAttribute;
using AE.Market.Application.Features.Catalog.Queries.Categories;
using AE.Market.Application.Features.Catalog.Queries.AttributeGroups;
using AE.Market.Application.Features.Catalog.Queries.CategoryAttributes;
using AE.Market.Application.Features.Catalog.Queries.RequiredAttributes;
using AE.Market.Domain.Aggregates.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class CategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoriesListQuery(), ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoryByIdQuery(id), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpPost]
    [Authorize]
    [HasPermission(Permission.MutateCategories)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateCategories)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand cmd, CancellationToken ct)
    {
        if (id != cmd.Id)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateCategories)]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteCategoryCommand(id), ct);
        return result.ToDeletedActionResult();
    }

    [HttpGet("{categoryId:guid}/attributes")]
    public async Task<IActionResult> GetCategoryAttributes(Guid categoryId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoryAttributesByCategoryQuery(categoryId), ct);
        return result.ToActionResult();
    }

    [HttpGet("{categoryId:guid}/attributes/{attributeId:guid}")]
    public async Task<IActionResult> GetCategoryAttributeById(Guid categoryId, Guid attributeId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoryAttributeByIdQuery(attributeId), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpPost("{categoryId:guid}/attributes")]
    [Authorize]
    [HasPermission(Permission.MutateCategories)]
    public async Task<IActionResult> AddCategoryAttribute(Guid categoryId, [FromBody] CreateCategoryAttributeCommand cmd, CancellationToken ct)
    {
        if (categoryId != cmd.CategoryId)
            return BadRequest("CategoryId mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpPut("{categoryId:guid}/attributes/{attributeId:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateCategories)]
    public async Task<IActionResult> UpdateCategoryAttribute(Guid categoryId, Guid attributeId, [FromBody] UpdateCategoryAttributeCommand cmd, CancellationToken ct)
    {
        if (attributeId != cmd.Id)
            return BadRequest("AttributeId mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{categoryId:guid}/attributes/{attributeId:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateCategories)]
    public async Task<IActionResult> DeleteCategoryAttribute(Guid categoryId, Guid attributeId, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteCategoryAttributeCommand(attributeId), ct);
        return result.ToDeletedActionResult();
    }

    [HttpGet("{categoryId:guid}/required-attributes")]
    public async Task<IActionResult> GetRequiredAttributes(Guid categoryId, CancellationToken ct, [FromQuery] Guid? productId = null)
    {
        var result = await mediator.Send(new GetRequiredAttributesForProductQuery(categoryId, productId), ct);
        return result.ToActionResult();
    }

    [HttpGet("{categoryId:guid}/attribute-groups")]
    public async Task<IActionResult> GetAttributeGroups(Guid categoryId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAttributeGroupsByCategoryQuery(categoryId), ct);
        return result.ToActionResult();
    }

    [HttpGet("{categoryId:guid}/attribute-groups/{groupId:guid}")]
    public async Task<IActionResult> GetAttributeGroupById(Guid categoryId, Guid groupId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAttributeGroupByIdQuery(groupId), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpPost("{categoryId:guid}/attribute-groups")]
    [Authorize]
    [HasPermission(Permission.MutateCategories)]
    public async Task<IActionResult> CreateAttributeGroup(Guid categoryId, [FromBody] CreateAttributeGroupCommand cmd, CancellationToken ct)
    {
        if (categoryId != cmd.CategoryId)
            return BadRequest("CategoryId mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpPut("{categoryId:guid}/attribute-groups/{groupId:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateCategories)]
    public async Task<IActionResult> UpdateAttributeGroup(Guid categoryId, Guid groupId, [FromBody] UpdateAttributeGroupCommand cmd, CancellationToken ct)
    {
        if (groupId != cmd.Id)
            return BadRequest("GroupId mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{categoryId:guid}/attribute-groups/{groupId:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateCategories)]
    public async Task<IActionResult> DeleteAttributeGroup(Guid categoryId, Guid groupId, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteAttributeGroupCommand(groupId), ct);
        return result.ToDeletedActionResult();
    }

    [HttpPatch("{categoryId:guid}/attribute-groups/{groupId:guid}/assign/{attributeId:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateCategories)]
    public async Task<IActionResult> AssignAttributeToGroup(Guid categoryId, Guid groupId, Guid attributeId, CancellationToken ct)
    {
        var result = await mediator.Send(new AssignAttributeToGroupCommand(attributeId, groupId), ct);
        return result.ToActionResult();
    }
}
