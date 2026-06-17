using AE.Market.Domain.Common.Abstracts;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Common;

public sealed class ErrorTests
{
    [Fact]
    public void Create_WithCodeAndMessage_SetsProperties()
    {
        var error = new Error("CODE", "Message");

        error.Code.Should().Be("CODE");
        error.Message.Should().Be("Message");
    }

    [Fact]
    public void None_IsDefault()
    {
        Error.None.Code.Should().Be(string.Empty);
        Error.None.Message.Should().Be(string.Empty);
    }

    [Fact]
    public void ImplicitConversion_ToValueTuple()
    {
        var error = new Error("CODE", "Message");

        var (code, message) = error;

        code.Should().Be("CODE");
        message.Should().Be("Message");
    }

    [Fact]
    public void Equality_SameCodeAndMessage_AreEqual()
    {
        var a = new Error("CODE", "Message");
        var b = new Error("CODE", "Message");

        a.Should().Be(b);
    }

    [Fact]
    public void Inequality_DifferentCode_AreNotEqual()
    {
        var a = new Error("CODE1", "Message");
        var b = new Error("CODE2", "Message");

        a.Should().NotBe(b);
    }
}
