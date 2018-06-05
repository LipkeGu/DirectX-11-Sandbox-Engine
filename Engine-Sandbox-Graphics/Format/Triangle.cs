namespace Sandbox.Engine
{
	public class Triangle
	{
		public Vertex A;
		public Vertex B;
		public Vertex C;

		public Triangle()
		{
			A = new Vertex();
			B = new Vertex();
			C = new Vertex();
		}

		public Triangle(Vertex a, Vertex b, Vertex c)
		{
			A = a;
			B = b;
			C = c;
		}
	}
}
