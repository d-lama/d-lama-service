
using System.IO;
using System.IO.Compression;

namespace d_lama_service.Models.DataProcessing
{
    public class ImageDataParser : DataParser
    {
        private readonly string[] _supportedCompressionExtensions = { ".zip" };
        private readonly string[] _supportedImgageFileExtensions = { ".jpg", ".jpeg", ".png" };

        public override bool IsValidFormat(IFormFile file, string[]? permittedExtensions = null)
        {
            return base.IsValidFormat(file, _supportedCompressionExtensions) || base.IsValidFormat(file, _supportedImgageFileExtensions);
        }

        public override async Task<ICollection<string>> ParseAsync(IFormFile file, int startIndex, string projectPath)
        {
            ICollection<string> dataPoints = new List<string>();

            // create a temporary directory to extract the images from the compressed file
            string tempDirPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDirPath);

            try
            {
                // Handle single image file
                if (IsSupportedImageFileExtension(file.FileName))
                {
                    string imageName = $"image_{startIndex}{Path.GetExtension(file.FileName)}";
                    string imagePath = Path.Combine(tempDirPath, imageName);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    string newPath = Path.Combine(projectPath, imageName);
                    File.Move(imagePath, newPath);
                    dataPoints.Add(newPath);
                }
                // Handle compressed file
                else if (IsSupportedCompressionFileExtension(file.FileName))
                {
                    // extract the images from the compressed file to the temporary directory
                    using (ZipArchive archive = new ZipArchive(file.OpenReadStream()))
                    {
                        var index = startIndex;
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            // check if the entry is an image
                            if (IsSupportedImageFileExtension(entry.Name))
                            {
                                // move the image file to the images directory and rename it with the index
                                string imageName = $"image_{index}{Path.GetExtension(entry.Name)}";
                                string imagePath = Path.Combine(tempDirPath, imageName);
                                // extract the image to the temporary directory in a separate task
                                await Task.Run(() => entry.ExtractToFile(imagePath));

                                string newPath = Path.Combine(projectPath, imageName);
                                File.Move(imagePath, newPath);
                                dataPoints.Add(newPath);
                                index++;
                            }
                        }
                    }
                }
            }
            catch
            {
                foreach (string addedFile in dataPoints)
                {
                    // delete the added files - add all or nothing
                    File.Delete(addedFile);
                }
                // delete the temporary directory if an exception occurs
                Directory.Delete(tempDirPath, true);
                throw;
            }

            // delete the temporary directory
            Directory.Delete(tempDirPath, true);

            return dataPoints;
        }

        private bool IsSupportedCompressionFileExtension(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            return _supportedCompressionExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }

        private bool IsSupportedImageFileExtension(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            return _supportedImgageFileExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }
    }
}

