using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;
using IniParser;
using Ninject.Core;

namespace QA.Common.TCApi
{
    /// <summary>
    /// Test case information, all in one place.
    /// </summary>
    public class TestInformation
    {
        private int m_test_number;
        private Guid m_test_guid;
        private String m_test_name;
        private List<String> m_test_groups;
        private Type m_test_class;

        private TestInformation()
        {
        }

        /// <summary>
        /// Only used by the static method getTestInfoFor(Type p_tc).
        /// </summary>
        /// <param name="p_tc">The type of the test case.</param>
        private TestInformation(Type p_tc)
        {
            m_test_number = TCNumber.getNumberFrom(p_tc);
            m_test_guid = TCGuid.getGuidFrom(p_tc);
            m_test_name = TCName.getNameFrom(p_tc);
            m_test_groups = TCGroup.getGroupsFrom(p_tc);
            m_test_class = p_tc;
        }

        /// <summary>
        /// Get the TestInformation for a test case.  Use this instead of a constructor.
        /// </summary>
        /// <param name="p_tc">The type of the "proposed" test case.</param>
        /// <returns>A populated TestInformation object if it is a test, null otherwise.</returns>
        static public TestInformation getTestInfoFor(Type p_tc)
        {
            TestInformation retval = null;

            if (TestInformation.isTestCase(p_tc))
            {
                retval = new TestInformation(p_tc);
            }

            return retval;
        }

        /// <summary>
        /// Check to see if a type is a test case.
        /// </summary>
        /// <remarks>
        /// Useful before calling getTestInfoFor(), but not necessary as getTestInfoFor will call this
        /// method and return null if it is not a test case.
        /// </remarks>
        /// <param name="p_tc">The type of the "proposed" test case.</param>
        /// <returns>
        /// True if the type:
        ///     <list type="number">
        ///         <item>is not null.</item>
        ///         <item>is assignable to <see cref="ITestCase"/>(either directly impliments or ancestor impliments).</item>
        ///         <item>is a class.</item>
        ///         <item>is not abstract.</item>
        ///     </list>
        /// </returns>
        static public bool isTestCase(Type p_tc)
        {
            return p_tc != null && 
                   typeof(ITestCase).IsAssignableFrom(p_tc) && 
                   p_tc.IsClass &&
                   p_tc.IsAbstract == false;
        }

        /// <summary>
        /// The name of the test case.  Defaults for TestName are the same as the return value of <see cref="TCName.getNameFrom"/>.
        /// </summary>
        public String TestName
        {
            get { return m_test_name; }
        }

        /// <summary>
        /// The number of the test case.  Defaults for TestNumber are the same as the return value of <see cref="TCNumber.getNumberFrom"/>.
        /// </summary>
        public int TestNumber
        {
            get { return m_test_number; }
        }

        /// <summary>
        /// The guid for the test case.  Defaults for TestGuid are the same as the return value of <see cref="TCGuid.getGuidFrom"/>.
        /// </summary>
        public Guid TestGuid
        {
            get { return m_test_guid; }
        }

        /// <summary>
        /// The list of groups the test case belongs to.  Defaults for TestGroups are the same as the return value of <see cref="TCGroup.getGroupsFrom"/>.
        /// </summary>
        public List<String> TestGroups
        {
            get { return m_test_groups; }
        }

        /// <summary>
        /// The class of the test case (or Type).
        /// </summary>
        public Type TestClass
        {
            get { return m_test_class; }
        }

        /// <summary>
        /// Get an instance of the test.  Depends on the test having a no argument constructor.
        /// </summary>
        /// <returns>An instance of ITestCase if everything is successful, null otherwise.</returns>
        /// <seealso cref="ITestCase"/>
        public ITestCase getInstance(IKernel p_injectionKernel)
        {
            ITestCase retval = null;
            ILog logger = LogManager.GetLogger(typeof(TestInformation).ToString() + ".getInstance");

            logger.DebugFormat("Getting instance of test {0}.", m_test_class);
            retval = p_injectionKernel.Get(m_test_class) as ITestCase;
            if (retval != null)
            {
                logger.DebugFormat("Successfully created an instance of {0}.", m_test_class);
            }
            else
            {
                logger.WarnFormat("Tried to create an instance of {0}, but instance returned was null.", m_test_class);
            }

            p_injectionKernel.Inject(retval);

            return retval;
        }

