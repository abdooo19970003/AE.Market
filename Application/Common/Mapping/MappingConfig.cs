using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Inventory.DTOs;
using AE.Market.Application.Features.Pricing.DTOs;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Catalog.Units;
using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Common.ValueObjects;
using Mapster;

namespace AE.Market.Application.Common.Mapping;

public static class MappingConfig
{
    public static TypeAdapterConfig Configure()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.NewConfig<Address, AddressDto>();

        config.NewConfig<UserProfile, UserProfileDto>();

        config.NewConfig<User, UserDetailsDto>()
            .Map(dest => dest.Permissions, src => src.Permissions.Select(x => x.Permission.ToString()).ToList())
            .Map(dest => dest.RefreshTokens, src => src.RefreshTokens.Adapt<List<RefreshTokenDto>>())
            .Map(dest => dest.Profile, src => src.Profile == null ? null : src.Profile.Adapt<UserProfileDto>());

        config.NewConfig<RefreshToken, RefreshTokenDto>()
            .Ignore(dest => dest.Token);

        config.NewConfig<User, UsersListItemDto>();

        config.NewConfig<Category, CategoryDto>()
            .Map(dest => dest.CategoryUrl, src => src.CategoryUrl.Value);

        config.NewConfig<Brand, BrandDto>()
            .Map(dest => dest.Slug, src => src.Slug.Value)
            .Map(dest => dest.WebsiteUrl, src => src.WebsiteUrl == null ? null : src.WebsiteUrl.Value);

        config.NewConfig<ProductTaxCode, ProductTaxCodeDto>();

        config.NewConfig<GroupUnit, GroupUnitDto>();
        config.NewConfig<Unit, UnitDto>();

        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.Sku, src => src.Sku.Value)
            .Map(dest => dest.Slug, src => src.Slug.Value)
            .Map(dest => dest.ProductType, src => src.ProductType.ToString())
            .Map(dest => dest.Url, src => src.Url.Value)
            .Map(dest => dest.Status, src => src.Status.ToString());

        config.NewConfig<ProductVariant, VariantDto>()
            .Map(dest => dest.Sku, src => src.Sku.Value)
            .Map(dest => dest.Status, src => src.Status.ToString());

        config.NewConfig<Product, ProductDetailDto>()
            .Map(dest => dest.Sku, src => src.Sku.Value)
            .Map(dest => dest.ProductType, src => src.ProductType.ToString())
            .Map(dest => dest.Url, src => src.Url.Value)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Variants, src => src.Variants.Adapt<List<VariantDto>>())
            .Map(dest => dest.Images, src => src.Images.Select(i => i.Url).ToList());

        config.NewConfig<Tag, TagDto>()
            .Map(dest => dest.Slug, src => src.Slug.Value);

        config.NewConfig<ProductImage, ProductImageDto>();

        config.NewConfig<ProductRelation, ProductRelationDto>()
            .Map(dest => dest.Type, src => src.Type.ToString());

        config.NewConfig<BundleItem, BundleItemDto>()
            .Map(dest => dest.ItemName, src => src.Item == null ? null : src.Item.Name);

        config.NewConfig<AttributeOption, AttributeOptionDto>();

        config.NewConfig<CategoryAttribute, CategoryAttributeDto>()
            .Map(dest => dest.Slug, src => src.Slug == null ? null : src.Slug.Value)
            .Map(dest => dest.InputType, src => src.InputType.ToString())
            .Map(dest => dest.Options, src => src.Options.Adapt<List<AttributeOptionDto>>());

        config.NewConfig<AttributeGroup, AttributeGroupDto>()
            .Map(dest => dest.Slug, src => src.Slug == null ? null : src.Slug.Value)
            .Map(dest => dest.AttributeIds, src => src.AttributeIds.ToList());

        config.NewConfig<Price, PriceDto>()
            .Map(dest => dest.Amount, src => src.PriceAmount.Amount)
            .Map(dest => dest.Currency, src => src.PriceAmount.Currency.Code)
            .Map(dest => dest.Type, src => src.Type.ToString())
            .Map(dest => dest.IsActive, src => src.ValidTo == null);

        config.NewConfig<PriceHistory, PriceHistoryDto>()
            .Map(dest => dest.OldAmount, src => src.OldAmount.Amount)
            .Map(dest => dest.NewAmount, src => src.NewAmount.Amount)
            .Map(dest => dest.Currency, src => src.NewAmount.Currency.Code)
            .Map(dest => dest.Reason, src => src.Reason.ToString());

        config.NewConfig<InventoryItem, InventoryItemDto>();

        config.Compile();
        return config;
    }
}
