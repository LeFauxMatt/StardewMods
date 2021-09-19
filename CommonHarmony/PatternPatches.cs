namespace CommonHarmony
{
    using System.Collections;
    using System.Collections.Generic;
    using HarmonyLib;
    using StardewModdingAPI;

    /// <inheritdoc />
    internal class PatternPatches : IEnumerable<CodeInstruction>
    {
        private static IMonitor Monitor;
        private readonly IEnumerable<CodeInstruction> _instructions;
        private readonly Queue<PatternPatch> _patternPatches = new();

        /// <summary></summary>
        /// <param name="instructions"></param>
        /// <param name="monitor"></param>
        public PatternPatches(IEnumerable<CodeInstruction> instructions, IMonitor monitor)
        {
            this._instructions = instructions;
            PatternPatches.Monitor = monitor;
        }

        /// <summary></summary>
        public bool Done
        {
            get => this._patternPatches.Count == 0;
        }

        /// <inheritdoc/>
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
                    PatternPatches.Monitor.LogOnce(currentOperation.Text);
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

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary></summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public PatternPatch Find(params CodeInstruction[] pattern)
        {
            var operation = new PatternPatch(pattern);
            this._patternPatches.Enqueue(operation);
            return operation;
        }
    }
}