﻿using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.Merging;

namespace Vostok.Configuration.Abstractions.SettingsTree
{
    /// <summary>
    /// <para>Represents a tree of raw settings. 'Raw' means that all values are stored as strings.</para>
    /// <para>This is an intermediate object model for settings. It's purpose is to abstract away different storage formats, such as JSON, XML or any other.</para>
    /// </summary>
    [PublicAPI]
    public interface ISettingsNode
    {
        /// <summary>
        /// Name of the tree node. Null for array elements.
        /// </summary>
        [CanBeNull]
        string Name { get; }

        /// <summary>
        /// Value of the tree node. Not null for leaf nodes only.
        /// </summary>
        [CanBeNull]
        string Value { get; }

        /// <summary>
        /// A view of child nodes as an ordered collection. The order is same as in the source.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IEnumerable<ISettingsNode> Children { get; }

        /// <summary>
        /// Merges two settings trees by rules specified in <paramref name="options"/>.
        /// </summary>
        [CanBeNull]
        ISettingsNode Merge([CanBeNull] ISettingsNode other, [CanBeNull] SettingsMergeOptions options = null);

        /// <summary>
        /// A view of child nodes as a collection indexed by node names. Used for nodes that represent dictionaries or classes. Array elements cannot be accessed this way.
        /// </summary>
        [CanBeNull]
        ISettingsNode this[string name] { get; }
    }
}