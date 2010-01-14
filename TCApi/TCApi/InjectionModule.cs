using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Ninject.Core;
using Ninject.Core.Binding;
using Ninject.Core.Behavior;
using Ninject.Conditions;

namespace QA.Common.TCApi
{
    public class SaveOption : Attribute {}

    public static class ModuleFactory
    {
        public static IModule getInjectionModule(IRuntimeOptions options)
        {
            if (options.PostResults)
                return new SlickCentralInjectionModule(options);
            else
                return new StarndardInjectionModule(options);
                
        }
    }

    /// <summary>
    /// Default injection module (configuration for Ninject).
    /// </summary>
    /// <remarks>
    /// This class uses the <see cref="DefaultLoggingController"/> and <see cref="INIConfigLoader"/>.  This means
    /// it will use log files instead of the new database model.
    /// </remarks>
    public class StarndardInjectionModule : StandardModule
    {
        private IRuntimeOptions m_options;
        private TCApiConfiguration m_configuration;

        public StarndardInjectionModule(IRuntimeOptions options)
        {
            m_options = options;
            DefaultPaths.ConfigName = m_options.EnvironmentName + ".ini";
            if (!File.Exists(DefaultPaths.ConfigFile))
            {
                throw new FileNotFoundException("Configuration file [" + DefaultPaths.ConfigFile + "] not found.", DefaultPaths.ConfigFile);
            }
            IConfigLoader loader = new INIConfigLoader();
            m_configuration = loader.getFlattenedConfiguration(DefaultPaths.ConfigFile);
        }

        /// <summary>
        /// Used by Ninject to configure the IoC injector.
        /// </summary>
        public override void Load()
        {
            Bind<IRuntimeOptions>().ToConstant<IRuntimeOptions>(m_options);
            Bind<ILoggingController>().To<DefaultLoggingController>();
            Bind<IConfigLoader>().To<INIConfigLoader>();
            Bind<TCApiConfiguration>().ToConstant<TCApiConfiguration>(m_configuration);
            Bind<IResourceFactory>().To<FileResourceFactory>().Using<SingletonBehavior>();
            Bind<String>().ToConstant<String>(DefaultPaths.Root).Only(When.Context.Target.HasAttribute<ExecutableBasePath>());
            Bind<Boolean>().ToConstant<Boolean>(m_options.SaveResources).Only(When.Context.Target.HasAttribute<SaveOption>());
            Bind<ITestOutput>().To<DefaultTestOutputManager>().Using<SingletonBehavior>();
            BindTestRun();
        }

        public virtual void BindTestRun()
        {
            Bind<ITestRun>().To<BasicTestRun>();
        }
    }

    public class SlickCentralInjectionModule : StarndardInjectionModule
    {
        public SlickCentralInjectionModule(IRuntimeOptions options)
            : base(options)
        {
        }

        public override void BindTestRun()
        {
            Bind<ITestRun>().To<SlickCentralTestRun>();
        }

    }
}
