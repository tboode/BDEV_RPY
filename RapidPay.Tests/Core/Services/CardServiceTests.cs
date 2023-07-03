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
        private const string CARD_NUMBER = "123456789012345";
        private const string MASKED_CARD_NUMBER = "1234 **** **** 345";
        private const string SUBJECT_ID = "test-id";
        private const decimal INITIAL_BALANCE = 500;

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
            var createCardRequestDTO = new CreateCardRequestDTO { InitialBalance = INITIAL_BALANCE };
            
            _cardNumberUtilsMock.Setup(f => f.GenerateCardNumber()).Returns(CARD_NUMBER);

            // Act
            var result = await _cardService.CreateCard(createCardRequestDTO, SUBJECT_ID);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<CreateCardResponseDTO>.ServiceActionResultStatus.Success));

            Assert.That(result.ActionResult.InitialBalance, Is.EqualTo(INITIAL_BALANCE));
            Assert.That(result.ActionResult.CardNumber, Is.EqualTo(CARD_NUMBER));

            _cardNumberUtilsMock.Verify(f => f.GenerateCardNumber(), Times.Once);
            _cardRepositoryMock.Verify(r => r.CreateCard(It.Is<Card>(c => c.CardNumber == CARD_NUMBER && c.UserId == SUBJECT_ID && c.Balance == INITIAL_BALANCE)), Times.Once);
        }

        [Test]
        public void GetBalance_ValidRequest_ShouldReturnSuccessResult()
        {
            // Arrange
            var balance = 500;
            var card = new Card { UserId = SUBJECT_ID, CardNumber = CARD_NUMBER, Balance = balance };

            _cardRepositoryMock.Setup(r => r.CardExists(CARD_NUMBER)).Returns(true);
            _cardRepositoryMock.Setup(r => r.GetCard(CARD_NUMBER)).Returns(card);

            // Act
            var result = _cardService.GetBalance(CARD_NUMBER, SUBJECT_ID);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Success));
            Assert.That(result.ActionResult.Balance, Is.EqualTo(balance));
            Assert.That(result.ActionResult.CardNumber, Is.EqualTo(CARD_NUMBER));

            _cardRepositoryMock.Verify(r => r.CardExists(CARD_NUMBER), Times.Once);
            _cardRepositoryMock.Verify(r => r.GetCard(CARD_NUMBER), Times.Exactly(2));
        }

        [Test]
        public void GetBalance_CardNumberContainsLetters_ShouldReturnInvalidCard()
        {
            // Arrange
            var invalidCardNumber = "a23456789123456";

            // Act
            var result = _cardService.GetBalance(invalidCardNumber, SUBJECT_ID);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Failure));
            Assert.That(result.ActionResultMessage, Is.EqualTo("Card number is not valid."));
        }

        [Test]
        public void GetBalance_CardNumberTooShort_ShouldReturnInvalidCard()
        {
            // Arrange
            var invalidCardNumber = "12345678912345";

            // Act
            var result = _cardService.GetBalance(invalidCardNumber, SUBJECT_ID);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Failure));
            Assert.That(result.ActionResultMessage, Is.EqualTo("Card number is not valid."));
        }

        [Test]
        public void GetBalance_CardNumberTooLong_ShouldReturnInvalidCard()
        {
            // Arrange
            var invalidCardNumber = "123456789123456789";

            // Act
            var result = _cardService.GetBalance(invalidCardNumber, SUBJECT_ID);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Failure));
            Assert.That(result.ActionResultMessage, Is.EqualTo("Card number is not valid."));
        }

        [Test]
        public void GetBalance_OnNonExistentCard_ShouldReturnSecureCardDoesNotExist()
        {
            // Arrange
            _cardRepositoryMock.Setup(r => r.CardExists(CARD_NUMBER)).Returns(false);

            // Act
            var result = _cardService.GetBalance(CARD_NUMBER, SUBJECT_ID);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.SecureFailure));
            Assert.That(result.ActionResultMessage, Is.EqualTo($"Card {MASKED_CARD_NUMBER} does not exist."));

            _cardRepositoryMock.Verify(r => r.CardExists(CARD_NUMBER), Times.Once);
        }

        [Test]
        public void GetBalance_OnCardNotOwnedByUser_ShouldReturnSecureCardDoesNotBelongToUser()
        {
            // Arrange
            var card = new Card { UserId = SUBJECT_ID, CardNumber = CARD_NUMBER, Balance = INITIAL_BALANCE };

            var otherUserSubjectId = "other-test-id";

            _cardRepositoryMock.Setup(r => r.CardExists(CARD_NUMBER)).Returns(true);
            _cardRepositoryMock.Setup(r => r.GetCard(CARD_NUMBER)).Returns(card);

            // Act
            var result = _cardService.GetBalance(CARD_NUMBER, otherUserSubjectId);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.SecureFailure));
            Assert.That(result.ActionResultMessage, Is.EqualTo($"Card {MASKED_CARD_NUMBER} does not belong to user."));

            _cardRepositoryMock.Verify(r => r.CardExists(CARD_NUMBER), Times.Once);
            _cardRepositoryMock.Verify(r => r.GetCard(CARD_NUMBER), Times.Once);
        }
    }
}