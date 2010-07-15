using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using log4net;

namespace QA.Common.TCApi
{
    /// <summary>
    /// The interface which all test runners must impliment.  The individual test runner may customize the workflow.
    /// </summary>
    public interface ITestRunner
    {
        /// <summary>
        /// The only and main method for a test runner.  It is responsible for running a test, and should NOT throw
        /// an exception.  Exceptions from this method would indicate a problem with the test framework, not the
        /// individual test case.
        /// </summary>
        /// <param name="tc">The test case to run.</param>
        /// <param name="conf">The configuration to use (and pass to test case).</param>
        /// <returns>Result of Test Case.</returns>
        TEST_RESULTS runTest(ITestCase tc, Dictionary<String, String> conf);
    }

    /// <summary>
    /// Provide a utility to find the default test runner for a particular class.
    /// </summary>
    public class DefaultRunner
    {
        /// <summary>
        /// Find the default Test Runner for a type.  In the future this will be changable by
        /// adding an annotation detailing which test runner should be used.  This will be
        /// helpful for AbstractTestCase's that need to customize the workflow.
        /// </summary>
        /// <param name="clazz">The type from which to find the default test runner for.</param>
        /// <returns>right now hard coded to return an instance of DefaultITestCaseTestRunner.</returns>
        public static ITestRunner getRunnerFor(Type clazz)
        {
            return new DefaultITestCaseTestRunner();
        }
    }

    /// <summary>
    /// Default test runner for anything implementing the ITestCase class.  This runner implements
    /// only the workflow detailed in the ITestCase interface.
    /// </summary>
    /// <seealso cref="ITestCase"/>
    public class DefaultITestCaseTestRunner : ITestRunner
    {
        private static ILog logger = LogManager.GetLogger(typeof(DefaultITestCaseTestRunner));

        /// <summary>
        /// Run the test according to the workflow described in the ITestCase interface documentation.
        /// </summary>
        /// <param name="tc">The test case to run.</param>
        /// <param name="conf">The configuration to use.</param>
        /// <returns></returns>
        public TEST_RESULTS runTest(ITestCase tc, Dictionary<String, String> conf)
        {
            TEST_RESULTS result = TEST_RESULTS.Crash;
            try
            {
                Type tcType = tc.GetType();
                String tcClassName = String.Join(".", new String[] {tcType.Namespace, tcType.Name});
                logger.DebugFormat("Recieved an instance of {0} to run.", tcClassName);
                logger.DebugFormat("Running tcSetup of instance of {0}.", tcClassName);
                bool runTcDoTest = true;
                using (LogicalThreadContext.Stacks["tcstage"].Push("Setup"))
                {
                    try
                    {
                        tc.tcSetup(conf);
                        logger.InfoFormat("Successfully Ran {0}'s tcSetup.", tcClassName);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Ran into an exception during " + tcClassName + "'s tcSetup(configuration): ", ex);
                        logger.Debug("Running handleException.");
                        runTcDoTest = tc.handleException(TestCaseStage.Setup, ex);
                        logger.DebugFormat("handleException returned {0}.", runTcDoTest);
                    }
                }

                if (runTcDoTest)
                {
                    using (LogicalThreadContext.Stacks["tcstage"].Push("DoTest"))
                    {
                        logger.DebugFormat("Running tcDoTest of class {0}.", tcClassName);
                        try
                        {
                            result = tc.tcDoTest();
                            logger.InfoFormat("Successfully ran {0}'s tcDoTest with a result of {1}.", tcClassName, result);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("Ran into an exception during " + tcClassName + "'s tcDoTest():", ex);
                            logger.DebugFormat("Calling handleException on class {0}, but ignoring result.", tcClassName);
                            tc.handleException(TestCaseStage.DoTest, ex);
                            logger.Debug("tcCleanup is always called regardless of an exception during tcDoTest().");

                            // if it's a validation error, the test will fail, otherwise it's a script error
                            if (typeof(ValidationError).IsAssignableFrom(ex.GetType()))
                            {
                                result = TEST_RESULTS.Fail;
                            }
                            else if (typeof(AssertionException).IsAssignableFrom(ex.GetType()))
                            {
                                result = TEST_RESULTS.Fail;
                            }
                            else
                            {
                                result = TEST_RESULTS.Crash;
                            }
                        }
                    }
                }
                else
                {
                    logger.InfoFormat("Not running tcDoTest on test {0}.", tcClassName);
                }
                logger.DebugFormat("Running {0}'s tcCleanup method.", tcClassName);
                using (LogicalThreadContext.Stacks["tcstage"].Push("Cleanup"))
                {
                    try
                    {
                        tc.tcCleanup();
                        logger.InfoFormat("Successfully ran {0}'s tcCleanup method.", tcClassName);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Ran into an exception during " + tcClassName + "'s tcCleanup(): ", ex);
                        logger.DebugFormat("Calling handleException on class {0}, but ignoring result.", tcClassName);
                        tc.handleException(TestCaseStage.Cleanup, ex);
                    }
                }
                logger.InfoFormat("Finished test {0} with a result of {1}.", tcClassName, result);
            }
            catch (Exception ex)
            {
                logger.Error("Unknown and unexpected error occured while running a test case: ", ex);
            }
            
            return result;
        }
    }
}
