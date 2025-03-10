﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.EditorConfig.Parsing;

/// <summary>
/// An abstraction over an editorconfig option that reprsents some type <typeparamref name="T"/> and the span in which that option was defined.
/// </summary>
internal sealed record class EditorConfigOption<T>(Section Section, TextSpan? Span, T Value)
    : EditorConfigOption(Section, Span);
