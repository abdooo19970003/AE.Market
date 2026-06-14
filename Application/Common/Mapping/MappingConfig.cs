using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Catalog.Units;
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
            .Map(dest => dest.Url, src => src.Url.Value);

        config.NewConfig<ProductVariant, VariantDto>()
            .Map(dest => dest.Sku, src => src.Sku.Value);

        config.NewConfig<Product, ProductDetailDto>()
            .Map(dest => dest.Sku, src => src.Sku.Value)
            .Map(dest => dest.ProductType, src => src.ProductType.ToString())
            .Map(dest => dest.Url, src => src.Url.Value)
            .Map(dest => dest.Variants, src => src.Variants.Adapt<List<VariantDto>>())
            .Map(dest => dest.Images, src => src.Images.Select(i => i.Url).ToList());

        config.Compile();
        return config;
    }
}
