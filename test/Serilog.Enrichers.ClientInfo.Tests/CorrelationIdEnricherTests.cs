using Microsoft.AspNetCore.Http;
using NSubstitute;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests;

public class CorrelationIdEnricherTests
{
    private const string HeaderKey = "x-correlation-id";
    private const string LogPropertyName = "CorrelationId";
    private readonly IHttpContextAccessor _contextAccessor;

    public CorrelationIdEnricherTests()
    {
        DefaultHttpContext httpContext = new();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _contextAccessor.HttpContext.Returns(httpContext);
    }

    [Fact]
    public void EnrichLogWithCorrelationId_WhenHttpRequestContainCorrelationHeader_ShouldCreateCorrelationIdProperty()
    {
        // Arrange
        string correlationId = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = correlationId;
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, false, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a correlation id.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.Equal(correlationId, evt.Properties[LogPropertyName].LiteralValue().ToString());
    }

    [Fact]
    public void
        EnrichLogWithCorrelationId_WhenHttpRequestContainCorrelationHeader_ShouldCreateCorrelationIdPropertyHasValue()
    {
        // Arrange
        string correlationId = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = correlationId;
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, false, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a correlation id.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.Equal(correlationId, evt.Properties[LogPropertyName].LiteralValue().ToString());
    }

    [Fact]
    public void
        EnrichLogWithCorrelationId_WhenHttpRequestNotContainCorrelationHeaderAndAddDefaultValueIsFalse_ShouldCreateCorrelationIdPropertyWithNoValue()
    {
        // Arrange
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, false, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a correlation id.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.Null(evt.Properties[LogPropertyName].LiteralValue());
    }

    [Fact]
    public void
        EnrichLogWithCorrelationId_WhenHttpRequestNotContainCorrelationHeaderAndAddDefaultValueIsTrue_ShouldCreateCorrelationIdPropertyHasValue()
    {
        // Arrange
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, true, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a correlation id.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.NotNull(evt.Properties[LogPropertyName].LiteralValue().ToString());
    }

    [Fact]
    public void
        EnrichLogWithCorrelationId_WhenHttpResponseContainsCorrelationIdHeader_ShouldCreateCorrelationIdProperty()
    {
        // Arrange
        string correlationId = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = correlationId;
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, false, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a correlation id.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.Equal(correlationId, evt.Properties[LogPropertyName].LiteralValue().ToString());
    }

    [Fact]
    public void
        EnrichLogWithCorrelationId_WhenHttpRequestAndResponseContainCorrelationIdHeader_ShouldCreateCorrelationIdPropertyFromHttpRequest()
    {
        // Arrange
        string requestCorrelationId = Guid.NewGuid().ToString();
        string responseCorrelationId = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = requestCorrelationId;
        _contextAccessor.HttpContext!.Response!.Headers[HeaderKey] = responseCorrelationId;
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, false, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a correlation id.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.Equal(requestCorrelationId, evt.Properties[LogPropertyName].LiteralValue().ToString());
    }

    [Fact]
    public void GetCorrelationId_WhenHttpRequestContainCorrelationHeader_ShouldReturnCorrelationIdFromHttpContext()
    {
        // Arrange
        string correlationId = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = correlationId;
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, false, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a correlation id.");
        string retrievedCorrelationId = _contextAccessor.HttpContext!.GetCorrelationId();

        // Assert
        Assert.NotNull(evt);
        Assert.Equal(correlationId, retrievedCorrelationId);
    }

    [Fact]
    public void
        GetCorrelationId_WhenHttpRequestNotContainCorrelationHeaderAndAddDefaultValueIsTrue_ShouldReturnGeneratedCorrelationIdFromHttpContext()
    {
        // Arrange
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, true, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a correlation id.");
        string retrievedCorrelationId = _contextAccessor.HttpContext!.GetCorrelationId();

        // Assert
        Assert.NotNull(evt);
        Assert.NotNull(retrievedCorrelationId);
        Assert.NotEmpty(retrievedCorrelationId);
        // Verify it's a valid GUID format
        Assert.True(Guid.TryParse(retrievedCorrelationId, out _));
    }

    [Fact]
    public void
        GetCorrelationId_WhenHttpRequestNotContainCorrelationHeaderAndAddDefaultValueIsFalse_ShouldReturnNullFromHttpContext()
    {
        // Arrange
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, false, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a correlation id.");
        string retrievedCorrelationId = _contextAccessor.HttpContext!.GetCorrelationId();

        // Assert
        Assert.NotNull(evt);
        Assert.Null(retrievedCorrelationId);
    }

    [Fact]
    public void GetCorrelationId_WhenCalledMultipleTimes_ShouldReturnSameCorrelationId()
    {
        // Arrange
        string correlationId = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = correlationId;
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, false, _contextAccessor);

        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();

        // Act
        log.Information("First log message.");
        string firstRetrieval = _contextAccessor.HttpContext!.GetCorrelationId();

        log.Information("Second log message.");
        string secondRetrieval = _contextAccessor.HttpContext!.GetCorrelationId();

