using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Common;

public sealed class DomainExceptionTests
{
    [Fact]
    public void Create_WithCodeAndMessage_SetsProperties()
    {
        var exception = new DomainException("TEST_CODE", "Test message");

        exception.Code.Should().Be("TEST_CODE");
        exception.Message.Should().Be("Test message");
    }

    [Fact]
    public void Create_WithCodeMessageAndInnerException_SetsAll()
    {
        var inner = new InvalidOperationException("Inner");
        var exception = new DomainException("TEST_CODE", "Test message", inner);

        exception.Code.Should().Be("TEST_CODE");
        exception.Message.Should().Be("Test message");
        exception.InnerException.Should().BeSameAs(inner);
    }
}
