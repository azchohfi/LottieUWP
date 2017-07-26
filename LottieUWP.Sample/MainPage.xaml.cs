using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace LottieUWP.Sample
{
    public sealed partial class MainPage
    {
        public ObservableCollection<string> Files { get; }

        public MainPage()
        {
            Files = new ObservableCollection<string>();
            LottieLog.TraceEnabled = true;
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
            var listView = sender as ListView;

            if (listView == null)
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
    }
}
