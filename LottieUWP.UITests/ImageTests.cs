using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;
using Xunit;

namespace LottieUWP.UITests
{
    public class ImageTests
    {
        private readonly WindowsDriver<WindowsElement> _session;

        public ImageTests()
        {
            DesiredCapabilities appCapabilities = new DesiredCapabilities();
            appCapabilities.SetCapability("app", "a291d3de-5b28-4950-902b-cb87a02a64c6_gspb8g6x97k2t!App");
            _session = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), appCapabilities);
        }

        [Theory]
        [InlineData(@"Assets\Logo\LogoSmall.json", true)]
        [InlineData(@"Assets\lottiefiles\___.json", false)]
        public async Task TestUI(string fileName, bool black)
        {
            var element = _session.FindElementByName(fileName);
            element.Click();

            SelectBackgroundColor(black);

            await SaveTileBitmap(fileName.Replace(".json", ".png"));
        }

        private void SelectBackgroundColor(bool black)
        {
            var blackWhiteButton = _session.FindElementByAccessibilityId("ChangeBackgroundButton");
            var buttonState = blackWhiteButton.Text;
            if ((buttonState == "Background White" && black) ||
                (buttonState == "Background Black" && !black))
            {
                blackWhiteButton.Click();
            }
        }

        private async Task SaveTileBitmap(string fileName)
        {
            var element = _session.FindElementByAccessibilityId("LottieAnimationView");
            var progressSlider = _session.FindElementByAccessibilityId("ProgressSlider");

            var frames = 11;

            var height = element.Size.Height;
            using (var bitmap = new Bitmap(element.Size.Width, height * frames))
            {
                using (var canvas = Graphics.FromImage(bitmap))
                {
                    progressSlider.Click();
                    for (int i = 0; i <= frames; i++)
                    {
                        _session.Mouse.MouseMove(progressSlider.Coordinates,
                            (int)(progressSlider.Size.Width * (i / (float)frames)) - 4, progressSlider.Size.Height / 2);
                        _session.Mouse.Click(null);

                        await Task.Delay(250);

                        element = _session.FindElementByAccessibilityId("LottieAnimationView");

                        var screenshot = element.GetScreenshot();
                        using (var image = new MemoryStream(screenshot.AsByteArray))
                        {
                            canvas.DrawImage(new Bitmap(image), 0, height * i);
                        }
                    }

                    canvas.Save();
                }

                var directoryName = Path.GetDirectoryName(fileName);
                if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                bitmap.Save(fileName, ImageFormat.Png);
            }
        }
    }
}
