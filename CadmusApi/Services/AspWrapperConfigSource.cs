using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace CadmusApi.Services
{
    /// <summary>
    /// Plugins configuration source wrapping ASP.NET Core Configuration.
    /// </summary>
    /// <seealso cref="Fusi.Config.IConfigSource" />
    public sealed class AspWrapperConfigSource : Fusi.Config.IConfigSource
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Gets or sets the root section key for this source. The root section key is
        /// used as a prefix, which is inserted before any key read from a configuration 
        /// source. For instance, if you set this property to <c>mailing</c> and read 
        /// a key like <c>sender</c> from a data source, the resulting read key will be
        /// <c>mailing:sender</c>.
        /// </summary>
        /// <value>The root section key or null.</value>
        public string Section { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspWrapperConfigSource"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <exception cref="ArgumentNullException">null config</exception>
        public AspWrapperConfigSource(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Get all the key/value pairs from this source.
        /// </summary>
        /// <returns>options dictionary</returns>
        public Dictionary<string, string> Get()
        {
            return _config.AsEnumerable().ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <summary>
        /// Sets all the source values picking them from the specified options.
        /// </summary>
        /// <param name="options">The options, which can be a superset of the options 
        /// handled by this source. Each source will set only the options it handles.
        /// </param>
        public void Set(Dictionary<string, string> options)
        {
            foreach (var pair in options)
                _config[pair.Key] = pair.Value;
        }
    }
}
