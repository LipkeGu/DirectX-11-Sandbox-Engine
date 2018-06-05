using Sandbox.Engine.Graphics;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using System;
using System.Drawing;

namespace Sandbox.Engine.Abstract
{
	public abstract class Element2D : IDisposable
	{
		public Texture2D Texture { get; private set; }

		public static int Faces { get; private set; } = 1; 

		public int Width { get; private set; }

		public int Height { get; private set; }

		public Vector2 Position { get; private set; }

		public int Frames { get; private set; }

		public void Dispose()
			=> Texture.Dispose();

		static ShaderResourceView resView;

		private static readonly ImagingFactory Imgfactory = new ImagingFactory();

		private static BitmapSource LoadBitmap(string filename)
		{
			var d = new BitmapDecoder(Imgfactory, filename,	DecodeOptions.CacheOnDemand);
			var frame = d.GetFrame(0);
			var fconv = new FormatConverter(Imgfactory);

			fconv.Initialize(
				frame,
				PixelFormat.Format32bppPRGBA,
				BitmapDitherType.None, null,
				0.0, BitmapPaletteType.Custom);
			return fconv;
		}

		public static Texture2D CreateTex2DFromBitmap(string filename)
		{
			var bsource = LoadBitmap(filename);
			Texture2D texture;

			var options = Faces == 6 ? ResourceOptionFlags.TextureCube 
				| ResourceOptionFlags.GenerateMipMaps :
					ResourceOptionFlags.GenerateMipMaps; 

			using (var s = new DataStream(bsource.Size.Height * bsource.Size.Width * 4, true, true))
			{
				bsource.CopyPixels(bsource.Size.Width * 4, s);

				texture = new Texture2D(Video.GraphicDevice, new Texture2DDescription()
				{
					Format = Format.D24_UNorm_S8_UInt,
					ArraySize = 1,
					MipLevels = 1,
					Width = bsource.Size.Width,
					Height = bsource.Size.Height,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default,
					BindFlags = BindFlags.None,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None
				}, new DataRectangle(s.DataPointer, bsource.Size.Width * 4));

				s.Close();
			}

			return texture;
		}

		public void Render()
		{
			Video.DeviceContext.PixelShader.SetShaderResource(0,
				new ShaderResourceView(Video.GraphicDevice, Texture));
		}
	}
}
