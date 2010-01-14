using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using System.Text.RegularExpressions;

namespace QA.Common.TCApi
{
    public interface ITestPlan
    {
        List<TestInformation> loadTestPlan(TestCatalog catalog);
        String TestPlanName { get; }
    }

    public abstract class AbstractLocatorBasedTestPlan : ITestPlan
    {
        protected abstract List<String> Locators { get; }

        public abstract String TestPlanName { get; }

        #region TestPlanLoader Members

        public List<TestInformation> loadTestPlan(TestCatalog catalog)
        {
            List<TestInformation> retval = new List<TestInformation>();

            foreach (String positional_arg in Locators)
            {
                ITestLocator locator = TestLocatorFactory.getTestLocator(positional_arg);
                if (locator != null)
                {
                    retval.AddRange(locator.findTestsIn(catalog));
                }
                else
                {
                    //TODO: Change to logging statement
                    // shouldn't ever happen because it's assumed it's a group locator
                    // if it doesn't match something else, but just in case the library
                    // get's changed...
                    Console.WriteLine("Invalid test locator: {0}", positional_arg);
                }
            }

            return retval;
        }

        #endregion

    }

    public class ParameterTestPlan : AbstractLocatorBasedTestPlan
    {
        private List<String> m_locators;

        protected override List<string> Locators
        {
            get { return m_locators; }
        }

        public ParameterTestPlan(String[] parameters)
        {
            m_locators = new List<string>(parameters);
        }

        public override String TestPlanName
        {
            get { return "No Test Plan"; }
        }

    }

    public class SimpleFileTestPlan : AbstractLocatorBasedTestPlan
    {
        public static String PLAN_EXTENSION = ".txt";
        private static ILog logger = LogManager.GetLogger(typeof(SimpleFileTestPlan));
        private List<String> m_locators;
        private String m_test_plan_name;

        public override String TestPlanName
        {
            get { return m_test_plan_name; }
        }

        public SimpleFileTestPlan(String name)
            : this(name, DefaultPaths.TestPlansDirectory)
        {
        }

        public SimpleFileTestPlan(String name, String planDirectory)
        {
            m_test_plan_name = name;
            m_locators = new List<string>();
            logger.DebugFormat("Creating new SimpleFileTestPlan('{0}', '{1}').", name, planDirectory);
            String file_name = Path.Combine(planDirectory, name + PLAN_EXTENSION);
            logger.DebugFormat("Looking for a test plan at: {0}", file_name);
            if (!File.Exists(file_name))
            {
                logger.ErrorFormat("Testplan '{0}' not found at: {1}", name, file_name);
                throw new FileNotFoundException("Test Plan '" + name + "' not found at: " + file_name);
            }
            logger.DebugFormat("Loading contents of file '{0}'", file_name);
            String[] lines = File.ReadAllLines(file_name);
            logger.InfoFormat("Testplan '{0}' (loaded from filename '{1}') has {2} lines.", name, file_name, lines.Length);
            foreach (String line in lines)
            {
                String trimmed_line = line.Trim();
                if (trimmed_line.StartsWith("#"))
                {
                    logger.DebugFormat("Ignoring testplan comment: {0}", trimmed_line);
                }
                else
                {
                    logger.DebugFormat("Adding locator {0} to the list.", trimmed_line);
                    m_locators.Add(trimmed_line);
                }
            }
        }

        protected override List<string> Locators
        {
            get { return m_locators; }
        }
    }
}
