using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Common.TCApi
{
    /// <summary>
    /// Runtime options that describe certain options of how the test cases should be run.
    /// </summary>
    public interface IRuntimeOptions
    {
        /// <summary>
        /// Should any resources requested for validation be saved?
        /// </summary>
        bool SaveResources { get; }

        /// <summary>
        /// Enable Debug mode?  This may imply more than just logging.
        /// </summary>
        bool Debug { get; }

        /// <summary>
        /// The name of the environment being used.  This also defines which sets of configs
        /// to load for tests.
        /// </summary>
        String EnvironmentName { get; }

        /// <summary>
        /// The name of the test plan if any.
        /// </summary>
        String TestPlanName { get; }

        /// <summary>
        /// Post the results to a server.
        /// </summary>
        bool PostResults { get; }
    }
}