        // Assert
        Assert.Equal(correlationId, firstRetrieval);
        Assert.Equal(correlationId, secondRetrieval);
        Assert.Equal(firstRetrieval, secondRetrieval);
    }

    [Fact]
    public void GetCorrelationId_WhenHttpContextIsNull_ShouldReturnNull()
    {
        // Arrange & Act
        string result = HttpContextExtensions.GetCorrelationId(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void EnrichLogWithCorrelationId_BackwardCompatibility_OldRetrievalMethodShouldStillWork()
    {
        // Arrange
        string correlationId = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = correlationId;
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, false, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a correlation id.");

        // Test that the old way (hacky way) still works
        HttpContext httpContext = _contextAccessor.HttpContext!;
        string retrievedCorrelationIdOldWay = null;

        if (httpContext.Items.TryGetValue("Serilog_CorrelationId", out object correlationIdItem) &&
            correlationIdItem is LogEventProperty { Name: "CorrelationId" } correlationIdProperty)
            retrievedCorrelationIdOldWay = ((ScalarValue)correlationIdProperty.Value).Value as string;

        // Test that the new way also works
        string retrievedCorrelationIdNewWay = httpContext.GetCorrelationId();

        // Assert
        Assert.NotNull(evt);
        Assert.Equal(correlationId, retrievedCorrelationIdOldWay);
        Assert.Equal(correlationId, retrievedCorrelationIdNewWay);
        Assert.Equal(retrievedCorrelationIdOldWay, retrievedCorrelationIdNewWay);
    }

    [Fact]
    public void EnrichLogWithCorrelationId_WhenHeaderExceedsMaxLength_ShouldTreatHeaderAsAbsent()
    {
        // Arrange
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = new string('a', 129);
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, false, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a correlation id.");

        // Assert
        Assert.NotNull(evt);
        Assert.Null(evt.Properties[LogPropertyName].LiteralValue());
        Assert.Null(_contextAccessor.HttpContext!.GetCorrelationId());
    }

    [Fact]
    public void EnrichLogWithCorrelationId_WhenHeaderExceedsMaxLengthAndAddDefaultValueIsTrue_ShouldGenerateValue()
    {
        // Arrange
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = new string('a', 30000);
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, true, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a correlation id.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(Guid.TryParse((string)evt.Properties[LogPropertyName].LiteralValue(), out _));
        Assert.True(Guid.TryParse(_contextAccessor.HttpContext!.GetCorrelationId(), out _));
    }

    [Fact]
    public void EnrichLogWithCorrelationId_WhenHeaderIsWithinCustomMaxLength_ShouldUseHeaderValue()
    {
        // Arrange
        string correlationId = new('a', 200);
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = correlationId;
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, false, _contextAccessor, 200);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a correlation id.");

        // Assert
        Assert.NotNull(evt);
        Assert.Equal(correlationId, evt.Properties[LogPropertyName].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithCorrelationId_WhenHeaderContainsControlCharacters_ShouldTreatHeaderAsAbsent()
    {
        // Arrange
        string lineFeed = ((char)0x0A).ToString();
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = "abc" + lineFeed + "[12:00:00] INF forged";
        CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, false, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a correlation id.");

        // Assert
        Assert.NotNull(evt);
        Assert.Null(evt.Properties[LogPropertyName].LiteralValue());
        Assert.Null(_contextAccessor.HttpContext!.GetCorrelationId());
    }

    [Fact]
    public async Task EnrichLogWithCorrelationId_WhenLoggedConcurrentlyInOneRequest_ShouldUseOneCorrelationId()
    {
        // Arrange
        const int threads = 8;
        const int requests = 300;
        ConcurrentBag<string> failures = [];

        for (int request = 0; request < requests; request++)
        {
            DefaultHttpContext httpContext = new();
            IHttpContextAccessor contextAccessor = Substitute.For<IHttpContextAccessor>();
            contextAccessor.HttpContext.Returns(httpContext);
            CorrelationIdEnricher correlationIdEnricher = new(HeaderKey, true, contextAccessor);

            ConcurrentBag<LogEvent> events = [];
            Logger log = new LoggerConfiguration()
                .Enrich.With(correlationIdEnricher)
                .WriteTo.Sink(new DelegatingSink(events.Add))
                .CreateLogger();

            // Act
            using Barrier barrier = new(threads);
            Task[] tasks = Enumerable.Range(0, threads).Select(_ => Task.Factory.StartNew(() =>
            {
                barrier.SignalAndWait();
                log.Information("Has a correlation id.");
            }, TaskCreationOptions.LongRunning)).ToArray();
            await Task.WhenAll(tasks);

            // Assert
            string[] correlationIds = events
                .Select(e => e.Properties.TryGetValue(LogPropertyName, out LogEventPropertyValue value)
                    ? value.LiteralValue() as string
                    : null)
                .Distinct()
                .ToArray();

            if (events.Count != threads || correlationIds.Length != 1 || correlationIds[0] == null ||
                correlationIds[0] != httpContext.GetCorrelationId())
                failures.Add($"request {request}: {events.Count} events, ids: {string.Join(", ", correlationIds)}");
        }

        Assert.Empty(failures);
    }

    [Fact]
    public void WithCorrelationId_WithNullHeaderName_ShouldThrowArgumentNullException()
    {
        // Arrange
        LoggerConfiguration loggerConfiguration = new();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => loggerConfiguration.Enrich.WithCorrelationId(null));
    }

    [Fact]
    public void WithCorrelationId_WithNonPositiveMaxLength_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        LoggerConfiguration loggerConfiguration = new();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            loggerConfiguration.Enrich.WithCorrelationId(HeaderKey, true, 0));
    }

    [Fact]
    public void WithClientIp_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        // Arrange
        Logger logger = new LoggerConfiguration()
            .Enrich.WithCorrelationId()
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();

        // Act
        Exception exception = Record.Exception(() => logger.Information("LOG"));

        // Assert
        Assert.Null(exception);
    }
}