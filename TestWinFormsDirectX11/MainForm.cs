using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace TestWinFormsDirectX11;

public partial class MainForm : Form
{
	private ComPtr<ID3D11Device> d3d11Device = default;
	private ComPtr<ID3D11DeviceContext> d3d11Context = default;
	private D3DFeatureLevel d3d11FeatureLevel = default;
	private ComPtr<IDXGIFactory2> dxgiFactory2 = default;
	private ComPtr<IDXGISwapChain1> dxgiSwapChain1 = default;

	private Single[] backgroundColour = new[] { 0.1f, 0.1f, 0.1f, 1.0f };

	public MainForm()
	{
		this.InitializeComponent();
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		DXGI dxgi = DXGI.GetApi(null);
		D3DCompiler d3d11Compiler = D3DCompiler.GetApi();
		D3D11 d3d11 = D3D11.GetApi(null);

		// Create our D3D11 logical device.
		unsafe
		{
			SilkMarshal.ThrowHResult
			(
				d3d11.CreateDevice
				(
					default(ComPtr<IDXGIAdapter>),
					D3DDriverType.Hardware,
					Software: default,
					(uint)CreateDeviceFlag.Debug,
					null,
					0,
					D3D11.SdkVersion,
					ref this.d3d11Device,
					ref d3d11FeatureLevel,
					ref this.d3d11Context
				)
			);
		}

		//// Log debug messages for this device (given that we've enabled the debug flag). Don't do this in release code!
		//if (OperatingSystem.IsWindows() && Debugger.IsAttached)
		//	this.d3d11Device.SetInfoQueueCallback(msg => Console.WriteLine(SilkMarshal.PtrToString((nint)msg.PDescription)));

		// Create our swapchain.
		var swapChainDesc = new SwapChainDesc1
		{
			BufferCount = 2, // double buffered
			Format = Format.FormatB8G8R8A8Unorm,
			BufferUsage = DXGI.UsageRenderTargetOutput,
			SwapEffect = SwapEffect.FlipDiscard,
			SampleDesc = new SampleDesc(1, 0)
		};

		// Create our DXGI factory to allow us to create a swapchain. 
		this.dxgiFactory2 = dxgi.CreateDXGIFactory<IDXGIFactory2>();

		// Create the swapchain.
		unsafe
		{
			SilkMarshal.ThrowHResult
			(
				this.dxgiFactory2.CreateSwapChainForHwnd
				(
					this.d3d11Device,
					this.Handle,
					in swapChainDesc,
					null,
					ref Unsafe.NullRef<IDXGIOutput>(),
					ref this.dxgiSwapChain1
				)
			);
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
	}

	protected override void OnHandleDestroyed(EventArgs e)
	{
		base.OnHandleDestroyed(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);


		// Obtain the framebuffer for the swapchain's backbuffer.
		using var framebuffer = this.dxgiSwapChain1.GetBuffer<ID3D11Texture2D>(0);

		// Create a view over the render target.
		ComPtr<ID3D11RenderTargetView> renderTargetView = default;
		unsafe
		{
			SilkMarshal.ThrowHResult(this.d3d11Device.CreateRenderTargetView(framebuffer, null, ref renderTargetView));
		}

		// Clear the render target to be all black ahead of rendering.
		this.d3d11Context.ClearRenderTargetView(renderTargetView, ref backgroundColour[0]);

		//// Update the rasterizer state with the current viewport.
		//var viewport = new Viewport(0, 0, window.FramebufferSize.X, window.FramebufferSize.Y, 0, 1);
		//deviceContext.RSSetViewports(1, in viewport);

		//// Tell the output merger about our render target view.
		//deviceContext.OMSetRenderTargets(1, ref renderTargetView, ref Unsafe.NullRef<ID3D11DepthStencilView>());

		//// Update the input assembler to use our shader input layout, and associated vertex & index buffers.
		//deviceContext.IASetPrimitiveTopology(D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist);
		//deviceContext.IASetInputLayout(inputLayout);
		//deviceContext.IASetVertexBuffers(0, 1, vertexBuffer, in vertexStride, in vertexOffset);
		//deviceContext.IASetIndexBuffer(indexBuffer, Format.FormatR32Uint, 0);

		//// Bind our shaders.
		//deviceContext.VSSetShader(vertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
		//deviceContext.PSSetShader(pixelShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
		//deviceContext.PSSetSamplers(0, 1, textureSampler);
		//deviceContext.PSSetShaderResources(0, 1, textureResourceView);

		//// Draw the quad.
		//deviceContext.DrawIndexed(6, 0, 0);

		// Present the drawn image.
		this.dxgiSwapChain1.Present(1, 0);

		// Clean up any resources created in this method.
		renderTargetView.Dispose();

		//Console.WriteLine($"OnRender {deltaTime.ToString("N5")}");
	}
}