        /// <summary>
        /// Get a test case runner for the test class.
        /// </summary>
        /// <returns>An instance of a test runner for this test.</returns>
        public ITestRunner getRunnerFor()
        {
            ILog logger = LogManager.GetLogger(typeof(TestInformation).ToString() + ".getRunnerFor");
            logger.DebugFormat("Getting a test runner for class {0}.", m_test_class);
            ITestRunner retval = DefaultRunner.getRunnerFor(m_test_class);
            if (retval == null)
            {
                logger.WarnFormat("DefaultRunner.getRunnerFor({0}) returned null, creating an instance of DefaultITestCaseTestRunner.", m_test_class);
                retval = new DefaultITestCaseTestRunner();
            }

            logger.DebugFormat("Returning an instance of {0} for test class {1}.", retval.GetType(), m_test_class);

            return retval;
        }
    }

    /// <summary>
    /// Factory class for getting TestLocator's from various types of input.
    /// </summary>
    public static class TestLocatorFactory
    {
        /// <summary>
        /// Class name with name space, this hopefully should be compliant with the C# 3.0 specification for identifiers.
        /// </summary>
        public static Regex ClassnameLocatorType = new Regex("^[_\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lu}\\p{Lm}\\p{Lo}\\p{Nl}][\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lu}\\p{Lm}\\p{Lo}\\p{Nl}\\p{Nd}\\p{Mn}\\p{Mc}\\p{Pc}\\p{Cf}]*(?:\\.[_a-zA-Z][a-zA-Z0-9\\p{Mn}\\p{Mc}\\p{Pc}\\p{Cf}]*)+$");

        /// <summary>
        /// A Regex used to detect a Guid.
        /// </summary>
        public static Regex GuidLocatorType = new Regex("^[0-9a-f]{8}\\-[0-9a-f]{4}\\-[0-9a-f]{4}\\-[0-9a-f]{4}\\-[0-9a-f]{12}$", RegexOptions.IgnoreCase);

        /// <summary>
        /// A Regex used to detect a Number (positive integer).
        /// </summary>
        public static Regex NumberLocatorType = new Regex("^[0-9]+$");

        /// <summary>
        /// Get a test locator based on a String.  An attempt will be made to
        /// determine the correct type of TestLocator.
        /// </summary>
        /// <remarks>
        /// To determine which type of test locator is returned, the input string
        /// is matched against some regular expressions.  Once a valid match is
        /// found, the specific type of locator is instanciated and returned.
        /// The types are:
        ///     <list type="number">
        ///         <item><see cref="GuidTestLocator"/>:<see cref="TestLocatorFactory.GuidLocatorType"/> regular expression.</item>
        ///         <item><see cref="NumberTestLocator"/>: <see cref="TestLocatorFactory.NumberLocatorType"/> regular expression.</item>
        ///         <item><see cref="GroupTestLocator"/>Since it can be any string this is only assumed if the input does not match the previous types.</item>
        ///     </list>
        /// </remarks>
        /// <param name="p_input">The input to match against.</param>
        /// <returns>A test locator.</returns>
        public static ITestLocator getTestLocator(String p_input)
        {
            ITestLocator retval = null;
            ILog logger = LogManager.GetLogger(typeof(TestLocatorFactory) + ".getTestLocator");
            logger.DebugFormat("Trying to determine which test locator type should be used for \"{0}\".", p_input);

            if (GuidLocatorType.IsMatch(p_input))
            {
                logger.DebugFormat("Using a GuidTestLocator based on Guid {0}.", p_input);
                retval = new GuidTestLocator(new Guid(p_input));
            }
            else if (NumberLocatorType.IsMatch(p_input))
            {
                logger.DebugFormat("Using a NumberTestLocator for test number {0}.", p_input);
                retval = new NumberTestLocator(int.Parse(p_input));
            }
            else if (ClassnameLocatorType.IsMatch(p_input))
            {
                logger.DebugFormat("Using a ClassNameLocator for test '{0}'.", p_input);
                retval = new ClassNameTestLocator(p_input);
            }
            else
            {
                logger.DebugFormat("Usgin a GroupTestLocator for input \"{0}\" because it doesn't appear to match any other locator type.", p_input);
                retval = new GroupTestLocator(p_input);
            }

            return retval;
        }
    }

    /// <summary>
    /// Common interface for all test locators (which their only job is to find test(s) in a
    /// <see cref="TestCatalog"/>.
    /// </summary>
    /// <seealso cref="TestLocatorFactory"/>
    public interface ITestLocator
    {
        /// <summary>
        /// Find all tests that match this locator in the catalog.
        /// </summary>
        /// <param name="p_catalog">A catalog of tests.</param>
        /// <returns>A list of TestInformation (everything we should need) objects that match this locator.</returns>
        List<TestInformation> findTestsIn(TestCatalog p_catalog);
    }

