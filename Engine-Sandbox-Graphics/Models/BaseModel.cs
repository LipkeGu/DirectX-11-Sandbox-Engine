using Sandbox.Engine.Graphics;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Sandbox.Engine.Models
{
	public abstract class BaseModel
	{
		private static readonly string shaderSource = @"
			float4x4 worldMatrix;

			cbuffer sunInfo : register(b0)
			{
				float4 Suncol;
				float3 Sunpos;
				float1 padding;
				float3 Sundir;
				float1 padding1;	
			};

			struct [#PREFIX#]_VS_IN
			{
				[#TYPE#]4 pos : [#PREFIX#]_POSITION;
				[#TYPE#]4 col : [#PREFIX#]_COLOR;
				[#TYPE#]4 nrm : [#PREFIX#]_NORMAL;
			};

			struct [#PREFIX#]_PS_IN
			{
				[#TYPE#]4 pos : SV_POSITION;
				[#TYPE#]4 col : [#PREFIX#]_COLOR;
				[#TYPE#]4 nrm : [#PREFIX#]_NORMAL;
			};

			[#PREFIX#]_PS_IN [#PREFIX#]_VS([#PREFIX#]_VS_IN input)
			{
				[#PREFIX#]_PS_IN output = ([#PREFIX#]_PS_IN)0;
				input.pos.w = 1.0f;
				input.pos.y = round(max(min(input.col.r * 255.0f, input.col.g * 255.0f), -input.col.b * 255.0f) / 15.0f -10.0f);
				
				output.nrm = normalize(mul(input.nrm, worldMatrix));
				output.pos = mul(input.pos, worldMatrix);
				output.col = input.col;
				
				return output;
			}

			[#TYPE#]4 [#PREFIX#]_PS([#PREFIX#]_PS_IN input) : SV_Target
			{
				float4 color = Suncol;
				float3 direction = Sundir;
				float1 strength = dot(min(input.pos, input.nrm), normalize(-direction));

				if (strength > 0.0f)
				{
					color += (input.col * strength);
				}

				color = color * input.col;
				color = saturate(color);
				
				return color;
			}";

		public VertexShader VertexShader { get; private set; }
		public PixelShader PixelShader { get; private set; }
		public HullShader HullShader { get; private set; }
		public GeometryShader GeometryShader { get; private set; }

		public SharpDX.Direct3D11.Buffer VertexBuffer { get; private set; }
		public SharpDX.Direct3D11.Buffer IndexBuffer { get; private set; }
		public SharpDX.Direct3D11.Buffer TesselBuffer { get; private set; }

		SharpDX.Direct3D11.Buffer cbWorldMatrix;
		SharpDX.Direct3D11.Buffer cbSunInfo;

		public List<Vertex> VertexBufferData { get; set; }
		public Tessel[] TesselationBufferData { get; private set; }

		public int[] IndexBufferData { get; set; }

		[StructLayout(LayoutKind.Sequential)]
		public struct SunInfo
		{
			public Vector4 Suncol;
			public Vector3 Sunpos;
			float padding;
			public Vector3 Sundir;
			float padding1;
		};

		SunInfo sun = new SunInfo();

		internal BaseModel()
		{}

		VertexBufferBinding vertexBufferBinding;

		public virtual bool CompileShader(string filename, string prefix, string version = "4_0")
		{
			var tmpShaderSource = shaderSource;
			if ((VertexBufferData == null) || VertexBufferData.Count == 0)
				throw new ArgumentNullException(string.Format("VertexBuffer: Tried to setup a" +
					"Vertexbuffer for '[#PREFIX#]' without input data!", prefix));

			var vbd = VertexBufferData.ToArray();

			VertexBuffer = SharpDX.Direct3D11.Buffer.Create(Video.GraphicDevice, BindFlags.VertexBuffer, vbd);
			if (IndexBufferData != null)
				IndexBuffer = SharpDX.Direct3D11.Buffer.Create(Video.GraphicDevice, BindFlags.IndexBuffer, IndexBufferData);

			var colorDataStart = 8;

			if (VertexBufferData[0].GetType() == typeof(Vector4))
			{
				tmpShaderSource = tmpShaderSource.Replace("[#TYPE#]", "float");
				colorDataStart = 16;
			}
			else if (VertexBufferData[0].GetType() == typeof(Vertex))
			{
				tmpShaderSource = tmpShaderSource.Replace("[#TYPE#]", "float");
				colorDataStart = 16;
			}
			else if (VertexBufferData[0].GetType() == typeof(Vector3))
			{
				tmpShaderSource = tmpShaderSource.Replace("[#TYPE#]", "float");
				colorDataStart = 12;
			}
			else if (VertexBufferData[0].GetType() == typeof(Vector2))
			{
				colorDataStart = 8;
				tmpShaderSource = tmpShaderSource.Replace("[#TYPE#]", "float");
			}
			else
				throw new Exception("Invalid datatype passed to Shader");

			var normalDataStart = colorDataStart + 16;

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
					using (var layout = new InputLayout(Video.GraphicDevice, vertexShaderSignature, new InputElement[]
					{
						new InputElement(string.Concat(prefix.ToUpper(), "_POSITION"), 0, Format.R32G32B32A32_Float, 0, 0),
						new InputElement(string.Concat(prefix.ToUpper(), "_COLOR"), 0, Format.R32G32B32A32_Float, colorDataStart, 0),
						new InputElement(string.Concat(prefix.ToUpper(), "_NORMAL"), 0, Format.R32G32B32A32_Float, normalDataStart, 0)
					}))
					{
						cbWorldMatrix = new SharpDX.Direct3D11.Buffer(Video.GraphicDevice, Utilities.SizeOf<Matrix>(),
							ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

						cbSunInfo = SharpDX.Direct3D11.Buffer.Create(Video.GraphicDevice, BindFlags.ConstantBuffer,
							ref sun, Utilities.SizeOf<SunInfo>(), ResourceUsage.Default, CpuAccessFlags.None);

						Video.DeviceContext.InputAssembler.InputLayout = layout;
					}
				}
			}

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

			vertexBufferBinding = new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<Vertex>(), 0);
			Video.DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

			return true;
		}

		public void SetVertex(Vertex vertex)
		{
			var i = VertexBufferData.FindIndex(m => m.Position == vertex.Position);
			VertexBufferData[i] = vertex;

			var vbd = VertexBufferData.ToArray();

			VertexBuffer = SharpDX.Direct3D11.Buffer.Create(Video.GraphicDevice, BindFlags.VertexBuffer, vbd);
		}

		public void Render()
		{
			Video.DeviceContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);

			if (IndexBuffer != null)
				Video.DeviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);

			var worldMatrix = Video.WorldMatrix;
			worldMatrix.Transpose();

			var viewMatrix = Video.ViewMatrix;
			viewMatrix.Transpose();

			var projMatrix = Video.ProjectionMatrix;
			projMatrix.Transpose();

			sun.Suncol = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
			sun.Sundir = Video.LookAt;
			sun.Sunpos = Video.Position;
			
			Video.DeviceContext.UpdateSubresource(ref worldMatrix, cbWorldMatrix);
			Video.DeviceContext.UpdateSubresource(ref sun, cbSunInfo);

			Video.DeviceContext.PixelShader.Set(PixelShader);
			Video.DeviceContext.VertexShader.Set(VertexShader);
			Video.DeviceContext.PixelShader.SetConstantBuffer(0, cbSunInfo);

			
			Video.DeviceContext.VertexShader.SetConstantBuffer(0, cbSunInfo);
			Video.DeviceContext.VertexShader.SetConstantBuffer(1, cbWorldMatrix);

			if (IndexBuffer != null)
				Video.DeviceContext.DrawIndexed(IndexBufferData.Length, 0, 0);
			else
				Video.DeviceContext.Draw(VertexBufferData.Count, 0);
		}

		public void Dispose()
		{
			VertexBuffer.Dispose();

			if (IndexBuffer != null)
				IndexBuffer.Dispose();

			VertexBufferData.Clear();
			VertexShader.Dispose();
			PixelShader.Dispose();

			cbWorldMatrix.Dispose();
			cbSunInfo.Dispose();
		}
	}
}