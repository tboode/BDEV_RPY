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
    private const string CARD_NUMBER = "123456789012345";
    private const string SUBJECT_ID = "test-id";
    private const decimal INITIAL_BALANCE = 500;
    
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
        decimal lastFee = 10;
        decimal newFee = 15;
        decimal resultingBalance = 385;
        
        var paymentRequest = new PaymentRequestDTO { CardNumber = CARD_NUMBER, Amount = 100 };
        var card = new Card { CardNumber = CARD_NUMBER, Balance = INITIAL_BALANCE, UserId = SUBJECT_ID, LastFee = lastFee };
        
        _cardRepositoryMock.Setup(x => x.CardExists(CARD_NUMBER)).Returns(true);
        _cardRepositoryMock.Setup(x => x.GetCard(CARD_NUMBER)).Returns(card);
        _universalFeeExchangeServiceMock.Setup(x => x.GetFee(lastFee)).Returns(newFee);

        // Act
        var result = await _paymentService.Pay(paymentRequest, SUBJECT_ID);

        // Assert
        Assert.That(result.Status, Is.EqualTo(ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.Success));
        Assert.That(result.ActionResult.Balance, Is.EqualTo(resultingBalance));
        
        _cardRepositoryMock.Verify(x => x.CardExists(CARD_NUMBER), Times.Once);
        _cardRepositoryMock.Verify(x => x.GetCard(CARD_NUMBER), Times.Exactly(2));
        _cardRepositoryMock.Verify(x => x.UpdateCard(It.Is<Card>(c => c.CardNumber == CARD_NUMBER && c.Balance == resultingBalance && c.LastFee == newFee)), Times.Once);
        
        _universalFeeExchangeServiceMock.Verify(x => x.GetFee(lastFee), Times.Exactly(2));
    }
    
    [Test]
    public async Task Pay_OnNonExistentCard_ShouldReturnSecureCardDoesNotExist()
    {
        // Arrange
        var paymentRequest = new PaymentRequestDTO { CardNumber = CARD_NUMBER, Amount = 100 };

        _cardRepositoryMock.Setup(r => r.CardExists(CARD_NUMBER)).Returns(false);

        // Act
        var result = await _paymentService.Pay(paymentRequest, SUBJECT_ID);

        // Assert
        Assert.That(result.Status, Is.EqualTo(ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.SecureFailure));
        Assert.That(result.ActionResultMessage, Is.EqualTo("Card does not exist."));

        _cardRepositoryMock.Verify(r => r.CardExists(CARD_NUMBER), Times.Once);
    }
    
    [Test]
    public async Task Pay_OnCardNotOwnedByUser_ShouldReturnSecureCardDoesNotBelongToUser()
    {
        // Arrange
        var amount = 500;
        var otherSubject = "other-user";
        
        var request = new PaymentRequestDTO() { CardNumber = CARD_NUMBER, Amount = amount};
        var card = new Card { CardNumber = CARD_NUMBER, Balance = INITIAL_BALANCE, UserId = SUBJECT_ID, LastFee = 10 };

        _cardRepositoryMock.Setup(r => r.CardExists(CARD_NUMBER)).Returns(true);
        _cardRepositoryMock.Setup(r => r.GetCard(CARD_NUMBER)).Returns(card);

        // Act
        var result = await _paymentService.Pay(request, otherSubject);

        // Assert
        Assert.That(result.Status, Is.EqualTo(ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.SecureFailure));
        Assert.That(result.ActionResultMessage, Is.EqualTo("Card does not belong to user."));

        _cardRepositoryMock.Verify(r => r.CardExists(CARD_NUMBER), Times.Once);
        _cardRepositoryMock.Verify(r => r.GetCard(CARD_NUMBER), Times.Once);
    }

    [Test]
    public async Task Pay_InsufficientFunds_ShouldReturnFailureInsufficientFunds()
    {
        // Arrange
        var amount = 5000;
        
        var request = new PaymentRequestDTO() { CardNumber = CARD_NUMBER, Amount = amount};
        var card = new Card { CardNumber = CARD_NUMBER, Balance = INITIAL_BALANCE, UserId = SUBJECT_ID, LastFee = 10 };

        _cardRepositoryMock.Setup(r => r.CardExists(CARD_NUMBER)).Returns(true);
        _cardRepositoryMock.Setup(r => r.GetCard(CARD_NUMBER)).Returns(card);

        // Act
        var result = await _paymentService.Pay(request, SUBJECT_ID);

        // Assert
        Assert.That(result.Status, Is.EqualTo(ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.Failure));
        Assert.That(result.ActionResultMessage, Is.EqualTo("Insufficient funds."));

        _cardRepositoryMock.Verify(r => r.CardExists(CARD_NUMBER), Times.Once);
        _cardRepositoryMock.Verify(r => r.GetCard(CARD_NUMBER), Times.Once);
    }
}