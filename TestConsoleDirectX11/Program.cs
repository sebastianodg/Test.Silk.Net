using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace TestConsoleDirectX11;

internal class Program
{
	private static IWindow _mainWindow;

	private static DXGI dxgi = null!;
	private static D3D11 d3d11 = null!;
	private static D3DCompiler compiler = null!;

	// These variables are initialized within the Load event.
	private static ComPtr<IDXGIFactory2> factory = default;
	private static ComPtr<IDXGISwapChain1> swapchain = default;
	private static ComPtr<ID3D11Device> device = default;
	private static ComPtr<ID3D11DeviceContext> deviceContext = default;
	private static ComPtr<ID3D11Buffer> vertexBuffer = default;
	private static ComPtr<ID3D11Buffer> indexBuffer = default;
	private static ComPtr<ID3D11VertexShader> vertexShader = default;
	private static ComPtr<ID3D11PixelShader> pixelShader = default;
	private static ComPtr<ID3D11InputLayout> inputLayout = default;
	private static ComPtr<ID3D11Texture2D> texture = default;
	private static ComPtr<ID3D11SamplerState> textureSampler = default;
	private static ComPtr<ID3D11ShaderResourceView> textureResourceView = default;

	private static Single[] backgroundColour = new[] { 0.1f, 0.1f, 0.1f, 1.0f };

	static void Main(string[] args)
	{
		WindowOptions options = WindowOptions.Default with
		{
			Size = new Vector2D<Int32>(800, 600),
			Position = new Vector2D<Int32>(100, 100),
			Title = "My first Silk.NET application!",
			API = GraphicsAPI.None,
		};
		Program._mainWindow = Window.Create(options);
		Program._mainWindow.Load += Program.OnLoad;
		Program._mainWindow.Update += Program.OnUpdate;
		Program._mainWindow.Render += Program.OnRender;
		Program._mainWindow.FramebufferResize += OnFramebufferResize;

		Program._mainWindow.Run();

		Program.textureResourceView.Dispose();
		Program.textureSampler.Dispose();
		Program.texture.Dispose();
		Program.factory.Dispose();
		Program.swapchain.Dispose();
		Program.device.Dispose();
		Program.deviceContext.Dispose();
		Program.vertexBuffer.Dispose();
		Program.indexBuffer.Dispose();
		Program.vertexShader.Dispose();
		Program.pixelShader.Dispose();
		Program.inputLayout.Dispose();
		Program.compiler.Dispose();
		Program.d3d11.Dispose();
		Program.dxgi.Dispose();
	}

	private static unsafe void OnLoad()
	{
		IInputContext inputContext = Program._mainWindow.CreateInput();
		for (Int32 keyboardIndex = 0; keyboardIndex < inputContext.Keyboards.Count; keyboardIndex++)
			inputContext.Keyboards[keyboardIndex].KeyDown += Program.KeyDown;

		const bool forceDxvk = false;

		Program.dxgi = DXGI.GetApi(Program._mainWindow, forceDxvk);
		Program.d3d11 = D3D11.GetApi(Program._mainWindow, forceDxvk);
		Program.compiler = D3DCompiler.GetApi();

		// Create our D3D11 logical device.
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
				ref Program.device,
				null,
				ref Program.deviceContext
			)
		);

		// Log debug messages for this device (given that we've enabled the debug flag). Don't do this in release code!
		if (OperatingSystem.IsWindows() && Debugger.IsAttached)
			Program.device.SetInfoQueueCallback(msg => Console.WriteLine(SilkMarshal.PtrToString((nint)msg.PDescription)));

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
		factory = dxgi.CreateDXGIFactory<IDXGIFactory2>();

		// Create the swapchain.
		SilkMarshal.ThrowHResult
		(
				factory.CreateSwapChainForHwnd
				(
						device,
						Program._mainWindow.Native!.DXHandle!.Value,
						in swapChainDesc,
						null,
						ref Unsafe.NullRef<IDXGIOutput>(),
						ref swapchain
				)
		);




		//Console.WriteLine("OnLoad");
	}

	private static unsafe void OnUpdate(Double deltaTime)
	{
		//Console.WriteLine($"OnUpdate {deltaTime.ToString("N5")}");
	}

	private static unsafe void OnRender(Double deltaTime)
	{
		// Obtain the framebuffer for the swapchain's backbuffer.
		using var framebuffer = swapchain.GetBuffer<ID3D11Texture2D>(0);

		// Create a view over the render target.
		ComPtr<ID3D11RenderTargetView> renderTargetView = default;
		SilkMarshal.ThrowHResult(device.CreateRenderTargetView(framebuffer, null, ref renderTargetView));

		// Clear the render target to be all black ahead of rendering.
		deviceContext.ClearRenderTargetView(renderTargetView, ref backgroundColour[0]);

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
		swapchain.Present(1, 0);

		// Clean up any resources created in this method.
		renderTargetView.Dispose();

		//Console.WriteLine($"OnRender {deltaTime.ToString("N5")}");
	}

	private static void OnFramebufferResize(Vector2D<Int32> newSize)
	{
		// If the window resizes, we need to be sure to update the swapchain's back buffers.
		SilkMarshal.ThrowHResult(swapchain.ResizeBuffers(0, (uint)newSize.X, (uint)newSize.Y, Format.FormatB8G8R8A8Unorm, 0));

		//Console.WriteLine($"OnFramebufferResize");
	}

	private static void KeyDown(IKeyboard keyboard, Key key, Int32 keyCode)
	{
		if (key == Key.Escape)
			Program._mainWindow.Close();
		Console.WriteLine($"KeyDown {key}");
	}
}