    /// <summary>
    /// Locate a test based on a <see cref="TCGuid"/> attribute.
    /// </summary>
    public class GuidTestLocator : ITestLocator
    {
        private Guid m_guid;
        private ILog logger;

        /// <summary>
        /// Create a new GuidTestLocator that will look for a test with the Guid passed in.
        /// </summary>
        /// <param name="p_guid">The guid to match against the <see cref="TCGuid"/> attribute.</param>
        public GuidTestLocator(Guid p_guid)
        {
            logger = LogManager.GetLogger(typeof(GuidTestLocator));
            logger.DebugFormat("Creating a new GuidTestLocator with guid {0}.", p_guid);
            m_guid = p_guid;
        }

        /// <summary>
        /// Find a single test with the guid matching the one passed into the constructor.
        /// </summary>
        /// <param name="p_catalog">The catalog to look in.</param>
        /// <returns>At most 1 TestInformation in the list, and if it's not found 0 TestInformations.</returns>
        public List<TestInformation> findTestsIn(TestCatalog p_catalog)
        {
            List<TestInformation> retval = new List<TestInformation>();

            logger.DebugFormat("Looking in catalog with {0} tests and {1} tests indexed by guid for a test with guid {2}.", p_catalog.AllTests.Count, p_catalog.TCGuidMapping.Count, m_guid);
            if (p_catalog.TCGuidMapping.ContainsKey(m_guid))
            {
                logger.DebugFormat("Found a test with guid {0}.", m_guid);
                retval.Add(p_catalog.TCGuidMapping[m_guid]);
            }
            else
            {
                logger.DebugFormat("Test case with guid {0} not found in catalog.", m_guid);
            }

            logger.DebugFormat("Returning a list of {0} TestInformation object(s) from findTestsIn(catalog).", retval.Count);
            return retval;
        }
    }

    /// <summary>
    /// Locate a test based on a <see cref="TCNumber"/> attribute.
    /// </summary>
    public class NumberTestLocator : ITestLocator
    {
        private int m_number;
        private ILog logger;

        /// <summary>
        /// Initialize a new NumberTestLocator looking for a test with a matching <see cref="TCNumber"/> attribute.
        /// </summary>
        /// <param name="p_number">The number to look for.</param>
        public NumberTestLocator(int p_number)
        {
            logger = LogManager.GetLogger(typeof(NumberTestLocator));
            logger.DebugFormat("Creating a new NumberTestLocator with number {0}.", p_number);
            m_number = p_number;
        }

        /// <summary>
        /// Find the test that matches the number.  Because of the way <see cref="TestCatalog"/>
        /// indexes (specifically <see cref="TestCatalog.TCNumberMapping"/>) you will find at
        /// most one test from this locator.
        /// </summary>
        /// <param name="p_catalog">The catalog of tests to search in.</param>
        /// <returns>A list of at most 1 test case matching the number we're looking for.</returns>
        public List<TestInformation> findTestsIn(TestCatalog p_catalog)
        {
            List<TestInformation> retval = new List<TestInformation>();

            logger.DebugFormat("Looking in catalog with {0} tests and {1} tests indexed by number for a test with number {2}.", p_catalog.AllTests.Count, p_catalog.TCNumberMapping.Count, m_number);
            if (p_catalog.TCNumberMapping.ContainsKey(m_number))
            {
                logger.DebugFormat("Found a test with number {0}.", m_number);
                retval.Add(p_catalog.TCNumberMapping[m_number]);
            }
            else
            {
                logger.DebugFormat("Test case with number {0} not found in catalog.", m_number);
            }

            logger.DebugFormat("Returning a list of {0} TestInformation object(s) from findTestsIn(catalog).", retval.Count);

            return retval;
        }
    }

    /// <summary>
    /// Locate a group of tests by the group name.
    /// </summary>
    public class GroupTestLocator : ITestLocator
    {
        private String m_group;
        private ILog logger;

        /// <summary>
        /// Create a new locator that will search for all tests with the group name provided.
        /// </summary>
        /// <param name="p_group_name">The name of the group (Case sensitive).</param>
        public GroupTestLocator(String p_group_name)
        {
            logger = LogManager.GetLogger(typeof(GroupTestLocator));
            logger.DebugFormat("Creating a new GroupTestLocator for group {0}.", p_group_name);
            m_group = p_group_name;
        }

