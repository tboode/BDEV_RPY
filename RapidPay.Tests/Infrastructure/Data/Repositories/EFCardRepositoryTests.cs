using Microsoft.EntityFrameworkCore;
using RapidPay.Core.Entities;

namespace RapidPay.Infrastructure.Data.Repositories.Tests;

[TestFixture]
public class EFCardRepositoryTests
{
    private EFCardRepository _cardRepository;
    private Mock<RapidPayDbContext> _mockDbContext;

    [SetUp]
    public void SetUp()
    {
        _mockDbContext = new Mock<RapidPayDbContext>();
        
        var data = new List<Card>().AsQueryable();

        var mockSet = new Mock<DbSet<Card>>();
        mockSet.As<IQueryable<Card>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

        _mockDbContext.Setup(db => db.Cards).Returns(mockSet.Object);
        
        _cardRepository = new EFCardRepository(_mockDbContext.Object);
    }

    [Test]
    public async Task CreateCard_ShouldAddCardToCacheAndDatabase()
    {
        // Arrange
        var card = new Card { CardNumber = "123456789012345" };
        
        // Act
        await _cardRepository.CreateCard(card);
        
        // Assert
        Assert.IsTrue(_cardRepository.CardExists(card.CardNumber));
        
        _mockDbContext.Verify(db => db.Add(card), Times.Once);
        _mockDbContext.Verify(db => db.SaveChanges(), Times.Once);
    }

    [Test]
    public async Task GetCard_ExistingCardNumber_ShouldReturnCard()
    {
        // Arrange
        var cardNumber = "123456789012345";
        var card = new Card { CardNumber = cardNumber };
        await _cardRepository.CreateCard(card);
        
        // Act
        var result = _cardRepository.GetCard(cardNumber);
        
        // Assert
        Assert.That(result, Is.EqualTo(card));
        
        _mockDbContext.Verify(db => db.Add(card), Times.Once);
        _mockDbContext.Verify(db => db.SaveChanges(), Times.Once);
    }

    [Test]
    public void GetCard_NonExistingCardNumber_ShouldReturnNull()
    {
        // Arrange
        var cardNumber = "123456789012345";
        
        // Act
        var result = _cardRepository.GetCard(cardNumber);
        
        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task UpdateCard_ExistingCard_ShouldUpdateCardInCacheAndDatabase()
    {
        // Arrange
        var cardNumber = "123456789012345";
        var card = new Card { CardNumber = cardNumber, UserId = "user", Balance = 500};
        await _cardRepository.CreateCard(card);

        decimal newBalance = 400;
        card.Balance = newBalance;
        
        // Act
        await _cardRepository.UpdateCard(card);
        
        // Assert
        Assert.That(_cardRepository.GetCard(cardNumber)?.Balance, Is.EqualTo(newBalance));
        _mockDbContext.Verify(db => db.Update(card), Times.Once);
        _mockDbContext.Verify(db => db.SaveChanges(), Times.Exactly(2));
    }

    [Test]
    public void UpdateCard_NonExistingCard_ShouldThrowArgumentException()
    {
        // Arrange
        var card = new Card { CardNumber = "123456789012345" };
        
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await _cardRepository.UpdateCard(card));
    }

    [Test]
    public async Task CardExists_ExistingCardNumber_ShouldReturnTrue()
    {
        // Arrange
        var cardNumber = "123456789012345";
        var card = new Card { CardNumber = cardNumber };
        
        await _cardRepository.CreateCard(card);;
        
        // Act & Assert
        Assert.IsTrue(_cardRepository.CardExists(cardNumber));
    }

    [Test]
    public void CardExists_NonExistingCardNumber_ShouldReturnFalse()
    {
        // Arrange
        var cardNumber = "123456789012345";
        
        // Act & Assert
        Assert.IsFalse(_cardRepository.CardExists(cardNumber));
    }
}