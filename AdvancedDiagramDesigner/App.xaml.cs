using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Threading;

namespace DiagramDesigner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
            ServicePointManager.DefaultConnectionLimit = 12;
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;

            base.OnStartup(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var exceptionJson = JsonConvert.SerializeObject(e.Exception);
            MessageBox.Show($"{DiagramDesigner.Properties.Resources.UnhandledException}\r\n{exceptionJson}", "Unhanded exception.", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
        }

        private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject == null)
            {
                MessageBox.Show($"{DiagramDesigner.Properties.Resources.UnhandledException}\r\nException object == null","Unhanded exception.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var exceptionJson = JsonConvert.SerializeObject(e.ExceptionObject);
            MessageBox.Show($"{DiagramDesigner.Properties.Resources.UnhandledException}\r\n{exceptionJson}", "Unhanded exception.", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
