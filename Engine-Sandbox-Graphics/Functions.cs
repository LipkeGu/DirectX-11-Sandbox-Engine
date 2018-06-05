using Sandbox.Engine.Graphics;
using SharpDX;
using System;
using System.Diagnostics;

namespace Sandbox.Engine
{
	public static class Functions
	{
		public static SharpDX.Color ToRGBA(this System.Drawing.Color color)
			=> new SharpDX.Color(color.R,color.G,color.B,color.A);

		public static Matrix CreateProjectionSpace(float aspectRatio, float nearZ, float farZ)
			=> Matrix.PerspectiveFovLH((float) Math.PI / 4.0f, aspectRatio, 0.0f, ushort.MaxValue);

		public static void LOG(string message, bool waitForInteraction = false)
		{
			if (Debugger.IsAttached)
			{
				Debug.Print(message);

				if (waitForInteraction)
					Debugger.Break();
			}
		}

		private static void PickingTriangle(Vector2 mouseLocation)
		{
			var near = new Vector3(mouseLocation.X, mouseLocation.Y, 0);
			var far = new Vector3(mouseLocation.X, mouseLocation.Y, ushort.MaxValue);

			Vector3.Unproject(near, Video.ViewPort.X, Video.ViewPort.Y,
				Video.ViewPort.Width, Video.ViewPort.Height, Video.ViewPort.MinDepth,
					Video.ViewPort.MaxDepth, Video.WorldMatrix);

			Vector3.Unproject(far, Video.ViewPort.X, Video.ViewPort.Y,
				Video.ViewPort.Width, Video.ViewPort.Height, Video.ViewPort.MinDepth,
					Video.ViewPort.MaxDepth, Video.WorldMatrix);
		}
	}
}
