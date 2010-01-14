using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net.Core;
using log4net;
using QA.Common.TCApi;
using RJH.CommandLineHelper;
using Ninject.Core;

namespace QA.Common.tcrun
{
    enum TEST_MODE
    {
        LIST,
        EXPORT,
        RUN,
        HELP
    }


    class TCRunOptions : IRuntimeOptions
    {
        private TEST_MODE m_mode = TEST_MODE.RUN;
        private bool m_debug = false;
        private bool m_save = false;
        private bool m_post = false;
        private String environment = "default";
        private String m_plan = String.Empty;
        private Dictionary<String, String> m_addon_config = new Dictionary<string, string>();

        [CommandLineSwitch("list", "List the tests instead of running them."),
         CommandLineAlias("l")]
        public bool List
        {
            set { setTestMode(TEST_MODE.LIST, value); }
            get { return m_mode == TEST_MODE.LIST; }
        }

        [CommandLineSwitch("export", "Export all the tests as a test plan."),
         CommandLineAlias("e")]
        public bool Export
        {
            set { setTestMode(TEST_MODE.EXPORT, value); }
            get { return m_mode == TEST_MODE.EXPORT; }
        }

        [CommandLineSwitch("help", "Get command line help for this command."),
         CommandLineAlias("h")]
        public bool Help
        {
            set { setTestMode(TEST_MODE.HELP, value); }
            get { return m_mode == TEST_MODE.HELP; }
        }

        [CommandLineSwitch("save", "Save the validation result as a resource."),
         CommandLineAlias("s")]
        public bool SaveResources
        {
            get { return m_save; }
            set { m_save = value; }
        }

        [CommandLineSwitch("debug", "Set this run to be a debug run (enable debug logging)."),
         CommandLineAlias("d")]
        public bool Debug
        {
            set { m_debug = value; }
            get { return m_debug; }
        }

        [CommandLineSwitch("environment", "Set which environment ini file is loaded."),
         CommandLineAlias("env")]
        public String EnvironmentName
        {
            get { return environment; }
            set { environment = value; }
        }

        [CommandLineSwitch("plan", "The name of the test plan to load."),
         CommandLineAlias("p")]
        public String TestPlanName
        {
            get { return m_plan; }
            set { m_plan = value; }
        }

        [CommandLineSwitch("post", "Post the results of this testing session")]
        public bool PostResults
        {
            get { return m_post; }
            set { m_post = value; }
        }

        [CommandLineSwitch("option", "Options to be added to the configuration sent to test cases."),
         CommandLineAlias("o")]
        public String Option
        {
            set
            {
                String key = String.Empty;
                String val = String.Empty;
                if (value.Contains('='))
                {
                    String[] keyvalue = value.Split(new char[] { '=' }, 2);
                    key = keyvalue[0];
                    val = keyvalue[1];
                } else
                {
                    key = value;
                }
                if (AddonConfiguration.ContainsKey(key))
                {
                    AddonConfiguration[key] = val;
                }
                else
                {
                    AddonConfiguration.Add(key, val);
                }
            }
        }

        public Dictionary<String, String> AddonConfiguration
        {
            get
            {
                return m_addon_config;
            }
        }

        public TEST_MODE Mode
        {
            get { return m_mode; }
        }

        private void setTestMode(TEST_MODE mode, bool should_set)
        {
            if(should_set)
            {
                m_mode = mode;
            }
        }

    }


    class TCRun
    {

        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("You must specify at least one test number or test group to run.");
                return 1;
            }
            TCRunOptions options = new TCRunOptions();
            Parser cmdLineParser = new Parser(System.Environment.CommandLine, options);

