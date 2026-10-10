using System;
using Newtonsoft.Json;

namespace ClipChopper.Common.Json
{
    public static class JsonHelper
    {
        private static readonly Lazy<JsonSerializerSettings> _defaultSerializerSettings =
            new(CreateDefaultSerializerSettings);

        public static JsonSerializerSettings DefaultSerializerSettings =>
            _defaultSerializerSettings.Value;


        public static JsonSerializerSettings CreateDefaultSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }
    }
}
