using System;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LottieUWP.Sample
{
    public sealed partial class MainPage
    {
        public ObservableCollection<string> Files { get; }

        public MainPage()
        {
            Files = new ObservableCollection<string>();
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var localizationDirectory = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync(@"Assets");
            var basePathLength = localizationDirectory.Path.Length - "Assets".Length;

            foreach (var file in await AssetUtils.GetJsonAssets())
            {
                Files.Add(file.Path.Substring(basePathLength, file.Path.Length - basePathLength));
            }
        }

        private async void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListView listView))
                return;

            var selectedItem = (string) listView.SelectedItem;
            if (selectedItem != null)
            {
                var assetsLength = "Assets\\".Length;
                var fileName = selectedItem.Substring(assetsLength, selectedItem.Length - assetsLength);
                fileName = fileName.Substring(0, fileName.Length - ".json".Length);

                LottieAnimationView.ImageAssetsFolder = $"Assets/Images/{fileName}";
                await LottieAnimationView.SetAnimationAsync(selectedItem);
                LottieAnimationView.PlayAnimation();
            }
        }

        private void RangeBase_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            LottieAnimationView.PauseAnimation();
            var slider = (Slider)sender;
            LottieAnimationView.Progress = (float) (e.NewValue / slider.Maximum);
        }

        private void Scale_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!double.IsNaN(e.NewValue))
                LottieAnimationView.Scale = (float)e.NewValue;
        }

        private void DebugButton_OnClick(object sender, RoutedEventArgs e)
        {
            var debugButton = (Button) sender;
            LottieLog.TraceEnabled = !LottieLog.TraceEnabled;
            if (LottieLog.TraceEnabled)
            {
                debugButton.Background = new SolidColorBrush(Colors.Red);
                debugButton.Content = "Debug On";
            }
            else
            {
                debugButton.Background = new SolidColorBrush(Colors.LightGray);
                debugButton.Content = "Debug Off";
            }
        }

        private void BackgroundColorButton_OnClick(object sender, RoutedEventArgs e)
        {
            var backgroundColorButton = (Button)sender;
            var solidColorBrush = (SolidColorBrush)LottieAnimationViewGrid.Background;
            if (solidColorBrush != null)
            {
                if (solidColorBrush.Color == Colors.White)
                {
                    backgroundColorButton.Content = "Background Black";
                    LottieAnimationViewGrid.Background = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    backgroundColorButton.Content = "Background White";
                    LottieAnimationViewGrid.Background = new SolidColorBrush(Colors.White);
                }
            }
        }

        private async void OpenFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".json");
            var file = await filePicker.PickSingleFileAsync();

            if (file != null)
            {
                await LottieAnimationView.SetAnimationAsync(new JsonReader(new StreamReader(await file.OpenStreamForReadAsync())));
                LottieAnimationView.PlayAnimation();
            }
        }
    }
}
