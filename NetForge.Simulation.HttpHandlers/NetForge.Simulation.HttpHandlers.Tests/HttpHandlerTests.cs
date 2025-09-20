using System.Net;
using NetForge.Simulation.HttpHandlers.Common;
using NetForge.Simulation.HttpHandlers.Common.Services;
using NetForge.Simulation.HttpHandlers.Cisco;
using NetForge.Simulation.HttpHandlers.Generic;
using NetForge.Simulation.Protocols.HTTP;
using NetForge.Simulation.Topology.Devices;
using Xunit;

namespace NetForge.Simulation.HttpHandlers.Tests;

/// <summary>
/// Comprehensive unit tests for HTTP handler system
/// </summary>
public class HttpHandlerTests
{
    [Fact]
    public async Task HttpConfig_Validation_WorksCorrectly()
    {
        // Arrange
        var config = new HttpConfig
        {
            Port = 80,
            HttpsPort = 443,
            MaxConnections = 100,
            RequestTimeout = 30,
            SessionTimeout = 1800
        };

        // Act
        var isValid = config.Validate();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task HttpConfig_InvalidPort_FailsValidation()
    {
        // Arrange
        var config = new HttpConfig
        {
            Port = 70000, // Invalid port
            HttpsPort = 443
        };

        // Act
        var isValid = config.Validate();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task HttpResult_SuccessCreation_WorksCorrectly()
    {
        // Arrange & Act
        var result = HttpResult.Ok("Test content", "application/json");

        // Assert
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.IsSuccess);
        Assert.Equal("application/json", result.ContentType);
        Assert.Equal("Test content", result.Content);
    }

    [Fact]
    public async Task HttpResult_ErrorCreation_WorksCorrectly()
    {
        // Arrange & Act
        var result = HttpResult.NotFound("Resource not found");

        // Assert
        Assert.Equal(404, result.StatusCode);
        Assert.False(result.IsSuccess);
        Assert.Equal("application/json", result.ContentType);
        Assert.NotNull(result.Content);
    }

    [Fact]
    public async Task HttpContext_ItemManagement_WorksCorrectly()
    {
        // Arrange
        var context = new HttpContext();

        // Act
        context.SetItem("testKey", "testValue");
        var retrievedValue = context.GetItem<string>("testKey");

        // Assert
        Assert.Equal("testValue", retrievedValue);
    }

    [Fact]
    public async Task HttpRequest_BodyParsing_WorksCorrectly()
    {
        // Arrange
        var request = new HttpRequest
        {
            Body = System.Text.Encoding.UTF8.GetBytes("{\"name\":\"test\"}"),
            Headers = { ["Content-Type"] = "application/json" }
        };

        // Act
        var jsonObject = request.GetBodyAsJson<TestObject>();

        // Assert
        Assert.NotNull(jsonObject);
        Assert.Equal("test", jsonObject?.Name);
    }

    [Fact]
    public async Task HttpResponse_HeaderManagement_WorksCorrectly()
    {
        // Arrange
        var response = new HttpResponse();

        // Act
        response.SetHeader("X-Custom", "test-value");
        var headerValue = response.GetHeader("X-Custom");

        // Assert
        Assert.Equal("test-value", headerValue);
    }

    [Fact]
    public async Task HttpResponse_CookieManagement_WorksCorrectly()
    {
        // Arrange
        var response = new HttpResponse();
        var options = new CookieOptions
        {
            Expires = DateTime.UtcNow.AddDays(1),
            HttpOnly = true,
            Secure = true
        };

        // Act
        response.SetCookie("session", "abc123", options);

        // Assert
        Assert.Contains("session=abc123", response.Cookies["session"]);
        Assert.Contains("HttpOnly", response.Cookies["session"]);
        Assert.Contains("Secure", response.Cookies["session"]);
    }

    [Fact]
    public async Task HttpAuthenticator_BasicAuth_WorksCorrectly()
    {
        // Arrange
        var authenticator = new HttpAuthenticator();
        var context = new HttpContext
        {
            Request = new HttpRequest
            {
                Headers = { ["Authorization"] = "Basic " + Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes("admin:admin")) }
            }
        };

        // Act
        var result = await authenticator.AuthenticateRequest(context);

        // Assert
        Assert.True(result.IsAuthenticated);
        Assert.Equal(HttpAuthMethod.Basic, result.AuthMethod);
        Assert.NotNull(result.User);
        Assert.Equal("admin", result.User?.Username);
    }

    [Fact]
    public async Task HttpAuthenticator_InvalidCredentials_Fails()
    {
        // Arrange
        var authenticator = new HttpAuthenticator();
        var context = new HttpContext
        {
            Request = new HttpRequest
            {
                Headers = { ["Authorization"] = "Basic " + Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes("admin:wrongpassword")) }
            }
        };

        // Act
        var result = await authenticator.AuthenticateRequest(context);

