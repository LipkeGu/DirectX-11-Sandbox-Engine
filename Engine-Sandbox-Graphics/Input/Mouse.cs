using System;
using System.Linq;
using System.Threading;
using SharpDX;
using SharpDX.DirectInput;

namespace Sandbox.Engine
{
	public class Input : IDisposable
	{
		static Mouse mouse;
		static Keyboard keyboard;
		public delegate void MouseButtonEventHandler(object source, MouseButtonEventArgs args);
		public static event MouseButtonEventHandler MouseButtonDown;

		public delegate void MousePositionEventHandler(object source, MousePositionEventArgs args);
		public static event MousePositionEventHandler MousePosition;

		public delegate void MouseScrollEventHandler(object source, MouseScrollEventArgs args);
		public static event MouseScrollEventHandler MouseScroll;

		public class MouseButtonEventArgs
		{
			public int Button;
			public int Pressed;

			public MouseButtonEventArgs(int button, int pressed)
			{
				Button = button;
				Pressed = pressed;
			}
		}

		public class MouseScrollEventArgs
		{
			public float Delta;

			public MouseScrollEventArgs(float delta)
			{
				Delta = delta;
			}
		}

		public class MousePositionEventArgs
		{
			public float X;
			public float Y;

			public MousePositionEventArgs(float x, float y)
			{
				X = x;
				Y = y;
			}
		}

		DirectInput dinput;
		Thread eventThread = new Thread(eventloop);
		public Input()
		{
			dinput = new DirectInput();
			mouse = new Mouse(dinput);
			keyboard = new Keyboard(dinput);
		}

		public bool Initialize()
		{
			if (mouse == null)
				return false;

			eventThread.Start();
			return true;
		}

		public static void eventloop()
		{
			mouse.Properties.BufferSize = 128;
			mouse.Acquire();

			var posX = (float)0;
			var posY = (float)0;

			var lastPosX = (float)1;
			var lastPosY = (float)1;

			while (true)
			{
				mouse.Poll();
				var datas = mouse.GetBufferedData();
				if (datas.LastOrDefault().IsButton)
				{
					MouseButtonDown?.Invoke(null, new MouseButtonEventArgs(
						(int)datas.LastOrDefault().Offset, datas.LastOrDefault().Value));
				}

				switch (datas.LastOrDefault().Offset)
				{
					case MouseOffset.X:
						if (lastPosX != posX)
							posX = mouse.GetCurrentState().X;

						break;
					case MouseOffset.Z:
						MouseScroll?.Invoke(null, new MouseScrollEventArgs(mouse.GetCurrentState().Z));
						break;
					case MouseOffset.Y:
						if (lastPosY != posY)
							posY = mouse.GetCurrentState().Y;
							
						break;
				}

//				MousePosition?.Invoke(null, new MousePositionEventArgs(posX, posY));
				Thread.Sleep(1);
			}
		}

		public void Dispose()
		{
			mouse.Unacquire();
			dinput.Dispose();
		}
	}
}
