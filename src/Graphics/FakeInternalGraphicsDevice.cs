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
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	internal class FakeInternalGraphicsDevice : OpenGLDevice
	{
		#region OpenGL Texture Container Class

		public class FakeOpenGLTexture : OpenGLTexture
		{
			public FakeOpenGLTexture() 
				: base ()
			{
			}
		}

		#endregion

		#region OpenGL Vertex Buffer Container Class

		public class FakeOpenGLVertexBuffer : OpenGLVertexBuffer
		{
			public FakeOpenGLVertexBuffer()
				: base()
			{
				
			}
		}

		#endregion

		#region OpenGL Index Buffer Container Class

		public class FakeOpenGLIndexBuffer : OpenGLIndexBuffer
		{
			public FakeOpenGLIndexBuffer()
				:base()
			{
			}
		}

		#endregion

		#region Alpha Blending State Variables

		internal bool alphaBlendEnable = false;
		private Color blendColor = Color.Transparent;
		private BlendFunction blendOp = BlendFunction.Add;
		private BlendFunction blendOpAlpha = BlendFunction.Add;
		private Blend srcBlend = Blend.One;
		private Blend dstBlend = Blend.Zero;
		private Blend srcBlendAlpha = Blend.One;
		private Blend dstBlendAlpha = Blend.Zero;
		private ColorWriteChannels colorWriteEnable = ColorWriteChannels.All;
		private ColorWriteChannels colorWriteEnable1 = ColorWriteChannels.All;
		private ColorWriteChannels colorWriteEnable2 = ColorWriteChannels.All;
		private ColorWriteChannels colorWriteEnable3 = ColorWriteChannels.All;

		#endregion

		#region Depth State Variables

		internal bool zEnable = false;
		private bool zWriteEnable = false;
		private CompareFunction depthFunc = CompareFunction.Less;

		#endregion

		#region Stencil State Variables

		public int ReferenceStencil
		{
			get
			{
				return stencilRef;
			}
			set
			{
				if (value != stencilRef)
				{
					stencilRef = value;
				}
			}
		}

		private bool stencilEnable = false;
		private int stencilWriteMask = -1; // AKA 0xFFFFFFFF, ugh -flibit
		private bool separateStencilEnable = false;
		private int stencilRef = 0;
		private int stencilMask = -1; // AKA 0xFFFFFFFF, ugh -flibit
		private CompareFunction stencilFunc = CompareFunction.Always;
		private StencilOperation stencilFail = StencilOperation.Keep;
		private StencilOperation stencilZFail = StencilOperation.Keep;
		private StencilOperation stencilPass = StencilOperation.Keep;
		private CompareFunction ccwStencilFunc = CompareFunction.Always;
		private StencilOperation ccwStencilFail = StencilOperation.Keep;
		private StencilOperation ccwStencilZFail = StencilOperation.Keep;
		private StencilOperation ccwStencilPass = StencilOperation.Keep;

		#endregion

		#region Rasterizer State Variables

		internal bool scissorTestEnable = false;
		internal CullMode cullFrontFace = CullMode.None;
		private FillMode fillMode = FillMode.Solid;
		private float depthBias = 0.0f;
		private float slopeScaleDepthBias = 0.0f;

		#endregion

		#region Viewport State Variables

		/* These two aren't actually empty rects by default in OpenGL,
		 * but we don't _really_ know the starting window size, so
		 * force apply this when the GraphicsDevice is initialized.
		 * -flibit
		 */
		private Rectangle scissorRectangle =  new Rectangle(
			0,
			0,
			0,
			0
		);
		private Rectangle viewport = new Rectangle(
			0,
			0,
			0,
			0
		);
		private float depthRangeMin = 0.0f;
		private float depthRangeMax = 1.0f;

		#endregion

		#region Texture Collection Variables

		// FIXME: This doesn't need to be public. Blame VideoPlayer. -flibit
		public Microsoft.Xna.Framework.Graphics.OpenGLDevice.OpenGLTexture[] Textures
		{
			get;
			private set;
		}

		#endregion

		#region Buffer Binding Cache Variables

		private uint currentVertexBuffer = 0;
		private uint currentIndexBuffer = 0;

		#endregion

		#region Render Target Cache Variables

		private uint currentReadFramebuffer = 0;
		public uint CurrentReadFramebuffer
		{
			get
			{
				return currentReadFramebuffer;
			}
		}

		private uint currentDrawFramebuffer = 0;
		public uint CurrentDrawFramebuffer
		{
			get
			{
				return currentDrawFramebuffer;
			}
		}

		private uint targetFramebuffer = 0;
		private uint[] currentAttachments;
		private Microsoft.Xna.Framework.Graphics.OpenGLDevice.GLenum[] currentAttachmentFaces;
		private int currentDrawBuffers;
		private Microsoft.Xna.Framework.Graphics.OpenGLDevice.GLenum[] drawBuffersArray;
		private uint currentRenderbuffer;
		private DepthFormat currentDepthStencilFormat;

		#endregion

		#region Clear Cache Variables

		private Vector4 currentClearColor = new Vector4(0, 0, 0, 0);
		private float currentClearDepth = 1.0f;
		private int currentClearStencil = 0;

		#endregion

		#region Private OpenGL Context Variable

		private IntPtr glContext;

		#endregion

		#region Faux-Backbuffer Variable

		public override Microsoft.Xna.Framework.Graphics.OpenGLDevice.FauxBackbuffer Backbuffer
		{
			get;
			protected set;
		}

		#endregion

		#region OpenGL Extensions List, Device Capabilities Variables

		public string Extensions
		{
			get;
			private set;
		}

		public bool SupportsDxt1
		{
			get;
			private set;
		}

		public bool SupportsS3tc
		{
			get;
			private set;
		}

		public bool SupportsHardwareInstancing
		{
			get;
			private set;
		}

		public int MaxTextureSlots
		{
			get;
			private set;
		}

		#endregion

		#region Public Constructor

		public FakeInternalGraphicsDevice(
			PresentationParameters presentationParameters
		) {
			this.Backbuffer = new FakeFauxBackbuffer(
				this,
				GraphicsDeviceManager.DefaultBackBufferWidth,
				GraphicsDeviceManager.DefaultBackBufferHeight,
				presentationParameters.DepthStencilFormat
			);

			int numSamplers = 32;
			Textures = new OpenGLTexture[numSamplers];
			for (int i = 0; i < numSamplers; i += 1)
			{
				Textures[i] = OpenGLTexture.NullTexture;
			}
			MaxTextureSlots = numSamplers;
		}

		#endregion

		#region Dispose Method

		public override void Dispose()
		{

		}

		#endregion

		#region Window SwapBuffers Method

		public override void SwapBuffers(IntPtr overrideWindowHandle)
		{
		}

		#endregion

		#region String Marker Method

		public override void SetStringMarker(string text)
		{
		}

		#endregion

		#region State Management Methods

		public override void SetViewport(Viewport vp, bool renderTargetBound)
		{
		}

		public override void SetScissorRect(
			Rectangle scissorRect,
			bool renderTargetBound
		) {
		}

		public override void SetBlendState(BlendState blendState)
		{
		}

		public override void SetDepthStencilState(DepthStencilState depthStencilState)
		{
		}

		public override void ApplyRasterizerState(
			RasterizerState rasterizerState,
			bool renderTargetBound
		) {
		}

		public override void VerifySampler(int index, Texture texture, SamplerState sampler)
		{
		}

		#endregion

        #region Effect Methods

        public override OpenGLEffect CreateEffect(byte[] effectCode)
        {
            IntPtr effect = IntPtr.Zero;
            IntPtr glEffect = IntPtr.Zero;
            return new OpenGLEffect(effect, glEffect);
        }

        public override void DeleteEffect(OpenGLEffect effect)
        {
        }

        public OpenGLEffect CloneEffect(OpenGLEffect cloneSource)
        {
            IntPtr effect = IntPtr.Zero;
            IntPtr glEffect = IntPtr.Zero;
            return new OpenGLEffect(effect, glEffect);
        }

        public override void ApplyEffect(
            OpenGLEffect effect,
            IntPtr technique,
            uint pass,
            ref MojoShader.MOJOSHADER_effectStateChanges stateChanges
        )
        {
        }

        #endregion

        #region glVertexAttribPointer/glVertexAttribDivisor Methods

        public override void ApplyVertexAttributes(
            VertexBufferBinding[] bindings,
            int numBindings,
            bool bindingsUpdated,
            int baseVertex
        )
        {
        }

        public override void ApplyVertexAttributes(
            VertexDeclaration vertexDeclaration,
            IntPtr ptr,
            int vertexOffset
        )
        {
        }

        #endregion

		#region glBindBuffer Methods

		public override void BindVertexBuffer(Microsoft.Xna.Framework.Graphics.OpenGLDevice.OpenGLVertexBuffer buffer)
		{
		}

		public override void BindIndexBuffer(Microsoft.Xna.Framework.Graphics.OpenGLDevice.OpenGLIndexBuffer buffer)
		{
		}

		#endregion

		#region glSetBufferData Methods

		public override void SetVertexBufferData<T>(
			Microsoft.Xna.Framework.Graphics.OpenGLDevice.OpenGLVertexBuffer handle,
			int elementSizeInBytes,
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount,
			SetDataOptions options
		) {
		}

		public override void SetIndexBufferData<T>(
			Microsoft.Xna.Framework.Graphics.OpenGLDevice.OpenGLIndexBuffer handle,
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount,
			SetDataOptions options
		) {
		}

		#endregion

		#region glGetBufferData Methods

		public override void GetVertexBufferData<T>(
			Microsoft.Xna.Framework.Graphics.OpenGLDevice.OpenGLVertexBuffer handle,
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount,
			int vertexStride
		) {
		}

		public override void GetIndexBufferData<T>(
			Microsoft.Xna.Framework.Graphics.OpenGLDevice.OpenGLIndexBuffer handle,
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount
		) {
		}

		#endregion

		#region glDeleteBuffers Methods

		public override void DeleteVertexBuffer(Microsoft.Xna.Framework.Graphics.OpenGLDevice.OpenGLVertexBuffer buffer)
		{
		}

		public override void DeleteIndexBuffer(Microsoft.Xna.Framework.Graphics.OpenGLDevice.OpenGLIndexBuffer buffer)
		{
		}

		#endregion

		#region glCreateTexture Method

		public override Microsoft.Xna.Framework.Graphics.OpenGLDevice.OpenGLTexture CreateTexture(Type target, SurfaceFormat format, bool hasMipmaps)
		{
			uint handle = 0;
			return new Microsoft.Xna.Framework.Graphics.FakeInternalGraphicsDevice.FakeOpenGLTexture();
		}

		#endregion

		#region glBindTexture Method

		public override void BindTexture(Microsoft.Xna.Framework.Graphics.OpenGLDevice.OpenGLTexture texture)
		{
		}

		#endregion

		#region glDeleteTexture Method

		public override void DeleteTexture(Microsoft.Xna.Framework.Graphics.OpenGLDevice.OpenGLTexture texture)
		{
		}

		#endregion

		#region glReadPixels Method

		/// <summary>
		/// Attempts to read the texture data directly from the FBO using glReadPixels
		/// </summary>
		/// <typeparam name="T">Texture data type</typeparam>
		/// <param name="texture">The texture to read from</param>
		/// <param name="level">The texture level</param>
		/// <param name="data">The texture data array</param>
		/// <param name="rect">The portion of the image to read from</param>
		/// <returns>True if we successfully read the texture data</returns>
		public bool ReadTargetIfApplicable<T>(
			Microsoft.Xna.Framework.Graphics.OpenGLDevice.OpenGLTexture texture,
			int level,
			T[] data,
			Rectangle? rect
		) where T : struct {
			return true;
		}

		#endregion

		#region glGenerateMipmap Method

		public override void GenerateTargetMipmaps(Microsoft.Xna.Framework.Graphics.OpenGLDevice.OpenGLTexture target)
		{
		}

		#endregion

		#region Framebuffer Methods

		public override void BindFramebuffer(uint handle)
		{
		}

		public override void BindReadFramebuffer(uint handle)
		{

		}

		public override void BindDrawFramebuffer(uint handle)
		{
		}

		#endregion

		#region Renderbuffer Methods

		public uint GenRenderbuffer(int width, int height, DepthFormat format)
		{
			return 1;
		}

		public override void DeleteRenderbuffer(uint renderbuffer)
		{
		}

		#endregion

		#region glEnable/glDisable Method

		private void ToggleGLState(Microsoft.Xna.Framework.Graphics.OpenGLDevice.GLenum feature, bool enable)
		{
		}

		#endregion

		#region glClear Method

		public override void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
		{
		}

		#endregion

		#region SetRenderTargets Method

        public virtual void SetRenderTargets(
            RenderTargetBinding[] renderTargets,
            uint renderbuffer,
            DepthFormat depthFormat
        ){
		}

		#endregion

		#region The Faux-Backbuffer

		public class FakeFauxBackbuffer : FauxBackbuffer
		{
			public uint Handle
			{
				get;
				private set;
			}

			public int Width
			{
				get;
				private set;
			}

			public int Height
			{
				get;
				private set;
			}

#if !DISABLE_FAUXBACKBUFFER
			private uint colorAttachment;
			private uint depthStencilAttachment;
			private DepthFormat depthStencilFormat;
			private OpenGLDevice glDevice;
#endif

			public FakeFauxBackbuffer(
				OpenGLDevice device,
				int width,
				int height,
				DepthFormat depthFormat
			)
				: base()
			{
				Width = width;
				Height = height;
#if DISABLE_FAUXBACKBUFFER
				Handle = 0;
#else
				glDevice = device;
				depthStencilFormat = depthFormat;

				Handle = 1;

				if (depthFormat == DepthFormat.None)
				{
					// Don't bother creating a depth/stencil texture.
					depthStencilAttachment = 0;

					return;
				}
#endif
			}

			public override void Dispose()
			{
#if !DISABLE_FAUXBACKBUFFER
				glDevice = null;
				Handle = 0;
#endif
			}

            public override void ResetFramebuffer(
                int width,
                int height,
                DepthFormat depthFormat,
                bool renderTargetBound
            )
			{
				Width = width;
				Height = height;
#if !DISABLE_FAUXBACKBUFFER
				
				if (depthFormat == DepthFormat.None)
				{
					if (depthStencilAttachment != 0)
					{
						depthStencilAttachment = 0;
					}
					return;
				}

				if (depthFormat != depthStencilFormat)
				{
					depthStencilFormat = depthFormat;
				}
#endif
			}
		}

		#endregion
		
	}
}
