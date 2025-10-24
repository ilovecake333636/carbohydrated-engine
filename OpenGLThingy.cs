using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using StbImageSharp;
using System.ComponentModel;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

// uses StbImageSharp

namespace GameEngineThing {
	public class Game : GameWindow {
		public Vector2i _clientSize;
		public static int _gameCount = 0;
		public int _gameID;
		private double _gameTime = .0;
		private double _semiRealTime = .0;
		private double _dT = .0;
		private Shader _shader;
		public Shader _textShader { get; private set; }
		private Texture _textureSheet;
		private Text _textRenderer;
		private Camera _camera = new(new Vector3(0f, 0f, 3f), Vector3.Zero, Vector3.UnitY);
		private ObjectMesh _cube;
		private ObjectMesh _tetrahedron;
		private ObjectMesh _plane;
		private double _DTOverTime = 0;
		public long _frameCount = 0;
		// private Random _random = new();
		private Stopwatch _stopwatch = new();
		// private bool _gameUpdating = true;
		private long _gameTick = 0;
		private float _gameTickSpeed = 60f;
		private float _gameTickLagCompensationAmount = 2f;
		// private int _seconds = 0;
		public static float _groundHeight = 0f;

		private Player _player;
		private ObjectMesh _playerTorsoMesh;
		private ObjectMesh _playerHeadMesh;
		private ObjectMesh _playerArmMesh;
		private ObjectMesh _playerLegMesh;
		private bool _isChatting = false;
		private byte _chattingBlinker = 0;
		private string _chattingText = "";
		private int _chattingTextLines = 1;
		private Vector2 _chattingTextSize = new(2);
		private float _chattingTextLineHeight = 10f;
		public bool WillReopen = false;
		public string ReopenData = "";
		public string OpenData;
		public string _gameMode;
		public DebugFlags _debugFlags = DebugFlags.none;

		private WindowState previousState;
		private Pong _pongGame;
		private VerticalOneKey _1kManiaPrototype;
		private ManiaRG _maniaRGPrototype;
		private VideoRecorder _videoRecorder;
		private long previousFrameTimestamp = 0;
		private long[] profilerFrameTimestamps = new long[128];
		// private int profilerIndex;
		private bool profilerOn = false;
		public readonly long gameStartTimestamp = Stopwatch.GetTimestamp();
		



