
using System.Transactions;

namespace d_lama_service.Models.DataProcessing
{
    public class TextDataParser : DataParser
    {
        private readonly string[] _permittedExtensions = { ".txt", ".csv" };

        public override bool IsValidFormat(IFormFile file, string[]? permittedExtensions = null)
        {
            return base.IsValidFormat(file, _permittedExtensions);
        }

        public override async Task<ICollection<string>> ParseAsync(StreamReader reader)
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
