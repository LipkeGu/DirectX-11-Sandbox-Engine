using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Engine
{
	public struct Tessel
	{
		public float Amount;
		public Vector3 Padding;

		public Tessel(float amount, Vector3 padding)
		{
			Amount = amount;
			Padding = padding;
		}
	}
}