		public Game(int width, int height, string title) :
		base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title })
		{ _gameID = _gameCount++; }


		static void Main()
		{
			Console.WriteLine("Starting OpenGL application...");
			bool Opening = true;
			string OpenData = "";
			while (Opening)
			{
				Opening = false;
				using (Game game = new(800, 600, "GameEngineThingy :3"))
				{
					game.VSync = VSyncMode.On;
					game.OpenData = OpenData;
					game.Run();
					Opening = game.WillReopen;
					OpenData = game.ReopenData;
				}
			}
			Console.WriteLine("game has closed.");

#if false
			int a = 1;
			int b = 1;
			int c = 1;
			Console.WriteLine("a = 1; (a-- == 0) is " + (a-- == 0) + "; b = 1; (b-- == 1) is " + (b-- == 1) + "; c = 1; c-- is " + c--);

			for (int j = 0; j < 4; j++) // debugging things
			{
				long time = Stopwatch.GetTimestamp();
				double dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("random nothing time thing: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++) { }
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("nothing loop: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++) { Vector3 nothing = new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()); }
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("generating vec3 but doing nothing loop: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++) { new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()).Deconstruct(out float x, out float y, out float z); }
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("deconstructing vec3 loop: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					// float x, y, z;
					Vector3 vec = new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
					(float x, float y, float z) = (vec.X, vec.Y, vec.Z);
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("deconstructing vec3 loop2: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					// float x, y, z;
					Vector3 vec = new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
					(float x, float y, float z) = (vec.X * 6, vec.Y * vec.Z, vec.Z);
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("deconstructing vec3 loop2a: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					// float x, y, z;
					Vector3 vec = new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
					float x = vec.X;
					float y = vec.Y;
					float z = vec.Z;
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("deconstructing vec3 loop3: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					// float x, y, z;
					Vector3 vec = new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
					float x = vec.X * 6;
					float y = vec.Y * vec.Z;
					float z = vec.Z;
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("deconstructing vec3 loop3a: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++) {
					(float x, float y, float z) = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("deconstructing vec3 loop4: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					// float x, y, z;
					Vector3 vec = new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
					float hue = vec.X * 6;
					float sxv = vec.Y * vec.Z;
					float value = vec.Z;
					float red = Math.Clamp(Math.Abs(hue - 3) - 1, 0, 1);
					float green = Math.Clamp(2 - Math.Abs(hue - 2), 0, 1);
					float blue = Math.Clamp(2 - Math.Abs(hue - 4), 0, 1);
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("bruh: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					// float x, y, z;
					Vector3 vec = new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
					float hue = vec.X * 6;
					float sxv = vec.Y * vec.Z;
					float value = vec.Z;
					float red = Math.Clamp(Math.Abs(hue - 3) - 1, 0, 1);
					float green = Math.Clamp(2 - Math.Abs(hue - 2), 0, 1);
					float blue = Math.Clamp(2 - Math.Abs(hue - 4), 0, 1);
					(red, green, blue) = (value + red * sxv, value + green * sxv, value + blue * sxv);
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("bruh2: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					// float x, y, z;
					Vector3 vec = new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
					float hue = vec.X * 6;
					float sxv = vec.Y * vec.Z;
					float value = vec.Z;
					float red = Math.Clamp(Math.Abs(hue - 3) - 1, 0, 1);
					float green = Math.Clamp(2 - Math.Abs(hue - 2), 0, 1);
					float blue = Math.Clamp(2 - Math.Abs(hue - 4), 0, 1);
					(red, green, blue) = (value + red * sxv, value + green * sxv, value + blue * sxv);
					Vector3 outthing = new(red, green, blue);
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("bruh3: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					// float x, y, z;
					Vector3 vec = new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
					float hue = vec.X * 6;
					float sxv = vec.Y * vec.Z;
					float value = vec.Z;
					float red = Math.Clamp(Math.Abs(hue - 3) - 1, 0, 1);
					float green = Math.Clamp(2 - Math.Abs(hue - 2), 0, 1);
					float blue = Math.Clamp(2 - Math.Abs(hue - 4), 0, 1);
					Vector3 outthing = new(value + red * sxv, value + green * sxv, value + blue * sxv);
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("bruh4: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					// float x, y, z;
					Vector3 vec = new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
					float hue = vec.X * 6;
					float sxv = vec.Y * vec.Z;
					float value = vec.Z;
					float red = Math.Abs(hue - 3) - 1;
					if (red > 1) red = 1;
					if (red < 0) red = 0;
					float green = 2 - Math.Abs(hue - 2);
					if (green > 1) green = 1;
					if (green < 0) green = 0;
					float blue = 2 - Math.Abs(hue - 4);
					if (blue > 1) blue = 1;
					if (blue < 0) blue = 0;
					Vector3 outthing = new(value + red * sxv, value + green * sxv, value + blue * sxv);
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("bruh5: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					// float x, y, z;
					Vector3 vec = new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
					float hue = vec.X * 6;
					float sxv = vec.Y * vec.Z;
					float value = vec.Z;
					float red = Math.Abs(hue - 3) - 1;
					if (red > 1) red = 1;
					else if (red < 0) red = 0;
					float green = 2 - Math.Abs(hue - 2);
					if (green > 1) green = 1;
					else if (green < 0) green = 0;
					float blue = 2 - Math.Abs(hue - 4);
					if (blue > 1) blue = 1;
					else if (blue < 0) blue = 0;
					Vector3 outthing = new(value + red * sxv, value + green * sxv, value + blue * sxv);
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("bruh6: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					// float x, y, z;
					Vector3 vec = new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
					float hue = vec.X * 6;
					float sxv = vec.Y * vec.Z;
					float value = vec.Z;
					float red = Math.Abs(hue - 3) - 1;
					if (red > 1) red = 1;
					else if (red < 0) red = 0;
					float green = 2 - Math.Abs(hue - 2);
					if (green > 1) green = 1;
					else if (green < 0) green = 0;
					float blue = 2 - Math.Abs(hue - 4);
					if (blue > 1) blue = 1;
					else if (blue < 0) blue = 0;
					Vector3 outthing = new(MathF.FusedMultiplyAdd(red, sxv, value), MathF.FusedMultiplyAdd(green, sxv, value), MathF.FusedMultiplyAdd(blue, sxv, value));
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("bruh7: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					// float x, y, z;
					Vector3 vec = new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
					float hue = vec.X * 6;
					float sxv = vec.Y * vec.Z;
					float value = vec.Z;
					float red = Math.Abs(hue - 3) - 1;
					if (red > 1) red = 1;
					else if (red < 0) red = 0;
					float green = 2 - Math.Abs(hue - 2);
					if (green > 1) green = 1;
					else if (green < 0) green = 0;
					float blue = 2 - Math.Abs(hue - 4);
					if (blue > 1) blue = 1;
					else if (blue < 0) blue = 0;
					Vector3 outthing = new((float)Math.FusedMultiplyAdd(red, sxv, value), (float)Math.FusedMultiplyAdd(green, sxv, value), (float)Math.FusedMultiplyAdd(blue, sxv, value));
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("bruh8: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					DataStuff.HSVToRGBUnoptimized(new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()));
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("somewhat 'unoptimized' code loop: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					DataStuff.HSVToRGB(new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()));
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("somewhat 'optimized' code loop: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					DataStuff.HSVToRGB2(new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()));
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("something 2: (hopefully this might be a bit faster) " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					DataStuff.HueToRGB(Random.Shared.NextSingle());
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("just hue, no sat or val code loop: " + dt + "ms.");
				time = Stopwatch.GetTimestamp();
				for (int i = 0; i < 25000000; i++)
				{
					DataStuff.HueToRGB(Random.Shared.NextSingle());
				}
				dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
				Console.WriteLine("h2rgb2: " + dt + "ms.");
			}
#endif
		}


		protected override void OnLoad() {
			base.OnLoad();
			GL.ClearColor(.2f, .3f, .3f, 1f);

			// Ensure text EBO is created with no mesh VAO bound
			Text.Initialize();

			_tetrahedron = new ObjectMesh(new Vector3(0f, 10f, 0f), Vector3.Zero, Vector3.One, DataStuff.TetrahedronV, DataStuff.TetrahedronI);
			_cube = new ObjectMesh(Vector3.Zero, Vector3.Zero, Vector3.One, DataStuff.CubeV, DataStuff.CubeI);
			_plane = new ObjectMesh(Vector3.Zero, Vector3.Zero, Vector3.One, DataStuff.PlaneV, DataStuff.PlaneI);

			_playerTorsoMesh = new ObjectMesh(Vector3.Zero, Vector3.Zero, Vector3.One, DataStuff.PlrTorsoV, DataStuff.PlrTorsoI);
			_playerArmMesh = new ObjectMesh(Vector3.Zero, Vector3.Zero, Vector3.One, DataStuff.PlrArmV, DataStuff.PlrArmI);
			_playerLegMesh = new ObjectMesh(Vector3.Zero, Vector3.Zero, Vector3.One, DataStuff.PlrLegV, DataStuff.PlrLegI);
			_playerHeadMesh = new ObjectMesh(Vector3.Zero, Vector3.Zero, Vector3.One, DataStuff.PlrHeadV, DataStuff.PlrHeadI);
			_player = new Player(Vector3.Zero, Vector3.Zero, Vector3.One, [
				new(_playerTorsoMesh, 0),
				new(_playerHeadMesh, 0),
				new(_playerArmMesh, 0),
				new(Vector3.Zero, Vector3.Zero, new Vector3(-1f, 1f, 1f), _playerArmMesh),
				new(_playerLegMesh, 0),
				new(Vector3.Zero, Vector3.Zero, new Vector3(-1f, 1f, 1f), _playerLegMesh),
			], [Vector3.Zero, new(0f, 1.2f, 0f), new(-.8f, 0f, 0f), new(.8f, 0f, 0f), new(-.25f, -1f, 0f), new(.25f, -1f, 0f),
			], [Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero], [
				Vector3.One, Vector3.One, Vector3.One,
				new(-1f, 1f, 1f),
				Vector3.One,
				new(-1f, 1f, 1f),
			]);
			_shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
			_shader.Use();

			GL.Enable(EnableCap.DepthTest);
			// _textureSheet = Texture.LoadFromFile("Textures/texturesheet.png", false, true);
			_textureSheet = Texture.LoadFromFile("Textures/texturesheet.png", false, false);
			_textureSheet.Use(TextureUnit.Texture0);
			_shader.SetInt("texture0", 0);
			_shader.SetInt("tx0", 0);

			_textShader = new Shader("Shaders/textShader.vert", "Shaders/textShader.frag");
			_textShader.Use();

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			_textShader.SetInt("text", 1);
			_textShader.SetVector3("textColor", Vector3.One);

			_textRenderer = new Text(Texture.LoadFromFile("Fonts/fonttest.png", true, false));
			_textRenderer.TextTexture.Use(TextureUnit.Texture1);


			_camera.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), 800f / 600f, .1f, 100f);

			_camera.Direction = Vector3.Normalize(_camera.Position - _camera.Target);
			_camera.Right = Vector3.Normalize(Vector3.Cross(_camera.Up, _camera.Direction));
			_camera.View = Matrix4.LookAt(_camera.Position, _camera.Target, _camera.Up);
			_clientSize = ClientSize;

			Console.WriteLine("Max vertices: " + GL.GetInteger(GetPName.MaxElementsVertices));
			Console.WriteLine("Max indices: " + GL.GetInteger(GetPName.MaxElementsIndices));

			Console.WriteLine("FontCharacterData info: FontCharDeeta has " + FontCharFillerThing.FontCharDeeta.Chars.Count + " normal characters, and " + FontCharFillerThing.FontCharDeeta.SChars.Count + " special characters.");
			float _lineHeight = 10f;

			// _textRenderer.NewTxtThing(new TxtOptions(posOffset, posScale, textScale, color, lineHeight, windowSize, fontCharData, useSpecialChar));
			_textRenderer.TextThingies.Add(new TxtOptions(Vector2i.Zero, new(-.9f, .9f), new(5), Vector3.One, _lineHeight, FontCharFillerThing.FontCharDeeta, true));
			_textRenderer.TextThingies.Add(new TxtOptions(Vector2i.Zero, new(-.5f, .9f), new(5), Vector3.One, _lineHeight, FontCharFillerThing.FontCharDeeta, true));
			_textRenderer.TextThingies.Add(new TxtOptions(Vector2i.Zero, new(-.9f, .9f), new(2), Vector3.One, _lineHeight, FontCharFillerThing.FontCharDeeta, true));
			_textRenderer.TextThingies.Add(new TxtOptions(Vector2i.Zero, new(-.9f, .9f), new(1), Vector3.One, _lineHeight, FontCharFillerThing.FontCharDeeta, true));
			_textRenderer.TextThingies.Add(new TxtOptions(Vector2i.Zero, new(-.9f, -.9f), new(10), new(1f, .5f, 1f), _lineHeight, FontCharFillerThing.FontCharDeeta, true));
			_textRenderer.TextThingies.Add(new TxtOptions(Vector2i.Zero, new(-.9f, -.4f), new(10), new(1f, .5f, 1f), _lineHeight, FontCharFillerThing.FontCharDeeta, true));
			_textRenderer.TextThingies.Add(new TxtOptions(Vector2i.Zero, new(-.9f, -.6f), new(1), new(1f, .5f, 1f), _lineHeight, FontCharFillerThing.FontCharDeeta, true));

			_textRenderer.TextThingies.Add(new TxtOptions(Vector2i.Zero, new(-.96f + (float)Math.Sin(_gameTime) * .05f, -.5f), Vector2.Zero, new(Random.Shared.Next(0, 100) / 100f), _lineHeight, FontCharFillerThing.FontCharDeeta, true));
			switch (OpenData.ToLower()) {
				case "pong":
					_pongGame = new Pong(new Vector3(10f, 0f, 10f), new Vector3(1f), new Vector3(270f, 0f, 0f));
					_gameMode = "pong";
					break;
				case "fnf": // do something else but rn the something else doesn't exist
				case "mania": // also do something else but the something else also doesn't exist
					_maniaRGPrototype = new ManiaRG(_textRenderer);
					_maniaRGPrototype.StartTraining();
					_maniaRGPrototype.RestartMap();
					_gameMode = "mania";
					break;
				case "1k fnf" or "1kfnf" or "fnf 1k" or "fnf1k" or
					"v1k" or "verticalonekey" or "vertical one key":
					_1kManiaPrototype = new VerticalOneKey(_textRenderer);
					_1kManiaPrototype.LoadMap(DataStuff.BuiltInV1KCharts[0]);
					_1kManiaPrototype.RestartMap();
					_gameMode = "v1k";
					break;
				case "v1k2":
					_1kManiaPrototype = new VerticalOneKey(_textRenderer);
					_1kManiaPrototype.LoadMap(DataStuff.BuiltInV1KCharts[1]);
					_1kManiaPrototype.RestartMap();
					_gameMode = "v1k";
					break;
				default: break;}

			_stopwatch.Start();}
		protected override void OnRenderFrame(FrameEventArgs e) {
			base.OnRenderFrame(e);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			_textureSheet.Use(TextureUnit.Texture0);
			// _textRenderer.TextTexture.Use(TextureUnit.Texture1); // should already be used though idk

			_shader.Use();
			_shader.SetMatrix4("view", _camera.View);
			_shader.SetMatrix4("projection", _camera.Projection);

			_tetrahedron.Draw(_shader, true);

			_cube.Draw(_shader, true);

			Matrix4 Scale = Matrix4.CreateScale(new Vector3((float)Math.Sin(_gameTime) + 1f, (float)Math.Sin(_gameTime) + 1f, (float)Math.Sin(_gameTime) + 1f));
			Matrix4[] models = [
				Scale * Matrix4.CreateTranslation(new Vector3(-10f, 0f, 0f)),
				Scale * Matrix4.CreateTranslation(new Vector3(10f, 0f, 0f)),
				Scale * Matrix4.CreateTranslation(new Vector3(0f, 10f, 0f)),
				Scale * Matrix4.CreateTranslation(new Vector3(0f, -10f, 0f)),
				Scale * Matrix4.CreateTranslation(new Vector3(0f, 0f, 10f)),
				Scale * Matrix4.CreateTranslation(new Vector3(0f, 0f, -10f)),
			];

			// // Draw multiple cubes with different texture layers
			// for (int i = 0; i < models.Length; i++) _cube.DrawWithModel(_shader, models[i], false);
			_cube.DrawWithModels(_shader, models, false);

			_shader.SetTextureLocation("tx0", new Vector4(0f, 0f, 1f, 1f));
			_player.Render(_shader);

			switch (_gameMode) {
				case "pong":
					_pongGame.Render(_shader, _cube, _plane, true);
					// _textRenderer.RenderText(this, _textShader, _pongGame.ScoreText, Vector2i.Zero, new(0f, .45f), new(6f), new(0f, 1f, 0f), 10f, _clientSize, FontCharFillerThing.FontCharDeeta, false);
					_textRenderer.Render(new TxtOptions(Vector2i.Zero, new(0f, .45f), new(4f), new(0f, 1f, 0f), 10f, FontCharFillerThing.FontCharDeeta, false), this, _pongGame.ScoreText);
					break;
				case "mania":
					_maniaRGPrototype.time = Stopwatch.GetElapsedTime(_maniaRGPrototype.timeOffset).TotalSeconds;
					Vector3 color;
					// switch (_frameCount / 512 % 6)
					// {
					// 	case 0: _maniaRGPrototype.Render(this, _frameCount % 512 == 0, DataStuff.HSVToRGB(new((_frameCount / 40.2f) % 1f, 0.7f, 0.7f))); break;

					// 	case 2: _maniaRGPrototype.Render(this, _frameCount % 512 == 0, DataStuff.HSVToRGBUnoptimized(new(_frameCount / 150f % 1f, 1f, 1f))); break;
					// 	case 3:
					// 		color = new(_frameCount % 100 / 100.0f, _frameCount % 721 / 721.0f, _frameCount % 1923 / 1923.0f);
					// 		_maniaRGPrototype.Render2(_textShader, this, DataStuff.HSVToRGB(color)); break;
					// 	case 4:
					// 		color = new(_frameCount % 100 / 100.0f, _frameCount % 721 / 721.0f, _frameCount % 1923 / 1923.0f);
					// 		_maniaRGPrototype.Render(this, _frameCount % 512 == 0, DataStuff.HSVToRGB(color)); break;
					// 	default: _maniaRGPrototype.Render(this, _frameCount % 512 == 0); break;
					// }
					// if (_frameCount % 128 == 0) Console.WriteLine(_frameCount / 512 % 6 + ", " + _frameCount / 512);
					color = new((float)(Stopwatch.GetElapsedTime(gameStartTimestamp).TotalSeconds*0.36%1), (float)(Math.Sin(Stopwatch.GetElapsedTime(gameStartTimestamp).TotalSeconds*0.2)*0.125+0.875), (float)(Math.Cos(Stopwatch.GetElapsedTime(gameStartTimestamp).TotalSeconds*0.09)*0.125+0.875));
					_maniaRGPrototype.Render2(_textShader, this, DataStuff.HSVToRGB(color)); break;
				case "v1k":
					_1kManiaPrototype.time = _1kManiaPrototype.stopwatch.Elapsed.TotalSeconds + _1kManiaPrototype.timeOffset;
					_1kManiaPrototype.Render(this);
					break;
				default: break;}

			// UI time!! :3
			// the text kind of breaks a lot of things, but for some reason doesn't throw any errors, and if anything else that isn't text tries to render after this, it just won't.
			bool debugText = _debugFlags.HasFlag(DebugFlags.debugText);
			if (debugText) {
				_textRenderer.Render(0, this, $"FPS: {1 / _dT:N4}");
				_textRenderer.Render(1, this, "FPS: {1 / _dT:N4}");
				_textRenderer.Render(2, this, "FPS: " + (1 / _dT));
				_textRenderer.Render(3, this, $"FPS: {1 / _dT:N4}");
				_textRenderer.Render(4, this, "abcdefghijklmnopqrstuvwxyz a b      d!? b");
				TxtOptions txtOptions = _textRenderer.TextThingies[4];
				txtOptions.posScaleX = (float)Math.Sin(_gameTime);
				_textRenderer.Render(txtOptions, this, "abcdefghijklmnopqrstuvwxyz a b      d!? b");
				_textRenderer.Render(5, this, "FPS");
				_textRenderer.Render(6, this, "The FitnessGram(TM) Pacer Test is an aerobic capacity test that progressively gets harder as it continues.\nThe thirty meter pacer test begins in 20 seconds.\nWhen you hear this signal a lap is completed if you don't complete the lap in time you get a strike if you get two strikes you are out\nblah blah when you hear this sound it starts on your mark get ready start\nthe quick brown fox jumped over the lazy dog THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG 0123456789\n?!?!?![]{}-=_+`~!@#$%^&*();':\",.<>/\\|        cabbage");
				txtOptions = _textRenderer.TextThingies[7];
				for (int i = 20; i > 0; i--) {
					if (i == 15) { i -= 4; continue; }
					txtOptions.posScaleX = (float)Math.Sin(_gameTime) * .05f - .96f;
					txtOptions.posScaleY = i * .05f - .5f;
					txtOptions.textScaleX = i * .5f;
					txtOptions.textScaleY = i * .5f;
					txtOptions.color = new(Random.Shared.Next(0, 100) / 100f);
					_textRenderer.Render(txtOptions, this, i + "FPS: " + 1 / _dT);}}
			if (_isChatting) {
				string chatTxt = "> " + _chattingText;
				if (_chattingBlinker < 128) chatTxt += "\\blinker|";
				Vector2i posOffset = new(0, (int)(_chattingTextLines * _chattingTextLineHeight * _chattingTextSize.Y));
				_textRenderer.RenderText(this, _textShader, chatTxt, posOffset, new(-1), _chattingTextSize, new(1), _chattingTextLineHeight, _clientSize, FontCharFillerThing.FontCharDeeta, true);
				if (debugText) _textRenderer.RenderText(this, _textShader, chatTxt, posOffset, new(-1, -.8f), _chattingTextSize, new(.7f), _chattingTextLineHeight, _clientSize, FontCharFillerThing.FontCharDeeta, false);}
			// _videoRecorder?.CaptureFrame(this.ClientSize, VSync == VSyncMode.On);
			_videoRecorder?.CaptureFrame(this.ClientSize, false);

			SwapBuffers();}
		protected override void OnUpdateFrame(FrameEventArgs e) {
			base.OnUpdateFrame(e);
			_frameCount++;
			_dT = e.Time;
			_semiRealTime += _dT;
			if (_frameCount % 128 == 0) {
				Title = "GameEngineThingy :3 FPS: " + 128d / _DTOverTime;
				_DTOverTime = _dT;}
			else _DTOverTime += _dT;

			if (_isChatting) { _chattingBlinker++; }
			_cube.Update(0, Vector3.Zero, new Vector3(0f, (float)_gameTime, (float)Math.Sin(_gameTime) * 2f), Vector3.One);
			_tetrahedron.Update(0, new Vector3(0f, 3f, 0f), new Vector3(0f, (float)_gameTime, (float)Math.Sin(_gameTime) * 2f), Vector3.One);
			if (IsFocused) {
				if (KeyboardState.IsKeyPressed(Keys.F11))
					if (WindowState == WindowState.Fullscreen) WindowState = previousState;
					else { previousState = WindowState; WindowState = WindowState.Fullscreen; VSync = VSyncMode.On; }
				if (_isChatting) {
					if (KeyboardState.IsKeyPressed(Keys.Escape)) { _isChatting = false; }}
				else {
					float TickSpeedInv = 1f / _gameTickSpeed;
					if (KeyboardState.IsKeyDown(Keys.I)) _camera.CameraDistFromTarget = Math.Max(_camera.MinDist, _camera.CameraDistFromTarget - 2f * TickSpeedInv);
					if (KeyboardState.IsKeyDown(Keys.O)) _camera.CameraDistFromTarget = Math.Min(_camera.MaxDist, _camera.CameraDistFromTarget + 2f * TickSpeedInv);
					if (MouseState.ScrollDelta.Y != 0) {
						_camera.CamSpeed = Math.Max(.1f, Math.Min(50f, _camera.CamSpeed + MouseState.ScrollDelta.Y * .1f));
						if (_debugFlags.HasFlag(DebugFlags.debugLogging)) Console.WriteLine("scroll speed changed; new speed: " + _camera.CamSpeed);}
					float deltaX = MouseState.X - MouseState.PreviousX;
					float deltaY = MouseState.Y - MouseState.PreviousY;
					if (deltaX != 0 || deltaY != 0) {
						_camera.Yaw = (_camera.Yaw + deltaX * _camera.MouseSensitivity) % (float)(2.0 * Math.PI);
						_camera.Pitch = Math.Max(MathHelper.DegreesToRadians(-89f), Math.Min(MathHelper.DegreesToRadians(89f), _camera.Pitch - deltaY * _camera.MouseSensitivity));

						_camera.CameraToTargetOffset = -Vector3.Normalize(new Vector3(
							(float)Math.Cos(_camera.Pitch) * (float)Math.Cos(_camera.Yaw),
							(float)Math.Sin(_camera.Pitch),
							(float)Math.Cos(_camera.Pitch) * (float)Math.Sin(_camera.Yaw)
						));}
					if (_semiRealTime < TickSpeedInv) { // if this frame is too early to go to the next game tick
					  // update camera vectors so the camera movement is smooth
						_camera.UpdateVectors();
						return;}
					// increment game tick and update game time
					_gameTick++;
					_gameTime = _gameTick * TickSpeedInv;
					// update semi real time; this is a fake time that is used to make the game run at a constant speed
					_semiRealTime -= TickSpeedInv;
					if (_semiRealTime > _gameTickLagCompensationAmount * TickSpeedInv) _semiRealTime = _gameTickLagCompensationAmount * TickSpeedInv;
					// ^ prevents the semi real time from getting too big; If this wasn't here, then for example, if a big lag spike happens, the semi real time will get really big and the game will run as fast as possible for a while, and that would feel really weird, and people might rage or something idk :p

					if (_debugFlags.HasFlag(DebugFlags.debugLogging) && _gameTick % (long)(_gameTickSpeed * 2) == 0) Console.WriteLine("Time: " + _stopwatch.Elapsed.TotalSeconds); // print time every 2 seconds

					// movement
					if (_camera.IsFlying) {
						if (KeyboardState.IsKeyDown(Keys.W)) _camera.Target -= _camera.Direction * _camera.CamSpeed * TickSpeedInv;
						if (KeyboardState.IsKeyDown(Keys.S)) _camera.Target += _camera.Direction * _camera.CamSpeed * TickSpeedInv;
						if (KeyboardState.IsKeyDown(Keys.A)) _camera.Target -= _camera.Right * _camera.CamSpeed * TickSpeedInv;
						if (KeyboardState.IsKeyDown(Keys.D)) _camera.Target += _camera.Right * _camera.CamSpeed * TickSpeedInv;
						if (KeyboardState.IsKeyDown(Keys.Space) || KeyboardState.IsKeyDown(Keys.E)) _camera.Target += _camera.Up * _camera.CamSpeed * TickSpeedInv;
						if (KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.Q)) _camera.Target -= _camera.Up * _camera.CamSpeed * TickSpeedInv;}
					else {
						if (KeyboardState.IsKeyDown(Keys.W)) _player.RootPosition -= Vector3.Normalize(new Vector3(_camera.Direction.X, 0f, _camera.Direction.Z)) * _camera.CamSpeed * TickSpeedInv;
						if (KeyboardState.IsKeyDown(Keys.S)) _player.RootPosition += Vector3.Normalize(new Vector3(_camera.Direction.X, 0f, _camera.Direction.Z)) * _camera.CamSpeed * TickSpeedInv;
						if (KeyboardState.IsKeyDown(Keys.A)) _player.RootPosition -= Vector3.Normalize(new Vector3(_camera.Right.X, 0f, _camera.Right.Z)) * _camera.CamSpeed * TickSpeedInv;
						if (KeyboardState.IsKeyDown(Keys.D)) _player.RootPosition += Vector3.Normalize(new Vector3(_camera.Right.X, 0f, _camera.Right.Z)) * _camera.CamSpeed * TickSpeedInv;
						if (KeyboardState.IsKeyDown(Keys.Space) && _player.IsGrounded) _player.Jump();
						_player.StepPhysics(TickSpeedInv);}
					_camera.Target = _player.RootPosition;
					_camera.UpdateVectors(); // update cam

					switch (_gameMode) {
						case "pong":
							_pongGame.UpdateRot(new Vector3(270f, (float)_gameTime * 15f, 0f));
							_pongGame.Update(TickSpeedInv);
							break;
						case "mania":
							// Console.WriteLine(_dT);
							_maniaRGPrototype.time = Stopwatch.GetElapsedTime(_maniaRGPrototype.timeOffset).TotalSeconds;
							_maniaRGPrototype.Update();
							break;
						case "v1k":
							// Console.WriteLine(_dT);
							_1kManiaPrototype.time = _1kManiaPrototype.stopwatch.Elapsed.TotalSeconds + _1kManiaPrototype.timeOffset;
							_1kManiaPrototype.Update();
							break;
						default:
							break;}

					if (_isChatting) _chattingBlinker += 3;}}
			else {/* window is not focused */}}
		protected override void OnResize(ResizeEventArgs e) {
			base.OnResize(e);
			_clientSize = ClientSize;
			GL.Viewport(0, 0, _clientSize.X, _clientSize.Y);
			_camera.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), _clientSize.X / (float)_clientSize.Y, .1f, 100f);}
		protected override void OnTextInput(TextInputEventArgs e) {
			base.OnTextInput(e);
			string s = e.AsString;
			if (_isChatting)
				_chattingText += s;
			else if (s == "/") {
				_isChatting = true;
				_chattingBlinker = 0;}}
		protected override void OnKeyDown(KeyboardKeyEventArgs e) {
			long timestamp = Stopwatch.GetTimestamp();
			base.OnKeyDown(e);
			if (_isChatting) {
				switch (e.Key) {
					case Keys.Delete or Keys.Backspace:
						if (_chattingText.Length > 0) { if (_chattingText[^1] == '\n') _chattingTextLines--; _chattingText = _chattingText[..^1]; }
						break;
					case Keys.Enter:
						if (e.Modifiers.HasFlag(KeyModifiers.Shift)) {
							_chattingText += "\n";
							_chattingTextLines++;}
						else {
							string lowercaseChatTxt = _chattingText.ToLower();
							switch (lowercaseChatTxt) {
								case "exit" or "quit" or "cabbage": // cabbage :3
									WillReopen = false;
									Close(); break;
								case "pong" or "snake" or "fnf":
									ReopenData = _chattingText;
									WillReopen = true;
									Close(); break;
								case "v1kshowalldata" or "v1k show all data" or "v1kshowall" or "v1k show all" or "v1kshowallinfo" or "v1k show all info": VerticalOneKey.DisplayFullInfo = !VerticalOneKey.DisplayFullInfo; break;
								case "maniashowalldata": ManiaRG.DisplayFullInfo = !ManiaRG.DisplayFullInfo; break;
								case "debugtxt" or "debugtext" or "debug text" or "debug txt" or "dbtxt": Console.WriteLine("debug txt entered debugging thing idk\nPrevious thing: " + _debugFlags.HasFlag(DebugFlags.debugText));
									if (_debugFlags.HasFlag(DebugFlags.debugText))
										_debugFlags &= (DebugFlags)0b1111111111111111111111111111110; else _debugFlags |= DebugFlags.debugText;
									Console.WriteLine("Now: " + _debugFlags.HasFlag(DebugFlags.debugText)); break;
								case "debuglog": Console.WriteLine("debug logging entered debugging thing idk\nPrevious: " + _debugFlags.HasFlag(DebugFlags.debugLogging));
									if (_debugFlags.HasFlag(DebugFlags.debugLogging))
										_debugFlags &= (DebugFlags)0b1111111111111111111111111111101; else _debugFlags |= DebugFlags.debugLogging;
									Console.WriteLine("Now: " + _debugFlags.HasFlag(DebugFlags.debugText)); break;
								case "showvsync": Console.WriteLine("Vsync mode right now: " + VSync); break;
								case "vsyncon": VSync = VSyncMode.On; break;
								case "vsyncoff": VSync = VSyncMode.Off; break;
								case "vsyncadapt": VSync = VSyncMode.Adaptive; break;
								case "stoprecording": Console.WriteLine("Stopping recording hopefully."); StopRecording(); Console.WriteLine("Stopped recording hopefully..."); break;
								default:
									if (lowercaseChatTxt.StartsWith("reopen")) {
										WillReopen = true;
										if (lowercaseChatTxt.Length > 6 && lowercaseChatTxt[6] == ' ') {
											ReopenData = lowercaseChatTxt[7..];
											Console.WriteLine("ReopenData: \"" + ReopenData + "\""); }
										Close();
									} else if (lowercaseChatTxt.Length > 7) {
										if (lowercaseChatTxt.StartsWith("record")) {
											switch (lowercaseChatTxt[6]) {
												case ' ':
													StartRecording(lowercaseChatTxt[7..]); Console.WriteLine("Recording with file path " + lowercaseChatTxt[7..]);
													break;
												case '_':
													int breakcharpos = lowercaseChatTxt.IndexOf(',', 7);
													if (breakcharpos == -1) { Console.WriteLine("fps not found..."); return; }
													int fps = Convert.ToInt32(lowercaseChatTxt[7..breakcharpos++]);
													StartRecording(lowercaseChatTxt[breakcharpos..], fps: fps); Console.WriteLine("Recording at " + fps + " fps with file path " + lowercaseChatTxt[breakcharpos..]);
													break;
											}
										}
									} else if (lowercaseChatTxt.Length > 8 && lowercaseChatTxt.StartsWith("loadmap ")) {
										if (_gameMode == "v1k") {
											string path = lowercaseChatTxt[8..];
											if (File.Exists(path)) {
												string data = File.ReadAllText(path);
												if (_1kManiaPrototype.TryLoadMapFromString(data)) {
													Console.WriteLine("Loadmap success!"); } else {
													Console.WriteLine("Failed to load map."); } }
											else Console.WriteLine("Path does not exist!");
										} else if (_gameMode == "mania") {
											string path = lowercaseChatTxt[8..];
											if (File.Exists(path)) {
												string data = File.ReadAllText(path);
												if (_maniaRGPrototype.TryLoadMapFromString(data)) { Console.WriteLine("Loadmap success!"); } else { Console.WriteLine("Failed to load map."); } }
											else Console.WriteLine("Path does not exist!"); }}
									break; }
							_chattingText = "";
							_chattingTextLines = 1;
							_isChatting = false;}
						break;
					case Keys.V:
						if (e.Modifiers.HasFlag(KeyModifiers.Control)) {
							string cbStr = ClipboardString;
							int count = cbStr.AsSpan().Count('\n');
							_chattingTextLines += count;
							_chattingText += cbStr; }
						break;
					case Keys.C: if (e.Modifiers.HasFlag(KeyModifiers.Control)) ClipboardString = _chattingText; break;
					default: break; }}
			else {
				switch (_gameMode) {
					case "pong": _pongGame.KeyInputQueue.Add(new GameKeyState1(e.Key, true)); break;
					case "v1k": _1kManiaPrototype.KeyDown(e.Key); break;
					case "mania": _maniaRGPrototype.KeyDown(e.Key,Stopwatch.GetElapsedTime(_maniaRGPrototype.timeOffset).TotalSeconds); break;
					default: break;}
				switch (e.Key)
				{
					case Keys.F6:
						if (e.Modifiers.HasFlag(KeyModifiers.Control))
						{
							profilerOn = !profilerOn;
						}
						break;
					default: break;
				}
				}}
		protected override void OnKeyUp(KeyboardKeyEventArgs e) {
			base.OnKeyUp(e);
			switch (_gameMode) {
				case "pong": _pongGame.KeyInputQueue.Add(new GameKeyState1(e.Key, false)); break;
				default: break;}}
		// protected override void OnMouseDown(MouseButtonEventArgs e){base.OnMouseDown(e);if(e.Action.HasFlag(InputAction.Repeat))Console.WriteLine("repmd("+e.Button+","+e.Modifiers+")");else Console.WriteLine("Mouse down:"+e.Button+","+ e.Modifiers);}
		// protected override void OnMouseUp(MouseButtonEventArgs e){base.OnMouseUp(e);Console.WriteLine("Mouse up: "+e.Button+", "+e.Modifiers);}
		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);
			// Clean up OpenGL resources
			_textRenderer?.Dispose();
			_cube?.Dispose();
			_tetrahedron?.Dispose();
			_plane?.Dispose();
			_playerTorsoMesh?.Dispose();
			_playerHeadMesh?.Dispose();
			_playerArmMesh?.Dispose();
			_playerLegMesh?.Dispose();
			_textureSheet?.Dispose();
			_shader?.Dispose();
			_textShader?.Dispose();}

		private void StartRecording(string output, int fps = 60)
		{
			_videoRecorder = new VideoRecorder(_clientSize.X, _clientSize.Y, fps, output, useNvenc: false, withAudio: false);
		}
		private void StopRecording()
		{
			_videoRecorder?.Stop();
			_videoRecorder?.Dispose();
			_videoRecorder = null;
		}
	}
	public class Shader {
		public readonly int Handle;
		public Shader(string vertexPath, string fragmentPath) {
			string ShaderSource = File.ReadAllText(vertexPath);
			var VertexShader = GL.CreateShader(ShaderType.VertexShader);
			GL.ShaderSource(VertexShader, ShaderSource);

			// compile shader
			GL.CompileShader(VertexShader);

			// check for errors
			GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success);
			if (success == 0) {
				string infoLog = GL.GetShaderInfoLog(VertexShader);
				throw new Exception("oh no the shader (" + VertexShader + ") failed to compile: " + infoLog);}
			// done compiling vertex shader

			ShaderSource = File.ReadAllText(fragmentPath);
			var FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
			GL.ShaderSource(FragmentShader, ShaderSource);

			// compile shader
			GL.CompileShader(FragmentShader);

			// check for errors
			GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out int success2);
			if (success2 == 0) {
				string infoLog = GL.GetShaderInfoLog(FragmentShader);
				throw new Exception("oh no the shader (" + FragmentShader + ") failed to compile: " + infoLog);}
			// done compiling fragment shader

			Handle = GL.CreateProgram();
			GL.AttachShader(Handle, VertexShader);
			GL.AttachShader(Handle, FragmentShader);
			GL.LinkProgram(Handle);

			GL.DetachShader(Handle, VertexShader);
			GL.DetachShader(Handle, FragmentShader);
			GL.DeleteShader(FragmentShader);
			GL.DeleteShader(VertexShader);}
		public void Use() {
			GL.UseProgram(Handle);}
		public int GetAttribLocation(string attribName) {
			return GL.GetAttribLocation(Handle, attribName);}
		public void SetInt(string name, int value) {
			int location = GL.GetUniformLocation(Handle, name);

			GL.Uniform1(location, value);}
		public void SetMatrix4(string name, Matrix4 value) {
			int location = GL.GetUniformLocation(Handle, name);

			GL.UniformMatrix4(location, true, ref value);}

		public void SetTextureLayer(int layer) {
			SetInt("textureLayer", layer);}
		public void SetTextureLocation(string name, Vector4 LocationAndSize) {
			int location = GL.GetUniformLocation(Handle, name);
			GL.Uniform4(location, LocationAndSize);}
		public void SetVector3(string name, Vector3 value) {
			int location = GL.GetUniformLocation(Handle, name);
			GL.Uniform3(location, value);}

		public void Dispose() {
			GL.DeleteProgram(Handle);}}
	public class Texture {
		public readonly int Handle;
		public readonly int Width;
		public readonly int Height;

		public static Texture LoadFromFile(string path, bool Grayscale, bool Mipmap) {
			int handle = GL.GenTexture();

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, handle);

			StbImage.stbi_set_flip_vertically_on_load(1);
			int width;
			int height;
			using (Stream stream = File.OpenRead(path)) {
				ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
				width = image.Width;
				height = image.Height;
				if (Grayscale) {
					byte[] data = image.Data;
					byte[] newData = new byte[data.Length / 4];
					for (int i = 0; i < newData.Length; i++) { newData[i] = data[i * 4]; }
					GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, image.Width, image.Height, 0, PixelFormat.Red, PixelType.UnsignedByte, newData);}
				else {
					GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);}}

			if (Mipmap) GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
			else GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);


			if (Mipmap) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);


			return new Texture(handle, width, height);}

		public Texture(int glHandle, int w, int h) {
			Handle = glHandle;
			Width = w;
			Height = h;}

		public void Use(TextureUnit unit) {
			GL.ActiveTexture(unit);
			GL.BindTexture(TextureTarget.Texture2D, Handle);}

		public void Dispose() {
			GL.DeleteTexture(Handle);}}
	public class Camera {
		public float CamSpeed { get; set; } = 3f;
		public Vector3 CameraToTargetOffset { get; set; } = new Vector3(0f, 0f, 1f);
		public float CameraDistFromTarget { get; set; } = 3f;
		public Vector3 CameraFront { get; set; } = new Vector3(0f, 0f, -1f);
		public float MinDist { get; } = .05f;
		public float MaxDist { get; } = 128f;
		public Vector3 Target { get; set; } = Vector3.Zero;
		public Vector3 Position { get; set; } = new Vector3(0f, 0f, 3f);
		public Vector3 Up { get; } = Vector3.UnitY;
		public Vector3 Direction { get; set; }
		public Vector3 Right { get; set; }
		public Matrix4 View { get; set; }

		public float Pitch { get; set; }
		public float Yaw { get; set; }
		public float MouseSensitivity { get; set; } = .005f;

		public float PlayerSpeed { get; set; } = 5f;
		public bool IsFlying { get; set; } = false;
		public float JumpPower { get; set; } = 5f;
		public Vector3 Gravity { get; set; } = new Vector3(0f, -9.81f, 0f);
		public Vector3 PlayerVelocity { get; set; } = Vector3.Zero;
		public bool IsFalling { get; set; } = false;
		public bool IsGrounded { get; set; } = true;
		public Matrix4 Projection { get; set; }

		public Camera(Vector3 position, Vector3 target, Vector3 up) {
			Direction = Vector3.Normalize(position - target);
			Right = Vector3.Normalize(Vector3.Cross(up, Direction));
			View = Matrix4.LookAt(position, target, up);

			Position = position;
			Target = target;
			Up = up;
			Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), 800f / 600f, .1f, 100f);}
		public void UpdateVectors() {
			Position = Target + CameraToTargetOffset * CameraDistFromTarget;
			Direction = Vector3.Normalize(Position - Target);
			Right = Vector3.Normalize(Vector3.Cross(Up, Direction));
			View = Matrix4.LookAt(Position, Target, Up);}}
	public class ObjectMesh {
		public static int MeshCount = 0;
		public static List<ObjectMesh> Meshes = [];
		public int Type { get; set; }
		public List<Vector3> Positions { get; set; }
		public List<Vector3> Rotations { get; set; }
		public List<Vector3> Scales { get; set; }
		public List<Matrix4> Models { get; set; }
		public int IndicesLen { get; set; }
		public int VertexArrayObject { get; set; }
		public int VertexBufferObject { get; set; }
		public int ElementBufferObject { get; set; }
		public ObjectMesh(Vector3 position, Vector3 rotation, Vector3 scale, float[] vertices, uint[] indices) {
			MeshCount++;
			Type = MeshCount;
			Positions = [position];
			Rotations = [rotation];
			Scales = [scale];
			Models = [Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationZ(rotation.Z) * Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(position)];
			VertexArrayObject = GL.GenVertexArray();
			GL.BindVertexArray(VertexArrayObject);

			VertexBufferObject = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

			IndicesLen = indices.Length;
			ElementBufferObject = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
			GL.BufferData(BufferTarget.ElementArrayBuffer, IndicesLen * sizeof(uint), indices, BufferUsageHint.StaticDraw);

			// Set up vertex attributes
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
			GL.EnableVertexAttribArray(1);
			GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

			Meshes.Add(this);
			// unbind to prevent later code from accidentally modifying this VAO/EBO
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);}
		public void Update(int index, Vector3 position, Vector3 rotation, Vector3 scale) {
			Positions[index] = position;
			Rotations[index] = rotation;
			Scales[index] = scale;
			Models[index] = Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationZ(rotation.Z) * Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(position);}
		public int AddNew(Vector3 position, Vector3 rotation, Vector3 scale) {
			Positions.Add(position);
			Rotations.Add(rotation);
			Scales.Add(scale);
			Models.Add(Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationZ(rotation.Z) * Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(position));
			return Models.Count - 1;}
		public int AddNewInBulk(Vector3[] positions, Vector3[] rotations, Vector3[] scales) {
			for (int i = 0; i < positions.Length; i++) {
				Positions.Add(positions[i]);
				Rotations.Add(rotations[i]);
				Scales.Add(scales[i]);
				Models.Add(Matrix4.CreateRotationX(rotations[i].X) * Matrix4.CreateRotationY(rotations[i].Y) * Matrix4.CreateRotationZ(rotations[i].Z) * Matrix4.CreateScale(scales[i]) * Matrix4.CreateTranslation(positions[i]));}
			return Models.Count - positions.Length;}
		public void Bind() {
			GL.BindVertexArray(VertexArrayObject);}
		public void Draw(Shader shader, bool bind) {
			if (bind) Bind();
			for (int i = 0; i < Models.Count; i++) {
				shader.SetMatrix4("model", Models[i]);
				GL.DrawElements(PrimitiveType.Triangles, IndicesLen, DrawElementsType.UnsignedInt, 0);}}
		public void DrawWithModel(Shader shader, Matrix4 model, bool bind) {
			if (bind) Bind();
			shader.SetMatrix4("model", model);
			GL.DrawElements(PrimitiveType.Triangles, IndicesLen, DrawElementsType.UnsignedInt, 0);}
		public void DrawWithModels(Shader shader, Matrix4[] models, bool bind) {
			if (bind) Bind();
			for (int i = 0; i < models.Length; i++) {
				shader.SetMatrix4("model", models[i]);
				GL.DrawElements(PrimitiveType.Triangles, IndicesLen, DrawElementsType.UnsignedInt, 0);}}
		public void DrawAtIndex(Shader shader, int index, bool bind) {
			if (bind) Bind();
			shader.SetMatrix4("model", Models[index]);
			GL.DrawElements(PrimitiveType.Triangles, IndicesLen, DrawElementsType.UnsignedInt, 0);}

		public void Dispose() {
			GL.DeleteBuffer(VertexBufferObject);
			GL.DeleteBuffer(ElementBufferObject);
			GL.DeleteVertexArray(VertexArrayObject);}}
	public class Obj {
		public Vector3 Velocity { get; set; }
		public ObjectMesh Mesh { get; set; }
		public int MeshIndex { get; set; }
		public Obj(Vector3 position, Vector3 rotation, Vector3 scale, ObjectMesh mesh) {
			Mesh = mesh;
			MeshIndex = mesh.AddNew(position, rotation, scale);}
		public Obj(ObjectMesh mesh, int meshIndex) {
			Mesh = mesh;
			MeshIndex = meshIndex;}
		public void Update(Vector3 position, Vector3 rotation, Vector3 scale) {
			Mesh.Update(MeshIndex, position, rotation, scale);}
		public void UpdateModel(Matrix4 model) {
			Mesh.Models[MeshIndex] = model;}
		public void StepPhysics(float deltaTime, Vector3 gravity) {
			Velocity += gravity * deltaTime;
			Mesh.Positions[MeshIndex] += Velocity * deltaTime;
			if (Mesh.Positions[MeshIndex].Y < Game._groundHeight) {
				Mesh.Positions[MeshIndex] = new Vector3(Mesh.Positions[MeshIndex].X, Game._groundHeight, Mesh.Positions[MeshIndex].Z);
				Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);}}
		public void Draw(Shader shader, bool bind) { Mesh.DrawAtIndex(shader, MeshIndex, bind); }}
	public class Player {
		public Vector3 RootPosition { get; set; }
		public Vector3 RootRotation { get; set; }
		public Matrix4 RootModel { get; set; }
		public Vector3 RootScale { get; set; }
		public Vector3 Velocity { get; set; }
		public float JumpPower { get; set; } = 5f;
		public Vector3 Gravity { get; set; } = new Vector3(0f, -9.81f, 0f);
		public bool IsGrounded { get; set; } = true;
		public Obj[] Limbs { get; set; }
		public Vector3[] LimbPositions { get; set; }
		public Vector3[] LimbRotations { get; set; }
		public Vector3[] LimbScales { get; set; }
		public Obj Torso { get; set; }
		public Obj Head { get; set; }
		public Obj LeftArm { get; set; }
		public Obj RightArm { get; set; }
		public Obj LeftLeg { get; set; }
		public Obj RightLeg { get; set; }

		public Player(Vector3 position, Vector3 rotation, Vector3 scale, Obj[] limbs, Vector3[] limbPositions, Vector3[] limbRotations, Vector3[] limbScales) {
			RootPosition = position;
			RootRotation = rotation;
			RootModel = Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationZ(rotation.Z) * Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(position);
			RootScale = scale;
			Velocity = Vector3.Zero;
			Limbs = limbs;
			Torso = Limbs[0];
			Head = Limbs[1];
			LeftArm = Limbs[2];
			RightArm = Limbs[3];
			LeftLeg = Limbs[4];
			RightLeg = Limbs[5];
			LimbPositions = limbPositions;
			LimbRotations = limbRotations;
			LimbScales = limbScales;
			for (int i = 0; i < Limbs.Length; i++) {
				Limbs[i].Update(LimbPositions[i] + RootPosition, LimbRotations[i] + RootRotation, LimbScales[i] * RootScale);}}
		public void UpdateLimb(int index) {
			Limbs[index].UpdateModel(
				Matrix4.CreateRotationX(LimbRotations[index].X) *
				Matrix4.CreateRotationY(LimbRotations[index].Y) *
				Matrix4.CreateRotationZ(LimbRotations[index].Z) *
				Matrix4.CreateScale(LimbScales[index] * RootScale) *
				Matrix4.CreateTranslation(LimbPositions[index]) *
				RootModel
			);}
		public void UpdateLimbs() { for (int i = 0; i < Limbs.Length; i++) { UpdateLimb(i); } }
		public void StepPhysics(float deltaTime) {
			if (!IsGrounded) {
				Velocity += Gravity * deltaTime;}
			RootPosition += Velocity * deltaTime;
			if (RootPosition.Y < Game._groundHeight) {
				RootPosition = new Vector3(RootPosition.X, Game._groundHeight, RootPosition.Z);
				Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);
				IsGrounded = true;}
			RootModel = Matrix4.CreateRotationX(RootRotation.X) * Matrix4.CreateRotationY(RootRotation.Y) * Matrix4.CreateRotationZ(RootRotation.Z) * Matrix4.CreateScale(RootScale) * Matrix4.CreateTranslation(RootPosition);
			UpdateLimbs();}
		public void Render(Shader shader) {
			Torso.Draw(shader, true);
			Head.Draw(shader, true);
			LeftArm.Draw(shader, true);
			RightArm.Draw(shader, false);
			LeftLeg.Draw(shader, true);
			RightLeg.Draw(shader, false);}
		public void Jump() {
			Velocity = new Vector3(Velocity.X, JumpPower, Velocity.Z);
			IsGrounded = false;}}
	public struct FontCharacterData {
		public Dictionary<char, GlyphData> Chars = [];
		public Dictionary<string, GlyphData> SChars = [];
		public FontCharacterData() { } // do not remove this line
		public FontCharacterData(Dictionary<char, GlyphData> Chars) { this.Chars = Chars; }
		public FontCharacterData(Dictionary<string, GlyphData> SChars) { this.SChars = SChars; }
		public FontCharacterData(Dictionary<char, GlyphData> Chars, Dictionary<string, GlyphData> SChars) { this.Chars = Chars; this.SChars = SChars; }}

	public class Text {
		public Texture TextTexture { get; set; }
		public static readonly char[] CharSearchThingy = ['|', '\\', '\n'];
		public const int BulkDrawConst = 4096;
		public const int MTILen = BulkDrawConst * 6;
		// the reason this is like this is because there are 4 floats in each vertex and 4 vertices for each character.
		public const int BulkDrawFloats = BulkDrawConst * 16;
		private static readonly uint[] MTI = new uint[MTILen]; // ManyTextIndices. This should only be initialized one time, because it is always the same.
		private static bool IsInitialized = false;
		public int VAO;
		public int VBO;
		public static int EBO { get; private set; }
		public List<TxtOptions> TextThingies = [];
		public Text(Texture textTexture) {
			TextTexture = textTexture;
			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * BulkDrawFloats, 0, BufferUsageHint.DynamicDraw);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);}
		static Text() {
			for (uint i = 0, j = 0; i < MTILen; i += 6, j += 4) {
				MTI[i] = j; MTI[i + 1] = j + 1; MTI[i + 2] = j + 2; MTI[i + 3] = j; MTI[i + 4] = j + 2; MTI[i + 5] = j + 3;}}
		public static void Initialize() {
			if (!IsInitialized) {
				RealInitialize();
				IsInitialized = true;}}
		public static void RealInitialize() {
			// Ensure no mesh VAO is currently bound so binding the EBO doesn't attach to a mesh VAO
			GL.BindVertexArray(0);
			EBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, MTILen * sizeof(uint), MTI, BufferUsageHint.DynamicDraw);}
		/// <summary> Renders text. </summary>
		/// <param name="shader">The shader.</param>
		/// <param name="text">The text to be rendered.</param>
		/// <param name="posOffset">Position; takes inspiration from Roblox's UDim2; offset.</param>
		/// <param name="posScale">Position; takes inspiration from Roblox's UDim2; scale. Visible range is -.5 to .5.</param>
		/// <param name="textScale">The text size. FOR BEST RESULTS USE A MULTIPLE OF 2 BECAUSE FOR SOME REASON A PIXEL IS ACTUALLY 2??</param>
		/// <param name="color">The color of the text so your text can be colorful.</param>
		/// <param name="windowSize">The size of the window. there is probably a better way to do this,,</param>
		public void RenderText(Game game, Shader shader, string text, Vector2i posOffset, Vector2 posScale, Vector2 textScale, Vector3 color, float lineHeight, Vector2i windowSize, FontCharacterData fontCharData, bool useSpecialChar = false) {
			shader.Use();
			shader.SetVector3("textColor", color);
			GL.BindVertexArray(VAO);


			Vector2 absPos = new(posOffset.X + posScale.X * windowSize.X, posOffset.Y + posScale.Y * windowSize.Y);
			float spaceSize = textScale.X * 3;
			float tabSize = spaceSize * 4;

			float[] vertices = new float[BulkDrawFloats];
			int vI = 0;

			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			if (useSpecialChar) {
				int incorrectSpecialChar = 0;
				for (int i = 0; i < text.Length; i++) {
					char c = text[i];
					GlyphData Chr;
					if (incorrectSpecialChar > 0 && --incorrectSpecialChar == 0 && c == '|') continue;
					switch (c) {
						case ' ': absPos = new(absPos.X + spaceSize, absPos.Y); continue;
						case '	': absPos = new(absPos.X + tabSize, absPos.Y); continue;
						case '\n': absPos = new(posOffset.X + posScale.X * windowSize.X, absPos.Y - textScale.Y * lineHeight); continue;
						case '\\':
							int ip1 = i + 1;
							if (ip1 >= text.Length || text[ip1] == '\\') { Chr = fontCharData.Chars['\\']; break; } // if this is the last char or the next char is another '\\' then show a '\\' char.
							char nextChar = text[ip1];
							if (nextChar == '|') { Chr = fontCharData.Chars['\\']; i++; break; } // if the next char is a | char (my format is \| for '\\' chars) then show a '\\', then increment i so the '|' isn't shown.
							if (nextChar == '\n') { i++; goto case '\n'; } // if the line goes to a new line then increment i and do the next line stuff.
							byte IsStacking = 0;
							int j = text.IndexOfAny(CharSearchThingy, ip1);
							if (j == -1) j = text.Length; else if (text[j] != '|') IsStacking = 1;
							int len = j - ip1;
							string s;
							if (len == -1) s = text[ip1..];
							else s = text.Substring(ip1, len);
							if (fontCharData.SChars.TryGetValue(s, out Chr)) { i = j - IsStacking; } // if it can find the special character then jump to the index of the last chr in the special character, and the next char will be a new one.
							else { // if it cant find the special character then color the characters red, if there is at least one character.
								shader.SetVector3("textColor", new(.5f, 0, 1f));
								incorrectSpecialChar = len + 1;
								Chr = fontCharData.Chars['\\'];}
							break;
						default: if (!fontCharData.Chars.TryGetValue(c, out Chr)) Chr = fontCharData.Chars['?']; break;}
					float startX = (MathF.Floor(absPos.X + Chr.bearing.X * textScale.X) + .5f) / windowSize.X;
					float startY = (MathF.Floor(absPos.Y + Chr.bearing.Y * textScale.Y) + .5f) / windowSize.Y;
					float endX = (MathF.Floor(absPos.X + Chr.bearing.X * textScale.X + MathF.Ceiling(Chr.size.X * textScale.X)) + .5f) / windowSize.X;
					float endY = (MathF.Floor(absPos.Y + Chr.bearing.Y * textScale.Y + MathF.Ceiling(Chr.size.Y * textScale.Y)) + .5f) / windowSize.Y;

					float tStartX = Chr.textureStart.X / (float)TextTexture.Width;
					float tStartY = Chr.textureStart.Y / (float)TextTexture.Height;
					float tEndX = (Chr.textureStart.X + Chr.textureSize.X) / (float)TextTexture.Width;
					float tEndY = (Chr.textureStart.Y + Chr.textureSize.Y) / (float)TextTexture.Height;
					vertices[vI] = startX; vertices[vI + 1] = startY; vertices[vI + 2] = tStartX; vertices[vI + 3] = tStartY;
					vertices[vI + 4] = startX; vertices[vI + 5] = endY; vertices[vI + 6] = tStartX; vertices[vI + 7] = tEndY;
					vertices[vI + 8] = endX; vertices[vI + 9] = endY; vertices[vI + 10] = tEndX; vertices[vI + 11] = tEndY;
					vertices[vI + 12] = endX; vertices[vI + 13] = startY; vertices[vI + 14] = tEndX; vertices[vI + 15] = tStartY;
					if (vI == BulkDrawFloats - 16) {
						GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * BulkDrawFloats, vertices);

						GL.DrawElements(PrimitiveType.Triangles, MTILen, DrawElementsType.UnsignedInt, 0);
						vI = 0;} else vI += 16;
					absPos += new Vector2i((int)(Chr.advance.X * textScale.X), (int)(Chr.advance.Y * textScale.Y));}
				if (vI != 0) {
					GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * vI, vertices);
					GL.DrawElements(PrimitiveType.Triangles, vI * 3 / 8, DrawElementsType.UnsignedInt, 0);}}
			else {
				for (int i = 0; i < text.Length; i++) {
					char c = text[i];
					GlyphData Chr;
					switch (c) {
						case ' ': absPos = new(absPos.X + spaceSize, absPos.Y); continue;
						case '	': absPos = new(absPos.X + tabSize, absPos.Y); continue;
						case '\n': absPos = new(posOffset.X + posScale.X * windowSize.X, absPos.Y - textScale.Y * lineHeight); continue;
						default: if (!fontCharData.Chars.TryGetValue(c, out Chr)) Chr = fontCharData.Chars['?']; break;}
					float startX = (MathF.Floor(absPos.X + Chr.bearing.X * textScale.X) + .5f) / windowSize.X;
					float startY = (MathF.Floor(absPos.Y + Chr.bearing.Y * textScale.Y) + .5f) / windowSize.Y;
					float endX = (MathF.Floor(absPos.X + Chr.bearing.X * textScale.X + MathF.Ceiling(Chr.size.X * textScale.X)) + .5f) / windowSize.X;
					float endY = (MathF.Floor(absPos.Y + Chr.bearing.Y * textScale.Y + MathF.Ceiling(Chr.size.Y * textScale.Y)) + .5f) / windowSize.Y;

					float tStartX = Chr.textureStart.X / (float)TextTexture.Width;
					float tStartY = Chr.textureStart.Y / (float)TextTexture.Height;
					float tEndX = (Chr.textureStart.X + Chr.textureSize.X) / (float)TextTexture.Width;
					float tEndY = (Chr.textureStart.Y + Chr.textureSize.Y) / (float)TextTexture.Height;

					vertices[vI] = startX; vertices[vI + 1] = startY; vertices[vI + 2] = tStartX; vertices[vI + 3] = tStartY;
					vertices[vI + 4] = startX; vertices[vI + 5] = endY; vertices[vI + 6] = tStartX; vertices[vI + 7] = tEndY;
					vertices[vI + 8] = endX; vertices[vI + 9] = endY; vertices[vI + 10] = tEndX; vertices[vI + 11] = tEndY;
					vertices[vI + 12] = endX; vertices[vI + 13] = startY; vertices[vI + 14] = tEndX; vertices[vI + 15] = tStartY;
					if (vI == BulkDrawFloats - 16) {
						GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * BulkDrawFloats, vertices);

						GL.DrawElements(PrimitiveType.Triangles, MTILen, DrawElementsType.UnsignedInt, 0);
						vI = 0;} else vI += 16;
					absPos += new Vector2i((int)(Chr.advance.X * textScale.X), (int)(Chr.advance.Y * textScale.Y));}
				if (vI != 0) {
					GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * vI, vertices);
					GL.DrawElements(PrimitiveType.Triangles, vI * 3 / 8, DrawElementsType.UnsignedInt, 0);}}
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);}
#nullable enable
		public void Render(int i, Game game, string text, Shader? shader = null, Vector2i? windowSize = null) {
			Render(TextThingies[i], game, text, shader ?? game._textShader, windowSize ?? game._clientSize);
		}
		public void Render(TxtOptions o, Game game, string text, Shader? _shader = null, Vector2i? winSize = null) {
#nullable disable
			Shader shader = _shader ?? game._textShader;
			Vector2i windowSize = winSize ?? game._clientSize;
			shader.Use();
			shader.SetVector3("textColor", o.color);
			GL.BindVertexArray(VAO);


			float absStartPosX = o.posOffsetX + o.posScaleX * windowSize.X;
			float absStartPosY = o.posOffsetY + o.posScaleY * windowSize.Y;
			float absPosX = absStartPosX;
			float absPosY = absStartPosY;
			float spaceSize = o.textScaleX * 3;
			float tabSize = spaceSize * 4;
			float newLineAmount = o.textScaleY * o.lineHeight;

			float[] vertices = new float[BulkDrawFloats];
			int vI = 0;

			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			if (o.useSpecialChar) {
				for (int i = 0; i < text.Length; i++) {
					char c = text[i];
					GlyphData Chr;
					switch (c) {
						case ' ': absPosX += spaceSize; continue;
						case '	': absPosX += tabSize; continue;
						case '\n': absPosX = absStartPosX; absPosY -= newLineAmount; continue;
						case '\\':
							int ip1 = i + 1;
							if (ip1 >= text.Length || text[ip1] == '\\') { Chr = o.fontCharData.Chars['\\']; break; } // if this is the last char or the next char is another '\\' then show a '\\' char.
							char nextChar = text[ip1];
							if (nextChar == '|') { Chr = o.fontCharData.Chars['\\']; i++; break; } // if the next char is a | char (my format is \| for '\\' chars) then show a '\\', then increment i so the '|' isn't shown.
							if (nextChar == '\n') { i++; goto case '\n'; } // if the line goes to a new line then increment i and do the next line stuff.
							byte IsStacking = 0;
							int j = text.IndexOfAny(CharSearchThingy, ip1);
							if (j == -1) j = text.Length; else if (text[j] != '|') IsStacking = 1;
							int len = j - ip1;
							string s;
							if (len == -1) s = text[ip1..];
							else s = text.Substring(ip1, len);
							// if (o.fontCharData.SChars.TryGetValue(s, out Chr)) { i = j - IsStacking; } // if it can find the special character then jump to the index of the last chr in the special character, and the next char will be a new one.
							// else
							// {
							// 	Chr = o.fontCharData.SChars["unknown"];
							// }
							i = j - IsStacking;
							if (!o.fontCharData.SChars.TryGetValue(s, out Chr)) Chr = o.fontCharData.SChars["unknown"];
							break;
						default: if (!o.fontCharData.Chars.TryGetValue(c, out Chr)) Chr = o.fontCharData.Chars['?']; break;}
					float startX = (MathF.Floor((int)absPosX + Chr.bearing.X * o.textScaleX) + .5f) / windowSize.X;
					float startY = (MathF.Floor((int)absPosY + Chr.bearing.Y * o.textScaleY) + .5f) / windowSize.Y;
					float endX = (MathF.Floor((int)absPosX + Chr.bearing.X * o.textScaleX + MathF.Ceiling(Chr.size.X * o.textScaleX)) + .5f) / windowSize.X;
					float endY = (MathF.Floor((int)absPosY + Chr.bearing.Y * o.textScaleY + MathF.Ceiling(Chr.size.Y * o.textScaleY)) + .5f) / windowSize.Y;

					float tStartX = Chr.textureStart.X / (float)TextTexture.Width;
					float tStartY = Chr.textureStart.Y / (float)TextTexture.Height;
					float tEndX = (Chr.textureStart.X + Chr.textureSize.X) / (float)TextTexture.Width;
					float tEndY = (Chr.textureStart.Y + Chr.textureSize.Y) / (float)TextTexture.Height;
					vertices[vI] = startX; vertices[vI + 1] = startY; vertices[vI + 2] = tStartX; vertices[vI + 3] = tStartY; vertices[vI + 4] = startX; vertices[vI + 5] = endY; vertices[vI + 6] = tStartX; vertices[vI + 7] = tEndY; vertices[vI + 8] = endX; vertices[vI + 9] = endY; vertices[vI + 10] = tEndX; vertices[vI + 11] = tEndY; vertices[vI + 12] = endX; vertices[vI + 13] = startY; vertices[vI + 14] = tEndX; vertices[vI + 15] = tStartY;
					if (vI == BulkDrawFloats - 16) {
						vI = 0;
						GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * BulkDrawFloats, vertices);
						GL.DrawElements(PrimitiveType.Triangles, MTILen, DrawElementsType.UnsignedInt, 0);} else vI += 16;
					absPosX += Chr.advance.X * o.textScaleX;
					absPosY += Chr.advance.Y * o.textScaleY;}
				if (vI != 0) {
					GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * vI, vertices);
					GL.DrawElements(PrimitiveType.Triangles, vI * 3 / 8, DrawElementsType.UnsignedInt, 0);}}
			else {
				for (int i = 0; i < text.Length; i++) {
					char c = text[i];
					GlyphData Chr;
					switch (c) {
						case ' ': absPosX += spaceSize; continue;
						case '	': absPosX += tabSize; continue;
						case '\n': absPosX = absStartPosX; absPosY -= newLineAmount; continue;
						default: if (!o.fontCharData.Chars.TryGetValue(c, out Chr)) Chr = o.fontCharData.Chars['?']; break;}
					float startX = (MathF.Floor((int)absPosX + Chr.bearing.X * o.textScaleX) + .5f) / windowSize.X;
					float startY = (MathF.Floor((int)absPosY + Chr.bearing.Y * o.textScaleY) + .5f) / windowSize.Y;
					float endX = (MathF.Floor((int)absPosX + Chr.bearing.X * o.textScaleX + MathF.Ceiling(Chr.size.X * o.textScaleX)) + .5f) / windowSize.X;
					float endY = (MathF.Floor((int)absPosY + Chr.bearing.Y * o.textScaleY + MathF.Ceiling(Chr.size.Y * o.textScaleY)) + .5f) / windowSize.Y;

					float tStartX = Chr.textureStart.X / (float)TextTexture.Width;
					float tStartY = Chr.textureStart.Y / (float)TextTexture.Height;
					float tEndX = (Chr.textureStart.X + Chr.textureSize.X) / (float)TextTexture.Width;
					float tEndY = (Chr.textureStart.Y + Chr.textureSize.Y) / (float)TextTexture.Height;

					vertices[vI] = startX; vertices[vI + 1] = startY; vertices[vI + 2] = tStartX; vertices[vI + 3] = tStartY;
					vertices[vI + 4] = startX; vertices[vI + 5] = endY; vertices[vI + 6] = tStartX; vertices[vI + 7] = tEndY;
					vertices[vI + 8] = endX; vertices[vI + 9] = endY; vertices[vI + 10] = tEndX; vertices[vI + 11] = tEndY;
					vertices[vI + 12] = endX; vertices[vI + 13] = startY; vertices[vI + 14] = tEndX; vertices[vI + 15] = tStartY;
					if (vI == BulkDrawFloats - 16) {
						vI = 0;
						GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * BulkDrawFloats, vertices);
						GL.DrawElements(PrimitiveType.Triangles, MTILen, DrawElementsType.UnsignedInt, 0);} else vI += 16;
					// absPos += new Vector2i((int)(Chr.advance.X * o.textScaleX), (int)(Chr.advance.Y * o.textScaleY));
					absPosX += Chr.advance.X * o.textScaleX;
					absPosY += Chr.advance.Y * o.textScaleY;}
				if (vI != 0) {
					GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * vI, vertices);
					GL.DrawElements(PrimitiveType.Triangles, vI * 3 / 8, DrawElementsType.UnsignedInt, 0);}}
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);}
		public float[][] PreCalculateVertices(string text, Vector2i posOffset, Vector2 posScale, Vector2 textScale, float lineHeight, Vector2i windowSize, FontCharacterData fontCharData, bool useSpecialChar) {
			Vector2 absPos = new(posOffset.X + posScale.X * windowSize.X, posOffset.Y + posScale.Y * windowSize.Y);
			float spaceSize = textScale.X * 3;
			float tabSize = spaceSize * 4;
			List<float[]> v = [];
			float[] vertices = new float[BulkDrawFloats];
			int vI = 0;

			if (useSpecialChar) {
				for (int i = 0; i < text.Length; i++) {
					char c = text[i];
					GlyphData Chr;
					switch (c) {
						case ' ': absPos = new(absPos.X + spaceSize, absPos.Y); continue;
						case '	': absPos = new(absPos.X + tabSize, absPos.Y); continue;
						case '\n': absPos = new(posOffset.X + posScale.X * windowSize.X, absPos.Y - textScale.Y * lineHeight); continue;
						case '\\':
							int ip1 = i + 1;
							if (ip1 >= text.Length || text[ip1] == '\\') { Chr = fontCharData.Chars['\\']; break; } // if this is the last char or the next char is another '\\' then show a '\\' char.
							char nextChar = text[ip1];
							if (nextChar == '|') { Chr = fontCharData.Chars['\\']; i++; break; } // if the next char is a | char (my format is \| for '\\' chars) then show a '\\', then increment i so the '|' isn't shown.
							if (nextChar == '\n') { i++; goto case '\n'; } // if the line goes to a new line then increment i and do the next line stuff.
							int IsStacking = 0;
							int j = text.IndexOfAny(CharSearchThingy, ip1);
							if (j == -1) j = text.Length; else if (text[j] != '|') IsStacking = 1;
							int len = j - ip1;
							string s;
							if (len == -1) s = text[ip1..];
							else s = text.Substring(ip1, len);
							if (fontCharData.SChars.TryGetValue(s, out Chr)) { i = j - IsStacking; } // if it can find the special character then jump to the index of the last chr in the special character, and the next char will be a new one.
							else Chr = fontCharData.Chars['\\'];

							break;
						default: if (!fontCharData.Chars.TryGetValue(c, out Chr)) Chr = fontCharData.Chars['?']; break;}
					float startX = (MathF.Floor(absPos.X + Chr.bearing.X * textScale.X) + .5f) / windowSize.X;
					float startY = (MathF.Floor(absPos.Y + Chr.bearing.Y * textScale.Y) + .5f) / windowSize.Y;
					float endX = (MathF.Floor(absPos.X + Chr.bearing.X * textScale.X + MathF.Ceiling(Chr.size.X * textScale.X)) + .5f) / windowSize.X;
					float endY = (MathF.Floor(absPos.Y + Chr.bearing.Y * textScale.Y + MathF.Ceiling(Chr.size.Y * textScale.Y)) + .5f) / windowSize.Y;

					float tStartX = Chr.textureStart.X / (float)TextTexture.Width;
					float tStartY = Chr.textureStart.Y / (float)TextTexture.Height;
					float tEndX = (Chr.textureStart.X + Chr.textureSize.X) / (float)TextTexture.Width;
					float tEndY = (Chr.textureStart.Y + Chr.textureSize.Y) / (float)TextTexture.Height;
					vertices[vI] = startX; vertices[vI + 1] = startY; vertices[vI + 2] = tStartX; vertices[vI + 3] = tStartY;
					vertices[vI + 4] = startX; vertices[vI + 5] = endY; vertices[vI + 6] = tStartX; vertices[vI + 7] = tEndY;
					vertices[vI + 8] = endX; vertices[vI + 9] = endY; vertices[vI + 10] = tEndX; vertices[vI + 11] = tEndY;
					vertices[vI + 12] = endX; vertices[vI + 13] = startY; vertices[vI + 14] = tEndX; vertices[vI + 15] = tStartY;
					if (vI == BulkDrawFloats - 16) {
						v.Add(vertices);
						vertices = new float[BulkDrawFloats]; // this step is VERY important! i think adding to the list just adds a pointer, and it doesn't actually clone it, so you need to create it again or else it will be filled with the same table over and over again.
						vI = 0;} else vI += 16;
					absPos += new Vector2i((int)(Chr.advance.X * textScale.X), (int)(Chr.advance.Y * textScale.Y));}
				if (vI != 0) {
					float[] verts = new float[vI];
					Array.Copy(vertices, verts, vI); // yo this is WAY faster than my attempt lol
					// for (int i = 0; i < vI; i += 16) {
					// 	verts[i] = vertices[i]; verts[i + 1] = vertices[i + 1]; verts[i + 2] = vertices[i + 2]; verts[i + 3] = vertices[i + 3];
					// 	verts[i + 4] = vertices[i + 4]; verts[i + 5] = vertices[i + 5]; verts[i + 6] = vertices[i + 6]; verts[i + 7] = vertices[i + 7];
					// 	verts[i + 8] = vertices[i + 8]; verts[i + 9] = vertices[i + 9]; verts[i + 10] = vertices[i + 10]; verts[i + 11] = vertices[i + 11];
					// 	verts[i + 12] = vertices[i + 12]; verts[i + 13] = vertices[i + 13]; verts[i + 14] = vertices[i + 14]; verts[i + 15] = vertices[i + 15];}
					v.Add(verts);}}
			else {
				for (int i = 0; i < text.Length; i++) {
					char c = text[i];
					GlyphData Chr;
					switch (c) {
						case ' ': absPos = new(absPos.X + spaceSize, absPos.Y); continue;
						case '	': absPos = new(absPos.X + tabSize, absPos.Y); continue;
						case '\n': absPos = new(posOffset.X + posScale.X * windowSize.X, absPos.Y - textScale.Y * lineHeight); continue;
						default: if (!fontCharData.Chars.TryGetValue(c, out Chr)) Chr = fontCharData.Chars['?']; break;}
					float startX = (MathF.Floor(absPos.X + Chr.bearing.X * textScale.X) + .5f) / windowSize.X;
					float startY = (MathF.Floor(absPos.Y + Chr.bearing.Y * textScale.Y) + .5f) / windowSize.Y;
					float endX = (MathF.Floor(absPos.X + Chr.bearing.X * textScale.X + MathF.Ceiling(Chr.size.X * textScale.X)) + .5f) / windowSize.X;
					float endY = (MathF.Floor(absPos.Y + Chr.bearing.Y * textScale.Y + MathF.Ceiling(Chr.size.Y * textScale.Y)) + .5f) / windowSize.Y;

					float tStartX = Chr.textureStart.X / (float)TextTexture.Width;
					float tStartY = Chr.textureStart.Y / (float)TextTexture.Height;
					float tEndX = (Chr.textureStart.X + Chr.textureSize.X) / (float)TextTexture.Width;
					float tEndY = (Chr.textureStart.Y + Chr.textureSize.Y) / (float)TextTexture.Height;

					vertices[vI] = startX; vertices[vI + 1] = startY; vertices[vI + 2] = tStartX; vertices[vI + 3] = tStartY;
					vertices[vI + 4] = startX; vertices[vI + 5] = endY; vertices[vI + 6] = tStartX; vertices[vI + 7] = tEndY;
					vertices[vI + 8] = endX; vertices[vI + 9] = endY; vertices[vI + 10] = tEndX; vertices[vI + 11] = tEndY;
					vertices[vI + 12] = endX; vertices[vI + 13] = startY; vertices[vI + 14] = tEndX; vertices[vI + 15] = tStartY;
					vI += 16;
					if (vI == BulkDrawFloats) {
						v.Add(vertices);
						vI = 0;}
					absPos += new Vector2i((int)(Chr.advance.X * textScale.X), (int)(Chr.advance.Y * textScale.Y));}
				if (vI != 0) {
					float[] verts = new float[vI];
					Array.Copy(vertices, verts, vI);
					// for (int i = 0; i < vI; i += 16) {
					// 	verts[i] = vertices[i]; verts[i + 1] = vertices[i + 1]; verts[i + 2] = vertices[i + 2]; verts[i + 3] = vertices[i + 3];
					// 	verts[i + 4] = vertices[i + 4]; verts[i + 5] = vertices[i + 5]; verts[i + 6] = vertices[i + 6]; verts[i + 7] = vertices[i + 7];
					// 	verts[i + 8] = vertices[i + 8]; verts[i + 9] = vertices[i + 9]; verts[i + 10] = vertices[i + 10]; verts[i + 11] = vertices[i + 11];
					// 	verts[i + 12] = vertices[i + 12]; verts[i + 13] = vertices[i + 13]; verts[i + 14] = vertices[i + 14]; verts[i + 15] = vertices[i + 15];}
					v.Add(verts);}}
			// return v.ToArray();

			float[][] V = new float[v.Count][];
			for (int i = 0; i < v.Count; i++) { V[i] = v[i]; }
			Console.WriteLine("v is V: " + (v.ToArray() == V));
			return V;}
		/// <summary>
		/// renders vertices using data from PreCalculateVertices. will error if you dont pass any data btw.
		/// </summary>
		/// <param name="data">data. Each float[] except for the last has to be BulkDrawFloats long.</param>
		/// <param name="shader">shader</param>
		/// <param name="color">color</param>
		public void RenderWithPrecalculatedVertices(float[][] data, Shader shader, Vector3 color) {
			shader.Use();
			shader.SetVector3("textColor", color);
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			for (int i = 0; i < data.Length; i++) {
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * data[i].Length, data[i]);
				GL.DrawElements(PrimitiveType.Triangles, data[i].Length * 3 / 8, DrawElementsType.UnsignedInt, 0);}
			// int length = data.Length;
			// for (int i = 0; i < length-1; i++) {
			// 	GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * MTILen, data[i]);
			// 	GL.DrawElements(PrimitiveType.Triangles, MTILen, DrawElementsType.UnsignedInt, 0);}
			// float[] deeta = data[length - 1];
			// int deetaLen = deeta.Length;
			// GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * deetaLen, deeta);
			// GL.DrawElements(PrimitiveType.Triangles, deetaLen * 3 / 8, DrawElementsType.UnsignedInt, 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);}
		public void RenderWithPrecalculatedLines(float[][] data, Shader shader, Vector3 color) {
			shader.Use();
			shader.SetVector3("textColor", color);
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			// for (int i = 0; i < data.Length; i++) {
			// 	GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * data[i].Length, data[i]);
			// 	GL.DrawArrays(PrimitiveType.Lines, 0, data[i].Length);}
			int length = data.Length;
			for (int i = 0; i < length-1; i++) {
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * BulkDrawFloats, data[i]);
				GL.DrawArrays(PrimitiveType.Lines, 0, BulkDrawConst);}
			float[] deeta = data[length - 1];
			int deetaLen = deeta.Length;
			GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * deetaLen, deeta);
			GL.DrawElements(PrimitiveType.Triangles, deetaLen * 3 / 8, DrawElementsType.UnsignedInt, 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);}
		public void R(float[] data, Shader shader, Vector3 color) {
			shader.Use();
			shader.SetVector3("textColor", color);
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * data.Length, data);
			GL.DrawElements(PrimitiveType.Triangles, data.Length * 3 / 8, DrawElementsType.UnsignedInt, 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);}
		public void RenderBarGraph(Game game, Vector2 position, Vector2 size, Vector3 color, long[] data)
		{
			(int windowSizeX, int windowSizeY) = (game._clientSize.X, game._clientSize.Y);
			Shader shader = game._textShader;
			shader.Use();
			shader.SetVector3("textColor", color);
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

			float absPosX = position.X * windowSizeX;
			float absPosY = position.Y * windowSizeY;

			float[] vertices = new float[BulkDrawFloats];
			int vI = 0;
			// int tRSX = 0;
			int tRSY = 32;
			// float tX = tRSX / (float)TextTexture.Width;
			float tX = 0;
			float tY = tRSY / (float)TextTexture.Height;
			float startY = (0.5f + (int)absPosY) / windowSizeY;

			// vertices[1] = startY;
			// vertices[2] = vertices[6] = tX;
			// vertices[3] = vertices[7] = tY;

			for (int i = 0; i < data.Length; i++) {
                float xPos = (0.5f + MathF.Floor(absPosX)) / windowSizeX;
				float endY = (0.5f + MathF.Floor(absPosY)) / windowSizeY;

				vertices[vI] = vertices[vI + 4] = xPos;
				vertices[vI + 1] = startY;
				vertices[vI + 2] = vertices[vI + 6] = tX;
				vertices[vI + 3] = vertices[vI + 7] = tY;
				vertices[vI + 5] = endY;
				if (vI == BulkDrawFloats - 8) { vI = 0;
					GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * BulkDrawFloats, vertices);
					GL.DrawArrays(PrimitiveType.Lines, 0, BulkDrawFloats); /*it is still bulkdrawfloats because i have an array thing of some length and i'll use ALL of it.*/
				}
				else vI += 8;
				// absPos += new Vector2i((int)(Chr.advance.X * o.textScaleX), (int)(Chr.advance.Y * o.textScaleY));
				absPosX += 1;
				if (vI != 0) {
					GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * vI, vertices);
					GL.DrawElements(PrimitiveType.Triangles, vI * 3 / 8, DrawElementsType.UnsignedInt, 0);}}
			

			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);}
		public int NewTxtThing(TxtOptions O) {
			TextThingies.Add(O);
			return TextThingies.Count - 1;}

		public void Dispose() {
			GL.DeleteBuffer(VBO);
			GL.DeleteBuffer(EBO);
			GL.DeleteVertexArray(VAO);
			// Reset static initialization flag for next instance
			IsInitialized = false;}}
	public struct TxtOptions(Vector2i posOffset, Vector2 posScale, Vector2 textScale, Vector3 color, float lineHeight, FontCharacterData fontCharData, bool useSpecialChar = false) {
		public int posOffsetX = posOffset.X;
		public int posOffsetY = posOffset.Y;
		public float posScaleX = posScale.X;
		public float posScaleY = posScale.Y;
		public float textScaleX = textScale.X;
		public float textScaleY = textScale.Y;
		public Vector3 color = color;
		public float lineHeight = lineHeight;
		public bool useSpecialChar = useSpecialChar;
		public FontCharacterData fontCharData = fontCharData;}
	public struct GameKeyState1(Keys key, bool down) {
		public Keys Key = key;
		public bool Down = down;}
	public struct GameKeyState2(Keys key, bool down, double thingy) {
		public Keys Key = key;
		public double Time = thingy;
		public bool Down = down;}
	[Flags]
	public enum DebugFlags {
		none = 0,
		debugText = 1,
		debugLogging = 2,}
	// the following is pretty much 99% ai code so uhhhh
	public sealed class VideoRecorder : IDisposable
	{
		private readonly int _width;
		private readonly int _height;
		private readonly int _fps;
		private readonly string _outputPath;
		private readonly Process _ffmpeg;
		private readonly Stream _stdin;
		private readonly byte[] _frameBuffer;
		private readonly object _lock = new();
		public bool _debugLoggingMode = false;

		private bool _recording;
		private long _nextTickNs;
		private readonly long _tickStepNs;
		public bool IsRecording => _recording && !_ffmpeg.HasExited;

		public VideoRecorder(int width, int height, int fps, string outputPath, bool useNvenc = true, bool withAudio = false)
		{
			_width = width;
			_height = height;
			_fps = fps;
			_outputPath = outputPath;

			_frameBuffer = new byte[_width * _height * 4]; // BGRA
			_tickStepNs = (long)(1_000_000_000.0 / fps);

			_ffmpeg = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "ffmpeg",
					Arguments = BuildFfmpegArgs(useNvenc, withAudio, width, height, fps, outputPath),
					RedirectStandardInput = true,
					RedirectStandardOutput = true,  // Add this
					RedirectStandardError = true,   // Add this
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};
			_ffmpeg.Start();

			Console.WriteLine("FFmpeg args: " + _ffmpeg.StartInfo.Arguments);
			if (_ffmpeg.HasExited)
			{
				throw new InvalidOperationException($"FFmpeg failed to start. Exit code: {_ffmpeg.ExitCode}");
			}

			_stdin = _ffmpeg.StandardInput.BaseStream;
			_recording = true;

			// Start reading error output in background to see what went wrong
			_ffmpeg.BeginErrorReadLine();
			_ffmpeg.ErrorDataReceived += (sender, e) =>
			{
				if (!string.IsNullOrEmpty(e.Data))
					Console.WriteLine("FFmpeg out: " + e.Data);
			};

			_nextTickNs = Stopwatch.GetTimestamp() * 1_000_000_000L / Stopwatch.Frequency;
		}

		private static string BuildFfmpegArgs(bool useNvenc, bool withAudio, int w, int h, int fps, string outPath)
		{
			// Raw BGRA frames on stdin
			// Video encoder: NVENC (if available) or libx264 fallback. yuv420p ensures wide compatibility.
			// Optional Windows audio capture: WASAPI default device or DirectShow loopback device.
			string videoEnc = useNvenc
				? "-c:v h264_nvenc -preset p5 -cq 19 "
				: "-c:v libx265 -preset veryfast -crf 18 ";

			// string audioIn = withAudio
			// 	? "-f wasapi -i default -c:a aac -b:a 192k" // yes the ai did write the comment right next to this one.
			// 	: "-f lavfi -t 0.0001 -i anullsrc -shortest -c:a aac -b:a 128k"; // silent track to avoid some players complaining
			string audioIn = withAudio
				? "-f wasapi -i default -c:a aac -b:a 192k "
				: "-an ";

			// return $"-y -f rawvideo -pix_fmt bgra -s {w}x{h} -r {fps} -i - " +
			// 	$"{audioIn} " +
			// 	$"{videoEnc} -pix_fmt yuv420p -movflags +faststart \"{outPath}\"";

			// return "-y -f rawvideo -pix_fmt bgra -s " + w + "x" + h + " -r " + fps + " -i - " +
			// 	audioIn + " " +
			// 	videoEnc + " -pix_fmt yuv420p -movflags +faststart \"" + outPath + "\"";

			// for debugging purposes
			string strThing = "-n -f rawvideo -pix_fmt bgra -s " + w + "x" + h + " -r " + fps + " -i - " +
				audioIn +
				videoEnc + "-pix_fmt yuv420p \"" + outPath + "\"";
			Console.WriteLine("ffmpeg args: " + strThing);
			return strThing;
		}

		public void CaptureFrame(Vector2i clientSize, bool recordAllFrames)
		{
			if (!_recording) return;
			
			// Check if FFmpeg process is still alive
			if (_ffmpeg.HasExited)
			{
				_recording = false;
				Console.WriteLine("FFmpeg exited with code: " + _ffmpeg.ExitCode);
				return;
			}

			// Optional: throttle to target fps when vsync is off
			if (!recordAllFrames)
			{
				long now = Stopwatch.GetTimestamp();
				long freq = Stopwatch.Frequency;
				long nowNs = now * 1_000_000_000L / freq;
				// if (nowNs < _nextTickNs)
				// { Console.Write(";RT`" + now + ";" + nowNs + ";" + freq + ";" + _nextTickNs + ";" + _tickStepNs + "`;"); return; }
				// else Console.Write(";`NR`");
				if (nowNs < _nextTickNs)
				{ Console.Write("`RT"); return; }
				else Console.Write("`NR");
				_nextTickNs += _tickStepNs;
			}

			// Ensure we read exactly the configured size; if window resizes, you may want to recreate the recorder.
			if (clientSize.X != _width || clientSize.Y != _height) return;

			lock (_lock)
			{
				try
				{
					// Read back buffer (BGRA)
					GL.ReadBuffer(ReadBufferMode.Back);
					GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
					GL.ReadPixels(0, 0, _width, _height, PixelFormat.Bgra, PixelType.UnsignedByte, _frameBuffer);

					// Flip vertically because OpenGL origin is bottom-left, raw expects top-left
					FlipInPlaceBgra(_frameBuffer, _width, _height);

					// Write to ffmpeg with error handling
					_stdin.Write(_frameBuffer, 0, _frameBuffer.Length);
					Console.WriteLine("wrote smth hopefully idk");
				}
				catch (IOException ex)
				{
					Console.WriteLine("Pipe error: " + ex.Message);
					_recording = false;
				}
				catch (Exception ex)
				{
					Console.WriteLine("Capture error: " + ex.Message);
					_recording = false;
				}
			}
		}

		private static void FlipInPlaceBgra(byte[] data, int w, int h)
		{
			int stride = w * 4;
			int half = h / 2;
			for (int y = 0; y < half; y++)
			{
				int top = y * stride;
				int bot = (h - 1 - y) * stride;
				for (int x = 0; x < stride; x++)
				{
                    (data[bot + x], data[top + x]) = (data[top + x], data[bot + x]);
                    (data[bot + ++x], data[top + x]) = (data[top + x], data[bot + x]);
                    (data[bot + ++x], data[top + x]) = (data[top + x], data[bot + x]);
                    (data[bot + ++x], data[top + x]) = (data[top + x], data[bot + x]);
                } // partial unrolling of 4. idk if this makes a difference, if it makes the performance better slightly, or even worse slightly but i hope not...
            }
		}

		public void Stop()
		{
			if (!_recording) return;
			_recording = false;
			try { _stdin.Flush(); } catch {}
			try { _stdin.Close(); } catch {}
			try { if (!_ffmpeg.WaitForExit(5000)) _ffmpeg.Kill(true); } catch {}
		}

		public void Dispose()
		{
			Stop();
			_ffmpeg?.Dispose();
		}
	}
}
