using SharpDX;
using SharpDX.Mathematics.Interop;

namespace Sandbox.Engine
{
	public struct Vertex
	{
		public Vector4 Position;
		public RawColor4 Color;
		public Vector4 Normal;

		public Vertex(Vector4 pos, RawColor4 color, Vector4 normal)
		{
			Position = pos;
			Color = color;
			Normal = normal;
		}
	}
}
