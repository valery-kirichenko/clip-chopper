using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Acolyte.Assertions;
using Acolyte.Linq;
using ClipChopper.Configuration;
using ClipChopper.Logging;
using ClipChopper.Models.IO;
using ClipChopper.Models.Wrappers.Tags;
using Newtonsoft.Json;
using NExifTool;

namespace ClipChopper.Core.Wrappers.Tags
{
    public sealed class NExifToolTagLoader : IExifTagLoader
    {
        /// <summary>
        /// Logger instance for current class.
        /// </summary>
        private static readonly ILogger _logger =
            LoggerFactory.CreateLoggerFor<NExifToolTagLoader>();


        public NExifToolTagLoader()
        {
        }

        #region ITagLoader Implementation

        public async Task<IReadOnlyList<ExifTag>> LoadTagsAsync(FilePath path)
        {
            var etOptions = new ExifToolOptions
            {
                ExifToolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigOptions.Core.ExifToolFileName)
            };
            var et = new ExifTool(etOptions);

            _logger.Info("Loading tags with ExifTool.");
            _logger.Info($"Tool path: [{etOptions.ExifToolPath}].");
            _logger.Info($"Media path: [{path.Value}].");

            try
            {
                var result = await et.GetTagsAsync(path.Value);

                var modelTags = result
                    .Select(tag => MapRawToModel(tag))
                    .ToReadOnlyList();

                _logger.Info($"{modelTags.Count} tags has been loaded.");
                return modelTags;
            }
            catch (JsonReaderException ex)
            {
                _logger.Error(ex, "Failed to read tags.");
                return Array.Empty<ExifTag>();
            }
        }

        #endregion

        private static ExifTag MapRawToModel(Tag tag)
        {
            tag.ThrowIfNull(nameof(tag));

            return new ExifTag(
                id: tag.Id,
                name: tag.Name,
                description: tag.Description,
                tableName: tag.TableName,
                group: tag.Group,
                value: tag.Value,
                numberValue: tag.NumberValue,
                list: tag.List
            );
        }
    }
}
