// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop", "SA1633", Justification = "License is repo/solution level.", Scope = "module")]
[assembly: SuppressMessage("StyleCop", "SA1309", Justification = "Private field names should begin with underscore", Scope = "module")]
[assembly: SuppressMessage("StyleCop", "SA1101", Justification = "StyleCop is dump and can't recognize pattern matches", Scope = "module")]
[assembly: SuppressMessage("StyleCop", "SA1507", Justification = "Externally provided interface", Scope = "namespaceanddescendants", Target = "Common.Integrations")]
[assembly: SuppressMessage("StyleCop", "SA1514", Justification = "Externally provided interface", Scope = "namespaceanddescendants", Target = "Common.Integrations")]
[assembly: SuppressMessage("StyleCop", "SA1515", Justification = "Externally provided interface", Scope = "namespaceanddescendants", Target = "Common.Integrations")]