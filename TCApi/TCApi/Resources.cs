using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using Ninject.Core;
using log4net;

namespace QA.Common.TCApi
{
    /// <summary>
    /// A resource is content that the test needs to use.
    /// </summary>
    public interface IResource
    {
        /// <summary>
        /// A location is a string based identifier of where this resource is.  It should be unique to this resource.
        /// Two Resources that have the same location should be the same resource.  A location will be constructed by
        /// use of the <see cref="Resource.Test"/> and <see cref="Resource.Name"/> components.
        /// </summary>
        String Location { get; }

        /// <summary>
        /// The name is how the test case refers to a resource.  Each test case can have several resources, with unique names.
        /// </summary>
        String Name { get; }

        /// <summary>
        /// A Test is the basic identifier of a resource.  This is the test's Type (class).
        /// </summary>
        Type Test { get; }

        /// <summary>
        /// A resource may not exist, and still have an object associated to it, this allows you to create a resource within a test case.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Get all the content of this resource (assuming it exists) in an array of bytes.
        /// </summary>
        /// <returns>An array of bytes containing all the resource's content.</returns>
        byte[] getContent();

        /// <summary>
        /// Get all the content of this resource (assuming it exists) as a string.
        /// </summary>
        /// <returns>The contents of the resource as a large string.</returns>
        String getContentAsString();

        /// <summary>
        /// Open a stream to the resource.  If the mode is write, the resource can be created if it does not already exist.
        /// If it does exist it will truncate the resource.  If the mode is ReadWrite and the resource exists it will be
        /// opened without truncating it.
        /// </summary>
        /// <param name="mode">The mode the stream is operating in.  Read or Write (or Read and Write).</param>
        /// <returns>A stream to the resource.</returns>
        Stream getStream(FileAccess mode);
    }

    /// <summary>
    /// A factory for creating / loading resources.
    /// </summary>
    public interface IResourceFactory
    {
        /// <summary>
        /// Load a resource for the test provided, with the given name.  
        /// </summary>
        /// <param name="test">The test (type or class) for which this resource is for.</param>
        /// <param name="name">The name of the resource, must be unique within the test type.</param>
        /// <returns>A resource instance (which may not exist).</returns>
        IResource loadResource(Type test, String name);
    }

    public class FileResource : IResource
    {
        public static String LOCATION_PREFIX = "fileresource:";
        
        private String m_location;
        private Type m_test;
        private String m_name;

        public FileResource(Type p_test, String p_name, String p_base_location)
        {
            m_test = p_test;
            m_name = p_name;
            m_location = Path.Combine(Path.Combine(p_base_location, p_test.ToString()), p_name);
        }

        public Type Test
        {
            get { return m_test; }
        }

        public String Name
        {
            get { return m_name; }
        }

        public bool Exists
        {
            get { return File.Exists(m_location); }
        }

        public String Location
        {
            get { return LOCATION_PREFIX + m_location; }
        }

        public byte[] getContent()
        {
            return File.ReadAllBytes(m_location);
        }

        public Stream getStream(FileAccess mode)
        {
            if (mode == FileAccess.Read)
            {
                if (File.Exists(m_location))
                {
                    return new FileStream(m_location, FileMode.Open, mode);
                }
                else
                {
                    return null;
                }
            }
            else if (mode == FileAccess.Write)
            {
                if (!Directory.Exists(Path.GetDirectoryName(m_location)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(m_location));
                }
                if (File.Exists(m_location))
                {
                    return new FileStream(m_location, FileMode.Truncate, mode);
                }
                else
                {
                    return new FileStream(m_location, FileMode.OpenOrCreate, mode);
                }
            }
            else
            {
                if (!Directory.Exists(Path.GetDirectoryName(m_location)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(m_location));
                }
                return new FileStream(m_location, FileMode.OpenOrCreate, mode);
            }
        }

        public String getContentAsString()
        {
            return File.ReadAllText(m_location);
        }
    }

    public class FileResourceFactory : IResourceFactory
    {
        public static String DIRECTORY_NAME = "resources";

        private static ILog logger = LogManager.GetLogger(typeof(FileResourceFactory));

        private String m_resource_base;

        [Inject]
        public FileResourceFactory([ExecutableBasePath]String basePath, TCApiConfiguration configuration)
        {
            logger.DebugFormat("Constructor called with base path {0}", basePath);
            m_resource_base = Path.Combine(basePath, DIRECTORY_NAME);

            String env_name = "default";
            if (configuration.ContainsKey("Environment.Name"))
            {
                env_name = configuration["Environment.Name"];
            }
            m_resource_base = Path.Combine(m_resource_base, env_name);

            if(!Directory.Exists(m_resource_base))
            {
                logger.DebugFormat("Having to create resource base {0}.", m_resource_base);
                Directory.CreateDirectory(m_resource_base);
            }
        }

        public IResource loadResource(Type test, String name)
        {
            return new FileResource(test, name, m_resource_base);
        }
    }
}
