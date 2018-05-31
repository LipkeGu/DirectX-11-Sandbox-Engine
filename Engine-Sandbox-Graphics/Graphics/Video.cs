using Sandbox.Engine.Models;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
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
		static extern IntPtr _MsgeBox(
			[In] IntPtr hwnd,
			[In] string text,
			[In] string message,
			[In] uint flags
			);

		public static void MessageBox(string message, string title, uint flags = (uint)0x00000010L)
		{
#if DEBUG
			Functions.LOG(message, flags == (uint)0x00000010L);
#endif
			_MsgeBox(IntPtr.Zero, message, title, flags);
		}

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
		private long windowStyle = 0x00000000L | 0x20000000L | 0x00080000L
			| 0x00C00000L | 0x00020000L | 0x00040000L;

		private static System.Drawing.Point cursorPosition;
		private static System.Drawing.Point lastCursorPosition;

		static Vector3 position;
		static Vector3 up = new Vector3(0, 1, 0);
		static Vector3 lookAt;

		Plate floor;

		static float movespeed = 10.07f;
		static float turnspeed = (float)(0.5f / (Math.PI * 10));

		static Vector2 rotation = new Vector2(0.0f, 0.9f);

		public static FillMode FillMode { get; set; } = FillMode.Solid;

		public static Matrix ViewMatrix { get; private set; }

		public static Matrix ProjectionMatrix { get; private set; }
		public MSG msg;

		string classname = "mainwindow";


		public static bool MiddleMousePressed { get; private set; }
		
		public static int Width;
		public static int Height;
		public static Viewport ViewPort { get; set; }

		public IntPtr HWnd { get; private set; } = IntPtr.Zero;
		public static SharpDX.Direct3D11.Device GraphicDevice { get; private set; }
		public static DeviceContext DeviceContext { get; private set; }
		public SwapChain SwapChain { get; private set; }

		DepthStencilView depthStencilView;
		RenderTargetView renderview;
		string title = string.Empty;

		public Video(string title)
			=> this.title = title;

		void CreateDeviceSwapChainAndContext()
		{
			var swapchainDesc = new SwapChainDescription()
			{
				BufferCount = 2,
				ModeDescription = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
				IsWindowed = true,
				OutputHandle = HWnd,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Discard,
				Usage = Usage.RenderTargetOutput | Usage.BackBuffer,
				Flags = SwapChainFlags.AllowModeSwitch
			};

			var features = new FeatureLevel[] { FeatureLevel.Level_10_1, FeatureLevel.Level_11_0, FeatureLevel.Level_11_1};
			
			SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware,
				DeviceCreationFlags.None, features, swapchainDesc, out var dev, out var swapChain);

			SwapChain = swapChain;
			SwapChain.ResizeBuffers(swapchainDesc.BufferCount, Width, Height, Format.Unknown, SwapChainFlags.None);

			GraphicDevice = (SharpDX.Direct3D11.Device)dev;
			DeviceContext = GraphicDevice.ImmediateContext;

			ViewPort = new Viewport(0, 0, Width, Height, 0.1f, 1.0f);
		}
		
		/// <summary>
		/// Initialize basic Window and Rendering environment. 
		/// </summary>
		/// <returns></returns>
		public bool Initialize()
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
			surface.Right = (uint)Width;
			surface.Top = 0;
			surface.Bottom = (uint)Height;

			var posX = (GetSystemMetrics(0) / 2 - Width / 2);
			var posY = (GetSystemMetrics(1) / 2 - Height / 2);

			RegisterClassEx(ref wc);

			HWnd = CreateWindowEx(0, classname, title, (uint)windowStyle,
				posX, posY, Width, Height, IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

			if (!AdjustWindowRect(ref surface, (uint)windowStyle, IntPtr.Zero))
				return false;

			Width = (int)(surface.Right - surface.Left);
			Height = (int)(surface.Bottom - surface.Top);
			#endregion

			#region "Direct3D Inizialization"

			CreateDeviceSwapChainAndContext();
			
			using (var backbuffer = SharpDX.Direct3D11.Resource.FromSwapChain<Texture2D>(SwapChain, 0))
				renderview = new RenderTargetView(GraphicDevice, backbuffer);

			#region "Depth & STencil Buffer"
			var depthBuffer = new Texture2D(GraphicDevice, new Texture2DDescription()
			{
				Format = Format.D24_UNorm_S8_UInt,
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

			depthStencilView = new DepthStencilView(GraphicDevice, depthBuffer, new DepthStencilViewDescription
			{
				Dimension = SwapChain.Description.SampleDescription.Count > 1
				|| SwapChain.Description.SampleDescription.Quality > 0
				? DepthStencilViewDimension.Texture2DMultisampled
				: DepthStencilViewDimension.Texture2D
			});

		

			DeviceContext.OutputMerger.SetTargets(depthStencilView, renderview);
			DeviceContext.OutputMerger.DepthStencilState = new DepthStencilState(GraphicDevice,
				new DepthStencilStateDescription
			{
				IsDepthEnabled = true,
				DepthComparison = Comparison.LessEqual,
				DepthWriteMask = DepthWriteMask.All,
				IsStencilEnabled = true,
				StencilReadMask = 0xff,
				StencilWriteMask = 0xff,
				FrontFace = new DepthStencilOperationDescription
				{
					Comparison = Comparison.LessEqual,
					PassOperation = StencilOperation.Keep,
					FailOperation = StencilOperation.Keep,
					DepthFailOperation = StencilOperation.Increment
				},
				BackFace = new DepthStencilOperationDescription
				{
					Comparison = Comparison.LessEqual,
					PassOperation = StencilOperation.Keep,
					FailOperation = StencilOperation.Keep,
					DepthFailOperation = StencilOperation.Decrement
				}


			});

			#endregion

			#endregion
			floor = new Plate();

			ShowWindow(HWnd, 1);

			return true;
		}

		public void BeginRender(SharpDX.Color clearcolor)
		{
			using (var rasterizerState = new RasterizerState(GraphicDevice, new RasterizerStateDescription()
			{
				CullMode = CullMode.Front,
				FillMode = FillMode,
				IsFrontCounterClockwise = false,
				DepthBias = 0,
				SlopeScaledDepthBias = 0.0f,
				DepthBiasClamp = 0.0f,
				IsDepthClipEnabled = true,
				IsAntialiasedLineEnabled = false,
				IsMultisampleEnabled = false,
				IsScissorEnabled = false
			}))
				DeviceContext.Rasterizer.State = rasterizerState;

			ProjectionMatrix = Functions.CreateProjectionSpace(Width / Height, 0.1f, ushort.MaxValue);
			ViewMatrix = Matrix.LookAtLH(position, lookAt, up);

			DeviceContext.ClearDepthStencilView(depthStencilView, 
				DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
			DeviceContext.ClearRenderTargetView(renderview, clearcolor);

			floor.Render();
		}

		public void EndRender()
		{
			SwapChain.Present(1, PresentFlags.None);
			ProjectionMatrix = Matrix.Identity;
		}

		public void Update()
		{
			DeviceContext.Rasterizer.SetViewport(ViewPort);
			lookAt.X = (float)Math.Sin(rotation.Y) + position.X;
			lookAt.Z = (float)Math.Cos(rotation.Y) + position.Z;
			lookAt.Y = position.Y - rotation.X;
		}

		public void MouseInput(int button, int pressed)
		{
			switch (button)
			{
				case 12:        // left
					if (pressed != 0)
						position.Y += movespeed - turnspeed; // fly up
					break;
				case 13:        // right
					if (pressed != 0)
						position.Y -= movespeed - turnspeed;  // fly down;
					break;
				case 14:        // middle
					MiddleMousePressed = pressed != 0;
					break;
				default:
					break;
			}
		}

		public void MouseInput(float x, float y)
		{
			rotation.Y -= x * 0.001f;

			rotation.X -= y * 0.001f;
		}

		public void MouseInput(float delta)
		{
			position.Y += (float)(delta / movespeed / Math.PI);
		}

		public void Dispose()
		{
			renderview.Dispose();
			DeviceContext.ClearState();
			DeviceContext.Flush();
			GraphicDevice.Dispose();
			DeviceContext.Dispose();
			SwapChain.Dispose();

			floor.Dispose();

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
				case WindowEvent.WM_MOUSEMOVE:
					if (DeviceContext == null)
						break;
					break;
				case WindowEvent.WM_KEYUP:
					if (DeviceContext == null)
						break;
					break;
				case WindowEvent.WM_KEYDOWN:
					if (DeviceContext == null)
						break;

					switch (wParam.ToInt32())
					{
						case 87:                        // W
						case 38:                        // Up
							position.X += movespeed * (float)Math.Sin(rotation.Y);
							position.Z += movespeed * (float)Math.Cos(rotation.Y);
							break;
						case 83:                        // S
						case 40:                        // Down
							position.X -= movespeed * (float)Math.Sin(rotation.Y);
							position.Z -= movespeed * (float)Math.Cos(rotation.Y);
							break;
						case 65:                        // A
						case 37:                        // Left
							position.X -= movespeed * (float)Math.Sin(rotation.Y + Math.PI / 2);
							position.Z -= movespeed * (float)Math.Cos(rotation.Y + Math.PI / 2);
							break;
						case 68:                        // D
						case 39:                        // Right
							position.X += movespeed * (float)Math.Sin(rotation.Y + Math.PI / 2);
							position.Z += movespeed * (float)Math.Cos(rotation.Y + Math.PI / 2);
							break;
						case 81:
							rotation.Y -= turnspeed;	// Q
							break;
						case 69:
							rotation.Y += turnspeed;	// E
							break;
						case 89:
							rotation.X -= turnspeed;    // Y
							break;
						case 88:
							rotation.X += turnspeed;    // X
							break;
						case 120:                       // F9
							switch (FillMode)
							{
								case FillMode.Wireframe:
									FillMode = SharpDX.Direct3D11.FillMode.Solid;
									break;
								case FillMode.Solid:
									FillMode = SharpDX.Direct3D11.FillMode.Wireframe;
									break;
								default:
									break;
							}
							break;
						default:
							Console.WriteLine(wParam.ToInt32());
							break;
					}
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
