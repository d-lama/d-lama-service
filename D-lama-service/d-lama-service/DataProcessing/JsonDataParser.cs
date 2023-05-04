﻿using System.Text.Json;

namespace d_lama_service.DataProcessing
{
    public class JsonDataParser : DataParser
    {
        private readonly string[] _permittedExtensions = { ".json" };

        public override bool IsValidFormat(IFormFile file, string[]? permittedExtensions = null)
        {
            return base.IsValidFormat(file, _permittedExtensions);
        }

        public override async Task<ICollection<string>> ParseAsync(StreamReader reader)
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