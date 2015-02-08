#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2014 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace Microsoft.Xna.Framework
{
	class NO_AVI_GamePlatform : GamePlatform
	{
		#region Private Game Loop Sentinel

		private bool INTERNAL_runApplication;

		#endregion

		#region Private Active XNA Key List

		private List<Keys> keys;

		#endregion

		#region Private Text Input Variables

		private int[] INTERNAL_TextInputControlRepeat;
		private bool[] INTERNAL_TextInputControlDown;
		private bool INTERNAL_TextInputSuppress;

		#endregion

		#region Private DisplayMode Variables

		private int displayIndex = 0;
		private DisplayModeCollection supportedDisplayModes = null;

		#endregion

		#region Public Constructor

		public NO_AVI_GamePlatform(Game game) : base(game, "NO_AVI")
		{
			Window = new NO_AVI_GameWindow();

			// Create the DisplayMode list
			displayIndex = 0;
			INTERNAL_GenerateDisplayModes();
			
			// We hide the mouse cursor by default.
			if (IsMouseVisible)
			{
				IsMouseVisible = false;
			}
			
			// Initialize Active Key List
			keys = new List<Keys>();

			// Setup Text Input Control Character Arrays (Only 4 control keys supported at this time)
			INTERNAL_TextInputControlDown = new bool[4];
			INTERNAL_TextInputControlRepeat = new int[4];

			// Assume we will have focus.
			IsActive = true;

			// Ready to run the loop!
			INTERNAL_runApplication = true;
		}

		#endregion

		#region Public GamePlatform Methods

		public override void RunLoop()
		{
			while (INTERNAL_runApplication)
			{
#if !THREADED_GL
				Threading.Run();
#endif
				while (INTERNAL_runApplication)
				{
					Game.Tick();
				}
			}

			// We out.
			Game.Exit();
		}

		public override void Exit()
		{
			// Stop the game loop
			INTERNAL_runApplication = false;
		}

		public override void BeforeInitialize()
		{
			base.BeforeInitialize();
		}

		public override bool BeforeUpdate(GameTime gameTime)
		{
			// Update our OpenAL context
			if (OpenALDevice.Instance != null)
			{
				OpenALDevice.Instance.Update();
			}

			return true;
		}

		public override bool BeforeDraw(GameTime gameTime)
		{
			return true;
		}

		public override void BeginScreenDeviceChange(bool willBeFullScreen)
		{
			Window.BeginScreenDeviceChange(willBeFullScreen);
		}

		public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
		{
			Window.EndScreenDeviceChange(screenDeviceName, clientWidth, clientHeight);

#if WIIU_GAMEPAD
			wiiuPixelData = new byte[clientWidth * clientHeight * 4];
#endif
		}

		public override void Log(string Message)
		{
			Console.WriteLine(Message);
		}

		public override void Present()
		{
			base.Present();

			GraphicsDevice device = Game.GraphicsDevice;
			if (device != null)
			{
				device.Present();
#if WIIU_GAMEPAD
				if (wiiuStream != IntPtr.Zero)
				{
					device.GetBackBufferData(wiiuPixelData);
					DRC.drc_push_vid_frame(
						wiiuStream,
						wiiuPixelData,
						(uint) wiiuPixelData.Length,
						(ushort) device.GLDevice.Backbuffer.Width,
						(ushort) device.GLDevice.Backbuffer.Height,
						DRC.drc_pixel_format.DRC_RGBA,
						DRC.drc_flipping_mode.DRC_NO_FLIP
					);
				}
#endif
			}
		}

		public override void ShowRuntimeError(string title, string message)
		{
			Log(title + " " + message);
		}

		#endregion

		#region Internal GamePlatform Methods

		internal override DisplayMode GetCurrentDisplayMode()
		{
			int i = 0;
			foreach (DisplayMode mode in supportedDisplayModes)
			{
				if (i == displayIndex)
					return mode;
			}
			throw new InvalidOperationException();
		}

		internal override DisplayModeCollection GetDisplayModes()
		{
			return supportedDisplayModes;
		}

		internal override void SetPresentationInterval(PresentInterval interval)
		{
		}

		internal override bool HasTouch()
		{
			return true;
		}

		#endregion

		#region Protected GamePlatform Methods

		protected override void OnIsMouseVisibleChanged()
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (Window != null)
				{
					Window = null;
				}

				if (OpenALDevice.Instance != null)
				{
					OpenALDevice.Instance.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Private DisplayMode Methods

		private void INTERNAL_GenerateDisplayModes()
		{
			List<DisplayMode> modes = new List<DisplayMode>();
			modes.Add(new DisplayMode(1280, 720, SurfaceFormat.Color));
			supportedDisplayModes = new DisplayModeCollection(modes);
		}

		#endregion

		#region Private TextInput Methods

		private void INTERNAL_TextInputIn(Keys key)
		{
			if (key == Keys.Back)
			{
				INTERNAL_TextInputControlDown[0] = true;
				INTERNAL_TextInputControlRepeat[0] = Environment.TickCount + 400;
				TextInputEXT.OnTextInput((char) 8); // Backspace
			}
			else if (key == Keys.Tab)
			{
				INTERNAL_TextInputControlDown[1] = true;
				INTERNAL_TextInputControlRepeat[1] = Environment.TickCount + 400;
				TextInputEXT.OnTextInput((char) 9); // Tab
			}
			else if (key == Keys.Enter)
			{
				INTERNAL_TextInputControlDown[2] = true;
				INTERNAL_TextInputControlRepeat[2] = Environment.TickCount + 400;
				TextInputEXT.OnTextInput((char) 13); // Enter
			}
			else if (keys.Contains(Keys.LeftControl) && key == Keys.V)
			{
				INTERNAL_TextInputControlDown[3] = true;
				INTERNAL_TextInputControlRepeat[3] = Environment.TickCount + 400;
				TextInputEXT.OnTextInput((char) 22); // Control-V (Paste)
				INTERNAL_TextInputSuppress = true;
			}
		}

		private void INTERNAL_TextInputOut(Keys key)
		{
			if (key == Keys.Back)
			{
				INTERNAL_TextInputControlDown[0] = false;
			}
			else if (key == Keys.Tab)
			{
				INTERNAL_TextInputControlDown[1] = false;
			}
			else if (key == Keys.Enter)
			{
				INTERNAL_TextInputControlDown[2] = false;
			}
			else if ((!keys.Contains(Keys.LeftControl) && INTERNAL_TextInputControlDown[3]) || key == Keys.V)
			{
				INTERNAL_TextInputControlDown[3] = false;
				INTERNAL_TextInputSuppress = false;
			}
		}

		private void INTERNAL_TextInputUpdate()
		{
			if (INTERNAL_TextInputControlDown[0] && INTERNAL_TextInputControlRepeat[0] <= Environment.TickCount)
			{
				TextInputEXT.OnTextInput((char) 8);
			}
			if (INTERNAL_TextInputControlDown[1] && INTERNAL_TextInputControlRepeat[1] <= Environment.TickCount)
			{
				TextInputEXT.OnTextInput((char) 9);
			}
			if (INTERNAL_TextInputControlDown[2] && INTERNAL_TextInputControlRepeat[2] <= Environment.TickCount)
			{
				TextInputEXT.OnTextInput((char) 13);
			}
			if (INTERNAL_TextInputControlDown[3] && INTERNAL_TextInputControlRepeat[3] <= Environment.TickCount)
			{
				TextInputEXT.OnTextInput((char) 22);
			}
		}

		#endregion
	}
}
