using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d_lama_service.DataProcessing;
using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Xml.Linq;

namespace d_lama_service_tests.DataProcessing
{
    [TestClass]
    public class DataSetReaderTest
    {
        private readonly Encoding encoding = Encoding.UTF8;

        [TestMethod]
        public void IsValidFormat_ValidExtension_ReturnsTrue()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            var file = fileMock.Object;
            var dataSetReader = new DataSetReader();

            // Act
            var result = dataSetReader.IsValidFormat(file);

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

            // Act
            var result = dataSetReader.IsValidFormat(file);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ReadFileAsync_TxtFile_ReturnsCorrectDataPoints()
        {
            // Arrange
            var expectedDataPoints = new List<string> { "hello", "world", "123" };
            var file = GetFormFile(expectedDataPoints, "test.txt");
            var dataSetReader = new DataSetReader();

            // Act
            var result = await dataSetReader.ReadFileAsync(file);

            // Assert
            Assert.AreEqual(expectedDataPoints.Count, result.Count);
            Assert.IsTrue(expectedDataPoints.All(dp => result.Contains(dp)));
        }

        [TestMethod]
        public async Task ReadFileAsync_CsvFile_ReturnsCorrectDataPoints()
        {
            // Arrange
            var expectedDataPoints = new List<string> { "hello", "world", "123" };
            var file = GetFormFile(expectedDataPoints, "test.csv");
            var dataSetReader = new DataSetReader();

            // Act
            var result = await dataSetReader.ReadFileAsync(file);

            // Assert
            Assert.AreEqual(expectedDataPoints.Count, result.Count);
            Assert.IsTrue(expectedDataPoints.All(dp => result.Contains(dp)));
        }

        [TestMethod]
        public async Task ReadFileAsync_JsonFile_ReturnsCorrectDataPoints()
        {
            // Arrange
            var expectedDataPoints = new List<string> { "hello", "world", "123" };
            var file = GetJsonFormFile(expectedDataPoints, "test.json");
            var dataSetReader = new DataSetReader();
            
            // Act
            var result = await dataSetReader.ReadFileAsync(file);

            // Assert
            Assert.AreEqual(expectedDataPoints.Count, result.Count);
            Assert.IsTrue(expectedDataPoints.All(dp => result.Contains(dp)));
        }

        [TestMethod]
        public async Task ReadFileAsync_EmptyTxtFile_ReturnsEmptyList()
        {
            // Arrange
            var expectedDataPoints = new List<string>();
            var file = GetFormFile(expectedDataPoints, "test.txt");
            var dataSetReader = new DataSetReader();

            // Act
            var result = await dataSetReader.ReadFileAsync(file);

            // Assert
            Assert.AreEqual(expectedDataPoints.Count, result.Count);
            Assert.IsTrue(expectedDataPoints.All(dp => result.Contains(dp)));
        }

        private IFormFile GetFormFile(List<string> dataPoints, string fileName)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            foreach (string line in dataPoints)
                writer.WriteLine(line);

            writer.Flush();
            stream.Position = 0;

            IFormFile file = new FormFile(stream, 0, stream.Length, Guid.NewGuid().ToString(), fileName);
            return file;
        }

        private IFormFile GetJsonFormFile(List<string> dataPoints, string fileName)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            string jsonString = JsonSerializer.Serialize(dataPoints);

            writer.Write(jsonString);
            writer.Flush();
            stream.Position = 0;

            IFormFile file = new FormFile(stream, 0, stream.Length, Guid.NewGuid().ToString(), fileName);
            return file;
        }
    }
}



