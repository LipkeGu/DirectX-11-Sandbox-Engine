using Sandbox.Engine.Models;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectInput;

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
			WM_MBUTTONDBLCLK = 0x0209,

			WM_KILLFOCUS = 0x0008,
			WM_SETFOCUS = 0x0007,
			WM_MOUSELEAVE = 0x02A3,
			WM_MOUSEHOVER = 0x02A1,

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

		public static Vector2 CursorPosition { get; private set; }

		static Vector3 up = new Vector3(0, 1, 0);
		static Vector3 lookAt;

		Plate floor;

		static float movespeed = 10.07f;
		static float turnspeed = (float)(0.5f / (Math.PI * 10));

		public static Vector2 Rotation = new Vector2(0.0f, 0.9f);

		#region "Renderer Settings"
		public static FillMode FillMode { get; set; } = FillMode.Solid;

		public static CullMode Culling { get; private set; } = CullMode.Front;

		public static bool Antialiased { get; private set; } = false;

		public static bool Multisampling { get; private set; } = false;

		public static bool DepthClip { get; private set; } = false;

		public static bool Scissoring { get; private set; } = false;
		#endregion

		public static Matrix ViewMatrix { get; private set; }

		public static Matrix WorldMatrix { get; private set; }

		public static Matrix ProjectionMatrix { get; private set; }
		public MSG msg;

		string classname = "mainwindow";

		public delegate void CloseEventHandler(object sender, EventArgs e);
		public static event CloseEventHandler CloseRequested;

		public delegate void ResizeEventHandler(object sender, EventArgs e);
		public static event ResizeEventHandler Resize;

		public static bool MiddleMousePressed { get; private set; }
		public static bool LeftShiftKeyPressed { get; private set; }
		public static bool CursorOverClientArea { get; private set; }

		public static int Width;
		public static int Height;
		public static Viewport ViewPort { get; private set; }
		public static Color ClearColor { get; set; } = new Color(96, 96, 96, 1);

		public IntPtr HWnd { get; private set; } = IntPtr.Zero;
		public static SharpDX.Direct3D11.Device GraphicDevice { get; private set; }
		public static SharpDX.Direct3D11.DeviceContext DeviceContext { get; private set; }
		public static SwapChain SwapChain { get; private set; }
		public static Vector3 Position;

		DepthStencilView depthStencilView;
		public static RenderTargetView RenderView { get; private set; }
		string title = string.Empty;

		public Video(string title)
			=> this.title = title;

		void CreateDeviceSwapChainAndContext()
		{
			var swapchainDesc = new SwapChainDescription()
			{
				BufferCount = 2,
				ModeDescription = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
				IsWindowed = false,
				OutputHandle = HWnd,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Discard,
				Usage = Usage.RenderTargetOutput | Usage.BackBuffer,
				Flags = SwapChainFlags.AllowModeSwitch
			};

			var features = new FeatureLevel[] { FeatureLevel.Level_10_1, FeatureLevel.Level_11_0, FeatureLevel.Level_11_1 };

			SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware,
				DeviceCreationFlags.Debug, features, swapchainDesc, out var dev, out var swapChain);

			SwapChain = swapChain;
			SwapChain.ResizeBuffers(swapchainDesc.BufferCount, Width, Height, Format.Unknown, SwapChainFlags.None);

			GraphicDevice = (SharpDX.Direct3D11.Device)dev;
			DeviceContext = GraphicDevice.ImmediateContext;

			SwapChain.SetFullscreenState(true, null);

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

			Resize += (sender, e) =>
			{
				surface.Left = 0;
				surface.Right = (uint)Width;
				surface.Top = 0;
				surface.Bottom = (uint)Height;

				Width = (int)(surface.Right - surface.Left);
				Height = (int)(surface.Bottom - surface.Top);

				ViewPort = new Viewport(0, 0, Width, Height, 0.1f, 1.0f);
			};
			#endregion

			#region "Direct3D Inizialization"

			CreateDeviceSwapChainAndContext();

			using (var backbuffer = SharpDX.Direct3D11.Resource.FromSwapChain<Texture2D>(SwapChain, 0))
				RenderView = new RenderTargetView(GraphicDevice, backbuffer);

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


			DeviceContext.OutputMerger.SetTargets(depthStencilView, RenderView);
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
			Position = new Vector3(Plate.MapSize.X / 2, Plate.MapSize.Z / 2, Plate.MapSize.Y / 2);
			ShowWindow(HWnd, 1);

			return true;
		}

		public void BeginRender()
		{
			using (var rasterizerState = new RasterizerState(GraphicDevice, new RasterizerStateDescription()
			{
				CullMode = Culling,
				FillMode = FillMode,
				IsFrontCounterClockwise = false,
				DepthBias = 1 / ushort.MaxValue,
				SlopeScaledDepthBias = 0.0f,
				DepthBiasClamp = ViewPort.MaxDepth,
				IsDepthClipEnabled = DepthClip,
				IsAntialiasedLineEnabled = Antialiased,
				IsMultisampleEnabled = Multisampling,
				IsScissorEnabled = Scissoring
			}))
				DeviceContext.Rasterizer.State = rasterizerState;

			ProjectionMatrix = Functions.CreateProjectionSpace(Width / Height, 0.1f, Plate.MapSize.Z);
			ViewMatrix = Matrix.LookAtLH(Position, lookAt, up);
			WorldMatrix = Matrix.Multiply(ViewMatrix, ProjectionMatrix);
			DeviceContext.ClearDepthStencilView(depthStencilView,
				DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);

			DeviceContext.ClearRenderTargetView(RenderView, ClearColor);

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
			lookAt.X = (float)Math.Sin(Rotation.Y) + Position.X;
			lookAt.Z = (float)Math.Cos(Rotation.Y) + Position.Z;
			lookAt.Y = Position.Y - Rotation.X;
		}

		public void MouseInput(int button, int pressed)
		{
			switch (button)
			{
				case 12:        // left
					break;
				case 13:        // right
					break;
				case 14:        // middle
					MiddleMousePressed = pressed != 0;
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// MouseInput for Cursor position
		/// </summary>
		/// <param name="x">Position X</param>
		/// <param name="y">Position Y</param>
		public void MouseInput(float x, float y)
			=> CursorPosition = new Vector2(x, y);

		public void MouseInput(float delta)
		{
			if (DeviceContext == null)
				return;

			if (LeftShiftKeyPressed)
				Rotation.X += (float)Math.Sin((delta / turnspeed) / (float)Math.PI);
			else
			{
				if (Position.Y > Plate.MapSize.W)
				{
					if (Position.Y < Plate.MapSize.Z)
						Position.Y -= (float)(delta / movespeed / Math.PI);
					else
						Position.Y = Plate.MapSize.Z - 0.1f;
				}
				else
					Position.Y = Plate.MapSize.W + 0.1f;
			}
		}

		public static void KeyboardKeyPressed(Key key)
		{
			switch (key)
			{
				case Key.W:                        // W
				case Key.Up:                        // Up
					if (Position.X < Plate.MapSize.X && Position.X > 0.1f)
						Position.X += movespeed * (float)Math.Sin(Rotation.Y);
					else
						Position.X = (Position.X < Plate.MapSize.X) ? 0.2f : Plate.MapSize.X - 0.2f;

					if (Position.Z < Plate.MapSize.Y && Position.Z > 0.1f)
						Position.Z += movespeed * (float)Math.Cos(Rotation.Y);
					else
						Position.Z = (Position.Z < Plate.MapSize.Y) ? 0.2f : Plate.MapSize.Y - 0.2f;
					break;
				case Key.S:                        // S
				case Key.Down:
					if (Position.X > 0.1f && Position.X < Plate.MapSize.X)
						Position.X -= movespeed * (float)Math.Sin(Rotation.Y);
					else
						Position.X = (Position.X < Plate.MapSize.X) ? 0.2f : Plate.MapSize.X - 0.2f;

					if (Position.Z > 0.1f && Position.Z < Plate.MapSize.Y)
						Position.Z -= movespeed * (float)Math.Cos(Rotation.Y);
					else
						Position.Z = (Position.Z < Plate.MapSize.Y) ? 0.2f : Plate.MapSize.Y - 0.2f;
					break;
				case Key.A:                        // A
				case Key.Left:                        // Left
					if (Position.X < Plate.MapSize.X && Position.X > 0.1f)
						Position.X -= movespeed * (float)Math.Sin(Rotation.Y + Math.PI / 2);
					else
						Position.X = (Position.X < Plate.MapSize.X) ? 0.2f : Plate.MapSize.X - 0.2f;

					if (Position.Z < Plate.MapSize.Y && Position.Z > 0.1f)
						Position.Z -= movespeed * (float)Math.Cos(Rotation.Y + Math.PI / 2);
					else
						Position.Z = (Position.Z < Plate.MapSize.Y) ? 0.2f : Plate.MapSize.Y - 0.2f;
					break;
				case Key.D:                        // D
				case Key.Right:                        // Right
					if (Position.X < Plate.MapSize.X && Position.X > 0.1f)
						Position.X += movespeed * (float)Math.Sin(Rotation.Y + Math.PI / 2);
					else
						Position.X = (Position.X < Plate.MapSize.X) ? 0.2f : Plate.MapSize.X - 0.2f;

					if (Position.Z < Plate.MapSize.Y && Position.Z > 0.1f)
						Position.Z += movespeed * (float)Math.Cos(Rotation.Y + Math.PI / 2);
					else
						Position.Z = (Position.Z < Plate.MapSize.Y) ? 0.2f : Plate.MapSize.Y - 0.2f;
					break;
				case Key.Q:
					Rotation.Y -= turnspeed;    // Q
					break;
				case Key.E:
					Rotation.Y += turnspeed;    // E
					break;
				case Key.Y:
					Rotation.X -= turnspeed;    // Y
					break;
				case Key.X:
					Rotation.X += turnspeed;    // X
					break;
				case Key.LeftShift:
					LeftShiftKeyPressed = true;
					break;
				case Key.F9:
					switch (FillMode)
					{
						case FillMode.Wireframe:
							FillMode = SharpDX.Direct3D11.FillMode.Solid;
							Antialiased = true;
							break;
						case FillMode.Solid:
							FillMode = SharpDX.Direct3D11.FillMode.Wireframe;
							Antialiased = false;
							break;
						default:
							break;
					}
					break;
				case Key.F11:
					switch (Culling)
					{
						case CullMode.Back:
							Culling = CullMode.Front;
							break;
						case CullMode.Front:
							Culling = CullMode.None;
							break;
						case CullMode.None:
							Culling = CullMode.Back;
							break;
					}
					break;
				case Key.F8:
					Antialiased = Antialiased ? false : true;
					break;
				case Key.F7:
					Multisampling = Multisampling ? false : true;
					break;
				case Key.F6:
					Scissoring = Scissoring ? false : true;
					break;
				case Key.F5:
					DepthClip = DepthClip ? false : true;
					break;
				default:
					break;
			}
		}

		public static void KeyboardKeyReleased(Key key)
		{
			switch (key)
			{
				case Key.LeftShift:
					LeftShiftKeyPressed = false;
					break;
			}
		}

		public void Dispose()
		{
			RenderView.Dispose();
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
					SwapChain.SetFullscreenState(false, null);
					CloseRequested?.Invoke(null, new EventArgs());
					DestroyWindow(hWnd);
					PostQuitMessage();
					break;
				case WindowEvent.WM_SETTEXT:
					break;
				case WindowEvent.WM_SIZE:
					Resize?.Invoke(null, new EventArgs());
					break;
				case WindowEvent.WM_MOUSELEAVE:
					CursorOverClientArea = false;
					break;
				case WindowEvent.WM_MOUSEHOVER:
					CursorOverClientArea = true;
					break;
				case WindowEvent.WM_KEYDOWN:
					var key = Key.Unknown;

					switch (wParam.ToInt32())
					{
						case 87:
							key = Key.W;
							break;
						case 65:
							key = Key.A;
							break;
						case 83:
							key = Key.S;
							break;
						case 68:
							key = Key.D;
							break;
						case 16:
							key = Key.LeftShift;
							break;
						case 81:
							key = Key.Q;
							break;
						case 69:
							key = Key.E;
							break;
						case 122:
							key = Key.F11;
							break;
						case 116:
							key = Key.F5;
							break;
						case 120:
							key = Key.F9;
							break;
						case 119:
							key = Key.F8;
							break;
						case 118:
							key = Key.F7;
							break;
						case 117:
							key = Key.F6;
							break;
						default:
							break;
					}

					KeyboardKeyPressed(key);
					break;
				case WindowEvent.WM_KEYUP:
					var _key = Key.Unknown;

					switch (wParam.ToInt32())
					{
						case 16:
							_key = Key.LeftShift;
							break;
						default:
							break;
					}

					KeyboardKeyReleased(_key);
					break;
				default:
					break;
			}

			return DefWindowProc(hWnd, msg, wParam, lParam);
		}
	}
}
