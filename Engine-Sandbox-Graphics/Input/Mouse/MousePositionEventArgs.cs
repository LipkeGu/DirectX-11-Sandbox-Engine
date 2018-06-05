namespace Sandbox.Engine
{
	public partial class Input
	{
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
	}
}
