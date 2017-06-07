using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;

            if (listView == null)
                return;

            LottieAnimationViewContainer.Children.RemoveAt(0);
            var lottieAnimationView = new LottieAnimationView
            {
                FileName = (string) listView.SelectedItem,
                AutoPlay = true,
                Loop = true,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            LottieAnimationViewContainer.Children.Add(lottieAnimationView);
        }
    }
}
