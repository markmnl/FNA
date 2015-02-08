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
using System.ComponentModel;
#endregion

namespace Microsoft.Xna.Framework
{
	class NO_AVI_GameWindow : GameWindow
	{
		#region Public GameWindow Properties

		[DefaultValue(false)]
		public override bool AllowUserResizing
		{
			get
			{
				return false;
			}
			set
			{
				// No-op. :(
			}
		}

		Rectangle clientBounds = new Rectangle(0, 0, 1280, 720);
		public override Rectangle ClientBounds
		{
			get
			{
				return clientBounds;
			}
		}

		public override DisplayOrientation CurrentOrientation
		{
			get
			{
				return DisplayOrientation.LandscapeLeft;
			}
		}

		public override IntPtr Handle
		{
			get
			{
				return INTERNAL_sdlWindow;
			}
		}

		private bool isBorderlessEXT = false;
		public override bool IsBorderlessEXT
		{
			get
			{
				return isBorderlessEXT;
			}
			set
			{
				isBorderlessEXT = value;
			}
		}

		public override string ScreenDeviceName
		{
			get
			{
				return INTERNAL_deviceName;
			}
		}

		#endregion

		#region Private SDL2 Window Variables

		private IntPtr INTERNAL_sdlWindow;

		private bool INTERNAL_isFullscreen;
		private bool INTERNAL_wantsFullscreen;

		private string INTERNAL_deviceName;

		#endregion

		#region Internal Constructor

		internal NO_AVI_GameWindow()
		{
		}

		#endregion

		#region Public GameWindow Methods

		public override void BeginScreenDeviceChange(bool willBeFullScreen)
		{
			INTERNAL_wantsFullscreen = willBeFullScreen;
		}

		public override void EndScreenDeviceChange(
			string screenDeviceName,
			int clientWidth,
			int clientHeight
		) {
			INTERNAL_deviceName = screenDeviceName;
			INTERNAL_isFullscreen = INTERNAL_wantsFullscreen;
			clientBounds.Width = clientWidth;
			clientBounds.Height = clientHeight;
		}

		#endregion

		#region Internal Methods

		internal void INTERNAL_ClientSizeChanged()
		{
			OnClientSizeChanged();
		}

		#endregion

		#region Protected GameWindow Methods

		protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
		{
		}

		protected override void SetTitle(string title)
		{
			Title = title;
		}

		#endregion
		
		#region Private Static Icon Filename Method

		private static string INTERNAL_GetIconName(string title, string extension)
		{
			string fileIn = String.Empty;
			if (System.IO.File.Exists(title + extension))
			{
				// If the title and filename work, it just works. Fine.
				fileIn = title + extension;
			}
			else
			{
				// But sometimes the title has invalid characters inside.

				/* In addition to the filesystem's invalid charset, we need to
				 * blacklist the Windows standard set too, no matter what.
				 * -flibit
				 */
				char[] hardCodeBadChars = new char[]
				{
					'<',
					'>',
					':',
					'"',
					'/',
					'\\',
					'|',
					'?',
					'*'
				};
				List<char> badChars = new List<char>();
				badChars.AddRange(System.IO.Path.GetInvalidFileNameChars());
				badChars.AddRange(hardCodeBadChars);

				string stripChars = title;
				foreach (char c in badChars)
				{
					stripChars = stripChars.Replace(c.ToString(), "");
				}
				stripChars += extension;

				if (System.IO.File.Exists(stripChars))
				{
					fileIn = stripChars;
				}
			}
			return fileIn;
		}

		#endregion
	}
}
