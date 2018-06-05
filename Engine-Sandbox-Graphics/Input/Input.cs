using System;
using System.Linq;
using System.Threading;
using Sandbox.Engine.Graphics;
using SharpDX;
using SharpDX.DirectInput;

namespace Sandbox.Engine
{
	public partial class Input : IDisposable
	{
		static Mouse mouse;

		static bool running;
		public delegate void MouseButtonEventHandler(object source, MouseButtonEventArgs args);
		public static event MouseButtonEventHandler MouseButtonDown;

		public delegate void MousePositionEventHandler(object source, MousePositionEventArgs args);
		public static event MousePositionEventHandler MousePosition;

		public delegate void MouseScrollEventHandler(object source, MouseScrollEventArgs args);
		public static event MouseScrollEventHandler MouseScroll;

		DirectInput dinput;
		Thread eventThread = new Thread(Eventloop);
		public Input()
		{
			Video.CloseRequested += (sender, e) =>
			{
				running = false;
			};

			dinput = new DirectInput();
			mouse = new Mouse(dinput);
		}

		public bool Initialize()
		{
			if (mouse == null)
				return false;

			running = true;
			eventThread.Start();

			return true;
		}

		private static void Handle_Mouse_Position(MouseUpdate data)
		{
			var posX = (float)0;
			var posY = (float)0;

			switch (data.Offset)
			{
				case MouseOffset.X:
					posX = mouse.GetCurrentState().X;
					break;
				case MouseOffset.Z:
					MouseScroll?.Invoke(null, new MouseScrollEventArgs(mouse.GetCurrentState().Z));
					break;
				case MouseOffset.Y:
					posY = mouse.GetCurrentState().Y;
					break;
			}

			MousePosition?.Invoke(null, new MousePositionEventArgs(posX, posY));
		}
		private static void Handle_Mouse_Buttons(MouseUpdate data)
		{
			if (data.IsButton)
				MouseButtonDown?.Invoke(null, new MouseButtonEventArgs((int)data.Offset, data.Value));
		}


		private static void Eventloop()
		{
			mouse.Properties.BufferSize = 128;
			mouse.Acquire();

			while (running)
			{
				mouse.Poll();
				
				var mouseData = mouse.GetBufferedData().FirstOrDefault();
				Handle_Mouse_Position(mouseData);
				Handle_Mouse_Buttons(mouseData);
				
				Thread.Sleep(1);
			}

			mouse.Unacquire();
		}

		public void Dispose()
		{
			mouse.Dispose();
			dinput.Dispose();
		}
	}
}