        /// <summary>
        /// Locate all tests that have the same group name as the one provided.
        /// </summary>
        /// <param name="p_catalog">The catalog to search in.</param>
        /// <returns>A list of tests with the group name, if no tests are found the list is empty.</returns>
        public List<TestInformation> findTestsIn(TestCatalog p_catalog)
        {
            List<TestInformation> retval = new List<TestInformation>();

            logger.DebugFormat("findTestsIn(catalog) called for group {0}, in a catalog with {1} tests, and {2} groups indexed.", m_group, p_catalog.AllTests.Count, p_catalog.TCGroupMapping.Count);

            if (p_catalog.TCGroupMapping.ContainsKey(m_group))
            {
                logger.DebugFormat("Group {0} found in test catalog, adding all {1} tests.", m_group, p_catalog.TCGroupMapping[m_group].Count);
                retval.AddRange(p_catalog.TCGroupMapping[m_group]);
            }
            else
            {
                logger.DebugFormat("Group {0} not found in catalog.", m_group);
            }

            logger.DebugFormat("Returning a list of {0} TestInformation object(s) from findTestsIn(catalog).", retval.Count);

            return retval;
        }
    }

    public class ClassNameTestLocator : ITestLocator
    {
        private ILog logger = LogManager.GetLogger(typeof(ClassNameTestLocator));

        private String m_class_name;

        public ClassNameTestLocator(String name)
        {
            logger.DebugFormat("Creating ClassNameTestLocator('{0}').", name);
            m_class_name = name;
        }

        #region ITestLocator Members

        public List<TestInformation> findTestsIn(TestCatalog p_catalog)
        {
            List<TestInformation> retval = new List<TestInformation>();
            if (p_catalog.TCClassNameMapping.ContainsKey(m_class_name))
            {
                logger.DebugFormat("Found class by name of '{0}' in catalog.", m_class_name);
                retval.Add(p_catalog.TCClassNameMapping[m_class_name]);
            }
            else
            {
                logger.WarnFormat("Class name '{0}' not found in list of tests.", m_class_name);
            }

            logger.DebugFormat("Returning a list of {0} TestInformation object(s) from findTestsIn(catalog).", retval.Count);
            return retval;
        }

        #endregion
    }

    /// <summary>
    /// A grouping of all test cases found by a test case loader.
    /// </summary>
    public class TestCatalog
    {
        private Dictionary<int, TestInformation> m_tcnumber_mapping;
        private Dictionary<String, List<TestInformation>> m_group_mapping;
        private List<TestInformation> m_all_tests;
        private Dictionary<Guid, TestInformation> m_guid_mapping;
        private Dictionary<String, TestInformation> m_class_name_mapping;

        private ILog logger;

        /// <summary>
        /// The mapping of all the tests that contain a test number attribute
        /// </summary>
        /// <seealso cref="TCNumber"/>
        public Dictionary<int, TestInformation> TCNumberMapping
        {
            get { return m_tcnumber_mapping; }
        }

        /// <summary>
        /// The mapping of all the tests that contain at least 1 group attribute.
        /// </summary>
        /// <seealso cref="TCGroup"/>
        public Dictionary<String, List<TestInformation>> TCGroupMapping
        {
            get { return m_group_mapping; }
        }

        /// <summary>
        /// All tests located including their TestInformation.
        /// </summary>
        /// <seealso cref="TestInformation"/>
        public List<TestInformation> AllTests
        {
            get { return m_all_tests; }
        }

        /// <summary>
        /// All tests that have a Guid assigned to them.
        /// </summary>
        /// <seealso cref="TCGuid"/>
        public Dictionary<Guid, TestInformation> TCGuidMapping
        {
            get { return m_guid_mapping; }
        }

        /// <summary>
        /// Class Name mapping to test information.
        /// </summary>
        public Dictionary<String, TestInformation> TCClassNameMapping
        {
            get { return m_class_name_mapping; }
        }

        /// <summary>
        /// Create a new (and empty) catalog of tests.
        /// </summary>
        public TestCatalog()
        {
            m_tcnumber_mapping = new Dictionary<int, TestInformation>();
            m_group_mapping = new Dictionary<string, List<TestInformation>>();
            m_all_tests = new List<TestInformation>();
            m_guid_mapping = new Dictionary<Guid, TestInformation>();
            m_class_name_mapping = new Dictionary<string, TestInformation>();
            logger = LogManager.GetLogger(typeof(TestCatalog));
            logger.Debug("New instance of TestCatalog initialized.");
        }

