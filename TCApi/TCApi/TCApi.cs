using System;
using System.IO;
using System.Collections.Generic;
using log4net;
using IniParser;
using Ninject.Core;
using NUnit.Framework.Constraints;
using NUnit.Framework;


namespace QA.Common.TCApi
{
    /// <summary>
    /// This class is for validation related errors, specifically those instances in which
    /// it is FOR SURE known that an expected result was not achieved.  This is NOT for
    /// unanticipated errors, but specifically for anticipated ones.
    /// </summary>
    public class ValidationError : Exception
    {
        public ValidationError(String validationMessage)
            : base("A Validation Error occured: " + validationMessage)
        {
        }

        public ValidationError(String message, Object expected, Object actual)
            : this("Expected <" + expected.ToString() + ">, Actual <" + actual + ">: " + message)
        {
        }
    }

    /// <summary>
    /// Indication of a test's success or failure.
    /// </summary>
    public enum TEST_RESULTS 
    { 
        /// <summary>
        /// Result of a passed test case.
        /// </summary>
        Pass = 0, 

        /// <summary>
        /// Result of a failed test case.
        /// </summary>
        Fail,
 
        /// <summary>
        /// Result of a crash or error in the test case.
        /// </summary>
        Crash 
    };

    /// <summary>
    /// Attribute providing test case number meta-data.
    /// </summary>
    /// <example>
    ///     <code>
    ///         [TCNumber(5413)]
    ///         public class GetAllContactsTest : AbstractTestCase
    ///         {
    ///             //...
    ///         }
    ///     </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class)]
    public class TCNumber : Attribute
    {
        private int m_number;

        /// <summary>
        /// Create a TCNumber attribute with a specific number (this is the only way to set the number).
        /// </summary>
        /// <param name="p_number">The test case number or "id".</param>
        public TCNumber(int p_number)
        {
            m_number = p_number;
        }

        /// <summary>
        /// Property providing read only access to the test case number.
        /// </summary>
        public int Number
        {
            get { return m_number; }
        }

        /// <summary>
        /// Retrieve the test case number from a type, if one exists.
        /// </summary>
        /// <param name="clazz">The type from which to get the value of the TCNumber attribute.</param>
        /// <returns>The value of the TCNumber attribute for that type if the attribute exists, and -1 on failure.</returns>
        static public int getNumberFrom(Type clazz)
        {
            int retval = -1;
            ILog logger = LogManager.GetLogger(typeof(TCNumber).Name + "." + "getNumberFrom");
            logger.DebugFormat("Trying to get test case number from {0}.{1}", clazz.Namespace, clazz.Name);
            Object[] attrs = clazz.GetCustomAttributes(typeof(TCNumber), true);
            if (attrs != null && attrs.Length > 0)
            {
                logger.DebugFormat("Found a TCNumber attribute on class {0}.{1}", clazz.Namespace, clazz.Name);
                TCNumber attr = attrs[0] as TCNumber;
                retval = attr.Number;
            }
            else
            {
                logger.DebugFormat("No TCNumber attribute on class {0}.{1}, returning -1", clazz.Namespace, clazz.Name);
            }

            logger.DebugFormat("Returning {0} from getNumberFrom({1}.{2}).", retval, clazz.Namespace, clazz.Name);

            return retval;
        }
    }

    /// <summary>
    /// Uniquely identify your test with a GUID.
    /// </summary>
    /// <remarks>
    /// This is a good way to uniquely identify a test, without tying it to a particular test
    /// database or other grouping.  GUID's aren't excatly user friendly, but for automation
    /// purposes they can be useful.
    /// </remarks>
    /// <example>
    ///     <code>
    ///         [TCNumber(5413),
    ///          TCGuid("6f47dd3a-9f4b-44ef-92fa-dcc4a4283793")]
    ///         public class GetAllContactsTest : AbstractTestCase
    ///         {
    ///             //...
    ///         }
    ///     </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class)]
    public class TCGuid : Attribute
    {
        private Guid m_guid;

