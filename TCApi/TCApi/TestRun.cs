using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using log4net;
using Ninject.Core;
using CookComputing.XmlRpc;

namespace QA.Common.TCApi
{
    public interface ITestRun
    {
        /// <summary>
        /// Run a test plan from the catalog.
        /// </summary>
        /// <param name="testPlan">TestPlan detailing which tests to run in which order.</param>
        /// <param name="catalog">The catalog of tests out of which to get test classes.</param>
        /// <returns>The number of tests run.</returns>
        int run(ITestPlan testPlan, TestCatalog catalog);
    }

    public class TestWithResult
    {
        public ITestCase Test { get; set; }
        public TEST_RESULTS Result { get; set; }
    }

    public class BasicTestRun : ITestRun
    {
        private IKernel kernel;
        protected TCApiConfiguration config;
        private ITestOutput output;
        private ILoggingController logging_controller;

        protected IRuntimeOptions Options { get; set; }

        protected int TestNumber
        {
            get;
            set;
        }

        protected int Passed { get; set; }
        protected int Failed { get; set; }
        protected int Crashed { get; set; }
        protected List<TestWithResult> Results { get; set; }

        [Inject]
        public BasicTestRun(IKernel p_kernel, TCApiConfiguration p_config, ITestOutput p_output, ILoggingController p_logging_controller, IRuntimeOptions p_options)
        {
            kernel = p_kernel;
            config = p_config;
            output = p_output;
            logging_controller = p_logging_controller;
            Options = p_options;
            Passed = 0;
            Failed = 0;
            Crashed = 0;
            Results = new List<TestWithResult>();
        }

        public virtual void initialize(List<TestInformation> tests)
        {
        }

        public virtual void prepareForTest(ITestCase tc)
        {
            String test_output_path = Path.Combine(output.getPath("SessionOutput"), TestNumber + "-" + tc.GetType().ToString());
            output.setTestCaseOutputPath(tc, test_output_path);
        }

        public int run(ITestPlan testPlan, TestCatalog catalog)
        {
            List<TestInformation> tests = testPlan.loadTestPlan(catalog);
            initialize(tests);
            for (TestNumber = 1; TestNumber <= tests.Count; TestNumber++)
            {
                TestInformation test = tests[TestNumber - 1];
                ITestCase test_instance = test.getInstance(kernel);
                prepareForTest(test_instance);
                ITestRunner runner = test.getRunnerFor();
                TEST_RESULTS result = runner.runTest(test_instance, config);
                handleResult(test_instance, result);
            }
            cleanup();
            return tests.Count;
        }

        public virtual void handleResult(ITestCase tc, TEST_RESULTS result)
        {
            // Used for TEST-[plan].xml output later
            TestWithResult tcres = new TestWithResult();
            tcres.Test = tc;
            tcres.Result = result;
            Results.Add(tcres);
            if (result == TEST_RESULTS.Pass)
                Passed++;
            else if (result == TEST_RESULTS.Fail)
                Failed++;
            else
                Crashed++;

            Console.WriteLine("{0} - {1}: {2}", TestNumber, tc.GetType(), result);
            try
            {
                // These need to be in a try block in case the test was not an AbstractTestCase, or never logged
                logging_controller.closeTestCaseLogger(tc);
                Directory.Move(output.getTestCaseOutputPath(tc), output.getTestCaseOutputPath(tc) + "-" + result.ToString());
            }
            catch (Exception)
            {
                //TODO: add logging to runtime.log
            }
        }

        public virtual void cleanup()
        {
            // Output the TEST-[plan].xml
            string plan_name = "results";
            if (Options.TestPlanName != null && Options.TestPlanName != String.Empty)
            {
                plan_name = Options.TestPlanName;
            }

            XmlTextWriter xmloutput = new XmlTextWriter(Path.Combine(Path.Combine(output.OutputRoot, "last"), "TEST-" + plan_name + ".xml"), Encoding.UTF8);
            xmloutput.Formatting = Formatting.Indented;;
            xmloutput.WriteStartDocument();
            xmloutput.WriteStartElement("testsuite");

            xmloutput.WriteStartAttribute("errors");
            xmloutput.WriteString(Crashed.ToString());
            xmloutput.WriteEndAttribute();

            xmloutput.WriteStartAttribute("failures");
            xmloutput.WriteString(Failed.ToString());
            xmloutput.WriteEndAttribute();

            xmloutput.WriteStartAttribute("name");
            xmloutput.WriteString(plan_name);
            xmloutput.WriteEndAttribute();

            xmloutput.WriteStartAttribute("tests");
            xmloutput.WriteString((Crashed + Failed + Passed).ToString());
            xmloutput.WriteEndAttribute();

            xmloutput.WriteStartElement("properties");
            foreach (String key in config.Keys)
            {
                xmloutput.WriteStartElement("property");
                xmloutput.WriteStartAttribute("name");
                xmloutput.WriteString(key);
                xmloutput.WriteEndAttribute();
                xmloutput.WriteStartAttribute("value");
                xmloutput.WriteString(config[key]);
                xmloutput.WriteEndAttribute();
                xmloutput.WriteEndElement();
            }
            xmloutput.WriteEndElement(); // properties

            foreach (TestWithResult test in Results)
            {
                xmloutput.WriteStartElement("testcase");
                xmloutput.WriteStartAttribute("classname");
                xmloutput.WriteString(test.Test.GetType().ToString());
                xmloutput.WriteEndAttribute();
                xmloutput.WriteStartAttribute("name");
                xmloutput.WriteString(TCName.getNameFrom(test.Test.GetType()));
                xmloutput.WriteEndAttribute();
                if (test.Result == TEST_RESULTS.Crash)
                {
                    xmloutput.WriteStartElement("error");
                    if (typeof(AbstractTestCase).IsAssignableFrom(test.Test.GetType()) && ((AbstractTestCase)test.Test).ErrorFromTest != null)
                    {
                        Exception ex = ((AbstractTestCase)test.Test).ErrorFromTest;
                        xmloutput.WriteStartAttribute("message");
                        xmloutput.WriteString(ex.Message);
                        xmloutput.WriteEndAttribute();
                        xmloutput.WriteStartAttribute("type");
                        xmloutput.WriteString(ex.GetType().ToString());
                        xmloutput.WriteEndAttribute();
                        xmloutput.WriteString(ex.StackTrace);
                    }
                    xmloutput.WriteEndElement(); // error
                }
                else if (test.Result == TEST_RESULTS.Fail)
                {
                    xmloutput.WriteStartElement("failure");
                    if (typeof(AbstractTestCase).IsAssignableFrom(test.Test.GetType()) && ((AbstractTestCase)test.Test).ErrorFromTest != null)
                    {
                        Exception ex = ((AbstractTestCase)test.Test).ErrorFromTest;
                        xmloutput.WriteStartAttribute("message");
                        xmloutput.WriteString(ex.Message);
                        xmloutput.WriteEndAttribute();
                        xmloutput.WriteStartAttribute("type");
                        xmloutput.WriteString(ex.GetType().ToString());
                        xmloutput.WriteEndAttribute();
                        xmloutput.WriteString(ex.StackTrace);
                    }
                    //TODO: if no exception, read in log file and put it in the failure report...
                    xmloutput.WriteEndElement(); // failure
                }
                xmloutput.WriteEndElement(); // testcase
            }

            // This is apparently always needed
            xmloutput.WriteStartElement("system-out");
            xmloutput.WriteCData(String.Empty);
            xmloutput.WriteEndElement(); // system-out
            xmloutput.WriteStartElement("system-err");
            xmloutput.WriteCData(String.Empty);
            xmloutput.WriteEndElement(); // system-err

            xmloutput.WriteEndElement(); // testsuite
            xmloutput.WriteEndDocument();
            xmloutput.Close();

        }
    }

