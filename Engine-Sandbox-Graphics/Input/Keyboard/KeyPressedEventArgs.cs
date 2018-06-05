using SharpDX.DirectInput;

namespace Sandbox.Engine
{
	public class KeyPressedEventArgs
	{
		public Key Key { get; private set; }

		public KeyPressedEventArgs(Key key)
		{
			Key = key;
		}
	}
}