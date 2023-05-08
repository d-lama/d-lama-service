using d_lama_service.Models;
using System.Net;
using System.Text;

namespace d_lama_service.DataProcessing
{
    /// <summary>
    /// Reads textual files and returns data points in a list.
    /// </summary>
    public class DataSetReader
    {
        private readonly IDictionary<string, DataParser> _parsers = new Dictionary<string, DataParser>
        {
            { ".txt", new TextDataParser() },
            { ".csv", new TextDataParser() },
            { ".json", new JsonDataParser() },
            { ".zip", new ImageDataParser() }
        };

        public DataSetReader() { }

        /// <summary>
        /// Reads a supported IFormFile with the correct parser.
        /// </summary>
        /// <param name="file"> The file. </param>
        /// <returns> A List of data points. </returns>
        /// <exception cref="RESTException"> Throws Rest Excetption if file is not supported. </exception>
        public async Task<ICollection<string>> ReadFileAsync(IFormFile file, int? index = 0, string? projectPath = "")
        {
            if (file.Length <= 0 )
            {
                throw new RESTException(HttpStatusCode.BadRequest, $"The file {file.FileName} is empty.");
            }

            
            var fileExt = Path.GetExtension(file.FileName).ToLowerInvariant();

            DataParser? parser;
            if (_parsers.TryGetValue(fileExt, out parser) && parser.IsValidFormat(file))
            {
                var dataPoints = await parser.ParseAsync(file, index, projectPath);
                
                if (dataPoints == null || dataPoints.Count == 0)
                {
                    throw new RESTException(HttpStatusCode.BadRequest, $"The file {file.FileName} could not be read.");
                }
                return dataPoints;
            }
            else
            {
                throw new RESTException(HttpStatusCode.BadRequest, $"The file {file.FileName} is not supported.");
            }
        }
    }
}
