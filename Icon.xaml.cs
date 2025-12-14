using ImageMagick;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TrayTools
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Icon : Page
    {
        private ObservableCollection<ActionItem> _actions = new ObservableCollection<ActionItem>();
        public String id;

        public Icon()
        {
            InitializeComponent();
            ActionsList.ItemsSource = _actions;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            id = e.Parameter as string;
            SetupPage(id);

        }

        private void ContextMenuAddActionClickHandler(object sender, RoutedEventArgs e)
        {
            _actions.Add(new ActionItem { Name = "", Details = "", Type = 2 });

        }

        private void ContextMenuRemoveActionClickHandler(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ActionItem action)
            {
                _actions.Remove(action); // removes it from the collection
            }
        }

        public void showNotice(String title, String description, String buttonText)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = title;
            dialog.PrimaryButtonText = buttonText;
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = description;

            var result = dialog.ShowAsync();

        }

        private void SetPreview()
        {
            ImageConvertionStatusInfoBar.Visibility = Visibility.Collapsed;
            // Get type
            var type = (IconType)IconTypeCombo.SelectedIndex;
            switch (type)
            {
                case IconType.Image:
                    if(IconImagePath.Text.ToLower().StartsWith("https://") || IconImagePath.Text.ToLower().StartsWith("http://"))
                    {
                        ImageFailedInfoBar.Visibility = Visibility.Visible;
                        ImageFailedInfoBar.Message = "This image can't be used (download it to your computer first)!";
                        return;
                    } else
                    {
                        ImageFailedInfoBar.Message = "This image could not be loaded!";
                    }
                    if (IconImagePath.Text.ToLower().EndsWith(".ico"))
                    {
                        try
                        {
                            ImageRequiresConvertingInfoBar.Visibility = Visibility.Collapsed;
                            ImageIconPreview.Source = new BitmapImage(new Uri(IconImagePath.Text));
                            ImageFailedInfoBar.Visibility = Visibility.Collapsed;
                        }
                        catch (Exception)
                        {
                            ImageFailedInfoBar.Visibility = Visibility.Visible;
                        }


                    }
                    else if (IconImagePath.Text.ToLower().EndsWith(".png") || IconImagePath.Text.ToLower().EndsWith(".jpg") || IconImagePath.Text.ToLower().EndsWith(".jpeg"))
                    {
                        try
                        {
                            ImageIconPreview.Source = new BitmapImage(new Uri(IconImagePath.Text));
                            ImageFailedInfoBar.Visibility = Visibility.Collapsed;
                            ImageRequiresConvertingInfoBar.Visibility = Visibility.Visible;
                        }
                        catch (Exception)
                        {
                            ImageFailedInfoBar.Visibility = Visibility.Visible;
                            ImageRequiresConvertingInfoBar.Visibility = Visibility.Collapsed;
                        }
                        return;
                    }
                    else
                    {
                        ImageRequiresConvertingInfoBar.Visibility = Visibility.Collapsed;
                        ImageFailedInfoBar.Visibility = Visibility.Visible;
                        return;
                    }

                        break;
                case IconType.Text:
                    ImageRequiresConvertingInfoBar.Visibility = Visibility.Collapsed;
                    ImageFailedInfoBar.Visibility = Visibility.Collapsed;
                    TextIconPreview.Text = IconImagePath.Text;
                    break;
                default:
                    ImageRequiresConvertingInfoBar.Visibility = Visibility.Collapsed;
                    ImageFailedInfoBar.Visibility = Visibility.Collapsed;
                    break;
            }
        }


        private void IconImagePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetPreview();
        }

        private void ChangeTextBox(int Type)
        {
            switch (Type)
            {
                case 0:
                    // Is an image
                    IconImagePath.PlaceholderText = "Image Path";
                    IconImagePath.IsEnabled = true;
                    IconImagePath.MaxLength = 0;
                    BrowseImageButton.Visibility = Visibility.Visible;
                    ImageRecommendationText.Text = "Recommended Dimensions: 120x120 (Or any 1:1 image). .ICO format only (PNG/JPG/JPEG can be converted)";

                    // Change previews
                    ImageIconPreview.Visibility = Visibility.Visible;
                    TextIconPreview.Visibility = Visibility.Collapsed;
                    // Set text input as image
                    try
                    {
                        ImageIconPreview.Source = new BitmapImage(new Uri(IconImagePath.Text));
                    } catch(Exception)
                    {
                        ImageFailedInfoBar.Visibility = Visibility.Visible;
                    }
                    break;
                case 1:
                    // Is text
                    IconImagePath.PlaceholderText = "Icon text";
                    IconImagePath.IsEnabled = true;
                    IconImagePath.MaxLength = 2;
                    BrowseImageButton.Visibility = Visibility.Collapsed;
                    ImageRecommendationText.Text = "Max 2 characters. Recommended: 1x Emoji or 2x Letters.";

                    // Change previews
                    ImageIconPreview.Visibility = Visibility.Collapsed;
                    TextIconPreview.Visibility = Visibility.Visible;
                    ImageFailedInfoBar.Visibility = Visibility.Collapsed; // Hide in case its already showing
                    break;
                case 2:
                    // Is separator
                    IconImagePath.PlaceholderText = "";
                    IconImagePath.IsEnabled = false;
                    IconImagePath.MaxLength = 0;
                    BrowseImageButton.Visibility = Visibility.Collapsed;
                    ImageRecommendationText.Text = "To set text/image, change the icon type to something other than 'Invisible' ";

                    // And change previews again
                    ImageIconPreview.Visibility = Visibility.Collapsed;
                    TextIconPreview.Visibility = Visibility.Visible;
                    ImageFailedInfoBar.Visibility = Visibility.Collapsed; // Hide in case its already showing
                    break;
                default:
                    break;
            }
        }

        private void IconTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeTextBox(IconTypeCombo.SelectedIndex);
        }

        public async void SetupPage(String Id)
        {
            var icon = (await ((App)App.Current).GetIconData()).Find((v) => v.Id == Id);
            if (icon == null)
            {
                showNotice("Whoops!", "That icon couldn't be found - The file may have corrupted. Try adding a new icon instead.", "OK");
            } else
            {
                IconImagePath.Text = icon.IconLocation;
                IconName.Text = icon.Name;
                PageTitle.Text = icon.Name;
                IconName.Text = icon.Name;
                var typeInt = (int)icon.Type;
                IconTypeCombo.SelectedIndex = typeInt;
                ChangeTextBox(typeInt);
                LeftClickCombo.SelectedIndex = (int)icon.LeftClickAction.Type;
                LeftClickActionDetails.Text = icon.LeftClickAction.ActionDetails;
                if (icon.ContextMenuActions != null)
                {
                    for (var i = 0; i < icon.ContextMenuActions.Count; i++)
                    {
                        _actions.Add(new ActionItem
                        {
                            Name = icon.ContextMenuActions[i].Name,
                            Details = icon.ContextMenuActions[i].ActionDetails,
                            Type = (int)icon.ContextMenuActions[i].Type
                        });
                    }
                }
            }

        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            ImageFailedInfoBar.Visibility = Visibility.Collapsed;
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ImageFailedInfoBar.Message = "This image could not be loaded: " + e.ErrorMessage;
            ImageFailedInfoBar.Visibility = Visibility.Visible;
        }

        private async void GetFileFromPicker(List<string> types)
        {
            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // See the sample code below for how to make the window accessible from the App class.
            var window = ((App)App.Current)._window;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            types.ForEach((type) => {
                openPicker.FileTypeFilter.Add(type);
            });

            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();

            if(file != null)
            {
                // If the file is not an ICO, we need to convert it to one!
                if(file.FileType == ".png" || file.FileType == ".jpg")
                {
                    IconImagePath.Text = file.Path;
                    //Debug.WriteLine("Converting from " + file.FileType);
                    //ConvertToIco(file.Path, System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName() + ".ico"), 120);
                } else if(file.FileType == ".ico" )
                {
                    IconImagePath.Text = file.Path;
                } else
                {
                    ImageFailedInfoBar.Message = "That file type is not supported.";
                    ImageFailedInfoBar.Visibility = Visibility.Visible;
                }
            }
        }

        private void BrowseImageButton_Click(object sender, RoutedEventArgs e)
        {
            GetFileFromPicker(new List<string> { ".png", ".jpg", ".ico"});
        }

        private async void DeleteIcon_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Delete icon?";
            dialog.PrimaryButtonText = "Delete Icon";
            dialog.SecondaryButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;

            var bst = new Style(typeof(Button));
            bst.Setters.Add(new Setter(Button.BackgroundProperty, Colors.Red));
            bst.Setters.Add(new Setter(Button.ForegroundProperty, Colors.White));
            dialog.PrimaryButtonStyle = bst;
            dialog.Content = "Are you sure you wish to delete this icon?";

            var result = await dialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                var app = ((App)App.Current);
                await app.SetIconData(id, null);
                app._window.OpenAbout();
                app._window.RefreshNav(await app.GetIconData());
                app.RefreshIcons(false);
            };
            
        }

        private async void SaveRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Error checking further up should disable the button, this is primarily to save + refresh

            // Prep context-menu actions
            var CMActions = new List<IconActionData>();
            foreach (var item in _actions)
            {
                CMActions.Add(new IconActionData
                {
                    Name = item.Name,
                    ActionDetails = item.Details,
                    Type = (IconActionType)item.Type
                });
            }

            // Create the icondata object
            var icon = new IconData
            {
                Id = id,
                Name = IconName.Text,
                Type = (IconType)IconTypeCombo.SelectedIndex,
                IconLocation = IconImagePath.Text,
                LeftClickAction = new IconActionData
                {
                    Type = (IconActionType)LeftClickCombo.SelectedIndex,
                    ActionDetails = LeftClickActionDetails.Text
                },
                ContextMenuActions = CMActions
            };

            // And replace it
            var app = ((App)App.Current);
            await app.SetIconData(id, icon);
            app.RefreshIcons(false);
            app._window.RefreshNav(await app.GetIconData());
        }

        private async void ConvertIcon_Click(object sender, RoutedEventArgs e)
        {
            // Set up infobar
            ImageRequiresConvertingInfoBar.Visibility = Visibility.Collapsed;
            ImageConvertionStatusInfoBar.Visibility = Visibility.Visible;
            ImageConvertionStatusInfoBar.Message = "Converting image to icon...";
            ImageConvertionStatusInfoBar.Severity = InfoBarSeverity.Informational;
            ImageConvertionStatusInfoBar.Content = new ProgressBar
            {
                IsIndeterminate = true,
                MaxWidth = 200
            };

            try
            {
                // Disable all other controls while it's converting (in case it takes a while)
                IconTypeCombo.IsEnabled = false;
                IconImagePath.IsEnabled = false;
                BrowseImageButton.IsEnabled = false;

                // Convert images with ImageMagick
                using var image = new MagickImage();
                await image.ReadAsync(IconImagePath.Text);
                image.Resize(120, 120);
                image.Format = MagickFormat.Ico;
                // Save to appdata folder
                var savePath = ((App)App.Current).GetPathToLocalFolder();
                // Grab the name of the file without path/extension
                var fileName = IconImagePath.Text.Split("/").Last().Split(".").First();

                await image.WriteAsync(Path.Combine(savePath, fileName + ".ico"));

                ImageConvertionStatusInfoBar.Message = "Image converted to icon. Saved at: " + Path.Combine(savePath, fileName + ".ico");
                ImageConvertionStatusInfoBar.ActionButton = new Button
                {
                    Content = "Apply to this icon"
                };
                ImageConvertionStatusInfoBar.ActionButton.Click += (s, args) =>
                {
                    IconImagePath.Text = Path.Combine(savePath, fileName + ".ico");
                    SetPreview();
                };
                ImageConvertionStatusInfoBar.Severity = InfoBarSeverity.Success;
                ConvertIcon.IsEnabled = true;
                ImageRequiresConvertingInfoBar.Content = null;
                // Disable all other controls while it's converting (in case it takes a while)
                IconTypeCombo.IsEnabled = true;
                IconImagePath.IsEnabled = true;
                BrowseImageButton.IsEnabled = true;
            } catch(Exception ex)
            {
                ImageRequiresConvertingInfoBar.Visibility = Visibility.Collapsed;
                ImageConvertionStatusInfoBar.Visibility = Visibility.Visible;
                ImageConvertionStatusInfoBar.Message = "Failed to convert image to icon: " + ex.Message;
                ImageConvertionStatusInfoBar.Severity = InfoBarSeverity.Error;
                ImageConvertionStatusInfoBar.Content = null;
                IconTypeCombo.IsEnabled = true;
                IconImagePath.IsEnabled = true;
                BrowseImageButton.IsEnabled = true;
            }
        }
    }

    public class ActionItem
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public string Details { get; set; }
    }


}
