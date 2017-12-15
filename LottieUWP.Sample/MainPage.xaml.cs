using System;
using System.Collections.ObjectModel;
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

            await LottieAnimationView.SetAnimationAsync((string)listView.SelectedItem);
            LottieAnimationView.PlayAnimation();
        }

        private void RangeBase_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            LottieAnimationView.PauseAnimation();
            LottieAnimationView.Progress = (float) (e.NewValue / 1000);
        }

        private void Scale_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!double.IsNaN(e.NewValue))
                LottieAnimationView.Scale = (float)e.NewValue;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            LottieLog.TraceEnabled = !LottieLog.TraceEnabled;
            if (LottieLog.TraceEnabled)
            {
                DebugButton.Background = new SolidColorBrush(Colors.Red);
                DebugButton.Content = "Debug On";
            }
            else
            {
                DebugButton.Background = new SolidColorBrush(Colors.LightGray);
                DebugButton.Content = "Debug Off";
            }
        }
    }
}
