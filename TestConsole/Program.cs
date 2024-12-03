using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Drawing;

namespace TestConsole;

internal class Program
{
	private static IWindow _glWindow;
	private static GL _gl;
	private static UInt32 _vao;
	private static UInt32 _vbo;
	private static UInt32 _ebo;

	static void Main(String[] args)
	{
		WindowOptions options = WindowOptions.Default with
		{
			Size = new Vector2D<Int32>(800, 600),
			Position = new Vector2D<Int32>(100, 100),
			Title = "My first Silk.NET application!"
		};
		Program._glWindow = Window.Create(options);
		Program._glWindow.Load += Program.OnLoad;
		Program._glWindow.Update += Program.OnUpdate;
		Program._glWindow.Render += Program.OnRender;
		Program._glWindow.Run();
	}

	private static unsafe void OnLoad()
	{
		IInputContext inputContext = Program._glWindow.CreateInput();
		for (Int32 keyboardIndex = 0; keyboardIndex < inputContext.Keyboards.Count; keyboardIndex++)
			inputContext.Keyboards[keyboardIndex].KeyDown += Program.KeyDown;

		Program._gl = Program._glWindow.CreateOpenGL();
		Program._gl.ClearColor(Color.DarkGray);

		Program._vao = Program._gl.GenVertexArray();
		Program._gl.BindVertexArray(Program._vao);

		Single[] vertices =
		{
			 0.5f,  0.5f, 0.0f,
			 0.5f, -0.5f, 0.0f,
			-0.5f, -0.5f, 0.0f,
			-0.5f,  0.5f, 0.0f
		};

		Program._vbo = Program._gl.GenBuffer();
		Program._gl.BindBuffer(BufferTargetARB.ArrayBuffer, Program._vbo);
		fixed (Single* buf = vertices) Program._gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(Single)), buf, BufferUsageARB.StaticDraw);

		uint[] indices =
		{
			0u, 1u, 3u,
			1u, 2u, 3u
		};

		Program._ebo = Program._gl.GenBuffer();
		Program._gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, Program._ebo);
		fixed (UInt32* buf = indices) Program._gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

		//Console.WriteLine("OnLoad");
	}

	private static unsafe void OnUpdate(Double deltaTime) 
	{
		//Console.WriteLine($"OnUpdate {deltaTime.ToString("N5")}");
	}

	private static unsafe void OnRender(Double deltaTime)
	{
		Program._gl.Clear(ClearBufferMask.ColorBufferBit);

		//Console.WriteLine($"OnRender {deltaTime.ToString("N5")}");
	}
	private static void KeyDown(IKeyboard keyboard, Key key, Int32 keyCode)
	{
		if (key == Key.Escape)
			Program._glWindow.Close(); 
		Console.WriteLine($"KeyDown {key}");
	}
}
