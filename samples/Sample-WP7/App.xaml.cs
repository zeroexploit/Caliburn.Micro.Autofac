using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Sample_WP7
{
  public partial class App : Application
  {
    /// <summary>
    /// Constructor for the Application object.
    /// </summary>
    public App()
    {
      // Global handler for uncaught exceptions. 
      UnhandledException += Application_UnhandledException;

      // Standard Silverlight initialization
      InitializeComponent();
    }

    // Code to execute on Unhandled Exceptions
    private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
    {
      if (System.Diagnostics.Debugger.IsAttached)
      {
        // An unhandled exception has occurred; break into the debugger
        System.Diagnostics.Debugger.Break();
      }
    }
  }
}