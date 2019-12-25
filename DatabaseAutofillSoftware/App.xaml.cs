using System;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using ViewModelInterfaces;
using ServicesInterface;
using ViewModel;
using Services;

namespace DatabaseAutofillSoftware
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow _mainWindow;
        private IServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            
            serviceCollection.AddTransient<IMainWindowVM, MainWindowVM>();
            serviceCollection.AddSingleton<IDatabaseService, MicrosoftAccess>();

            serviceCollection.AddTransient<MainWindow, MainWindow>();

            _serviceProvider = serviceCollection.BuildServiceProvider();

            _mainWindow = _serviceProvider.GetService<MainWindow>();
            _mainWindow.Show();
        }
    }
}
