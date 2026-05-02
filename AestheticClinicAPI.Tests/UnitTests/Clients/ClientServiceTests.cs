using Moq;
using Xunit;
using AestheticClinicAPI.Modules.Clients.Services;
using AestheticClinicAPI.Modules.Clients.Repositories;
using AestheticClinicAPI.Modules.Clients.Models;
using AestheticClinicAPI.Shared;
using System.Linq.Expressions;

namespace AestheticClinicAPI.Tests.UnitTests.Clients;

public class ClientServiceTests
{
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly ClientService _clientService;

    public ClientServiceTests()
    {
        _clientRepoMock = new Mock<IClientRepository>();
        _clientService = new ClientService(_clientRepoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsClient()
    {
        // Arrange
        var client = new Client { Id = 1, FirstName = "Juan", LastName = "Dela Cruz", Email = "juan@example.com" };
        _clientRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(client);

        // Act
        var result = await _clientService.GetByIdAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Juan Dela Cruz", result.Data.FullName);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsFailure()
    {
        // Arrange
        _clientRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Client?)null);

        // Act
        var result = await _clientService.GetByIdAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Client not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsClient()
    {
        // Arrange
        var dto = new CreateClientDto
        {
            FirstName = "Maria",
            LastName = "Santos",
            Email = "maria@example.com"
        };
        var createdClient = new Client
        {
            Id = 2,
            FirstName = "Maria",
            LastName = "Santos",
            Email = "maria@example.com"
        };
        _clientRepoMock.Setup(r => r.AddAsync(It.IsAny<Client>())).ReturnsAsync(createdClient);
        _clientRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Client, bool>>>())).ReturnsAsync(false);

        // Act
        var result = await _clientService.CreateClientAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("maria@example.com", result.Data?.Email);
    }
}