using NetForge.Simulation.HttpHandlers.Common;
using NetForge.Simulation.HttpHandlers.Common.Services;
using NetForge.Simulation.HttpHandlers.Cisco;
using NetForge.Simulation.Protocols.HTTP;
using NetForge.Simulation.Topology.Devices.Cisco;
using Xunit;

namespace NetForge.Simulation.HttpHandlers.Tests;

/// <summary>
/// Integration tests for HTTP handlers with device system
/// </summary>
public class HttpHandlerIntegrationTests
{
    [Fact]
    public async Task CiscoDevice_HttpConfiguration_WorksCorrectly()
    {
        // Arrange
        var device = new CiscoDevice("test-cisco", "Cisco");
        var httpConfig = new HttpConfig
        {
            IsEnabled = true,
            Port = 8080,
            HttpsPort = 8443,
            ServerName = "TestCiscoDevice"
        };

        // Act
        device.SetHttpConfiguration(httpConfig);
        var retrievedConfig = device.GetHttpConfiguration();

        // Assert
        Assert.NotNull(retrievedConfig);
        Assert.Equal(8080, retrievedConfig.Port);
        Assert.Equal(8443, retrievedConfig.HttpsPort);
        Assert.Equal("TestCiscoDevice", retrievedConfig.ServerName);
        Assert.True(retrievedConfig.IsEnabled);
    }

    [Fact]
    public async Task CiscoHttpHandler_DeviceIntegration_WorksCorrectly()
    {
        // Arrange
        var device = new CiscoDevice("test-cisco", "Cisco");
        var authenticator = new HttpAuthenticator();
        var apiProvider = new HttpApiProvider();
        var contentProvider = new HttpContentProvider();

        var handler = new CiscoHttpHandler(authenticator, apiProvider, contentProvider);

        // Act
        await handler.Initialize(device);
        var endpoints = handler.GetSupportedEndpoints();

        // Assert
        Assert.NotEmpty(endpoints);
        Assert.Contains(endpoints, e => e.Path == "/" && e.Method == "GET");
        Assert.Contains(endpoints, e => e.Path == "/api/interfaces/configure" && e.Method == "POST");
        Assert.Equal("Cisco", handler.VendorName);
        Assert.Equal(200, handler.Priority); // Vendor-specific priority
    }

