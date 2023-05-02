
namespace d_lama_service.DataProcessing
{
    public class TextDataParser : IDataParser
    {
        private readonly string[] permittedExtensions = { ".txt", ".csv" };

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

            using (reader)
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
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
