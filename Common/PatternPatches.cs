using System.Collections;
using System.Collections.Generic;
using Harmony;
using StardewModdingAPI;

namespace Common
{
    public class PatternPatches : IEnumerable<CodeInstruction>
    {
        private static IMonitor _monitor;
        private readonly Queue<PatternPatch> _patternPatches = new Queue<PatternPatch>();
        private readonly IEnumerable<CodeInstruction> _instructions;
        public bool Done => _patternPatches.Count == 0;
        public PatternPatches(IEnumerable<CodeInstruction> instructions, IMonitor monitor)
        {
            _instructions = instructions;
            _monitor = monitor;
        }
        public PatternPatch Find(IList<CodeInstruction> pattern)
        {
            var operation = new PatternPatch(pattern);
            _patternPatches.Enqueue(operation);
            return operation;
        }
        public IEnumerator<CodeInstruction> GetEnumerator()
        {
            var currentOperation = _patternPatches.Dequeue();
            var skipped = 0;
            var done = false;
            foreach (var instruction in _instructions)
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
                    yield return instruction;
                    continue;
                }
                
                // Return patched code
                if (currentOperation.Text != null)
                    _monitor.Log(currentOperation.Text);
                var patches = currentOperation.Patches(instruction);
                foreach (var patch in patches)
                {
                    yield return patch;
                }
                skipped = currentOperation.Skipped;
                
                // Repeat
                if (currentOperation.Loop)
                    continue;
                
                // Next pattern
                if (_patternPatches.Count > 0)
                    currentOperation = _patternPatches.Dequeue();
                else
                    done = true;
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}