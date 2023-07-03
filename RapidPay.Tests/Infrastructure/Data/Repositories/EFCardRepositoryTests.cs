using Microsoft.EntityFrameworkCore;
using RapidPay.Core.Entities;
using RapidPay.Infrastructure.Data;
using RapidPay.Infrastructure.Data.Repositories;

namespace RapidPay.Tests.Infrastructure.Data.Repositories;

#pragma warning disable CS8618 // Disable nullable warning in tests.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

[TestFixture]
public class EFCardRepositoryTests
{
    private const string CARD_NUMBER = "123456789012345";
    
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
        var card = new Card { CardNumber = CARD_NUMBER };
        
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
        var card = new Card { CardNumber = CARD_NUMBER };
        await _cardRepository.CreateCard(card);
        
        // Act
        var result = _cardRepository.GetCard(CARD_NUMBER);
        
        // Assert
        Assert.That(result, Is.EqualTo(card));
        
        _mockDbContext.Verify(db => db.Add(card), Times.Once);
        _mockDbContext.Verify(db => db.SaveChanges(), Times.Once);
    }

    [Test]
    public void GetCard_NonExistingCardNumber_ShouldReturnNull()
    {
        // Act
        var result = _cardRepository.GetCard(CARD_NUMBER);
        
        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task UpdateCard_ExistingCard_ShouldUpdateCardInCacheAndDatabase()
    {
        // Arrange
        var card = new Card { CardNumber = CARD_NUMBER, UserId = "user", Balance = 500};
        await _cardRepository.CreateCard(card);

        decimal newBalance = 400;
        card.Balance = newBalance;
        
        // Act
        await _cardRepository.UpdateCard(card);
        
        // Assert
        Assert.That(_cardRepository.GetCard(CARD_NUMBER)?.Balance, Is.EqualTo(newBalance));
        _mockDbContext.Verify(db => db.Update(card), Times.Once);
        _mockDbContext.Verify(db => db.SaveChanges(), Times.Exactly(2));
    }

    [Test]
    public void UpdateCard_NonExistingCard_ShouldThrowArgumentException()
    {
        // Arrange
        var card = new Card { CardNumber = CARD_NUMBER };
        
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await _cardRepository.UpdateCard(card));
    }

    [Test]
    public async Task CardExists_ExistingCardNumber_ShouldReturnTrue()
    {
        // Arrange
        var card = new Card { CardNumber = CARD_NUMBER };
        
        await _cardRepository.CreateCard(card);;
        
        // Act & Assert
        Assert.IsTrue(_cardRepository.CardExists(CARD_NUMBER));
    }

    [Test]
    public void CardExists_NonExistingCardNumber_ShouldReturnFalse()
    {
        // Act & Assert
        Assert.IsFalse(_cardRepository.CardExists(CARD_NUMBER));
    }
}