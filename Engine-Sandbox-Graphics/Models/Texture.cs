using Sandbox.Engine.Abstract;
using Sandbox.Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Engine.Models
{
	class Texture : Element2D
	{
		public Texture(string filename) : base()
		{
			CompileShader(filename);
		}
	}
}
