using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;
using RapidPay.Core.Services.Utils;

namespace RapidPay.Tests.Core.Services;

#pragma warning disable CS8618 // Disable nullable warning in tests.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

[TestFixture]
public class CardNumberFactoryTests
{
    private CardNumberUtils _cardNumberUtils;
    private Mock<ICardRepository> _cardRepositoryMock;
    
    [SetUp]
    public void Setup()
    {
        _cardRepositoryMock = new Mock<ICardRepository>();
        _cardNumberUtils = new CardNumberUtils(_cardRepositoryMock.Object);
    }
    
    [Test]
    public void GenerateCardNumber_ShouldReturnValidCardNumber()
    {
        // Arrange
        _cardRepositoryMock.Setup(c => c.CardExists(It.IsAny<string>())).Returns(false);
        
        // Act
        string cardNumber = _cardNumberUtils.GenerateCardNumber();
        
        // Assert
        Assert.That(cardNumber, Is.Not.Null.And.Length.EqualTo(15));
        Assert.That(cardNumber.All(char.IsDigit));

        Assert.Fail();
    }
    
    [Test]
    public void MaskCardNumber_ShouldMaskCardNumber()
    {
        // Arrange
        string cardNumber = "123456789012345";
        
        // Act
        string maskedCardNumber = CardNumberUtils.MaskCardNumber(cardNumber);
        
        // Assert
        Assert.That(maskedCardNumber, Is.EqualTo("1234 **** **** 345"));
    }
}