        /// <summary>
        /// Initialize a new TCGuid attribute with a Guid object.
        /// </summary>
        /// <param name="p_guid">The guid to identify this test case.</param>
        public TCGuid(Guid p_guid)
        {
            m_guid = p_guid;
        }

        /// <summary>
        /// Initialize a new TCGuid attribute using a String version of the Guid.
        /// </summary>
        /// <param name="p_guid_string">The guid in string form.</param>
        public TCGuid(String p_guid_string)
        {
            try
            {
                m_guid = new Guid(p_guid_string);
            } 
            catch(FormatException ex)
            {
                m_guid = Guid.Empty;
            }
        }

        /// <summary>
        /// The guid for this test case.
        /// </summary>
        public Guid GUID
        {
            get { return m_guid; }
        }

        /// <summary>
        /// Get the GUID from an attribute on a test case class.
        /// </summary>
        /// <param name="clazz">The type from which to find a TCGuid attribute.</param>
        /// <returns>Empty Guid attribute found, the Guid otherwise.</returns>
        static public Guid getGuidFrom(Type clazz)
        {
            Guid retval = Guid.Empty;
            ILog logger = LogManager.GetLogger(typeof(TCGuid).Name + "." + "getGuidFrom");
            logger.DebugFormat("Attempting to get a guid from class {0}.", clazz.ToString());

            Object[] attrs = clazz.GetCustomAttributes(typeof(TCGuid), true);
            if (attrs != null && attrs.Length > 0)
            {
                logger.DebugFormat("Found a TCGuid attribute on class {0}.", clazz.ToString());
                retval = (attrs[0] as TCGuid).GUID;
            }
            else
            {
                logger.DebugFormat("No TCGuid attribute on class {0}, returning empty Guid.", clazz.ToString());
            }

            logger.DebugFormat("Returning {0} from getGuidFrom({1}).", retval, clazz.ToString());
            return retval;
        }
    }

    /// <summary>
    /// Give your automated test a Name.
    /// </summary>
    /// <remarks>
    /// Naming your test can help others identify what it is your test is supposed to do.  Though not required,
    /// this can help with determining which tests to run.  Also if/when the synchronization of Perforce and
    /// SCTM is done this may be necessary to name your tests.
    /// </remarks>
    /// <example>
    ///     <code>
    ///         [TCNumber(5413),
    ///          TCName("Get All Contacts - Simple Validation")]
    ///         public class GetAllContactsTest : AbstractTestCase
    ///         {
    ///             //...
    ///         }
    ///     </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class)]
    public class TCName : Attribute
    {
        private String m_name;

        /// <summary>
        /// Provide the name of the test to label it.
        /// </summary>
        /// <param name="name"></param>
        public TCName(String name)
        {
            m_name = name;
        }

        /// <summary>
        /// The name of the test.
        /// </summary>
        public String Name
        {
            get { return m_name; }
        }

        /// <summary>
        /// Get the test case's name from a type.
        /// </summary>
        /// <param name="clazz">The type of the test case.</param>
        /// <returns>String.Empty if the type is null, the class name if there is no TCName attribute, the value of the TCName attribute otherwise.</returns>
        static public String getNameFrom(Type clazz)
        {
            String retval = String.Empty;
            if (clazz != null)
            {
                retval = clazz.ToString();
            }

            ILog logger = LogManager.GetLogger(typeof(TCName).Name + "." + "getNameFrom");
            logger.DebugFormat("Attempting to get a test name from class {0}.", clazz.ToString());

            Object[] attrs = clazz.GetCustomAttributes(typeof(TCName), true);
            if (attrs != null && attrs.Length > 0)
            {
                logger.DebugFormat("Found a TCName attribute on class {0}.", clazz.ToString());
                retval = (attrs[0] as TCName).Name;
            }
            else
            {
                logger.DebugFormat("No TCName attribute on class {0}, returning empty String.", clazz.ToString());
            }

            logger.DebugFormat("Returning {0} from getNameFrom({1}).", retval, clazz.ToString());
            return retval;
        }
    }

