﻿using Moq;
using System.Text;
using d_lama_service.Models.DataProcessing;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using d_lama_service.Models;

namespace Test.UnitTests
{
    [TestClass]
    public class DataSetReaderTest
    {
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
        public async Task ReadFileAsync_TxtFileWithEmptyLines_ReturnsCorrectDataPoints()
        {
            // Arrange
            var inputDataPoints = new List<string> { "", "hello", "", "world", "" };
            var expectedDataPoints = new List<string> { "hello", "world" };
            var file = GetFormFile(expectedDataPoints, "test.txt");
            var dataSetReader = new DataSetReader();

            // Act
            var result = await dataSetReader.ReadFileAsync(file);

            // Assert
            Assert.AreEqual(expectedDataPoints.Count, result.Count);
            Assert.IsTrue(expectedDataPoints.All(dp => result.Contains(dp)));
        }

        [TestMethod]
        public async Task ReadFileAsync_CsvFileWithEmptyLines_ReturnsCorrectDataPoints()
        {
            // Arrange
            var inputDataPoints = new List<string> { "", "hello", "", "world", "" };
            var expectedDataPoints = new List<string> { "hello", "world" };
            var file = GetFormFile(expectedDataPoints, "test.csv");
            var dataSetReader = new DataSetReader();

            // Act
            var result = await dataSetReader.ReadFileAsync(file);

            // Assert
            Assert.AreEqual(expectedDataPoints.Count, result.Count);
            Assert.IsTrue(expectedDataPoints.All(dp => result.Contains(dp)));
        }

        [TestMethod]
        public async Task ReadFileAsync_JsonFileWithEmptyLines_ReturnsCorrectDataPoints()
        {
            // Arrange
            var inputDataPoints = new List<string> { "", "hello", "", "world", "" };
            var expectedDataPoints = new List<string> { "hello", "world" };
            var file = GetJsonFormFile(expectedDataPoints, "test.json");
            var dataSetReader = new DataSetReader();

            // Act
            var result = await dataSetReader.ReadFileAsync(file);

            // Assert
            Assert.AreEqual(expectedDataPoints.Count, result.Count);
            Assert.IsTrue(expectedDataPoints.All(dp => result.Contains(dp)));
        }

        [TestMethod]
        public async Task ReadFileAsync_EmptyTxtFile_ThrowsException()
        {
            // Arrange
            var expectedDataPoints = new List<string>();
            var file = GetFormFile(expectedDataPoints, "test.txt");
            var dataSetReader = new DataSetReader();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<RESTException>(() => dataSetReader.ReadFileAsync(file));
        }

        [TestMethod]
        public async Task ReadFileAsync_EmptyCsvFile_ThrowsException()
        {
            // Arrange
            var expectedDataPoints = new List<string>();
            var file = GetFormFile(expectedDataPoints, "test.csv");
            var dataSetReader = new DataSetReader();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<RESTException>(() => dataSetReader.ReadFileAsync(file));
        }

        [TestMethod]
        public async Task ReadFileAsync_EmptyJsonFile_ThrowsException()
        {
            // Arrange
            var expectedDataPoints = new List<string>();
            var file = GetJsonFormFile(expectedDataPoints, "test.json");
            var dataSetReader = new DataSetReader();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<RESTException>(() => dataSetReader.ReadFileAsync(file));
        }

        private IFormFile GetFormFile(List<string> dataPoints, string fileName)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            foreach (string line in dataPoints)
                writer.WriteLine(line.Trim());

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



