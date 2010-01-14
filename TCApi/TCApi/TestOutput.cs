using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ninject.Core;
using log4net;

namespace QA.Common.TCApi
{
    public interface ITestOutput
    {
        String OutputRoot { get; set; }
        String getTestCaseOutputPath(ITestCase tc);
        void setTestCaseOutputPath(ITestCase tc, String path);
        String getPath(String name);
        void setPath(String name, String value);
    }

    public class DefaultTestOutputManager : ITestOutput
    {
        public static String OutputSubDirectoryName = "results";

        private static ILog logger = LogManager.GetLogger(typeof(DefaultTestOutputManager));

        public virtual String OutputRoot
        {
            get;
            set;
        }

        private Dictionary<ITestCase, String> m_test_output_paths;

        private Dictionary<String, String> m_other_paths;

        [Inject]
        public DefaultTestOutputManager([ExecutableBasePath] String project_root)
        {
            logger.DebugFormat("Creating a new DefaultTestOutputManager with project root '{0}'.", project_root);
            OutputRoot = Path.Combine(project_root, OutputSubDirectoryName);
            logger.DebugFormat("Output root for all output will be '{0}'.", OutputRoot);
            m_test_output_paths = new Dictionary<ITestCase, string>();
            m_other_paths = new Dictionary<string, string>();
        }

        #region ITestOutput Members

        public string getTestCaseOutputPath(ITestCase tc)
        {
            String retval = null;
            logger.DebugFormat("Recieving request for test case output path for test with type '{0}'.", tc.GetType());
            if (m_test_output_paths.ContainsKey(tc))
            {
                retval = m_test_output_paths[tc];
            }
            else
            {
                logger.WarnFormat("No test output path found for test with type '{0}'.", tc.GetType());
            }
            logger.DebugFormat("Returning '{0}' for request for test case output path for test with type '{1}'.", retval, tc.GetType());
            return retval;
        }

        public void setTestCaseOutputPath(ITestCase tc, string path)
        {
            logger.DebugFormat("setTestCaseOutputPath(<ITestCase with type '{0}'>, '{1}') called.", tc.GetType(), path);
            if (path.StartsWith(OutputRoot))
            {
                logger.DebugFormat("No need to join the output root to the begining of path '{0}' as it's already in there.", path);
            }
            else
            {
                path = Path.Combine(OutputRoot, path);
                logger.DebugFormat("Prepended output path for test case with type '{0}' with default Output Root, it is now '{1}'.", tc.GetType(), path);
            }
            if(m_test_output_paths.ContainsKey(tc))
            {
                logger.WarnFormat("Output path for test with type '{0}' already exists as '{1}', it will be changed to '{2}'.", tc.GetType(), m_test_output_paths[tc], path);
            }
            m_test_output_paths[tc] = path;
        }

        public string getPath(string name)
        {
            String retval = null;
            logger.DebugFormat("Request for path made with getPath('{0}');", name);
            if (m_other_paths.ContainsKey(name))
            {
                retval = m_other_paths[name];
            }
            else
            {
                logger.WarnFormat("Output for path with name '{0}', requested, but not previously set.", name);
            }

            logger.DebugFormat("Returning '{0}' from getPath('{1}');", retval, name);
            return retval;
        }

        public void setPath(string name, string value)
        {
            logger.DebugFormat("setPath('{0}', '{1}') called.", name, value);
            if (m_other_paths.ContainsKey(name))
            {
                logger.WarnFormat("Previous value '{0}' for path with name '{1}' will be replaced with '{2}'.", m_other_paths[name], name, value); 
            }
            m_other_paths[name] = value;
        }

        #endregion
    }
}
