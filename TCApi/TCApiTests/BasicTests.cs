using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Ninject.Core;
using QA.Common.TCApi;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace QA.Common.TCApi.Tests.Basic
{
    public class OptionPresent : AbstractTestCase
    {
        public override TEST_RESULTS tcDoTest()
        {
            if (!tc_info.ContainsKey("TestOption"))
            {
                TCLog.Audit("Failure: TestOption not present.");
                return TEST_RESULTS.Fail;
            }
            else
            {
                TCLog.AuditFormat("Pass: TestOption present with value '{0}'.", tc_info["TestOption"]);
                return TEST_RESULTS.Pass;
            }
        }
    }

    [TCName("No Logging Test"),
     TCGroup("TCApi")]
    public class NoLoggingTest : AbstractTestCase
    {
        public override TEST_RESULTS tcDoTest()
        {
            return TEST_RESULTS.Pass;
        }
    }

    [TCName("Shutdown Hook"),
     TCGroup("TCApi")]
    public class ShutdownHook : AbstractTestCase, IShutdownHook
    {
        public override TEST_RESULTS tcDoTest()
        {
            ShutdownHooks.register(this);
            TCLog.Audit("This test always passes, look for the print out at the end that the shutdown hook occured.");
            return TEST_RESULTS.Pass;
        }

        public void onShutdown()
        {
            Console.WriteLine("Shutdown hook succeeded.");
        }
    }

    // Took it out of TCApi as this test doesn't work on mono / on a build server
    [TCName("TCApi: Screenshot Output"),
     TCGroup("Extended"),
     TCGroup("WindowsOnly")]
    public class ScreenshotTest : AbstractTestCase
    {
        String testOutputPath = String.Empty;

        [Inject]
        public ITestOutput TestOutput { get; set; }

        public override void tcSetup(Dictionary<string, string> configuration)
        {
            base.tcSetup(configuration);
            TCLog.Debug("Initializing logging for this test.");
            testOutputPath = TestOutput.getTestCaseOutputPath(this);
            TCLog.AuditFormat("Output path for test case: {0}", testOutputPath);
        }

        public override TEST_RESULTS tcDoTest()
        {
            String[] pictures = Directory.GetFiles(testOutputPath, "*.png");
            if (pictures.Length != 0)
            {
                TCLog.AuditFormat("SCRIPT ERROR: There are already {0} pictures in the output directory '{1}'.", pictures.Length, testOutputPath);
                return TEST_RESULTS.Crash;
            }
            else
            {
                TCLog.AuditFormat("There are no pictures currently in output directory '{0}'.", testOutputPath);
            }

            TCLog.logScreenShot();

            pictures = Directory.GetFiles(testOutputPath, "*.png");

            if (pictures.Length == 1)
            {
                TCLog.AuditFormat("PASS: There was one picture ({0}) in the output directory after calling logScreenShot().", pictures[0]);
                return TEST_RESULTS.Pass;
            } else
            {
                TCLog.AuditFormat("FAIL: There was {0} pictures in the output directory after calling logScreenShot, expecting 1.", pictures.Length);
                return TEST_RESULTS.Fail;
            }
        }
    }

    [TCName("TCApi: ValidationError thrown causes Failure."),
     TCGroup("TCApi")]
    public class ValidationErrorTest : AbstractTestCase
    {

        public override TEST_RESULTS tcDoTest()
        {
            ITestRunner test_runner = DefaultRunner.getRunnerFor(typeof(CausesValidationError));
            TEST_RESULTS result = test_runner.runTest(new CausesValidationError(), tc_info);
            if (result == TEST_RESULTS.Fail)
            {
                TCLog.Audit("PASS: A Testcase that threw a ValidationError during tcDoTest caused a Failure as expected.");
                return TEST_RESULTS.Pass;
            }
            else
            {
                TCLog.AuditFormat("FAIL: the inner test thew a ValidationError during tcDoTest which should have caused a Failure, but resulted in: {0}", result);
                return TEST_RESULTS.Fail;
            }
        }
    }

    /// <summary>
    /// Do not use, only for use inside of ValidationErrorTest
    /// </summary>
    public class CausesValidationError : ITestCase
    {
        public TEST_RESULTS tcDoTest()
        {
            throw new ValidationError("This error is expected.");
        }


        #region ITestCase Members

        public void tcSetup(Dictionary<string, string> configuration)
        {
        }

        public void tcCleanup()
        {
        }

        public bool handleException(TestCaseStage p_stage, Exception p_error)
        {
            return false;
        }

        #endregion
    }

    [TCName("TCApi: configValue throws exception (with config key) if config key missing"),
     TCGroup("TCApi")]
    public class ConfigValueTest : AbstractTestCase
    {
        public override TEST_RESULTS tcDoTest()
        {
            try
            {
                configValue("invalid key");
            }
            catch (ConfigurationMissingError ex)
            {
                TCLog.Audit("Caught expected exception", ex);
                if (ex.Message.Contains("invalid key"))
                {
                    TCLog.Audit("PASS: exception thrown has 'invalid key' in the message.");
                    return TEST_RESULTS.Pass;
                }
                else
                {
                    TCLog.Audit("FAIL: exception thrown did not contain the key name 'invalid key'.");
                    return TEST_RESULTS.Fail;
                }
            }
            TCLog.Audit("FAIL: no exception thrown when looking for key 'invalid key'");
            return TEST_RESULTS.Fail;
        }
    }
	
	[TCName("TCApi: Bad guid causes tcrun to crash."),
	 TCGroup("TCApi"),
	 TCGuid("BadGuid")]
	public class BadGuidTest : AbstractTestCase
	{
		public override TEST_RESULTS tcDoTest ()
		{
			TCLog.Audit ("PASS: If this test ran, then it passed as it has a bad guid.");
			return TEST_RESULTS.Pass;
		}
	}

    [TCName("TCApi: Asserts from NUnit.Framework cause Failure."),
     TCGroup("TCApi")]
    public class NUnitAssertsTest : AbstractTestCase
    {

        public override TEST_RESULTS tcDoTest()
        {
            ITestRunner test_runner = DefaultRunner.getRunnerFor(typeof(CausesAssertionException));
            TEST_RESULTS result = test_runner.runTest(new CausesAssertionException(), tc_info);
            Check.That(result, Is.EqualTo(TEST_RESULTS.Fail), "Assert.IsTrue(false) during tcDoTest should cause a failure.");
            return TEST_RESULTS.Pass;
        }
    }

    /// <summary>
    /// Do not use, only for use inside of NUnitAssertsTest
    /// </summary>
    public class CausesAssertionException : ITestCase
    {
        public TEST_RESULTS tcDoTest()
        {
            Assert.IsTrue(false, "This should cause a failure.");
            // this code should never be reached.
            return TEST_RESULTS.Pass;
        }

        public void tcSetup(Dictionary<string, string> configuration)
        {
        }

        public void tcCleanup()
        {
        }

        public bool handleException(TestCaseStage p_stage, Exception p_error)
        {
            return false;
        }
    }

	[TCName("TCApi: Version reported with -v command line option."),
	 TCGroup("TCApi")]
	public class VersionInformationTest : AbstractTestCase
	{
		public override TEST_RESULTS tcDoTest ()
		{
			ProcessStartInfo psi = new ProcessStartInfo (Assembly.GetEntryAssembly ().Location, "-v");
			psi.RedirectStandardOutput = true;
			psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			psi.UseShellExecute = false;
			Process version_output = Process.Start (psi);
			System.IO.StreamReader myOutput = version_output.StandardOutput;
			version_output.WaitForExit (10000);
			if (version_output.HasExited) 
			{
				string output = myOutput.ReadToEnd ().Trim ();
				TCLog.AuditFormat ("Output from tcrun -v: {0}", output);
				Check.That (output, Is.StringMatching ("tcrun version: \\d\\.\\d\\.\\d\\..*"));
			}
			else
			{
				Assert.Fail ("tcrun -v didn't exit after 10 seconds.");
			}
			return TEST_RESULTS.Pass;
		}
	}

}
