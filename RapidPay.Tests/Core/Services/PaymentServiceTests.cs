using RapidPay.Core.DTOs.Payment;
using RapidPay.Core.Entities;
using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;
using RapidPay.Core.Interfaces.Services;
using RapidPay.Core.Services;

#pragma warning disable CS8618 // Disable nullable warning in tests.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace RapidPay.Tests.Core.Services;

[TestFixture]
public class PaymentServiceTests
{
    private Mock<ICardRepository> _cardRepositoryMock;
    private Mock<IUniversalFeeExchangeService> _universalFeeExchangeServiceMock;
    private Mock<ILogger<PaymentService>> _loggerMock;
    private PaymentService _paymentService;

    [SetUp]
    public void Setup()
    {
        _cardRepositoryMock = new Mock<ICardRepository>();
        _universalFeeExchangeServiceMock = new Mock<IUniversalFeeExchangeService>();
        _loggerMock = new Mock<ILogger<PaymentService>>();
        _paymentService = new PaymentService(_cardRepositoryMock.Object, _universalFeeExchangeServiceMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task Pay_ValidPayment_ShouldReturnSuccessResult()
    {
        // Arrange
        var cardNumber = "123456789123456";
        decimal lastFee = 10;
        decimal newFee = 15;
        
        var paymentRequest = new PaymentRequestDTO { CardNumber = cardNumber, Amount = 100 };
        var card = new Card { CardNumber = cardNumber, Balance = 200, UserId = "user1", LastFee = lastFee };
        
        _cardRepositoryMock.Setup(x => x.CardExists(cardNumber)).Returns(true);
        _cardRepositoryMock.Setup(x => x.GetCard(cardNumber)).Returns(card);
        _universalFeeExchangeServiceMock.Setup(x => x.GetFee(lastFee)).Returns(newFee);

        // Act
        var result = await _paymentService.Pay(paymentRequest, "user1");

        // Assert
        Assert.That(result.Status, Is.EqualTo(ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.Success));
        Assert.That(result.ActionResult.Balance, Is.EqualTo(85));
        
        _cardRepositoryMock.Verify(x => x.CardExists(cardNumber), Times.Once);
        _cardRepositoryMock.Verify(x => x.GetCard(cardNumber), Times.Exactly(2));
        _cardRepositoryMock.Verify(x => x.UpdateCard(It.Is<Card>(c => c.CardNumber == cardNumber && c.Balance == 85 && c.LastFee == newFee)), Times.Once);
        
        _universalFeeExchangeServiceMock.Verify(x => x.GetFee(lastFee), Times.Exactly(2));
    }

    [Test]
    public async Task Pay_CardDoesNotExist_ShouldReturnSecureCardDoesNotExist()
    {
        // Arrange
        var paymentRequest = new PaymentRequestDTO { CardNumber = "123456789123456", Amount = 100 };
        _cardRepositoryMock.Setup(x => x.CardExists(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _paymentService.Pay(paymentRequest, "user1");

        // Assert
        Assert.That(result.Status, Is.EqualTo(ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.SecureFailure));
    }
    
    [Test]
    public async Task Pay_OnNonExistentCard_ShouldReturnSecureCardDoesNotExist()
    {
        // Arrange
        var cardNumber = "123456789123456";
        var paymentRequest = new PaymentRequestDTO { CardNumber = cardNumber, Amount = 100 };

        _cardRepositoryMock.Setup(r => r.CardExists(cardNumber)).Returns(false);

        // Act
        var result = await _paymentService.Pay(paymentRequest, "user1");

        // Assert
        Assert.That(result.Status, Is.EqualTo(ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.SecureFailure));
        Assert.That(result.ActionResultMessage, Is.EqualTo("Card does not exist."));

        _cardRepositoryMock.Verify(r => r.CardExists(cardNumber), Times.Once);
    }
    
    [Test]
    public async Task Pay_OnCardNotOwnedByUser_ShouldReturnSecureCardDoesNotBelongToUser()
    {
        // Arrange
        var cardNumber = "123456789123456";
        var amount = 500;
        var cardUser = "user";
        var requestUser = "other-user";
        
        var request = new PaymentRequestDTO() { CardNumber = cardNumber, Amount = amount};
        var card = new Card { CardNumber = cardNumber, Balance = 200, UserId = cardUser, LastFee = 10 };

        _cardRepositoryMock.Setup(r => r.CardExists(cardNumber)).Returns(true);
        _cardRepositoryMock.Setup(r => r.GetCard(cardNumber)).Returns(card);

        // Act
        var result = await _paymentService.Pay(request, requestUser);

        // Assert
        Assert.That(result.Status, Is.EqualTo(ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.SecureFailure));
        Assert.That(result.ActionResultMessage, Is.EqualTo("Card does not belong to user."));

        _cardRepositoryMock.Verify(r => r.CardExists(cardNumber), Times.Once);
        _cardRepositoryMock.Verify(r => r.GetCard(cardNumber), Times.Once);
    }

    [Test]
    public async Task Pay_InsufficientFunds_ShouldReturnFailureInsufficientFunds()
    {
        // Arrange
        var cardNumber = "123456789123456";
        var amount = 500;
        var cardUser = "user";
        
        var request = new PaymentRequestDTO() { CardNumber = cardNumber, Amount = amount};
        var card = new Card { CardNumber = cardNumber, Balance = 200, UserId = cardUser, LastFee = 10 };

        _cardRepositoryMock.Setup(r => r.CardExists(cardNumber)).Returns(true);
        _cardRepositoryMock.Setup(r => r.GetCard(cardNumber)).Returns(card);

        // Act
        var result = await _paymentService.Pay(request, cardUser);

        // Assert
        Assert.That(result.Status, Is.EqualTo(ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.Failure));
        Assert.That(result.ActionResultMessage, Is.EqualTo("Insufficient funds."));

        _cardRepositoryMock.Verify(r => r.CardExists(cardNumber), Times.Once);
        _cardRepositoryMock.Verify(r => r.GetCard(cardNumber), Times.Once);
    }
}