using System.Text;
using System.Text.Json;

namespace d_lama_service.DataProcessing
{
    /// <summary>
    /// Reads textual files and returns data points in a list.
    /// </summary>
    public class DataSetReader
    {
        private readonly string[] permittedExtensions = { ".csv", ".json", ".txt" };
        private readonly Encoding encoding = Encoding.UTF8;


        public DataSetReader() { }

        /// <summary>
        /// Checks id the given file is supported.
        /// </summary>
        /// <param name="file"> The project ID. </param>
        /// <returns> True if file extension is supported, else False. </returns>
        public bool IsValidFormat(IFormFile file)
        {
            string fileExt = GetFileExtension(file);
            if (string.IsNullOrEmpty(fileExt) || !permittedExtensions.Contains(fileExt))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks id the given file is supported.
        /// </summary>
        /// <param name="file"> The project ID. </param>
        /// <returns> A List of data point strings or an empty list. </returns>
        public async Task<ICollection<string>> ReadFileAsync(IFormFile file)
        {
            ICollection<string> dataPoints = new List<string>();
            var reader = new StreamReader(file.OpenReadStream(), encoding);

            // read txt; DataPoints separated by new line
            if (GetFileExtension(file) == ".txt" || GetFileExtension(file) == ".csv")
            {
                using (reader)
                {
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            line.Trim();
                            dataPoints.Add(line);
                        }
                    }
                }
            }

            // read json
            if (GetFileExtension(file) == ".json")
            {
                using (JsonDocument document = await JsonDocument.ParseAsync(reader.BaseStream))
                {
                    JsonElement root = document.RootElement;
                    foreach (JsonElement element in root.EnumerateArray())
                    {
                        var line = element.GetString();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            line.Trim();
                            dataPoints.Add(line);
                        }
                    }
                }
            }

            reader.Close();
            return dataPoints;
        }

        private string GetFileExtension(IFormFile file)
        {
            string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return fileExtension;
        }
    }
}
