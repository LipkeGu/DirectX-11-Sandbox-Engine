namespace Sandbox.Engine
{
	public partial class Input
	{
		public class MouseScrollEventArgs
		{
			public float Delta;

			public MouseScrollEventArgs(float delta)
			{
				Delta = delta;
			}
		}
	}
}
