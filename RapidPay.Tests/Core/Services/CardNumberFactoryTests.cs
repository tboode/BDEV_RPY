using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;
using RapidPay.Core.Services;

namespace RapidPay.Tests.Core.Services;

[TestFixture]
public class CardNumberFactoryTests
{
    private CardNumberFactory _cardNumberFactory;
    private Mock<ICardRepository> _cardRepositoryMock;
    
    [SetUp]
    public void Setup()
    {
        _cardRepositoryMock = new Mock<ICardRepository>();
        _cardNumberFactory = new CardNumberFactory(_cardRepositoryMock.Object);
    }
    
    [Test]
    public void GenerateCardNumber_ShouldReturnValidCardNumber()
    {
        // Arrange
        _cardRepositoryMock.Setup(c => c.CardExists(It.IsAny<string>())).Returns(false);
        
        // Act
        string cardNumber = _cardNumberFactory.GenerateCardNumber();
        
        // Assert
        Assert.That(cardNumber, Is.Not.Null.And.Length.EqualTo(15));
        Assert.That(cardNumber.All(char.IsDigit));
    }
    
    [Test]
    public void MaskCardNumber_ShouldMaskCardNumber()
    {
        // Arrange
        string cardNumber = "123456789012345";
        
        // Act
        string maskedCardNumber = CardNumberFactory.MaskCardNumber(cardNumber);
        
        // Assert
        Assert.That(maskedCardNumber, Is.EqualTo("1234 **** **** 345"));
    }
}