    public class SlickCentralTestRun : BasicTestRun
    {
        private ISlickCentral websrvc;
        private SlickBuildInfo build;
        private SlickProjectInfo project;
        private SlickTestPlanInfo test_plan;
        private static ILog logger = LogManager.GetLogger(typeof(SlickCentralTestRun));

        [Inject]
        public SlickCentralTestRun(IKernel p_kernel, TCApiConfiguration p_config, ITestOutput p_output, ILoggingController p_logging_controller, IRuntimeOptions p_options)
            : base(p_kernel, p_config, p_output, p_logging_controller, p_options)
        {
            websrvc = XmlRpcProxyGen.Create<ISlickCentral>();
        }

        public override void initialize(List<TestInformation> tests)
        {
            base.initialize(tests);
            ListResult<SlickProjectInfo> projects = websrvc.read_projects_by_name(config["slick.project"]);
            if (projects.SuccessfulResult)
            {
                project = projects.ListData[0];
            }
            else
            {
                project = new SlickProjectInfo();
                project.ProjectId = 1;
            }
            build = websrvc.read_build_by_id(project.DefaultBuildId).Data;
            test_plan = findTestPlan();
        }

        public SlickBuildInfo findMostRecentBuild()
        {
            logger.Debug("Finding most recent build.");
            SlickBuildInfo retval = null;
            ListResult<SlickBuildInfo> builds = websrvc.read_builds(project.ProjectId);
            logger.DebugFormat("Retrieved a list of {0} builds, finding most recent.", builds.ListData.Length);

            foreach (SlickBuildInfo possible in builds.ListData)
            {
                Console.WriteLine("Build {0} built on {1}", possible.BuildName, possible.BuildTimestamp);
                if (retval == null || possible.BuildTimestamp.CompareTo(retval.BuildTimestamp) > 0)
                {
                    logger.DebugFormat("Found a more recent build {0}.", possible.BuildName);
                    retval = possible;
                }
            }

            return retval;
        }

        public SlickTestPlanInfo findTestPlan()
        {
            SlickTestPlanInfo retval = null;
            if (Options.TestPlanName != null && Options.TestPlanName != String.Empty)
            {
                ListResult<SlickTestPlanInfo> test_plans = websrvc.read_all_test_plans(project.ProjectId);
                String filename = Options.TestPlanName + ".txt";
                foreach (SlickTestPlanInfo possible in test_plans.ListData)
                {
                    if (possible.FileName == filename)
                    {
                        retval = possible;
                        break;
                    }
                }
            }

            return retval;
        }

        public override void handleResult(ITestCase tc, TEST_RESULTS result)
        {
            base.handleResult(tc, result);
            SlickResultInfo result_info = new SlickResultInfo();
            result_info.BuildId = build.BuildId;
            result_info.TestName = TCName.getNameFrom(tc.GetType());
            result_info.Result = result.ToString().ToUpper();
            if (result_info.Result == "CRASH")
                result_info.Result = "SCRIPT_ERROR";
            if (test_plan == null)
            {
                result_info.TestPlanId = 1; // defaults to No Test Plan
            }
            else
            {
                result_info.TestPlanId = test_plan.TestPlanId;
            }

            SingleResult<SlickResultCreationInfo> result_id_info = websrvc.create_result(result_info);
            if (result_id_info.SuccessfulResult)
            {
                logger.InfoFormat("Posted result with id {0}.", result_id_info.Data.ResultId);
            }
            else
            {
                foreach (String error in result_id_info.Errors)
                {
                    logger.WarnFormat("Error recieved while trying to create result: {0}", error);
                }
            }

        }
    }
}

