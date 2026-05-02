using Moq;
using Xunit;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Billing.Services;
using AestheticClinicAPI.Modules.Billing.Repositories;
using AestheticClinicAPI.Modules.Billing.Models;
using AestheticClinicAPI.Modules.Billing.DTOs;
using AestheticClinicAPI.Modules.Clients.Services;
using AestheticClinicAPI.Modules.Clients.Models;
using System.Linq.Expressions;

namespace AestheticClinicAPI.Tests.UnitTests.Billing;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
    private readonly Mock<IClientService> _clientServiceMock;
    private readonly InvoiceService _invoiceService;

    public InvoiceServiceTests()
    {
        _invoiceRepoMock = new Mock<IInvoiceRepository>();
        _clientServiceMock = new Mock<IClientService>();
        _invoiceService = new InvoiceService(_invoiceRepoMock.Object, _clientServiceMock.Object);
    }

    private ClientResponseDto CreateSampleClientDto() => new ClientResponseDto
    {
        Id = 1,
        FirstName = "Juan",
        LastName = "Dela Cruz",
    };

    [Fact]
    public async Task CreateAsync_ValidDto_GeneratesInvoiceNumberAndTotal()
    {
        // Arrange
        var dto = new CreateInvoiceDto
        {
            ClientId = 1,
            IssueDate = DateTime.UtcNow,
            Subtotal = 1000,
            Tax = 100
        };
        Invoice? capturedInvoice = null;
        _invoiceRepoMock.Setup(r => r.AddAsync(It.IsAny<Invoice>()))
            .Callback<Invoice>(inv => capturedInvoice = inv)
            .ReturnsAsync((Invoice inv) => inv);
        _invoiceRepoMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Invoice, bool>>>()))
            .ReturnsAsync(0);
        _invoiceRepoMock.Setup(r => r.GetTotalPaidByInvoiceAsync(It.IsAny<int>()))
            .ReturnsAsync(0);
        _clientServiceMock.Setup(c => c.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<ClientResponseDto>.Success(CreateSampleClientDto()));

        // Act
        var result = await _invoiceService.CreateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedInvoice);
        Assert.Equal(1100, capturedInvoice.Total);
        Assert.StartsWith("INV-", capturedInvoice.InvoiceNumber);
        Assert.Equal("Draft", capturedInvoice.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingInvoice_ReturnsDtoWithBalance()
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = 1,
            ClientId = 1,
            Total = 1000,
            InvoiceNumber = "INV-001",
            IssueDate = DateTime.UtcNow,
            Status = "Draft"
        };
        _invoiceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(invoice);
        _invoiceRepoMock.Setup(r => r.GetTotalPaidByInvoiceAsync(1)).ReturnsAsync(300);
        _clientServiceMock.Setup(c => c.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<ClientResponseDto>.Success(CreateSampleClientDto()));

        // Act
        var result = await _invoiceService.GetByIdAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(700, result.Data!.BalanceDue);
        Assert.Equal("Juan Dela Cruz", result.Data.ClientName);
    }

    [Fact]
    public async Task DeleteAsync_PaidInvoice_ReturnsFailure()
    {
        // Arrange
        var invoice = new Invoice { Id = 1, Status = "Paid" };
        _invoiceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(invoice);

        // Act
        var result = await _invoiceService.DeleteAsync(1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot delete a paid invoice.", result.ErrorMessage);
        _invoiceRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Invoice>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidStatus_UpdatesInvoice()
    {
        // Arrange
        var invoice = new Invoice { Id = 1, Status = "Draft" };
        _invoiceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(invoice);
        _invoiceRepoMock.Setup(r => r.UpdateAsync(invoice)).Returns(Task.CompletedTask);

        // Act
        var result = await _invoiceService.UpdateStatusAsync(1, "Sent");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Sent", invoice.Status);
    }
}