        // Assert
        Assert.False(result.IsAuthenticated);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task HttpSessionManager_SessionCreation_WorksCorrectly()
    {
        // Arrange
        var sessionManager = new HttpSessionManager(30);
        var user = new HttpUser { Username = "testuser" };

        // Act
        var session = sessionManager.CreateSession(user);

        // Assert
        Assert.NotNull(session);
        Assert.NotEmpty(session.Id);
        Assert.Equal("testuser", session.User.Username);
        Assert.True(session.IsActive);
    }

    [Fact]
    public async Task HttpSessionManager_SessionRetrieval_WorksCorrectly()
    {
        // Arrange
        var sessionManager = new HttpSessionManager(30);
        var user = new HttpUser { Username = "testuser" };
        var session = sessionManager.CreateSession(user);

        // Act
        var retrievedSession = sessionManager.GetSession(session.Id);

        // Assert
        Assert.NotNull(retrievedSession);
        Assert.Equal(session.Id, retrievedSession?.Id);
        Assert.Equal("testuser", retrievedSession?.User.Username);
    }

    [Fact]
    public async Task HttpSessionManager_SessionExpiration_WorksCorrectly()
    {
        // Arrange
        var sessionManager = new HttpSessionManager(0); // Expire immediately
        var user = new HttpUser { Username = "testuser" };
        var session = sessionManager.CreateSession(user);

        // Act
        var retrievedSession = sessionManager.GetSession(session.Id);

        // Assert
        Assert.Null(retrievedSession); // Should be expired and cleaned up
    }

    [Fact]
    public async Task HttpHandlerManager_HandlerDiscovery_WorksCorrectly()
    {
        // Arrange
        var handlerManager = new HttpHandlerManager();

        // Act
        var handlers = handlerManager.GetAllHandlers();

        // Assert
        Assert.NotEmpty(handlers);
        Assert.Contains(handlers, h => h.VendorName == "Cisco");
        Assert.Contains(handlers, h => h.VendorName == "Generic");
    }

    [Fact]
    public async Task HttpHandlerManager_VendorSpecificRouting_WorksCorrectly()
    {
        // Arrange
        var handlerManager = new HttpHandlerManager();
        var ciscoHandler = handlerManager.GetAllHandlers()
            .First(h => h.VendorName == "Cisco");

        // Act
        var retrievedHandler = handlerManager.GetHandler("Cisco", "/");

        // Assert
        Assert.NotNull(retrievedHandler);
        Assert.Equal("Cisco", retrievedHandler?.VendorName);
    }

    [Fact]
    public async Task CiscoHttpHandler_EndpointSupport_WorksCorrectly()
    {
        // Arrange
        var handler = new CiscoHttpHandler(
            new HttpAuthenticator(),
            new HttpApiProvider(),
            new HttpContentProvider()
        );

        // Act
        var endpoints = handler.GetSupportedEndpoints();

        // Assert
        Assert.NotEmpty(endpoints);
        Assert.Contains(endpoints, e => e.Path == "/" && e.Method == "GET");
        Assert.Contains(endpoints, e => e.Path == "/api/interfaces/configure" && e.Method == "POST");
    }

    [Fact]
    public async Task GenericHttpHandler_EndpointSupport_WorksCorrectly()
    {
        // Arrange
        var handler = new GenericHttpHandler(
            new HttpAuthenticator(),
            new HttpApiProvider(),
            new HttpContentProvider()
        );

        // Act
        var endpoints = handler.GetSupportedEndpoints();

        // Assert
        Assert.NotEmpty(endpoints);
        Assert.Contains(endpoints, e => e.Path == "/" && e.Method == "GET");
        Assert.Contains(endpoints, e => e.Path == "/api/system/info" && e.Method == "GET");
    }

    [Fact]
    public async Task HttpContentProvider_ContentTypeDetection_WorksCorrectly()
    {
        // Arrange
        var provider = new HttpContentProvider();

        // Act & Assert
        Assert.Equal("text/html", provider.GetContentType(".html"));
        Assert.Equal("text/css", provider.GetContentType(".css"));
        Assert.Equal("application/javascript", provider.GetContentType(".js"));
        Assert.Equal("image/png", provider.GetContentType(".png"));
        Assert.Equal("application/json", provider.GetContentType(".json"));
    }

    [Fact]
    public async Task HttpApiProvider_EndpointRegistration_WorksCorrectly()
    {
        // Arrange
        var provider = new HttpApiProvider();

        // Act
        var hasEndpoint = provider.HasEndpoint("Cisco", "/api/system/info");

        // Assert
        Assert.True(hasEndpoint);
    }

    [Fact]
    public async Task HttpApiProvider_EndpointRetrieval_WorksCorrectly()
    {
        // Arrange
        var provider = new HttpApiProvider();

        // Act
        var endpoints = provider.GetApiEndpoints("Cisco");

        // Assert
        Assert.NotEmpty(endpoints);
        Assert.Contains(endpoints, e => e.Path == "/api/system/info");
        Assert.Contains(endpoints, e => e.Path == "/api/interfaces");
    }

    // Supporting test classes
    private class TestObject
    {
        public string? Name { get; set; }
    }
}