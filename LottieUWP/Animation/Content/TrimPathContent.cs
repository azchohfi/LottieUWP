using System;
using System.Collections.Generic;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Model.Content;
using LottieUWP.Model.Layer;

namespace LottieUWP.Animation.Content
{
    internal class TrimPathContent : IContent
    {
        public event EventHandler ValueChanged;

        internal TrimPathContent(BaseLayer layer, ShapeTrimPath trimPath)
        {
            Name = trimPath.Name;
            IsHidden = trimPath.IsHidden;
            Type = trimPath.GetType();
            Start = trimPath.Start.CreateAnimation();
            End = trimPath.End.CreateAnimation();
            Offset = trimPath.Offset.CreateAnimation();

            layer.AddAnimation(Start);
            layer.AddAnimation(End);
            layer.AddAnimation(Offset);

            Start.ValueChanged += OnValueChanged;
            End.ValueChanged += OnValueChanged;
            Offset.ValueChanged += OnValueChanged;
        }

        private void OnValueChanged(object sender, EventArgs eventArgs)
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter)
        {
            // Do nothing.
        }

        public string Name { get; }

        internal ShapeTrimPath.Type Type { get; }

        public IBaseKeyframeAnimation<float?, float?> Start { get; }

        public IBaseKeyframeAnimation<float?, float?> End { get; }

        public IBaseKeyframeAnimation<float?, float?> Offset { get; }

        public bool IsHidden { get; }
    }
}