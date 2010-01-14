using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Common.TCApi
{
    public interface IShutdownHook
    {
        void onShutdown();
    }

    public static class ShutdownHooks
    {
        private static List<IShutdownHook> hooks = new List<IShutdownHook>();

        public static List<IShutdownHook> All
        {
            get
            {
                // create a copy
                return new List<IShutdownHook>(hooks);
            }
        }

        public static void register(IShutdownHook hook)
        {
            if (hook != null)
            {
                hooks.Add(hook);
            }
        }
    }
}
