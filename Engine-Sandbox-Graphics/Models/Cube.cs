using SharpDX;
using static Sandbox.Engine.Functions;

namespace Sandbox.Engine.Models
{
	public class Cube : BaseModel
	{
		public Cube(Vector3 position, Color color) : base()
		{
			VertexBufferData = DrawCube(position, color);

			CompileShader("Models/Cube/Cube.hlsl", "CUBE");
		}
	}
}
