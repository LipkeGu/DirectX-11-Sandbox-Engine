using Sandbox.Engine;
using Sandbox.Engine.Graphics;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;

namespace Sandbox.Engine.Models
{
	public abstract class BaseModel
	{
		private static readonly string shaderSource = @"
			float4x4 worldMatrix;

			struct [#PREFIX#]_VS_IN
			{
				[#TYPE#] pos : [#PREFIX#]_POSITION;
				[#TYPE#] col : [#PREFIX#]_COLOR;
				[#TYPE#] nrm : [#PREFIX#]_NORMAL;
			};

			struct [#PREFIX#]_PS_IN
			{
				[#TYPE#] pos : SV_POSITION;
				[#TYPE#] col : [#PREFIX#]_COLOR;
				[#TYPE#] nrm : [#PREFIX#]_NORMAL;
			};

			[#PREFIX#]_PS_IN [#PREFIX#]_VS([#PREFIX#]_VS_IN input)
			{
				[#PREFIX#]_PS_IN output = ([#PREFIX#]_PS_IN)0;
				input.pos.w = 1.0f;

				output.pos = mul(input.pos, worldMatrix);
				output.col = input.col;
				
				return output;
			}

			float4 [#PREFIX#]_PS([#PREFIX#]_PS_IN input) : SV_Target
			{
				return input.col;
			}";

		public VertexShader VertexShader { get; private set; }
		public PixelShader PixelShader { get; private set; }

		public HullShader HullShader { get; private set; }
		public GeometryShader GeometryShader { get; private set; }


		public SharpDX.Direct3D11.Buffer VertexBuffer { get; private set; }
		public SharpDX.Direct3D11.Buffer IndexBuffer { get; private set; }
		public SharpDX.Direct3D11.Buffer TesselBuffer { get; private set; }

		SharpDX.Direct3D11.Buffer cbworldMatrix;

		public Vertex[] VertexBufferData { get; set; }

		public Tessel[] TesselationBufferData { get; private set; }

		public int[] IndexBufferData { get; set; }

		internal BaseModel() { }

		VertexBufferBinding vertexBufferBinding;

		public virtual bool CompileShader(string filename, string prefix, string version = "4_0")
		{
			var tmpShaderSource = shaderSource;
			if ((VertexBufferData == null) || VertexBufferData.Length == 0)
				throw new ArgumentNullException(string.Format("VertexBuffer: Tried to setup a" +
					"Vertexbuffer for '[#PREFIX#]' without input data!", prefix));

			VertexBuffer = SharpDX.Direct3D11.Buffer.Create(Video.GraphicDevice, BindFlags.VertexBuffer, VertexBufferData);
			if (IndexBufferData != null)
				IndexBuffer = SharpDX.Direct3D11.Buffer.Create(Video.GraphicDevice, BindFlags.IndexBuffer, IndexBufferData);

			var colorDataStart = 2;

			if (VertexBufferData[0].GetType() == typeof(Vector4))
			{
				tmpShaderSource = tmpShaderSource.Replace("[#TYPE#]", "float4");
				colorDataStart = 16;
			}
			else if (VertexBufferData[0].GetType() == typeof(Vertex))
			{
				tmpShaderSource = tmpShaderSource.Replace("[#TYPE#]", "float4");
				colorDataStart = 16;
			}
			else if (VertexBufferData[0].GetType() == typeof(Vector3))
			{
				tmpShaderSource = tmpShaderSource.Replace("[#TYPE#]", "float3");
				colorDataStart = 12;
			}
			else if (VertexBufferData[0].GetType() == typeof(Vector2))
			{
				colorDataStart = 8;
				tmpShaderSource = tmpShaderSource.Replace("[#TYPE#]", "float2");
			}
			else
				throw new Exception("Invalid datatype passed to Shader");

			tmpShaderSource = tmpShaderSource.Replace("[#PREFIX#]", prefix);
			using (var vertexShaderByteCode = ShaderBytecode.Compile(tmpShaderSource.Replace("[#PREFIX#]", prefix),
				string.Concat(prefix.ToUpper(), "_VS"), string.Concat("vs_", version)))
			{
				if (vertexShaderByteCode.Bytecode == null)
				{
					Video.MessageBox(string.Format("VertexShader compilation for \"{0}\" failed!.", prefix) +
						"This is mostly related to syntax errors!", "Shader compiler");

					return false;
				}
				VertexShader = new VertexShader(Video.GraphicDevice, vertexShaderByteCode);

				using (var vertexShaderSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode))
				{
					using (var pixelShaderByteCode = ShaderBytecode.Compile(tmpShaderSource.Replace("[#PREFIX#]", prefix),
						string.Concat(prefix.ToUpper(), "_PS"), string.Concat("ps_", version)))
					{
						if (pixelShaderByteCode.Bytecode == null)
						{
							Video.MessageBox(string.Format("PixelShader compilation for \"{0}\" failed!", prefix) +
							"This is mostly related to syntax errors!", "Shader compiler");

							return false;
						}

						PixelShader = new PixelShader(Video.GraphicDevice, pixelShaderByteCode);
					}

					using (var layout = new InputLayout(Video.GraphicDevice, vertexShaderSignature, new InputElement[]
					{
						new InputElement(string.Concat(prefix.ToUpper(), "_POSITION"), 0, Format.R32G32B32A32_Float, 0, 0),
						new InputElement(string.Concat(prefix.ToUpper(), "_COLOR"), 0, Format.R32G32B32A32_Float, colorDataStart, 0),
						new InputElement(string.Concat(prefix.ToUpper(), "_NORMAL"), 0, Format.R32G32B32A32_Float, colorDataStart + 16, 0),
					}))
					{
						cbworldMatrix = new SharpDX.Direct3D11.Buffer(Video.GraphicDevice, Utilities.SizeOf<Matrix>(),
							ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

						Video.DeviceContext.InputAssembler.InputLayout = layout;
					}
				}
			}

			vertexBufferBinding = new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<Vertex>(), 0);
			Video.DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

			return true;
		}

		public void Render()
		{
			Video.DeviceContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);

			if (IndexBuffer != null)
				Video.DeviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);

			var worldMatrix = Matrix.Multiply(Video.ViewMatrix, Video.ProjectionMatrix);
			Matrix.Multiply(ref worldMatrix, 2.5f, out worldMatrix);
			worldMatrix.Transpose();
			
			var viewMatrix = Video.ViewMatrix;
			viewMatrix.Transpose();

			var projMatrix = Video.ProjectionMatrix;
			projMatrix.Transpose();

			Video.DeviceContext.UpdateSubresource(ref worldMatrix, cbworldMatrix);
			Video.DeviceContext.VertexShader.SetConstantBuffer(0, cbworldMatrix);

			Video.DeviceContext.VertexShader.Set(VertexShader);
			Video.DeviceContext.PixelShader.Set(PixelShader);

			if (IndexBuffer != null)
				Video.DeviceContext.DrawIndexed(IndexBufferData.Length, 0, 0);
			else
				Video.DeviceContext.Draw(VertexBufferData.Length, 0);
		}

		public void Dispose()
		{
			VertexBuffer.Dispose();

			if (IndexBuffer != null)
				IndexBuffer.Dispose();

			VertexShader.Dispose();
			PixelShader.Dispose();

			cbworldMatrix.Dispose();
		}
	}
}
