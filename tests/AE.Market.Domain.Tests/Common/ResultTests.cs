using AE.Market.Domain.Common.Abstracts;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Common;

public sealed class ResultTests
{
    private sealed class TestResult : Result
    {
        public TestResult(bool isSuccess, Error error) : base(isSuccess, error)
        {
        }
    }

    [Fact]
    public void Success_IsSuccess()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Success_ErrorIsNone()
    {
        var result = Result.Success();

        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Fail_IsFailure()
    {
        var error = new Error("CODE", "msg");
        var result = Result.Fail(error);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Fail_ErrorIsSet()
    {
        var error = new Error("CODE", "msg");
        var result = Result.Fail(error);

        result.Error.Should().Be(error);
    }

    [Fact]
    public void SuccessOfT_HasValue()
    {
        var result = Result<int>.Success(42);

        result.Value.Should().Be(42);
    }

    [Fact]
    public void FailOfT_ValueAccess_Throws()
    {
        var result = Result<int>.Fail(new Error("CODE", "msg"));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConstructingSuccessWithNonNoneError_Throws()
    {
        var error = new Error("CODE", "msg");

        var act = () => new TestResult(true, error);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConstructingFailureWithNoneError_Throws()
    {
        var act = () => new TestResult(false, Error.None);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SuccessOfT_ErrorIsNone()
    {
        var result = Result<int>.Success(42);

        result.Error.Should().Be(Error.None);
    }
}