    /// <summary>
    /// Attribute for labeling the group of a test case.  This may be useful for
    /// filtering tests or loading a specific set of tests together.  This attribute
    /// may be used several times to indicate several group memberships.
    /// </summary>
    /// <example>
    ///     <code>
    ///         [TCNumber(425),
    ///          TCGroup("WebServiceTests"),
    ///          TCGroup("Regression")]
    ///         public class GetAllContactsTest : AbstractTestCase
    ///         {
    ///             //...
    ///         }
    ///     </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public class TCGroup : Attribute
    {
        private String m_group_name;

        /// <summary>
        /// Create a group label attribute.
        /// </summary>
        /// <param name="p_group_name">Name of the group, it is case sensitive, and should not be null.</param>
        public TCGroup(String p_group_name)
        {
            m_group_name = p_group_name;
        }

        /// <summary>
        /// The name of this group.
        /// </summary>
        public String GroupName
        {
            get { return m_group_name; }
        }

        /// <summary>
        /// Get all the names of the groups to which this test belongs to.
        /// </summary>
        /// <param name="p_tc">The test case type to get groups from.</param>
        /// <returns>A list of groups the test case has been annotated with.  If it doesn't belong
        /// to any groups, or is not a test case, an empty list is returned.</returns>
        static public List<String> getGroupsFrom(Type p_tc)
        {
            List<String> groups = new List<string>();

            ILog logger = LogManager.GetLogger(typeof(TCGroup) + ".getGroupsFrom");
            logger.DebugFormat("Trying to get test groups from {0}", p_tc);
            Object[] attrs = p_tc.GetCustomAttributes(typeof(TCGroup), true);
            if (attrs != null && attrs.Length > 0)
            {
                logger.DebugFormat("Found {0} TCGroup attribute(s) on class {1}.", attrs.Length, p_tc);
                foreach (Object attr in attrs)
                {
                    TCGroup groupAttr = attr as TCGroup;
                    logger.DebugFormat("Found TCGroup({0}) attribute on class {1}.", groupAttr.GroupName, p_tc);
                    groups.Add(groupAttr.GroupName);
                }
            }
            else
            {
                logger.DebugFormat("No TCGroup attributes on class {0}, returning empty list.", p_tc);
            }

            logger.DebugFormat("Returning {0} group name(s) from getGroupsFrom({1}).", groups.Count, p_tc);

            return groups;
        }
    }

    /// <summary>
    /// An enum describing the various stages of an automated test case.
    /// </summary>
    public enum TestCaseStage
    {
        /// <summary>
        /// The setup or initialization phase of a test.
        /// </summary>
        Setup,

        /// <summary>
        /// The testing or main portion of a test.
        /// </summary>
        DoTest,

        /// <summary>
        /// The cleanup or finishing portion of a test.
        /// </summary>
        Cleanup
    };

    /// <summary>
    /// Interface of a test case.  ALL Tests MUST implement this interface!  
    /// </summary>
    /// <remarks>
    ///     <para>Requirements:
    ///         <list type="bullet">
    ///             <item>Implementing class must have a public no argument constructor.</item>
    ///             <item>A null return value from tcDoTest will indicate a TEST_RESULTS.Passed</item>
    ///         </list>
    ///     </para>
    ///     <para>Workflow of a test case is as follows:
    ///         <list type="number">
    ///             <item>Using reflection create a new instance of the class.</item>
    ///             <item>Provide configuration data and call the first stage (tcSetup).</item>
    ///             <item>If exception thrown during stage, call handleException and only call tcDoTest if return value is true, otherwise skip to cleanup.</item>
    ///             <item>Call stage 2 (DoTest) and record result of test.</item>
    ///             <item>If exception thrown during stage, call handleException, ignore return value (Cleanup is always called).  Result of stage is automatically set to TEST_RESULTS.Crashed.</item>
    ///             <item>Call stage 3 (Cleanup) and record stage result.</item>
    ///             <item>Report and log result of test.</item>
    ///         </list>
    ///     </para>
    /// </remarks>
    public interface ITestCase
    {
        /// <summary>
        /// This method should initialize the test case and it's requirements.
        /// </summary>
        /// <param name="configuration">A flat listing of all configuration and data needed for the test.</param>
        void tcSetup(Dictionary<String, String> configuration);

