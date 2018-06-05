namespace Sandbox.Engine
{
	public partial class Input
	{
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
	}
}
