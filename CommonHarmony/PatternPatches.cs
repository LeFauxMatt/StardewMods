namespace CommonHarmony
{
    using System.Collections;
    using System.Collections.Generic;
    using HarmonyLib;

    /// <inheritdoc />
    internal class PatternPatches : IEnumerable<CodeInstruction>
    {
        private readonly IEnumerable<CodeInstruction> _instructions;
        private readonly Queue<PatternPatch> _patternPatches = new();

        /// <summary></summary>
        /// <param name="instructions"></param>
        public PatternPatches(IEnumerable<CodeInstruction> instructions)
        {
            this._instructions = instructions;
            this._patternPatches = new();
        }

        public PatternPatches(IEnumerable<CodeInstruction> instructions, params PatternPatch[] patches)
        {
            this._instructions = instructions;
            this._patternPatches = new(patches);
        }

        public PatternPatches(IEnumerable<CodeInstruction> instructions, PatternPatch patch)
        {
            this._instructions = instructions;
            this._patternPatches = new(
                new[]
                {
                    patch,
                });
        }

        /// <summary>Gets a value indicating whether gets whether all patches were successfully applied.</summary>
        public bool Done
        {
            get => this._patternPatches.Count == 0;
        }

        /// <inheritdoc />
        public IEnumerator<CodeInstruction> GetEnumerator()
        {
            var currentOperation = this._patternPatches.Dequeue();
            var rawStack = new LinkedList<CodeInstruction>();
            var skipped = 0;
            var done = false;

            foreach (var instruction in this._instructions)
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

                rawStack.AddLast(instruction);
                currentOperation.Patches(rawStack);
                foreach (var patch in rawStack)
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

            foreach (var instruction in rawStack)
            {
                yield return instruction;
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void AddPatch(PatternPatch patch)
        {
            this._patternPatches.Enqueue(patch);
        }
    }
}