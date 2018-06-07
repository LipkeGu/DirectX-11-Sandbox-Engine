using Sandbox.Engine.Graphics;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;

namespace Sandbox.Engine.Abstract
{
	public abstract class Element2D : IDisposable
	{
		public static int Faces { get; private set; } = 1;

		public int Width { get; private set; }
		public int Height { get; private set; }
		public Vector2 Position { get; private set; }
		public int Frames { get; private set; }

		SharpDX.Direct3D11.Buffer cbWorldMatrix;

		ShaderResourceView resView;
		SamplerState sampler;
		VertexBufferBinding vertexBufferBinding;
		InputLayout layout;
		Texture2D tex;

		private VertexShader vertexShader;
		private PixelShader pixelShader;
		SharpDX.Direct3D11.Buffer vertexBuffer;
		SharpDX.Direct3D11.Buffer texture;

		private string textureShaderSource = @"
		struct VS_IN
		{
			float4 pos : POSITION;
			float2 tex : TEXCOORD;
		};

		struct PS_IN
		{
			float4 pos : SV_POSITION;
			float2 tex : TEXCOORD;
		};

		float4x4 worldView;

		Texture2D picture : register(t0);
		SamplerState pictureSampler : register(s0);

		PS_IN VS_MAIN (VS_IN input)
		{
			PS_IN output = (PS_IN)0;

			output.pos = mul(input.pos, worldView);
			output.tex = input.tex;
	
			return output;
		}

		float4 PS_MAIN (PS_IN input) : SV_Target
		{
			return picture.Sample(pictureSampler, input.tex);
		}";

		public static TexVertex[] vertexBufferData = new TexVertex[]
		{
        new TexVertex(new Vector4(-1.0f,-1.0f, -1.0f, 1.0f), new Vector2(1.0f, 0.0f)), // Front
        new TexVertex(new Vector4(1.0f,-1.0f, 1.0f, 1.0f), new Vector2(0.0f, 1.0f)),
		new TexVertex(new Vector4(-1.0f,-1.0f, 1.0f, 1.0f), new Vector2(1.0f, 1.0f)),

		new TexVertex(new Vector4(-1.0f,-1.0f, -1.0f, 1.0f), new Vector2(1.0f, 0.0f)),
		new TexVertex(new Vector4(1.0f,-1.0f, -1.0f, 1.0f), new Vector2(0.0f, 0.0f)),
		new TexVertex(new Vector4(1.0f,-1.0f, 1.0f, 1.0f), new Vector2(0.0f, 1.0f))
		};

		public Element2D()
		{
			
		}

		public void CompileShader(string filename)
		{
			using (var vertexByteCode = ShaderBytecode.Compile(textureShaderSource, "VS_MAIN", "vs_4_0"))
			{
				vertexShader = new VertexShader(Video.GraphicDevice, vertexByteCode);

				layout = new InputLayout(Video.GraphicDevice, vertexByteCode, new[]
						{
						new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
						new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0)});

				pixelShader = new PixelShader(Video.GraphicDevice,
					ShaderBytecode.Compile(textureShaderSource, "PS_MAIN", "ps_4_0"));

				cbWorldMatrix = new SharpDX.Direct3D11.Buffer(Video.GraphicDevice, Utilities.SizeOf<Matrix>(),
						ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

				vertexBufferBinding = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<TexVertex>(), 0);

				tex = Functions.CreateTexture(filename, out resView);
				vertexBuffer = SharpDX.Direct3D11.Buffer.Create(Video.GraphicDevice,
					BindFlags.VertexBuffer, vertexBufferData, Utilities.SizeOf<TexVertex>());

				sampler = new SamplerState(Video.GraphicDevice, new SamplerStateDescription()
				{
					Filter = Filter.MinMagMipLinear,
					AddressU = TextureAddressMode.Clamp,
					AddressV = TextureAddressMode.Clamp,
					AddressW = TextureAddressMode.Clamp,
					BorderColor = Color.Red,
					ComparisonFunction = Comparison.Never,
					MaximumAnisotropy = 16,
					MipLodBias = 0,
					MinimumLod = -float.MaxValue,
					MaximumLod = float.MaxValue
				});

				
			}
		}

		public void Render()
		{
			Video.DeviceContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
			Video.DeviceContext.InputAssembler.InputLayout = layout;
			
			var worldMatrix = Video.WorldMatrix;
			worldMatrix.Transpose();

			Video.DeviceContext.UpdateSubresource(ref worldMatrix, cbWorldMatrix, 0);

			Video.DeviceContext.VertexShader.SetConstantBuffer(1, cbWorldMatrix);
			Video.DeviceContext.VertexShader.Set(vertexShader);

			Video.DeviceContext.PixelShader.SetShaderResource(0, resView);
			Video.DeviceContext.PixelShader.SetSampler(0, sampler);
			Video.DeviceContext.PixelShader.Set(pixelShader);

			Video.DeviceContext.Draw(vertexBufferData.Length, 0);
		}

		public void Dispose()
		{
			vertexShader.Dispose();
			pixelShader.Dispose();
			sampler.Dispose();
			resView.Dispose();

			cbWorldMatrix.Dispose();
		}
	}
}
