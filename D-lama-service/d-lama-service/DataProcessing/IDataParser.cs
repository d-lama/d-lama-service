namespace d_lama_service.DataProcessing
{
    /// <summary>
    /// Parser for data point files.
    /// </summary>
    public interface IDataParser
    {
        /// <summary>
        /// Checks id the given IFormFile is supported.
        /// </summary>
        /// <param name="file"> The file. </param>
        /// <returns> True if supported, else False. </returns>
        bool IsValidFormat(IFormFile file);

        /// <summary>
        /// Parses a supported IFormFile and saves the content as a List strings.
        /// </summary>
        /// <param name="reader"> The StreamReader instance of a file. </param>
        /// <returns> A List with the content of the file. </returns>
        Task<ICollection<string>> ParseAsync(StreamReader reader);
    }

}
