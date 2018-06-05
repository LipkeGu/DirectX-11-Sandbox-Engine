using SharpDX.DirectInput;

namespace Sandbox.Engine
{
	public class KeyReleasedEventArgs
	{
		public Key Key { get; private set; }

		public KeyReleasedEventArgs(Key key)
		{
			Key = key;
		}
	}
}