    [Fact]
    public async Task HttpHandlerManager_CiscoDeviceRouting_WorksCorrectly()
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
        Assert.IsType<CiscoHttpHandler>(retrievedHandler);
    }

    [Fact]
    public async Task HttpHandlerManager_GenericFallback_WorksCorrectly()
    {
        // Arrange
        var handlerManager = new HttpHandlerManager();

        // Act
        var genericHandler = handlerManager.GetHandler("UnknownVendor", "/");

        // Assert
        Assert.NotNull(genericHandler);
        Assert.Equal("Generic", genericHandler?.VendorName);
    }

    [Fact]
    public async Task HttpProtocol_ConfigurationIntegration_WorksCorrectly()
    {
        // Arrange
        var device = new CiscoDevice("test-cisco", "Cisco");
        var protocol = new HttpProtocol();

        var httpConfig = new HttpConfig
        {
            IsEnabled = true,
            Port = 8080,
            HttpsPort = 8443
        };

        // Act
        device.SetHttpConfiguration(httpConfig);
        var retrievedConfig = device.GetHttpConfiguration();

        // Assert
        Assert.NotNull(retrievedConfig);
        Assert.Equal(httpConfig.Port, retrievedConfig.Port);
        Assert.Equal(httpConfig.HttpsPort, retrievedConfig.HttpsPort);
    }

    [Fact]
    public async Task HttpContext_DeviceContext_WorksCorrectly()
    {
        // Arrange
        var device = new CiscoDevice("test-cisco", "Cisco");
        var context = new HttpContext
        {
            Device = device,
            Request = new HttpRequest
            {
                Path = "/",
                Method = "GET",
                Headers = { ["User-Agent"] = "TestAgent/1.0" }
            }
        };

        // Act
        var deviceFromContext = context.Device;

        // Assert
        Assert.NotNull(deviceFromContext);
        Assert.Equal("test-cisco", deviceFromContext.Name);
        Assert.Equal("Cisco", deviceFromContext.Vendor);
    }

    [Fact]
    public async Task HttpRequest_QueryParameters_WorksCorrectly()
    {
        // Arrange
        var request = new HttpRequest
        {
            QueryString = "?param1=value1&param2=value2",
            QueryParameters =
            {
                ["param1"] = "value1",
                ["param2"] = "value2"
            }
        };

        // Act
        var param1 = request.GetQueryParameter("param1");
        var param2 = request.GetQueryParameter("param2");
        var param3 = request.GetQueryParameter("param3"); // Non-existent

        // Assert
        Assert.Equal("value1", param1);
        Assert.Equal("value2", param2);
        Assert.Equal("", param3); // Should return empty string for non-existent
    }

    [Fact]
    public async Task HttpRequest_Headers_WorksCorrectly()
    {
        // Arrange
        var request = new HttpRequest
        {
            Headers =
            {
                ["Content-Type"] = "application/json",
                ["Authorization"] = "Bearer token123",
                ["User-Agent"] = "TestAgent/1.0"
            }
        };

        // Act
        var contentType = request.GetHeader("Content-Type");
        var auth = request.GetHeader("Authorization");
        var userAgent = request.GetHeader("User-Agent");

        // Assert
        Assert.Equal("application/json", contentType);
        Assert.Equal("Bearer token123", auth);
        Assert.Equal("TestAgent/1.0", userAgent);
    }

    [Fact]
    public async Task HttpResponse_JsonSerialization_WorksCorrectly()
    {
        // Arrange
        var response = new HttpResponse();
        var testData = new { message = "test", id = 123 };

        // Act
        response.SetJsonBody(testData);

        // Assert
        Assert.Equal("application/json", response.Headers["Content-Type"]);
        Assert.Contains("test", System.Text.Encoding.UTF8.GetString(response.Body));
        Assert.Contains("123", System.Text.Encoding.UTF8.GetString(response.Body));
    }

    [Fact]
    public async Task HttpResponse_HtmlSerialization_WorksCorrectly()
    {
        // Arrange
        var response = new HttpResponse();
        var html = "<html><body><h1>Test</h1></body></html>";

        // Act
        response.SetHtmlBody(html);

        // Assert
        Assert.Equal("text/html", response.Headers["Content-Type"]);
        Assert.Equal(html, System.Text.Encoding.UTF8.GetString(response.Body));
    }

    [Fact]
    public async Task HttpResult_FactoryMethods_WorksCorrectly()
    {
        // Arrange & Act
        var okResult = HttpResult.Ok("Success");
        var badRequest = HttpResult.BadRequest("Invalid input");
        var notFound = HttpResult.NotFound("Resource not found");
        var error = HttpResult.Error(500, "Internal error");

        // Assert
        Assert.Equal(200, okResult.StatusCode);
        Assert.True(okResult.IsSuccess);

        Assert.Equal(400, badRequest.StatusCode);
        Assert.False(badRequest.IsSuccess);

        Assert.Equal(404, notFound.StatusCode);
        Assert.False(notFound.IsSuccess);

        Assert.Equal(500, error.StatusCode);
        Assert.False(error.IsSuccess);
    }

    [Fact]
    public async Task HttpEndpoint_Matching_WorksCorrectly()
    {
        // Arrange
        var endpoint = new HttpEndpoint
        {
            Path = "/api/test",
            Method = "POST"
        };

        // Act & Assert
        Assert.True(endpoint.Matches("/api/test", "POST"));
        Assert.True(endpoint.Matches("/api/test")); // Method-agnostic
        Assert.False(endpoint.Matches("/api/other", "POST"));
        Assert.False(endpoint.Matches("/api/test", "GET"));
    }

    [Fact]
    public async Task HttpUser_Permissions_WorksCorrectly()
    {
        // Arrange
        var user = new HttpUser
        {
            Username = "admin",
            Role = "administrator",
            Permissions = { "read", "write", "admin" }
        };

        // Act & Assert
        Assert.True(user.HasPermission("read"));
        Assert.True(user.HasPermission("write"));
        Assert.True(user.HasPermission("admin"));
        Assert.False(user.HasPermission("delete"));

        Assert.True(user.HasRole("administrator"));
        Assert.False(user.HasRole("user"));
    }

    [Fact]
    public async Task HttpSession_DataManagement_WorksCorrectly()
    {
        // Arrange
        var session = new HttpSession
        {
            Id = "test-session",
            User = new HttpUser { Username = "testuser" },
            IsActive = true
        };

        // Act
        session.SetData("key1", "value1");
        session.SetData("key2", 42);

        var stringValue = session.GetData<string>("key1");
        var intValue = session.GetData<int>("key2");
        var missingValue = session.GetData<string>("key3");

        // Assert
        Assert.Equal("value1", stringValue);
        Assert.Equal(42, intValue);
        Assert.Null(missingValue);
    }
}