        /// <summary>
        /// Perform the test itself.  Any exceptions thrown by this method will cause an automatic result of TEST_RESULTS.Crashed.
        /// A null return value will indicate an automatic result of TEST_RESULTS.Passed.
        /// </summary>
        /// <returns>Result of the test case.</returns>
        TEST_RESULTS tcDoTest();

        /// <summary>
        /// Cleanup any changes to the system and resources used while testing.
        /// </summary>
        void tcCleanup();

        /// <summary>
        /// Handle an exception from any one of the stages.  the return value is only considered if the stage is
        /// the setup stage.
        /// </summary>
        /// <param name="p_stage">The stage of the test.</param>
        /// <param name="p_error">The exception that was thrown.</param>
        /// <returns>true if workflow should continue, false otherwise.</returns>
        bool handleException(TestCaseStage p_stage, Exception p_error);
    }

    /// <summary>
    /// Feature filled base class for a test case.  This class implements the ITestCase interface partially, but the
    /// sub class must implement the tcDoTest method.
    /// </summary>
    /// <remarks>
    /// AbstractTestCase provides default implementations of tcSetup and tcCleanup.  They are marked virtual, so
    /// you can override them if you wish. This class also adds a getLogger function to help you log test events.
    /// </remarks>
    public abstract class AbstractTestCase : ITestCase
    {
        /// <summary>
        /// Configuration information passed into tcSetup.
        /// </summary>
        protected Dictionary<string, string> tc_info;

        private ITestCaseLogger tc_logger = null;

        private ILoggingController m_loggingController;

        private IResourceFactory m_resourceFactory;

        /// <summary>
        /// For automatic injection use. Setter only.
        /// </summary>
        [Inject]
        public ILoggingController LoggingController
        {
            set { m_loggingController = value; }
        }

        /// <summary>
        /// Resource Factory, used for obtaining resources.  Automatically injected.
        /// </summary>
        [Inject]
        public IResourceFactory ResourceFactory
        {
            set { m_resourceFactory = value; }
            get { return m_resourceFactory; }
        }
        [Inject,
         SaveOption]
        public Boolean SaveValidation;

        /// <summary>
        /// This will be set to the exception that caused the test to crash or fail, if handleException was called.
        /// </summary>
        public Exception ErrorFromTest { get; set; }

        /// <summary>
        /// Test case logger that allows audit level logging, and logging of files and screen shots.
        /// </summary>
        protected ITestCaseLogger TCLog
        {
            get
            {
                    if (tc_logger == null)
                    {
                        tc_logger = m_loggingController.configureTestCaseLogger(this);
                    }
                return tc_logger;
            }
        }

        static private ILog abstract_tc_log = LogManager.GetLogger(typeof(AbstractTestCase));

        private NUnitAssertConstraint check_constraint;

        /// <summary>
        /// Property used for NUnit style constraints.  See http://nunit.org/index.php?p=constraintModel&r=2.5.5 for details.
        /// Instead of using:
        ///  <code>
        ///     Assert.That( myString, Is.EqualTo("Hello") );
        ///  </code>
        /// Do:
        ///  <code>
        ///     Check.That( myString, Is.EqualTo("Hello") );
        ///  </code>
        /// Credit goes to Lee Higginson for the name of the property.
        /// </summary>
        protected NUnitAssertConstraint Check 
        { 
            get 
            { 
                if (check_constraint == null)
                {
                    check_constraint = new NUnitAssertConstraint(TCLog);
                }
                return check_constraint;
            }
        }

        /// <summary>
        /// Default setup (you can override in a sub class), assigns configuration to a tc_info member variable.
        /// </summary>
        /// <param name="configuration">Runtime configuration information.</param>
        public virtual void tcSetup(Dictionary<String, String> configuration)
        {
            abstract_tc_log.Debug("Inside default implementation of tcSetup(configuration)");
            tc_info = configuration;
        }

