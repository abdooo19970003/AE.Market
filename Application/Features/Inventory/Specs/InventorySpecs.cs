using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Inventory.Specs;

public sealed class InventoryItemByVariantSpec : BaseSpecification<InventoryItem>
{
    public InventoryItemByVariantSpec(Guid variantId)
        : base(i => i.VariantId == variantId && !i.IsDeleted)
    {
    }
}

public sealed class InventoryItemByIdSpec : BaseSpecification<InventoryItem>
{
    public InventoryItemByIdSpec(Guid id)
        : base(i => i.Id == id && !i.IsDeleted)
    {
    }
}

public sealed class LowStockReportSpec : BaseSpecification<InventoryItem>
{
    public LowStockReportSpec()
        : base(i => i.LowStockThreshold > 0 && i.StockQuantity <= i.LowStockThreshold && !i.IsDeleted)
    {
    }

    public LowStockReportSpec(int page, int pageSize)
        : base(i => i.LowStockThreshold > 0 && i.StockQuantity <= i.LowStockThreshold && !i.IsDeleted)
    {
        SetPagination((page - 1) * pageSize, pageSize);
    }
}

public sealed class InventoryItemsListSpec : BaseSpecification<InventoryItem>
{
    public InventoryItemsListSpec()
        : base(i => !i.IsDeleted)
    {
    }

    public InventoryItemsListSpec(int page, int pageSize)
        : base(i => !i.IsDeleted)
    {
        SetPagination((page - 1) * pageSize, pageSize);
    }
}
