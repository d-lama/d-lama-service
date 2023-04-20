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
        private IFormFile _file;
        private string _fileExt;

        public DataSetReader(IFormFile file)
        {
            this._file = file;
            this._fileExt = Path.GetExtension(file.FileName).ToLowerInvariant();
        }

        public bool IsValidFormat()
        {
            if (string.IsNullOrEmpty(_fileExt) || !permittedExtensions.Contains(_fileExt))
            {
                return false;
            }
            return true;
        }

        public async Task<bool> ReadFileAsync(Project project, IUnitOfWork unitOfWork)
        {
            var reader = new StreamReader(_file.OpenReadStream(), encoding);

            // read txt; DataPoints separated by new line
            if (_fileExt == ".txt")
            {
                using (reader)
                {
                    int dataPointIndex = 0;
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (line != null)
                        {
                            await AddDataPoint(project, unitOfWork, line, dataPointIndex);
                            dataPointIndex++;
                        }
                    }
                    return reader.EndOfStream;
                }
            }

            // read csv; DataPoints separated by commas
            if (_fileExt == ".csv")
            {
                using (reader)
                {
                    int dataPointIndex = 0;
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (line != null) {
                            var entries = line.Split(",");
                            foreach (var entry in entries)
                            {
                                await AddDataPoint(project, unitOfWork, line, dataPointIndex);
                                dataPointIndex++;
                            }
                        }
                    }
                    return reader.EndOfStream;
                }
            }

            // read json
            if (_fileExt == ".json")
            {
                while (!reader.EndOfStream)
                {
                    // TODO
                }
                return reader.EndOfStream;
            }

            reader.Close();
            return false;
        }

        private async Task AddDataPoint(Project project, IUnitOfWork unitOfWork, string content, int row)
        {
            var dataPoint = new TextDataPoint(content, row);
            unitOfWork.TextDataPointRepository.Update(dataPoint);
            project.TextDataPoints.Add(dataPoint);
            await unitOfWork.SaveAsync();
        }
    }
}