        /// <summary>
        /// Abstract (you must implement in subclass) testing method.
        /// </summary>
        /// <returns>Test Result.</returns>
        public abstract TEST_RESULTS tcDoTest();

        /// <summary>
        /// Default implementation of tcCleanup does nothing (except a debug log statement).
        /// </summary>
        public virtual void tcCleanup()
        {
            // by default do nothing...
            abstract_tc_log.Debug("Inside default implementation of tcCleanup().");
        }

        /// <summary>
        /// By default the handleException method will log the error and always return false.
        /// The return value is only considered when an exception is thrown from the tcSetup stage.
        /// </summary>
        /// <param name="stage">stage of the test</param>
        /// <param name="ex">The exception thrown.</param>
        /// <returns>false, always.</returns>
        public virtual bool handleException (TestCaseStage stage, Exception ex)
        {
        	abstract_tc_log.Debug ("Inside default implementation of handleException(stage, exception).");
        	abstract_tc_log.Error ("Exception occured during " + stage.ToString () + " stage of testing.", ex);
        	ErrorFromTest = ex;
        	try
			{
        		TCLog.logScreenShot (stage + "-exception.png");
        	} catch (Exception)
			{
				// ignore, don't go into an infinate loop here.
			}
			
            if (typeof(ValidationError).IsAssignableFrom(ex.GetType()))
            {
                TCLog.Error("FAIL: " + ex.Message, ex);
            }
            else
            {
                TCLog.Error("SCRIPT ERROR: " + ex.Message, ex);
            }

            if (stage == TestCaseStage.Setup)
            {
                TCLog.Info("DoTest will not be called as handleException will be returning false");
            }

            return false;
        }

        public IResource resource(String name)
        {
            return ResourceFactory.loadResource(this.GetType(), name);
        }

        public bool validate(Object result, Type validator)
        {
            return validate(result, validator, new Dictionary<String, String>(), "result");
        }

        public bool validate(Object result, Type validator, Dictionary<String, String> options)
        {
            return validate(result, validator, options, "result");
        }

        public bool validate(Object result, Type validator, String resource_name)
        {
            return validate(result, validator, new Dictionary<String, String>(), resource_name);
        }

        public bool validate(Object result, Type validator, Dictionary<String, String> options, String resource_name)
        {
            IValidator worker = Activator.CreateInstance(validator) as IValidator;
            IResource expected = resource(resource_name + worker.extension());
            ValidationResult retval = worker.validateResource(expected, result, options, SaveValidation);
            foreach(String message in retval.Messages)
            {
                TCLog.InfoFormat("{0}: {1}", validator.Name, message);
            }
            return retval.Valid;
        }

        public String configValue(String key)
        {
            abstract_tc_log.DebugFormat("Attempting to get config value for key '{0}'.", key);
            if (!tc_info.ContainsKey(key))
            {
                TCLog.ErrorFormat("Configuration key '{0}' missing.", key);
                throw new ConfigurationMissingError(key);
            }
            return tc_info[key];
        }
    }

    public class ConfigurationMissingError : Exception
    {
        public ConfigurationMissingError(String key)
            : base("ERROR: Test Case configuration key '" + key + "' is missing from any configuration source.")
        {
        }
    }

    /// <summary>
    /// This code comes from NUnit's Assert class.  It is being modified specifically to log
    /// successful constraints, otherwise it is just a copy of the Assert.That portion of
    /// NUnit Assert Class.
    /// </summary>
    public class NUnitAssertConstraint
    {
        private ITestCaseLogger logger;

