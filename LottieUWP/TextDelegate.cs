using System.Collections.Generic;

namespace LottieUWP
{
    /// <summary>
    /// Extend this class to replace animation text with custom text. This can be useful to handle
    /// translations.
    /// 
    /// The only method you should have to override is <seealso cref="GetText(string)"/>.
    /// </summary>
    public class TextDelegate
    {
        private readonly Dictionary<string, string> _stringMap = new Dictionary<string, string>();
        private readonly ILottieInvalidatable _invalidatable;
        private bool _cacheText = true;

        /// <summary>
        /// This normally needs to be able to invalidate the view/drawable but not for the test.
        /// </summary>
        internal TextDelegate()
        {
            _invalidatable = null;
        }

        public TextDelegate(ILottieInvalidatable invalidatable)
        {
            _invalidatable = invalidatable;
        }

        /// <summary>
        /// Override this to replace the animation text with something dynamic. This can be used for
        /// translations or custom data.
        /// </summary>
        private string GetText(string input)
        {
            return input;
        }

        /// <summary>
        /// Update the text that will be rendered for the given input text.
        /// </summary>
        public void SetText(string input, string output)
        {
            _stringMap[input] = output;
            Invalidate();
        }

        /// <summary>
        /// Sets whether or not <seealso cref="TextDelegate"/> will cache (memoize) the results of getText.
        /// If this isn't necessary then set it to false.
        /// </summary>
        public bool CacheText
        {
            set => _cacheText = value;
        }

        /// <summary>
        /// Invalidates a cached string with the given input.
        /// </summary>
        public void InvalidateText(string input)
        {
            _stringMap.Remove(input);
            Invalidate();
        }

        /// <summary>
        /// Invalidates all cached strings
        /// </summary>
        public void InvalidateAllText()
        {
            _stringMap.Clear();
            Invalidate();
        }

        internal string GetTextInternal(string input)
        {
            if (_cacheText && _stringMap.ContainsKey(input))
            {
                return _stringMap[input];
            }
            var text = GetText(input);
            if (_cacheText)
            {
                _stringMap[input] = text;
            }
            return text;
        }

        private void Invalidate()
        {
            _invalidatable?.InvalidateSelf();
        }
    }
}
