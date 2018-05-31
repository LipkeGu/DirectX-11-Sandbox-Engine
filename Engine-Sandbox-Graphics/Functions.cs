using SharpDX;
using SharpDX.Mathematics.Interop;
using System;
using System.Diagnostics;

namespace Sandbox.Engine
{
	public static class Functions
	{
		public static SharpDX.Color ToRGBA(this System.Drawing.Color color)
			=> new SharpDX.Color(color.R,color.G,color.B,color.A);

		public static Matrix CreateProjectionSpace(float aspectRatio, float nearZ, float farZ)
			=> Matrix.PerspectiveFovLH((float) Math.PI / 4.0f, aspectRatio, 0.0f, ushort.MaxValue);

		public static void LOG(string message, bool waitForInteraction = false)
		{
			if (Debugger.IsAttached)
			{
				Debug.Print(message);
				if (waitForInteraction)
					Debugger.Break();
			}

		}
	}
}
