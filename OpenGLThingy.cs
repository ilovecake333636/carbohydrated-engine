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
		public static Game currentGame = null;
		public Vector2i _clientSize;
		public double _gameTime = .0;
		private double _semiRealTime = .0;
		public double _dT = .0;
		public Shader _shader;
		public Shader _textShader;
		public Texture _textureSheet;
		public Text _textRenderer;
		public Camera _camera = new();
		private double _DTOverTime = 0;
		public long _frameCount = 0;
		// private bool _gameUpdating = true;
		public long _gameTick = 0;
		public float _gameTickSpeed = 60f;
		/// <summary>
		/// there's nothing stopping you from making this not the inverse of the tick speed but like maybe don't i guess idk lol
		/// </summary>
		public float _tickSpeedInv = 1f/60f;
		private float _maxLagCompensationTime = 0.1f;
		// private int _seconds = 0;
		public static float _groundHeight = 0f;

		public Player _player;
		private ObjectMesh _playerTorsoMesh, _playerHeadMesh, _playerArmMesh, _playerLegMesh;
		public bool _isChatting = false;
		public double _consoleScroll = 0;
		private int _chattingBlinker = 0;
		public string _chattingText = "";
		private int _chattingTextLines = 1;
		private Vector2 _chattingTextSize = new(2);
		private float _chattingTextLineHeight = 10f;
		public bool WillReopen = false;
		public string ReopenData = "";
		public string OpenData;
		public List<string> _gameModes = [];

		private WindowState previousState;
		// private Pong _pongGame;
		// private VerticalOneKey _1kManiaPrototype;
		// private ManiaRG _maniaRGPrototype;
		public List<IMinigame> _currentMinigames = [];
		public int _minigameCount;
		private VideoRecorder _videoRecorder;
		private long previousFrameTimestamp = 0;
		public double[] profilerFrameTimes = new double[2048];
		public float[] profilerVD; // profiler vertex data
		public int profilerIndex = 0;
		private bool profilerOn = false;
		public readonly long gameStartTimestamp = Stopwatch.GetTimestamp();
		public static readonly long programStartTimestamp = Stopwatch.GetTimestamp();
		public bool renderPlayer = true;


		public Game(int width, int height, string title) :
		base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title }) { }
		private static readonly Action<Game> SecretMessage = delegate(Game g) {
			long ts = Stopwatch.GetTimestamp();
			long ticksSinceGameStart = ts - g.gameStartTimestamp;
			if (ticksSinceGameStart < Stopwatch.Frequency*3) return;
			StringBuilder s = new("SECRET MESSAGE! :3\nhi :3\n:3");
			while (Random.Shared.Next(20) > 0) s.Append(" :3");
			AnnouncementsManager.Announcements.Add(new(s.ToString(), Stopwatch.GetTimestamp() + Stopwatch.Frequency*10+Random.Shared.Next((int)Stopwatch.Frequency*4),(.3f,.5f,Random.Shared.NextSingle(),.95f),(.5f,Random.Shared.NextSingle(),.5f,.5f), fot:3f));
			if (ticksSinceGameStart > Stopwatch.Frequency*5)g.DeferredTasks -= SecretMessage;
		};
		public Action<Game> DeferredTasks = delegate(Game game){};
		static void Main() {
			long startTS = programStartTimestamp;
			Console.Write(startTS+" is the program start tick.\n");
			AnnouncementsManager.Announcements = [
				new("Welcome to carbohydrated-engine, have fun!", Stopwatch.GetTimestamp() + Stopwatch.Frequency*12, (.8f, .2f, .8f, .95f), (.1f, .1f, .1f, .8f), fot:4)];
			bool Opening = true;
			string OpenData = "";
			while (Opening) {
				Opening = false;
				using Game game = new(800, 600, "GameEngineThingy :3");
				if (Random.Shared.Next(1000) == 0) game.DeferredTasks += SecretMessage;
				game.VSync = VSyncMode.On;
				game.OpenData = OpenData;
				game.Run();
				Opening = game.WillReopen;
				OpenData = game.ReopenData; }
			Console.Write("game has closed.\n"); }
		protected override void OnLoad() {
			base.OnLoad();
			if (currentGame == null) currentGame = this; else throw new Exception("uhh screw you ig i don't want to deal with multiple windows yet lol");
			GL.ClearColor(.3f, .5f, .7f, 1f);

			// Ensure text EBO is created with no mesh VAO bound
			Text.OnLoad();

			_playerTorsoMesh = new ObjectMesh((0f,0f,0f), (0f,0f,0f), (1f,1f,1f), DataStuff.PlrTorsoV, DataStuff.PlrTorsoI);
			_playerArmMesh = new ObjectMesh((0f,0f,0f), (0f,0f,0f), (1f,1f,1f), DataStuff.PlrArmV, DataStuff.PlrArmI);
			_playerLegMesh = new ObjectMesh((0f,0f,0f), (0f,0f,0f), (1f,1f,1f), DataStuff.PlrLegV, DataStuff.PlrLegI);
			_playerHeadMesh = new ObjectMesh((0f,0f,0f), (0f,0f,0f), (1f,1f,1f), DataStuff.PlrHeadV, DataStuff.PlrHeadI);
			_player = new Player((0f,0f,0f), (0f,0f,0f), (1f,1f,1f), [
				new(_playerTorsoMesh, 0),
				new(_playerHeadMesh, 0),
				new(_playerArmMesh, 0),
				new((0f,0f,0f), (0f,0f,0f), (-1f, 1f, 1f), _playerArmMesh),
				new(_playerLegMesh, 0),
				new((0f,0f,0f), (0f,0f,0f), (-1f, 1f, 1f), _playerLegMesh),
			], [(0f,0f,0f), (0f, 1.2f, 0f), (-1.1f, 0.3f, 0f), (1.1f, 0.3f, 0f), (-.3f, -1.15f, 0f), (.3f, -1.15f, 0f),
			], [(0f,0f,0f), (0f,0f,0f), (0f,0f,0f), (0f,0f,0f), (0f,0f,0f), (0f,0f,0f)], [
				(1.1f,1.1f,1.1f), (0.875f,0.875f,0.875f),
				(1f,1f,1f), (-1f, 1f, 1f),
				(1.05f,.9f,1.05f), (-1.05f, .9f, 1.05f),
			]);
			_shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
			_shader.Use();

			GL.Enable(EnableCap.DepthTest);
			// _textureSheet = Texture.LoadFromFile("Textures/texturesheet.png", false, true);
			_textureSheet = Texture.LoadFromFile("Textures/texturesheet.png", false, false);
			_textureSheet.Use(TextureUnit.Texture0);
			_shader.SetInt("texture0", 0);

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

			AnnouncementsManager.OnLoad();

			if (DataStuff.MinigameInitializers.TryGetValue(OpenData, out Action<Game> v)) v(this); else DataStuff.MinigameInitializers["DEFAULT_BEHAVIOR"](this);
			foreach (IMinigame minigame in _currentMinigames) minigame.OnLoad(this);}
		protected override void OnRenderFrame(FrameEventArgs e) {
			base.OnRenderFrame(e);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			_camera.UpdateVectors();

			_textureSheet.Use(TextureUnit.Texture0);
			// _textRenderer.TextTexture.Use(TextureUnit.Texture1); // should already be used though idk

			_shader.Use();
			_shader.SetMatrix4("view", _camera.View);
			// _shader.SetMatrix4("projection", _camera.Projection);


			if (renderPlayer) _player.Render(_shader);

			foreach(IMinigame minigame in _currentMinigames) minigame.OnRenderFrame(this, e.Time);

			// UI time!! :3
			// remember that this renderer is pretty weird or smth and if you render in the wrong order the game may not render properly.
			if (_isChatting) {
				string chattxt = (_chattingBlinker<128)?("> "+_chattingText+"\\blinker|"):("> "+_chattingText);
				_textRenderer.RenderText(this, _textShader, chattxt, (0,(int)_consoleScroll), new(-.5f), _chattingTextSize, new(1), _chattingTextLineHeight, _clientSize, FontCharFillerThing.FontCharDeeta, true);
			}
			AnnouncementsManager.OnRender(_clientSize);
			long timestampNow = Stopwatch.GetTimestamp();
			profilerFrameTimes[profilerIndex] = (timestampNow - previousFrameTimestamp) / (double)Stopwatch.Frequency * 1000;
			previousFrameTimestamp = timestampNow;
			if (profilerOn) _textRenderer.ProfilerRender(this);
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
				MouseState ms = MouseState;
				KeyboardState ks = KeyboardState;
				System.Collections.BitArray ksAsBitArray = DataStuff.GetKSBitArray(ks);
				byte[] ksData = DataStuff.GetBitArrayByteArray(ksAsBitArray);
				if (_isChatting) {
					_chattingBlinker=(_chattingBlinker+3)&255;
				} else {
					if (ms[MouseButton.Right] || ms[MouseButton.Middle]){
						Vector2 delta = ms.Position - ms.PreviousPosition;
						if (delta.X != 0 || delta.Y != 0) {
							_camera.Yaw = (((_camera.Yaw + delta.X * _camera.MouseSensitivity) % MathF.Tau) + MathF.Tau) % MathF.Tau;
							_camera.Pitch = Math.Clamp(_camera.Pitch - delta.Y * _camera.MouseSensitivity, -89.9f*DataStuff.D2RConst, 89.9f*DataStuff.D2RConst);}}
				}
				if (_semiRealTime < _tickSpeedInv) return; // if this frame is too early to go to the next game tick
				// increment game tick and update game time
				_gameTick++;
				_gameTime = _gameTick / _gameTickSpeed;
				// update semi real time; this is a fake time that is used to make the game run at a constant speed
				_semiRealTime -= _tickSpeedInv;
				if (_semiRealTime > _maxLagCompensationTime) _semiRealTime = _maxLagCompensationTime;
				// ^ prevents the semi real time from getting too big; without this, then if lag happens, the semi real time'll get really big and if the fps increases again the game will have new engine ticks every single frame for a while and that wouldn't be very good :p
				foreach (IMinigame minigame in _currentMinigames) minigame.OnEngineTick(this, _tickSpeedInv);
				// camera zoom n stuff
				if (!_isChatting){
					if ((ksData[((int)Keys.I)>>3]&(1<<(((int)Keys.I)&7)))>0) _camera.CameraDistFromTarget = Math.Max(_camera.MinDist, _camera.CameraDistFromTarget * (_gameTickSpeed / (_gameTickSpeed + 3f)));
					if ((ksData[((int)Keys.O)>>3]&(1<<(((int)Keys.O)&7)))>0) _camera.CameraDistFromTarget = Math.Min(_camera.MaxDist, _camera.CameraDistFromTarget * (3f * _tickSpeedInv + 1));

					// movement
					_player.OnUpdateFrame(this, ksData);
				}
				_camera.Target = _player.RootPosition;} else {/* window is not focused */}}
		protected override void OnMouseWheel(MouseWheelEventArgs e) {
			base.OnMouseWheel(e);
			Vector2 scrollDelta = e.Offset;
			if (_isChatting) {
				if (scrollDelta.Y != 0) _consoleScroll -= (KeyboardState[Keys.LeftAlt]?5:1)*(scrollDelta.Y * _chattingTextLineHeight * _chattingTextSize.Y);
			} else {
				if (scrollDelta.Y != 0) {
					if (KeyboardState[Keys.LeftAlt]) _player.Walkspeed = Math.Max(.001f, Math.Min(2048f, _player.Walkspeed * float.FusedMultiplyAdd(scrollDelta.Y, 0.1f, 1)));
					else _camera.CameraDistFromTarget = Math.Clamp(_camera.CameraDistFromTarget * MathF.Pow(.8333333333333f,scrollDelta.Y), _camera.MinDist, _camera.MaxDist);}
			}
		}
		protected override void OnResize(ResizeEventArgs e) {
			base.OnResize(e);
			_clientSize = ClientSize;
			GL.Viewport(0, 0, _clientSize.X, _clientSize.Y);
			_camera.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), _clientSize.X / (float)_clientSize.Y, .1f, 10000f);
			foreach (IMinigame minigame in _currentMinigames) minigame.OnResize(e);}
		protected override void OnTextInput(TextInputEventArgs e) {
			base.OnTextInput(e);
			string s = e.AsString;
			if (_isChatting) _chattingText += s;
			else if (s == "/") {
				_isChatting = true;
				_consoleScroll = _chattingTextLines * _chattingTextLineHeight * _chattingTextSize.Y;
				_chattingBlinker = 0;}}
		protected override void OnKeyDown(KeyboardKeyEventArgs e) {
			long timestamp = Stopwatch.GetTimestamp();
			base.OnKeyDown(e);
			switch (e.Key) {
				case Keys.F11:
					if (WindowState == WindowState.Fullscreen) WindowState = previousState;
					else { previousState = WindowState; WindowState = WindowState.Fullscreen; } break;
			}
			if (_isChatting) {
				switch (e.Key) {
					case Keys.Escape:
						_isChatting = false; break;
					case Keys.Delete or Keys.Backspace:
						if (_chattingText.Length > 0) { if (_chattingText[^1] == '\n') _chattingTextLines--; _chattingText = _chattingText[..^1]; }
						break;
					case Keys.Enter:
						if (e.Modifiers.HasFlag(KeyModifiers.Shift)) {
							_chattingText += '\n';
							_chattingTextLines++;}
						else {
							string lowercaseChatTxt = _chattingText.ToLower();
							if (DataStuff.noInputChatCommands.TryGetValue(lowercaseChatTxt, out Action<Game> noInputCmd)) noInputCmd(this);
							else {
								if (DataStuff.chatCommands.TryGetValue(lowercaseChatTxt, out Action<Game, string> inputCmd)) inputCmd(this, "");
								else for (int i = lowercaseChatTxt.Length-1; i > 0; i--)
									if (DataStuff.chatCommands.TryGetValue(lowercaseChatTxt[..i], out inputCmd)) {
										inputCmd(this, lowercaseChatTxt[i..]); break; }
							}
							_chattingText = "";
							_chattingTextLines = 1;
							_isChatting = false;}
						break;
					case Keys.V:
						if (e.Modifiers.HasFlag(KeyModifiers.Control)) {
							string cbStr = ClipboardString;
							_chattingTextLines += cbStr.AsSpan().Count('\n');
							_chattingText += cbStr; }
						break;
					case Keys.C: if (e.Modifiers.HasFlag(KeyModifiers.Control)) ClipboardString = _chattingText; break;
					default: break; }}
			else {
				foreach (IMinigame minigame in _currentMinigames) minigame.OnKeyDown(e);
				switch (e.Key) {
					case Keys.F6: if (e.Modifiers.HasFlag(KeyModifiers.Control)) if (profilerOn) {profilerOn = false; profilerVD = [];} else {
								profilerOn = true;
								int amt = profilerFrameTimes.Length << 3;
								profilerVD = new float[amt];
								// int tRSX = 0;
								// float tX = tRSX / (float)TextTexture.Width;

								profilerVD[1]=profilerVD[9]=0.5f/_clientSize.Y+1;
								// profilerVD[2]=profilerVD[10]=profilerVD[6]=profilerVD[14]=0;
								profilerVD[3]=profilerVD[11]=profilerVD[7]=profilerVD[15]=32f/_textRenderer.TextTexture.Height;
								int i=16;
								for (; i < (amt>>1)+1; i <<= 1) Array.Copy(profilerVD, 1, profilerVD, i + 1, i - 1);
								if (i < amt) Array.Copy(profilerVD, 1, profilerVD, i + 1, amt - 1 - i);
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
			base.OnClosing(e);
			currentGame = null;
			foreach (IMinigame minigame in _currentMinigames) minigame.OnClosing(e);
			_textRenderer?.Dispose();
			_playerTorsoMesh?.Dispose();
			_playerHeadMesh?.Dispose();
			_playerArmMesh?.Dispose();
			_playerLegMesh?.Dispose();
			_textureSheet?.Dispose();
			_shader?.Dispose();
			_textShader?.Dispose();}

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
	public class Shader : IDisposable {
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
		public void SetVector3(string name, float v0, float v1, float v2) {
			int location = GL.GetUniformLocation(Handle, name);
			GL.Uniform3(location, v0, v1, v2);}

		public void Dispose() {GL.DeleteProgram(Handle);}}
	// public class CompShader {
	// 	public readonly int Handle;
	// 	public CompShader(string computePath) {
	// 		string ShaderSource = File.ReadAllText(computePath);
	// 		var ComputeShader = GL.CreateShader(ShaderType.ComputeShader);
	// 		GL.ShaderSource(ComputeShader, ShaderSource);

	// 		// compile shader
	// 		GL.CompileShader(ComputeShader);

	// 		// check for errors
	// 		GL.GetShader(ComputeShader, ShaderParameter.CompileStatus, out int success);
	// 		if (success == 0) {
	// 			string infoLog = GL.GetShaderInfoLog(ComputeShader);
	// 			throw new Exception("oh no the shader (" + ComputeShader + ") failed to compile: " + infoLog);}
	// 		// done compiling vertex shader

	// 		Handle = GL.CreateProgram();
	// 		GL.AttachShader(Handle, ComputeShader);
	// 		GL.LinkProgram(Handle);

	// 		GL.DetachShader(Handle, ComputeShader);
	// 		GL.DeleteShader(ComputeShader);}
	// 	public void Use() {GL.UseProgram(Handle);}
	// 	public int GetAttribLocation(string attribName) {
	// 		return GL.GetAttribLocation(Handle, attribName);}
	// 	public void SetInt(string name, int value) {
	// 		int location = GL.GetUniformLocation(Handle, name);
	// 		GL.Uniform1(location, value);}
	// 	public void SetMatrix4(string name, Matrix4 value) {
	// 		int location = GL.GetUniformLocation(Handle, name);
	// 		GL.UniformMatrix4(location, true, ref value);}

	// 	public void SetTextureLayer(int layer) {SetInt("textureLayer", layer);}
	// 	public void SetTextureLocation(string name, Vector4 LocationAndSize) {
	// 		int location = GL.GetUniformLocation(Handle, name);
	// 		GL.Uniform4(location, LocationAndSize);}
	// 	public void SetVector3(string name, Vector3 value) {
	// 		int location = GL.GetUniformLocation(Handle, name);
	// 		GL.Uniform3(location, value);}

	// 	public void Dispose() {GL.DeleteProgram(Handle);}}
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
		public Vector3 CameraToTargetOffset = new(1f/MathF.Sqrt(3f), 1f/MathF.Sqrt(3f), -1f/MathF.Sqrt(3f));
		public float CameraDistFromTarget = 8f;
		// public Vector3 CameraFront; = new Vector3(0f, 0f, -1f);
		public float MinDist = .01f, MaxDist = 1024f;
		public Vector3 Target;
		public Vector3 Position;
		public Vector3 Up = Vector3.UnitY;
		public Vector3 Direction;
		public Vector3 Right;
		/// <summary>
		/// The matrix to project objects to the screen. It is the view but also multiplied by the projection matrix. idk if it helps but yeah.
		/// </summary>
		public Matrix4 View, otherview;

		public float Pitch, Yaw, MouseSensitivity = .005f;

		public Matrix4 Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), 800f / 600f, .1f, 10000f);

		public Camera(Vector3 position, Vector3 target, Vector3 up) {
			Direction = Vector3.Normalize(position - target);
			Right = Vector3.Normalize(Vector3.Cross(up, Direction));
			otherview = Matrix4.LookAt(position, target, up);
			View = otherview * Projection;

			Position = position;
			Target = target;
			Up = up; }
		public Camera() { }
		public void UpdateVectors() {
			float NCosPitch = -MathF.Cos(Pitch);
			DataStuff.NormalizeVec(NCosPitch*MathF.Cos(Yaw),-MathF.Sin(Pitch),NCosPitch*MathF.Sin(Yaw),out CameraToTargetOffset);
			Position = Target + CameraToTargetOffset * CameraDistFromTarget;
			// Direction = Vector3.Normalize(Position - Target);
			// Right = Vector3.Normalize(Vector3.Cross(Up, Direction));
			// View = Matrix4.LookAt(Position, Target, Up) * Projection;
			Direction = Vector3.Normalize(CameraToTargetOffset);
			Right = Vector3.Normalize(Vector3.Cross(Up, Direction));
			otherview = Matrix4.LookAt(Position, Target, Up);
			View = otherview * Projection;
		}}
	public class ObjectMesh {
		public static int MeshCount = 0;
		public static List<ObjectMesh> Meshes = [];
		public int Type;
		public List<Vector3> Positions, Rotations, Scales;
		public List<Matrix4> Models;
		public int IndicesLen;
		public int VertexArrayObject;
		public int VertexBufferObject;
		public int ElementBufferObject;
		public ObjectMesh(Vector3 position, Vector3 rotation, Vector3 scale, float[] vertices, uint[] indices) {
			MeshCount++;
			Type = MeshCount;
			Positions = [position];
			Rotations = [rotation];
			Scales = [scale];
			Models = [DataStuff.CreateScaleRotXYZTrans(scale, rotation, position)];
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

			Meshes.Add(this);}
		public void Update(int index, Vector3 position, Vector3 rotation, Vector3 scale) {
			Positions[index] = position;
			Rotations[index] = rotation;
			Scales[index] = scale;
			Models[index] = DataStuff.CreateScaleRotXYZTrans(scale, rotation, position);}
		public int AddNew(Vector3 position, Vector3 rotation, Vector3 scale) {
			Positions.Add(position);
			Rotations.Add(rotation);
			Scales.Add(scale);
			Models.Add(DataStuff.CreateScaleRotXYZTrans(scale, rotation, position));
			return Models.Count - 1;}
		public int AddNewInBulk(Vector3[] positions, Vector3[] rotations, Vector3[] scales) {
			for (int i = 0; i < positions.Length; i++) {
				Vector3 pos = positions[i], rot = rotations[i], scale = scales[i];
				Positions.Add(pos);
				Rotations.Add(rot);
				Scales.Add(scale);
				Models.Add(DataStuff.CreateScaleRotXYZTrans(scale,rot,pos));}
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
		public Vector3 Velocity;
		public ObjectMesh Mesh;
		public int MeshIndex;
		public Obj(Vector3 position, Vector3 rotation, Vector3 scale, ObjectMesh mesh) {
			Mesh = mesh;
			MeshIndex = mesh.AddNew(position, rotation, scale);}
		public Obj(ObjectMesh mesh, int meshIndex) {
			Mesh = mesh;
			MeshIndex = meshIndex;}
		public void Update(Vector3 position, Vector3 rotation, Vector3 scale) {
			Mesh.Update(MeshIndex, position, rotation, scale);}
		public void UpdateModel(Matrix4 model) {Mesh.Models[MeshIndex] = model;}
		public void StepPhysics(float deltaTime, Vector3 gravity) {
			Velocity += gravity * deltaTime;
			Mesh.Positions[MeshIndex] += Velocity * deltaTime;
			if (Mesh.Positions[MeshIndex].Y < Game._groundHeight) {
				Mesh.Positions[MeshIndex] = new Vector3(Mesh.Positions[MeshIndex].X, Game._groundHeight, Mesh.Positions[MeshIndex].Z);
				Velocity.Y = 0f;}}
		public void Draw(Shader shader, bool bind) { Mesh.DrawAtIndex(shader, MeshIndex, bind); }}
	public class Player {
		public Vector3 RootPosition, RootRotation, RootScale, Velocity, CamTargetOffset;
		public Matrix4 RootModel;
		public float JumpPower = 5f, Walkspeed = 5f;
		public Vector3 Gravity = (0f, -9.81f, 0f);
		public bool IsGrounded = true, IsFlying = false;
		public Obj[] Limbs;
		public Vector3[] LimbPositions, LimbRotations, LimbScales;
		public Obj Torso, Head, LeftArm, RightArm, LeftLeg, RightLeg;
		public static readonly int LimbCount = 6;

		public Player(Vector3 position, Vector3 rotation, Vector3 scale, Obj[] limbs, Vector3[] limbPositions, Vector3[] limbRotations, Vector3[] limbScales) {
			RootPosition = position;
			RootRotation = rotation;
			DataStuff.CreateScaleRotXYZTrans(scale,rotation,position, out RootModel);
			RootScale = scale;
			Velocity.X = 0f;
			Velocity.Y = 0f;
			Velocity.Z = 0f;
			CamTargetOffset.X = 0;
			CamTargetOffset.Y = 1;
			CamTargetOffset.Z = 0;
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
			for (int i = 0; i < LimbCount; i++) Limbs[i].Update(LimbPositions[i] + RootPosition, LimbRotations[i] + RootRotation, LimbScales[i] * RootScale);}
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
		public void UpdateMats() {
			DataStuff.CreateScaleRotXYZTrans(RootScale,RootRotation,RootPosition, out RootModel);
			Limbs[0].UpdateModel(DataStuff.CreateScaleRotXYZTrans(LimbScales[0], LimbRotations[0], LimbPositions[0]) * RootModel);
			Limbs[1].UpdateModel(
				DataStuff.LerpMatrices(
					Limbs[1].Mesh.Models[Limbs[1].MeshIndex],
					DataStuff.CreateScaleRotXYZTrans(LimbScales[1], LimbRotations[1], LimbPositions[1]) * RootModel, 0.8f));
			for (int i = 2; i < LimbCount; i++){
				Obj limb = Limbs[i];
				limb.UpdateModel(
					DataStuff.LerpMatrices(
						limb.Mesh.Models[limb.MeshIndex],
						DataStuff.CreateScaleRotXYZTrans(LimbScales[i], LimbRotations[i], LimbPositions[i]) * RootModel, 0.6f));} }
		public void UpdateLimbs() {
			Limbs[0].UpdateModel(DataStuff.CreateScaleRotXYZTrans(LimbScales[0], LimbRotations[0], LimbPositions[0]) * RootModel);
			Limbs[1].UpdateModel(
				DataStuff.LerpMatrices(
					Limbs[1].Mesh.Models[Limbs[1].MeshIndex],
					DataStuff.CreateScaleRotXYZTrans(LimbScales[1], LimbRotations[1], LimbPositions[1]) * RootModel, 0.8f));
			for (int i = 2; i < LimbCount; i++){
				Obj limb = Limbs[i];
				limb.UpdateModel(
					DataStuff.LerpMatrices(
						limb.Mesh.Models[limb.MeshIndex],
						DataStuff.CreateScaleRotXYZTrans(LimbScales[i], LimbRotations[i], LimbPositions[i]) * RootModel, 0.6f));} }
		public void StepPhysics(float deltaTime) {
			if (!IsGrounded) Velocity += Gravity * deltaTime;
			RootPosition += Velocity * deltaTime;
			if (RootPosition.Y < Game._groundHeight) {
				RootPosition.Y = Game._groundHeight;
				Velocity.Y = 0f;
				IsGrounded = true;}
			UpdateMats();}
		public void Render(Shader shader) {
			Torso.Draw(shader, true);
			Head.Draw(shader, true);
			LeftArm.Draw(shader, true);
			RightArm.Draw(shader, false);
			LeftLeg.Draw(shader, true);
			RightLeg.Draw(shader, false);}
		public void OnUpdateFrame(Game game, byte[] ksData) {
			moveBehavior(this, game, ksData);
		}
		public Action<Player, Game, byte[]> moveBehavior = defaultMoveBehavior;
		public static readonly Action<Player, Game, byte[]> defaultMoveBehavior = delegate (Player plr, Game game, byte[] ksData) {
			float moveAmount = plr.Walkspeed * game._tickSpeedInv;
			if (plr.IsFlying) {
				int movement = ((ksData[((int)Keys.S)>>3]>>(((int)Keys.S)&7))&1)-((ksData[((int)Keys.W)>>3]>>(((int)Keys.W)&7))&1);
				int movement2 = ((ksData[((int)Keys.D)>>3]>>(((int)Keys.D)&7))&1)-((ksData[((int)Keys.A)>>3]>>(((int)Keys.A)&7))&1);
				int movement3 = (((ksData[((int)Keys.Space)>>3]>>(((int)Keys.Space)&7))&1)|((ksData[((int)Keys.E)>>3]>>(((int)Keys.E)&7))&1))-
				(((ksData[((int)Keys.LeftShift)>>3]>>(((int)Keys.LeftShift)&7))&1)|((ksData[((int)Keys.Q)>>3]>>(((int)Keys.Q)&7))&1));
				if ((movement|movement2|movement3)!=0) {
					Vector3 plrMovement;
					float scale = moveAmount;
					Vector3 input = game._camera.Direction*movement+game._camera.Right*movement2+game._camera.Up*movement3;
					float mag = MathF.Sqrt(input.X*input.X+input.Y*input.Y+input.Z*input.Z);
					float num = scale / mag;
					plrMovement.X = input.X * num;
					plrMovement.Y = input.Y * num;
					plrMovement.Z = input.Z * num;
					plr.RootRotation.Y = ((plrMovement.Z>0?MathF.Asin(input.X/mag):(MathF.PI-MathF.Asin(input.X/mag)))+MathF.Tau)%MathF.Tau;
					plr.RootRotation.X = 0.25f + 0.5f*(float)Math.Sin(game._gameTime);
					plr.RootPosition += plrMovement;
					plr.UpdateMats();
				}
			} else { // OPTIMIZATION FOR KS USING THE KS'S BITARRAY BYTE ARRAY: (ks_keys[index>>3] & (1 << (index & 7))) != 0;
				int s1 = ((ksData[((int)Keys.S)>>3]>>(((int)Keys.S)&7))&1)-((ksData[((int)Keys.W)>>3]>>(((int)Keys.W)&7))&1);
				int s2 = ((ksData[((int)Keys.D)>>3]>>(((int)Keys.D)&7))&1)-((ksData[((int)Keys.A)>>3]>>(((int)Keys.A)&7))&1);
				if ((s1|s2)!=0) {
					Vector3 plrMovement;
					plrMovement.X = game._camera.Direction.X*s1+game._camera.Right.X*s2;
					plrMovement.Y = 0;
					plrMovement.Z = game._camera.Direction.Z*s1+game._camera.Right.Z*s2;
					float scale = moveAmount/MathF.Sqrt(plrMovement.X*plrMovement.X+plrMovement.Z*plrMovement.Z);
					plrMovement.X *= scale;
					plrMovement.Z *= scale;
					float targetRot = ((plrMovement.Z>0?MathF.Asin(plrMovement.X/moveAmount):(MathF.PI-MathF.Asin(plrMovement.X/moveAmount)))+MathF.Tau)%MathF.Tau;
					float diff = targetRot-plr.RootRotation.Y;
					float prevRot = plr.RootRotation.Y;
					plr.RootRotation.Y = (diff<MathF.PI&&diff>-MathF.PI)?
						plr.RootRotation.Y+diff*.3333333333f:
						((plr.RootRotation.Y + ((diff>0)?
							(MathF.Tau+(diff-MathF.Tau)*.3333333333f):
							((diff+MathF.Tau)*.3333333333f)))%MathF.Tau);
					if (plr.RootRotation.Y == prevRot) plr.RootRotation.Y = targetRot;
					Console.WriteLine(plr.RootRotation.Y+", "+diff);
					plr.RootPosition += plrMovement;
				}
				if (plr.IsGrounded && (ksData[((int)Keys.Space)>>3]&(1<<(((int)Keys.Space)&7)))>0) plr.Jump();
				plr.StepPhysics(game._tickSpeedInv);}
		};
		public void Jump() {
			Velocity.Y = JumpPower;
			IsGrounded = false;}}
	public struct FontCharacterData {
		public Dictionary<char, GlyphData> Chars = [];
		public Dictionary<string, GlyphData> SChars = [];
		public FontCharacterData() {} // do not remove this line
		public FontCharacterData(Dictionary<char, GlyphData> Chars) { this.Chars = Chars; }
		public FontCharacterData(Dictionary<string, GlyphData> SChars) { this.SChars = SChars; }
		public FontCharacterData(Dictionary<char, GlyphData> Chars, Dictionary<string, GlyphData> SChars) { this.Chars = Chars; this.SChars = SChars; }}

	public sealed class VideoRecorder : IDisposable {
		private readonly int _w, _h;
		private bool _recAllFrames;
		private readonly Process _ffmpeg;
		private readonly Stream _stdin;
		private readonly byte[] _fBuffer;
		public bool IsRecording {get {return _recording;}}
		private bool _recording;
		private long _nextTickNs, _tickStepNs;
		public VideoRecorder(int w, int h, int fps, string p, float speed = 1) { // no nvenc yet also no audio
			// (_w, _h, _recAllFrames, _path) = (w,h,fps<0,p);
			_w = w; _h = h; _recAllFrames = fps < 0;
			_fBuffer = new byte[w*h<<2];
			_tickStepNs = (long)(1000000000d/(fps*speed));
			string args = "-n -f rawvideo -pix_fmt bgra -s "+w+'x'+h+" -r "+(fps*speed).ToString("N4")+" -i - -vf \"vflip\" -an -c:v libx265 -preset superfast -crf 25 -pix_fmt yuv420p \""+p+'\"';
			_ffmpeg = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "ffmpeg",
					Arguments = args,
					RedirectStandardInput=true,
					RedirectStandardOutput=true,
					RedirectStandardError=true,
					UseShellExecute=false,
					CreateNoWindow=true}};
			_ffmpeg.Start();
			Console.Write("argument thingy for ffmpeg is "+_ffmpeg.StartInfo.Arguments+'\n');
			if (_ffmpeg.HasExited) throw new InvalidOperationException("oops my ffmpeg crashed. I lost my data, but I had an antivirus. code: "+_ffmpeg.ExitCode);
			_stdin = _ffmpeg.StandardInput.BaseStream;
			_recording = true;
			_ffmpeg.BeginErrorReadLine();
			_ffmpeg.ErrorDataReceived += (sender, e) => Console.Write("FFmpeg:"+e.Data+'\n');
			_nextTickNs = Stopwatch.GetTimestamp()*1000000000L/Stopwatch.Frequency; }
		public VideoRecorder(int w, int h, float resfps, string p, float inpfps) {
			_w = w; _h = h; _recAllFrames = resfps < 0;
			_fBuffer = new byte[w*h<<2];
			_tickStepNs = (long)(1000000000d/inpfps);
			StringBuilder args = new("-n -f rawvideo -pix_fmt bgra -s ", 256);args.Append(w);args.Append('x');args.Append(h);args.Append(" -r ");args.Append(resfps.ToString("N4"));args.Append(" -i - -vf \"vflip\" -an -c:v libx265 -preset superfast -crf 25 -pix_fmt yuv420p \"");args.Append(p);args.Append('\"');
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
			_ffmpeg.ErrorDataReceived += (sender, e) => Console.Write("FFmpeg:"+e.Data+'\n');
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
			_ffmpeg.ErrorDataReceived += (sender, e) => Console.Write("FFmpeg:"+e.Data+'\n');
			_nextTickNs = Stopwatch.GetTimestamp()*1000000000L/Stopwatch.Frequency; }
		public void CaptureFrame(Vector2i ClientSize) {
			if (!_recording) return;
			if (_ffmpeg.HasExited) { _recording = false; Console.WriteLine("ffmpeg exited: "+_ffmpeg.ExitCode); Dispose(); return; }
			if (!_recAllFrames) {
				if ((Stopwatch.GetTimestamp() * 1000000000L / Stopwatch.Frequency) < _nextTickNs) return;
				_nextTickNs += _tickStepNs; }
			if (ClientSize.X != _w || ClientSize.Y != _h) {Dispose(); return;}
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
			try { if (!_ffmpeg.WaitForExit(8000)) _ffmpeg.Kill(true); } catch {} _ffmpeg?.Dispose(); }
	}
	public static class AnnouncementsManager {
		public struct Announcement {
			public string Message;
			public long AppearTS, DisappearTS;
			public Vector4 TextColor;
			public uint BGColor;
			public bool SpecialText;
			public float FadeInTime = .25f, FadeOutTime;
			public Announcement(string msg, long dsts, Vector4 textColor, Vector4 bgColor, bool st = false, float fot = 1) {
				AppearTS = Stopwatch.GetTimestamp();
				Message = msg;
				DisappearTS = dsts;
				TextColor = textColor;
				BGColor = ((uint)(bgColor.W*255)<<24)|((uint)(bgColor.Z*255)<<16)|((uint)(bgColor.Y*255)<<8)|(uint)(bgColor.X*255);
				SpecialText = st;
				FadeOutTime = fot;
			}
			public Announcement(string msg, long dsts, Vector4 textColor, uint bgColor, bool st = false, float fot = 1) {
				AppearTS = Stopwatch.GetTimestamp();
				Message = msg;
				DisappearTS = dsts;
				TextColor = textColor;
				BGColor = bgColor;
				SpecialText = st;
				FadeOutTime = fot;
			}
			public Announcement(string msg, long dsts, long apts, Vector4 textColor, Vector4 bgColor, bool st = false, float fot = 1) {
				Message = msg;
				DisappearTS = dsts; AppearTS = apts;
				TextColor = textColor;
				BGColor = ((uint)(bgColor.W*255)<<24)|((uint)(bgColor.Z*255)<<16)|((uint)(bgColor.Y*255)<<8)|(uint)(bgColor.X*255);
				SpecialText = st;
				FadeOutTime = fot;
			}
			public Announcement(string msg, long dsts, long apts, Vector4 textColor, uint bgColor, bool st = false, float fot = 1) {
				Message = msg;
				DisappearTS = dsts; AppearTS = apts;
				TextColor = textColor;
				BGColor = bgColor;
				SpecialText = st;
				FadeOutTime = fot;
			}
		}
		public static List<Announcement> Announcements = [];
		public static int VAO, VBO, tcUniformLocation;
		public static AnnouncementVertexStruct[] dataBuffer = new AnnouncementVertexStruct[Text.BulkDrawConst*4];
		public static Shader AnnShader;
		public static void OnLoad() {
			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData(BufferTarget.ArrayBuffer, Text.BulkDrawConst*4*(sizeof(float)*4+sizeof(uint)), 0, BufferUsageHint.DynamicDraw);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, Text.EBO);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4*sizeof(float)+sizeof(uint), 0);
			GL.EnableVertexAttribArray(1);
			GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, false, 4*sizeof(float)+sizeof(uint), 4*sizeof(float));
			AnnShader = new("Shaders/annTS.vert", "Shaders/annTS.frag");
			AnnShader.Use();
			tcUniformLocation = GL.GetUniformLocation(AnnShader.Handle, "tc");
			AnnShader.SetInt("text", 1);
		}
		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct AnnouncementVertexStruct {
			public float a,b,c,d;
			public uint color;
		}
		public static void OnRender(Vector2i windowSize) {
			int AnnCount = Announcements.Count;
			if (AnnCount == 0) return;
			long ts = Stopwatch.GetTimestamp();
			AnnShader.Use();
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			FontCharacterData fontCharData = FontCharFillerThing.FontCharDeeta;
			Dictionary<char, GlyphData> Chars = fontCharData.Chars;
			Dictionary<string, GlyphData> SChars = fontCharData.SChars;
			int vI = 4;
			const float posScaleX = -0.9f, posScaleY = .95f,
			textScaleX = 6, textScaleY = 6;
			float WSIxTSX = textScaleX/windowSize.X,
			WSIxTSY = textScaleY/windowSize.Y,
			spaceSize = WSIxTSX*3,
			tabSize = spaceSize*4,
			lineHeight = WSIxTSY*10f,
			NextMessageYOffset = lineHeight+spaceSize*2.5f;
			float startaX = posScaleX;
			float startaY = posScaleY;
			float bpY = startaY;
			GlyphData thingyWithDataUsefulForTheBG = fontCharData.SChars["0x0ON"];
			for (int index = AnnCount-1; index > -1; index--){
				Announcement tA = Announcements[index];
				if (tA.DisappearTS < ts) {Announcements.RemoveAt(index); continue;}
				float SFromAppear = (float)((ts-tA.AppearTS)/(double)Stopwatch.Frequency);
				if (SFromAppear < 0) continue;
				float SToDisappear = (float)((tA.DisappearTS-ts)/(double)Stopwatch.Frequency);
				float TransMulti = ((SToDisappear<tA.FadeOutTime)?SToDisappear/tA.FadeOutTime:1)*((SFromAppear<tA.FadeInTime)?SFromAppear/tA.FadeInTime:1);

				float startHeight = bpY;
				float bpX = startaX;
				bpY -= NextMessageYOffset;
				float maxX = bpX, minY = bpY;

				string msg = tA.Message;
				int msgLen = msg.Length;

				if (tA.SpecialText) {
					for (int i = 0; i < msgLen; i++) {
						uint PerCharColor = 0xFFFFFFFF;
						char c = msg[i];
						GlyphData Chr;
						switch (c) {
							case ' ': bpX += spaceSize; continue;
							case '	': bpX += tabSize; continue;
							case '\n': bpX = startaX; bpY -= lineHeight; continue;
							case '\\':
								i++;
								char nextChar;
								if (i+1 > msgLen || (nextChar=msg[i]) == '\\') { Chr = Chars['\\']; i--; break; } // if this is the last char or the next char is another '\\' then show a '\\' char.
								if (nextChar == '|') { Chr = Chars['\\']; break; } // if the next char is a | char (my format is \| for '\\' chars) then show a '\\', then increment i so the '|' isn't shown.
								if (nextChar == '\n') goto case '\n'; // if the line goes to a new line then increment i and do the next line stuff.
								int IsStacking = 0;
								int j = msg.IndexOfAny(Text.CharSearchThingy, i);
								if (j == -1) j = msgLen; else if (msg[j] != '|') IsStacking = 1;
								int len = j - i;
								string s = (len==-1)?msg[i..]:msg.Substring(i, len);
								if (!SChars.TryGetValue(s, out Chr)) { // if specil char not found, uses unknown w/ red-ish color.
									PerCharColor = (uint)Random.Shared.Next()<<1^(uint)Random.Shared.Next();
									Chr = SChars["unknown"];} i = j - IsStacking; // jmps 2 da next real char's index
								break;
							default: if (!Chars.TryGetValue(c, out Chr)) Chr = Chars['?']; break;}
						dataBuffer[vI].a=dataBuffer[vI+1].a=bpX+Chr.bearingX*WSIxTSX;
						float endX = dataBuffer[vI+2].a=dataBuffer[vI+3].a=bpX+Chr.spbX*WSIxTSX;
						float startY = dataBuffer[vI].b=dataBuffer[vI+3].b=bpY+Chr.bearingY*WSIxTSY;
						dataBuffer[vI+1].b=dataBuffer[vI+2].b=bpY+Chr.spbY*WSIxTSY;
						dataBuffer[vI].c=dataBuffer[vI+1].c=Chr.tStartX;dataBuffer[vI].d=dataBuffer[vI+3].d=Chr.tStartY;
						dataBuffer[vI+2].c=dataBuffer[vI+3].c=Chr.tEndX;dataBuffer[vI+1].d=dataBuffer[vI+2].d=Chr.tEndY;
						dataBuffer[vI].color=dataBuffer[vI+1].color=dataBuffer[vI+2].color=dataBuffer[vI+3].color=PerCharColor;
						if (endX > maxX) maxX = endX; if (startY < minY) minY = startY;
						vI += 4; bpX += Chr.advanceX*WSIxTSX; bpY += Chr.advanceY*WSIxTSY;}
				} else {
					for (int i = 0; i < msgLen; i++) {
						const uint PerCharColor = 0xFFFFFFFF;
						char c = msg[i];
						GlyphData Chr;
						switch (c) {
							case ' ': bpX += spaceSize; continue;
							case '	': bpX += tabSize; continue;
							case '\n': bpX = startaX; bpY -= lineHeight; continue;
							default: if (!Chars.TryGetValue(c, out Chr)) Chr = Chars['?']; break;}
						dataBuffer[vI].a=dataBuffer[vI+1].a=bpX+Chr.bearingX*WSIxTSX;
						float endX = dataBuffer[vI+2].a=dataBuffer[vI+3].a=bpX+Chr.spbX*WSIxTSX;
						float startY = dataBuffer[vI].b=dataBuffer[vI+3].b=bpY+Chr.bearingY*WSIxTSY;
						dataBuffer[vI+1].b=dataBuffer[vI+2].b=bpY+Chr.spbY*WSIxTSY;
						dataBuffer[vI].c=dataBuffer[vI+1].c=Chr.tStartX;dataBuffer[vI].d=dataBuffer[vI+3].d=Chr.tStartY;
						dataBuffer[vI+2].c=dataBuffer[vI+3].c=Chr.tEndX;dataBuffer[vI+1].d=dataBuffer[vI+2].d=Chr.tEndY;
						dataBuffer[vI].color=dataBuffer[vI+1].color=dataBuffer[vI+2].color=dataBuffer[vI+3].color=PerCharColor;
						if (endX > maxX) maxX = endX; if (startY < minY) minY = startY;
						vI += 4; bpX += Chr.advanceX*WSIxTSX; bpY += Chr.advanceY*WSIxTSY;}}
				// AnnShader.SetVector4("tc", tA.BGColor);
				dataBuffer[0].a=dataBuffer[1].a=startaX-spaceSize;
				dataBuffer[2].a=dataBuffer[3].a=maxX+spaceSize;
				dataBuffer[0].b=dataBuffer[3].b=startHeight+spaceSize;
				dataBuffer[1].b=dataBuffer[2].b=minY-spaceSize;
				dataBuffer[0].c=dataBuffer[1].c=dataBuffer[2].c=dataBuffer[3].c=thingyWithDataUsefulForTheBG.tStartX;
				dataBuffer[0].d=dataBuffer[3].d=dataBuffer[1].d=dataBuffer[2].d=-thingyWithDataUsefulForTheBG.tStartY;
				switch (tA.BGColor) {
					case 1:
						uint _Transparency = (uint)(255*TransMulti)<<24;
						float timeCycle = (float)((double)(ts%Stopwatch.Frequency)*6/Stopwatch.Frequency);
						dataBuffer[0].color=_Transparency|DataStuff.HueTo0ABGR_uint(timeCycle);
						dataBuffer[1].color=_Transparency|DataStuff.HueTo0ABGR_uint((timeCycle+.25f)%6);
						dataBuffer[2].color=_Transparency|DataStuff.HueTo0ABGR_uint((timeCycle+1f)%6);
						dataBuffer[3].color=_Transparency|DataStuff.HueTo0ABGR_uint((timeCycle+.75f)%6);
						GL.Uniform4(tcUniformLocation, tA.TextColor.X/255f,tA.TextColor.Y/255f,tA.TextColor.Z/255f,tA.TextColor.W*TransMulti/255f);
					break;
					default:
						uint Transparency = (uint)((tA.BGColor>>24&255)*TransMulti)<<24;
						dataBuffer[1].color=tA.BGColor&0xffffff|Transparency;
						dataBuffer[2].color=(((tA.BGColor&255)*3)>>2)| // 75% desat and 25% 0x000000
						((((tA.BGColor>>2)&(255u<<6))*3)&(255u<<8))|
						((((tA.BGColor>>2)&(255u<<14))*3)&(255u<<16))|Transparency;
						uint DesatColor = dataBuffer[0].color=((255+(tA.BGColor&255)*3)>>2)| // mix with 75% color 25% 0xFFFFFFFF
						(((255u<<6)+((tA.BGColor>>2)&(255u<<6))*3)&(255u<<8))|
						(((255<<14)+((tA.BGColor>>2)&(255u<<14))*3)&(255u<<16))|Transparency;
						dataBuffer[3].color=(((DesatColor&255)*3)>>2)| // 75% desat and 25% 0x000000
						((((DesatColor>>2)&(255u<<6))*3)&(255u<<8))|
						((((DesatColor>>2)&(255u<<14))*3)&(255u<<16))|Transparency;
						GL.Uniform4(tcUniformLocation, tA.TextColor.X/255f,tA.TextColor.Y/255f,tA.TextColor.Z/255f,tA.TextColor.W*TransMulti/255f);
						break;
				}
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, (sizeof(float)*4+sizeof(uint))*4*vI, dataBuffer);
				GL.DrawElements(PrimitiveType.Triangles, (vI>>1)*3, DrawElementsType.UnsignedInt, 0); vI = 4;
			}
		}
	}
}
