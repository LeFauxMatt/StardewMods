namespace CommonHarmony
{
    using System.Collections;
    using System.Collections.Generic;
    using HarmonyLib;
    using StardewModdingAPI;

    internal class PatternPatches : IEnumerable<CodeInstruction>
    {
        private static IMonitor _monitor;
        private readonly IEnumerable<CodeInstruction> _instructions;
        private readonly Queue<PatternPatch> _patternPatches = new();

        public PatternPatches(IEnumerable<CodeInstruction> instructions, IMonitor monitor)
        {
            this._instructions = instructions;
            PatternPatches._monitor = monitor;
        }

        public bool Done
        {
            get => this._patternPatches.Count == 0;
        }

        public IEnumerator<CodeInstruction> GetEnumerator()
        {
            PatternPatch currentOperation = this._patternPatches.Dequeue();
            var rawStack = new LinkedList<CodeInstruction>();
            int skipped = 0;
            bool done = false;

            foreach (CodeInstruction instruction in this._instructions)
            {
                // Skipped instructions
                if (skipped > 0)
                {
                    skipped--;
                    continue;
                }

                // Pattern does not match or done matching patterns
                if (done || !currentOperation.Matches(instruction))
                {
                    rawStack.AddLast(instruction);
                    continue;
                }

                // Return patched code
                if (currentOperation.Text != null)
                {
                    PatternPatches._monitor.LogOnce(currentOperation.Text);
                }

                rawStack.AddLast(instruction);
                currentOperation.Patches(rawStack);
                foreach (CodeInstruction patch in rawStack)
                {
                    yield return patch;
                }

                rawStack.Clear();
                skipped = currentOperation.Skipped;

                // Repeat
                if (currentOperation.Loop)
                {
                    continue;
                }

                // Next pattern
                if (this._patternPatches.Count > 0)
                {
                    currentOperation = this._patternPatches.Dequeue();
                }
                else
                {
                    done = true;
                }
            }

            foreach (CodeInstruction instruction in rawStack)
            {
                yield return instruction;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public PatternPatch Find(params CodeInstruction[] pattern)
        {
            var operation = new PatternPatch(pattern);
            this._patternPatches.Enqueue(operation);
            return operation;
        }
    }
}