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
using System.Text;

// uses StbImageSharp

namespace GameEngineThing {
	public class Game : GameWindow {
		public Vector2i _clientSize;
		public static int _gameCount = 0;
		public int _gameID;
		public double _gameTime = .0;
		private double _semiRealTime = .0;
		public double _dT = .0;
		public Shader _shader;
		public Shader _textShader { get; private set; }
		private Texture _textureSheet;
		public Text _textRenderer;
		public Camera _camera = new(new Vector3(0f, 0f, 3f), Vector3.Zero, Vector3.UnitY);
		private double _DTOverTime = 0;
		public long _frameCount = 0;
		// private Random _random = new();
		private Stopwatch _stopwatch = new();
		// private bool _gameUpdating = true;
		public long _gameTick { get; private set; } = 0;
		public float _gameTickSpeed { get; set; } = 60f;
		/// <summary>
		/// there's nothing stopping you from making this not the inverse of the tick speed but like maybe don't i guess idk lol
		/// </summary>
		public float _tickSpeedInv {get; set;} = 1f/60f;
		private float _gameTickLagCompensationAmount = 2f;
		// private int _seconds = 0;
		public static float _groundHeight = 0f;

		public Player _player;
		private ObjectMesh _playerTorsoMesh;
		private ObjectMesh _playerHeadMesh;
		private ObjectMesh _playerArmMesh;
		private ObjectMesh _playerLegMesh;
		public bool _isChatting {get; private set;} = false;
		private int _chattingBlinker = 0;
		public string _chattingText { get; private set; } = "";
		private int _chattingTextLines = 1;
		private Vector2 _chattingTextSize = new(2);
		private float _chattingTextLineHeight = 10f;
		public bool WillReopen = false;
		public string ReopenData = "";
		public string OpenData;
		public List<string> _gameModes = [];
		public DebugFlags _debugFlags = DebugFlags.none;

		private WindowState previousState;
		// private Pong _pongGame;
		// private VerticalOneKey _1kManiaPrototype;
		// private ManiaRG _maniaRGPrototype;
		public List<IMinigame> _currentMinigames { get; private set; } = [];
		public int _minigameCount { get; private set; }
		private VideoRecorder _videoRecorder;
		private long previousFrameTimestamp = 0;
		public double[] profilerFrameTimes = new double[2048];
		public float[] profilerVD; // profiler vertex data
		public int profilerIndex = 0;
		private bool profilerOn = false;
		public readonly long gameStartTimestamp = Stopwatch.GetTimestamp();
		public bool renderPlayer = true;
		public static readonly Action<Game> DefaultFlyBehavior = delegate (Game game){
			Camera cam = game._camera;
			var KeyboardState = game.KeyboardState;
			float moveAmount = cam.CamSpeed / game._gameTickSpeed;
			Player player = game._player;
			long frameCount = game._frameCount;
			float movement = (KeyboardState.IsKeyDown(Keys.W) ? -moveAmount : 0) + (KeyboardState.IsKeyDown(Keys.S) ? moveAmount : 0);
			Vector3 plrMovement = Vector3.Zero;
			plrMovement += cam.Direction * movement;
			movement = (KeyboardState.IsKeyDown(Keys.A) ? -moveAmount : 0) + (KeyboardState.IsKeyDown(Keys.D) ? moveAmount : 0);
			plrMovement += cam.Right * movement;
			movement = ((KeyboardState.IsKeyDown(Keys.Space) || KeyboardState.IsKeyDown(Keys.E)) ? moveAmount : 0) + ((KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.Q)) ? -moveAmount : 0);
			plrMovement += cam.Up * movement;
			player.RootPosition += plrMovement;
			(float ax, float ay, float az) = player.RootRotation;
			float num = MathF.Cos(ax),
			num2 = MathF.Sin(ax),
			num3 = MathF.Cos(ay),
			num4 = MathF.Sin(ay),
			num5 = MathF.Cos(az),
			num6 = MathF.Sin(az),
			x2 = num2 * num4,x3 = num * num4;
			(float sX, float sY, float sZ) = player.RootScale;
			(float tX, float tY, float tZ) = player.RootPosition;
			player.RootModel = new(sX * num3 * num5,sX * num3 * num6,-sX * num4,0,
			sY * (x2 * num5 - num * num6),sY * (x2 * num6 + num * num5),sY * num2 * num3,0,
			sZ * (x3 * num5 + num2 * num6),sZ * (x3 * num6 - num2 * num5),sZ * num * num3,0,
			tX,tY,tZ,1);
			player.UpdateLimbs();
		};
		public Action<Game> FlyBehavior;
		public List<Announcement> _announcementsThing;


