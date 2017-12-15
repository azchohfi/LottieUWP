using System.Collections.Generic;
using LottieUWP.Value;

namespace LottieUWP.Model
{
    /// <summary>
    /// Any item that can be a part of a <see cref="KeyPath"/> should implement this.
    /// </summary>
    public interface IKeyPathElement
    {
        /// <summary>
        /// Called recursively during keypath resolution.
        /// 
        /// The overridden method should just call:
        ///     MiscUtils.ResolveKeyPath(keyPath, depth, accumulator, currentPartialKeyPath, this);
        /// </summary>
        /// <param name="keyPath">The full keypath being resolved.</param>
        /// <param name="depth">The current depth that this element should be checked at in the keypath.</param>
        /// <param name="accumulator">A list of fully resolved keypaths. If this element fully matches the keypath then it should add itself to this list.</param>
        /// <param name="currentPartialKeyPath">A keypath that contains all parent element of this one. This element should create a copy of this and append itself with KeyPath#addKey when it adds itself to the accumulator or propagates resolution to its children.</param>
        void ResolveKeyPath(KeyPath keyPath, int depth, List<KeyPath> accumulator, KeyPath currentPartialKeyPath);

        /// <summary>
        /// The overridden method should handle appropriate properties and set value callbacks on their 
        /// animations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="callback"></param>
        void AddValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback);
    }
}
