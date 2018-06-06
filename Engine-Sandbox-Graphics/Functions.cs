using Sandbox.Engine.Graphics;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace Sandbox.Engine
{
	public static class Functions
	{
		public static SharpDX.Color ToRGBA(this System.Drawing.Color color)
			=> new SharpDX.Color(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

		public static Matrix CreateProjectionSpace(float aspectRatio, float nearZ, float farZ)
			=> Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, aspectRatio, 0.0f, ushort.MaxValue);

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
			var far = new Vector3(mouseLocation.X, mouseLocation.Y, 100.0f);

			Vector3.Unproject(near, Video.ViewPort.X, Video.ViewPort.Y,
				Video.ViewPort.Width, Video.ViewPort.Height, Video.ViewPort.MinDepth,
					Video.ViewPort.MaxDepth, Video.WorldMatrix);

			Vector3.Unproject(far, Video.ViewPort.X, Video.ViewPort.Y,
				Video.ViewPort.Width, Video.ViewPort.Height, Video.ViewPort.MinDepth,
					Video.ViewPort.MaxDepth, Video.WorldMatrix);

			var dir = near - far;
		}

		public static Texture2D LoadTexture(string filename)
		{
			var bitmap = new Bitmap(filename);

			if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
				bitmap = bitmap.Clone(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.Format32bppArgb);

			var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, 
				bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

			var ret = new Texture2D(Video.GraphicDevice, new Texture2DDescription()
			{
				Width = bitmap.Width,
				Height = bitmap.Height,
				ArraySize = 1,
				BindFlags = BindFlags.ShaderResource,
				Usage = ResourceUsage.Immutable,
				CpuAccessFlags = CpuAccessFlags.None,
				Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
				MipLevels = 1,
				OptionFlags = ResourceOptionFlags.None,
				SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
			}, new DataRectangle(data.Scan0, data.Stride));

			bitmap.UnlockBits(data);
			bitmap.Dispose();

			return ret;
		}

	}
}