		public Game(int width, int height, string title) :
		base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title }) { _gameID = _gameCount++; }

		static void Main() {
			Console.WriteLine("Starting OpenGL application...");
			bool Opening = true;
			string OpenData = "";
			while (Opening) {
				Opening = false;
				using Game game = new(800, 600, "GameEngineThingy :3");
				game.VSync = VSyncMode.On;
				game.OpenData = OpenData;
				game.Run();
				Opening = game.WillReopen;
				OpenData = game.ReopenData; }
			Console.WriteLine("game has closed.");
			debuggingThingClass.someDebugThing(debuggingThingClass.IsDebugging); }
		protected override void OnLoad() {
			base.OnLoad();
			GL.ClearColor(.3f, .6f, .5f, 1f);

			// Ensure text EBO is created with no mesh VAO bound
			Text.OnLoad();

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
			], [Vector3.Zero, new(0f, 1.2f, 0f), new(-1f, 0f, 0f), new(1f, 0f, 0f), new(-.3f, -1.2f, 0f), new(.3f, -1.2f, 0f),
			], [Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero], [
				Vector3.One, (0.9f,0.9f,0.9f),
				Vector3.One, new(-1f, 1f, 1f),
				Vector3.One, new(-1f, 1f, 1f),
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
			// _camera.UpdateVectors();
			// _camera.Direction = Vector3.Normalize(_camera.Position - _camera.Target);
			// _camera.Right = Vector3.Normalize(Vector3.Cross(_camera.Up, _camera.Direction));
			// _camera.View = Matrix4.LookAt(_camera.Position, _camera.Target, _camera.Up);
			_clientSize = ClientSize;
			_camera.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), _clientSize.X / (float)_clientSize.Y, .1f, 10000f);

			Console.WriteLine("Max vertices: " + GL.GetInteger(GetPName.MaxElementsVertices));
			Console.WriteLine("Max indices: " + GL.GetInteger(GetPName.MaxElementsIndices));

			// Console.WriteLine("FontCharacterData info: FontCharDeeta has " + FontCharFillerThing.FontCharDeeta.Chars.Count + " normal characters, and " + FontCharFillerThing.FontCharDeeta.SChars.Count + " special characters.");
			// foreach (var a in FontCharFillerThing.FontCharDeeta.Chars) {
			// 	Console.Write("key:"+a.Key+";value:sizeX:"+a.Value.sizeX+",Y:"+a.Value.sizeY+",advX:"+a.Value.advanceX+",Y:"+a.Value.advanceY+",bX:"+a.Value.bearingX+",Y:"+a.Value.bearingY+",tsX:"+a.Value.tStartX+",Y:"+a.Value.tStartY+",teX:"+a.Value.tEndX+",Y:"+a.Value.tEndY+'\n');
			// }
			// foreach (var a in FontCharFillerThing.FontCharDeeta.SChars) {
			// 	Console.Write("key:"+a.Key+";value:sizeX:"+a.Value.sizeX+",Y:"+a.Value.sizeY+",advX:"+a.Value.advanceX+",Y:"+a.Value.advanceY+",bX:"+a.Value.bearingX+",Y:"+a.Value.bearingY+",tsX:"+a.Value.tStartX+",Y:"+a.Value.tStartY+",teX:"+a.Value.tEndX+",Y:"+a.Value.tEndY+'\n');
			// }
			// GameEngineFlyBehavior = 0;

			_announcementsThing = [new("testing testing", Stopwatch.GetTimestamp() + Stopwatch.Frequency*12, (.8f, .2f, .8f), (.1f, .1f, .1f), .8f, fot:4),new("tst test2 dsfsdfsdds", Stopwatch.GetTimestamp() + Stopwatch.Frequency*30,(.3f,.5f,.3f),(.5f,.5f,.5f),.5f, fot:0)];

			// switch (OpenData.ToLower()) {
			// 	case "pong":
			// 		_currentMinigames = [new Pong(new Vector3(10f, 0f, 10f), new Vector3(1f), new Vector3(270f, 0f, 0f))];
			// 		_gameModes = ["pong"];
			// 		break;
			// 	case "fnf": // do something else but rn the something else doesn't exist
			// 	case "mania": // also do something else but the something else also doesn't exist
			// 		_currentMinigames = [new ManiaRG(_textRenderer)];
			// 		_gameModes = ["mania"];
			// 		break;
			// 	case "1k fnf" or "1kfnf" or "fnf 1k" or "fnf1k" or
			// 		"v1k" or "verticalonekey" or "vertical one key":
			// 		_currentMinigames = [new VerticalOneKey(_textRenderer, DataStuff.BuiltInV1KCharts[0])];
			// 		_gameModes = ["v1k"];
			// 		break;
			// 	case "v1k2":
			// 		_currentMinigames = [new VerticalOneKey(_textRenderer, DataStuff.BuiltInV1KCharts[1])];
			// 		_gameModes = ["v1k"];
			// 		break;
			// 	case "miner": // also do something else but the something else also doesn't exist
			// 		_currentMinigames = [new MiningGame()];
			// 		_gameModes = ["miner"];
			// 		break;
			// 	case "animate":
			// 		_currentMinigames = [new Animate()];
			// 		_gameModes = ["animate"];
			// 		break;
			// 	default:
			// 		_currentMinigames = [new DefaultGameBehavior()];
			// 		_gameModes = ["DEFAULT_BEHAVIOR"];
			// 		break;
			// }
			FlyBehavior = DefaultFlyBehavior;
			if (DataStuff.MinigameInitializers.TryGetValue(OpenData, out Action<Game> v)) v(this); else DataStuff.MinigameInitializers["DEFAULT_BEHAVIOR"](this);
			foreach (IMinigame minigame in _currentMinigames) minigame.OnLoad(this);

			_stopwatch.Start();}
		protected override void OnRenderFrame(FrameEventArgs e) {
			base.OnRenderFrame(e);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			_textureSheet.Use(TextureUnit.Texture0);
			// _textRenderer.TextTexture.Use(TextureUnit.Texture1); // should already be used though idk

			_shader.Use();
			_shader.SetMatrix4("view", _camera.View);
			// _shader.SetMatrix4("projection", _camera.Projection);


			if (renderPlayer){_shader.SetTextureLocation("tx0", new Vector4(0f, 0f, 1f, 1f)); _player.Render(_shader);}

			foreach(IMinigame minigame in _currentMinigames) minigame.OnRenderFrame(this, e.Time);

			// UI time!! :3
			// remember that this renderer is pretty weird or smth and if you render in the wrong order the game may not render properly.
			bool debugText = _debugFlags.HasFlag(DebugFlags.debugText);
			if (_isChatting) {
				string chatTxt = "> " + _chattingText;
				if (_chattingBlinker < 128) chatTxt += "\\blinker|";
				Vector2i posOffset = new(0, (int)(_chattingTextLines * _chattingTextLineHeight * _chattingTextSize.Y));
				_textRenderer.RenderText(this, _textShader, chatTxt, posOffset, new(-.5f), _chattingTextSize, new(1), _chattingTextLineHeight, _clientSize, FontCharFillerThing.FontCharDeeta, true);
				if (debugText) _textRenderer.RenderText(this, _textShader, chatTxt, posOffset, new(-1, -.8f), _chattingTextSize, new(.7f), _chattingTextLineHeight, _clientSize, FontCharFillerThing.FontCharDeeta, false);}
			// _videoRecorder?.CaptureFrame(this.ClientSize, VSync == VSyncMode.On);
			long timestampNow = Stopwatch.GetTimestamp();
			profilerFrameTimes[profilerIndex] = (timestampNow - previousFrameTimestamp) / (double)Stopwatch.Frequency * 1000;
			previousFrameTimestamp = timestampNow;
			if (profilerOn) { _textRenderer.ProfilerRender(this);}
			profilerIndex = (profilerIndex + 1) % profilerFrameTimes.Length;
			_videoRecorder?.CaptureFrame(_clientSize);
			SwapBuffers();}
		protected override void OnUpdateFrame(FrameEventArgs e) {
			base.OnUpdateFrame(e);
			_frameCount++;
			_dT = e.Time;
			_semiRealTime += _dT;
			if ((_frameCount & 127) == 0) {
				Title = "GameEngineThingy :3 FPS: " + 128d / _DTOverTime;
				_DTOverTime = _dT;}
			else _DTOverTime += _dT;

			foreach (IMinigame minigame in _currentMinigames)
			minigame.OnUpdateFrame(this, _dT);

			if (IsFocused) {
				if (_isChatting) {_chattingBlinker=(_chattingBlinker+3)&255;}
				else {
					if (MouseState.ScrollDelta.Y != 0) { // (3f * _tickSpeedInv + 1)
						_camera.CamSpeed = Math.Max(.1f, Math.Min(1024f, _camera.CamSpeed * (MouseState.ScrollDelta.Y * 0.1f + 1)));
						if (_debugFlags.HasFlag(DebugFlags.debugLogging)) Console.WriteLine("scroll speed changed; new speed: " + _camera.CamSpeed);}
					bool IsNotEngineTick = _semiRealTime < _tickSpeedInv; if (MouseState[MouseButton.Right] || MouseState[MouseButton.Middle]){
						// float deltaX = MouseState.X - MouseState.PreviousX;
						// float deltaY = MouseState.Y - MouseState.PreviousY;
						(float deltaX, float deltaY) = MouseState.Position - MouseState.PreviousPosition;
						if (deltaX != 0 || deltaY != 0) {
							float Yaw = (_camera.Yaw + deltaX * _camera.MouseSensitivity) % (float)(2.0 * Math.PI);
							float Pitch = Math.Max(MathHelper.DegreesToRadians(-89f), Math.Min(MathHelper.DegreesToRadians(89f), _camera.Pitch - deltaY * _camera.MouseSensitivity));
							_camera.Yaw = Yaw;
							_camera.Pitch = Pitch;
							float NCosPitch = -(float)Math.Cos(Pitch);

							_camera.CameraToTargetOffset = Vector3.Normalize(new Vector3(
								NCosPitch * (float)Math.Cos(Yaw),
								-(float)Math.Sin(Pitch),
								NCosPitch * (float)Math.Sin(Yaw)));if (IsNotEngineTick) {_camera.UpdateVectors(); return;}}}
					// if (IsNotEngineTick) { // if this frame is too early to go to the next game tick
					//   // update camera vectors so the camera movement is smooth
					// 	_camera.UpdateVectors();
					// 	return;}
					if (IsNotEngineTick) return; // if this frame is too early to go to the next game tick
					// increment game tick and update game time
					_gameTick++;
					_gameTime = _gameTick * _tickSpeedInv;
					// update semi real time; this is a fake time that is used to make the game run at a constant speed
					_semiRealTime -= _tickSpeedInv;
					if (_semiRealTime > _gameTickLagCompensationAmount * _tickSpeedInv) _semiRealTime = _gameTickLagCompensationAmount * _tickSpeedInv;
					// ^ prevents the semi real time from getting too big; If this wasn't here, then for example, if a big lag spike happens, the semi real time will get really big and the game will run as fast as possible for a while, and that would feel really weird, and people might rage or something idk :p
					foreach (IMinigame minigame in _currentMinigames)
					minigame.OnEngineTick(this, _tickSpeedInv);
					// camera zoom n stuff
					if (KeyboardState.IsKeyDown(Keys.I)) _camera.CameraDistFromTarget = Math.Max(_camera.MinDist, _camera.CameraDistFromTarget * (_gameTickSpeed / (_gameTickSpeed + 3f)));
					if (KeyboardState.IsKeyDown(Keys.O)) _camera.CameraDistFromTarget = Math.Min(_camera.MaxDist, _camera.CameraDistFromTarget * (3f * _tickSpeedInv + 1));

					// movement
					if (_camera.IsFlying) {
						// GameEngineFlyBehaviorsThing[GameEngineFlyBehavior](this);
						FlyBehavior(this);
					} else {
						float moveAmount = _camera.CamSpeed * _tickSpeedInv;
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
						_player.StepPhysics(_tickSpeedInv);}
					_camera.Target = _player.RootPosition;
					_camera.UpdateVectors(); /* update cam */ }} else {/* window is not focused */}}
		protected override void OnResize(ResizeEventArgs e) {
			base.OnResize(e);
			_clientSize = ClientSize;
			GL.Viewport(0, 0, _clientSize.X, _clientSize.Y);
			_camera.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), _clientSize.X / (float)_clientSize.Y, .1f, 10000f);
			foreach (IMinigame minigame in _currentMinigames) minigame.OnResize(e);}
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
					case Keys.F11:
						if (WindowState == WindowState.Fullscreen) WindowState = previousState;
						else { previousState = WindowState; WindowState = WindowState.Fullscreen; VSync = VSyncMode.On; } break;
					case Keys.Escape:
						_isChatting = false; break;
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
							if (DataStuff.noInputChatCommands.TryGetValue(lowercaseChatTxt, out Action<Game> noInputCmd)) {
								if (debug) Console.WriteLine("Found a no-input command for " + lowercaseChatTxt);
								noInputCmd(this); }
							else {
								if (debug) Console.WriteLine("Did not find a no-input command for " + lowercaseChatTxt + ".\nSearching for input commands...");
								if (DataStuff.chatCommands.TryGetValue(lowercaseChatTxt, out Action<Game, string> inputCmd)) {
									if (debug) Console.WriteLine("Found command " + lowercaseChatTxt + ".");
									inputCmd(this, "");
									break;
								} else if (debug) Console.WriteLine(lowercaseChatTxt + " is not a valid input command...");
								if (debug) {
									for (int i = lowercaseChatTxt.Length-1; i > 0; i--) {
										string s = lowercaseChatTxt[..i];
										if (DataStuff.chatCommands.TryGetValue(s, out inputCmd)) {
											Console.WriteLine("Found command " + s + ".");
											inputCmd(this, lowercaseChatTxt[i..]);
											break; } else Console.WriteLine(s + " is not a valid input command..."); }
								} else {
									for (int i = lowercaseChatTxt.Length-1; i > 0; i--) {
										string s = lowercaseChatTxt[..i];
										if (DataStuff.chatCommands.TryGetValue(s, out inputCmd)) {
											inputCmd(this, lowercaseChatTxt[i..]);
											break; } }
								}
							}
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
					case Keys.F6: if (e.Modifiers.HasFlag(KeyModifiers.Control)) {
							if (profilerOn) {profilerOn = false; profilerVD = [];} else {
								profilerOn = true;
								float winSizeY = _clientSize.Y;
								int amt = profilerFrameTimes.Length << 3;
								profilerVD = new float[amt];
								// int tRSX = 0;
								// float tX = tRSX / (float)TextTexture.Width;

								profilerVD[1]=profilerVD[9]=(winSizeY + 0.5f) / winSizeY;
								// profilerVD[2]=profilerVD[10]=profilerVD[6]=profilerVD[14]=0;
								profilerVD[3]=profilerVD[11]=profilerVD[7]=profilerVD[15]=32f / _textRenderer.TextTexture.Height;
								// if (length > BulkDrawFloats / 8) { // if it takes a full array or more to store all of the data
								int i = 16;
								for (; i < (amt>>1)+1; i <<= 1) Array.Copy(profilerVD, 1, profilerVD, i + 1, i - 1);
								if (i < amt) Array.Copy(profilerVD, 1, profilerVD, i + 1, amt - 1 - i);
							}
					} break;
					default: break; } }}
		protected override void OnKeyUp(KeyboardKeyEventArgs e) {
			base.OnKeyUp(e);
			foreach (IMinigame minigame in _currentMinigames) minigame.OnKeyUp(e);
		}
		protected override void OnMouseDown(MouseButtonEventArgs e) {
			base.OnMouseDown(e);
			foreach (IMinigame minigame in _currentMinigames) minigame.OnMouseDown(e);
			// if (e.Action.HasFlag(InputAction.Repeat))
			// 	Console.WriteLine("repmd(" + e.Button + "," + e.Modifiers + ")");
			// else Console.WriteLine("Mouse down:" + e.Button + "," + e.Modifiers);
		}
		protected override void OnMouseUp(MouseButtonEventArgs e) {
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
				_playerTorsoMesh?.Dispose();
				_playerHeadMesh?.Dispose();
				_playerArmMesh?.Dispose();
				_playerLegMesh?.Dispose();
				_textureSheet?.Dispose();
				_shader?.Dispose();
				_textShader?.Dispose();}}

		public void StartRecording(string output, int fps = 60, float speed = 1) {
			_videoRecorder = new VideoRecorder(_clientSize.X, _clientSize.Y, fps, output, speed); }
		public void StartRecording(string output, float resfps, float inpfps) {
			_videoRecorder = new VideoRecorder(_clientSize.X, _clientSize.Y, resfps, output, inpfps); }
		public void StartRecording(string output, float resfps, float inpfps, string parameters) {
			_videoRecorder = new VideoRecorder(_clientSize.X, _clientSize.Y, output, resfps, inpfps, parameters); }
		public void StopRecording() {
			// _videoRecorder?.Stop();
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
		public void Use() {GL.UseProgram(Handle);}
		public int GetAttribLocation(string attribName) {
			return GL.GetAttribLocation(Handle, attribName);}
		public void SetInt(string name, int value) {
			int location = GL.GetUniformLocation(Handle, name);
			GL.Uniform1(location, value);}
		public void SetMatrix4(string name, Matrix4 value) {
			int location = GL.GetUniformLocation(Handle, name);
			GL.UniformMatrix4(location, true, ref value);}

		public void SetTextureLayer(int layer) {SetInt("textureLayer", layer);}
		public void SetTextureLocation(string name, Vector4 LocationAndSize) {
			int location = GL.GetUniformLocation(Handle, name);
			GL.Uniform4(location, LocationAndSize);}
		public void SetVector3(string name, Vector3 value) {
			int location = GL.GetUniformLocation(Handle, name);
			GL.Uniform3(location, value);}

		public void Dispose() {GL.DeleteProgram(Handle);}}
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
		public float CamSpeed = 3f;
		public Vector3 CameraToTargetOffset = new Vector3(1f/MathF.Sqrt(3f), 1f/MathF.Sqrt(3f), -1f/MathF.Sqrt(3f));
		public float CameraDistFromTarget = 8f;
		// public Vector3 CameraFront { get; set; } = new Vector3(0f, 0f, -1f);
		public float MinDist = .05f;
		public float MaxDist = 128f;
		public Vector3 Target = Vector3.Zero;
		public Vector3 Position = new Vector3(0f, 0f, 3f);
		public readonly Vector3 Up = Vector3.UnitY;
		public Vector3 Direction;
		public Vector3 Right;
		/// <summary>
		/// The matrix to project objects to the screen. It is the view but also multiplied by the projection matrix. idk if it helps but yeah.
		/// </summary>
		public Matrix4 View;

		public float Pitch;
		public float Yaw;
		public float MouseSensitivity = .005f;

		// public float PlayerSpeed { get; set; } = 5f;
		public bool IsFlying = false;
		// public float JumpPower { get; set; } = 5f;
		// public Vector3 Gravity { get; set; } = new Vector3(0f, -9.81f, 0f);
		// public Vector3 PlayerVelocity { get; set; } = Vector3.Zero;
		// public bool IsFalling { get; set; } = false;
		// public bool IsGrounded { get; set; } = true;
		public Matrix4 Projection;

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
			// Direction = Vector3.Normalize(Position - Target);
			// Right = Vector3.Normalize(Vector3.Cross(Up, Direction));
			// View = Matrix4.LookAt(Position, Target, Up) * Projection;
			Direction = Vector3.Normalize(CameraToTargetOffset);
			Right = Vector3.Normalize(Vector3.Cross(Up, Direction));
			View = Matrix4.LookAt(Position, Target, Up) * Projection;
		}}
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
		// public void UpdateLimb(int index) {
		// 	Limbs[index].UpdateModel(
		// 		Matrix4.CreateRotationX(LimbRotations[index].X) *
		// 		Matrix4.CreateRotationY(LimbRotations[index].Y) *
		// 		Matrix4.CreateRotationZ(LimbRotations[index].Z) *
		// 		Matrix4.CreateScale(LimbScales[index] * RootScale) *
		// 		Matrix4.CreateTranslation(LimbPositions[index]) *
		// 		RootModel
		// 	);}
		// public void UpdateLimbs() { for (int i = 0; i < Limbs.Length; i++) { UpdateLimb(i); } }
		public void UpdateLimbs() { for (int i = 0; i < Limbs.Length; i++) {
				Limbs[i].UpdateModel(/*Matrix4.CreateRotationX(LimbRotations[i].X) *
				Matrix4.CreateRotationY(LimbRotations[i].Y) *
				Matrix4.CreateRotationZ(LimbRotations[i].Z) **/
				DataStuff.CreateRotationXYZ(LimbRotations[i]) *
				Matrix4.CreateScale(LimbScales[i] * RootScale) *
				Matrix4.CreateTranslation(LimbPositions[i]) *
				RootModel);} }
		public void StepPhysics(float deltaTime) {
			if (!IsGrounded) {
				Velocity += Gravity * deltaTime;}
			RootPosition += Velocity * deltaTime;
			if (RootPosition.Y < Game._groundHeight) {
				RootPosition = new Vector3(RootPosition.X, Game._groundHeight, RootPosition.Z);
				Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);
				IsGrounded = true;}
			// RootModel = Matrix4.CreateRotationX(RootRotation.X) * Matrix4.CreateRotationY(RootRotation.Y) * Matrix4.CreateRotationZ(RootRotation.Z) * Matrix4.CreateScale(RootScale) * Matrix4.CreateTranslation(RootPosition);
			RootModel = DataStuff.CreateRotationXYZ(RootRotation) * Matrix4.CreateScale(RootScale) * Matrix4.CreateTranslation(RootPosition);
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
	public sealed class VideoRecorder : IDisposable {
		private readonly int _w, _h;
		private bool _recAllFrames;
		private readonly Process _ffmpeg;
		private readonly Stream _stdin;
		private readonly byte[] _fBuffer;
		public bool _recording {get; private set;}
		private long _nextTickNs, _tickStepNs;
		public VideoRecorder(int w, int h, int fps, string p, float speed = 1) { // no nvenc yet also no audio
			// (_w, _h, _recAllFrames, _path) = (w,h,fps<0,p);
			_w = w; _h = h; _recAllFrames = fps < 0;
			_fBuffer = new byte[w*h<<2];
			_tickStepNs = (long)(1000000000d/(fps*speed));
			StringBuilder args = new("-n -f rawvideo -pix_fmt bgra -s ", 256);args.Append(w);args.Append('x');args.Append(h);args.Append(" -r ");args.Append((fps*speed).ToString("N4"));args.Append(" -i - -vf \"vflip\" -an -c:v libx265 -preset slow -crf 25 -pix_fmt yuv420p \"");args.Append(p);args.Append('\"');
			_ffmpeg = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "ffmpeg",
					Arguments = args.ToString(),
					RedirectStandardInput=true,
					RedirectStandardOutput=true,
					RedirectStandardError=true,
					UseShellExecute=false,
					CreateNoWindow=true}};
			_ffmpeg.Start();
			Console.WriteLine("argument thingy for ffmpeg is "+_ffmpeg.StartInfo.Arguments);
			if (_ffmpeg.HasExited) throw new InvalidOperationException("oops my ffmpeg crashed. I lost my data, but I had an antivirus. code: "+_ffmpeg.ExitCode);
			_stdin = _ffmpeg.StandardInput.BaseStream;
			_recording = true;
			_ffmpeg.BeginErrorReadLine();
			_ffmpeg.ErrorDataReceived += (sender, e) => Console.WriteLine("FFmpeg: "+e.Data);
			_nextTickNs = Stopwatch.GetTimestamp()*1000000000L/Stopwatch.Frequency; }
		public VideoRecorder(int w, int h, float resfps, string p, float inpfps) {
			_w = w; _h = h; _recAllFrames = resfps < 0;
			_fBuffer = new byte[w*h<<2];
			_tickStepNs = (long)(1000000000d/inpfps);
			StringBuilder args = new("-n -f rawvideo -pix_fmt bgra -s ", 256);args.Append(w);args.Append('x');args.Append(h);args.Append(" -r ");args.Append(resfps.ToString("N4"));args.Append(" -i - -vf \"vflip\" -an -c:v libx265 -preset slow -crf 25 -pix_fmt yuv420p \"");args.Append(p);args.Append('\"');
			_ffmpeg = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "ffmpeg",
					Arguments = args.ToString(),
					RedirectStandardInput=true,
					RedirectStandardOutput=true,
					RedirectStandardError=true,
					UseShellExecute=false,
					CreateNoWindow=true}};
			_ffmpeg.Start();
			Console.WriteLine("argument thingy for ffmpeg is "+_ffmpeg.StartInfo.Arguments);
			if (_ffmpeg.HasExited) throw new InvalidOperationException("oops my ffmpeg crashed. I lost my data, but I had an antivirus. code: "+_ffmpeg.ExitCode);
			_stdin = _ffmpeg.StandardInput.BaseStream;
			_recording = true;
			_ffmpeg.BeginErrorReadLine();
			_ffmpeg.ErrorDataReceived += (sender, e) => Console.WriteLine("FFmpeg: "+e.Data);
			_nextTickNs = Stopwatch.GetTimestamp()*1000000000L/Stopwatch.Frequency; }
		public VideoRecorder(int w, int h, string p, float resfps, float inpfps, string parameters) {
			_w = w; _h = h; _recAllFrames = resfps < 0;
			_fBuffer = new byte[w*h<<2];
			_tickStepNs = (long)(1000000000d/inpfps);
			_ffmpeg = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "ffmpeg",
					Arguments = parameters,
					RedirectStandardInput=true,
					RedirectStandardOutput=true,
					RedirectStandardError=true,
					UseShellExecute=false,
					CreateNoWindow=true}};
			_ffmpeg.Start();
			Console.WriteLine("argument thingy for ffmpeg is "+_ffmpeg.StartInfo.Arguments);
			if (_ffmpeg.HasExited) throw new InvalidOperationException("oops my ffmpeg crashed. I lost my data, but I had an antivirus. code: "+_ffmpeg.ExitCode);
			_stdin = _ffmpeg.StandardInput.BaseStream;
			_recording = true;
			_ffmpeg.BeginErrorReadLine();
			_ffmpeg.ErrorDataReceived += (sender, e) => Console.WriteLine("FFmpeg: "+e.Data);
			_nextTickNs = Stopwatch.GetTimestamp()*1000000000L/Stopwatch.Frequency; }
		public void CaptureFrame(Vector2i ClientSize) {
			if (!_recording) return;
			if (_ffmpeg.HasExited) { _recording = false; Console.WriteLine("ffmpeg exited: "+_ffmpeg.ExitCode); return; }
			if (!_recAllFrames) {
				if ((Stopwatch.GetTimestamp() * 1000000000L / Stopwatch.Frequency) < _nextTickNs) return;
				_nextTickNs += _tickStepNs; }
			if (ClientSize.X != _w || ClientSize.Y != _h) Dispose();
			try {
				GL.ReadBuffer(ReadBufferMode.Back);
				GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
				GL.ReadPixels(0, 0, _w, _h, PixelFormat.Bgra, PixelType.UnsignedByte, _fBuffer);
				_stdin.Write(_fBuffer, 0, _fBuffer.Length);}
			catch (IOException ex) { Console.WriteLine("Pipe err: " + ex.Message); Dispose(); }
			catch (Exception ex) { Console.WriteLine("Capture err: " + ex.Message); Dispose(); }
		}
		public void Dispose() {
			_recording = false;
			try { _stdin.Flush(); } catch {}
			try { _stdin.Close(); } catch {}
			try { if (!_ffmpeg.WaitForExit(5000)) _ffmpeg.Kill(true); } catch {} _ffmpeg?.Dispose(); }
	}
}
