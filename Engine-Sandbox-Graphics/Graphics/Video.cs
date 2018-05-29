using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Sandbox.Engine.Graphics
{
	public class Video : IDisposable
	{
		#region "DLL Imports"
		[DllImport("user32.dll", EntryPoint = "BeginPaint")]
		public static extern IntPtr BeginPaint([In] IntPtr hWnd, ref PAINTSTRUCT lpPaint);

		[DllImport("user32.dll", EntryPoint = "EndPaint")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EndPaint([In] IntPtr hWnd, [In] ref PAINTSTRUCT lpPaint);

		[DllImport("user32.dll", EntryPoint = "AdjustWindowRect")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool AdjustWindowRect(
			ref RECT lpRect,
			[In] uint dwStyle,
			[In] IntPtr bMenu
		);

		[DllImport("user32.dll", EntryPoint = "GetWindowRect")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowRect([In] IntPtr hwnd, ref RECT lpRect);

		[DllImport("user32.dll", EntryPoint = "UpdateWindow")]
		private static extern bool UpdateWindow(IntPtr hWnd);

		[DllImport("user32.dll", EntryPoint = "ShowWindow")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll", EntryPoint = "SetWindowText")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowText([In] IntPtr hwnd,
			[MarshalAs(UnmanagedType.LPStr)]
			string title
			);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "DestroyWindow")]
		public static extern bool DestroyWindow(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "CreateWindowEx")]
		public static extern IntPtr CreateWindowEx(
			uint dwExStyle,
			[MarshalAs(UnmanagedType.LPStr)]
			string lpClassName,
			[MarshalAs(UnmanagedType.LPStr)]
			string lpWindowName,
			uint dwStyle,
			int x,
			int y,
			int width,
			int height,
			IntPtr hWndParent,
			IntPtr hMenu,
			IntPtr hInstance,
			IntPtr lpParam
		);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterClassEx")]
		public static extern UInt16 RegisterClassEx([In] ref WNDCLASSEX lpWndClass);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "UnregisterClassA")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UnRegisterClass(
			[MarshalAs(UnmanagedType.LPStr)]
			[In] string lpClassName,
			[In] IntPtr hInstance
		);

		[DllImport("user32.dll", EntryPoint = "DefWindowProcA")]
		public static extern IntPtr DefWindowProc(
			[In] IntPtr hWnd,
			[In] WindowEvent msg,
			[In] IntPtr wParam,
			[In] IntPtr lParam
		);

		[DllImport("kernel32.dll", EntryPoint = "GetModuleHandleA")]
		public static extern IntPtr GetModuleHandle([In] IntPtr hWnd);

		[DllImport("user32.dll", EntryPoint = "PostQuitMessage")]
		public static extern void PostQuitMessage([In] int exitCode = 0);

		[DllImport("user32.dll", EntryPoint = "LoadCursorW")]
		public static extern IntPtr LoadCursor(IntPtr hInstance, [In] int lpCursorName);

		[DllImport("user32.dll", EntryPoint = "LoadIconW")]
		public static extern IntPtr LoadIcon(IntPtr hInstance, [In] int lpIconName);

		[DllImport("user32.dll", EntryPoint = "GetCursorPos")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

		[DllImport("user32.dll", EntryPoint = "ScreenToClient")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ScreenToClient([In] IntPtr hwnd, ref System.Drawing.Point lpPoint);

		[DllImport("user32.dll", EntryPoint = "TranslateMessage")]
		public static extern bool TranslateMessage([In] ref MSG lpMsg);

		[DllImport("user32.dll", EntryPoint = "DispatchMessage")]
		public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

		[DllImport("user32.dll", EntryPoint = "MessageBoxA")]
		public static extern IntPtr MessageBox(
			[In] IntPtr hwnd,
			[In] string text,
			[In] string message,
			[In] uint flags
			);

		[DllImport("user32.dll", EntryPoint = "PeekMessageA")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool PeekMessage(
			ref MSG pmsg,
			[In] IntPtr hwnd,
			[In] uint wMsgFilterMin,
			[In] uint wMsgFilterMax,
			[In] uint wRemoveMsg
		);

		[DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
		public static extern int GetSystemMetrics([In] int nIndex);

		public struct PAINTSTRUCT
		{
			public IntPtr hdc;
			public bool fErase;
			public RECT rcPaint;
			public bool fRestore;
			public bool fIncUpdate;
			public byte[] rgbReserved;
		}

		public struct RECT
		{
			/// <summary>
			/// Left is usally used for X,
			/// </summary>
			public uint Left;

			/// <summary>
			/// Top is usally used for Y,
			/// </summary>
			public uint Top;
			/// <summary>
			/// Right is usally used for Width,
			/// </summary>
			public uint Right;
			/// <summary>
			/// Bottom is usally used for Height,
			/// </summary>
			public uint Bottom;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct WNDCLASSEX
		{
			[MarshalAs(UnmanagedType.U4)]
			public int cbSize;
			[MarshalAs(UnmanagedType.U4)]
			public int style;
			public IntPtr lpfnWndProc;
			public int cbClsExtra;
			public int cbWndExtra;
			public IntPtr hInstance;
			public IntPtr hIcon;
			public IntPtr hCursor;
			public IntPtr hbrBackground;
			[MarshalAs(UnmanagedType.LPStr)]
			public string lpszMenuName;
			[MarshalAs(UnmanagedType.LPStr)]
			public string lpszClassName;
			public IntPtr hIconSm;
		}

		public struct MSG
		{
			public IntPtr hwnd;
			public uint message;
			public uint wParam;
			public long lParam;
			public long time;
			public long pt;
			public long lPrivate;
		}

		public enum WindowEvent
		{
			WM_PAINT = 0x000F,
			WM_CLOSE = 0x0010,
			WM_QUIT = 0x0012,
			WM_DESTROY = 0x0002,
			WM_MOVE = 0x0003,
			WM_SETTEXT = 0x000C,
			WM_SIZE = 0x0005,

			// Tastatur Events
			WM_CHAR = 0x0102,
			WM_KEYDOWN = 0x0100,
			WM_KEYUP = 0x0101,

			// Maus events
			WM_MOUSEHWHEEL = 0x020E,
			WM_MOUSEMOVE = 0x0200,

			// Linke Maustaste
			WM_LBUTTONDOWN = 0x0201,
			WM_LBUTTONUP = 0x0202,
			WM_LBUTTONDBLCLK = 0x0203,

			// Rechte Maustase...
			WM_RBUTTONDOWN = 0x0204,
			WM_RBUTTONUP = 0x0205,
			WM_RBUTTONDBLCLK = 0x0206,

			// Mittlere Maustaste
			WM_MBUTTONDOWN = 0x0207,
			WM_MBUTTONUP = 0x0208,
			WM_MBUTTONDBLCLK = 0x0209
		}
		#endregion
		private WNDCLASSEX wc;
		private static PAINTSTRUCT ps;
		private static RECT surface;
		private delegate IntPtr __WndProc(IntPtr hWnd, WindowEvent msg, IntPtr wParam, IntPtr lParam);
		private readonly IntPtr hInstance = IntPtr.Zero;
		private readonly __WndProc delegatedWndProc = _wndProc;
		private long windowStyle = 0x00000000L | 0x20000000L | 0x00080000L | 0x00C00000L | 0x00020000L | 0x00040000L;

		public MSG msg;

		string classname = "mainwindow";


		private int Width;
		private int Height;

		public IntPtr HWnd { get; private set; } = IntPtr.Zero;

		readonly bool enableVSync = false;

		#region "Shader Sources"
		string ShaderSrc = @"
		struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

float4x4 worldViewProj;

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = mul(input.pos, worldViewProj);
	output.col = input.col;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return input.col;
}";
		#endregion

		SharpDX.Direct3D11.Device device;
		SwapChain swapChain;
		DepthStencilView depthView;
		RenderTargetView renderview;
		PixelShader pixelShader;
		VertexShader vertexShader;
		ShaderSignature shaderSignature;
		DeviceContext context;
		Matrix view;
		Matrix proj;
		SharpDX.Direct3D11.Buffer constantBuffer;
		SharpDX.Direct3D11.Buffer vertexBuffer;


		public Video() { }

		public bool Initialize(ref Vector4[] vertices, bool fullscreen = true)
		{
			Width = GetSystemMetrics(0);
			Height = GetSystemMetrics(1);
			
			#region "Window Initialization"
			wc = new WNDCLASSEX()
			{
				cbSize = Marshal.SizeOf(typeof(WNDCLASSEX)),
				style = 1 | 2,
				hbrBackground = (IntPtr)1 + 1,
				cbClsExtra = 0,
				cbWndExtra = 0,
				hInstance = GetModuleHandle(HWnd),
				hCursor = LoadCursor(hInstance, 32512),
				lpszMenuName = null,
				lpszClassName = classname,
				lpfnWndProc = Marshal.GetFunctionPointerForDelegate(delegatedWndProc)
			};

			surface.Left = 0;
			surface.Right = (uint)this.Width;
			surface.Top = 0;
			surface.Bottom = (uint)this.Height;

			var posX = (GetSystemMetrics(0) / 2 - this.Width / 2);
			var posY = (GetSystemMetrics(1) / 2 - this.Height / 2);

			RegisterClassEx(ref wc);

			HWnd = CreateWindowEx(0, classname, string.Empty, (uint)windowStyle,
				posX, posY, this.Width, this.Height, IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

			if (!AdjustWindowRect(ref surface, (uint)windowStyle, IntPtr.Zero))
				throw new InvalidOperationException("Window adjustment failed!");

			Width = (int)(surface.Right - surface.Left);
			Height = (int)(surface.Bottom - surface.Top);
			#endregion

			#region "Direct3D Inizialization"
			var desc = new SwapChainDescription()
			{
				BufferCount = 1,
				ModeDescription = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
				IsWindowed = false,
				OutputHandle = HWnd,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Discard,
				Usage = Usage.RenderTargetOutput
			};

			SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware,
				DeviceCreationFlags.None, desc, out device, out swapChain);

			context = device.ImmediateContext;

			#region "Shaders & VertexBuffer"


			using (var vertexShaderByteCode = ShaderBytecode.Compile(ShaderSrc, "VS", "vs_4_0"))
			{
				if (vertexShaderByteCode.Bytecode == null)
				{
					MessageBox(IntPtr.Zero, "VertexShader compilation failed!", "Shader Compiler", (uint)0x00000010L);
					return false;
				}

				vertexShader = new VertexShader(device, vertexShaderByteCode);

				using (var pixelShaderByteCode = ShaderBytecode.Compile(ShaderSrc, "PS", "ps_4_0"))
				{
					if (pixelShaderByteCode.Bytecode == null)
					{
						MessageBox(IntPtr.Zero, "PixelShader compilation failed!", "Shader Compiler", (uint)0x00000010L);
						return false;
					}

					pixelShader = new PixelShader(device, pixelShaderByteCode);
				}

				shaderSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
			}

			using (var layout = new InputLayout(device, shaderSignature, new[] {
				new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
				new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
			}))
			{
				vertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, vertices);

				constantBuffer = new SharpDX.Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(),
					ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

				context.InputAssembler.InputLayout = layout;
			}

			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vector4>() * 2, 0));

			context.VertexShader.SetConstantBuffer(0, constantBuffer);
			context.VertexShader.Set(vertexShader);
			context.PixelShader.Set(pixelShader);
			#endregion

			view = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);
			proj = Matrix.Identity;

			swapChain.ResizeBuffers(desc.BufferCount, Width, Height, Format.Unknown, SwapChainFlags.None);

			using (var backbuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0))
				renderview = new RenderTargetView(device, backbuffer);

			#region "Depth Buffer"
			var depthBuffer = new Texture2D(device, new Texture2DDescription()
			{
				Format = Format.D32_Float_S8X24_UInt,
				ArraySize = 1,
				MipLevels = 1,
				Width = Width,
				Height = Height,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.DepthStencil,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None
			});

			depthView = new DepthStencilView(device, depthBuffer);
			#endregion

			context.Rasterizer.SetViewport(new Viewport(0, 0, Width, Height, 0.0f, 1.0f));
			context.OutputMerger.SetTargets(depthView, renderview);
			#endregion

			ShowWindow(HWnd, 1);

			return true;
		}

		public void BeginRender()
		{
			proj = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, Width / Height, 0.1f, 100.0f);
			var viewProj = Matrix.Multiply(view, proj);

			context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
			context.ClearRenderTargetView(renderview, Color4.Black);

			var worldViewProj = Matrix.RotationX(1) * Matrix.RotationY(1 * 2) * Matrix.RotationZ(1 * .7f) * viewProj;
			worldViewProj.Transpose();
			
			context.UpdateSubresource(ref worldViewProj, constantBuffer);
			context.Draw(36, 0);
		}

		public void EndRender()
		{
			swapChain.Present(enableVSync ? 1 : 0, PresentFlags.None);
		}

		public void Dispose()
		{
			vertexShader.Dispose();
			pixelShader.Dispose();
			vertexShader.Dispose();
			constantBuffer.Dispose();
			renderview.Dispose();
			context.ClearState();
			context.Flush();
			device.Dispose();
			context.Dispose();
			swapChain.Dispose();
			UnRegisterClass(classname, hInstance);
		}

		private static IntPtr _wndProc(IntPtr hWnd, WindowEvent msg, IntPtr wParam, IntPtr lParam)
		{
			switch (msg)
			{
				case WindowEvent.WM_PAINT:
					BeginPaint(hWnd, ref ps);
					EndPaint(hWnd, ref ps);
					break;
				case WindowEvent.WM_DESTROY:
				case WindowEvent.WM_CLOSE:
					DestroyWindow(hWnd);
					PostQuitMessage();
					break;
				case WindowEvent.WM_LBUTTONDOWN:
					break;
				case WindowEvent.WM_RBUTTONDOWN:
					break;
				case WindowEvent.WM_MBUTTONDOWN:
					break;
				case WindowEvent.WM_MOUSEMOVE:
					break;
				case WindowEvent.WM_KEYUP:
					break;
				case WindowEvent.WM_KEYDOWN:
					break;
				case WindowEvent.WM_SETTEXT:
					break;
				case WindowEvent.WM_SIZE:
					break;
				default:
					break;
			}

			return DefWindowProc(hWnd, msg, wParam, lParam);
		}


	}
}
