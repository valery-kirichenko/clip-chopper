using System;
using System.Runtime.InteropServices;
using ClipChopper.Common.Json;
using ClipChopper.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace ClipChopper.Configuration
{
    public static partial class ConfigOptions
    {
        private static readonly Lazy<IConfigurationRoot> LazyRoot = new(LoadOptions);
        private static IConfigurationRoot Root => LazyRoot.Value;

        public static string DefaultOptionsPath => PredefinedPaths.DefaultOptionsPath;

        public static string AlternativeOptionsPath => PredefinedPaths.AlternativeOptionsPath;

        #region Options

        public static ClipChopperCoreOptions Core => GetOptions<ClipChopperCoreOptions>();

        #endregion


        public static TOptions? FindOptions<TOptions>()
             where TOptions : class, IOptions, new()
        {
            IConfigurationSection section = GetConfigurationSection<TOptions>();
            return section.Get<TOptions>();
        }

        public static TOptions GetOptions<TOptions>()
            where TOptions : class, IOptions, new()
        {
            TOptions? options = FindOptions<TOptions>();

            // Sometimes options can be null because configuration data is reloading.
            if (options is null) return new TOptions();

            return options;
        }

        public static void SetOptions<TOptions>(TOptions? options)
        where TOptions : class, IOptions, new()
        {
            if (options is null) return;

            IConfigurationSection section = GetConfigurationSection<TOptions>();

            string output = JsonConvert.SerializeObject(
                options, JsonHelper.DefaultSerializerSettings
            );
            section.Value = output;
        }

        public static IChangeToken GetReloadToken()
        {
            return Root.GetReloadToken();
        }

        public static IChangeToken GetReloadToken<TOptions>()
            where TOptions : class, IOptions, new()
        {
            IConfigurationSection section = GetConfigurationSection<TOptions>();
            return section.GetReloadToken();
        }

        private static IConfigurationSection GetConfigurationSection<TOptions>()
            where TOptions : class, IOptions, new()
        {
            return Root.GetSection(typeof(TOptions).Name);
        }

        private static IConfigurationRoot LoadOptions()
        {
            var configurationBuilder = new ConfigurationBuilder();

            string configPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? DefaultOptionsPath
                : AlternativeOptionsPath;

            configurationBuilder.AddJsonFile(
                path: configPath,
                optional: true,
                reloadOnChange: true
            );

            return configurationBuilder.Build();
        }
    }
}
