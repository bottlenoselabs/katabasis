// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using bottlenoselabs.Katabasis.ImGui;
using static bottlenoselabs.imgui;

namespace bottlenoselabs.Katabasis.Samples
{
	public class App : Game
	{
		private ImGuiRenderer _imGuiRenderer = null!;
		private float _float;
		private bool _showTestWindow;
		private bool _showAnotherWindow;
		private Vector3 _clearColor = new(114f / 255f, 144f / 255f, 154f / 255f);

		public App()
		{
			Window.Title = "Katabasis Samples; ImGui: Hello, world!";
			IsMouseVisible = true;
			Window.AllowUserResizing = true;
		}

		protected override void Initialize()
		{
			_imGuiRenderer = new ImGuiRenderer();
			_imGuiRenderer.BuildFontAtlas();
		}

		protected override void Update(GameTime gameTime)
		{
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(new Color(_clearColor.X, _clearColor.Y, _clearColor.Z));
			_imGuiRenderer.Begin(gameTime);
			DrawUserInterface();
			_imGuiRenderer.End();
		}

		protected virtual unsafe void DrawUserInterface()
		{
			// tip: if we don't call ImGui.Begin()/ImGui.End() the widgets appears in a window automatically called "Debug"
			{
				igText("Hello, world!");
				igColorEdit3("Clear color", (float*)Unsafe.AsPointer(ref _clearColor), default);
				igSliderFloat("Float value", (float*)Unsafe.AsPointer(ref _float), 0.0f, 1.0f, string.Empty, default);
				if (igButton("Test Window", default)) _showTestWindow = !_showTestWindow;
				if (igButton("Another Window", default)) _showAnotherWindow = !_showAnotherWindow;
				igText(
					$"Application average {1000f / igGetIO()->Framerate:F3} ms/frame ({igGetIO()->Framerate:F1} FPS)");
			}
			
			// show the ImGui test window
			if (_showTestWindow)
			{
				ImGuiCond cond;
				cond.Data = (int)ImGuiCond_.ImGuiCond_FirstUseEver;
				igSetNextWindowPos(new Vector2(650, 20), cond, default);
				igShowDemoWindow((Runtime.CBool*)Unsafe.AsPointer(ref _showTestWindow));
			}

			// show another simple window, this time using an explicit Begin/End pair
			if (_showAnotherWindow)
			{
				ImGuiCond cond;
				cond.Data = (int)ImGuiCond_.ImGuiCond_FirstUseEver;
				igSetNextWindowSize(new Vector2(200, 100), cond);
				igBegin("Another Window", (Runtime.CBool*)Unsafe.AsPointer(ref _showAnotherWindow), default);
				igText("Hello");
				igEnd();
			}
		}
	}
}
