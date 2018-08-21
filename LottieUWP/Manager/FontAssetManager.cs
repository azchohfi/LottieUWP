using System;
using System.Collections.Generic;
using Windows.UI.Text;

namespace LottieUWP.Manager
{
    internal class FontAssetManager
    {
        private Tuple<string, string> _tempPair;

        /// <summary>
        /// Pair is (fontName, fontStyle) </summary>
        private readonly Dictionary<Tuple<string, string>, Typeface> _fontMap = new Dictionary<Tuple<string, string>, Typeface>();
        /// <summary>
        /// Map of font families to their fonts. Necessary to create a font with a different style </summary>
        private readonly Dictionary<string, Typeface> _fontFamilies = new Dictionary<string, Typeface>();
        private FontAssetDelegate _delegate;
        private string _defaultFontFileExtension = ".ttf";

        internal FontAssetManager(FontAssetDelegate @delegate)
        {
            _delegate = @delegate;
        }

        internal FontAssetDelegate Delegate
        {
            set => _delegate = value;
        }

        /// <summary>
        /// Sets the default file extension (include the `.`).
        /// 
        /// e.g. `.ttf` `.otf`
        /// 
        /// Defaults to `.ttf`
        /// </summary>
        public string DefaultFontFileExtension
        {
            set => _defaultFontFileExtension = value;
        }

        internal Typeface GetTypeface(string fontFamily, string style)
        {
            _tempPair = new Tuple<string, string>(fontFamily, style);
            if (_fontMap.TryGetValue(_tempPair, out var typeface))
            {
                return typeface;
            }
            var typefaceWithDefaultStyle = GetFontFamily(fontFamily);
            typeface = TypefaceForStyle(typefaceWithDefaultStyle, style);
            _fontMap[_tempPair] = typeface;
            return typeface;
        }

        private Typeface GetFontFamily(string fontFamily)
        {
            if (_fontFamilies.TryGetValue(fontFamily, out var defaultTypeface))
            {
                return defaultTypeface;
            }

            Typeface typeface = null;
            if (_delegate != null)
            {
                typeface = _delegate.FetchFont(fontFamily);
            }

            if (_delegate != null && typeface == null)
            {
                var path = _delegate.GetFontPath(fontFamily);
                if (!ReferenceEquals(path, null))
                {
                    typeface = Typeface.CreateFromAsset(path);
                }
            }

            if (typeface == null)
            {
                var path = $"Assets/Fonts/{fontFamily.Replace(" ", "")}{_defaultFontFileExtension}#{fontFamily}";
                typeface = Typeface.CreateFromAsset(path);
            }

            _fontFamilies[fontFamily] = typeface;
            return typeface;
        }

        private Typeface TypefaceForStyle(Typeface typeface, string style)
        {
            var containsItalic = style.Contains("Italic");
            var containsBold = style.Contains("Bold");

            var fontStyle = containsItalic ? FontStyle.Italic : FontStyle.Normal;
            var fontWeight = containsBold ? FontWeights.Bold : FontWeights.Normal;

            if (typeface.Style == fontStyle && typeface.Weight.Weight == fontWeight.Weight)
            {
                return typeface;
            }
            
            return Typeface.Create(typeface, fontStyle, fontWeight);
        }
    }
}
