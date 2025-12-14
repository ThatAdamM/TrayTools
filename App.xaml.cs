using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Text;
using IWshRuntimeLibrary;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TrayTools
{
    /// <summary>
    /// How the icon should be displayed. If a Separator, the icon can be left as an empty image.
    /// The Text option should also support displaying emoji.
    /// Types: Separator, Text, Image
    /// </summary>
    public enum IconType
    {
        Image,
        Text,
        Separator
    }

    /// <summary>
    /// The type of action to perform with the ActionDetails. Nothing may also be used as a Separator in Context Menus.
    /// Types: Nothing, OpenFile, OpenURL
    /// </summary>
    public enum IconActionType
    {
        Nothing,
        OpenFile,
        OpenURL
    }

    /// <summary>
    /// An action for this icon
    /// </summary>
    public class IconActionData
    {
        public String? Name { get; set; }
        public IconActionType Type { get; set; }
        public String ActionDetails { get; set; }
    }

    /// <summary>
    /// An instance of an Icon
    /// </summary>
    public class IconData
    {
        public String IconLocation { get; set; }
        public String Id { get; set; } = System.Guid.NewGuid().ToString();
        public IconType Type { get; set; }
        public String Name { get; set; }
        public IList<IconActionData> ContextMenuActions { get; set; }
        public IconActionData LeftClickAction { get; set; }
    }

    /// <summary>
    /// Manages Icons on the System Tray
    /// </summary>
    public class TrayIconManager
    {
        public List<System.Windows.Forms.NotifyIcon> TrayIcons = new();

        public async Task<System.Windows.Forms.NotifyIcon> CreateTrayIcon(IconData icon)
        {
            var source = SystemIcons.Error;
            var sourceImage = "";
            if (icon.Type.Equals(IconType.Image))
            {
                source = new System.Drawing.Icon(icon.IconLocation);
            } else if (icon.Type.Equals(IconType.Text))
            {
                Debug.WriteLine("Creating text");
                using var bitmap = new Bitmap(120, 120);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.Clear(System.Drawing.Color.Transparent);
                using var font = new Font("Segoe UI Emoji", 90, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                graphics.DrawString(icon.IconLocation, font, Brushes.White, new PointF(-20, -10));
                source = System.Drawing.Icon.FromHandle(bitmap.GetHicon());
            } else if (icon.Type.Equals(IconType.Separator))
            {
                using var bitmap = new Bitmap(120, 120);
                source = System.Drawing.Icon.FromHandle(bitmap.GetHicon());
            }

            var trayIcon = new System.Windows.Forms.NotifyIcon();

            
            try
            {
                trayIcon = new System.Windows.Forms.NotifyIcon
                {
                    Icon = source,
                    Text = icon.Name,
                    Visible=true
                };
                trayIcon.MouseClick += (object sender, MouseEventArgs e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        switch (icon.LeftClickAction.Type)
                        {
                            case IconActionType.Nothing:
                                break;

                            case IconActionType.OpenURL:
                                RelayCommand.RunAction(icon.LeftClickAction.ActionDetails);
                                break;

                            case IconActionType.OpenFile:
                                RelayCommand.RunAction(icon.LeftClickAction.ActionDetails);
                                break;
                        }
                    }
                };

            } catch(Exception e)
            {
                Debug.WriteLine("Failed to create icon", e);
            }
            var menu = new System.Windows.Forms.ContextMenuStrip();
            for (var i = 0; i < (icon.ContextMenuActions ?? new List<IconActionData>()).Count; i++) {
                if(icon.ContextMenuActions[i].Type == IconActionType.Nothing)
                {
                    // Create a separator
                    menu.Items.Add(new ToolStripSeparator());
                } else
                {
                    // Create a menu item
                    var action = icon.ContextMenuActions[i];
                    menu.Items.Add(action.Name, null, (not, needed) =>
                    {
                        RelayCommand.RunAction(action.ActionDetails);

                    });
                }
            }
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Settings...", null, (not, needed) => {
                ((App)App.Current).OpenSettingsWindow();
            });
            menu.Items.Add("Stop TrayTools", null, (not, needed) => {
                Environment.Exit(0);
            });

            trayIcon.ContextMenuStrip = menu;
            try
            {
                TrayIcons.Add(trayIcon);
            } catch(Exception e)
            {
                Debug.WriteLine("Icon not added to list", e);
            }
            return trayIcon;
        }

        public void RemoveAllTrayIcons()
        {
            foreach (var icon in TrayIcons)
            {
                icon.Dispose();
            }
            TrayIcons.Clear();
        }
    }

    // Robbed this from the AI since idk what commands are ¯\_(ツ)_/¯
    public class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Action<object?> execute;
        private readonly Func<object?, bool>? canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => execute(parameter);

        // Adding my own lil function to run actions since I like it that way
        /// <summary>
        /// Runs an action in the shell
        /// </summary>
        public static void RunAction(String path)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true // <-- tells Windows to use default handler
                };
                Process.Start(psi);
            } catch(Exception e)
            {
                Debug.WriteLine(e);
                new ToastContentBuilder()
                    .AddText("TrayTools Action Error")
                    .AddText($"An error occurred while running your action - {e.Message}")
                    .Show();
            }
        }
    }

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Microsoft.UI.Xaml.Application
    {
        public MainWindow? _window;
        public Boolean _windowIsOpen = false;
        private TrayIconManager _trayManager = new TrayIconManager();


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            bool createdNew;
            var instanceMutex = new Mutex(true, "TrayTools_AppMutex", out createdNew);
            if (!createdNew)
            {
                // Another instance is already running
                Environment.Exit(0);
            }

            InitializeComponent();
        }

        public void logIconNumbers()
        {
            Debug.WriteLine("Number of icons:");
            Debug.WriteLine(_trayManager.TrayIcons.Count);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // If we don't want to open silently, get that window open!
            if(ApplicationData.Current.LocalSettings.Values["silentOpen"] == null) {
                ApplicationData.Current.LocalSettings.Values["silentOpen"] = false;
            }
            var silentOpen = (bool)(ApplicationData.Current.LocalSettings.Values["silentOpen"]);
            if (!silentOpen) {
                OpenSettingsWindow();
            }
            RefreshIcons(true);
        }

        public void SetSetting(String name, bool value)
        {
            ApplicationData.Current.LocalSettings.Values[name] = value;
        }
        public bool GetSetting(String name)
        {
            try
            {
                return (bool)(ApplicationData.Current.LocalSettings.Values[name]);
            } catch(Exception e)
            {
                // If not already set, return false
                ApplicationData.Current.LocalSettings.Values[name] = false;
                return false;
            }
        }

        public void OpenSettingsWindow()
        {
            if (_windowIsOpen)
            {
                // If the window is already open, just activate it
                _window.Activate();
                return;
            }
            else
            {
                _window = new MainWindow();
                _window.Activated += (notused, lol) => { _windowIsOpen = true; };
                _window.Closed += (notused, lol) => { _windowIsOpen = false;  };
                _window.Activate();
            }
        }

        // Gets the icon data from saved file
        public async Task<List<IconData>> GetIconData()
        {
            // Load icons from the settings JSON file
            var loadedFile = null as StorageFile;
            try
            {
                loadedFile = await ApplicationData.Current.LocalFolder.GetFileAsync("data.json");
            } catch (FileNotFoundException)
            {
                // File not found, will do first-time setup
                await FirstTimeSetup();
                // And then get the file
                loadedFile = await ApplicationData.Current.LocalFolder.GetFileAsync("data.json");
            }
            var json = await FileIO.ReadTextAsync(loadedFile);
            var icons = new List<IconData>(JsonSerializer.Deserialize<List<IconData>>(json) ?? new List<IconData>());
            Debug.WriteLine(ApplicationData.Current.LocalFolder.Path);
            return icons;
        }

        // And save it using the IconData class list format
        public async Task SaveIconDataToFile(List<IconData> data)
        {
            var json = JsonSerializer.Serialize(data);
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("data.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, json);
            return;
        }

        public String GetPathToLocalFolder()
        {
            return ApplicationData.Current.LocalFolder.Path;
        }

        // Make changes to an individual icon and change it

        /// <summary>
        /// Make changes to an individual icon and save it
        /// </summary>
        /// <param name="id">The GUID of this icon</param>
        /// <param name="data">The icon data to save</param>
        /// <returns>The vast nothingness of a void</returns>
        public async Task SetIconData(String id, IconData data)
        {
            // If no id or data, ignore
            if (id == null && data == null)
            {
                return;
            // If just no id, create a new icon in the file with this data
            } else if(id == null)
            {
                // Get full icon data
                var icons = await GetIconData();
                // Add new data and save
                icons.Add(data);
                await SaveIconDataToFile(icons);
                return;
            // If an id but no data, remove the item.
            } else if(data == null)
            {
                // Get full icon data
                var icons = await GetIconData();
                // Remove this icon from the list by its index
                var index = icons.FindIndex((v) => v.Id == id); 
                icons.RemoveAt(index);
                // And save
                await SaveIconDataToFile(icons);
                return;
            // If both, overwrite the old icon at that position
            } else
            {
                // Get full icon data
                var icons = await GetIconData();
                // Change the icon at this index
                var index = icons.FindIndex((v) => v.Id == id);
                icons[index] = data;
                // And save
                await SaveIconDataToFile(icons);
                return;
            }
        }


        // Will refresh the Tray Icons, firing them up if not already on
        public async void RefreshIcons(Boolean noneActive)
        {
            if(!noneActive) _trayManager.RemoveAllTrayIcons();

            var icons = await GetIconData();

            foreach (var icon in icons)
            {
                try
                {
                    var trayIcon = _trayManager.CreateTrayIcon(icon);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            Debug.WriteLine(_trayManager.TrayIcons.Count);
        }

        public async Task FirstTimeSetup() {   
            
            var firstIconData = new IconData
            {
                IconLocation = "Assets/logo.ico",
                Type = IconType.Image,
                Name = "Icon 1",
                ContextMenuActions = new List<IconActionData> {},
                LeftClickAction = new IconActionData { Name = "", Type = IconActionType.Nothing, ActionDetails = "" }
            };
            // Create the data.json file with default values
            var json = JsonSerializer.Serialize(new List<IconData>());
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("data.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, json);
            return;
        }

        // Create shortcut to app
        public void AddAppToStartup()
        {
            SetSetting("runAtStartup", true);
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = System.IO.Path.Combine(startupFolder, "TrayTools.lnk");

            var shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = Environment.ProcessPath;
            shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(Environment.ProcessPath);
            shortcut.Save();
        }

        // And remove it!
        public void RemoveAppFromStartup()
        {
            SetSetting("runAtStartup", false);
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = System.IO.Path.Combine(startupFolder, "TrayTools.lnk");
            if (System.IO.File.Exists(shortcutPath))
            {
                System.IO.File.Delete(shortcutPath);
            }
        }
    }
}