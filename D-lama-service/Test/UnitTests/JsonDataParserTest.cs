using d_lama_service.Models.DataProcessing;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Test.UnitTests
{
    [TestClass]
    public class JsonDataParserTest
    {
        [TestMethod]
        public void IsValidFormat_ValidExtensionJson_ReturnsTrue()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.json");
            var file = fileMock.Object;
            var parser = new JsonDataParser();

            // Act
            var result = parser.IsValidFormat(file);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidFormat_InvalidExtension_ReturnsFalse()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.xyz");
            var file = fileMock.Object;
            var dataSetReader = new DataSetReader();
            var parser = new TextDataParser();

            // Act
            var result = parser.IsValidFormat(file);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
