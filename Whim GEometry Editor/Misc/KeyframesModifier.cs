using MDLLib;
using SharpGL.SceneGraph.Transformations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Whim_Model_Editor;

namespace Whim_GEometry_Editor.Misc
{
    internal static class KeyframesModifier
    {
        public static void Quantize(List<w3Keyframe> keyframes)
        {
            if (keyframes.Count <= 1)
            {
                MessageBox.Show("Keyframes count must be greater than 1"); return;
            }
            //unfinished
        }
        public static void FillGaps(List<w3Keyframe> keyframes)
        {
            if (keyframes.Count <= 1)
            {
                MessageBox.Show("Keyframes count must be greater than 1"); return;
            }
            //unfinished
        }
        public static void TileLoop(List<w3Keyframe> keyframes)
        {
            if (keyframes.Count <= 1)
            {
                MessageBox.Show("Keyframes count must be greater than 1"); return;
            }

            UserInput i = new UserInput();
            i.Title = "How much times?";
            i.ShowDialog();
            if (i.ShowDialog() == true)
            {
                bool parsed = int.TryParse(i.Box.Text, out int times);
                if (parsed)
                {
                    if (times <= 0) { MessageBox.Show("Invalid value. Expected greater than 1"); return; }
                    if (times > 1)
                    {
                        //unfinished
                    }
                }
                else
                {
                    MessageBox.Show("Invalid input. Integer expected.");
                }
            }
        }

        internal static void ReverseLocalOrder(List<w3Keyframe> keyframes, int start, int end)
        {
            if (keyframes == null || keyframes.Count <= 1)
            {
                return;
            }
            if (start < 0 || end >= keyframes.Count || start >= end)
            {
                return;
            }

            // Reverse only the specified range in the list
            while (start < end)
            {
                // Swap elements at start and end
                var temp = keyframes[start];
                keyframes[start] = keyframes[end];
                keyframes[end] = temp;

                start++;
                end--;
            }
        }

        internal static void QuantizeLocal(List<w3Keyframe> keyframes, int from, int to)
        {
            throw new NotImplementedException();
        }

        internal static void FillGapsLocal(List<w3Keyframe> keyframes, int from, int to)
        {
            throw new NotImplementedException();
        }

        internal static void TileLoopLocal(List<w3Keyframe> keyframes, int from, int to)
        {
            throw new NotImplementedException();
        }
    }

    class IntervalFinder
    {
        public static List<string> FindUnusedIntervals(List<w3Sequence> sequences, int maxLimit = 999999)
        {
            List<string> freeIntervals = new List<string>();

            if (sequences == null || sequences.Count == 0)
            {
                freeIntervals.Add($"Free: 0 - {maxLimit}");
                return freeIntervals;
            }

            // Sort sequences by start and then by end
            sequences = sequences.OrderBy(s => s.From).ThenBy(s => s.To).ToList();

            // Track the end of the last checked sequence
            int currentEnd = -1;

            // Check for gaps between consecutive sequences
            foreach (var seq in sequences)
            {
                if (seq.From > currentEnd + 1)
                {
                    // Gap found between currentEnd and next sequence start
                    freeIntervals.Add($"Free: {currentEnd + 1} - {seq.From - 1}");
                }

                // Update currentEnd to the max end of overlapping intervals
                currentEnd = Math.Max(currentEnd, seq.To);
            }

            // Check for a gap after the last sequence until max limit
            if (currentEnd < maxLimit)
            {
                freeIntervals.Add($"Free: {currentEnd + 1} - {maxLimit}");
            }

            return freeIntervals;
        }

    }
}
