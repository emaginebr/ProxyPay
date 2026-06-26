using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using ProxyPay.Domain.Models;
using ProxyPay.Domain.Services;
using ProxyPay.DTO.AbacatePay;
using ProxyPay.DTO.Invoice;
using ProxyPay.Infra.Interfaces.AppServices;
using ProxyPay.Infra.Interfaces.Repository;

namespace ProxyPay.Tests.Domain.Services;

// Feature: 002-store-abacatepay-apikey — US3 (FR-014: sem fallback para chave de ambiente)
public class InvoiceServiceNoFallbackTests
{
    [Fact]
    public async Task CheckQRCodeStatusAsync_StoreWithoutApiKey_ThrowsAndDoesNotCallProvider()
    {
        var invoice = new InvoiceModel { ExternalCode = "ext-123" };
        invoice.SetStore(1);
        invoice.MarkAsPending();

        var invoiceRepo = new Mock<IInvoiceRepository<InvoiceModel>>();
        invoiceRepo.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(invoice);

        var storeRepo = new Mock<IStoreRepository<StoreModel>>();
        storeRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new StoreModel { StoreId = 1, AbacatePayApiKey = null });

        var abacatePay = new Mock<IAbacatePayAppService>();

        var service = new InvoiceService(
            invoiceRepo.Object,
            new Mock<IInvoiceItemRepository<InvoiceItemModel>>().Object,
            storeRepo.Object,
            abacatePay.Object,
            new Mock<IValidator<InvoiceRequest>>().Object,
            new Mock<IValidator<QRCodeRequest>>().Object,
            new Mock<IMapper>().Object,
            new Mock<ILogger<InvoiceService>>().Object);

        await Assert.ThrowsAsync<Exception>(() => service.CheckQRCodeStatusAsync(100));

        abacatePay.Verify(
            a => a.CheckStatusAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }
}
