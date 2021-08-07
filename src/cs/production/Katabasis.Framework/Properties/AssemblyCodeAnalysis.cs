// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
	"Microsoft.Naming",
	"CA1051:DoNotDeclareVisibleInstanceFields",
	Justification = "No Microsoft, 'fields should be as an implementation detail' is wrong; fields are data and game developers want to work with data not abstractions!")]

[assembly: SuppressMessage(
	"Microsoft.Naming",
	"CA1720:IdentifiersShouldNotContainTypeNames",
	Justification = "Assume developers know the difference between a builtin type and an identifier on a type with the same name as a builtin type.")]

[assembly: SuppressMessage(
	"Microsoft.Naming",
	"CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
	Justification = "Mostly because XNA uses naming of types with a certain suffix 'Collection'. Funny how Microsoft now disagrees with how things are named in XNA.")]

[assembly: SuppressMessage(
	"Microsoft.Naming",
	"CA1806:DoNotIgnoreMethodResults",
	Justification = "FNA uses PInvoke where sometimes the result of a native function call is ignored.")]

[assembly: SuppressMessage(
	"Microsoft.Naming",
	"CA1822:MarkMembersAsStatic",
	Justification = "For game developers we really want EVERYTHING to be static, so if we don't want something to be static it's an API choice.")]

[assembly: SuppressMessage(
	"Microsoft.Naming",
	"CA1051:DoNotDeclareVisibleInstanceFields",
	Justification = "No Microsoft, 'fields should be as an implementation detail' is wrong; fields are data and game developers want to work with data not abstractions!")]

[assembly: SuppressMessage("ReSharper", "InconsistentNaming", Justification = "FNA has some inconsistencies in naming conventions.")]
