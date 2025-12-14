using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.Notifications;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TrayTools
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    
    public sealed partial class MainWindow : Window
    {
        public Page CurrentPage;
        public App app = ((App) Application.Current);
        public MainWindow()
        {
            InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            this.AppWindow.SetIcon("Assets/logo.ico");
            ContentFrame.Navigate(typeof(About));

            // Add min window size
            OverlappedPresenter presenter = OverlappedPresenter.Create();
            presenter.PreferredMinimumWidth = 1100;
            presenter.PreferredMinimumHeight = 550;
            this.AppWindow.SetPresenter(presenter);

            RefreshNavStartup();
        }

        private async void RefreshNavStartup()
        {
            var icons = await ((App)App.Current).GetIconData();
            RefreshNav(icons);
        }

        private void NavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag)
                {
                    case "about":
                        ContentFrame.Navigate(typeof(About));
                        break;
                    case "settings":
                        ContentFrame.Navigate(typeof(Settings));
                        break;
                }

                if(item.Tag.ToString().StartsWith("icon::"))
                {
                    try
                    {
                        ContentFrame.Navigate(typeof(Icon), item.Tag.ToString().Substring(6));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        private Boolean isQuitting = false;

        public async Task CloseAppButton()
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = ContentFrame.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Keep open in background?";
            dialog.PrimaryButtonText = "Keep Open";
            dialog.SecondaryButtonText = "Quit";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = "Closing the background process will stop all tray icons from showing";

            var result = await dialog.ShowAsync();

            if (result.Equals(ContentDialogResult.Secondary))
            {
                // Quit whole process
                isQuitting = true;
                Application.Current.Exit();
            }
            else if (result.Equals(ContentDialogResult.Primary))
            {
                // Just close this window
                this.AppWindow.Destroy();
                app._windowIsOpen = false;
            }

            // Otherwise, do nothing and just close the dialog
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem is NavigationViewItem item)
            {
                if(item.Tag.ToString() == "close")
                {
                    
                    CloseAppButton();
                }
            }
        }

        private void CloseAppButtonNavHandler(object sender, TappedRoutedEventArgs e)
        {
            CloseAppButton();
        }

        public void RefreshNav(List<IconData> Icons)
        {
            // Remove all icons first
            for (int i = NavView.MenuItems.Count - 1; i >= 0; i--)
            {
                if (NavView.MenuItems[i] is NavigationViewItem nvi &&
                    nvi.Tag is string tag &&
                    tag.StartsWith("icon"))
                {
                    // Do something
                    NavView.MenuItems.RemoveAt(i);
                }
            }

            Debug.WriteLine("Refreshing Nav with " + Icons.Count + " icons");

            // Re-order as required
            foreach (var icon in Icons)
            {
                // If an image or invisible...
                // (If invisible we'll just show the default TrayTools logo)
                if (icon.Type == IconType.Image || icon.Type == IconType.Separator)
                {
                    var image = null as BitmapImage;
                    try
                    {
                        // Try first with icon. If not, use default
                        image = new BitmapImage(new Uri(icon.IconLocation, UriKind.RelativeOrAbsolute));
                    }
                    catch (Exception e)
                    {
                        // Fallback to TrayTools logo
                        image = new BitmapImage(new Uri("ms-appx:///Assets/logo.png", UriKind.RelativeOrAbsolute));
                    }
                    try
                    {
                        NavView.MenuItems.Add(
                            new NavigationViewItem
                            {
                                Icon = new ImageIcon
                                {
                                    Source = image
                                },
                                Content = icon.Name,
                                Tag = "icon::" + icon.Id
                            }
                        );
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Something bad happened here", e);
                    }
                    } else
                {
                    // Show as text
                    NavView.MenuItems.Add(
                        new NavigationViewItem
                        {
                            Content = icon.IconLocation + "   " + icon.Name,
                            Tag = "icon::" + icon.Id
                        }
                    );
                }
                    
            }
        }

        // No idea if I'll use this, but will be useful for dynamic updates
        public void RefreshSingleNavItem(String changeType, String id, String content)
        {
            for (int i = NavView.MenuItems.Count - 1; i >= 0; i--)
            {
                if(NavView.MenuItems[i] is NavigationViewItem nvi &&
                    nvi.Tag is string tag &&
                    tag.StartsWith("icon") &&
                    tag.Substring(6) == id)
                {
                    // Make changes based on the correct changeType
                    switch (changeType)
                    {
                        case "name":
                            nvi.Content = content;
                            break;

                        case "icon":
                            nvi.Icon = new ImageIcon { Source = new BitmapImage(new Uri(content, UriKind.RelativeOrAbsolute)) };
                            break;

                        default:
                            break;
                    }

                }
            }
        }

        public async void CreateIcon(object sender, TappedRoutedEventArgs e)
        {
            var icon = new IconData { Name = "New Icon", IconLocation = "ms-appx:///Assets/logo.png", Type = IconType.Separator, LeftClickAction = new IconActionData { Type = IconActionType.Nothing, ActionDetails = "" } };
            await app.SetIconData(null, icon);
            RefreshNav(await app.GetIconData());
            return;
        }

        public async void RefreshIcons(object sender, TappedRoutedEventArgs e)
        {

            var icons = await app.GetIconData();
            try
            {
                RefreshNav(icons);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Caught at RefreshNav", ex);
            }

            app.RefreshIcons(false);
        }

        public void OpenAbout()
        {
            ContentFrame.Navigate(typeof(About));
        }

        private void Window_Closed(object sender, WindowEventArgs e)
        {
            if (isQuitting) return;
            e.Handled = true;

            CloseAppButton();
        }
    }

}
