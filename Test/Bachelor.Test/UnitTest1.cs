namespace Bachelor.Test;
using Xunit;


public class UnitTest1
{
    [Fact]
    public void Addition_WorksCorrectly()
    {
        // Arrange
        int a = 2;
        int b = 3;

        // Act
        int result = a + b;

        // Assert
        Assert.Equal(5, result);
    }
}