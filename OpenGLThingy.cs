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
		public double _gameTime = .0;
		private double _semiRealTime = .0;
		private double _dT = .0;
		public Shader _shader;
		public Shader _textShader { get; private set; }
		private Texture _textureSheet;
		public Text _textRenderer;
		public Camera _camera = new(new Vector3(0f, 0f, 3f), Vector3.Zero, Vector3.UnitY);
		public ObjectMesh _cube;
		public ObjectMesh _tetrahedron;
		public ObjectMesh _plane;
		private double _DTOverTime = 0;
		public long _frameCount = 0;
		// private Random _random = new();
		private Stopwatch _stopwatch = new();
		// private bool _gameUpdating = true;
		public long _gameTick { get; private set; } = 0;
		public float _gameTickSpeed { get; private set; } = 60f;
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
		public string _chattingText { get; private set; } = "";
		private int _chattingTextLines = 1;
		private Vector2 _chattingTextSize = new(2);
		private float _chattingTextLineHeight = 10f;
		public bool WillReopen = false;
		public string ReopenData = "";
		public string OpenData;
		public List<string> _gameModes;
		public DebugFlags _debugFlags = DebugFlags.none;

		private WindowState previousState;
		// private Pong _pongGame;
		// private VerticalOneKey _1kManiaPrototype;
		// private ManiaRG _maniaRGPrototype;
		public List<IMinigame> _currentMinigames { get; private set; }
		public int _minigameCount { get; private set; }
		private VideoRecorder _videoRecorder;
		private long previousFrameTimestamp = 0;
		public double[] profilerFrameTimes = new double[2048];
		public int profilerIndex = 0;
		private bool profilerOn = false;
		public readonly long gameStartTimestamp = Stopwatch.GetTimestamp();

		public List<Announcement> _announcementsThing;


		public Game(int width, int height, string title) :
		base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title }) { _gameID = _gameCount++; }


		static void Main() {
			Console.WriteLine("Starting OpenGL application...");
			bool Opening = true;
			string OpenData = "";
			while (Opening) {
				Opening = false;
				using (Game game = new(800, 600, "GameEngineThingy :3")) {
					game.VSync = VSyncMode.On;
					game.OpenData = OpenData;
					game.Run();
					Opening = game.WillReopen;
					OpenData = game.ReopenData; } }
			Console.WriteLine("game has closed.");
			debuggingThingClass.someDebugThing(debuggingThingClass.IsDebugging); }
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


			// _camera.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), 800f / 600f, .1f, 100f);
			_camera.UpdateVectors();

			_camera.Direction = Vector3.Normalize(_camera.Position - _camera.Target);
			_camera.Right = Vector3.Normalize(Vector3.Cross(_camera.Up, _camera.Direction));
			_camera.View = Matrix4.LookAt(_camera.Position, _camera.Target, _camera.Up);
			_clientSize = ClientSize;

			Console.WriteLine("Max vertices: " + GL.GetInteger(GetPName.MaxElementsVertices));
			Console.WriteLine("Max indices: " + GL.GetInteger(GetPName.MaxElementsIndices));

			Console.WriteLine("FontCharacterData info: FontCharDeeta has " + FontCharFillerThing.FontCharDeeta.Chars.Count + " normal characters, and " + FontCharFillerThing.FontCharDeeta.SChars.Count + " special characters.");
			float _lineHeight = 10f;

			_announcementsThing = [new("testing testing", Stopwatch.GetTimestamp() + Stopwatch.Frequency*12, (.8f, .2f, .8f), (.1f, .1f, .1f), .8f, fot:4),new("tst test2 dsfsdfsdds", Stopwatch.GetTimestamp() + Stopwatch.Frequency*30,(.3f,.5f,.3f),(.5f,.5f,.5f),.5f, fot:0)];

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
					_currentMinigames = [new Pong(new Vector3(10f, 0f, 10f), new Vector3(1f), new Vector3(270f, 0f, 0f))];
					_gameModes = ["pong"];
					break;
				case "fnf": // do something else but rn the something else doesn't exist
				case "mania": // also do something else but the something else also doesn't exist
					_currentMinigames = [new ManiaRG(_textRenderer)];
					_gameModes = ["mania"];
					break;
				case "1k fnf" or "1kfnf" or "fnf 1k" or "fnf1k" or
					"v1k" or "verticalonekey" or "vertical one key":
					_currentMinigames = [new VerticalOneKey(_textRenderer, DataStuff.BuiltInV1KCharts[0])];
					_gameModes = ["v1k"];
					break;
				case "v1k2":
					_currentMinigames = [new VerticalOneKey(_textRenderer, DataStuff.BuiltInV1KCharts[1])];
					_gameModes = ["v1k"];
					break;
				case "miner": // also do something else but the something else also doesn't exist
					_currentMinigames = [new MiningGame()];
					_gameModes = ["miner"];
					break;
				default:
					_currentMinigames = [new DefaultGameBehavior()];
					_gameModes = ["DEFAULT_BEHAVIOR"];
					break;
			}
			foreach (IMinigame minigame in _currentMinigames)
			minigame.OnLoad(this);

			_stopwatch.Start();}
		protected override void OnRenderFrame(FrameEventArgs e) {
			base.OnRenderFrame(e);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			_textureSheet.Use(TextureUnit.Texture0);
			// _textRenderer.TextTexture.Use(TextureUnit.Texture1); // should already be used though idk

			_shader.Use();
			_shader.SetMatrix4("view", _camera.View);
			// _shader.SetMatrix4("projection", _camera.Projection);


			_shader.SetTextureLocation("tx0", new Vector4(0f, 0f, 1f, 1f));
			_player.Render(_shader);

			foreach(IMinigame minigame in _currentMinigames)
			minigame.OnRenderFrame(this, e.Time);

			// UI time!! :3
			// remember that this renderer is pretty weird or smth and if you render in the wrong order the game may not render properly.
			if (profilerOn) {
				_textRenderer.RenderProfilerIndexedLoopingGraph(this, new(1f, 1f), new(-1f, -1f / 30f), new(0.8f, 0.4f, 0f), profilerFrameTimes, profilerIndex, length: profilerFrameTimes.Length);
				int pl = profilerFrameTimes.Length;
				int pi = profilerIndex + pl - 1;
				double a = profilerFrameTimes[pi%pl];
				string txt = "FPS1: " + (1000d / a);
				int i;
				for(i=pi-1;i>pi-10;i--){a+=profilerFrameTimes[i%pl];}txt+="\nFPS10: "+(10000d/a);
				for(i=pi-10;i>pi-30;i--){a+=profilerFrameTimes[i%pl];}txt+="\nFPS30: "+(30000d/a);
				for(i=pi-30;i>pi-50;i--){a+=profilerFrameTimes[i%pl];}txt+="\nFPS50: "+(50000d/a);
				for(i=pi-50;i>pi-100;i--){a+=profilerFrameTimes[i%pl];}txt+="\nFPS100: "+(100000d/a);
				_textRenderer.RenderText(this,_textShader,txt,new(0,-40),new(-1,1f),new(4),new Vector3(1,1,0),10f,_clientSize,FontCharFillerThing.FontCharDeeta,true);}
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
			_textRenderer.AnnouncementsRender(_announcementsThing, this, _textShader, 10f, _clientSize);
			// _videoRecorder?.CaptureFrame(this.ClientSize, VSync == VSyncMode.On);
			_videoRecorder?.CaptureFrame(this.ClientSize, false);
			long timestampNow = Stopwatch.GetTimestamp();
			profilerFrameTimes[profilerIndex] = (timestampNow - previousFrameTimestamp) / (double)Stopwatch.Frequency * 1000;
			previousFrameTimestamp = timestampNow;
			profilerIndex = (profilerIndex + 1) % profilerFrameTimes.Length;
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
				if (KeyboardState.IsKeyPressed(Keys.F11) || KeyboardState.IsKeyDown(Keys.GraveAccent) && KeyboardState.IsKeyDown(Keys.F11))
					if (WindowState == WindowState.Fullscreen) WindowState = previousState;
					else { previousState = WindowState; WindowState = WindowState.Fullscreen; VSync = VSyncMode.On; }
				if (_isChatting) {
					if (KeyboardState.IsKeyPressed(Keys.Escape)) { _isChatting = false; }}
				else {
					float TickSpeedInv = 1f / _gameTickSpeed;
					if (MouseState.ScrollDelta.Y != 0) { // (3f * TickSpeedInv + 1)
						_camera.CamSpeed = Math.Max(.1f, Math.Min(1024f, _camera.CamSpeed * (MouseState.ScrollDelta.Y * 0.1f + 1)));
						if (_debugFlags.HasFlag(DebugFlags.debugLogging)) Console.WriteLine("scroll speed changed; new speed: " + _camera.CamSpeed);}
					if (MouseState.IsButtonDown(MouseButton.Right)){
						float deltaX = MouseState.X - MouseState.PreviousX;
						float deltaY = MouseState.Y - MouseState.PreviousY;
						if (deltaX != 0 || deltaY != 0) {
							_camera.Yaw = (_camera.Yaw + deltaX * _camera.MouseSensitivity) % (float)(2.0 * Math.PI);
							_camera.Pitch = Math.Max(MathHelper.DegreesToRadians(-89f), Math.Min(MathHelper.DegreesToRadians(89f), _camera.Pitch - deltaY * _camera.MouseSensitivity));

							_camera.CameraToTargetOffset = -Vector3.Normalize(new Vector3(
								(float)Math.Cos(_camera.Pitch) * (float)Math.Cos(_camera.Yaw),
								(float)Math.Sin(_camera.Pitch),
								(float)Math.Cos(_camera.Pitch) * (float)Math.Sin(_camera.Yaw)));}}
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
					foreach (IMinigame minigame in _currentMinigames)
					minigame.OnEngineTick(this, TickSpeedInv);
					// camera zoom n stuff
					if (KeyboardState.IsKeyDown(Keys.I)) _camera.CameraDistFromTarget = Math.Max(_camera.MinDist, _camera.CameraDistFromTarget * (_gameTickSpeed / (_gameTickSpeed + 3f)));
					if (KeyboardState.IsKeyDown(Keys.O)) _camera.CameraDistFromTarget = Math.Min(_camera.MaxDist, _camera.CameraDistFromTarget * (3f * TickSpeedInv + 1));
					if (_debugFlags.HasFlag(DebugFlags.debugLogging) && _gameTick % (long)(_gameTickSpeed * 2) == 0) Console.WriteLine("Time: " + _stopwatch.Elapsed.TotalSeconds); // print time every 2 seconds

					// movement
					float moveAmount = _camera.CamSpeed * TickSpeedInv;
					if (_camera.IsFlying) {
						// if (KeyboardState.IsKeyDown(Keys.W)) _camera.Target -= _camera.Direction * moveAmount;
						// if (KeyboardState.IsKeyDown(Keys.S)) _camera.Target += _camera.Direction * moveAmount;
						// if (KeyboardState.IsKeyDown(Keys.A)) _camera.Target -= _camera.Right * moveAmount;
						// if (KeyboardState.IsKeyDown(Keys.D)) _camera.Target += _camera.Right * moveAmount;
						// if (KeyboardState.IsKeyDown(Keys.Space) || KeyboardState.IsKeyDown(Keys.E)) _camera.Target += _camera.Up * moveAmount;
						// if (KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.Q)) _camera.Target -= _camera.Up * moveAmount;
						float movement = (KeyboardState.IsKeyDown(Keys.W) ? -moveAmount : 0) + (KeyboardState.IsKeyDown(Keys.S) ? moveAmount : 0);
						Vector3 plrMovement = Vector3.Zero;
						plrMovement += _camera.Direction * movement;
						movement = (KeyboardState.IsKeyDown(Keys.A) ? -moveAmount : 0) + (KeyboardState.IsKeyDown(Keys.D) ? moveAmount : 0);
						plrMovement += _camera.Right * movement;
						movement = ((KeyboardState.IsKeyDown(Keys.Space) || KeyboardState.IsKeyDown(Keys.E)) ? moveAmount : 0) + ((KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.Q)) ? -moveAmount : 0);
						plrMovement += _camera.Up * movement;
						_player.RootPosition += plrMovement;
						_player.RootModel = Matrix4.CreateRotationX(_player.RootRotation.X) * Matrix4.CreateRotationY(_player.RootRotation.Y) * Matrix4.CreateRotationZ(_player.RootRotation.Z) * Matrix4.CreateScale(_player.RootScale) * Matrix4.CreateTranslation(_player.RootPosition);
						_player.UpdateLimbs();
					} else {
						// if (KeyboardState.IsKeyDown(Keys.W)) _player.RootPosition -= Vector3.Normalize(new Vector3(_camera.Direction.X, 0f, _camera.Direction.Z)) * moveAmount;
						// if (KeyboardState.IsKeyDown(Keys.S)) _player.RootPosition += Vector3.Normalize(new Vector3(_camera.Direction.X, 0f, _camera.Direction.Z)) * moveAmount;
						// if (KeyboardState.IsKeyDown(Keys.A)) _player.RootPosition -= Vector3.Normalize(new Vector3(_camera.Right.X, 0f, _camera.Right.Z)) * moveAmount;
						// if (KeyboardState.IsKeyDown(Keys.D)) _player.RootPosition += Vector3.Normalize(new Vector3(_camera.Right.X, 0f, _camera.Right.Z)) * moveAmount;
						float movement = (KeyboardState.IsKeyDown(Keys.W) ? -moveAmount : 0) + (KeyboardState.IsKeyDown(Keys.S) ? moveAmount : 0);
						Vector3 plrMovement = Vector3.Zero;
						if (movement != 0) {
							(float x, float z) = (_camera.Direction.X, _camera.Direction.Z);
							float s = movement / MathF.Sqrt(x * x + z * z);
							plrMovement = new Vector3(x*s, 0, z*s);
						}
						movement = (KeyboardState.IsKeyDown(Keys.A) ? -moveAmount : 0) + (KeyboardState.IsKeyDown(Keys.D) ? moveAmount : 0);
						if (movement != 0) {
							(float x, float z) = (_camera.Right.X, _camera.Right.Z);
							float s = movement / MathF.Sqrt(x * x + z * z);
							plrMovement += new Vector3(x*s, 0, z*s);
						}
						_player.RootPosition += plrMovement;
						if (KeyboardState.IsKeyDown(Keys.Space) && _player.IsGrounded) _player.Jump();
						_player.StepPhysics(TickSpeedInv);}
					_camera.Target = _player.RootPosition;
					_camera.UpdateVectors(); // update cam

					foreach (IMinigame minigame in _currentMinigames)
					minigame.OnUpdateFrame(this, e.Time);

					if (_isChatting) _chattingBlinker += 3;}}
			else {/* window is not focused */}}
		protected override void OnResize(ResizeEventArgs e) {
			base.OnResize(e);
			_clientSize = ClientSize;
			GL.Viewport(0, 0, _clientSize.X, _clientSize.Y);
			_camera.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), _clientSize.X / (float)_clientSize.Y, .1f, 10000f);}
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
							bool debug = _debugFlags.HasFlag(DebugFlags.debugLogging);
							if (debug) Console.WriteLine("chattxt: " + _chattingText + "; lower: " + lowercaseChatTxt);
							bool continueAfter = true;
							if (DataStuff.noInputChatCommands.TryGetValue(lowercaseChatTxt, out (Action<Game> action, bool breakOut) noInputCmd)) {
								if (debug) Console.WriteLine("Found a no-input command for " + lowercaseChatTxt + "; breakOut: " + noInputCmd.breakOut);
								noInputCmd.action(this); continueAfter = !noInputCmd.breakOut; }
							else if (debug) Console.WriteLine("Did not find a no-input command for " + lowercaseChatTxt + ".");
							if (continueAfter) {
								if (debug) Console.WriteLine("Searching for input commands...");
								if (DataStuff.chatCommands.TryGetValue(lowercaseChatTxt, out (Action<Game, string> action, bool breakOut) inputCmd))
								{
									if (debug) Console.WriteLine("Found command " + lowercaseChatTxt + ". breakOut: " + inputCmd.breakOut);
									inputCmd.action(this, "");
									if (inputCmd.breakOut) break;
								} else if (debug) Console.WriteLine(lowercaseChatTxt + " is not a valid input command...");
								for (int i = lowercaseChatTxt.Length-1; i > 0; i--) {
									string s = lowercaseChatTxt[..i];
									if (DataStuff.chatCommands.TryGetValue(s, out inputCmd)) {
										if (debug) Console.WriteLine("Found command " + s + ". breakOut: " + inputCmd.breakOut);
										inputCmd.action(this, lowercaseChatTxt[i..]);
										if (inputCmd.breakOut) break; } else if (debug) Console.WriteLine(s + " is not a valid input command..."); } }
							if (debug) Console.WriteLine("finalizing or smth idk");
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
				foreach (IMinigame minigame in _currentMinigames) minigame.OnKeyDown(e);
				switch (e.Key) {
					case Keys.F6: if (e.Modifiers.HasFlag(KeyModifiers.Control)) { profilerOn = !profilerOn; } break;
					default: break; } }}
		protected override void OnKeyUp(KeyboardKeyEventArgs e) {
			base.OnKeyUp(e);
			foreach (IMinigame minigame in _currentMinigames) minigame.OnKeyUp(e);
		}
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			foreach (IMinigame minigame in _currentMinigames) minigame.OnMouseDown(e);
			// if (e.Action.HasFlag(InputAction.Repeat))
			// 	Console.WriteLine("repmd(" + e.Button + "," + e.Modifiers + ")");
			// else Console.WriteLine("Mouse down:" + e.Button + "," + e.Modifiers);
		}
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);
			foreach (IMinigame minigame in _currentMinigames) minigame.OnMouseUp(e);
			// Console.WriteLine("Mouse up: " + e.Button + ", " + e.Modifiers);
		}
		protected override void OnClosing(CancelEventArgs e) {
			long ts0 = Stopwatch.GetTimestamp();
			base.OnClosing(e);
			// Clean up OpenGL resources
			if (_debugFlags.HasFlag(DebugFlags.debugLogging)) {
				long ts1 = Stopwatch.GetTimestamp();
				foreach (IMinigame minigame in _currentMinigames) minigame.OnClosing(e);
				double t0 = Stopwatch.GetElapsedTime(ts1).TotalMilliseconds;
				long ts2 = Stopwatch.GetTimestamp();
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
				_textShader?.Dispose();
				double t1 = Stopwatch.GetElapsedTime(ts0).TotalMilliseconds;
				double t2 = Stopwatch.GetElapsedTime(ts1).TotalMilliseconds;
				double t3 = Stopwatch.GetElapsedTime(ts2).TotalMilliseconds;
				Console.WriteLine("Closing time: "+t1+"ms; game: "+t2+"ms; minigame took "+t0+"ms, or "+t0/t1*100+"% time. Disposing took "+t3+"ms.");
			} else {
				foreach (IMinigame minigame in _currentMinigames) minigame.OnClosing(e);
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
				_textShader?.Dispose();}}

		public void StartRecording(string output, int fps = 60, float speed = 1) {
			_videoRecorder = new VideoRecorder(_clientSize.X, _clientSize.Y, fps, output, speed, useNvenc: false, withAudio: false); }
		public void StopRecording() {
			_videoRecorder?.Stop();
			_videoRecorder?.Dispose();
			_videoRecorder = null; } }
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
		public Vector3 CameraToTargetOffset { get; set; } = new Vector3(0f, 0f, -1f);
		public float CameraDistFromTarget { get; set; } = 3f;
		// public Vector3 CameraFront { get; set; } = new Vector3(0f, 0f, -1f);
		public float MinDist = .05f;
		public float MaxDist = 128f;
		public Vector3 Target { get; set; } = Vector3.Zero;
		public Vector3 Position { get; set; } = new Vector3(0f, 0f, 3f);
		public Vector3 Up { get; } = Vector3.UnitY;
		public Vector3 Direction { get; set; }
		public Vector3 Right { get; set; }
		/// <summary>
		/// The matrix to project objects to the screen. It is the view but also multiplied by the projection matrix. idk if it helps but yeah.
		/// </summary>
		public Matrix4 View { get; set; }

		public float Pitch { get; set; }
		public float Yaw { get; set; }
		public float MouseSensitivity { get; set; } = .005f;

		// public float PlayerSpeed { get; set; } = 5f;
		public bool IsFlying { get; set; } = false;
		// public float JumpPower { get; set; } = 5f;
		// public Vector3 Gravity { get; set; } = new Vector3(0f, -9.81f, 0f);
		// public Vector3 PlayerVelocity { get; set; } = Vector3.Zero;
		// public bool IsFalling { get; set; } = false;
		// public bool IsGrounded { get; set; } = true;
		public Matrix4 Projection { get; set; }

		public Camera(Vector3 position, Vector3 target, Vector3 up) {
			Direction = Vector3.Normalize(position - target);
			Right = Vector3.Normalize(Vector3.Cross(up, Direction));
			Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), 800f / 600f, .1f, 10000f);
			View = Matrix4.LookAt(position, target, up) * Projection;

			Position = position;
			Target = target;
			Up = up; }
		public void UpdateVectors() {
			Position = Target + CameraToTargetOffset * CameraDistFromTarget;
			Direction = Vector3.Normalize(Position - Target);
			Right = Vector3.Normalize(Vector3.Cross(Up, Direction));
			View = Matrix4.LookAt(Position, Target, Up) * Projection;}}
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
	public sealed class VideoRecorder : IDisposable {
		private readonly int _width;
		private readonly int _height;
		private readonly int _fps;
		private readonly float _speed; // speed not done by ai
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

		public VideoRecorder(int width, int height, int fps, string outputPath, float speed = 1, bool useNvenc = true, bool withAudio = false) {
			_width = width;
			_height = height;
			_fps = fps;
			_speed = speed;
			_outputPath = outputPath;

			_frameBuffer = new byte[_width * _height * 4]; // BGRA
			_tickStepNs = (long)(1_000_000_000.0 / fps);

			_ffmpeg = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "ffmpeg",
					Arguments = BuildFfmpegArgs(useNvenc, withAudio, width, height, fps * speed, outputPath),
					RedirectStandardInput = true,
					RedirectStandardOutput = true,  // Add this
					RedirectStandardError = true,   // Add this
					UseShellExecute = false,
					CreateNoWindow = true } };
			_ffmpeg.Start();

			Console.WriteLine("FFmpeg args: " + _ffmpeg.StartInfo.Arguments);
			if (_ffmpeg.HasExited) {
				throw new InvalidOperationException($"FFmpeg failed to start. Exit code: {_ffmpeg.ExitCode}"); }

			_stdin = _ffmpeg.StandardInput.BaseStream;
			_recording = true;

			// Start reading error output in background to see what went wrong
			_ffmpeg.BeginErrorReadLine();
			_ffmpeg.ErrorDataReceived += (sender, e) => {
				if (!string.IsNullOrEmpty(e.Data))
					Console.WriteLine("FFmpeg out: " + e.Data); };

			_nextTickNs = Stopwatch.GetTimestamp() * 1_000_000_000L / Stopwatch.Frequency; }
		private static string BuildFfmpegArgs(bool useNvenc, bool withAudio, int w, int h, float fps, string outPath) {
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
			string strThing = "-n -f rawvideo -pix_fmt bgra -s " + w + "x" + h + $" -r {fps:N4} -i - " +
				audioIn +
				videoEnc + "-pix_fmt yuv420p \"" + outPath + "\"";
			Console.WriteLine("ffmpeg args: " + strThing);
			return strThing; }
		public void CaptureFrame(Vector2i clientSize, bool recordAllFrames) {
			if (!_recording) return;

			// Check if FFmpeg process is still alive
			if (_ffmpeg.HasExited) {
				_recording = false;
				Console.WriteLine("FFmpeg exited with code: " + _ffmpeg.ExitCode);
				return; }

			// Optional: throttle to target fps when vsync is off
			if (!recordAllFrames) {
				long now = Stopwatch.GetTimestamp();
				long freq = Stopwatch.Frequency;
				long nowNs = now * 1_000_000_000L / freq;
				if (nowNs < _nextTickNs) return;
				_nextTickNs += _tickStepNs; }

			// Ensure we read exactly the configured size; if window resizes, you may want to recreate the recorder.
			if (clientSize.X != _width || clientSize.Y != _height) return;

			lock (_lock) {
				try {
					// Read back buffer (BGRA)
					GL.ReadBuffer(ReadBufferMode.Back);
					GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
					GL.ReadPixels(0, 0, _width, _height, PixelFormat.Bgra, PixelType.UnsignedByte, _frameBuffer);

					// Flip vertically because OpenGL origin is bottom-left, raw expects top-left
					FlipInPlaceBgra(_frameBuffer, _width, _height);

					// Write to ffmpeg with error handling
					_stdin.Write(_frameBuffer, 0, _frameBuffer.Length);
					/* Console.WriteLine("wrote smth hopefully idk"); */}
				catch (IOException ex) {
					Console.WriteLine("Pipe error: " + ex.Message);
					_recording = false; }
				catch (Exception ex) {
					Console.WriteLine("Capture error: " + ex.Message);
					_recording = false; } } }
		private static void FlipInPlaceBgra(byte[] data, int w, int h) {
			int stride = w * 4;
			int half = h / 2;
			for (int y = 0; y < half; y++) {
				int top = y * stride;
				int bot = (h - 1 - y) * stride;
				for (int x = 0; x < stride; x++) {
                    (data[bot + x], data[top + x]) = (data[top + x], data[bot + x]);
                    (data[bot + ++x], data[top + x]) = (data[top + x], data[bot + x]);
                    (data[bot + ++x], data[top + x]) = (data[top + x], data[bot + x]);
                    (data[bot + ++x], data[top + x]) = (data[top + x], data[bot + x]); } /*4-unroll. idk if it makes any difference.*/} }
		public void Stop() {
			if (!_recording) return;
			_recording = false;
			try { _stdin.Flush(); } catch {}
			try { _stdin.Close(); } catch {}
			try { if (!_ffmpeg.WaitForExit(5000)) _ffmpeg.Kill(true); } catch {} }
		public void Dispose() {
			Stop();
			_ffmpeg?.Dispose(); } } }
