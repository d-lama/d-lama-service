namespace d_lama_service.Models.DataProcessing
{
    /// <summary>
    /// Parser for data point files.
    /// </summary>
    public abstract class DataParser
    {
        protected readonly Encoding _encoding = Encoding.UTF8;

        /// <summary>
        /// Checks id the given IFormFile is supported.
        /// </summary>
        /// <param name="file"> The file. </param>
        /// <returns> True if supported, else False. </returns>
        public virtual bool IsValidFormat(IFormFile file, string[]? permittedExtensions = null)
        {
            if (permittedExtensions == null) { return false; }
            string fileExt = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExt) || !permittedExtensions.Contains(fileExt))
            {
                return false;
            }
            return true;
        }

        public abstract Task<ICollection<string>> ParseAsync(IFormFile file, int index, string projectPath);
    }

}
