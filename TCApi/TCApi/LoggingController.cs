using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ninject.Core;
using log4net;
using log4net.Core;

namespace QA.Common.TCApi
{
    /// <summary>
    /// A Logging controller is responsible for doing 2 things:
    /// <list type="number">
    ///     <item>Configuring logging globally.</item>
    ///     <item>Preparing instances of <see cref="ITestCaseLogger"/> for test cases.</item>
    /// </list>
    /// </summary>
    public interface ILoggingController
    {
        /// <summary>
        /// Configure logging using the provided level as the default minimum logging level.
        /// </summary>
        /// <param name="logLevel">Logging level to use as the default minimum level.</param>
        void configureLogging(Level logLevel);

        /// <summary>
        /// Get a test case logger from the 
        /// </summary>
        /// <param name="tc"></param>
        /// <returns></returns>
        ITestCaseLogger configureTestCaseLogger(ITestCase tc);

        /// <summary>
        /// Cleanup and close any files that the logger may have opened.
        /// </summary>
        /// <param name="tc"></param>
        void closeTestCaseLogger(ITestCase tc);
    }

    /// <summary>
    /// The default logging controller uses configuration files, logging to logs\runtime.log and logs\testcases\[class name].log.
    /// </summary>
    public class DefaultLoggingController : ILoggingController
    {
        private ITestOutput m_output_manager;

        /// <summary>
        /// Create the logging controller with the output manager as it's only dependency.
        /// </summary>
        /// <param name="outputManager">Gives the Logging controller the ability to use the correct paths for output.</param>
        [Inject]
        public DefaultLoggingController(ITestOutput outputManager)
        {
            m_output_manager = outputManager;
        }

        /// <summary>
        /// Configure the runtime log.
        /// </summary>
        /// <param name="logLevel">The default minimum level to use.  All logging events where the event's level is >= to logLevel will get logged.</param>
        public void configureLogging(Level logLevel)
        {
            log4net.Appender.FileAppender runtimeAppender = new log4net.Appender.FileAppender();
            String runtime_log_path = m_output_manager.getPath("RuntimeLog");
            if (runtime_log_path == null)
            {
                String session_path = m_output_manager.getPath("SessionOutput");
                if (session_path == null)
                {
                    session_path = Path.Combine(m_output_manager.OutputRoot, "last");
                    m_output_manager.setPath("SessionOutput", session_path);
                }

                FileUtility.backupDirectory(session_path);
                Directory.CreateDirectory(session_path);
                runtime_log_path = Path.Combine(session_path, "runtime.log");
                m_output_manager.setPath("RuntimeLog", runtime_log_path);
            }
            runtimeAppender.File = runtime_log_path;
            runtimeAppender.Layout = new log4net.Layout.PatternLayout("[%d{MMM dd yyyy hh:mm:ss tt},%level,%logger]: %m%n%exception");
            runtimeAppender.Threshold = logLevel;
            runtimeAppender.ActivateOptions();
            log4net.Config.BasicConfigurator.Configure(runtimeAppender);
        }

        /// <summary>
        /// Configure a test case logger, logging to logs\testcases\[classname].log.
        /// </summary>
        /// <param name="tc">The type of the test case, used to get the class name.</param>
        /// <returns>A TestCaseLogger (<see cref="BasicTestCaseLogger"/>).</returns>
        public ITestCaseLogger configureTestCaseLogger(ITestCase tc)
        {
            log4net.Appender.FileAppender fileAppender = new log4net.Appender.FileAppender();
            fileAppender.Layout = new log4net.Layout.PatternLayout("[%d{ddd, MMMM dd yyyy hh:mm:ss tt},%level,%property{tcstage}]: %m%n%exception");
            String test_output_path = m_output_manager.getTestCaseOutputPath(tc);
            if(test_output_path == null)
            {
                String test_output_base = Path.Combine(m_output_manager.getPath("SessionOutput"), tc.GetType().ToString());
                test_output_path = test_output_base;
                
                int i = 0;
                while (Directory.Exists(test_output_path))
                {
                    test_output_path = test_output_base + "-" + ++i;
                }
                Directory.CreateDirectory(test_output_path);
            }
            String output_log_file = Path.Combine(test_output_path, "default.log");
            fileAppender.Name = tc.GetType().ToString();
            fileAppender.File = output_log_file;
            fileAppender.AppendToFile = false;
            fileAppender.ActivateOptions();
            ILog logger = LogManager.GetLogger(tc.GetType().ToString());
            if (typeof(log4net.Repository.Hierarchy.Logger).IsAssignableFrom(logger.Logger.GetType()))
            {
                log4net.Repository.Hierarchy.Logger realLogger = (log4net.Repository.Hierarchy.Logger)logger.Logger;
                realLogger.AddAppender(fileAppender);
            }
            else
            {
                throw new Exception("Logger was not of expected type.");
            }
            return new BasicTestCaseLogger(logger.Logger, test_output_path);

        }

        public void closeTestCaseLogger(ITestCase tc)
        {
            ILog logger = LogManager.GetLogger(tc.GetType().ToString());
            if (typeof(log4net.Repository.Hierarchy.Logger).IsAssignableFrom(logger.Logger.GetType()))
            {
                log4net.Repository.Hierarchy.Logger realLogger = (log4net.Repository.Hierarchy.Logger)logger.Logger;
                log4net.Appender.IAppender appender = realLogger.GetAppender(tc.GetType().ToString());
                appender.Close();
                realLogger.RemoveAppender(appender);
            }
        }
    }
}
