using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using log4net;
using log4net.Core;

namespace QA.Common.TCApi
{
    /// <summary>
    /// Test Case Logger, allows for audit logging, as well as logging files and screen shots.
    /// </summary>
    public interface ITestCaseLogger : ILog
    {
        /// <summary>
        /// Log an audit log message.
        /// </summary>
        /// <param name="message"></param>
        void Audit(Object message);

        /// <summary>
        /// Log an audit log message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Audit(Object message, Exception exception);

        /// <summary>
        /// Log an audit log message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void AuditFormat(String format, Object[] args);

        /// <summary>
        /// Log an audit log message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg1"></param>
        void AuditFormat(String format, Object arg1);

        /// <summary>
        /// Log an audit log message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void AuditFormat(String format, Object arg1, Object arg2);

        /// <summary>
        /// Log an audit log message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        void AuditFormat(String format, Object arg1, Object arg2, Object arg3);

        /// <summary>
        /// Log a file to the test case log (file must exist).
        /// </summary>
        /// <param name="location">path to the file (relative or absolute).</param>
        void logFile(String location);

        /// <summary>
        /// Log a file's contents to the test case log.
        /// </summary>
        /// <param name="name">The name of the file to store.</param>
        /// <param name="contents">The contents of the log file.</param>
        void logFile(String name, byte[] contents);

        /// <summary>
        /// Take a screen shot and save it to the log.
        /// </summary>
        /// <param name="name">The name of the screen shot (without extension).</param>
        void logScreenShot(String name);

        /// <summary>
        /// Take a screen shot and save it to the log (using automatic naming).
        /// </summary>
        void logScreenShot();
    }

    /// <summary>
    /// Default test case logger, logs to files.  This class adds the FILE and AUDIT log levels if needed.
    /// </summary>
    public class BasicTestCaseLogger : LogImpl, ITestCaseLogger
    {
        /// <summary>
        /// Log level that is used when a file is logged.  It is above Info by default.
        /// </summary>
        static public Level FILE = new Level(log4net.Core.Level.Info.Value + 1000, "FILE");

        /// <summary>
        /// AUDIT log level, indicating messages that detail a step or important info that happens during the test case.
        /// This should be used for test case logic, not automation code logic.
        /// </summary>
        static public Level AUDIT = new Level(FILE.Value + 1000, "AUDIT");

        private static int s_ssnumber = 1;

        /// <summary>
        /// Automatically incrimenting property used for automatic naming of screen shots.
        /// </summary>
        public static int ScreenShotNumber
        {
            get
            {
                lock (typeof(BasicTestCaseLogger))
                {
                    return s_ssnumber++;
                }
            }
        }

        public String OutputPath { get; set; }

        /// <summary>
        /// Create a new BasicTestCaseLogger from a logger implimentation.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public BasicTestCaseLogger(ILogger logger, String outputPath)
            : base(logger)
        {
            foreach (Level level in new Level[] { FILE, AUDIT })
            {
                if (!logger.Repository.LevelMap.AllLevels.Contains(level))
                {
                    logger.Repository.LevelMap.Add(level);
                }
            }
            OutputPath = outputPath;
        }

        /// <summary>
        /// Property checking to see if Audit level logging is enabled.
        /// </summary>
        public bool IsAuditEnabled
        {
            get { return Logger.IsEnabledFor(AUDIT); }
        }

        /// <summary>
        /// Property reporting if the File Level log is enabled.
        /// </summary>
        public bool IsFileEnabled
        {
            get { return Logger.IsEnabledFor(FILE); }
        }

        /// <summary>
        /// Log an audit level log message, should be used for test case logic.
        /// </summary>
        /// <param name="message"></param>
        public void Audit(Object message)
        {
            Audit(message, null);
        }

        /// <summary>
        /// Log an audit level log message, should be used for test case logic.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Audit(Object message, Exception exception)
        {
            if (IsAuditEnabled)
            {
                Logger.Log(typeof(BasicTestCaseLogger), AUDIT, message, exception);
            }
        }

        /// <summary>
        /// Log an audit level log message, should be used for test case logic.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void AuditFormat(String format, Object[] args)
        {
            Audit(new log4net.Util.SystemStringFormat(System.Globalization.CultureInfo.InvariantCulture, format, args), null);
        }

        /// <summary>
        /// Log an audit level log message, should be used for test case logic.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg1"></param>
        public void AuditFormat(String format, Object arg1)
        {
            Audit(new log4net.Util.SystemStringFormat(System.Globalization.CultureInfo.InvariantCulture, format, new Object[] { arg1 }), null);
        }

        /// <summary>
        /// Log an audit level log message, should be used for test case logic.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public void AuditFormat(String format, Object arg1, Object arg2)
        {
            Audit(new log4net.Util.SystemStringFormat(System.Globalization.CultureInfo.InvariantCulture, format, new Object[] { arg1, arg2 }), null);
        }

        /// <summary>
        /// Log an audit level log message, should be used for test case logic.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        public void AuditFormat(String format, Object arg1, Object arg2, Object arg3)
        {
            Audit(new log4net.Util.SystemStringFormat(System.Globalization.CultureInfo.InvariantCulture, format, new Object[] { arg1, arg2, arg3 }), null);
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="location"></param>
        public void logFile(String location)
        {
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        public void logFile(String name, byte[] contents)
        {
        }

        public Bitmap ScreenShot()
        {
            Rectangle totalSize = Rectangle.Empty;

            foreach (Screen s in Screen.AllScreens)
                totalSize = Rectangle.Union(totalSize, s.Bounds);

            Bitmap screenShotBMP = new Bitmap(totalSize.Width, totalSize.Height,
                PixelFormat.Format32bppArgb);

            Graphics screenShotGraphics = Graphics.FromImage(screenShotBMP);

            screenShotGraphics.CopyFromScreen(totalSize.X, totalSize.Y,
                0, 0, totalSize.Size, CopyPixelOperation.SourceCopy);

            screenShotGraphics.Dispose();

            return screenShotBMP;
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="name"></param>
        public void logScreenShot(String name)
        {
            String screenShotPathName = Path.Combine(OutputPath, name);
            using (Bitmap screenshot = ScreenShot())
            {
                screenshot.Save(screenShotPathName);
            }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public void logScreenShot()
        {
            logScreenShot("Screenshot" + ScreenShotNumber + ".png");
        }
    }
}