        public NUnitAssertConstraint(ITestCaseLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="expression">A Constraint to be applied</param>
        /// <param name="actual">The actual value to test</param>
        public void That(object actual, IResolveConstraint expression)
        {
            That(actual, expression, null, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="expression">A Constraint to be applied</param>
        /// <param name="actual">The actual value to test</param>
        /// <param name="message">The message that will be displayed on failure</param>
        public void That(object actual, IResolveConstraint expression, string message)
        {
            That(actual, expression, message, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="expression">A Constraint expression to be applied</param>
        /// <param name="actual">The actual value to test</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public void That(object actual, IResolveConstraint expression, string message, params object[] args)
        {
            Constraint constraint = expression.Resolve();

            MessageWriter writer = new TextMessageWriter(message, args);
            if (!constraint.Matches(actual))
            {
                constraint.WriteMessageTo(writer);
                throw new AssertionException(writer.ToString());
            } else
                constraint.WriteMessageTo(writer);
            {
                logger.AuditFormat("SUCCESSFUL CHECK: {0}", writer.ToString().Replace("\r\n", " ").Replace("\n", " ").Replace("But was", "Actual"));
            }
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="expr">A Constraint expression to be applied</param>
        /// <param name="del">An ActualValueDelegate returning the value to be tested</param>
        public void That(ActualValueDelegate del, IResolveConstraint expr)
        {
            That(del, expr.Resolve(), null, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="expr">A Constraint expression to be applied</param>
        /// <param name="del">An ActualValueDelegate returning the value to be tested</param>
        /// <param name="message">The message that will be displayed on failure</param>
        public void That(ActualValueDelegate del, IResolveConstraint expr, string message)
        {
            That(del, expr.Resolve(), message, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="del">An ActualValueDelegate returning the value to be tested</param>
        /// <param name="expr">A Constraint expression to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public void That(ActualValueDelegate del, IResolveConstraint expr, string message, params object[] args)
        {
            Constraint constraint = expr.Resolve();

            MessageWriter writer = new TextMessageWriter(message, args);
            if (!constraint.Matches(del))
            {
                constraint.WriteMessageTo(writer);
                throw new AssertionException(writer.ToString());
            }
            else
            {
                constraint.WriteMessageTo(writer);
                logger.AuditFormat("SUCCESSFUL CHECK: {0}", writer.ToString().Replace("\r\n", " ").Replace("\n", " ").Replace("But was", "Actual"));
            }
        }


        /// <summary>
        /// Apply a constraint to a referenced value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="expression">A Constraint to be applied</param>
        /// <param name="actual">The actual value to test</param>
        public void That<T>(ref T actual, IResolveConstraint expression)
        {
            That(ref actual, expression.Resolve(), null, null);
        }

        /// <summary>
        /// Apply a constraint to a referenced value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="expression">A Constraint to be applied</param>
        /// <param name="actual">The actual value to test</param>
        /// <param name="message">The message that will be displayed on failure</param>
        public void That<T>(ref T actual, IResolveConstraint expression, string message)
        {
            That(ref actual, expression.Resolve(), message, null);
        }

        /// <summary>
        /// Apply a constraint to a referenced value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="expression">A Constraint to be applied</param>
        /// <param name="actual">The actual value to test</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public void That<T>(ref T actual, IResolveConstraint expression, string message, params object[] args)
        {
            Constraint constraint = expression.Resolve();

            MessageWriter writer = new TextMessageWriter(message, args);
            if (!constraint.Matches(ref actual))
            {
                constraint.WriteMessageTo(writer);
                throw new AssertionException(writer.ToString());
            }
            else
            {
                constraint.WriteMessageTo(writer);
                logger.AuditFormat("SUCCESSFUL CHECK: {0}", writer.ToString().Replace("\r\n", " ").Replace("\n", " ").Replace("But was", "Actual"));
            }
        }

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary> 
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display if the condition is false</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public void That(bool condition, string message, params object[] args)
        {
            That(condition, Is.True, message, args);
        }

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display if the condition is false</param>
        public void That(bool condition, string message)
        {
            That(condition, Is.True, message, null);
        }

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="condition">The evaluated condition</param>
        public void That(bool condition)
        {
            That(condition, Is.True, null, null);
        }

        /// <summary>
        /// Asserts that the code represented by a delegate throws an exception
        /// that satisfies the constraint provided.
        /// </summary>
        /// <param name="code">A TestDelegate to be executed</param>
        /// <param name="constraint">A ThrowsConstraint used in the test</param>
        public void That(TestDelegate code, IResolveConstraint constraint)
        {
            That((object)code, constraint);
        }

    }
}
