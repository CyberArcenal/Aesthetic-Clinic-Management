using AestheticClinicAPI.Modules.Billing.DTOs;
using AestheticClinicAPI.Modules.Billing.Models;
using AestheticClinicAPI.Modules.Billing.Repositories;
using AestheticClinicAPI.Modules.Billing.Services;
using AestheticClinicAPI.Shared;
using Moq;
using Xunit;

namespace AestheticClinicAPI.Tests.UnitTests.Billing;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<IInvoiceService> _invoiceServiceMock;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _invoiceServiceMock = new Mock<IInvoiceService>();
        _paymentService = new PaymentService(_paymentRepoMock.Object, _invoiceServiceMock.Object);
    }

    private InvoiceResponseDto CreateSampleInvoiceDto() =>
        new InvoiceResponseDto
        {
            Id = 1,
            InvoiceNumber = "INV-001",
            Total = 1000,
            Status = "Draft",
        };

    [Fact]
    public async Task CreateAsync_ValidPayment_UpdatesInvoiceStatusToPartial()
    {
        // Arrange
        var dto = new CreatePaymentDto
        {
            InvoiceId = 1,
            Amount = 300,
            PaymentDate = DateTime.UtcNow,
            Method = "Cash",
        };
        var invoiceDto = CreateSampleInvoiceDto();
        _invoiceServiceMock
            .Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<InvoiceResponseDto>.Success(invoiceDto));
        _invoiceServiceMock
            .Setup(s => s.GetTotalPaidForInvoiceAsync(1))
            .ReturnsAsync(ServiceResult<decimal>.Success(300));
        _invoiceServiceMock
            .Setup(s => s.UpdateStatusAsync(1, "Partial"))
            .ReturnsAsync(ServiceResult<bool>.Success(true));

        Payment? capturedPayment = null;
        _paymentRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .Callback<Payment>(p => capturedPayment = p)
            .ReturnsAsync((Payment p) => p);
        _paymentRepoMock
            .Setup(r =>
                r.GetTotalPaymentsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())
            )
            .ReturnsAsync(0); // for safe measure

        // Act
        var result = await _paymentService.CreateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedPayment);
        Assert.Equal(300, capturedPayment.Amount);
        _invoiceServiceMock.Verify(s => s.UpdateStatusAsync(1, "Partial"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvoiceNotFound_ReturnsFailure()
    {
        // Arrange
        var dto = new CreatePaymentDto { InvoiceId = 99 };
        _invoiceServiceMock
            .Setup(s => s.GetByIdAsync(99))
            .ReturnsAsync(ServiceResult<InvoiceResponseDto>.Failure("Invoice not found."));

        // Act
        var result = await _paymentService.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invoice not found.", result.ErrorMessage);
        _paymentRepoMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RemovesPaymentAndRecalculatesInvoiceStatus()
    {
        // Arrange
        var payment = new Payment
        {
            Id = 1,
            InvoiceId = 1,
            Amount = 300,
        };
        var invoiceDto = CreateSampleInvoiceDto();
        _paymentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(payment);
        _paymentRepoMock.Setup(r => r.DeleteAsync(payment)).Returns(Task.CompletedTask);
        _invoiceServiceMock
            .Setup(s => s.GetTotalPaidForInvoiceAsync(1))
            .ReturnsAsync(ServiceResult<decimal>.Success(0));
        _invoiceServiceMock
            .Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<InvoiceResponseDto>.Success(invoiceDto));
        _invoiceServiceMock
            .Setup(s => s.UpdateStatusAsync(1, "Sent"))
            .ReturnsAsync(ServiceResult<bool>.Success(true));

        // Act
        var result = await _paymentService.DeleteAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        _paymentRepoMock.Verify(r => r.DeleteAsync(payment), Times.Once);
        _invoiceServiceMock.Verify(s => s.UpdateStatusAsync(1, "Sent"), Times.Once);
    }
}
