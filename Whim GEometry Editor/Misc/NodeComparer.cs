using MDLLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whim_GEometry_Editor.Misc
{
    public static class NodeComparer
    {
        public static bool AreNodesListIdentical(List<w3Node> list1, List<w3Node> list2)
        {
            // Step 1: Check if both lists have the same count of nodes
            if (list1.Count != list2.Count) return false;

            // Step 2: Create dictionaries for fast lookup based on node names and their parent names
            var list1Relationships = BuildNodeRelationships(list1);
            var list2Relationships = BuildNodeRelationships(list2);

            // Step 3: Compare dictionaries for identical relationships
            return list1Relationships.Count == list2Relationships.Count
                && list1Relationships.All(kvp => list2Relationships.TryGetValue(kvp.Key, out var parent) && parent == kvp.Value);
        }

        private static Dictionary<string, string> BuildNodeRelationships(List<w3Node> nodes)
        {
            var relationships = new Dictionary<string, string>();

            // Map each node name to its parent's name (or null if no parent)
            var nodeById = nodes.ToDictionary(node => node.objectId);

            foreach (var node in nodes)
            {
                string parentName = nodeById.TryGetValue(node.parentId, out var parentNode) ? parentNode.Name : null;
                relationships[node.Name] = parentName;
            }

            return relationships;
        }
    }
}
