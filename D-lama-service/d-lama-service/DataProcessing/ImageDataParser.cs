
using System.IO;
using System.IO.Compression;

namespace d_lama_service.DataProcessing
{
    public class ImageDataParser : DataParser
    {
        private readonly string[] _permittedExtensions = { ".zip" };

        public override bool IsValidFormat(IFormFile file, string[]? permittedExtensions = null)
        {
            return base.IsValidFormat(file, _permittedExtensions);
        }

        public override async Task<ICollection<string>> ParseAsync(IFormFile file, int? index)
        {
            ICollection<string> dataPoints = new List<string>();

            // create a temporary directory to extract the images from the compressed file
            string tempDirPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDirPath);

            try
            {
                // extract the images from the compressed file to the temporary directory
                using (ZipArchive archive = new ZipArchive(file.OpenReadStream()))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        // check if the entry is an image
                        if (IsSupportedImageFile(entry))
                        {
                            string imagePath = Path.Combine(tempDirPath, entry.Name);
                            // extract the image to the temporary directory
                            entry.ExtractToFile(imagePath);

                            // move the image file to the images directory and rename it with the index
                            string imageName = $"{index}{Path.GetExtension(imagePath)}";
                            string imagesDirPath = Path.Combine("ImageDataPointsDirectory", index.ToString());
                            Directory.CreateDirectory(imagesDirPath);
                            string newPath = Path.Combine(imagesDirPath, imageName);
                            File.Move(imagePath, newPath);
                            dataPoints.Add(newPath);
                        }
                    }
                }
            }
            catch
            {
                // delete the temporary directory if an exception occurs
                Directory.Delete(tempDirPath, true);
                throw;
            }

            // delete the temporary directory
            Directory.Delete(tempDirPath, true);

            return dataPoints;
        }

        private bool IsSupportedImageFile(ZipArchiveEntry entry)
        {
            return entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                   entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase);
        }
    }
}

