using SharpDX;
using SharpDX.Direct3D11;
using System;

namespace Sandbox.Engine.Abstract
{
	public abstract class Element2D : IDisposable
	{
		public Texture2D Texture { get; private set; }
		public static int Faces { get; private set; } = 1; 

		public int Width { get; private set; }
		public int Height { get; private set; }
		public Vector2 Position { get; private set; }
		public int Frames { get; private set; }

		public void Dispose()
			=> Texture.Dispose();
	}
}
