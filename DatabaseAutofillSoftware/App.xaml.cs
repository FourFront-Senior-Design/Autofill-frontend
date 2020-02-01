using System;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using ViewModelInterfaces;
using ServicesInterface;
using ViewModel;
using Services;
using NLog;
using NLog.Targets;
using NLog.Conditions;

namespace DatabaseAutofillSoftware
{
    public partial class App : Application
    {
        private MainWindow _mainWindow;
        private IServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            
            serviceCollection.AddTransient<IMainWindowVM, MainWindowVM>();
            serviceCollection.AddSingleton<IDatabaseService, MicrosoftAccess>();
            serviceCollection.AddSingleton<IOutputReader, JsonOutputReader>();
            serviceCollection.AddTransient<IAutofillController, AutofillController>();

            serviceCollection.AddTransient<MainWindow, MainWindow>();

            _serviceProvider = serviceCollection.BuildServiceProvider();

            _mainWindow = _serviceProvider.GetService<MainWindow>();
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            _mainWindow.Show();
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        // not used currenlty using app.config
        private void ConfigureNLog()
        {
            // Log Levels
            // Trace - very detailed logs, which may include high - volume information such as protocol payloads.This log level is typically only enabled during development
            // Debug - debugging information, less detailed than trace, typically not enabled in production environment.
            // Info - information messages, which are normally enabled in production environment
            // Warn - warning messages, typically for non - critical issues, which can be recovered or which are temporary failures
            // Error - error messages - most of the time these are Exceptions
            // Fatal - very serious errors!

            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = $"logs/{DateTime.Now.ToShortDateString().Replace('/', '-')}.txt" };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            var consoleTarget = new ColoredConsoleTarget();

            var highlightRule = new ConsoleRowHighlightingRule();
            highlightRule.Condition = ConditionParser.ParseExpression("level == LogLevel.Error");
            highlightRule.ForegroundColor = ConsoleOutputColor.Red;
            consoleTarget.RowHighlightingRules.Add(highlightRule);

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);

            NLog.LogManager.Configuration = config;
        }
    }
}
