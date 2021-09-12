// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Numerics;
using System.Reflection;
using JetBrains.Annotations;

namespace Katabasis.ImGui
{
    [PublicAPI]
    public class ImGuiEffect : Effect
    {
        private readonly EffectParameter _worldViewProjectionMatrix;

        public ImGuiEffect()
			: base(FromStream(Assembly.GetAssembly(typeof(ImGuiEffect))!
                .GetManifestResourceStream("Katabasis.ImGui.Main.fxb")!))
        {
            _worldViewProjectionMatrix = Parameters!["WorldViewProjectionMatrix"]!;
        }

        public ImGuiEffect(Effect effectClone)
            : base(effectClone)
        {
            _worldViewProjectionMatrix = Parameters!["WorldViewProjectionMatrix"]!;
        }

        public void UpdateWorldViewProjectionMatrix(ref Matrix4x4 matrix)
        {
            _worldViewProjectionMatrix.SetValue(matrix);
        }
    }
}
