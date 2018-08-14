using Windows.UI.Xaml;

namespace LottieUWP.Sample
{
    public sealed partial class InputDialog
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            private set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(InputDialog), new PropertyMetadata(""));

        public InputDialog()
        {
            InitializeComponent();
        }
    }
}
