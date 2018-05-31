using SharpDX;
using System;
using System.Drawing;

namespace Sandbox.Engine.Models
{
	public class Plate : BaseModel
	{
		void GenerateVertexData(string filename)
		{
			var bitmap = new Bitmap(filename);
			bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

			var mapSize = new Vector2(bitmap.Size.Width, bitmap.Size.Height);
			VertexBufferData = new Vertex[(int)mapSize.X * (int)mapSize.Y];
			var k = 0;

			for (var x = 0; x < mapSize.X; x++)
				for (var y = 0; y < mapSize.Y; y++)
				{
					var pixel = bitmap.GetPixel(x, y).ToRGBA();
					
					VertexBufferData[k] = new Vertex(new Vector4(x, (float)
						(Math.Max(Math.Max(pixel.R, pixel.G), -pixel.B) / 15 -10), y, 1),
						pixel, new Vector4(0));

					k++;
				}

			bitmap.Dispose();

			IndexBufferData = new int[(int)(mapSize.X - 1) * (int)(mapSize.Y -1) * 6];

			k = 0;
			var l = 0;

			for (var i = 0; i < IndexBufferData.Length; i += 6)
			{
				IndexBufferData[i] = k;
				IndexBufferData[i + 1] = k + (int)mapSize.Y;
				IndexBufferData[i + 2] = k + (int)mapSize.Y + 1;
				IndexBufferData[i + 3] = k;
				IndexBufferData[i + 4] = k + (int)mapSize.Y + 1;
				IndexBufferData[i + 5] = k + 1;

				k++;
				l++;

				if (l == (int)mapSize.Y - 1)
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
