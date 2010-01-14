using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Core;

namespace QA.Common.TCApi
{
    /// <summary>
    /// Runtime Services include functions important to all running tests, such as
    /// logging, configuration, paths, etc..
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class DefaultPaths
    {
        /// <summary>
        /// The root of the testing tree.  By default it uses the "entry" assembly's directory.
        /// If you update this most other default paths will automatically be updated.
        /// </summary>
        static public String Root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        /// <summary>
        /// The name of the default config file.  This is not a full path, but just the name of the file,
        /// by default "default.ini".
        /// </summary>
        static public String ConfigName = "default.ini";

        /// <summary>
        /// The log file destination, read only, uses the PathRoot and adds "logs" to it.
        /// </summary>
        static public String Log
        {
            get { return Path.Combine(Root, "logs"); }
        }

        /// <summary>
        /// The path to the configuration files, read only, it is the PathRoot + "conf".
        /// </summary>
        static public String ConfigDirectory
        {
            get { return Path.Combine(Root, "conf"); }
        }

        static public String TestPlansDirectory
        {
            get { return Path.Combine(Root, "plans"); }
        }

        /// <summary>
        /// The path to the default configuration file.  This is a programatic combination of PathConfig + DefaultConfigName.
        /// </summary>
        static public String ConfigFile
        {
            get { return Path.Combine(ConfigDirectory, ConfigName); }
        }

        /// <summary>
        /// The path to the tests (by default in assemblies).
        /// </summary>
        /// <seealso cref="DirectoryBasedAssemblyTestLoader"/>
        static public String TestsDirectory
        {
            get { return Path.Combine(Root, "tests"); }
        }

        /// <summary>
        /// The default log level.  Change (if needed) before calling configureLogging.
        /// </summary>
        static public Level DefaultLogLevel = Level.Info;

    }

    /// <summary>
    /// Small class of useful file related utilities.  There is no state with this class, or it's methods.
    /// </summary>
    public static class FileUtility
    {
        /// <summary>
        /// Back up a file (if it exists) by renaming it to [date]-origfilename.
        /// </summary>
        /// <param name="basePath">The directory containing the file.</param>
        /// <param name="filename">The file name itself.</param>
        static public void backupFile(String basePath, String filename)
        {
            String originalFileName = Path.Combine(basePath, filename);
            if (File.Exists(originalFileName))
            {
                DateTime created = File.GetLastWriteTime(originalFileName);
                String backupFileName = Path.Combine(basePath, created.ToString("MMM-dd-yyyy-HH-mm-ss-") + filename);
                File.Move(originalFileName, backupFileName);
            }
        }

        /// <summary>
        /// Back up a directory (if it exists) by renaming it to just a datetime stamp of the creation date.
        /// </summary>
        /// <param name="directory">The directory containing the file.</param>
        static public void backupDirectory(String directory)
        {
            if (Directory.Exists(directory))
            {
                DirectoryInfo target = new DirectoryInfo(directory);
                Directory.Move(target.FullName, Path.Combine(target.Parent.FullName, target.CreationTime.ToString("MMM-dd-yyyy-HH-mm-ss")));
            }   
        }

    }

    public class ExecutableBasePath : Attribute
    {
    }
}