using RapidPay.Core.Entities;
using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;
using RapidPay.Core.DTOs.Card;
using RapidPay.Core.Interfaces.Services.Utils;
using RapidPay.Core.Services;

#pragma warning disable CS8618 // Disable nullable warning in tests.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace RapidPay.Tests.Core.Services
{
    public class CardServiceTests
    {
        private CardService _cardService;
        private Mock<ICardRepository> _cardRepositoryMock;
        private Mock<ICardNumberUtils> _cardNumberUtilsMock;
        private Mock<ILogger<CardService>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _cardRepositoryMock = new Mock<ICardRepository>();
            _cardNumberUtilsMock = new Mock<ICardNumberUtils>();
            _loggerMock = new Mock<ILogger<CardService>>();
            _cardService = new CardService(_cardRepositoryMock.Object, _cardNumberUtilsMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task CreateCard_ValidRequest_ShouldReturnSuccessResult()
        {
            // Arrange
            var userSubjectId = "test-id";
            var initialBalance = 500;
            var cardNumber = "123456789123456";

            var createCardRequestDTO = new CreateCardRequestDTO { InitialBalance = initialBalance };
            
            _cardNumberUtilsMock.Setup(f => f.GenerateCardNumber()).Returns(cardNumber);

            // Act
            var result = await _cardService.CreateCard(createCardRequestDTO, userSubjectId);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<CreateCardResponseDTO>.ServiceActionResultStatus.Success));

            Assert.That(result.ActionResult.InitialBalance, Is.EqualTo(initialBalance));
            Assert.That(result.ActionResult.CardNumber, Is.EqualTo(cardNumber));

            _cardNumberUtilsMock.Verify(f => f.GenerateCardNumber(), Times.Once);
            _cardRepositoryMock.Verify(r => r.CreateCard(It.Is<Card>(c => c.CardNumber == cardNumber && c.UserId == userSubjectId && c.Balance == initialBalance)), Times.Once);
        }

        [Test]
        public void GetBalance_ValidRequest_ShouldReturnSuccessResult()
        {
            // Arrange
            var userSubjectId = "test-id";
            var cardNumber = "123456789123456";
            var balance = 500;
            var card = new Card { UserId = userSubjectId, CardNumber = cardNumber, Balance = balance };

            _cardRepositoryMock.Setup(r => r.CardExists(cardNumber)).Returns(true);
            _cardRepositoryMock.Setup(r => r.GetCard(cardNumber)).Returns(card);

            // Act
            var result = _cardService.GetBalance(cardNumber, userSubjectId);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Success));
            Assert.That(result.ActionResult.Balance, Is.EqualTo(balance));
            Assert.That(result.ActionResult.CardNumber, Is.EqualTo(cardNumber));

            _cardRepositoryMock.Verify(r => r.CardExists(cardNumber), Times.Once);
            _cardRepositoryMock.Verify(r => r.GetCard(cardNumber), Times.Exactly(2));
        }

        [Test]
        public void GetBalance_CardNumberContainsLetters_ShouldReturnInvalidCard()
        {
            // Arrange
            var userSubjectId = "test-id";
            var cardNumber = "a23456789123456";

            // Act
            var result = _cardService.GetBalance(cardNumber, userSubjectId);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Failure));
            Assert.That(result.ActionResultMessage, Is.EqualTo("Card number is not valid."));
        }

        [Test]
        public void GetBalance_CardNumberTooShort_ShouldReturnInvalidCard()
        {
            // Arrange
            var userSubjectId = "test-id";
            var cardNumber = "12345678912345";

            // Act
            var result = _cardService.GetBalance(cardNumber, userSubjectId);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Failure));
            Assert.That(result.ActionResultMessage, Is.EqualTo("Card number is not valid."));
        }

        [Test]
        public void GetBalance_CardNumberTooLong_ShouldReturnInvalidCard()
        {
            // Arrange
            var userSubjectId = "test-id";
            var cardNumber = "123456789123456789";

            // Act
            var result = _cardService.GetBalance(cardNumber, userSubjectId);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Failure));
            Assert.That(result.ActionResultMessage, Is.EqualTo("Card number is not valid."));
        }

        [Test]
        public void GetBalance_OnNonExistentCard_ShouldReturnSecureCardDoesNotExist()
        {
            // Arrange
            var userSubjectId = "test-id";
            var cardNumber = "123456789123456";
            var maskedCardNumer = "1234 **** **** 456";
            var balance = 500;
            var card = new Card { UserId = userSubjectId, CardNumber = cardNumber, Balance = balance };

            _cardRepositoryMock.Setup(r => r.CardExists(cardNumber)).Returns(false);

            // Act
            var result = _cardService.GetBalance(cardNumber, userSubjectId);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.SecureFailure));
            Assert.That(result.ActionResultMessage, Is.EqualTo($"Card {maskedCardNumer} does not exist."));

            _cardRepositoryMock.Verify(r => r.CardExists(cardNumber), Times.Once);
        }

        [Test]
        public void GetBalance_OnCardNotOwnedByUser_ShouldReturnSecureCardDoesNotBelongToUser()
        {
            // Arrange
            var userSubjectId = "test-id";
            var cardNumber = "123456789123456";
            var maskedCardNumer = "1234 **** **** 456";
            var balance = 500;
            var card = new Card { UserId = userSubjectId, CardNumber = cardNumber, Balance = balance };

            var otherUserSubjectId = "other-test-id";

            _cardRepositoryMock.Setup(r => r.CardExists(cardNumber)).Returns(true);
            _cardRepositoryMock.Setup(r => r.GetCard(cardNumber)).Returns(card);

            // Act
            var result = _cardService.GetBalance(cardNumber, otherUserSubjectId);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.SecureFailure));
            Assert.That(result.ActionResultMessage, Is.EqualTo($"Card {maskedCardNumer} does not belong to user."));

            _cardRepositoryMock.Verify(r => r.CardExists(cardNumber), Times.Once);
            _cardRepositoryMock.Verify(r => r.GetCard(cardNumber), Times.Once);
        }
    }
}