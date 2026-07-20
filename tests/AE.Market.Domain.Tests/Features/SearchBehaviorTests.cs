using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Search.DTOs;
using AE.Market.Domain.Common.Abstracts;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace AE.Market.Domain.Tests.Features;

public sealed class SearchBehaviorTests
{
    private readonly Mock<IElasticsearchService> _esServiceMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();

    [Fact]
    public async Task Handle_non_search_query_delegates_to_next()
    {
        var query = new NonSearchQuery();
        var expected = Result<string>.Success("ok");
        RequestHandlerDelegate<Result<string>> next = _ => Task.FromResult(expected);

        var behavior = new SearchBehavior<NonSearchQuery, Result<string>>(
            _esServiceMock.Object, _currentUserMock.Object);

        var result = await behavior.Handle(query, next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _esServiceMock.Verify(
            x => x.SearchProductsAsync(It.IsAny<SearchProductsQuery>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_search_products_query_calls_elasticsearch()
    {
        var query = new SearchProductsQuery { Q = "laptop" };
        var searchResult = new SearchProductsResult { Items = [], TotalCount = 5 };
        _esServiceMock
            .Setup(x => x.SearchProductsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        RequestHandlerDelegate<Result<SearchProductsResult>> next =
            _ => throw new InvalidOperationException("Next should not be called for search queries");

        var behavior = new SearchBehavior<SearchProductsQuery, Result<SearchProductsResult>>(
            _esServiceMock.Object, _currentUserMock.Object);

        var result = await behavior.Handle(query, next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(5);
        _esServiceMock.Verify(
            x => x.SearchProductsAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_search_returns_error_when_service_fails()
    {
        var query = new SearchProductsQuery { Q = "laptop" };
        _esServiceMock
            .Setup(x => x.SearchProductsAsync(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("ES unavailable"));

        RequestHandlerDelegate<Result<SearchProductsResult>> next =
            _ => throw new InvalidOperationException("Next should not be called");

        var behavior = new SearchBehavior<SearchProductsQuery, Result<SearchProductsResult>>(
            _esServiceMock.Object, _currentUserMock.Object);

        var act = () => behavior.Handle(query, next, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("ES unavailable");
    }

    [Fact]
    public async Task Handle_search_suggest_query_calls_elasticsearch()
    {
        var query = new SearchSuggestQuery { Q = "phone" };
        var suggestResult = new SearchSuggestResult
        {
            Suggestions = [new SuggestionItem { Text = "phone case", Score = 0.9f }]
        };
        _esServiceMock
            .Setup(x => x.SearchSuggestAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(suggestResult);

        RequestHandlerDelegate<Result<SearchSuggestResult>> next =
            _ => throw new InvalidOperationException("Next should not be called for search queries");

        var behavior = new SearchBehavior<SearchSuggestQuery, Result<SearchSuggestResult>>(
            _esServiceMock.Object, _currentUserMock.Object);

        var result = await behavior.Handle(query, next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Suggestions.Should().HaveCount(1);
        _esServiceMock.Verify(
            x => x.SearchSuggestAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_search_brands_query_calls_elasticsearch()
    {
        var query = new SearchBrandsQuery { Q = "apple" };
        var brandResult = new SearchBrandsResult
        {
            Items = [new SearchBrandsResultItem { Id = Guid.NewGuid(), Name = "Apple", ProductCount = 5 }],
            TotalCount = 1
        };
        _esServiceMock
            .Setup(x => x.SearchBrandsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(brandResult);

        RequestHandlerDelegate<Result<SearchBrandsResult>> next =
            _ => throw new InvalidOperationException("Next should not be called for search queries");

        var behavior = new SearchBehavior<SearchBrandsQuery, Result<SearchBrandsResult>>(
            _esServiceMock.Object, _currentUserMock.Object);

        var result = await behavior.Handle(query, next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items.Should().HaveCount(1);
        _esServiceMock.Verify(
            x => x.SearchBrandsAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private sealed class NonSearchQuery : IRequest<Result<string>>;
}
