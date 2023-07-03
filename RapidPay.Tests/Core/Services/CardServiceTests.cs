using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using RapidPay.Core.Entities;
using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;
using RapidPay.Core.Interfaces.Services;
using RapidPay.Core.DTOs.Card;
using RapidPay.Core.Services;

#pragma warning disable CS8618 // Disable nullable warning in tests.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace RapidPay.Tests.Core.Services
{
    public class CardServiceTests
    {
        private CardService _cardService;
        private Mock<ICardRepository> _cardRepositoryMock;
        private Mock<ICardNumberFactory> _cardNumberFactoryMock;
        private Mock<ILogger<CardService>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _cardRepositoryMock = new Mock<ICardRepository>();
            _cardNumberFactoryMock = new Mock<ICardNumberFactory>();
            _loggerMock = new Mock<ILogger<CardService>>();
            _cardService = new CardService(_cardRepositoryMock.Object, _cardNumberFactoryMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task CreateCard_ValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var userSubjectId = "test-id";
            var initialBalance = 500;
            var cardNumber = "123456789123456";

            var createCardRequestDTO = new CreateCardRequestDTO { InitialBalance = initialBalance };
            _cardNumberFactoryMock.Setup(f => f.GenerateCardNumber()).Returns(cardNumber);

            // Act
            var result = await _cardService.CreateCard(createCardRequestDTO, userSubjectId);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<CreateCardResponseDTO>.ServiceActionResultStatus.Success));

            Assert.That(result.ActionResult.InitialBalance, Is.EqualTo(initialBalance));
            Assert.That(result.ActionResult.CardNumber, Is.EqualTo(cardNumber));

            _cardNumberFactoryMock.Verify(f => f.GenerateCardNumber(), Times.Once);
            _cardRepositoryMock.Verify(r => r.CreateCard(It.Is<Card>(c => c.CardNumber == cardNumber && c.UserId == userSubjectId && c.Balance == initialBalance)), Times.Once);
        }

        [Test]
        public void GetBalance_ValidRequest_ReturnsSuccessResult()
        {
            var userSubjectId = "test-id";
            var cardNumber = "123456789123456";
            var balance = 500;
            var card = new Card { UserId = userSubjectId, CardNumber = cardNumber, Balance = balance };

            _cardRepositoryMock.Setup(r => r.CardExists(cardNumber)).Returns(true);
            _cardRepositoryMock.Setup(r => r.GetCard(cardNumber)).Returns(card);

            var result = _cardService.GetBalance(cardNumber, userSubjectId);

            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Success));
            Assert.That(result.ActionResult.Balance, Is.EqualTo(balance));
            Assert.That(result.ActionResult.CardNumber, Is.EqualTo(cardNumber));

            _cardRepositoryMock.Verify(r => r.CardExists(cardNumber), Times.Once);
            _cardRepositoryMock.Verify(r => r.GetCard(cardNumber), Times.Exactly(2));
        }

        [Test]
        public void GetBalance_CardNumberContainsLetters_ReturnsInvalidCard()
        {
            var userSubjectId = "test-id";
            var cardNumber = "a23456789123456";
            var balance = 500;
            var card = new Card { UserId = userSubjectId, CardNumber = cardNumber, Balance = balance };

            var result = _cardService.GetBalance(cardNumber, userSubjectId);

            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Failure));
            Assert.That(result.ActionResultMessage, Is.EqualTo("Card number is not valid."));
        }

        [Test]
        public void GetBalance_CardNumberTooShort_ReturnsInvalidCard()
        {
            var userSubjectId = "test-id";
            var cardNumber = "12345678912345";
            var balance = 500;
            var card = new Card { UserId = userSubjectId, CardNumber = cardNumber, Balance = balance };

            var result = _cardService.GetBalance(cardNumber, userSubjectId);

            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Failure));
            Assert.That(result.ActionResultMessage, Is.EqualTo("Card number is not valid."));
        }

        [Test]
        public void GetBalance_CardNumberTooLong_ReturnsInvalidCard()
        {
            var userSubjectId = "test-id";
            var cardNumber = "123456789123456789";
            var balance = 500;
            var card = new Card { UserId = userSubjectId, CardNumber = cardNumber, Balance = balance };

            var result = _cardService.GetBalance(cardNumber, userSubjectId);

            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Failure));
            Assert.That(result.ActionResultMessage, Is.EqualTo("Card number is not valid."));
        }

        [Test]
        public void GetBalance_OnNonExistentCard_ReturnsSecureCardDoesNotExist()
        {
            var userSubjectId = "test-id";
            var cardNumber = "123456789123456";
            var balance = 500;
            var card = new Card { UserId = userSubjectId, CardNumber = cardNumber, Balance = balance };

            _cardRepositoryMock.Setup(r => r.CardExists(cardNumber)).Returns(false);

            var result = _cardService.GetBalance(cardNumber, userSubjectId);

            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.SecureFailure));
            Assert.That(result.ActionResultMessage, Is.EqualTo("Card does not exist."));

            _cardRepositoryMock.Verify(r => r.CardExists(cardNumber), Times.Once);
        }

        [Test]
        public void GetBalance_OnCardNotOwnedByUser_ReturnsSecureCardDoesNotBelongToUser()
        {
            var userSubjectId = "test-id";
            var cardNumber = "123456789123456";
            var balance = 500;
            var card = new Card { UserId = userSubjectId, CardNumber = cardNumber, Balance = balance };

            var otherUserSubjectId = "other-test-id";

            _cardRepositoryMock.Setup(r => r.CardExists(cardNumber)).Returns(true);
            _cardRepositoryMock.Setup(r => r.GetCard(cardNumber)).Returns(card);

            var result = _cardService.GetBalance(cardNumber, otherUserSubjectId);

            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.SecureFailure));
            Assert.That(result.ActionResultMessage, Is.EqualTo("Card does not belong to user."));

            _cardRepositoryMock.Verify(r => r.CardExists(cardNumber), Times.Once);
            _cardRepositoryMock.Verify(r => r.GetCard(cardNumber), Times.Once);
        }
    }
}

#pragma warning restore CS8618
#pragma warning restore CS8602