        /// <summary>
        /// Add a type as a test case.  By default this method will ignore the type if it is not a test.
        /// </summary>
        /// <param name="p_tc">The class (type) that is a test case.</param>
        /// <seealso cref="ITestCase" />
        public void addTestCase(Type p_tc)
        {
            logger.DebugFormat("addTestCase({0}) called.", p_tc);

            TestInformation test_info = TestInformation.getTestInfoFor(p_tc);
            if (test_info != null)
            {
                if (test_info.TestNumber != -1)
                {
                    logger.DebugFormat("Adding Test Case {0} to TCNumberMapping[{1}].", p_tc, test_info.TestNumber);
                    m_tcnumber_mapping.Add(test_info.TestNumber, test_info);
                }
                else
                {
                    logger.DebugFormat("Test Case {0} has no test case number.", p_tc);
                }

                foreach (String group in test_info.TestGroups)
                {
                    logger.DebugFormat("Adding Test Case {0} to TCGroupMapping[{1}].", p_tc, group);
                    if (!m_group_mapping.ContainsKey(group))
                    {
                        logger.DebugFormat("Creating new group {0} in TCGroupMapping.", group);
                        m_group_mapping[group] = new List<TestInformation>();
                    }
                    m_group_mapping[group].Add(test_info);
                }

                if (test_info.TestGuid != Guid.Empty)
                {
                    logger.DebugFormat("Adding test case {0} to TCGuidMapping[{1}].", p_tc, test_info.TestGuid);
                    m_guid_mapping[test_info.TestGuid] = test_info;
                }
                else
                {
                    logger.DebugFormat("Test Case {0} had no Guid assigned to it.", p_tc);
                }

                logger.DebugFormat("Adding test case {0} to AllTests.", p_tc);
                m_all_tests.Add(test_info);
                m_class_name_mapping.Add(test_info.TestClass.ToString(), test_info);
            }
            else
            {
                logger.DebugFormat("Unable to add class {0} as a test case because at least one of the following is true: it is a null type, it is not a class that impliments ITestCase, or it is not a class.", p_tc);
            }
        }
    }

    /// <summary>
    /// Interface for all TestLoaders, their only purpose being to load a catalog of tests that can be run.
    /// </summary>
    public interface TestLoader
    {
        /// <summary>
        /// Load all tests into a test catalog.
        /// </summary>
        /// <returns></returns>
        TestCatalog loadAllTests();
    }

    /// <summary>
    /// Loading all the tests that are in assemblies in a particular folder.
    /// </summary>
    public class DirectoryBasedAssemblyTestLoader : TestLoader
    {
        /// <summary>
        /// Only look for assemblies in a directory that match this file pattern.
        /// </summary>
        static public String AssemblyFilter = "*.dll";

        private String m_path_to_tests;
        private ILog logger;

        /// <summary>
        /// Create a Test Loader that will examine the default directory (root executable dir + "lib") for assemblies that contain tests.  
        /// </summary>
        public DirectoryBasedAssemblyTestLoader() :
            this(DefaultPaths.TestsDirectory)
        {
        }

        /// <summary>
        /// Create a Test Loader that will examine a particular directory for assemblies that contain tests.  
        /// </summary>
        /// <param name="path_to_assemblies">The path to the directory containing assemblies.</param>
        public DirectoryBasedAssemblyTestLoader(String path_to_assemblies)
        {
            logger = LogManager.GetLogger(typeof(DirectoryBasedAssemblyTestLoader));
            logger.DebugFormat("Creating a new DirectoryBasedAssemblyTestLoader({0})", path_to_assemblies);
            m_path_to_tests = path_to_assemblies;
            
        }

        /// <summary>
        /// Load all the tests into a test catalog that belong to the directory used during creation.
        /// </summary>
        /// <returns>A Catalog of found tests.  If no tests found an empty catalog is returned.</returns>
        public TestCatalog loadAllTests()
        {
            TestCatalog retval = new TestCatalog();
            logger.DebugFormat("loadAllTests() called for DirectoryBasedAssemblyTestLoader({0})", m_path_to_tests);

            if (Directory.Exists(m_path_to_tests))
            {
                logger.DebugFormat("Directory {0} exists, looking for assemblies in that directory.", m_path_to_tests);
                String[] assemblies = Directory.GetFiles(m_path_to_tests, AssemblyFilter);
                logger.DebugFormat("Found {0} assemblies in directory {1}.", assemblies.Length, m_path_to_tests);
                foreach (String assembly in assemblies)
                {
                    logger.DebugFormat("Attempting to load assembly {0}.", assembly);
                    Assembly asm = Assembly.LoadFrom(assembly);
                    foreach (Type t in asm.GetTypes())
                    {
                        retval.addTestCase(t);
                    }
                }
            }
            else
            {
                logger.WarnFormat("loadAllTests() called, but directory {0} does not exists.  TestCatalog will be empty.", m_path_to_tests);
            }
            return retval;
        }
    }
}