using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.CreateAttributeGroup;

internal sealed class CreateAttributeGroupCommandHandler(
    IRepository<AttributeGroup> repo,
    IMapper mapper
) : IRequestHandler<CreateAttributeGroupCommand, Result<AttributeGroupDto>>
{
    public async Task<Result<AttributeGroupDto>> Handle(CreateAttributeGroupCommand request, CancellationToken cancellationToken)
    {
        var group = AttributeGroup.Create(
            Guid.NewGuid(),
            request.CategoryId,
            request.GroupName,
            request.Slug,
            request.SortOrder
        );

        await repo.AddAsync(group, cancellationToken);

        var dto = mapper.Map<AttributeGroupDto>(group);
        return Result<AttributeGroupDto>.Success(dto);
    }
}
