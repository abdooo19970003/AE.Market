using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateAttributeGroup;

internal sealed class UpdateAttributeGroupCommandHandler(
    IRepository<AttributeGroup> repo,
    IMapper mapper
) : IRequestHandler<UpdateAttributeGroupCommand, Result<AttributeGroupDto>>
{
    public async Task<Result<AttributeGroupDto>> Handle(UpdateAttributeGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (group is null)
            return Result<AttributeGroupDto>.Fail(CatalogErrors.AttributeGroupNotFound);

        group.Rename(request.GroupName, request.Slug, request.SortOrder);

        var dto = mapper.Map<AttributeGroupDto>(group);
        return Result<AttributeGroupDto>.Success(dto);
    }
}
