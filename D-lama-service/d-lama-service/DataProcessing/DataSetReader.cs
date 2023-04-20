using d_lama_service.Repositories;
using Data.ProjectEntities;
using System.Text;

namespace d_lama_service.DataProcessing
{
    /// <summary>
    /// Reads data sets into the database.
    /// </summary>
    public class DataSetReader
    {
        private readonly string[] permittedExtensions = { ".csv", ".json", ".txt" };
        private readonly Encoding encoding = Encoding.UTF8;


        public DataSetReader()
        {

        }

        public bool IsValidFormat(IFormFile file)
        {
            string fileExt = GetFileExtension(file);
            if (string.IsNullOrEmpty(fileExt) || !permittedExtensions.Contains(fileExt))
            {
                return false;
            }
            return true;
        }

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
                            // TODO: maybe trim
                            dataPoints.Add(line);
                        }
                    }
                }
            }

            // read json
            if (GetFileExtension(file) == ".json")
            {
                while (!reader.EndOfStream)
                {
                    // TODO
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
