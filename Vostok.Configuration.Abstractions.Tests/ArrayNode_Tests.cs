﻿using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Abstractions.Tests
{
    [TestFixture]
    public class ArrayNode_Tests : TreeConstructionSet
    {
        [Test]
        public void Equals_returns_false_by_name()
        {
            var sets1 = Array("Name1", "x");
            var sets2 = Array("Name2", "x");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_children_value()
        {
            var sets1 = Array("Name", "x1");
            var sets2 = Array("Name", "x2");
            Equals(sets1, sets2).Should().BeFalse();
        }
        
        [Test]
        public void Equals_returns_true_for_instances_with_null_and_empty_children()
        {
            var sets1 = new ArrayNode("Name", null);
            var sets2 = new ArrayNode("Name", new ISettingsNode[0]);
            Equals(sets1, sets2).Should().BeTrue();
        }

        [Test]
        public void Hashes_should_be_equal_for_equal_instances()
        {
            var sets1 = Array("Name", "x").GetHashCode();
            var sets2 = Array("Name", "x").GetHashCode();
            sets1.Should().Be(sets2);
        }

        [Test]
        public void Should_return_other_on_merging_with_another_node_type()
        {
            var sets1 = Array(null, "x1");
            var sets2 = Value("x2");

            var merge = sets1.Merge(sets2);
            merge.Value.Should().Be("x2");

            merge = sets2.Merge(sets1);
            merge.Children.First().Value.Should().Be("x1");
        }

        [TestCase(ArrayMergeStyle.Replace)]
        [TestCase(ArrayMergeStyle.Concat)]
        [TestCase(ArrayMergeStyle.Union)]
        [TestCase(ArrayMergeStyle.PerElement)]
        public void Should_merge_with_different_options(ArrayMergeStyle style)
        {
            var sets1 = Array(null, "x1", "x2", "x3");
            var sets2 = Array(null, "x1", "x4", "x5");

            var merge = sets1.Merge(sets2, new SettingsMergeOptions { ArrayMergeStyle = style });
            switch (style)
            {
                case ArrayMergeStyle.Replace:
                case ArrayMergeStyle.PerElement:
                    merge.Children.Select(c => c.Value).Should().Equal("x1", "x4", "x5");
                    break;
                case ArrayMergeStyle.Concat:
                    merge.Children.Select(c => c.Value).Should().Equal("x1", "x2", "x3", "x1", "x4", "x5");
                    break;
                case ArrayMergeStyle.Union:
                    merge.Children.Select(c => c.Value).Should().Equal("x1", "x2", "x3", "x4", "x5");
                    break;
            }
        }

        [Test]
        public void Should_merge_arrays_of_object_nodes_with_per_element_merge_style()
        {
            var array1 = Array("array1", 
                Object(null, ("A", "1")), 
                Object(null, ("A", "2")), 
                Object(null, ("A", "3")));

            var array2 = Array("array2",
                Object(null, ("B", "2")),
                Object(null, ("B", "1")));

            var options = new SettingsMergeOptions
            {
                ArrayMergeStyle = ArrayMergeStyle.PerElement,
                ObjectMergeStyle = ObjectMergeStyle.Deep
            };

            array1.Merge(array2, options).Should().Be(Array("array2",
                Object(null, ("A", "1"), ("B", "2")),
                Object(null, ("A", "2"), ("B", "1")),
                Object(null, ("A", "3")))
            );

            array2.Merge(array1, options).Should().Be(Array("array1",
                Object(null, ("A", "1"), ("B", "2")),
                Object(null, ("A", "2"), ("B", "1")),
                Object(null, ("A", "3")))
            );
        }

        [Test]
        public void Should_merge_with_different_options_right_way()
        {
            var sets1 = Array(Value("x1"), Object(("value", "x11")), Object(("value", "x12")));
            var sets2 = Array(Value("x1"), Value("x2"), Object(("value", "x11")), Object(("value", "x21")));

            var merge = sets1.Merge(sets2, new SettingsMergeOptions { ObjectMergeStyle = ObjectMergeStyle.Shallow, ArrayMergeStyle = ArrayMergeStyle.Union });
            var children = merge.Children.ToArray();
            children.Length.Should().Be(5);
            children[0].Value.Should().Be("x1");
            children[1]["value"].Value.Should().Be("x11");
            children[2]["value"].Value.Should().Be("x12");
            children[3].Value.Should().Be("x2");
            children[4]["value"].Value.Should().Be("x21");
        }

        [Test]
        public void Merge_with_null_should_keep_non_null_node([Values] ArrayMergeStyle mergeStyle)
        {
            var node = Array("a", "b", "c");

            node.Merge(null, new SettingsMergeOptions {ArrayMergeStyle = mergeStyle}).Should().BeSameAs(node);
        }

        [Test]
        public void Merge_should_still_merge_if_names_differ_and_prefer_other_node_name()
        {
            var node1 = Array("xx", "yy");
            var node2 = Array("yy", "yy");

            node1.Merge(node2, new SettingsMergeOptions {ArrayMergeStyle = ArrayMergeStyle.Concat})
                .Should().Be(Array("yy", "yy", "yy"));

            node1 = Array("xx", "yy");
            node2 = Array("XX", "yy");

            node1.Merge(node2, new SettingsMergeOptions {ArrayMergeStyle = ArrayMergeStyle.Concat})
                .Should().Be(Array("xx", "yy", "yy"));
        }

        [Test]
        public void Equals_should_respect_order_of_elements()
        {
            var node1 = Array("xx", "a", "b");
            var node2 = Array("xx", "b", "a");

            node1.Equals(node2).Should().BeFalse();
        }

        [Test]
        public void GetHashCode_should_respect_order_of_elements()
        {
            var node1 = Array("xx", "a", "b");
            var node2 = Array("xx", "b", "a");

            node1.GetHashCode().Should().NotBe(node2.GetHashCode());
        }

        [Test]
        public void GetHashCode_should_support_null_children()
        {
            var node = Array("xx", Value("a"), null, Value("b"));

            node.GetHashCode().Should().NotBe(0);
        }

        [Test]
        public void Children_must_never_be_null()
        {
            new ArrayNode("xx", null).Children.Should().NotBeNull();
        }

        [Test]
        public void Equals_should_be_case_insensitive_for_names()
        {
            var node1 = Array("xx", "yy");
            var node2 = Array("XX", "yy");

            node1.Equals(node2).Should().BeTrue();
        }

        [Test]
        public void GetHashCode_should_use_node_name()
        {
            var node1 = Array("xx", "yy");
            var node2 = Array("yy", "yy");

            node1.GetHashCode().Should().NotBe(node2.GetHashCode());
        }

        [Test]
        public void GetHashCode_should_use_case_insensitive_hash_for_node_name()
        {
            var node1 = Array("xx", "yy");
            var node2 = Array("XX", "yy");

            node1.GetHashCode().Should().Be(node2.GetHashCode());
        }
    }
}