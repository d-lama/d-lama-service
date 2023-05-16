
using System.Transactions;

namespace d_lama_service.Models.DataProcessing
{
    public class TextDataParser : DataParser
    {
        private readonly string[] _supportedExtensions = { ".txt", ".csv" };

        public override bool IsValidFormat(IFormFile file, string[]? permittedExtensions = null)
        {
            return base.IsValidFormat(file, _supportedExtensions);
        }

        public override async Task<ICollection<string>> ParseAsync(IFormFile file, int index, string projectPath)
        {
            var reader = new StreamReader(file.OpenReadStream(), _encoding);
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
            reader.Close();
            return dataPoints;
        }
    }
}
