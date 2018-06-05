using SharpDX;
using System;
using System.Drawing;

namespace Sandbox.Engine.Models
{
	public class Plate : BaseModel
	{
		public static Vector4 MapSize { get; private set; } 
		void GenerateVertexData(string filename)
		{
			var bitmap = new Bitmap(filename);
			bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
			MapSize = new Vector4(bitmap.Size.Width, bitmap.Size.Height,
				(float)byte.MaxValue - 0.1f, 0.1f);

			VertexBufferData = new System.Collections.Generic.List<Vertex>();
			var k = 0;

			for (var x = 0; x < MapSize.X; x++)
				for (var y = 0; y < MapSize.Y; y++)
				{
					var pixel = bitmap.GetPixel(x, y).ToRGBA();

					var _color = new Color4(pixel.R / 255.0f, pixel.G / 255.0f, pixel.B / 255.0f, pixel.A / 255.0f);

					VertexBufferData.Add(new Vertex(new Vector4(x, 0, y, 1),
						_color, new Vector4()));
				}

			VertexBufferData.TrimExcess();

			bitmap.Dispose();

			IndexBufferData = new int[(int)(MapSize.X - 1) * (int)(MapSize.Y -1) * 6];

			k = 0;
			var l = 0;

			for (var i = 0; i < IndexBufferData.Length; i += 6)
			{
				IndexBufferData[i] = k;
				IndexBufferData[i + 1] = k + (int)MapSize.Y;
				IndexBufferData[i + 2] = k + (int)MapSize.Y + 1;


				IndexBufferData[i + 3] = k;
				IndexBufferData[i + 4] = k + (int)MapSize.Y + 1;
				IndexBufferData[i + 5] = k + 1;

				k++;
				l++;

				if (l == (int)MapSize.Y - 1)
				{
					l = 0;
					k++;
				}
			}
		}

		public Plate() : base()
		{
			GenerateVertexData("heightmap.bmp");

			CompileShader("Models/Plate/Plate.hlsl", "PLATE");
		}
	}
}
