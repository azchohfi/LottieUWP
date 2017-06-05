using System.Collections.Generic;

namespace LottieUWP
{
    internal class TrimPathContent : IContent, BaseKeyframeAnimation.IAnimationListener
    {
        private readonly IList<BaseKeyframeAnimation.IAnimationListener> _listeners = new List<BaseKeyframeAnimation.IAnimationListener>();
        private readonly IBaseKeyframeAnimation<float?> _startAnimation;
        private readonly IBaseKeyframeAnimation<float?> _endAnimation;
        private readonly IBaseKeyframeAnimation<float?> _offsetAnimation;

        internal TrimPathContent(BaseLayer layer, ShapeTrimPath trimPath)
        {
            Name = trimPath.Name;
            Type = trimPath.GetType();
            _startAnimation = trimPath.Start.CreateAnimation();
            _endAnimation = trimPath.End.CreateAnimation();
            _offsetAnimation = trimPath.Offset.CreateAnimation();

            layer.AddAnimation(_startAnimation);
            layer.AddAnimation(_endAnimation);
            layer.AddAnimation(_offsetAnimation);

            _startAnimation.AddUpdateListener(this);
            _endAnimation.AddUpdateListener(this);
            _offsetAnimation.AddUpdateListener(this);
        }

        public void OnValueChanged()
        {
            for (int i = 0; i < _listeners.Count; i++)
            {
                _listeners[i].OnValueChanged();
            }
        }

        public void SetContents(IList<IContent> contentsBefore, IList<IContent> contentsAfter)
        {
            // Do nothing.
        }

        public string Name { get; }

        internal virtual void AddListener(BaseKeyframeAnimation.IAnimationListener listener)
        {
            _listeners.Add(listener);
        }

        internal virtual ShapeTrimPath.Type Type { get; }

        public virtual IBaseKeyframeAnimation<float?> Start => _startAnimation;

        public virtual IBaseKeyframeAnimation<float?> End => _endAnimation;

        public virtual IBaseKeyframeAnimation<float?> Offset => _offsetAnimation;
    }
}