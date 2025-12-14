using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TrayTools
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RunAtStartup.Unchecked += (s, e) =>
            {
                ((App)App.Current).RemoveAppFromStartup();
            };
            RunAtStartup.Checked += (s, e) =>
            {
                ((App)App.Current).AddAppToStartup();
            };

            StartSilently.Unchecked += (s, e) =>
            {
                ((App)App.Current).SetSetting("silentOpen", false);
            };
            StartSilently.Checked += (s, e) =>
            {
                ((App)App.Current).SetSetting("silentOpen", true);
            };

            RunAtStartup.IsChecked = ((App)App.Current).GetSetting("runAtStartup");
            StartSilently.IsChecked = ((App)App.Current).GetSetting("silentOpen");
        }
    }
}