            if (cmdLineParser.Parse())
            {
                IKernel injector = null;
                try
                {
                    injector = new StandardKernel(ModuleFactory.getInjectionModule(options));
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine(ex.Message);
                    return 1;
                }

                Level loglevel = Level.Info;
                if (options.Debug)
                {
                    loglevel = Level.Debug;
                }
                ILoggingController logcontroller = injector.Get<ILoggingController>();
                try
                {
                    logcontroller.configureLogging(loglevel);
                }
                catch (IOException)
                {
                    Console.WriteLine("Error occured during logging configuration, make sure you have no open files (or directories) under the results directory.");
                    return 1;
                }

                Dictionary<String, String> configuration = injector.Get<TCApiConfiguration>();
                
                // I shouldn't have to do this, and maybe there is a call to do it, but I can't find it.
                foreach (String key in options.AddonConfiguration.Keys)
                {
                    configuration[key] = options.AddonConfiguration[key];
                }

                if (options.Mode == TEST_MODE.HELP)
                {
                    Console.WriteLine("Usage: tcrun [-d(--debug)] [-env(--environment) \"default\"] [-h(--help)|-l(--list)|-e(--export)] <-p(--plan) <testplan>|<tests>>");
                    Console.WriteLine("\tenvironment: ini filename (minus the .ini) in the conf directory");
                    Console.WriteLine("\tList exports test information to the screen");
                    Console.WriteLine("\tExport (if it was implimented) would write tests to an xml file.");
                    Console.WriteLine("\ttests: can be either a name of a group of tests, test numbers, guids, or a class name.");
                    Console.WriteLine("\ttestplan: plain text file in the plans directory that contains the list of tests to run.");
                    return 0;
                }

                TestLoader test_loader = new DirectoryBasedAssemblyTestLoader();
                TestCatalog test_catalog = test_loader.loadAllTests();

                ITestPlan loader = null;
                if (options.TestPlanName != String.Empty)
                {
                    loader = new SimpleFileTestPlan(options.TestPlanName);
                }
                else
                {
                    loader = new ParameterTestPlan(cmdLineParser.Parameters);
                }

                if (options.Mode == TEST_MODE.RUN)
                {
                    ITestRun runner = injector.Get<ITestRun>();
                    if (runner.run(loader, test_catalog) == 0)
                    {
                        Console.WriteLine("No Tests Found.");
                    }
                    foreach (IShutdownHook hook in ShutdownHooks.All)
                    {
                        try
                        {
                            hook.onShutdown();
                        }
                        catch (Exception ex)
                        {
                            ILog logger = LogManager.GetLogger("main");
                            logger.Error("Shutdown hook caused exception.", ex);
                        }
                    }
                }
                else if (options.Mode == TEST_MODE.LIST)
                {
                    List<TestInformation> tests = loader.loadTestPlan(test_catalog);
                    if(options.TestPlanName == String.Empty && cmdLineParser.Parameters.Length == 0)
                    {
                        Console.WriteLine("Getting info for all tests...");
                        tests = test_catalog.AllTests;
                    }

                    Console.WriteLine("{0,-38}{1,-9}{2,-15}{3}", new Object[] { "TCGuid", "TCNumber", "TCGroup", "TCName" });
                    foreach (TestInformation test in tests)
                    {
                        List<String> groups = new List<string>();
                        groups.AddRange(test.TestGroups);
                        if(groups.Count == 0)
                        {
                            groups.Add(String.Empty);
                        }
                        
                        String guid = String.Empty;
                        if (test.TestGuid != Guid.Empty)
                        {
                            guid = test.TestGuid.ToString();
                        }

                        String number = String.Empty;
                        if (test.TestNumber != -1)
                        {
                            number = test.TestNumber.ToString();
                        }

                        Console.WriteLine("{0,-38}{1,-9}{2,-15}{3}", new Object[] { guid, number, groups[0], test.TestName });
                        for (int i = 1; i < groups.Count; i++)
                        {
                            Console.WriteLine("{0,47}{1}", String.Empty, groups[i]);
                        }
                    }
                }
                else if (options.Mode == TEST_MODE.EXPORT)
                {
                    Console.WriteLine("Export not implimented yet.");
                }


            }
            else
            {
                Console.WriteLine("Unable to parse command line parameters...");
                return 1;
            }



            return 0;
        }
    }
}
