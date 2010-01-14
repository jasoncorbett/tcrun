using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using IniParser;
using log4net;

namespace QA.Common.TCApi
{
    public class TCApiConfiguration : Dictionary<String, String>
    {
        public TCApiConfiguration()
            : base()
        {
        }

        public TCApiConfiguration(IDictionary<String, String> from)
            : base(from)
        {
        }
    }

    /// <summary>
    /// Configuration Loader is used to load configuration from a source into a form that can be consumed by the test framework.
    /// </summary>
    public interface IConfigLoader
    {
        /// <summary>
        /// Get the configuration in a key=value format.
        /// </summary>
        /// <param name="location">A string indicating where to get the configuration, can be implimentation specific.</param>
        /// <returns>A dictionary of key=value string items.</returns>
        TCApiConfiguration getFlattenedConfiguration(String location);
    }

    /// <summary>
    /// Default implimentation of <see cref="IConfigLoader"/>, loading from an ini file.
    /// </summary>
    public class INIConfigLoader : IConfigLoader
    {
        /// <summary>
        /// Load the INI file from the location specified, returning a "flattened" list of configuration items.
        /// </summary>
        /// <param name="location">The filesystem path to the ini file.</param>
        /// <returns>KEY=VALUE version of the ini file, where the section plus a period is prepended to the key name.</returns>
        public TCApiConfiguration getFlattenedConfiguration(String location)
        {
            TCApiConfiguration config_info = new TCApiConfiguration();
            ILog logger = LogManager.GetLogger(typeof(INIConfigLoader).ToString() + ".parseConfig");
            logger.DebugFormat("Attempting to load ini file {0}", location);
            if (!File.Exists(location))
            {
                logger.WarnFormat("Ini configuration file {0} does not exist, and cannot be loaded", location);
            }
            else
            {
                    FileIniDataParser parser = new IniParser.FileIniDataParser();
                    IniData parsed_data = parser.LoadFile(location);
                    logger.DebugFormat("Loaded ini file {0} with {1} sections in it.", location, parsed_data.Sections.Count);
                    foreach (SectionData section in parsed_data.Sections)
                    {
                        logger.DebugFormat("Loading data from Section {0}", section.SectionName);
                        foreach (KeyData key in section.Keys)
                        {
                            logger.DebugFormat("Loading key {0}.{1}", section.SectionName, key.KeyName);
                            config_info[section.SectionName + "." + key.KeyName] = key.Value;
                        }
                    }
            }
            logger.DebugFormat("Returning configuration information with {0} key(s).", config_info.Keys.Count);
            return config_info;
        }
    }
}
