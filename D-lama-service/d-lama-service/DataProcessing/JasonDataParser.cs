using System.Text.Json;

namespace d_lama_service.DataProcessing
{
    public class JsonDataParser : IDataParser
    {
        private readonly string[] permittedExtensions = { ".json" };

        public bool IsValidFormat(IFormFile file)
        {
            string fileExt = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExt) || !permittedExtensions.Contains(fileExt))
            {
                return false;
            }
            return true;
        }

        public async Task<ICollection<string>> ParseAsync(StreamReader reader)
        {
            ICollection<string> dataPoints = new List<string>();

            using (JsonDocument document = await JsonDocument.ParseAsync(reader.BaseStream))
            {
                JsonElement root = document.RootElement;
                foreach (JsonElement element in root.EnumerateArray())
                {
                    var line = element.GetString();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        dataPoints.Add(line.Trim());
                    }
                }
            }

            return dataPoints;
        }
    }
}
