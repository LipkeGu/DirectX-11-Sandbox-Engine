using System;
using System.Threading;
using Sandbox.Engine.Graphics;

namespace Sandbox.Engine
{
	public class Engine
	{
		public Video Video { get; private set; }
		public Input Input { get; private set; }

		string title = string.Empty;

		public Engine(string title = "Main WIndow")
		{
			this.title = title;
			Video = new Video(this.title);
			Input = new Input();

			Input.MouseButtonDown += (sender, e) =>
			{
				Video.MouseInput(e.Button, e.Pressed);
			};

			Input.MouseScroll += (sender, e) =>
			{
				Video.MouseInput(e.Delta);
			};

			Input.MousePosition += (sender, e) =>
			{
				Video.MouseInput(e.X, e.Y);
			};
		}
		public bool Initialize()
		{
			if (!Video.Initialize())
			{
				Video.MessageBox("Video initialization failed!", title);
				return false;
			}

			if (!Input.Initialize())
			{
				Video.MessageBox("Input initialization failed!", title);
				return false;
			}
			return true;
		}

		public void Run()
		{
			if (!Initialize())
				return;

			GC.Collect();

			while ((uint)Video.WindowEvent.WM_QUIT != Video.msg.message)
			{
				if (Video.PeekMessage(ref Video.msg, IntPtr.Zero, 0, 0, 1))
				{
					Video.TranslateMessage(ref Video.msg);
					Video.DispatchMessage(ref Video.msg);
				}
				else
				{
					Video.Update();
					Video.BeginRender(SharpDX.Color.Black);
					Video.EndRender();

					Thread.Sleep(1);
				}
			}
		}



		public void Dispose()
		{
			Input.Dispose();
			Video.Dispose();
		}
	}
}
