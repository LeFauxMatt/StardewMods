using System;
using System.Collections.Generic;
using Harmony;

namespace Common
{
    public class PatternPatch
    {
        public int Skipped { get; private set; }
        public string Text { get; private set; }
        public bool Loop => _loop == -1 || --_loop > 0;

        private readonly IList<CodeInstruction> _patterns;
        private Func<CodeInstruction, CodeInstruction[]> _patch;
        private int _index;
        private int _loop;
        public PatternPatch(IList<CodeInstruction> patterns)
        {
            _patterns = patterns;
        }

        public PatternPatch Patch(Func<CodeInstruction, CodeInstruction[]> patch)
        {
            _patch = patch;
            return this;
        }
        
        public PatternPatch Log(string text)
        {
            Text = text;
            return this;
        }

        public PatternPatch Skip(int skip)
        {
            Skipped = skip;
            return this;
        }

        public PatternPatch Repeat(int loop)
        {
            _loop = loop;
            return this;
        }
        public bool Matches(CodeInstruction instruction)
        {
            // Reset on loop
            if (_index == _patterns.Count)
                _index = 0;
            
            // Opcode not matching
            if (!_patterns[_index].opcode.Equals(instruction.opcode))
            {
                _index = 0;
                return false;
            }
            
            // Operand not matching
            if (_patterns[_index].operand != null && !_patterns[_index].operand.Equals(instruction.operand))
            {
                _index = 0;
                return false;
            }
            
            // True if full pattern match
            return ++_index == _patterns.Count;
        }

        public IEnumerable<CodeInstruction> Patches(CodeInstruction instruction) =>
            _patch?.Invoke(instruction) ?? new [] {instruction};
    }
}