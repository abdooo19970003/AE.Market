using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.CreateProductTaxCode;

internal sealed class CreateProductTaxCodeCommandHandler(
    IRepository<ProductTaxCode> repo,
    IMapper mapper
) : IRequestHandler<CreateProductTaxCodeCommand, Result<ProductTaxCodeDto>>
{
    public async Task<Result<ProductTaxCodeDto>> Handle(CreateProductTaxCodeCommand request, CancellationToken cancellationToken)
    {
        var taxCode = ProductTaxCode.Create(
            Guid.NewGuid(),
            request.Code,
            request.Type,
            request.PerformanceLocationRequirement,
            request.Name,
            request.Description
        );

        await repo.AddAsync(taxCode, cancellationToken);

        var dto = mapper.Map<ProductTaxCodeDto>(taxCode);
        return Result<ProductTaxCodeDto>.Success(dto);
    }
}
