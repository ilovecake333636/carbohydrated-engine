#undef MINING_GAME_DEBUG
#undef MINING_GAME_PROFILELOG
#define MINING_GAME_PER_FACE_CULL
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace GameEngineThing {
	/// <summary>
	/// HEY YOU! For your minigame to actually work, you need the following:
	/// public static Dictionary<string, Action<Game>> InGameConstructorThings;
	/// public static string GameIdentifier;
	/// For the dict, it needs to exist and have at least one value which constructs based off a reopen command.
	/// For the string, it needs to be set to something at least i think.
	/// If you want, you CAN implement a static StartInit() function. don't have to.
	/// </summary>
	public class IMinigame {
		// /// <summary>
		// /// HEY BUDDY you've gotta override this and also the GameIdentifier or something i think or else your game won't be playable i think (bc can't launch)
		// /// </summary>
		// public static Dictionary<string, Action<Game>> InGameConstructorthings;
		// public static string GameIdentifier;
		// public static void StartInit() { }
		public virtual void OnLoad(Game game) { }
		public virtual void OnUpdateFrame(Game game, double dt) { }
		public virtual void OnRenderFrame(Game game, double dt) { }
		public virtual void OnInGameConsoleInput() { }
		public virtual void OnKeyDown(KeyboardKeyEventArgs e) { }
		public virtual void OnKeyUp(KeyboardKeyEventArgs e) { }
		public virtual void OnMouseDown(MouseButtonEventArgs e) { }
		public virtual void OnMouseUp(MouseButtonEventArgs e) { }
		public virtual void OnClosing(CancelEventArgs e) { }
		public virtual void OnEngineTick(Game game, double tickDT) { }
		public virtual void OnResize(ResizeEventArgs e) {}
		}
	public class Pong : IMinigame {
		// public static void StartInit() {
		// 	GameIdentifier = "pong";
		// 	InGameConstructorthings = new() {
		// 		["pong"] = delegate (Game game) {
		// 			game._currentMinigames.Add(new Pong(new Vector3(10f, 0f, 10f), new Vector3(1f), new Vector3(270f, 0f, 0f)));
		// 			game._gameModes.Add("pong");}
		// 	};
		// }
		public const string GameIdentifier = "pong";
		public static Dictionary<string, Action<Game>> InGameConstructorthings = new() {
			["pong"] = delegate (Game game) {
				game._currentMinigames.Add(new Pong((10f,0,10), Vector3.One, (270f,0,0)));
				game._gameModes.Add(GameIdentifier);
			}
		};
		public bool AngleMode = true; // true is degrees, and false is radians
		public Vector3 Pos;
		public Vector3 Scale;
		public Vector3 Rot;
		public Matrix4 Transformation;
		// game should be from -5 to 5. idk why i set it this way i just did.
		// the Y value: positive is up, negative is down. X val: positive goes right, negative goes left
		public float Paddle1X = -4.5f;
		public float Paddle1Y = 0f;
		public float Paddle2X = 4.5f;
		public float Paddle2Y = 0f;
		public float BallX = 0f;
		public float BallY = 0f;
		public float BallVX = 7f;
		public float BallVY = -7f;
		public Keys P1UpBind = Keys.R;
		public Keys P1DownBind = Keys.F;
		public Keys P2UpBind = Keys.Y;
		public Keys P2DownBind = Keys.H;
		public bool GameActive = true;
		public float GameUpdRate = 30;
		public float HittingWidth = .5f;
		public float PaddleHeight = 3f;
		public int P1Score = 0;
		public int P2Score = 0;
		public float BallRadius = .5f;
		public float PaddleSpeed = 19f;
		public sbyte P1DownDown; // 0 is false, 1 is true. please dont put anything else okay idk whats gonna happen.
		public sbyte P1UpDown; // 0 is false, 1 is true. please dont put anything else okay idk whats gonna happen.
		public sbyte P2DownDown; // 0 is false, 1 is true. please dont put anything else okay idk whats gonna happen.
		public sbyte P2UpDown; // 0 is false, 1 is true. please dont put anything else okay idk whats gonna happen.
		public sbyte P1State = 0;
		public sbyte P2State = 0;
		public ObjectMesh _cube, _plane;
		public struct GameKeyState1(Keys key, bool down) {
			public Keys Key = key;
			public bool Down = down;}
		public List<GameKeyState1> KeyInputQueue = [];
		public Pong() { }
		public Pong(Vector3 pos, Vector3 scale, Vector3 rot) {
			Pos = pos; Scale = scale; Rot = rot;
			Transformation = Matrix4.CreateRotationX(Rot.X) * Matrix4.CreateRotationY(Rot.Y) * Matrix4.CreateRotationZ(Rot.Z) * Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Pos);}
		public Pong(Matrix4 Transformation) {
			this.Transformation = Transformation;}
		public void Update(float DT) {
			// process key presses

			for (int i = 0; i < KeyInputQueue.Count; i++) {
				GameKeyState1 e = KeyInputQueue[i];
				if (e.Down) {
					if (e.Key == P1DownBind) P1DownDown = 1;
					if (e.Key == P1UpBind) P1UpDown = 1;
					if (e.Key == P2DownBind) P2DownDown = 1;
					if (e.Key == P2UpBind) P2UpDown = 1;}
				else {
					if (e.Key == P1DownBind) P1DownDown = 0;
					if (e.Key == P1UpBind) P1UpDown = 0;
					if (e.Key == P2DownBind) P2DownDown = 0;
					if (e.Key == P2UpBind) P2UpDown = 0;}}
			P1State = (sbyte)(P1UpDown - P1DownDown);
			P2State = (sbyte)(P2UpDown - P2DownDown);
			KeyInputQueue = [];

			Paddle1Y = Math.Max(Math.Min(Paddle1Y + P1State * PaddleSpeed * DT, 5f), -5f);
			Paddle2Y = Math.Max(Math.Min(Paddle2Y + P2State * PaddleSpeed * DT, 5f), -5f);
			// Paddle1X - W1 - W2 <= BallX <= Paddle1X + W1 + W2; W1 is HittingWidth; W2 BallRadius
			if (BallX >= Paddle1X - HittingWidth - BallRadius && BallX <= Paddle1X + HittingWidth + BallRadius) /* if ball is within ponging distance */ {
				// ball is within ponging distance.
				// Paddle1Y - H1 - H2 <= BallY <= Paddle1Y + H1 + H2; H1 is PaddleHeight * .5f; H2 is BallRadius
				if (BallY >= Paddle1Y - PaddleHeight * .5f - BallRadius && BallY <= Paddle1Y + PaddleHeight * .5f + BallRadius) {
					// paddle is at the ball and the ball can be bounced back.
					BallVX = -BallVX * 1.05f; // ball accelerates slightly each time :3
					BallX = Paddle1X + HittingWidth + Math.Sign(BallVX) * HittingWidth;
					BallX += BallVX * DT;
					BallY += BallVY * DT;}
				else {
					// ball is not touching the paddle
					BallX += BallVX * DT;
					BallY += BallVY * DT;
					if (BallX >= Paddle1X - HittingWidth && BallX <= Paddle1X + HittingWidth && BallY >= Paddle1Y - PaddleHeight * .5f && BallY <= Paddle1Y + PaddleHeight * .5f) {
						// the ball, after moving, is still within ponging distance, and the paddle is at the correct position, aka touching the paddle
						BallVX = -BallVX * 1.05f;
						if (BallVX == 0) BallX = 0;
						else
							BallX = HittingWidth + Math.Sign(BallVX) * HittingWidth;}}}
			else {
				// ball is not within ponging distance.
				BallX += BallVX * DT;
				BallY += BallVY * DT;
				if (BallX >= 5f) /*ball bounces on the right wall.*/ {
					BallVX = -BallVX * 1.05f;
					BallX = 5f + BallVX * DT;}
				if (BallX <= -5f) /* ball went to the left wall (aka you lost lol) */ {
					Console.WriteLine("you lost lol");
					BallX = 0f;
					BallY = 0f;
					BallVX = -BallVX * .9f;
					BallVY *= .9f;
					P2Score++;}}
			if (BallY >= 5f) {
				BallY = 5f - BallVY * DT;
				BallVY = -BallVY * 1.05f;}
			else if (BallY <= -5f) {
				BallY = -5f - BallVY * DT;
				BallVY = -BallVY * 1.05f;}} // probably a bit glitchy maybe? idk
		public void Render(Shader shader, bool bind) {
			_cube.DrawWithModels(shader, [
				Matrix4.CreateScale(new Vector3(HittingWidth*2,PaddleHeight,1f)) * Matrix4.CreateTranslation(Paddle1X,Paddle1Y,0) * Transformation,
				Matrix4.CreateScale(new Vector3(HittingWidth*2,PaddleHeight,1f)) * Matrix4.CreateTranslation(Paddle2X,Paddle2Y,0) * Transformation,
				Matrix4.CreateScale(new Vector3(BallRadius*2)) * Matrix4.CreateTranslation(BallX,BallY,0) * Transformation,
			], bind);
			_plane.DrawWithModel(shader, Matrix4.CreateScale(new Vector3(5.5f, 5.5f, 1f)) * Matrix4.CreateTranslation(0f, 0f, -1f) * Transformation, true);}
		public string ScoreText => "Score: " + P1Score + " - " + P2Score;
		public void UpdateTransformation(Matrix4 Transformation) {
			this.Transformation = Transformation;}
		public void UpdateTransformation() {DataStuff.CreateScaleRotXYZTrans(Scale, Rot, Pos, out Transformation);}
		public void UpdatePos(Vector3 pos) {
			Pos = pos;
			DataStuff.CreateScaleRotXYZTrans(Scale, Rot, Pos, out Transformation);}
		public void UpdateRot(Vector3 rot) {
			if (AngleMode) Rot = rot * ((float)Math.PI / 180f);
			else Rot = rot;
			DataStuff.CreateScaleRotXYZTrans(Scale, Rot, Pos, out Transformation);}
		public void UpdateScale(Vector3 scale) {
			Scale = scale;
			DataStuff.CreateScaleRotXYZTrans(Scale, Rot, Pos, out Transformation);}
		public void Loss(byte Player1Side) {}
		public override void OnLoad(Game game)
		{
			base.OnLoad(game);
			_cube = new ObjectMesh((2,0,0), Vector3.Zero, Vector3.One, DataStuff.CubeV, DataStuff.CubeI);
			_plane = new ObjectMesh(Vector3.Zero, Vector3.Zero, Vector3.One, DataStuff.PlaneV, DataStuff.PlaneI);
		}
		public override void OnRenderFrame(Game game, double dt) {
			base.OnRenderFrame(game, dt);
			Render(game._shader, true);
			// _textRenderer.RenderText(this, _textShader, _pongGame.ScoreText, Vector2i.Zero, new(0f, .45f), new(6f), new(0f, 1f, 0f), 10f, _clientSize, FontCharFillerThing.FontCharDeeta, false);
			game._textRenderer.RenderText(game, game._textShader, ScoreText, (0,0), (0f,.45f), (4f,4f), (0f,1f,0f), 10f, game._clientSize, FontCharFillerThing.FontCharDeeta, false);
			}
		public override void OnUpdateFrame(Game game, double dt)
		{
			base.OnUpdateFrame(game, dt);
			UpdateRot(new Vector3(270f, (float)game._gameTime * 15f, 0f));
			Update(game._tickSpeedInv);
		}
		public override void OnKeyDown(KeyboardKeyEventArgs e)
		{
			base.OnKeyDown(e);
			KeyInputQueue.Add(new GameKeyState1(e.Key, true));
		}
		public override void OnKeyUp(KeyboardKeyEventArgs e)
		{
			base.OnKeyUp(e);
			KeyInputQueue.Add(new GameKeyState1(e.Key, false));
		}
		
	}
	public enum MKNoteType {
		Normal,
		Hurt,
		Death,
		Bullet,
		Poison,}
	public struct ManiaKey {
		public ManiaKey(double time, float holdLength, MKNoteType type) {
			this.time = time;
			this.holdLength = holdLength;
			this.type = type; }
		public ManiaKey(double time, MKNoteType type) {
			this.time = time;
			this.type = type; }
		public ManiaKey(double time, float holdLength) {
			this.time = time;
			this.holdLength = holdLength; }
		public ManiaKey(double time) { this.time = time; }
		public ManiaKey() { }
		public double time;
		public float holdLength;
		public MKNoteType type; }
	[Flags]
	public enum ManiaMods {
		none = 0,
		speedChanged = 1,
		autoPlay = 2,
		hard = 4,
		easy = 8,
		suddenDeath = 16,
		perfect = 32}
	public enum ManiaModChartTypes {
		none,
		BPM,
		ScrollSpeed,
		Zoom,
		LaneSpacing,}
	public struct ManiaModChart {
		public double Time;
		public ManiaModChartTypes ModChartType;}
	public struct ManiaChart(ManiaKey[][] keyData) {
		public ManiaKey[][] KeyData = keyData;
		public ManiaModChart[] ModChart;
		public double[] ModChartDoubles;
		public float[] ModChartFloats;
		public int[] ModChartInts;
		public long[] ModChartLongs;
		public Vector2i[] ModChartV2is;}
	// public class RhythmGame {
	// 	public uint KeyCount;
	// 	public RhythmGame(uint keys) {
	// 		KeyCount = keys;}
	// 	public RhythmGame() {
	// 		KeyCount = 4;}
	// 	private uint[] currentKey;
	// 	private uint[] lastDisplayedKey;
	// 	public double time;
	// 	public double dt;
	// 	public float scrollSpeed = 1; // how many seconds does it take for a note to go from the top to the bottom.
	// 	private RhythmGameKChart chart;
	// 	public void LoadMap(RhythmGameKChart chart) {
	// 		currentKey = new uint[KeyCount];
	// 		lastDisplayedKey = new uint[KeyCount];
	// 		this.chart = chart;}
	// 	public GameKeyState1[] KeyInputQueue = [];
	// 	/// <summary>
	// 	/// updates with the specified amount of time to go to.
	// 	/// </summary>
	// 	/// <param name="t">the amount of time to set it to.</param>
	// 	public void UpdateWT(double t) {
	// 		dt = t - time;
	// 		time = t;
	// 		Upd();}
	// 	/// <summary>
	// 	/// updates with the specified delta time.
	// 	/// </summary>
	// 	/// <param name="DT">the delta time; the amount of time that has passed since the last update.</param>
	// 	public void UpdateWDT(double DT) {
	// 		dt = DT;
	// 		time += dt;
	// 		Upd();}
	// 	/// <summary>
	// 	/// Updates the game.
	// 	/// This does not set the time or anything. If you want to do that, you can use UpdateWT, UpdateWDT, or you can set the time yourself.
	// 	/// </summary>
	// 	public void Upd() {
	// 		switch (dt) {
	// 			case < 0:
	// 				break;
	// 			case 0:
	// 				break;
	// 			case > 0:


	// 				break;
	// 			default:
	// 				break;}}}
	public class ManiaRG : IMinigame {
		public static readonly ManiaKey[][][] BuiltInCharts = [
			[
				[

				],[

				],[

				],[

				],
			],

		];
		public const string GameIdentifier = "mania";
		public static Dictionary<string, Action<Game>> InGameConstructorthings = new() {
			["mania"] = delegate (Game game) {
				game._currentMinigames.Add(new ManiaRG(game._textRenderer));
				game._gameModes.Add(GameIdentifier);}
		};
		public static void StartInit()
		{
			DataStuff.noInputChatCommands["maniashowalldata"] = delegate (Game game) { DisplayFullInfo = !DisplayFullInfo; };
			DataStuff.chatCommands["manialoadmap "] = delegate (Game game, string path) {
				List<ManiaRG> mrgl = [];
				foreach (IMinigame minigame in game._currentMinigames) if (minigame is ManiaRG m1) mrgl.Add(m1);
				if (mrgl.Count > 0) {
					if (File.Exists(path))
						foreach (ManiaRG _m in mrgl)
							if (_m.TryLoadMapFromString(File.ReadAllText(path))) Console.WriteLine("Loadmap success!"); else Console.WriteLine("Failed to load map.");
					else Console.WriteLine("Path does not exist!"); } };
		}
		// public static void StartInit() {
		// 	GameIdentifier = "mania";
		// 	InGameConstructorthings = new() {
		// 		["mania"] = delegate (Game game) {
		// 			game._currentMinigames.Add(new ManiaRG(game._textRenderer));
		// 			game._gameModes.Add("mania");}
		// 	};InGameConstructorthings["fnf"] = InGameConstructorthings["mania"];
		// }
		public Keys[] keybinds;
		public uint KeyCount;
		public Text gameRenderer;
		public bool Playing = true;
		public long timeAtPause = 0;
		private string lingeringTxt = "No note hit yet";
		public long timeOffset = 0;
		private uint[] currentKeys;
		// private uint lastDisplayedKey = 0; // might be used to cull the notes at some point.
		public double time = 0;
		// public double dt = 0;
		public long score = 0;
		public int combo = 0;
		public float scrollSpeed = 1.0f; // what fraction of a second does it take for a note to go from the top to the bottom.
		public static bool DisplayFullInfo = false;
		public uint[] noteHitAccAmts = new uint[Enum.GetValues<ManiaAcc>().Length]; // an array containing how many times the player has hit each of the accuracies including misses.
		private ManiaChart chart;
		private float jBasePosX;
		private float jBasePosY;
		private Vector2[] jPosSOffs;
		private uint training = 0;
		public ManiaRG(Text renderer, uint keyAmount = 4, Keys[] binds = null, float JBasePosX = float.NaN, float JBasePosY = float.NaN, Vector2[] JPosSOffs = null) {
			keybinds = binds ?? [Keys.E, Keys.R, Keys.U, Keys.I];
			KeyCount = keyAmount;
			gameRenderer = renderer;
			currentKeys = new uint[KeyCount];
			jBasePosX = float.IsNaN(JBasePosX) ? 0 : JBasePosX;
			jBasePosY = float.IsNaN(JBasePosY) ? -0.8f : JBasePosY;
			if (JPosSOffs == null || JPosSOffs.Length != KeyCount) {
				jPosSOffs = new Vector2[KeyCount];
				for (uint i = KeyCount; i-- > 0;) {
					jPosSOffs[i] = new((float)i / KeyCount * 0.5f - 0.25f, 0); } }
			else jPosSOffs = JPosSOffs;
			// Console.WriteLine("mania rhythm game thing " + keybinds + "," + keyCount + "," + renderer + "," + currentKeys.Length + "," + jBasePosX + "," + jBasePosY+","+jPosSOffs+","+jPosSOffs == null ? -1 : jPosSOffs.Length+","+JPosSOffs+","+JPosSOffs == null ? -1 : JPosSOffs.Length);
			Console.WriteLine("mania rhythm game thing");
			Console.WriteLine(keybinds+","+KeyCount);
			Console.WriteLine(renderer+","+currentKeys.Length);
			Console.WriteLine(jBasePosX+","+jBasePosY);
			Console.WriteLine(jPosSOffs);
			Console.WriteLine(jPosSOffs==null?-1:jPosSOffs.Length);
			if (jPosSOffs != null) {
				Console.WriteLine("jPosSOffs is not null.");
				for (int i = 0; i < jPosSOffs.Length; i++) {
					(float jx, float jy) = (jPosSOffs[i].X, jPosSOffs[i].Y);
					Console.WriteLine("lane " + i + " thing: " + jx + "," + jy); } }
			Console.WriteLine(JPosSOffs);
			Console.WriteLine(JPosSOffs==null?-1:JPosSOffs.Length); }
		public void LoadMap(ManiaChart chart) {
			currentKeys = new uint[KeyCount];
			// lastDisplayedKey = 0;
			this.chart = chart; }
		public void StartMap() {
			if (!Playing) {
				Playing = true;
				// time that has passed is the current time minus the time at pause
				timeOffset += Stopwatch.GetTimestamp() - timeAtPause; } }
		public void RestartMap() {
			if (training == 0) {
				timeOffset = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 3;
				currentKeys = new uint[KeyCount];
				/* lastDisplayedKey = 0; */}
			else {
				timeOffset = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 3;
				currentKeys = new uint[KeyCount];
				// lastDisplayedKey = 0;
				RestartTraining(); } }
		public void TogglePauseMap() {
			if (Playing) {
				Playing = false; timeAtPause = Stopwatch.GetTimestamp(); }
			else {
				Playing = true;
				// time that has passed is the current time minus the time at pause
				timeOffset += Stopwatch.GetTimestamp() - timeAtPause; } }
		public void PauseMap() {
			if (Playing) {
				Playing = false;
				timeAtPause = Stopwatch.GetTimestamp(); } }
		public float[][] CalcVertices(Vector2 posScale, Vector2 noteScale, GlyphData noteGlyphData, Vector2i windowSize, Vector2i? posOffset = null) {
			float ftexW = gameRenderer.TextTexture.Width;
			float ftexH = gameRenderer.TextTexture.Height;
			float halfOfCeilTrueSizeX = MathF.Ceiling(noteGlyphData.sizeX * noteScale.X) * 0.5f;
			float halfOfCeilTrueSizeY = MathF.Ceiling(noteGlyphData.sizeY * noteScale.Y) * 0.5f;
			// int WinSX = windowSize.X; int WinSY = windowSize.Y;
			(int WinSX, int WinSY) = (windowSize.X, windowSize.Y);
			float oX = posOffset?.X??0 + posScale.X * WinSX; // offset x
			float oY = posOffset?.Y??0 + posScale.Y * WinSY; // offset y
			List<float[]> vertices = []; int I = 0; // I is vertex index
			float[] v = new float[Text.BulkDrawFloats];
			// Vector2i texStart = noteGlyphData.textureStart;
			float tSX = noteGlyphData.tStartX, tSY = noteGlyphData.tStartY; // no need to divide each time anymore lol
			// float tSX = texStart.X / ftexW; // texture start x
			// float tSY = texStart.Y / ftexH; // texture start y
			float tNX = noteGlyphData.tEndX; // texture end x
			float tNY = noteGlyphData.tEndY; // texture end y
			float baseSX = (MathF.Floor(oX - halfOfCeilTrueSizeX) + .5f) / WinSX + jBasePosX; // the base value for the start X
			float baseSY = (MathF.Floor(oY - halfOfCeilTrueSizeY) + .5f) / WinSY + jBasePosY; // the base value for the start Y
			float baseNX = (MathF.Floor(oX + halfOfCeilTrueSizeX) + .5f) / WinSX + jBasePosX; // the base value for the end X
			float baseNY = (MathF.Floor(oY + halfOfCeilTrueSizeY) + .5f) / WinSY + jBasePosY; // the base value for the end Y

			// float baseMY = (MathF.Floor(oY) + .5f) / WinSY; // the base value for the middle value of Y. commented bc not used rn. also one line below bc this gets auto-indented to the comment above it for some dumb reason idk why it even does that ._.

			// when the time is exactly the note's time, it should be exactly at the judgement offset.
			// when the time is the note's time minus the reciprocral of the scroll speed, it will be above the judgement line by exactly the height of the screen.
			// Anywhere inbetween will be inbetween.
			for (int lane = (int)KeyCount; lane-- > 0;) {
				ManiaKey[] laneKeyData = chart.KeyData[lane];
				(float jLaneOffsX, float jLaneOffsY) = (jPosSOffs[lane].X, jPosSOffs[lane].Y);
				float laneBSX = baseSX + jLaneOffsX; // the base value for the start X
				float laneBSY = baseSY + jLaneOffsY; // the base value for the start Y
				float laneBNX = baseNX + jLaneOffsX; // the base value for the end X
				float laneBNY = baseNY + jLaneOffsY; // the base value for the end Y
				for (int i = laneKeyData.Length - 1; i-- > currentKeys[lane];) {
					ManiaKey noteData = laneKeyData[i];
					double t = noteData.time; // the time that the note appears at
					float noteOffset = (float)(t - time) * scrollSpeed * 2;
					float sY = laneBSY + noteOffset;
					float nY = laneBNY + noteOffset;
					v[I]=laneBSX;v[I+1]=sY;v[I+2]=tSX;v[I+3]=tSY;v[I+4]=laneBSX;v[I+5]=nY;v[I+6]=tSX;v[I+7]=tNY;v[I+8]=laneBNX;v[I+9]=nY;v[I+10]=tNX;v[I+11]=tNY;v[I+12]=laneBNX;v[I+13]=sY;v[I+14]=tNX;v[I+15]=tSY;
					if (I == Text.BulkDrawFloats - 16) {
						vertices.Add(v);
						v = new float[Text.BulkDrawFloats]; // this is VERY important! adding to the list just adds a pointer i think, and it doesn't actually clone it, so you need to create it again or else it will be filled with the same array over and over again.
						I = 0; }
					else I += 16; }
				float halfOfCeilTrueJudgementSizeX = MathF.Ceiling(noteGlyphData.sizeX * noteScale.X) * 0.55f; // scaled wider slightly
				float halfOfCeilTrueJudgementSizeY = MathF.Ceiling(noteGlyphData.sizeY * noteScale.Y) * 0.5f;
				float jGeneralOffsetX = jBasePosX + jLaneOffsX;
				float jGeneralOffsetY = jBasePosY + jLaneOffsY;
				float SX = (MathF.Floor(oX - halfOfCeilTrueJudgementSizeX) + .5f) / WinSX + jGeneralOffsetX;
				float SY = (MathF.Floor(oY - halfOfCeilTrueJudgementSizeY) + .5f) / WinSY + jGeneralOffsetY;
				float NX = (MathF.Floor(oX + halfOfCeilTrueJudgementSizeX) + .5f) / WinSX + jGeneralOffsetX;
				float NY = (MathF.Floor(oY + halfOfCeilTrueJudgementSizeY) + .5f) / WinSY + jGeneralOffsetY;
				for (uint i = 0; i < 2; i++) {
					v[I]=SX;v[I+1]=SY;v[I+2]=tSX;v[I+3]=tSY;v[I+4]=SX;v[I+5]=NY;v[I+6]=tSX;v[I+7]=tNY;v[I+8]=NX;v[I+9]=NY;v[I+10]=tNX;v[I+11]=tNY;v[I+12]=NX;v[I+13]=SY;v[I+14]=tNX;v[I+15]=tSY;
					if (I == Text.BulkDrawFloats-16) {
						vertices.Add(v);
						v = new float[Text.BulkDrawFloats];
						I = 0;} else I += 16;}
				// v[I] = SX; v[I + 1] = SY; v[I + 2] = tSX; v[I + 3] = tSY; v[I + 4] = SX; v[I + 5] = NY; v[I + 6] = tSX; v[I + 7] = tNY; v[I + 8] = NX; v[I + 9] = NY; v[I + 10] = tNX; v[I + 11] = tNY; v[I + 12] = NX; v[I + 13] = SY; v[I + 14] = tNX; v[I + 15] = tSY;
				// if (I == Text.BulkDrawFloats - 16) {
				// 	vertices.Add(v);
				// 	v = new float[Text.BulkDrawFloats];
				// 	I = 0;
				// }
				
				
				// else
				// {
				// 	float[] vt = new float[I];
				// 	Array.Copy(v, vt, I);
				/* 	vertices.Add(vt); }*/}
			if (I != 0) {
				float[] vt = new float[I];
				Array.Copy(v, vt, I);
				vertices.Add(vt); }
			float[][] V = new float[vertices.Count][];
			for (int i = vertices.Count; i-- > 0;) { V[i] = vertices[i]; }
			return V; }
		public void Update() {
			if (!Playing) {
				time = (timeAtPause - timeOffset) / (double)Stopwatch.Frequency; }
			for (int i = 0; i < KeyCount; i++) {
				ManiaKey[] laneData = chart.KeyData[i];
				uint currentKey = currentKeys[i];
				uint currentKeyNow = currentKey;
				while (currentKey < laneData.Length && laneData[currentKey].time - time <= -0.3) { currentKey++; } // calculates miss amounts
				currentKeys[i] = currentKey;
				uint missAmount = currentKey - currentKeyNow;
				if (missAmount != 0) { Console.WriteLine("Missed " + missAmount + " notes this frame."); noteHitAccAmts[(int)ManiaAcc.miss] += missAmount; } }
			if (training == 1) {
				uint currentKey;
				for (int i = 0; i < KeyCount; i++) {
					if ((currentKey = currentKeys[i]) > 64) {
						ManiaKey[] laneData = chart.KeyData[i];
						Console.Write("rf" + i);
						// current key is greater than 64; at least 64 notes were hit or missed or smth
						// so anyways generate more notes
						ManiaKey[] newLaneData = new ManiaKey[4096];
						Array.Copy(laneData, currentKey, newLaneData, 0, 4096 - currentKey);
						double lastNoteTime = laneData[4095].time;
						uint startingIndex = 4096 - currentKey;
						for (uint j = 0; j < currentKey;) {
							newLaneData[j + startingIndex] = new ManiaKey(lastNoteTime + 1.8 / Math.Sqrt(++j + 80)); }
						chart.KeyData[i] = newLaneData;
						currentKeys[i] = 0; } } } }
		public void Render(Game game, bool sdfjskls = false, Vector3? color = null) {
			GlyphData noteGlyphData = FontCharFillerThing.FontCharDeeta.SChars["note"];
			float[][] data = CalcVertices(new((float)Math.Sin(time) * .4f, (float)(Math.Cos(time) * .05f)), new(12), noteGlyphData, game._clientSize);
			if (data.Length > 0) { gameRenderer.RenderWithPrecalculatedVertices(data, game._textShader, color ?? new(1));}
			gameRenderer.RenderText(game, game._textShader, "Map time: " + time + "\n" + lingeringTxt, new(0), new(-.5f, 0.3f), new(8), color ?? new(1, 0, 1), 10f, game._clientSize, FontCharFillerThing.FontCharDeeta, false);
			if (sdfjskls) {
				Console.WriteLine("data thing: " + data);
				Console.WriteLine("data length: " + data.Length);
				foreach (float[] d in data) { Console.WriteLine("data: " + d+","+d.Length); } }
			if (DisplayFullInfo) {
				string FullInfoText = "Ratings:";
				foreach (ManiaAcc s in Enum.GetValues<ManiaAcc>()) FullInfoText += "\n" + Enum.GetName(s) + ": " + noteHitAccAmts[(int)s];
				gameRenderer.RenderText(game, game._textShader, FullInfoText, new(0), new(0.5f, 0.3f), new(2), color ?? new(1, 0, 1), 10f, game._clientSize, FontCharFillerThing.FontCharDeeta, false); } }
		public void Render2(Shader shader, Game game, Vector3? _color = null) {
			if (!Playing) {
				time = (timeAtPause - timeOffset) / (double)Stopwatch.Frequency; }
			GlyphData noteGlyphData = FontCharFillerThing.FontCharDeeta.SChars["note"];
			Vector3 color = _color ?? new(1);
			shader.Use();
			shader.SetVector3("textColor", color);
			GL.BindVertexArray(gameRenderer.VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, gameRenderer.VBO);
			(float noteScaleX, float noteScaleY) = (12, 12);
			// { // drawing routine
			// 	GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * data[i].Length, data[i]);
			// 	GL.DrawElements(PrimitiveType.Triangles, data[i].Length * 3 / 8, DrawElementsType.UnsignedInt, 0); }
			Vector2i windowSize = game._clientSize;
			(float posScaleX, float posScaleY) = ((float)Math.Sin(time) * .4f, (float)(Math.Cos(time) * .05f));

			float ftexW = gameRenderer.TextTexture.Width;
			float ftexH = gameRenderer.TextTexture.Height;
			float halfOfCeilTrueSizeX = MathF.Ceiling(noteGlyphData.sizeX * noteScaleX) * 0.5f;
			float halfOfCeilTrueSizeY = MathF.Ceiling(noteGlyphData.sizeY * noteScaleY) * 0.5f;
			(int WinSX, int WinSY) = windowSize;
			float oX = posScaleX * WinSX; // offset x
			float oY = posScaleY * WinSY; // offset y
			int I = 0; // I is vertex index
			float[] v = new float[Text.BulkDrawFloats];
			float tSX = noteGlyphData.tStartX; // texture start x
			float tSY = noteGlyphData.tStartY; // texture start y
			float tNX = noteGlyphData.tEndX; // texture end x
			float tNY = noteGlyphData.tEndY; // texture end y
			float baseSX = (MathF.Floor(oX - halfOfCeilTrueSizeX) + .5f) / WinSX + jBasePosX; // the base value for the start X
			float baseSY = (MathF.Floor(oY - halfOfCeilTrueSizeY) + .5f) / WinSY + jBasePosY; // the base value for the start Y
			float baseNX = (MathF.Floor(oX + halfOfCeilTrueSizeX) + .5f) / WinSX + jBasePosX; // the base value for the end X
			float baseNY = (MathF.Floor(oY + halfOfCeilTrueSizeY) + .5f) / WinSY + jBasePosY; // the base value for the end Y

			// float baseMY = (MathF.Floor(oY) + .5f) / WinSY; // the base value for the middle value of Y. commented bc not used rn. also one line below bc this gets auto-indented to the comment above it for some dumb reason idk why it even does that ._.

			// when the time is exactly the note's time, it should be exactly at the judgement offset.
			// when the time is the note's time minus the reciprocral of the scroll speed, it will be above the judgement line by exactly the height of the screen.
			// Anywhere inbetween will be inbetween.
			for (int lane = (int)KeyCount; lane-- > 0;) {
				ManiaKey[] laneKeyData = chart.KeyData[lane];
				(float jLaneOffsX, float jLaneOffsY) = (jPosSOffs[lane].X, jPosSOffs[lane].Y);
				float laneBSX = baseSX + jLaneOffsX; // the base value for the start X
				float laneBSY = baseSY + jLaneOffsY; // the base value for the start Y
				float laneBNX = baseNX + jLaneOffsX; // the base value for the end X
				float laneBNY = baseNY + jLaneOffsY; // the base value for the end Y
				for (int i = laneKeyData.Length - 1; i-- > currentKeys[lane];) {
					ManiaKey noteData = laneKeyData[i];
					double t = noteData.time; // the time that the note appears at
					float noteOffset = (float)(t - time) * scrollSpeed * 2;
					float sY = laneBSY + noteOffset;
					float nY = laneBNY + noteOffset;
					v[I]=laneBSX;v[I+1]=sY;v[I+2]=tSX;v[I+3]=tSY;v[I+4]=laneBSX;v[I+5]=nY;v[I+6]=tSX;v[I+7]=tNY;v[I+8]=laneBNX;v[I+9]=nY;v[I+10]=tNX;v[I+11]=tNY;v[I+12]=laneBNX;v[I+13]=sY;v[I+14]=tNX;v[I+15]=tSY;
					if (I == Text.BulkDrawFloats - 16) {
						GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * Text.BulkDrawFloats, v);
						GL.DrawElements(PrimitiveType.Triangles, Text.MTILen, DrawElementsType.UnsignedInt, 0);
						// render notes!
						I = 0; }
					else I += 16; }
				float halfOfCeilTrueJudgementSizeX = MathF.Ceiling(noteGlyphData.sizeX * noteScaleX) * 0.55f; // scaled wider slightly
				float halfOfCeilTrueJudgementSizeY = MathF.Ceiling(noteGlyphData.sizeY * noteScaleY) * 0.5f;
				float jGeneralOffsetX = jBasePosX + jLaneOffsX;
				float jGeneralOffsetY = jBasePosY + jLaneOffsY;
				float SX = (MathF.Floor(oX - halfOfCeilTrueJudgementSizeX) + .5f) / WinSX + jGeneralOffsetX;
				float SY = (MathF.Floor(oY - halfOfCeilTrueJudgementSizeY) + .5f) / WinSY + jGeneralOffsetY;
				float NX = (MathF.Floor(oX + halfOfCeilTrueJudgementSizeX) + .5f) / WinSX + jGeneralOffsetX;
				float NY = (MathF.Floor(oY + halfOfCeilTrueJudgementSizeY) + .5f) / WinSY + jGeneralOffsetY;
				// for (uint i = 0; i < 2; i++) {
				// 	v[I]=SX;v[I+1]=SY;v[I+2]=tSX;v[I+3]=tSY;v[I+4]=SX;v[I+5]=NY;v[I+6]=tSX;v[I+7]=tNY;v[I+8]=NX;v[I+9]=NY;v[I+10]=tNX;v[I+11]=tNY;v[I+12]=NX;v[I+13]=SY;v[I+14]=tNX;v[I+15]=tSY;
				// 	if (I == Text.BulkDrawFloats-16) {
				// 		GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * Text.BulkDrawFloats, v);
				// 		GL.DrawElements(PrimitiveType.Triangles, Text.MTILen, DrawElementsType.UnsignedInt, 0);
				// 		I = 0;} else I += 16;}
				v[I] = SX; v[I + 1] = SY; v[I + 2] = tSX; v[I + 3] = tSY; v[I + 4] = SX; v[I + 5] = NY; v[I + 6] = tSX; v[I + 7] = tNY; v[I + 8] = NX; v[I + 9] = NY; v[I + 10] = tNX; v[I + 11] = tNY; v[I + 12] = NX; v[I + 13] = SY; v[I + 14] = tNX; v[I + 15] = tSY;
				if (I == Text.BulkDrawFloats - 16) {
					GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * Text.BulkDrawFloats, v);
					GL.DrawElements(PrimitiveType.Triangles, Text.MTILen, DrawElementsType.UnsignedInt, 0);
					I = 0; } else I += 16; }
			if (I != 0) {
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * I, v);
				GL.DrawElements(PrimitiveType.Triangles, I * 3 / 8, DrawElementsType.UnsignedInt, 0); }

			gameRenderer.RenderText(game, game._textShader, "Map time: " + time + "\n" + lingeringTxt, new(0), new(-.5f, 0.3f), new(8), new(1, 0, 1), 10f, game._clientSize, FontCharFillerThing.FontCharDeeta, false);
			if (DisplayFullInfo) {
				string FullInfoText = "Ratings:";
				foreach (ManiaAcc s in Enum.GetValues<ManiaAcc>()) FullInfoText += "\n" + Enum.GetName(s) + ": " + noteHitAccAmts[(int)s];
				gameRenderer.RenderText(game, game._textShader, FullInfoText, new(0), new(0.5f, 0.3f), new(2), new(1, 0, 1), 10f, game._clientSize, FontCharFillerThing.FontCharDeeta, false); } }
		/// <summary>
		/// when a key is pressed down but you don't really care enough to give the time that it happened for some reason.
		/// </summary>
		/// <param name="key">the key that was pressed.</param>
		public void KeyDown(Keys key) => KeyDown(key, Stopwatch.GetElapsedTime(timeOffset).TotalSeconds);
		/// <summary>
		/// when a key is pressed down.
		/// </summary>
		/// <param name="key">the key that was pressed.</param>
		/// <param name="time">yeah. this is just the timestamp of whatever time this is divided by frequency.</param>
		public void KeyDown(Keys key, double time) {
			int laneNumberThing = keybinds[0] == key ? 0 : -1;
			for (uint m = KeyCount; m-- > 1;) if (keybinds[m] == key) { laneNumberThing = (int)m; break; }
			if (laneNumberThing == -1) { // do this only if it is the keybind.
				Console.Write("not a keybind");
				switch (key) {
					case Keys.P: TogglePauseMap(); break;
					case Keys.GraveAccent: RestartMap(); break;
					default: break;} } else if (Playing) {
				Console.Write("lane: " + laneNumberThing + " ");
				// key is one of the keybinds.
				uint currentKey = currentKeys[laneNumberThing];
				ManiaKey[] laneData = chart.KeyData[laneNumberThing];
				uint currentKeyNow = currentKey;
				while (currentKey < laneData.Length && laneData[currentKey].time - time <= -0.3) { currentKey++; }
				uint missAmount = currentKey - currentKeyNow;
				if (missAmount != 0) { Console.WriteLine("missed " + missAmount + " notes this frame."); noteHitAccAmts[(int)ManiaAcc.miss] += missAmount; }
				if (currentKey >= laneData.Length) { Console.WriteLine("There are no more notes on lane " + laneNumberThing); return; }
				ManiaKey CurrentKey = laneData[currentKey];
				double keyTime = CurrentKey.time;
				double timeDiff = keyTime - time; // the amount of time before the note would be perfectly at the place.
				string DiffMSStr = timeDiff*1000+"MS"; // the amount of time before the note would be perfectly at the place.
				bool Hit = true;
				switch (timeDiff) { // big beautiful wall of text
					case > 0.5: Hit = false; noteHitAccAmts[(int)ManiaAcc.nothing]++;/* lingeringText="No note was hit. " + timeDiff * 1000 + "MS";*/ Console.Write("No note was hit."); break;
					case > 0.3: noteHitAccAmts[(int)ManiaAcc.early]++; lingeringTxt = "TOO early... " + DiffMSStr; Console.Write("Too early."); break;
					case 0: noteHitAccAmts[(int)ManiaAcc.PERFECT]++; lingeringTxt = "PERFECT " + DiffMSStr; Console.Write("PERFECT TIMING?? :O;"); break;
					case > -.0000005 and < .0000005: noteHitAccAmts[(int)ManiaAcc.pHMCS]++; lingeringTxt = "HALF MICROSECOND PERFECT " + DiffMSStr; Console.Write("+-0.5μs TIMING??????;"); break;
					case > -.000001 and < .000001: noteHitAccAmts[(int)ManiaAcc.pMCS]++; lingeringTxt = "MICROSECOND PERFECT " + DiffMSStr; Console.Write("+-1μs TIMING??????;"); break;
					case > -.000002 and < .000002: noteHitAccAmts[(int)ManiaAcc.p2mcs]++; lingeringTxt = "2MCS PERFECT " + DiffMSStr; Console.Write("+-2μs???;"); break;
					case > -.000003 and < .000003: noteHitAccAmts[(int)ManiaAcc.p3mcs]++; lingeringTxt = "3MCS PERFECT " + DiffMSStr; Console.Write("+-3μs???;"); break;
					case > -.000005 and < .000005: noteHitAccAmts[(int)ManiaAcc.p5mcs]++; lingeringTxt = "5MCS PERFECT " + DiffMSStr; Console.Write("+-5μs???;"); break;
					case > -.00001 and < .00001: noteHitAccAmts[(int)ManiaAcc.p10mcs]++; lingeringTxt = "10MCS PERFECT " + DiffMSStr; Console.Write("+-10μs???;"); break;
					case > -.000025 and < .000025: noteHitAccAmts[(int)ManiaAcc.p25mcs]++; lingeringTxt = "25MCS PERFECT " + DiffMSStr; Console.Write("+-25μs???;"); break;
					case > -.00005 and < .00005: noteHitAccAmts[(int)ManiaAcc.p50mcs]++; lingeringTxt = ".05MS Perfect! " + DiffMSStr; Console.Write("+-50μs???;"); break;
					case > -.0001 and < .0001: noteHitAccAmts[(int)ManiaAcc.p100mcs]++; lingeringTxt = ".1MS Perfect! " + DiffMSStr; Console.Write("+-100μs!!;"); break;
					case > -.00025 and < .00025: noteHitAccAmts[(int)ManiaAcc.qmsPerfect]++; lingeringTxt = ".25ms perfect! " + DiffMSStr; Console.Write("Hit within +-0.25ms!!;"); break;
					case > -.0005 and < .0005: noteHitAccAmts[(int)ManiaAcc.hmsPerfect]++; lingeringTxt = ".5ms perfect! " + DiffMSStr; Console.Write("Hit within +-0.5ms!!;"); break;
					case > -.001 and < .001: noteHitAccAmts[(int)ManiaAcc.msPerfect]++; lingeringTxt = "Millisecond perfect! " + DiffMSStr; Console.Write("Hit within +-1ms!!;"); break;
					case > -1d / 480d and < 1d / 480d: noteHitAccAmts[(int)ManiaAcc.fp240]++; lingeringTxt = "Frame perfect at 240fps! " + DiffMSStr; Console.Write("Frame perfect at 240fps!;"); break;
					case > -1d / 240d and < 1d / 240d: noteHitAccAmts[(int)ManiaAcc.fp120]++; lingeringTxt = "Frame perfect at 120fps! " + DiffMSStr; Console.Write("Frame perfect at 120fps!;"); break;
					case > -1d / 120d and < 1d / 120d: noteHitAccAmts[(int)ManiaAcc.fp60]++; lingeringTxt = "Frame perfect at 60fps! " + DiffMSStr; Console.Write("Frame perfect at 60fps!;"); break;
					case > -.01 and < .01: noteHitAccAmts[(int)ManiaAcc.excellent]++; lingeringTxt = "Excellent! " + DiffMSStr; Console.Write("Excellent +-10ms!;"); break;
					case > -1d / 60d and < 1d / 60d: noteHitAccAmts[(int)ManiaAcc.fp30]++; lingeringTxt = "Frame perfect at 30fps! " + DiffMSStr; Console.Write("Frame perfect at 30fps! (+-1/60);"); break;
					case > -.03 and < .03: noteHitAccAmts[(int)ManiaAcc.sick]++; lingeringTxt = "Sick! " + DiffMSStr; Console.Write("Sick! (+-30ms);"); break;
					case > -.05 and < .05: noteHitAccAmts[(int)ManiaAcc.great]++; lingeringTxt = "Great! " + DiffMSStr; Console.Write("Great (+-50ms);"); break;
					case > -.08 and < .08: noteHitAccAmts[(int)ManiaAcc.good]++; lingeringTxt = "Good. " + DiffMSStr; Console.Write("Good. (+-80ms);"); break;
					case > -.11 and < .11: noteHitAccAmts[(int)ManiaAcc.okay]++; lingeringTxt = "Okay. " + DiffMSStr; Console.Write("Okay. (+-110ms);"); break;
					case > -.15 and < .15: noteHitAccAmts[(int)ManiaAcc.bad]++; lingeringTxt = "Bad. " + DiffMSStr; Console.Write("Bad. (+-150ms);"); break;
					case > -.3 and < .3: noteHitAccAmts[(int)ManiaAcc.yikes]++; lingeringTxt = "Yikes. " + DiffMSStr; Console.Write("Yikes. (+-300ms);"); break;
					default: Hit = false; break;}
				Console.WriteLine(" TimeDiff: " + timeDiff);
				if (Hit) currentKeys[laneNumberThing]++; } }
		public bool TryLoadMapFromString(string chart) {
			string[] processedS1 = chart.Split('\n');
			if (processedS1.Length < 2) return false; // must have at least 2 lines; first is version number
			Console.WriteLine("Version number: " + processedS1[0]);
			ManiaKey[][] KeyData = new ManiaKey[KeyCount][];
			for (int j = 1; j < Math.Min(processedS1.Length, KeyCount); j++) {
				string[] processedS2 = processedS1[j].Split(',');
				KeyData[j] = new ManiaKey[processedS2.Length];
				ManiaKey[] keyData1 = KeyData[j];
				for (int i = 0; i < processedS2.Length; i++) {
					string[] stringThingy = processedS2[i].Split(' ');
					switch (stringThingy.Length) {
						case 1: // not a hold note
							keyData1[i] = new ManiaKey(Convert.ToDouble(stringThingy[0])); break;
						case 2: // hold note
							keyData1[i] = new ManiaKey(Convert.ToDouble(stringThingy[0]), Convert.ToSingle(stringThingy[1])); break;
						default: return false; } } }
			ManiaChart NewChart = new(KeyData);
			LoadMap(NewChart);
			return true; }
		public void StartTraining(uint difficulty = 1) {
			Console.WriteLine("starting training dksflsfd");
			ManiaKey[][] keyData = new ManiaKey[KeyCount][];
			for (int i = 0; i < KeyCount; i++) {
				double iOverkC = (double)i / KeyCount; // i divided by key count; i over key count.
				keyData[i] = new ManiaKey[4096];
				for (int j = 0; j < 4096; j++) { keyData[i][j] = new ManiaKey(0.2d * (j + iOverkC)); } }
			chart = new ManiaChart(keyData);
			training = difficulty;
			Console.WriteLine(training);
			Console.WriteLine("keydata length: " + keyData.Length);
			for (int i = 0; i < keyData.Length; i++) {
				Console.WriteLine("lane " + i + " length: " + keyData[i].Length); } }
		public void RestartTraining() {
			ManiaKey[][] keyData = new ManiaKey[KeyCount][];
			for (int i = 0; i < KeyCount; i++) {
				double iOverkC = (double)i / KeyCount; // i divided by key count; i over key count.
				ManiaKey[] laneKeyData = new ManiaKey[4096];
				for (int j = 0; j < 4096; j++) { laneKeyData[j] = new ManiaKey(0.2d*(j+iOverkC)); }
				keyData[i] = laneKeyData; }
			chart = new ManiaChart(keyData); }
		public override void OnLoad(Game game) {
			base.OnLoad(game);
			StartTraining();
			RestartMap(); }
		public override void OnRenderFrame(Game game, double dt) {
			base.OnRenderFrame(game, dt);
			
			time = Stopwatch.GetElapsedTime(timeOffset).TotalSeconds;
			Render2(game._textShader, game, DataStuff.HSVToRGB((float)(Stopwatch.GetElapsedTime(game.gameStartTimestamp).TotalSeconds*.36%1), (float)(Math.Sin(Stopwatch.GetElapsedTime(game.gameStartTimestamp).TotalSeconds*.2)*.125+.875), (float)(Math.Cos(Stopwatch.GetElapsedTime(game.gameStartTimestamp).TotalSeconds*.09)*.125+.875))); }
		public override void OnUpdateFrame(Game game, double dt)
		{
			base.OnUpdateFrame(game, dt);
			
			// Console.WriteLine(_dT);
			time = Stopwatch.GetElapsedTime(timeOffset).TotalSeconds;
			Update();
		}
		public override void OnKeyDown(KeyboardKeyEventArgs e)
		{
			base.OnKeyDown(e);
			KeyDown(e.Key,Stopwatch.GetElapsedTime(timeOffset).TotalSeconds);
		}
		public override void OnKeyUp(KeyboardKeyEventArgs e)
		{
			base.OnKeyUp(e);
		}
	}
	public enum V1KNoteType {
		Normal,
		Hurt,
		Death,
		Bullet,
		Poison,}
	public struct V1KKey(double Time, float HoldLength = 0, V1KNoteType Type = V1KNoteType.Normal)
	{
		public double time = Time;
		public float holdLength = HoldLength;
		public V1KNoteType type = Type;
	}
	[Flags]
	public enum V1KMods {
		none = 0,
		speedChanged = 1,
		autoPlay = 2,
		hard = 4,
		easy = 8,
		suddenDeath = 16,
		perfect = 32}
	public enum V1KModChartTypes {
		none,
		BPM,
		ScrollSpeed,
		Zoom,}
	public struct V1KKModChart {
		public double Time;
		public V1KModChartTypes ModChartType;}
	public struct V1KChart(V1KKey[] KeyData) {
		public V1KKey[] KeyData = KeyData;
		public V1KKModChart[] ModChart;
		public double[] ModChartDoubles;
		public float[] ModChartFloats;
		public int[] ModChartInts;
		public long[] ModChartLongs;
		public Vector2i[] ModChartV2is;}
	public enum ManiaAcc {
		miss,nothing,/*nonotepressed*/early,/*300-500msearly;formerly tooEarly*/
		yikes,/*+-300ms*/bad,/*+-150ms*/okay,/*+-110ms*/good,/*+-80ms*/great,/*+-50ms*/sick,
		/*+-30ms*/
		fp30,/*+-1/60s;FramePerfect@30FPS.*/excellent,/*10ms*/fp60,/*1/120,~8ms*/fp120,/*1/240,~4ms*/fp240,/*1/480,~2ms*/msPerfect,/*1ms*/hmsPerfect,/*.5ms*/qmsPerfect,/*.25ms*/
		p100mcs,p50mcs,p25mcs,p10mcs,p5mcs,p3mcs,p2mcs,pMCS,pHMCS,PERFECT }
	public class VerticalOneKey : IMinigame {
		public static Dictionary<string, Action<Game>> InGameConstructorthings = new() {
			["v1k"] = delegate (Game game) {
				game._currentMinigames.Add(new VerticalOneKey(game._textRenderer, BuiltInV1KCharts[0]));
				game._gameModes.Add(GameIdentifier);},
			["v1k2"] = delegate (Game game) {
				game._currentMinigames.Add(new VerticalOneKey(game._textRenderer, BuiltInV1KCharts[1]));
				game._gameModes.Add(GameIdentifier);}
		};
		public const string GameIdentifier = "v1k";
		static readonly V1KChart[] BuiltInV1KCharts = new V1KChart[2];
		public static void StartInit() {
			V1KKey[] V1KChart1KeyData = new V1KKey[4096];
			for (int i = 0; i < 4096; i++) { V1KChart1KeyData[i] = new V1KKey(Math.Pow(i, 0.5)); }
			V1KKey[] V1KChart2KeyData = new V1KKey[32768];
			for (int i = 0; i < 32768; i++) { V1KChart2KeyData[i] = new V1KKey(i / 694.20); }
			BuiltInV1KCharts[0] = new V1KChart(V1KChart1KeyData);
			BuiltInV1KCharts[1] = new V1KChart(V1KChart2KeyData);
			DataStuff.noInputChatCommands["v1kshowalldata"] = DataStuff.noInputChatCommands["v1kshowall"] = DataStuff.noInputChatCommands["v1kshowallinfo"] = delegate (Game game) {
				DisplayFullInfo = !DisplayFullInfo; };
			InGameConstructorthings["1k fnf"] = InGameConstructorthings["1kfnf"] = InGameConstructorthings["fnf 1k"] = InGameConstructorthings["fnf1k"] =
			InGameConstructorthings["verticalonekey"] = InGameConstructorthings["vertical one key"] = InGameConstructorthings["v1k"];
			DataStuff.chatCommands["v1kloadmap "] = delegate (Game game, string path) {
				List<VerticalOneKey> v1kl = [];
				foreach (IMinigame minigame in game._currentMinigames) { if (minigame is VerticalOneKey m0) v1kl.Add(m0); }
				if (v1kl.Count > 0) {
					if (File.Exists(path)) {
						string data = File.ReadAllText(path);
						foreach (VerticalOneKey _m in v1kl)
						if (_m.TryLoadMapFromString(data)) Console.WriteLine("Loadmap success!"); else Console.WriteLine("Failed to load map.");}
					else Console.WriteLine("Path does not exist!"); } };
		}
		public Keys keybind = Keys.E;
		public Text gameRenderer;
		public Stopwatch stopwatch = new();
		public VerticalOneKey(Text renderer, V1KChart chart) { gameRenderer = renderer; this.chart = chart; LoadMap(chart); RestartMap(); }
		public VerticalOneKey(Text renderer) { gameRenderer = renderer; }
		private string lingeringText = "No note hit yet";
		public float timeOffset = 0;
		private uint currentKey = 0;
		// private uint lastDisplayedKey = 0; // might be used to cull the notes at some point.
		public double time = 0;
		// public double dt = 0;
		public float scrollSpeed = 1.0f; // what fraction of a second does it take for a note to go from the top to the bottom.
		public static bool DisplayFullInfo = false;
		public uint[] AccAmts = new uint[Enum.GetValues<ManiaAcc>().Length]; // an array containing how many times the player has hit each of the accuracies including misses.
		private V1KChart chart;
		public void LoadMap(V1KChart chart) {
			currentKey = 0;
			// lastDisplayedKey = 0;
			stopwatch.Reset();
			this.chart = chart;}
		public void StartMap() { stopwatch.Start(); }
		public void RestartMap() {
			timeOffset = -3;
			currentKey = 0;
			// lastDisplayedKey = 0;
			stopwatch.Restart(); }
		public void TogglePauseMap() { if (stopwatch.IsRunning) stopwatch.Stop(); else stopwatch.Start(); }
		public void PauseMap() { stopwatch.Stop(); }
		public float[][] CalcVertices(Vector2i posOffset, Vector2 posScale, Vector2 noteScale, GlyphData noteGlyphData, Vector2i windowSize) {
			float ftexW = gameRenderer.TextTexture.Width;
			float ftexH = gameRenderer.TextTexture.Height;
			float halfCeilTrueSzX = MathF.Ceiling(noteGlyphData.sizeX * noteScale.X) * 0.5f; // the x value for half of the ceiling of the true size
			float halfCeilTrueSzY = MathF.Ceiling(noteGlyphData.sizeY * noteScale.Y) * 0.5f; // the y value for half of the ceiling of the true size
			int WinSX = windowSize.X; int WinSY = windowSize.Y;
			float oX = posOffset.X + posScale.X * WinSX; // offset x
			float oY = posOffset.Y + posScale.Y * WinSY; // offset y
			List<float[]> vertices = [];    int I = 0; // I is vertex index
			float[] v = new float[Text.BulkDrawFloats];
			float tSX = noteGlyphData.tStartX; // texture start x
			float tSY = noteGlyphData.tStartY; // texture start y
			float tNX = noteGlyphData.tEndX; // texture end x
			float tNY = noteGlyphData.tEndY; // texture end y
			float baseSX = (MathF.Floor(oX - halfCeilTrueSzX) + .5f) / WinSX; // the base value for the start X
			float baseSY = (MathF.Floor(oY - halfCeilTrueSzY) + .5f) / WinSY; // the base value for the start Y
			float baseNX = (MathF.Floor(oX + halfCeilTrueSzX) + .5f) / WinSX; // the base value for the end X
			float baseNY = (MathF.Floor(oY + halfCeilTrueSzY) + .5f) / WinSY; // the base value for the end Y
			// float baseMY = (MathF.Floor(oY) + .5f) / WinSY; // the base value for the middle value of Y. commented bc not used rn.
			
			// when the time is exactly the note's time, it should be exactly at the judgement offset.
			// when the time is the note's time minus the reciprocral of the scroll speed, it will be above the judgement line by exactly the height of the screen.
			// Anywhere inbetween will be inbetween.
			// for (uint i = currentKey; i < chart.KeyData.Length; i++) {
			for (int i = chart.KeyData.Length; i-- > currentKey;) {
				V1KKey noteData = chart.KeyData[i];
				double t = noteData.time; // the time that the note appears at
				float noteOffset = (float)(t-time)*scrollSpeed*2;
				float sY = baseSY + noteOffset; float nY = baseNY + noteOffset;
				v[I]=baseSX;v[I+1]=sY;v[I+2]=tSX;v[I+3]=tSY;v[I+4]=baseSX;v[I+5]=nY;v[I+6]=tSX;v[I+7]=tNY;v[I+8]=baseNX;v[I+9]=nY;v[I+10]=tNX;v[I+11]=tNY;v[I+12]=baseNX;v[I+13]=sY;v[I+14]=tNX;v[I+15]=tSY;
				if (I == Text.BulkDrawFloats-16) {
					vertices.Add(v);
					v = new float[Text.BulkDrawFloats]; // this step is VERY important! i think adding to the list just adds a pointer, and it doesn't actually clone it, so you need to create it again or else it will be filled with the same table over and over again.
					I = 0;} else I += 16;}
			float halfCeilTrueJudgementSzX = MathF.Ceiling(noteGlyphData.sizeX * noteScale.X) * 0.625f;
			float halfCeilTrueJudgementSzY = MathF.Ceiling(noteGlyphData.sizeY * noteScale.Y) * 0.5f;
			float SX = (MathF.Floor(oX - halfCeilTrueJudgementSzX) + .5f) / WinSX;
			float SY = (MathF.Floor(oY - halfCeilTrueJudgementSzY) + .5f) / WinSY;
			float NX = (MathF.Floor(oX + halfCeilTrueJudgementSzX) + .5f) / WinSX;
			float NY = (MathF.Floor(oY + halfCeilTrueJudgementSzY) + .5f) / WinSY;
			for (uint i = 0; i < 2; i++) {
				v[I]=SX;v[I+1]=SY;v[I+2]=tSX;v[I+3]=tSY;v[I+4]=SX;v[I+5]=NY;v[I+6]=tSX;v[I+7]=tNY;v[I+8]=NX;v[I+9]=NY;v[I+10]=tNX;v[I+11]=tNY;v[I+12]=NX;v[I+13]=SY;v[I+14]=tNX;v[I+15]=tSY;
				if (I == Text.BulkDrawFloats-16) {
					vertices.Add(v);
					v = new float[Text.BulkDrawFloats]; // this step is VERY important! i think adding to the list just adds a pointer, and it doesn't actually clone it, so you need to create it again or else it will be filled with the same table over and over again.
					I = 0;} else I += 16;}
			if (I == Text.BulkDrawFloats) { vertices.Add(v); } else {
				float[] vt = new float[I];
				Array.Copy(v, vt, I);
				vertices.Add(vt);}
			float[][] V = new float[vertices.Count][];
			for (int i = vertices.Count; i-- > 0;) { V[i] = vertices[i]; }
			return V;}
		public void Update() {
			uint currentKeyNow = currentKey;
			while (currentKey < chart.KeyData.Length && chart.KeyData[currentKey].time - time <= -0.3) { currentKey++; } // calculates miss amounts
			uint missAmount = currentKey - currentKeyNow;
			if (missAmount != 0) { Console.WriteLine("Missed " + missAmount + " notes this frame."); AccAmts[(int)ManiaAcc.miss] += missAmount; }
		}
		public void Render(Game game) {
			GlyphData noteGlyphData = FontCharFillerThing.FontCharDeeta.SChars["note"];
			float[][] data = CalcVertices(Vector2i.Zero, new((float)Math.Sin(time)*.4f,(float)(-.2+Math.Cos(time)*.05f)), new(16), noteGlyphData, game._clientSize);
			if (data.Length > 0) gameRenderer.RenderWithPrecalculatedVertices(data, game._textShader, new(1));
			gameRenderer.RenderText(game, game._textShader, "Map time: " + time + "\n" + lingeringText, new(0), new(-.5f,0.3f), new(8), new(1,0,1), 10f, game._clientSize, FontCharFillerThing.FontCharDeeta, false);
			if (DisplayFullInfo) {
				string FullInfoText = "Ratings:";
				foreach (ManiaAcc s in Enum.GetValues<ManiaAcc>()) FullInfoText += "\n" + Enum.GetName(s) + ": " + AccAmts[(int)s];
				gameRenderer.RenderText(game, game._textShader, FullInfoText, new(0), new(0.5f,0.3f), new(2), new(1,0,1), 10f, game._clientSize, FontCharFillerThing.FontCharDeeta, false); }}
		public override void OnKeyDown(KeyboardKeyEventArgs e) {
			Keys key = e.Key;
			if (key == keybind) { // do this only if it is the keybind.
				double time = stopwatch.Elapsed.TotalSeconds + timeOffset;
				uint currentKeyNow = currentKey;
				while (currentKey < chart.KeyData.Length && chart.KeyData[currentKey].time - time <= -0.3) { currentKey++; }
				uint missAmount = currentKey - currentKeyNow;
				if (missAmount != 0) { Console.WriteLine("Missed " + missAmount + " notes this frame."); AccAmts[(int)ManiaAcc.miss] += missAmount; }
				if (currentKey >= chart.KeyData.Length) { Console.WriteLine("There are no more notes; the chart has ended ig"); return; }
				V1KKey CurrentKey = chart.KeyData[currentKey];
				double keyTime = CurrentKey.time;
				double timeDiff = keyTime - time; // the amount of time before the note would be perfectly at the place.
				double timeDiffMS = timeDiff * 1000;
				string timeDiffMSStr = timeDiffMS + "MS";
				bool Hit = true;
				switch (timeDiff) {
					case > 0.5:
						Hit = false;
						AccAmts[(int)ManiaAcc.nothing]++;
						// lingeringText = "No note was hit. " + timeDiffMS + "MS";
						Console.Write("No note was hit.");
						break;
					case > 0.3:
						AccAmts[(int)ManiaAcc.early]++;
						lingeringText = "TOO early... " + timeDiffMSStr;
						Console.Write("Too early.");
						break;
					/* case > -.0000000000001 and < .0000000000001: Console.WriteLine("+-100femptoseconds??????"); break;
					case > -.000000000001 and < .000000000001: Console.WriteLine("+-1ps??????"); break;
					case > -.00000000001 and < .00000000001: Console.WriteLine("+-10ps??????"); break;
					case > -.0000000001 and < .0000000001: Console.WriteLine("+-100ps TIMING??????"); break;
					case > -.000000001 and < .000000001: Console.WriteLine("+-1ns TIMING??????"); break;
					case > -.00000001 and < .00000001: Console.WriteLine("+-10ns TIMING??????"); break;
					case > -.0000001 and < .0000001: Console.WriteLine("+-100ns TIMING??????"); break; */
					case 0:
						AccAmts[(int)ManiaAcc.PERFECT]++;
						lingeringText = "PERFECT " + timeDiffMSStr;
						Console.Write("PERFECT TIMING?? :O;"); break;
					case > -.0000005 and < .0000005:
						AccAmts[(int)ManiaAcc.pHMCS]++;
						lingeringText = "HALF MICROSECOND PERFECT " + timeDiffMSStr;
						Console.Write("+-0.5μs TIMING??????;"); break;
					case > -.000001 and < .000001:
						AccAmts[(int)ManiaAcc.pMCS]++;
						lingeringText = "MICROSECOND PERFECT " + timeDiffMSStr;
						Console.Write("+-1μs TIMING??????;"); break;
					case > -.000002 and < .000002:
						AccAmts[(int)ManiaAcc.p2mcs]++;
						lingeringText = "2MCS PERFECT " + timeDiffMSStr;
						Console.Write("+-2μs???;"); break;
					case > -.000003 and < .000003:
						AccAmts[(int)ManiaAcc.p3mcs]++;
						lingeringText = "3MCS PERFECT " + timeDiffMSStr;
						Console.Write("+-3μs???;"); break;
					case > -.000005 and < .000005:
						AccAmts[(int)ManiaAcc.p5mcs]++;
						lingeringText = "5MCS PERFECT " + timeDiffMSStr;
						Console.Write("+-5μs???;"); break;
					case > -.00001 and < .00001:
						AccAmts[(int)ManiaAcc.p10mcs]++;
						lingeringText = "10MCS PERFECT " + timeDiffMSStr;
						Console.Write("+-10μs???;"); break;
					case > -.000025 and < .000025:
						AccAmts[(int)ManiaAcc.p25mcs]++;
						lingeringText = "25MCS PERFECT " + timeDiffMSStr;
						Console.Write("+-25μs???;"); break;
					case > -.00005 and < .00005:
						AccAmts[(int)ManiaAcc.p50mcs]++;
						lingeringText = ".05MS Perfect! " + timeDiffMSStr;
						Console.Write("+-50μs???;"); break;
					case > -.0001 and < .0001:
						AccAmts[(int)ManiaAcc.p100mcs]++;
						lingeringText = ".1MS Perfect! " + timeDiffMSStr;
						Console.Write("+-100μs!!;"); break;
					case > -.00025 and < .00025:
						AccAmts[(int)ManiaAcc.qmsPerfect]++;
						lingeringText = ".25ms perfect! " + timeDiffMSStr;
						Console.Write("Hit within +-0.25ms!!;"); break;
					case > -.0005 and < .0005:
						AccAmts[(int)ManiaAcc.hmsPerfect]++;
						lingeringText = ".5ms perfect! " + timeDiffMSStr;
						Console.Write("Hit within +-0.5ms!!;"); break;
					case > -.001 and < .001:
						AccAmts[(int)ManiaAcc.msPerfect]++;
						lingeringText = "Millisecond perfect! " + timeDiffMSStr;
						Console.Write("Hit within +-1ms!!;"); break;
					case > -1d / 480d and < 1d / 480d:
						AccAmts[(int)ManiaAcc.fp240]++;
						lingeringText = "Frame perfect at 240fps! " + timeDiffMSStr;
						Console.Write("Frame perfect at 240fps!;"); break;
					case > -1d / 240d and < 1d / 240d:
						AccAmts[(int)ManiaAcc.fp120]++;
						lingeringText = "Frame perfect at 120fps! " + timeDiffMSStr;
						Console.Write("Frame perfect at 120fps!;"); break;
					case > -1d / 120d and < 1d / 120d:
						AccAmts[(int)ManiaAcc.fp60]++;
						lingeringText = "Frame perfect at 60fps! " + timeDiffMSStr;
						Console.Write("Frame perfect at 60fps!;"); break;
					case > -.01 and < .01:
						AccAmts[(int)ManiaAcc.excellent]++;
						lingeringText = "Excellent! " + timeDiffMSStr;
						Console.Write("Excellent +-10ms!;"); break;
					case > -1d / 60d and < 1d / 60d:
						AccAmts[(int)ManiaAcc.fp30]++;
						lingeringText = "Frame perfect at 30fps! " + timeDiffMSStr;
						Console.Write("Frame perfect at 30fps! (+-1/60);"); break;
					case > -.03 and < .03:
						AccAmts[(int)ManiaAcc.sick]++;
						lingeringText = "Sick! " + timeDiffMSStr;
						Console.Write("Sick! (+-30ms);"); break;
					case > -.05 and < .05:
						AccAmts[(int)ManiaAcc.great]++;
						lingeringText = "Great! " + timeDiffMSStr;
						Console.Write("Great (+-50ms);"); break;
					case > -.08 and < .08:
						AccAmts[(int)ManiaAcc.good]++;
						lingeringText = "Good. " + timeDiffMSStr;
						Console.Write("Good. (+-80ms);"); break;
					case > -.11 and < .11:
						AccAmts[(int)ManiaAcc.okay]++;
						lingeringText = "Okay. " + timeDiffMSStr;
						Console.Write("Okay. (+-110ms);"); break;
					case > -.15 and < .15:
						AccAmts[(int)ManiaAcc.bad]++;
						lingeringText = "Bad. " + timeDiffMSStr;
						Console.Write("Bad. (+-150ms);"); break;
					case > -.3 and < .3:
						AccAmts[(int)ManiaAcc.yikes]++;
						lingeringText = "Yikes. " + timeDiffMSStr;
						Console.Write("Yikes. (+-300ms);"); break;
					default: Hit = false; break; }
				Console.WriteLine(" TimeDiff: " + timeDiff);
				if (Hit) currentKey++; } else {
				switch (key) {
					case Keys.P:
						TogglePauseMap(); break;
					case Keys.GraveAccent:
						RestartMap(); break;
					default: break; } } }
		public bool TryLoadMapFromString(string chart) {
			string[] processedS1 = chart.Split('\n');
			if (processedS1.Length < 2) return false; // must have at least 2 lines; first is version number
			Console.WriteLine("Version number: " + processedS1[0]);
			string[] processedS2 = processedS1[1].Split(',');
			V1KKey[] keyData1 = new V1KKey[processedS2.Length];
			for (int i = 0; i < processedS2.Length; i++) {
				string[] stringThingy = processedS2[i].Split(' ');
				switch (stringThingy.Length) {
					case 1: keyData1[i] = new V1KKey(Convert.ToDouble(stringThingy[0])); break; // not a hold note
					case 2: keyData1[i] = new V1KKey(Convert.ToDouble(stringThingy[0]), Convert.ToSingle(stringThingy[1])); break; // hold note
					default: return false; } }
			V1KChart NewChart = new(keyData1);
			LoadMap(NewChart);
			return true; }
		public override void OnRenderFrame(Game game, double dt) {
			base.OnRenderFrame(game, dt);
			time = stopwatch.Elapsed.TotalSeconds + timeOffset;
			Render(game); }
		public override void OnUpdateFrame(Game game, double dt) {
			base.OnUpdateFrame(game, dt);
			// Console.WriteLine(_dT);
			time = stopwatch.Elapsed.TotalSeconds + timeOffset;
			Update(); }
		public override void OnKeyUp(KeyboardKeyEventArgs e) {
			base.OnKeyUp(e); } }
	public class OldMiningGame : IMinigame {
		public static Dictionary<string, Action<Game>> InGameConstructorthings = new() {
			["old:miner"] = delegate (Game game) {
				game._currentMinigames.Add(new OldMiningGame());
				game._gameModes.Add(GameIdentifier);}
		};
		public const string GameIdentifier = "old:miner";
		public static void StartInit() {
			DataStuff.chatCommands["oldmininggame "] = delegate (Game game, string str) {
				int i;
				bool ret = true;
				for (i = 0; i < game._currentMinigames.Count; i++) if (game._currentMinigames[i] is OldMiningGame) { ret = false; break; }
				if (ret) { Console.WriteLine("nuh uh there ain't an old mining game active bozo"); return; }
				switch (str) {
					case "newmininggame": game._currentMinigames[i] = new MiningGame(); break;
					default:
						Console.WriteLine("uhh um what you entered i haven't really implemented yet or you've misspelled or you're Searching For a Code That Doesn't Exist /j (i haven't actually watched that idk what happens in it :3)");
						break; } };
		}
		public const int cBSN = 5; // chunk bitshift number; 1<<cBSAmt is the chunk size. the name also coincidentally references chaotic bean simulator :3
		public const int cSz = 1<<cBSN; // chunk size
		public const int cSzSq = cSz * cSz; // chunk size squared
		public const int cBS2 = 10; // chunk bitshift number * 2; 1<<cBS2 is cSzSq. this is now PURPOSEFULLY referencing cbs :3
		public const int cSzCb = cSzSq * cSz; // chunk size cubed
		public const int cBSX3 = 15; // chunk bitshift number * 3; 1<<cBSX3 is cSzCb. the name PURPOSEFULLY references cbs and has a cool X3 face :3
		public Dictionary<Vector3i, ushort[]> bData = []; // for key, should have probably like 16 bits for x, 22 bits for y, 16 bits for z. for val, should probably have 10 bits for type and 6 for faces.
		public Dictionary<Vector3i, int[]> fData = []; // for key, should have probably like 16 bits for x, 22 bits for y, 16 bits for z. for val, should probably have 10 bits for type and 6 for faces.
		public Dictionary<Vector3i, uint> CDT = []; // for key, should have probably like 16 bits for x, 22 bits for y, 16 bits for z. for val, should probably have 10 bits for type and 6 for faces.
		public Shader shader;
		// public static UInt128 DictionaryKey(uint x, uint y, uint z) => ((UInt128)x << 64) + ((UInt128)y << 32) + z;
		// Vector3i[] vdataBF = new Vector3i[Text.BulkDrawConst];
		int[] pos = new int[Text.BulkDrawConst*3];
		public void SetBlock(int x, int y, int z, ushort v)
		{
			Vector3i key1 = (x >> cBSN, y >> cBSN, z >> cBSN);
			(uint X, uint Y, uint Z) = unchecked(((uint)x, (uint)y, (uint)z));
			// uint key2=X-(X>>cBSN<<cBSN)|((Y-(Y>>cBSN<<cBSN))<<cBSN)|((Z-(Z>>cBSN<<cBSN))<<cBS2);
			uint key2=(X&31)|((Y&31)<<cBSN)|((Z&31)<<cBS2);
			if (!bData.TryGetValue(key1, out ushort[] value)) {
				value = new ushort[cSzCb];
				bData[key1] = value;
				// fData[key1] = new ulong[cSzCb/8];
				fData[key1] = new int[cSzCb*3];
				CDT[key1] = 0;
			}
			// try {value[key2] = v;} catch { throw new Exception("bruh anyways " + x + "," + y + "," + z + "," + v + ", " + key1 + ", " + key2); }
			// if (v != value.v[key2]) { value.v[key2] = v; CDT[key1][key2>>5] = 1u<<(int)(key2&31); }
			if (v != value[key2]) { value[key2] = v; CDT[key1] = 1u; }
		}
		public OldMiningGame() { }
		int cubeVAO;
		int cubeVBO;
		int instanceVBO;
		static float[] cubeVerts = [
			0,0,1, 0,0, 1,0,1, 1,0, 0,1,1, 0,1,  1,0,1, 1,0, 1,1,1, 1,1, 0,1,1, 1,0, // front
			0,0,0, 0,0, 0,1,0, 1,0, 1,0,0, 0,1,  1,0,0, 1,0, 0,1,0, 1,1, 1,1,0, 1,0, // back
			1,0,0, 0,0, 1,1,0, 1,0, 1,0,1, 0,1,  1,0,1, 1,0, 1,1,0, 1,1, 1,1,1, 1,0, // right
			0,0,0, 0,0, 0,0,1, 1,0, 0,1,0, 0,1,  0,0,1, 1,0, 0,1,1, 1,1, 0,1,0, 1,0, // left
			0,1,1, 0,0, 0,1,0, 1,0, 1,1,1, 0,1,  1,1,1, 1,0, 0,1,0, 1,1, 1,1,0, 1,0, // top
			0,0,1, 0,0, 1,0,1, 1,0, 0,0,0, 0,1,  1,0,1, 1,0, 1,0,0, 1,1, 0,0,0, 1,0, // bottom
		];
		// private void UpdateBlocks()
		// {
		// 	Vector3i[] translations = new Vector3i[blockData.Count];
		// 	int i = 0;
		// 	foreach (KeyValuePair<UInt128, uint> thingy in blockData) {
		// 		if (thingy.Value == 0) continue;
		// 		translations[i] = new Vector3i((int)(thingy.Key >> 64), (int)(thingy.Key >> 32), (int)thingy.Key);
		// 		i++;
		// 	}
		// 	translations = translations[..i];
		// 	GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
		// 	renderingAmount = translations.Length;
		// 	GL.BufferSubData(BufferTarget.ArrayBuffer, 0, renderingAmount * 3, translations);

		// }
		private void GenDefaultWorld() {
			const int r = 18;
			int x, y, z, innerCircle, circleAmount;
			for (x = 0; x < r; x++) {
				circleAmount = (int)MathF.Sqrt(r * r - x * x);
				for (z = 0; z < circleAmount; z++)
					for (y = 0; y < 100; y += 17){
						SetBlock(x,y,z,1);SetBlock(-x,y,z,1);SetBlock(x,y,-z,1);SetBlock(-x,y,-z,1);}}
			for (x = 0; x < r; x++) {
				circleAmount = (int)MathF.Sqrt(r * r - x * x);
				for (y = 0; y < circleAmount; y++){
					int circleAmount2 = (int)MathF.Sqrt(r * r - x * x - y * y);
					for (z = 0; z < circleAmount2; z++){
						SetBlock(50+x, y, z,1);
						SetBlock(50-x, y, z,1);
						SetBlock(50+x, y,-z,1);
						SetBlock(50-x, y,-z,1);
						SetBlock(50+x,-y, z,1);
						SetBlock(50-x,-y, z,1);
						SetBlock(50+x,-y,-z,1);
						SetBlock(50-x,-y,-z,1);}}}
			for (x = 0; x < 75; x++) {
				circleAmount = (int)MathF.Sqrt(75 * 75 - x * x);
				int px = 200 + x;
				int nx = 200 - x;
				for (y = 0; y < circleAmount; y++){
					int circleAmount2 = (int)MathF.Sqrt(75 * 75 - x * x - y * y);
					for (z = 0; z < circleAmount2; z++){
						SetBlock(px, y, z,1);
						SetBlock(nx, y, z,1);
						SetBlock(px, y,-z,1);
						SetBlock(nx, y,-z,1);
						SetBlock(px,-y, z,1);
						SetBlock(nx,-y, z,1);
						SetBlock(px,-y,-z,1);
						SetBlock(nx,-y,-z,1);}}}
			long bruh = Stopwatch.GetTimestamp();
			const int r2 = 53; float rsq=r2*r2;float inRSQ=(r2-8f)*(r2-8f);float temp;
			for (x = r2; x > 0; x--) {
				int xsq = x * x;int px = x + 50;int nx = -x + 50; // "positive" x and "negative" x
				circleAmount = (int)MathF.Sqrt(rsq - xsq);
				for (y = circleAmount; y > 0; y--) {
					int ysqmxsq = -(y * y + xsq); // y^2 plus x^2
					int circleAmount2 = (int)MathF.Sqrt(rsq - ysqmxsq);
					temp = MathF.Sqrt(inRSQ - ysqmxsq);
					innerCircle = float.IsNaN(temp) ? 0 : (int)temp;
					for (z = circleAmount2; z > innerCircle; z--) {
						SetBlock(px, y, z,1);SetBlock(nx, y, z,1);SetBlock(px,-y, z,1);SetBlock(nx,-y, z,1);
						SetBlock(px, y,-z,1);SetBlock(nx, y,-z,1);SetBlock(px,-y,-z,1);SetBlock(nx,-y,-z,1);}
					if (innerCircle == 0)
					{SetBlock(px,y,0,1);SetBlock(nx,y,0,1);SetBlock(px,-y,0,1);SetBlock(nx,-y,0,1);}}
				temp = MathF.Sqrt(inRSQ - xsq);
				innerCircle = float.IsNaN(temp) ? 0 : (int)temp;
				for (z = circleAmount; z > innerCircle; z--) {
					SetBlock(px,0,z,1);SetBlock(nx,0,z,1);SetBlock(px,0,-z,1);SetBlock(nx,0,-z,1);}
				SetBlock(px, 0, 0, 1); SetBlock(nx, 0, 0, 1);}
			circleAmount = (int)r2;
			for(y=circleAmount;y>0;y--){int ysq=y*y;int circleAmount2=(int)MathF.Sqrt(rsq-ysq);temp=MathF.Sqrt(inRSQ - ysq);
				innerCircle=float.IsNaN(temp)?0:(int)temp;
				for(z=circleAmount2;z>innerCircle;z--){SetBlock(50,y,z,1);SetBlock(50,-y,z,1);SetBlock(50,y,-z,1);SetBlock(50,-y,-z,1);}
				SetBlock(50, y, 0, 1); SetBlock(50, -y, 0, 1);}
			for(z=circleAmount;z>inRSQ;z--){ SetBlock(50, 0, z, 1); SetBlock(50, 0, -z, 1);}SetBlock(50, 0, 0, 1);
			Console.WriteLine("sphere gen took " + Stopwatch.GetElapsedTime(bruh).TotalMilliseconds + "ms.");
			for(int i = 0; i < 1000; i++) { SetBlock(Random.Shared.Next(128), Random.Shared.Next(128), Random.Shared.Next(128), (ushort)Random.Shared.Next(2)); }
			/* Vector2[] translations = new Vector2[100];
			for (int i = 0; i < 100; i++) {
				translations[i] = new Vector2((Random.Shared.NextSingle()-0.5f)*2, (Random.Shared.NextSingle()-0.5f)*2); }
			Vector3i[] translations = new Vector3i[blockData.Count];int i = 0;
			foreach (KeyValuePair<UInt128, uint> thingy in blockData) {
				translations[i] = new Vector3i((int)(thingy.Key >> 64), (int)(thingy.Key >> 32), (int)thingy.Key); i++;}
			for (i = 0; i < translations.Length; i++) { Vector3i v = translations[i];
				Console.Write(v.X + "," + v.Y + "," + v.Z + "; ");}
			for (int i = 0; i < 100; i++){shader.SetVector2("offsets[" + i + "]", translations[i]);}*/}
		public override void OnLoad(Game game) {
			base.OnLoad(game);
			game._player.IsFlying = true;
			game._camera.MaxDist = 1024;
			shader = new("Shaders/miningGame/shader.vert", "Shaders/miningGame/shader.frag");
			GL.UseProgram(shader.Handle);

			GenDefaultWorld();

			cubeVAO = GL.GenVertexArray();
			GL.BindVertexArray(cubeVAO);

			cubeVBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, cubeVBO);
			GL.BufferData(BufferTarget.ArrayBuffer, cubeVerts.Length * sizeof(float), cubeVerts, BufferUsageHint.StaticDraw);

			// Set up vertex attributes
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
			GL.EnableVertexAttribArray(1);
			GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

			instanceVBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
			GL.BufferData(BufferTarget.ArrayBuffer, sizeof(int) * Text.BulkDrawConst * 3, pos, BufferUsageHint.DynamicDraw);

			GL.EnableVertexAttribArray(2);
			GL.VertexAttribIPointer(2, 3, VertexAttribIntegerType.Int, 3 * sizeof(int), 0);
			GL.VertexAttribDivisor(2, 1);
		}
		public override void OnRenderFrame(Game game, double dt) {// 1154
			base.OnRenderFrame(game, dt);
			shader.Use();
			// Vector3 up = game._camera.Up;
			// assume up is 0,1,0
			(float vX,float vY,float vZ)=game._camera.Position-game._camera.Target;float n=1f/MathF.Sqrt(vX*vX+vY*vY+vZ*vZ);vX*=n;vY*=n;vZ*=n;
			n=1f/MathF.Sqrt(vZ*vZ+vX*vX);(float rX,float rZ)=(vZ*n,-vX*n);
			(float v2X,float v2Y,float v2Z)=(vY*rZ,vZ*rX-vX*rZ,-vY*rX);
			n=1f/MathF.Sqrt(v2X*v2X+v2Y*v2Y+v2Z*v2Z);v2X*=n;v2Y*=n;v2Z*=n;
			(float eX,float eY,float eZ)=game._camera.Position;Matrix4 r=game._camera.Projection;
			float x4=-rX*eX-rZ*eZ;float y4=-v2X*eX-v2Y*eY-v2Z*eZ;float z4=-vX*eX-vY*eY-vZ*eZ;
			(float x5,float y5,float z5,float w5,float x6,float y6,float z6,float w6,float x7,float y7,float z7,float w7,float x8,float y8,float z8,float w8)
			=(r.Row0.X,r.Row0.Y,r.Row0.Z,r.Row0.W,r.Row1.X,r.Row1.Y,r.Row1.Z,r.Row1.W,r.Row2.X,r.Row2.Y,r.Row2.Z,r.Row2.W,r.Row3.X,r.Row3.Y,r.Row3.Z,r.Row3.W);
			shader.SetMatrix4("view", new(new(rX*x5+v2X*x6+vX*x7,rX*y5+v2X*y6+vX*y7,rX*z5+v2X*z6+vX*z7,rX*w5+v2X*w6+vX*w7),
			new(v2Y*x6+vY*x7,v2Y*y6+vY*y7,v2Y*z6+vY*z7,v2Y*w6+vY*w7),
			new(rZ*x5+v2Z*x6+vZ*x7,rZ*y5+v2Z*y6+vZ*y7,rZ*z5+v2Z*z6+vZ*z7,rZ*w5+v2Z*w6+vZ*w7),
			new(x4*x5+y4*x6+z4*x7+x8,x4*y5+y4*y6+z4*y7+y8,x4*z5+y4*z6+z4*z7+z8,x4*w5+y4*w6+z4*w7+w8)));
			// int location = GL.GetUniformLocation(shader.Handle, "texOffset");
			// GL.Uniform2(location, Vector2.Zero);
			// shader.SetMatrix4("projection", game._camera.Projection);
			GL.BindVertexArray(cubeVAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
			int i = 0;
			// Vector3i[] pos = new Vector3i[Text.BulkDrawConst];
#if MINING_GAME_PROFILELOG
			int test = 0;
			long sdfjkl0, sdfjkl, sdfjkl1, sdfjkl2, sdfjkl3, sdfjkl4, sdfjklsjd = Stopwatch.GetTimestamp();
			double testt = 0, testt2 = 0, testt3 = 0, testt4, testt5 = 0, testt6 = 0, testt10 = 0, nonrendertime = 0;
#endif
			foreach (KeyValuePair<Vector3i, ushort[]> d in bData) {
				ushort[] B = d.Value;
				// ulong[] F = fData[d.Key];
				ulong[] F = new ulong[cSzCb >> 3];
				int[] realF = fData[d.Key];
#if MINING_GAME_PROFILELOG
				sdfjkl0 = Stopwatch.GetTimestamp();
#endif
				(int chunkPosX, int chunkPosY, int chunkPosZ) = d.Key;
				(int basePosX, int basePosY, int basePosZ) = (chunkPosX << cBSN, chunkPosY << cBSN, chunkPosZ << cBSN);
				// Console.WriteLine("1184lengths: " + _d.Length + ", " + _f.Length);
				int j, ind, tmp2, j1, k, j2, j1xcs, j1xcssq, _i = 0;
				uint tmp;
#if MINING_GAME_PER_FACE_CULL
				uint tmp1;
#endif
				// Console.WriteLine("1185");
#if MINING_GAME_PROFILELOG
				sdfjkl = Stopwatch.GetTimestamp();
#endif
				if (CDT[d.Key] > 0) {
					Array.Fill(F, 0ul);
					ushort[] ocfd = new ushort[cSzSq * 6]; // other chunk face deeta
#if MINING_GAME_PROFILELOG
					test++;
					testt10 += Stopwatch.GetElapsedTime(sdfjkl).TotalMilliseconds;
					sdfjkl1 = Stopwatch.GetTimestamp();
#endif
					{
						if (bData.TryGetValue((chunkPosX, chunkPosY, chunkPosZ - 1), out ushort[] _B)) {
							Array.Copy(_B, cSzCb-cSzSq, ocfd, cSzSq*4, cSzSq); }
						if (bData.TryGetValue((chunkPosX, chunkPosY, chunkPosZ + 1), out _B)) {
							Array.Copy(_B, 0, ocfd, cSzSq*5, cSzSq); }
						int Y1, Y2;
						if (bData.TryGetValue((chunkPosX - 1, chunkPosY, chunkPosZ), out _B)) {
							for(Y2=0;Y2<cSzCb;Y2+=cSzSq){Y1=Y2>>cBSN;
								ocfd[Y1]=_B[Y2|31];ocfd[Y1|1]=_B[Y2|63];ocfd[Y1|2]=_B[Y2|95];ocfd[Y1|3]=_B[Y2|127];
								ocfd[Y1|4]=_B[Y2|159];ocfd[Y1|5]=_B[Y2|191];ocfd[Y1|6]=_B[Y2|223];ocfd[Y1|7]=_B[Y2|255];
								ocfd[Y1|8]=_B[Y2|287];ocfd[Y1|9]=_B[Y2|319];ocfd[Y1|10]=_B[Y2|351];ocfd[Y1|11]=_B[Y2|383];
								ocfd[Y1|12]=_B[Y2|415];ocfd[Y1|13]=_B[Y2|447];ocfd[Y1|14]=_B[Y2|479];ocfd[Y1|15]=_B[Y2|511];
								ocfd[Y1|16]=_B[Y2|543];ocfd[Y1|17]=_B[Y2|575];ocfd[Y1|18]=_B[Y2|607];ocfd[Y1|19]=_B[Y2|639];
								ocfd[Y1|20]=_B[Y2|671];ocfd[Y1|21]=_B[Y2|703];ocfd[Y1|22]=_B[Y2|735];ocfd[Y1|23]=_B[Y2|767];
								ocfd[Y1|24]=_B[Y2|799];ocfd[Y1|25]=_B[Y2|831];ocfd[Y1|26]=_B[Y2|863];ocfd[Y1|27]=_B[Y2|895];
								ocfd[Y1|28]=_B[Y2|927];ocfd[Y1|29]=_B[Y2|959];ocfd[Y1|30]=_B[Y2|991];ocfd[Y1|31]=_B[Y2|1023];}}
						if (bData.TryGetValue((chunkPosX + 1, chunkPosY, chunkPosZ), out _B)) {
							// for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;
							// 	YP = cSzSq|YB1;
							for(Y2=0;Y2<cSzCb;Y2+=cSzSq){Y1=cSzSq|(Y2>>cBSN);
								ocfd[Y1]=_B[Y2];ocfd[Y1|1]=_B[Y2|32];ocfd[Y1|2]=_B[Y2|64];ocfd[Y1|3]=_B[Y2|96];
								ocfd[Y1|4]=_B[Y2|128];ocfd[Y1|5]=_B[Y2|160];ocfd[Y1|6]=_B[Y2|192];ocfd[Y1|7]=_B[Y2|224];
								ocfd[Y1|8]=_B[Y2|256];ocfd[Y1|9]=_B[Y2|288];ocfd[Y1|10]=_B[Y2|320];ocfd[Y1|11]=_B[Y2|352];
								ocfd[Y1|12]=_B[Y2|384];ocfd[Y1|13]=_B[Y2|416];ocfd[Y1|14]=_B[Y2|448];ocfd[Y1|15]=_B[Y2|480];
								ocfd[Y1|16]=_B[Y2|512];ocfd[Y1|17]=_B[Y2|544];ocfd[Y1|18]=_B[Y2|576];ocfd[Y1|19]=_B[Y2|608];
								ocfd[Y1|20]=_B[Y2|640];ocfd[Y1|21]=_B[Y2|672];ocfd[Y1|22]=_B[Y2|704];ocfd[Y1|23]=_B[Y2|736];
								ocfd[Y1|24]=_B[Y2|768];ocfd[Y1|25]=_B[Y2|800];ocfd[Y1|26]=_B[Y2|832];ocfd[Y1|27]=_B[Y2|864];
								ocfd[Y1|28]=_B[Y2|896];ocfd[Y1|29]=_B[Y2|928];ocfd[Y1|30]=_B[Y2|960];ocfd[Y1|31]=_B[Y2|992];}}
						if (bData.TryGetValue((chunkPosX, chunkPosY - 1, chunkPosZ), out _B)) {
							// for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;
							// 	YP = cSzSq*2|YB1;
							for(Y2=0;Y2<cSzCb;Y2+=cSzSq){Y1=(cSzSq*2)|(Y2>>cBSN);
								ocfd[Y1]=_B[Y2|992];ocfd[Y1|1]=_B[Y2|993];ocfd[Y1|2]=_B[Y2|994];ocfd[Y1|3]=_B[Y2|995];
								ocfd[Y1|4]=_B[Y2|996];ocfd[Y1|5]=_B[Y2|997];ocfd[Y1|6]=_B[Y2|998];ocfd[Y1|7]=_B[Y2|999];
								ocfd[Y1|8]=_B[Y2|1000];ocfd[Y1|9]=_B[Y2|1001];ocfd[Y1|10]=_B[Y2|1002];ocfd[Y1|11]=_B[Y2|1003];
								ocfd[Y1|12]=_B[Y2|1004];ocfd[Y1|13]=_B[Y2|1005];ocfd[Y1|14]=_B[Y2|1006];ocfd[Y1|15]=_B[Y2|1007];
								ocfd[Y1|16]=_B[Y2|1008];ocfd[Y1|17]=_B[Y2|1009];ocfd[Y1|18]=_B[Y2|1010];ocfd[Y1|19]=_B[Y2|1011];
								ocfd[Y1|20]=_B[Y2|1012];ocfd[Y1|21]=_B[Y2|1013];ocfd[Y1|22]=_B[Y2|1014];ocfd[Y1|23]=_B[Y2|1015];
								ocfd[Y1|24]=_B[Y2|1016];ocfd[Y1|25]=_B[Y2|1017];ocfd[Y1|26]=_B[Y2|1018];ocfd[Y1|27]=_B[Y2|1019];
								ocfd[Y1|28]=_B[Y2|1020];ocfd[Y1|29]=_B[Y2|1021];ocfd[Y1|30]=_B[Y2|1022];ocfd[Y1|31]=_B[Y2|1023];}}
						if (bData.TryGetValue((chunkPosX, chunkPosY + 1, chunkPosZ), out _B)) {
							// for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;
							// 	YP = cSzSq*3|YB1;
							for(Y2=0;Y2<cSzCb;Y2+=cSzSq){Y1=(cSzSq*3)|(Y2>>cBSN);
								ocfd[Y1]=_B[Y2];ocfd[Y1|1]=_B[Y2|1];ocfd[Y1|2]=_B[Y2|2];ocfd[Y1|3]=_B[Y2|3];
								ocfd[Y1|4]=_B[Y2|4];ocfd[Y1|5]=_B[Y2|5];ocfd[Y1|6]=_B[Y2|6];ocfd[Y1|7]=_B[Y2|7];
								ocfd[Y1|8]=_B[Y2|8];ocfd[Y1|9]=_B[Y2|9];ocfd[Y1|10]=_B[Y2|10];ocfd[Y1|11]=_B[Y2|11];
								ocfd[Y1|12]=_B[Y2|12];ocfd[Y1|13]=_B[Y2|13];ocfd[Y1|14]=_B[Y2|14];ocfd[Y1|15]=_B[Y2|15];
								ocfd[Y1|16]=_B[Y2|16];ocfd[Y1|17]=_B[Y2|17];ocfd[Y1|18]=_B[Y2|18];ocfd[Y1|19]=_B[Y2|19];
								ocfd[Y1|20]=_B[Y2|20];ocfd[Y1|21]=_B[Y2|21];ocfd[Y1|22]=_B[Y2|22];ocfd[Y1|23]=_B[Y2|23];
								ocfd[Y1|24]=_B[Y2|24];ocfd[Y1|25]=_B[Y2|25];ocfd[Y1|26]=_B[Y2|26];ocfd[Y1|27]=_B[Y2|27];
								ocfd[Y1|28]=_B[Y2|28];ocfd[Y1|29]=_B[Y2|29];ocfd[Y1|30]=_B[Y2|30];ocfd[Y1|31]=_B[Y2|31];}}}

					// for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;
					// 	ocfd[YB1|0]=v.B[YB2|31];ocfd[YB1|1]=v.B[YB2|63];ocfd[YB1|2]=v.B[YB2|95];ocfd[YB1|3]=v.B[YB2|127];
					// 	ocfd[YB1|4]=v.B[YB2|159];ocfd[YB1|5]=v.B[YB2|191];ocfd[YB1|6]=v.B[YB2|223];ocfd[YB1|7]=v.B[YB2|255];
					// 	ocfd[YB1|8]=v.B[YB2|287];ocfd[YB1|9]=v.B[YB2|319];ocfd[YB1|10]=v.B[YB2|351];ocfd[YB1|11]=v.B[YB2|383];
					// 	ocfd[YB1|12]=v.B[YB2|415];ocfd[YB1|13]=v.B[YB2|447];ocfd[YB1|14]=v.B[YB2|479];ocfd[YB1|15]=v.B[YB2|511];
					// 	ocfd[YB1|16]=v.B[YB2|543];ocfd[YB1|17]=v.B[YB2|575];ocfd[YB1|18]=v.B[YB2|607];ocfd[YB1|19]=v.B[YB2|639];
					// 	ocfd[YB1|20]=v.B[YB2|671];ocfd[YB1|21]=v.B[YB2|703];ocfd[YB1|22]=v.B[YB2|735];ocfd[YB1|23]=v.B[YB2|767];
					// 	ocfd[YB1|24]=v.B[YB2|799];ocfd[YB1|25]=v.B[YB2|831];ocfd[YB1|26]=v.B[YB2|863];ocfd[YB1|27]=v.B[YB2|895];
					// 	ocfd[YB1|28]=v.B[YB2|927];ocfd[YB1|29]=v.B[YB2|959];ocfd[YB1|30]=v.B[YB2|991];ocfd[YB1|31]=v.B[YB2|1023];
					// 	YP = cSzSq|YB1;
					// 	ocfd[YP|0]=v2.B[YB2|31];ocfd[YP|1]=v2.B[YB2|63];ocfd[YP|2]=v2.B[YB2|95];ocfd[YP|3]=v2.B[YB2|127];
					// 	ocfd[YP|4]=v2.B[YB2|159];ocfd[YP|5]=v2.B[YB2|191];ocfd[YP|6]=v2.B[YB2|223];ocfd[YP|7]=v2.B[YB2|255];
					// 	ocfd[YP|8]=v2.B[YB2|287];ocfd[YP|9]=v2.B[YB2|319];ocfd[YP|10]=v2.B[YB2|351];ocfd[YP|11]=v2.B[YB2|383];
					// 	ocfd[YP|12]=v2.B[YB2|415];ocfd[YP|13]=v2.B[YB2|447];ocfd[YP|14]=v2.B[YB2|479];ocfd[YP|15]=v2.B[YB2|511];
					// 	ocfd[YP|16]=v2.B[YB2|543];ocfd[YP|17]=v2.B[YB2|575];ocfd[YP|18]=v2.B[YB2|607];ocfd[YP|19]=v2.B[YB2|639];
					// 	ocfd[YP|20]=v2.B[YB2|671];ocfd[YP|21]=v2.B[YB2|703];ocfd[YP|22]=v2.B[YB2|735];ocfd[YP|23]=v2.B[YB2|767];
					// 	ocfd[YP|24]=v2.B[YB2|799];ocfd[YP|25]=v2.B[YB2|831];ocfd[YP|26]=v2.B[YB2|863];ocfd[YP|27]=v2.B[YB2|895];
					// 	ocfd[YP|28]=v2.B[YB2|927];ocfd[YP|29]=v2.B[YB2|959];ocfd[YP|30]=v2.B[YB2|991];ocfd[YP|31]=v2.B[YB2|1023];
					// 	YP = cSzSq*2|YB1;
					// 	ocfd[YP|0]=v3.B[YB2|992];ocfd[YP|1]=v3.B[YB2|993];ocfd[YP|2]=v3.B[YB2|994];ocfd[YP|3]=v3.B[YB2|995];
					// 	ocfd[YP|4]=v3.B[YB2|996];ocfd[YP|5]=v3.B[YB2|997];ocfd[YP|6]=v3.B[YB2|998];ocfd[YP|7]=v3.B[YB2|999];
					// 	ocfd[YP|8]=v3.B[YB2|1000];ocfd[YP|9]=v3.B[YB2|1001];ocfd[YP|10]=v3.B[YB2|1002];ocfd[YP|11]=v3.B[YB2|1003];
					// 	ocfd[YP|12]=v3.B[YB2|1004];ocfd[YP|13]=v3.B[YB2|1005];ocfd[YP|14]=v3.B[YB2|1006];ocfd[YP|15]=v3.B[YB2|1007];
					// 	ocfd[YP|16]=v3.B[YB2|1008];ocfd[YP|17]=v3.B[YB2|1009];ocfd[YP|18]=v3.B[YB2|1010];ocfd[YP|19]=v3.B[YB2|1011];
					// 	ocfd[YP|20]=v3.B[YB2|1012];ocfd[YP|21]=v3.B[YB2|1013];ocfd[YP|22]=v3.B[YB2|1014];ocfd[YP|23]=v3.B[YB2|1015];
					// 	ocfd[YP|24]=v3.B[YB2|1016];ocfd[YP|25]=v3.B[YB2|1017];ocfd[YP|26]=v3.B[YB2|1018];ocfd[YP|27]=v3.B[YB2|1019];
					// 	ocfd[YP|28]=v3.B[YB2|1020];ocfd[YP|29]=v3.B[YB2|1021];ocfd[YP|30]=v3.B[YB2|1022];ocfd[YP|31]=v3.B[YB2|1023];
					// 	YP = cSzSq*3|YB1;
					// 	ocfd[YP|0]=v4.B[YB2|0];ocfd[YP|1]=v4.B[YB2|1];ocfd[YP|2]=v4.B[YB2|2];ocfd[YP|3]=v4.B[YB2|3];
					// 	ocfd[YP|4]=v4.B[YB2|4];ocfd[YP|5]=v4.B[YB2|5];ocfd[YP|6]=v4.B[YB2|6];ocfd[YP|7]=v4.B[YB2|7];
					// 	ocfd[YP|8]=v4.B[YB2|8];ocfd[YP|9]=v4.B[YB2|9];ocfd[YP|10]=v4.B[YB2|10];ocfd[YP|11]=v4.B[YB2|11];
					// 	ocfd[YP|12]=v4.B[YB2|12];ocfd[YP|13]=v4.B[YB2|13];ocfd[YP|14]=v4.B[YB2|14];ocfd[YP|15]=v4.B[YB2|15];
					// 	ocfd[YP|16]=v4.B[YB2|16];ocfd[YP|17]=v4.B[YB2|17];ocfd[YP|18]=v4.B[YB2|18];ocfd[YP|19]=v4.B[YB2|19];
					// 	ocfd[YP|20]=v4.B[YB2|20];ocfd[YP|21]=v4.B[YB2|21];ocfd[YP|22]=v4.B[YB2|22];ocfd[YP|23]=v4.B[YB2|23];
					// 	ocfd[YP|24]=v4.B[YB2|24];ocfd[YP|25]=v4.B[YB2|25];ocfd[YP|26]=v4.B[YB2|26];ocfd[YP|27]=v4.B[YB2|27];
					// 	ocfd[YP|28]=v4.B[YB2|28];ocfd[YP|29]=v4.B[YB2|29];ocfd[YP|30]=v4.B[YB2|30];ocfd[YP|31]=v4.B[YB2|31];
					// }
					// switch (selection) {
					// 	case 0b0000:break;
					// 	case 0b0001:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;for(X=0;X<cSz;X++)ocfd[YB1|X]=v.B[YB2|(X<<cBSN)|31];}break;
					// 	case 0b0010:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;for(X=0;X<cSz;X++)ocfd[cSzSq|YB1|X]=v2.B[YB2|(X<<cBSN)];}break;
					// 	case 0b0011:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;for(X=0;X<cSz;X++){ocfd[YB1|X]=v.B[YB2|(X<<cBSN)|31];ocfd[cSzSq|YB1|X]=v2.B[YB2|(X<<cBSN)];}}break;
					// 	case 0b0100:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;for(X=0;X<cSz;X++)ocfd[cSzSq*2|YB1|X]=v3.B[X|YB2|cSz*31];}break;
					// 	case 0b0101:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;for(X=0;X<cSz;X++){ocfd[YB1|X]=v.B[YB2|(X<<cBSN)|31];ocfd[cSzSq*2|YB1|X]=v3.B[X|YB2|cSz*31];}}break;
					// 	case 0b0110:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;for(X=0;X<cSz;X++){ocfd[cSzSq|YB1|X]=v2.B[YB2|(X<<cBSN)];ocfd[cSzSq*2|YB1|X]=v3.B[X|YB2|cSz*31];}}break;
					// 	case 0b0111:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;for(X=0;X<cSz;X++){ind=YB1|X;
					// 				ocfd[ind]=v.B[YB2|(X<<cBSN)|31];ocfd[cSzSq|ind]=v2.B[YB2|(X<<cBSN)];ocfd[cSzSq*2|ind]=v3.B[X|YB2|cSz*31];}}break;
					// 	case 0b1000:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;for(X=0;X<cSz;X++)ocfd[cSzSq*3|YB1|X]=v4.B[X|YB2];}break;
					// 	case 0b1001:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;for(X=0;X<cSz;X++){ocfd[YB1|X]=v.B[YB2|(X<<cBSN)|31];ocfd[cSzSq*3|YB1|X]=v4.B[X|YB2];}}break;
					// 	case 0b1010:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;for(X=0;X<cSz;X++){ocfd[cSzSq|YB1|X]=v2.B[YB2|(X<<cBSN)];ocfd[cSzSq*3|YB1|X]=v4.B[X|YB2];}}break;
					// 	case 0b1011:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;for(X=0;X<cSz;X++){ind=YB1|X;
					// 				ocfd[ind]=v.B[YB2|(X<<cBSN)|31];ocfd[cSzSq|ind]=v2.B[YB2|(X<<cBSN)];ocfd[cSzSq*3|ind]=v4.B[X|YB2];}}break;
					// 	case 0b1100:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;for(X=0;X<cSz;X++){ocfd[cSzSq*2|YB1|X]=v3.B[X|YB2|cSz*31];ocfd[cSzSq*3|YB1|X]=v4.B[X|YB2];}}break;
					// 	case 0b1101:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;for(X=0;X<cSz;X++){ind=YB1|X;
					// 				ocfd[ind]=v.B[YB2|(X<<cBSN)|31];ocfd[cSzSq*2|ind]=v3.B[X|YB2|cSz*31];ocfd[cSzSq*3|ind]=v4.B[X|YB2];}}break;
					// 	case 0b1110:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;for(X=0;X<cSz;X++){ind=YB1|X;
					// 				ocfd[cSzSq|YB1|X]=v2.B[YB2|(X<<cBSN)];ocfd[cSzSq*2|YB1|X]=v3.B[X|YB2|cSz*31];ocfd[cSzSq*3|YB1|X]=v4.B[X|YB2];}}break;
					// 	// case 0b1111:for(Y=0;Y<cSz;Y++){YB1=Y<<cBSN;YB2=Y<<cBS2;for(X=0;X<cSz;X+=2){ind = YB1|X;
					// 	// 			ocfd[ind]=v.B[YB2|(X<<cBSN)|31]; // -x; yz
					// 	// 			ocfd[cSzSq|ind]=v2.B[YB2|(X<<cBSN)]; // +x; yz
					// 	// 			ocfd[cSzSq*2|ind]=v3.B[X|YB2|cSz*31]; // -y; xz
					// 	// 			ocfd[cSzSq*3|ind]=v4.B[X|YB2]; /* +y; xz */}}break;
					// 	case 0b1111:for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;
					// 			// ocfd[YB1]=v.B[YB2|31];//-x;yz
					// 			// ocfd[cSzSq|YB1]=v2.B[YB2];//+x;yz
					// 			// ocfd[cSzSq*2|YB1]=v3.B[YB2|cSz*31];//-y;xz
					// 			// ocfd[cSzSq*3|YB1]=v4.B[YB2];/*+y;xz*/
					// 			// X=0;ind=YB1|X;
					// 			// ocfd[ind]=v.B[YB2|(X<<cBSN)|31];
					// 			// ocfd[cSzSq|ind]=v2.B[YB2|(X<<cBSN)];
					// 			// ocfd[cSzSq*2|ind]=v3.B[X|YB2|cSz*31];
					// 			// ocfd[cSzSq*3|ind]=v4.B[X|YB2];
					// 			ocfd[YB1|0]=v.B[YB2|31];ocfd[YB1|1]=v.B[YB2|63];ocfd[YB1|2]=v.B[YB2|95];ocfd[YB1|3]=v.B[YB2|127];
					// 			ocfd[YB1|4]=v.B[YB2|159];ocfd[YB1|5]=v.B[YB2|191];ocfd[YB1|6]=v.B[YB2|223];ocfd[YB1|7]=v.B[YB2|255];
					// 			ocfd[YB1|8]=v.B[YB2|287];ocfd[YB1|9]=v.B[YB2|319];ocfd[YB1|10]=v.B[YB2|351];ocfd[YB1|11]=v.B[YB2|383];
					// 			ocfd[YB1|12]=v.B[YB2|415];ocfd[YB1|13]=v.B[YB2|447];ocfd[YB1|14]=v.B[YB2|479];ocfd[YB1|15]=v.B[YB2|511];
					// 			ocfd[YB1|16]=v.B[YB2|543];ocfd[YB1|17]=v.B[YB2|575];ocfd[YB1|18]=v.B[YB2|607];ocfd[YB1|19]=v.B[YB2|639];
					// 			ocfd[YB1|20]=v.B[YB2|671];ocfd[YB1|21]=v.B[YB2|703];ocfd[YB1|22]=v.B[YB2|735];ocfd[YB1|23]=v.B[YB2|767];
					// 			ocfd[YB1|24]=v.B[YB2|799];ocfd[YB1|25]=v.B[YB2|831];ocfd[YB1|26]=v.B[YB2|863];ocfd[YB1|27]=v.B[YB2|895];
					// 			ocfd[YB1|28]=v.B[YB2|927];ocfd[YB1|29]=v.B[YB2|959];ocfd[YB1|30]=v.B[YB2|991];ocfd[YB1|31]=v.B[YB2|1023];
					// 			int _sdjfl = cSzSq|YB1;
					// 			ocfd[_sdjfl|0]=v2.B[YB2|31];ocfd[_sdjfl|1]=v2.B[YB2|63];ocfd[_sdjfl|2]=v2.B[YB2|95];ocfd[_sdjfl|3]=v2.B[YB2|127];
					// 			ocfd[_sdjfl|4]=v2.B[YB2|159];ocfd[_sdjfl|5]=v2.B[YB2|191];ocfd[_sdjfl|6]=v2.B[YB2|223];ocfd[_sdjfl|7]=v2.B[YB2|255];
					// 			ocfd[_sdjfl|8]=v2.B[YB2|287];ocfd[_sdjfl|9]=v2.B[YB2|319];ocfd[_sdjfl|10]=v2.B[YB2|351];ocfd[_sdjfl|11]=v2.B[YB2|383];
					// 			ocfd[_sdjfl|12]=v2.B[YB2|415];ocfd[_sdjfl|13]=v2.B[YB2|447];ocfd[_sdjfl|14]=v2.B[YB2|479];ocfd[_sdjfl|15]=v2.B[YB2|511];
					// 			ocfd[_sdjfl|16]=v2.B[YB2|543];ocfd[_sdjfl|17]=v2.B[YB2|575];ocfd[_sdjfl|18]=v2.B[YB2|607];ocfd[_sdjfl|19]=v2.B[YB2|639];
					// 			ocfd[_sdjfl|20]=v2.B[YB2|671];ocfd[_sdjfl|21]=v2.B[YB2|703];ocfd[_sdjfl|22]=v2.B[YB2|735];ocfd[_sdjfl|23]=v2.B[YB2|767];
					// 			ocfd[_sdjfl|24]=v2.B[YB2|799];ocfd[_sdjfl|25]=v2.B[YB2|831];ocfd[_sdjfl|26]=v2.B[YB2|863];ocfd[_sdjfl|27]=v2.B[YB2|895];
					// 			ocfd[_sdjfl|28]=v2.B[YB2|927];ocfd[_sdjfl|29]=v2.B[YB2|959];ocfd[_sdjfl|30]=v2.B[YB2|991];ocfd[_sdjfl|31]=v2.B[YB2|1023];
					// 			_sdjfl = cSzSq*2|YB1;
					// 			ocfd[_sdjfl|0]=v3.B[YB2|992];ocfd[_sdjfl|1]=v3.B[YB2|993];ocfd[_sdjfl|2]=v3.B[YB2|994];ocfd[_sdjfl|3]=v3.B[YB2|995];
					// 			ocfd[_sdjfl|4]=v3.B[YB2|996];ocfd[_sdjfl|5]=v3.B[YB2|997];ocfd[_sdjfl|6]=v3.B[YB2|998];ocfd[_sdjfl|7]=v3.B[YB2|999];
					// 			ocfd[_sdjfl|8]=v3.B[YB2|1000];ocfd[_sdjfl|9]=v3.B[YB2|1001];ocfd[_sdjfl|10]=v3.B[YB2|1002];ocfd[_sdjfl|11]=v3.B[YB2|1003];
					// 			ocfd[_sdjfl|12]=v3.B[YB2|1004];ocfd[_sdjfl|13]=v3.B[YB2|1005];ocfd[_sdjfl|14]=v3.B[YB2|1006];ocfd[_sdjfl|15]=v3.B[YB2|1007];
					// 			ocfd[_sdjfl|16]=v3.B[YB2|1008];ocfd[_sdjfl|17]=v3.B[YB2|1009];ocfd[_sdjfl|18]=v3.B[YB2|1010];ocfd[_sdjfl|19]=v3.B[YB2|1011];
					// 			ocfd[_sdjfl|20]=v3.B[YB2|1012];ocfd[_sdjfl|21]=v3.B[YB2|1013];ocfd[_sdjfl|22]=v3.B[YB2|1014];ocfd[_sdjfl|23]=v3.B[YB2|1015];
					// 			ocfd[_sdjfl|24]=v3.B[YB2|1016];ocfd[_sdjfl|25]=v3.B[YB2|1017];ocfd[_sdjfl|26]=v3.B[YB2|1018];ocfd[_sdjfl|27]=v3.B[YB2|1019];
					// 			ocfd[_sdjfl|28]=v3.B[YB2|1020];ocfd[_sdjfl|29]=v3.B[YB2|1021];ocfd[_sdjfl|30]=v3.B[YB2|1022];ocfd[_sdjfl|31]=v3.B[YB2|1023];
					// 			_sdjfl = cSzSq*3|YB1;
					// 			ocfd[_sdjfl|0]=v4.B[YB2|0];ocfd[_sdjfl|1]=v4.B[YB2|1];ocfd[_sdjfl|2]=v4.B[YB2|2];ocfd[_sdjfl|3]=v4.B[YB2|3];
					// 			ocfd[_sdjfl|4]=v4.B[YB2|4];ocfd[_sdjfl|5]=v4.B[YB2|5];ocfd[_sdjfl|6]=v4.B[YB2|6];ocfd[_sdjfl|7]=v4.B[YB2|7];
					// 			ocfd[_sdjfl|8]=v4.B[YB2|8];ocfd[_sdjfl|9]=v4.B[YB2|9];ocfd[_sdjfl|10]=v4.B[YB2|10];ocfd[_sdjfl|11]=v4.B[YB2|11];
					// 			ocfd[_sdjfl|12]=v4.B[YB2|12];ocfd[_sdjfl|13]=v4.B[YB2|13];ocfd[_sdjfl|14]=v4.B[YB2|14];ocfd[_sdjfl|15]=v4.B[YB2|15];
					// 			ocfd[_sdjfl|16]=v4.B[YB2|16];ocfd[_sdjfl|17]=v4.B[YB2|17];ocfd[_sdjfl|18]=v4.B[YB2|18];ocfd[_sdjfl|19]=v4.B[YB2|19];
					// 			ocfd[_sdjfl|20]=v4.B[YB2|20];ocfd[_sdjfl|21]=v4.B[YB2|21];ocfd[_sdjfl|22]=v4.B[YB2|22];ocfd[_sdjfl|23]=v4.B[YB2|23];
					// 			ocfd[_sdjfl|24]=v4.B[YB2|24];ocfd[_sdjfl|25]=v4.B[YB2|25];ocfd[_sdjfl|26]=v4.B[YB2|26];ocfd[_sdjfl|27]=v4.B[YB2|27];
					// 			ocfd[_sdjfl|28]=v4.B[YB2|28];ocfd[_sdjfl|29]=v4.B[YB2|29];ocfd[_sdjfl|30]=v4.B[YB2|30];ocfd[_sdjfl|31]=v4.B[YB2|31];
					// 		}break;
					// 	default:break;}// woah this probably belongs in horriblecode.cs
#if MINING_GAME_PROFILELOG
					testt += Stopwatch.GetElapsedTime(sdfjkl1).TotalMicroseconds;
					sdfjkl2 = Stopwatch.GetTimestamp();
#endif
					for (j1=0,k=0;j1<cSz;j1++){
						j1xcs=j1<<cBSN;
						j1xcssq=j1<<cBS2;
						for(j2=0;j2<cSz;j2++,k+=cSz,_i++){
							tmp=(B[k]==0?0:1u)|(B[k|1]==0?0:2u)|(B[k|2]==0?0:4u)|(B[k|3]==0?0:8u)|(B[k|4]==0?0:0x10u)|(B[k|5]==0?0:0x20u)|(B[k|6]==0?0:0x40u)|
							(B[k|7]==0?0:0x80u)|(B[k|8]==0?0:0x100u)|(B[k|9]==0?0:0x200u)|(B[k|10]==0?0:0x400u)|(B[k|11]==0?0:0x800u)|(B[k|12]==0?0:0x1000u)|
							(B[k|13]==0?0:0x2000u)|(B[k|14]==0?0:0x4000u)|(B[k|15]==0?0:0x8000u)|(B[k|16]==0?0:0x10000u)|(B[k|17]==0?0:0x20000u)|(B[k|18]==0?0:0x40000u)|
							(B[k|19]==0?0:0x80000u)|(B[k|20]==0?0:0x100000u)|(B[k|21]==0?0:0x200000u)|(B[k|22]==0?0:0x400000u)|(B[k|23]==0?0:0x800000u)|
							(B[k|24]==0?0:0x1000000u)|(B[k|25]==0?0:0x2000000u)|(B[k|26]==0?0:0x4000000u)|(B[k|27]==0?0:0x8000000u)|(B[k|28]==0?0:0x10000000u)|
							(B[k|29]==0?0:0x20000000u)|(B[k|30]==0?0:0x40000000u)|(B[k|31]==0?0:0x80000000);
							//tmp=(tmp&~(tmp<<1))|(tmp&~(tmp>>1));
							ind=(j2<<cBSN)+j1xcssq;
#if MINING_GAME_PER_FACE_CULL
							tmp1=tmp&~((tmp<<1)|(ocfd[_i]==0?0:1u));
							tmp&=~((tmp>>1)|(ocfd[_i|cSzSq]==0?0:0x80000000));
							//ind=(y<<cBSN)+(z<<cBS2);
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);
							while(tmp2<32) {
								F[(ind|tmp2)>>3]|=0b00000010ul<<((tmp2&7)<<3);
								tmp^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp); }
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp1);
							while(tmp2<32) {
								F[(ind|tmp2)>>3]|=0b00000001ul<<((tmp2&7)<<3); // BRUH ONE CHARACTER MISSING LOL (THE L CHARACTER)
								tmp1^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp1); }
#else
							tmp&=~((tmp<<1)|(tmp>>1)|(ocfd[_i]==0?0:1u)|(ocfd[_i|cSzSq]==0?0:0x80000000));
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);
							while(tmp2<32) {
								F[(ind|tmp2)>>3]|=0b00111111ul<<((tmp2&7)<<3);
								tmp^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp); }
#endif
							int irs3 = _i >> 3; // i right shifted by 3. INTERNAL REVENUE SERVICE MENTION!?!?1?!?!1?!1?!!?1?!1?3991680010395?!1!?1?10395?!?!1?1039511?
							tmp=(B[_i]==0?0:1u)|(B[_i|cSzSq]==0?0:2u)|(B[_i|(cSzSq*2)]==0?0:4u)|(B[_i|(cSzSq*3)]==0?0:8u)|(B[_i|(cSzSq*4)]==0?0:16u)|(B[_i|(cSzSq*5)]==0?0:32u)|(B[_i|(cSzSq*6)]==0?0:64u)|
							(B[_i|(cSzSq*7)]==0?0:128u)|(B[_i|(cSzSq*8)]==0?0:256u)|(B[_i|(cSzSq*9)]==0?0:512u)|(B[_i|(cSzSq*10)]==0?0:1024u)|(B[_i|(cSzSq*11)]==0?0:2048u)|(B[_i|(cSzSq*12)]==0?0:4096u)|
							(B[_i|(cSzSq*13)]==0?0:8192u)|(B[_i|(cSzSq*14)]==0?0:16384u)|(B[_i|(cSzSq*15)]==0?0:32768u)|(B[_i|(cSzSq*16)]==0?0:65536u)|(B[_i|(cSzSq*17)]==0?0:131072u)|(B[_i|(cSzSq*18)]==0?0:262144u)|
							(B[_i|(cSzSq*19)]==0?0:524288u)|(B[_i|(cSzSq*20)]==0?0:1048576u)|(B[_i|(cSzSq*21)]==0?0:2097152u)|(B[_i|(cSzSq*22)]==0?0:4194304u)|(B[_i|(cSzSq*23)]==0?0:8388608u)|
							(B[_i|(cSzSq*24)]==0?0:16777216u)|(B[_i|(cSzSq*25)]==0?0:33554432u)|(B[_i|(cSzSq*26)]==0?0:67108864u)|(B[_i|(cSzSq*27)]==0?0:0x8000000u)|(B[_i|(cSzSq*28)]==0?0:0x10000000u)|
							(B[_i|(cSzSq*29)]==0?0:0x20000000u)|(B[_i|(cSzSq*30)]==0?0:0x40000000u)|(B[_i|(cSzSq*31)]==0?0:0x80000000);
#if MINING_GAME_PER_FACE_CULL
							ulong bsamtish = 0b00100000ul<<((_i&7)<<3); // bitshift amount ish
							// tmp=(tmp&~(tmp<<1))|(tmp&~(tmp>>1));
							// tmp&=~(tmp<<1)|~(tmp>>1);
							// tmp&=~((tmp<<1)&(tmp>>1));
							tmp1=tmp&~((tmp<<1)|(ocfd[cSzSq*4|_i]==0?0:1u));
							tmp&=~((tmp>>1)|(ocfd[cSzSq*5|_i]==0?0:0x80000000));
							// x = basePosX | j2;
							// y = basePosY | j1;
							// ind = x | (y<<cBSN);
							// ind = j2 | j1xcs;
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);
							// while (tmp2 < 32) {
							// 	// vdataBF[i] = (x, y, basePosZ+tmp2);
							// 	_f[ind | tmp2<<cBS2] |= 0b00110000;
							// 	tmp ^= 1u<<tmp2;
							// 	tmp2 = System.Numerics.BitOperations.TrailingZeroCount(tmp); }
							while(tmp2<32){
								// vdataBF[i] = (basePosX + tmp2, y, z);
								F[irs3|(tmp2<<(cBS2-3))]|=bsamtish;
								tmp^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);}
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp1);
							bsamtish >>= 1;
							while(tmp2<32){
								// vdataBF[i] = (basePosX + tmp2, y, z);
								F[irs3|(tmp2<<(cBS2-3))]|=bsamtish;
								tmp1^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp1);}
#else
							ulong bsamtish = 0b00111111ul<<((_i&7)<<3); // bitshift amount ish
							tmp&=~((tmp<<1)|(tmp>>1)|(ocfd[cSzSq*4|_i]==0?0:1u)|(ocfd[cSzSq*5|_i]==0?0:0x80000000));
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);
							while(tmp2<32){
								// vdataBF[i] = (basePosX + tmp2, y, z);
								F[irs3|(tmp2<<(cBS2-3))]|=bsamtish;
								tmp^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);}
#endif
							j=j1xcssq|j2;
							tmp=(B[j]==0?0:1u)|(B[j|cSz]==0?0:2u)|(B[j|cSz*2]==0?0:4u)|(B[j|cSz*3]==0?0:8u)|(B[j|cSz*4]==0?0:16u)|(B[j|cSz*5]==0?0:32u)|(B[j|cSz*6]==0?0:64u)|
							(B[j|cSz*7]==0?0:128u)|(B[j|cSz*8]==0?0:256u)|(B[j|cSz*9]==0?0:512u)|(B[j|cSz*10]==0?0:1024u)|(B[j|cSz*11]==0?0:2048u)|(B[j|cSz*12]==0?0:4096u)|
							(B[j|cSz*13]==0?0:8192u)|(B[j|cSz*14]==0?0:16384u)|(B[j|cSz*15]==0?0:32768u)|(B[j|cSz*16]==0?0:65536u)|(B[j|cSz*17]==0?0:131072u)|(B[j|cSz*18]==0?0:262144u)|
							(B[j|cSz*19]==0?0:524288u)|(B[j|cSz*20]==0?0:1048576u)|(B[j|cSz*21]==0?0:2097152u)|(B[j|cSz*22]==0?0:4194304u)|(B[j|cSz*23]==0?0:8388608u)|
							(B[j|cSz*24]==0?0:16777216u)|(B[j|cSz*25]==0?0:33554432u)|(B[j|cSz*26]==0?0:67108864u)|(B[j|cSz*27]==0?0:0x8000000u)|(B[j|cSz*28]==0?0:0x10000000u)|
							(B[j|cSz*29]==0?0:0x20000000u)|(B[j|cSz*30]==0?0:0x40000000u)|(B[j|cSz*31]==0?0:0x80000000);
							irs3 = (j2|j1xcssq) >> 3; // i right shifted by 3. INTERNAL REVENUE SERVICE MENTION!?!?1?!?!1?!1?!!?1?!1?3991680010395?!1!?1?10395?!?!1?1039511?
#if MINING_GAME_PER_FACE_CULL
							tmp1=tmp&~((tmp<<1)|(ocfd[cSzSq*2|_i]==0?0:1u));
							tmp&=~((tmp>>1)|(ocfd[cSzSq*3|_i]==0?0:0x80000000));
							// ind=j2|j1xcssq;
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);
							bsamtish >>= 1;
							while(tmp2<32){
								F[irs3|(tmp2<<(cBSN-3))]|=bsamtish;
								tmp^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);}
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp1);
							bsamtish >>= 1;
							while(tmp2<32){
								F[irs3|(tmp2<<(cBSN-3))]|=bsamtish;
								tmp1^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp1);}
#else
							tmp&=~((tmp<<1)|(tmp>>1)|(ocfd[cSzSq*3|_i]==0?0:0x80000000)|(ocfd[cSzSq*2|_i]==0?0:1u));
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);
							while(tmp2<32){
								F[irs3|(tmp2<<(cBSN-3))]|=bsamtish;
								tmp^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);}
#endif
					}}
					CDT[d.Key] = 0;
#if MINING_GAME_PROFILELOG
					testt3 +=Stopwatch.GetElapsedTime(sdfjkl2).TotalMicroseconds;
					testt2+=Stopwatch.GetElapsedTime(sdfjkl).TotalMicroseconds;
					/*Console.WriteLine("culling took " + Stopwatch.GetElapsedTime(asdasda).TotalMilliseconds + "ms");*/
					sdfjkl3 = Stopwatch.GetTimestamp();
#endif
					ulong a;
					j = 0;
					int y, z;
					// realF = fData[d.Key];
					// (ulong rx, ulong ry, ulong rz) = ((basePosX>0?(ulong)basePosX:(ulong)(basePosX+1048576))<<43,(basePosY>0?(ulong)basePosY:(ulong)(basePosY+2097152))<<21,basePosZ>0?(ulong)basePosZ:(ulong)(basePosZ+1048576));
					// (uint rx, uint ry, uint rz) = ((uint)basePosX,(uint)basePosY,(uint)basePosZ);
					int count = 0;
					for (z=basePosZ;z<basePosZ+cSz;z++){
						for(y=basePosY;y<basePosY+cSz;y++,j+=4){
							a=F[j];if(a>0){if((a&255)>0){realF[count]=basePosX;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF00)>0){realF[count]=basePosX|1;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF0000)>0){realF[count]=basePosX|2;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF000000)>0){realF[count]=basePosX|3;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF00000000)>0){realF[count]=basePosX|4;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF0000000000)>0){realF[count]=basePosX|5;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF000000000000)>0){realF[count]=basePosX|6;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a>>56)>0){realF[count]=basePosX|7;realF[count+1]=y;realF[count+2]=z;count+=3;}}
							a=F[j|1];if(a>0){if((a&255)>0){realF[count]=basePosX|8;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF00)>0){realF[count]=basePosX|9;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF0000)>0){realF[count]=basePosX|10;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF000000)>0){realF[count]=basePosX|11;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF00000000)>0){realF[count]=basePosX|12;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF0000000000)>0){realF[count]=basePosX|13;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF000000000000)>0){realF[count]=basePosX|14;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a>>56)>0){realF[count]=basePosX|15;realF[count+1]=y;realF[count+2]=z;count+=3;}}
							a=F[j|2];if(a>0){if((a&255)>0){realF[count]=basePosX|16;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF00)>0){realF[count]=basePosX|17;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF0000)>0){realF[count]=basePosX|18;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF000000)>0){realF[count]=basePosX|19;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF00000000)>0){realF[count]=basePosX|20;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF0000000000)>0){realF[count]=basePosX|21;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF000000000000)>0){realF[count]=basePosX|22;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a>>56)>0){realF[count]=basePosX|23;realF[count+1]=y;realF[count+2]=z;count+=3;}}
							a=F[j|3];if(a>0){if((a&255)>0){realF[count]=basePosX|24;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF00)>0){realF[count]=basePosX|25;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF0000)>0){realF[count]=basePosX|26;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF000000)>0){realF[count]=basePosX|27;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF00000000)>0){realF[count]=basePosX|28;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF0000000000)>0){realF[count]=basePosX|29;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF000000000000)>0){realF[count]=basePosX|30;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a>>56)>0){realF[count]=basePosX|31;realF[count+1]=y;realF[count+2]=z;count+=3;}} }}
					realF[cSzCb*3-1] = count/3;
#if MINING_GAME_PROFILELOG
					testt5 += Stopwatch.GetElapsedTime(sdfjkl3).TotalMicroseconds; }nonrendertime += Stopwatch.GetElapsedTime(sdfjkl0).TotalMilliseconds;
#else
				}
#endif
					int amt = realF[cSzCb*3-1];
				if (amt > 0){
					if (i+amt > Text.BulkDrawConst) {
#if MINING_GAME_PROFILELOG
						sdfjkl4 = Stopwatch.GetTimestamp();
						Console.WriteLine("1308bruh"+i);
						GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(int)*3*i, pos);
						GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, i); i = 0;
						testt6 += Stopwatch.GetElapsedTime(sdfjkl4).TotalMilliseconds;
#else
						GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(int)*3*i, pos);
						GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, i); i = 0;
#endif
					}// realF.CopyTo(pos, i);
					Array.Copy(realF, 0, pos, i*3, amt*3);
					i += amt;}}
#if MINING_GAME_PROFILELOG
			// Console.WriteLine("1313:" + i);
			Console.WriteLine("1493," + i);if (i > 0) {
				sdfjkl4 = Stopwatch.GetTimestamp();
				// Console.WriteLine("1316bruh"+i);
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(int)*3*i, pos);
				GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, i);
				testt6 += Stopwatch.GetElapsedTime(sdfjkl4).TotalMilliseconds;}
			testt4 = Stopwatch.GetElapsedTime(sdfjklsjd).TotalMilliseconds;
			Console.WriteLine("updated " + test + " chunks/"+bData.Count+". took " + testt10 + "ms to fill," + testt + "μs for other side loading; " + (testt / test) + "μsper for othersides, "+(testt10/test)+"msper for fill. chunkasm "+testt3+"μs;"+(testt3/test)+"μsper; full asm time:"+testt2+"μs,"+(testt2/test)+"μsper. total time was "+testt4+"ms;rendertime(?):"+testt6+"ms.(that's "+(100*testt6/testt4)+"%.)datagathering:"+testt5+"μs,"+(testt5/bData.Count)+"μsper. nonrender time: "+nonrendertime+"ms (that's "+(100*nonrendertime/testt4)+"%.)");
#else
			if (i > 0) {
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(int)*3*i, pos);
				GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, i);
			}
#endif
		}// 1434
		public override void OnUpdateFrame(Game game, double dt)
		{
			base.OnUpdateFrame(game, dt);

			long bruh = Stopwatch.GetTimestamp();
			// if (game._gameTick%4==0){
			// 	bData = [];
			// 	int x, y, z, innerCircle, circleAmount;
			// 	float r = (int)(Math.Sin(game._gameTick/32f)*8f+32); float rsq=r*r;float inRSQ=(r-8f)*(r-8f);float temp;
			// 	int ri = (int)MathF.Ceiling(r);
				
			// 	for (x = ri; x > 0; x--) {
			// 		int xsq = x * x;int px = x + 50;int nx = -x + 50; // "positive" x and "negative" x
			// 		circleAmount = (int)MathF.Ceiling(MathF.Sqrt(rsq - xsq));
			// 		for (y = circleAmount; y > 0; y--) {
			// 			int ysqmxsq = -(y * y + xsq); // y^2 plus x^2
			// 			int circleAmount2 = (int)MathF.Sqrt(rsq - ysqmxsq);
			// 			temp = MathF.Sqrt(inRSQ - ysqmxsq);
			// 			innerCircle = float.IsNaN(temp) ? 0 : (int)temp;
			// 			for (z = circleAmount2; z > innerCircle; z--) {
			// 				SetBlock(px, y, z,1);SetBlock(nx, y, z,1);SetBlock(px,-y, z,1);SetBlock(nx,-y, z,1);
			// 				SetBlock(px, y,-z,1);SetBlock(nx, y,-z,1);SetBlock(px,-y,-z,1);SetBlock(nx,-y,-z,1);}
			// 			if (innerCircle == 0)
			// 			{SetBlock(px,y,0,1);SetBlock(nx,y,0,1);SetBlock(px,-y,0,1);SetBlock(nx,-y,0,1);}}
			// 		temp = MathF.Sqrt(inRSQ - xsq);
			// 		innerCircle = float.IsNaN(temp) ? 0 : (int)temp;
			// 		for (z = circleAmount; z > innerCircle; z--) {
			// 			SetBlock(px,0,z,1);SetBlock(nx,0,z,1);SetBlock(px,0,-z,1);SetBlock(nx,0,-z,1);}
			// 		SetBlock(px, 0, 0, 1); SetBlock(nx, 0, 0, 1);}
			// 	// circleAmount = ri;
			// 	for(y=ri;y>0;y--){int ysq=y*y;int circleAmount2=(int)MathF.Sqrt(rsq-ysq);temp=MathF.Sqrt(inRSQ - ysq);
			// 		innerCircle=float.IsNaN(temp)?0:(int)temp;
			// 		for(z=circleAmount2;z>innerCircle;z--){SetBlock(50,y,z,1);SetBlock(50,-y,z,1);SetBlock(50,y,-z,1);SetBlock(50,-y,-z,1);}
			// 		SetBlock(50, y, 0, 1); SetBlock(50, -y, 0, 1);}
			// 	for(z=ri;z>inRSQ;z--){ SetBlock(50, 0, z, 1); SetBlock(50, 0, -z, 1);}SetBlock(50, 0, 0, 1);
			// 	Console.Write("sphere gen took " + Stopwatch.GetElapsedTime(bruh).TotalMilliseconds + "ms.");bruh = Stopwatch.GetTimestamp();}
			if (game._gameTick % 256 > 127){
				for (int i = 0; i < Random.Shared.Next(500); i++) { SetBlock(Random.Shared.Next(128), 200, Random.Shared.Next(40), (ushort)Random.Shared.Next(2)); }
				for (int i = 0; i < Random.Shared.Next(500); i++) { SetBlock(200, Random.Shared.Next(40), Random.Shared.Next(128), (ushort)Random.Shared.Next(2)); }
				for (int i = 0; i < Random.Shared.Next(500); i++) { SetBlock(Random.Shared.Next(40), Random.Shared.Next(128), 200, (ushort)Random.Shared.Next(2)); }
				for (int i = 0; i < Random.Shared.Next(2000); i++) { SetBlock(Random.Shared.Next(400), Random.Shared.Next(400), Random.Shared.Next(400), (ushort)Random.Shared.Next(2)); }}
#if MINING_GAME_DEBUG
			Console.Write("thingy took " + Stopwatch.GetElapsedTime(bruh).TotalMilliseconds + "ms.");
#endif
		}
		public override void OnKeyDown(KeyboardKeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Key == Keys.L) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point + Random.Shared.Next(3));
		}
		public override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			Console.WriteLine("mining game closing");
#if MINING_GAME_DEBUG
				string write0 = "\n\nyo this mining game is closing i guess\n\n";
				string write1 = "";
				// int skipAmount = bData.Count - 2;
				uint thisVal;
				foreach (var v in bData) {
					// if (skipAmount > 0) { skipAmount--; continue; } // this will probably perform terribly but oh well
					uint currentVal = v.Value[0];
					write0 += v.Key + "; "+currentVal;
					uint repeatCount = 1;
					for (int i = 1; i < cSzCb; i++) {
						if (v.Value[i] == currentVal) repeatCount++;
						else {
							thisVal = v.Value[i];
							if (repeatCount > 1) {write0 += "x" + repeatCount + "," + thisVal; repeatCount = 1;}
							else write0 += "," + thisVal;
							currentVal = thisVal;
							if (write0.Length > 1024) { write1 += write0; write0 = ""; }}}
					if (repeatCount > 1) {write0 += "x" + repeatCount;}
					write0 += "\n";
					Console.Write(write1 + write0);
					write1 = write0 = "";}
				Console.Write("\n\nhas like a lot of voxels, like " + bData.Count + " chunks of them, which is(n't) " + bData.Count * 36 + " verts, or " + bData.Count * 12 + " faces.\n");
#endif
		}}
	public class MiningGame : IMinigame {
		public static Dictionary<string, Action<Game>> InGameConstructorthings = new() {
			["miner"] = delegate (Game game) {
				game._currentMinigames.Add(new MiningGame());
				game._gameModes.Add(GameIdentifier);}
		};
		public const string GameIdentifier = "miner";
		public static readonly Action<Player, Game, byte[]> FlySillyBehavior = delegate (Player plr, Game game, byte[] ksData){
			var ks = game.KeyboardState;
			float movAmt = plr.Walkspeed / game._gameTickSpeed;
			long frameCount = game._frameCount;
			plr.RootPosition += game._camera.Direction*((ks[Keys.S]?movAmt:0)-(ks[Keys.W]?movAmt:0))+
			game._camera.Right*((ks[Keys.D]?movAmt:0)-(ks[Keys.A]?movAmt:0))+
			game._camera.Up*(((ks[Keys.Space] || ks[Keys.E])?movAmt:0)-((ks[Keys.LeftShift] || ks[Keys.Q])?movAmt:0));
			bool silly = (frameCount & 256) == 0;//(float ax, float ay, float az) = plr.RootRotation;
			if (silly) {
				plr.RootScale += (MathF.Sin(frameCount*0.0017f)*1.5f,MathF.Sin(frameCount*0.0022f)*1.5f,MathF.Sin(frameCount*0.0031f)*1.5f);
				plr.RootRotation += (MathF.Sin(frameCount*.009f)*5,MathF.Sin(frameCount*.012f)*5,MathF.Sin(frameCount*.006f)*5);
				for (int i = 0; i < plr.Limbs.Length; i++) plr.LimbRotations[i] = (Random.Shared.NextSingle()*MathF.Tau,Random.Shared.NextSingle()*MathF.Tau,Random.Shared.NextSingle()*MathF.Tau);
			}
			plr.UpdateMats();
		};
		public static void StartInit() {
			DataStuff.noInputChatCommands["/flysilly"] = delegate (Game game) { game._player.moveBehavior = FlySillyBehavior; };
			DataStuff.noInputChatCommands["/flyunsilly"] = delegate (Game game) { game._player.moveBehavior = Player.defaultMoveBehavior; };
			DataStuff.chatCommands["mininggame "] = DataStuff.chatCommands["miner "] = delegate (Game game, string str) {
				int i;
				bool ret = true;
				for (i = 0; i < game._currentMinigames.Count; i++) if (game._currentMinigames[i] is MiningGame) { ret = false; break; }
				if (ret) { Console.WriteLine("nuh uh there ain't a mining game active bozo"); return; }
				switch (str) {
					case "oldmininggame": game._currentMinigames[i] = new OldMiningGame(); break;
					default:
						Console.WriteLine("uhh what you entered i haven't really implemented yet or you've misspelled or you're Searching For a Code That Doesn't Exist /j (i haven't actually watched that idk what happens in it :3)");
						break; } };
		}
		public const int cBSN = 5; // chunk bitshift number; 1<<cBSAmt is the chunk size. the name also coincidentally references chaotic bean simulator :3
		public const int cSz = 1<<cBSN; // chunk size
		public const int cSzSq = cSz * cSz; // chunk size squared
		public const int cBS2 = 10; // chunk bitshift number * 2; 1<<cBS2 is cSzSq. this is now PURPOSEFULLY referencing cbs :3
		public const int cSzCb = cSzSq * cSz; // chunk size cubed
		public const int cBSX3 = 15; // chunk bitshift number * 3; 1<<cBSX3 is cSzCb. the name PURPOSEFULLY references cbs and has a cool X3 face :3
		public Dictionary<Vector3i, ushort[]> bData = []; // for key, should have probably like 16 bits for x, 22 bits for y, 16 bits for z. for val, should probably have 10 bits for type and 6 for faces.
		public Dictionary<Vector3i, int[]> fData = []; // for key, should have probably like 16 bits for x, 22 bits for y, 16 bits for z. for val, should probably have 10 bits for type and 6 for faces.
		public Dictionary<Vector3i, uint> CDT = []; // for key, should have probably like 16 bits for x, 22 bits for y, 16 bits for z. for val, should probably have 10 bits for type and 6 for faces.
		public Shader shader;
		// public static UInt128 DictionaryKey(uint x, uint y, uint z) => ((UInt128)x << 64) + ((UInt128)y << 32) + z;
		// Vector3i[] vdataBF = new Vector3i[Text.BulkDrawConst];
		int[] pos = new int[Text.BulkDrawConst*3];
		public void SetBlock(int x, int y, int z, ushort v)
		{
			Vector3i key1 = (x >> cBSN, y >> cBSN, z >> cBSN);
			(uint X, uint Y, uint Z) = unchecked(((uint)x, (uint)y, (uint)z));
			// uint key2=X-(X>>cBSN<<cBSN)|((Y-(Y>>cBSN<<cBSN))<<cBSN)|((Z-(Z>>cBSN<<cBSN))<<cBS2);
			uint key2=(X&31)|((Y&31)<<cBSN)|((Z&31)<<cBS2);
			if (!bData.TryGetValue(key1, out ushort[] value)) {
				value = new ushort[cSzCb];
				bData[key1] = value;
				// fData[key1] = new ulong[cSzCb/8];
				fData[key1] = new int[cSzCb*3];
				CDT[key1] = 0;
			}
			// try {value[key2] = v;} catch { throw new Exception("bruh anyways " + x + "," + y + "," + z + "," + v + ", " + key1 + ", " + key2); }
			// if (v != value.v[key2]) { value.v[key2] = v; CDT[key1][key2>>5] = 1u<<(int)(key2&31); }
			if (v != value[key2]) { value[key2] = v; CDT[key1] = 1u; }
		}
		public MiningGame() { }
		int cubeVAO, cubeVBO, instanceVBO;
		static float[] cubeVerts = [
			0,0,1, 0,15f/128,  1,0,1, 1f/128,15f/128,  0,1,1, 0,16f/128,   1,0,1, 1f/128,15f/128,  1,1,1, 1f/128,16f/128,  0,1,1, 0,16f/128, // front
			0,0,0, 0,15f/128,  0,1,0, 0,16f/128,  1,0,0, 1f/128,15f/128,   1,0,0, 1f/128,15f/128,  0,1,0, 0,16f/128,  1,1,0, 1f/128,16f/128, // back
			1,0,0, 0,15f/128,  1,1,0, 0,16f/128,  1,0,1, 1f/128,15f/128,   1,0,1, 1f/128,15f/128,  1,1,0, 0,16f/128,  1,1,1, 1f/128,16f/128, // right
			0,0,0, 1f/128,15f/128,  0,0,1, 0,15f/128,  0,1,0, 1f/128,16f/128,   0,0,1, 0,15f/128,  0,1,1, 0,16f/128,  0,1,0, 1f/128,16f/128, // left
			0,1,1, 0,16f/128,  0,1,0, 0,15f/128,  1,1,1, 1f/128,16f/128,   1,1,1, 1f/128,16f/128,  0,1,0, 0,15f/128,  1,1,0, 1f/128,15f/128, // top
			0,0,1, 0,15f/128,  1,0,1, 1f/128,15f/128,  0,0,0, 0,16f/128,   1,0,1, 1f/128,15f/128,  1,0,0, 1f/128,16f/128,  0,0,0, 0,16f/128, // bottom
		];
		// private void UpdateBlocks()
		// {
		// 	Vector3i[] translations = new Vector3i[blockData.Count];
		// 	int i = 0;
		// 	foreach (KeyValuePair<UInt128, uint> thingy in blockData) {
		// 		if (thingy.Value == 0) continue;
		// 		translations[i] = new Vector3i((int)(thingy.Key >> 64), (int)(thingy.Key >> 32), (int)thingy.Key);
		// 		i++;
		// 	}
		// 	translations = translations[..i];
		// 	GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
		// 	renderingAmount = translations.Length;
		// 	GL.BufferSubData(BufferTarget.ArrayBuffer, 0, renderingAmount * 3, translations);

		// }
		private void GenDefaultWorld() {
			const int r = 18;
			int x, y, z, innerCircle, circleAmount;
			for (x = 0; x < r; x++) {
				circleAmount = (int)MathF.Sqrt(r * r - x * x);
				for (z = 0; z < circleAmount; z++)
					for (y = 0; y < 100; y += 17){
						SetBlock(x,y,z,1);SetBlock(-x,y,z,1);SetBlock(x,y,-z,1);SetBlock(-x,y,-z,1);}}
			for (x = 0; x < r; x++) {
				circleAmount = (int)MathF.Sqrt(r * r - x * x);
				for (y = 0; y < circleAmount; y++){
					int circleAmount2 = (int)MathF.Sqrt(r * r - x * x - y * y);
					for (z = 0; z < circleAmount2; z++){
						SetBlock(50+x, y, z,1);
						SetBlock(50-x, y, z,1);
						SetBlock(50+x, y,-z,1);
						SetBlock(50-x, y,-z,1);
						SetBlock(50+x,-y, z,1);
						SetBlock(50-x,-y, z,1);
						SetBlock(50+x,-y,-z,1);
						SetBlock(50-x,-y,-z,1);}}}
			for (x = 0; x < 75; x++) {
				circleAmount = (int)MathF.Sqrt(75 * 75 - x * x);
				int px = 200 + x;
				int nx = 200 - x;
				for (y = 0; y < circleAmount; y++){
					int circleAmount2 = (int)MathF.Sqrt(75 * 75 - x * x - y * y);
					for (z = 0; z < circleAmount2; z++){
						SetBlock(px, y, z,1);
						SetBlock(nx, y, z,1);
						SetBlock(px, y,-z,1);
						SetBlock(nx, y,-z,1);
						SetBlock(px,-y, z,1);
						SetBlock(nx,-y, z,1);
						SetBlock(px,-y,-z,1);
						SetBlock(nx,-y,-z,1);}}}
			long bruh = Stopwatch.GetTimestamp();
			const int r2 = 53; float rsq=r2*r2;float inRSQ=(r2-8f)*(r2-8f);float temp;
			for (x = r2; x > 0; x--) {
				int xsq = x * x;int px = x + 50;int nx = -x + 50; // "positive" x and "negative" x
				circleAmount = (int)MathF.Sqrt(rsq - xsq);
				for (y = circleAmount; y > 0; y--) {
					int ysqmxsq = -(y * y + xsq); // y^2 plus x^2
					int circleAmount2 = (int)MathF.Sqrt(rsq - ysqmxsq);
					temp = MathF.Sqrt(inRSQ - ysqmxsq);
					innerCircle = float.IsNaN(temp) ? 0 : (int)temp;
					for (z = circleAmount2; z > innerCircle; z--) {
						SetBlock(px, y, z,1);SetBlock(nx, y, z,1);SetBlock(px,-y, z,1);SetBlock(nx,-y, z,1);
						SetBlock(px, y,-z,1);SetBlock(nx, y,-z,1);SetBlock(px,-y,-z,1);SetBlock(nx,-y,-z,1);}
					if (innerCircle == 0)
					{SetBlock(px,y,0,1);SetBlock(nx,y,0,1);SetBlock(px,-y,0,1);SetBlock(nx,-y,0,1);}}
				temp = MathF.Sqrt(inRSQ - xsq);
				innerCircle = float.IsNaN(temp) ? 0 : (int)temp;
				for (z = circleAmount; z > innerCircle; z--) {
					SetBlock(px,0,z,1);SetBlock(nx,0,z,1);SetBlock(px,0,-z,1);SetBlock(nx,0,-z,1);}
				SetBlock(px, 0, 0, 1); SetBlock(nx, 0, 0, 1);}
			circleAmount = (int)r2;
			for(y=circleAmount;y>0;y--){int ysq=y*y;int circleAmount2=(int)MathF.Sqrt(rsq-ysq);temp=MathF.Sqrt(inRSQ - ysq);
				innerCircle=float.IsNaN(temp)?0:(int)temp;
				for(z=circleAmount2;z>innerCircle;z--){SetBlock(50,y,z,1);SetBlock(50,-y,z,1);SetBlock(50,y,-z,1);SetBlock(50,-y,-z,1);}
				SetBlock(50, y, 0, 1); SetBlock(50, -y, 0, 1);}
			for(z=circleAmount;z>inRSQ;z--){ SetBlock(50, 0, z, 1); SetBlock(50, 0, -z, 1);}SetBlock(50, 0, 0, 1);
			Console.WriteLine("sphere gen took " + Stopwatch.GetElapsedTime(bruh).TotalMilliseconds + "ms.");}
		public override void OnLoad(Game game) {
			base.OnLoad(game);
			game._player.IsFlying = true;
			game._camera.MaxDist = 1024;
			shader = new("Shaders/miningGame/shader.vert", "Shaders/miningGame/shader.frag");
			GL.UseProgram(shader.Handle);

			GenDefaultWorld();

			cubeVAO = GL.GenVertexArray();
			GL.BindVertexArray(cubeVAO);

			cubeVBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, cubeVBO);
			GL.BufferData(BufferTarget.ArrayBuffer, cubeVerts.Length * sizeof(float), cubeVerts, BufferUsageHint.StaticDraw);

			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
			GL.EnableVertexAttribArray(1);
			GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

			instanceVBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
			GL.BufferData(BufferTarget.ArrayBuffer, sizeof(int) * Text.BulkDrawConst * 3, pos, BufferUsageHint.DynamicDraw);

			GL.EnableVertexAttribArray(2);
			GL.VertexAttribIPointer(2, 3, VertexAttribIntegerType.Int, 3 * sizeof(int), 0);
			GL.VertexAttribDivisor(2, 1);
		}
		public override void OnRenderFrame(Game game, double dt) {// 1154
			base.OnRenderFrame(game, dt);
			shader.Use();
			// Vector3 up = game._camera.Up;
			// assume up is 0,1,0
			(float vX,float vY,float vZ)=game._camera.Position-game._camera.Target;float n=1f/MathF.Sqrt(vX*vX+vY*vY+vZ*vZ);vX*=n;vY*=n;vZ*=n;
			n=1f/MathF.Sqrt(vZ*vZ+vX*vX);(float rX,float rZ)=(vZ*n,-vX*n);
			(float v2X,float v2Y,float v2Z)=(vY*rZ,vZ*rX-vX*rZ,-vY*rX);
			n=1f/MathF.Sqrt(v2X*v2X+v2Y*v2Y+v2Z*v2Z);v2X*=n;v2Y*=n;v2Z*=n;
			(float eX,float eY,float eZ)=game._camera.Position;Matrix4 r=game._camera.Projection;
			float x4=-rX*eX-rZ*eZ;float y4=-v2X*eX-v2Y*eY-v2Z*eZ;float z4=-vX*eX-vY*eY-vZ*eZ;
			(float x5,float y5,float z5,float w5,float x6,float y6,float z6,float w6,float x7,float y7,float z7,float w7,float x8,float y8,float z8,float w8)
			=(r.Row0.X,r.Row0.Y,r.Row0.Z,r.Row0.W,r.Row1.X,r.Row1.Y,r.Row1.Z,r.Row1.W,r.Row2.X,r.Row2.Y,r.Row2.Z,r.Row2.W,r.Row3.X,r.Row3.Y,r.Row3.Z,r.Row3.W);
			shader.SetMatrix4("view", new(new(rX*x5+v2X*x6+vX*x7,rX*y5+v2X*y6+vX*y7,rX*z5+v2X*z6+vX*z7,rX*w5+v2X*w6+vX*w7),
			new(v2Y*x6+vY*x7,v2Y*y6+vY*y7,v2Y*z6+vY*z7,v2Y*w6+vY*w7),
			new(rZ*x5+v2Z*x6+vZ*x7,rZ*y5+v2Z*y6+vZ*y7,rZ*z5+v2Z*z6+vZ*z7,rZ*w5+v2Z*w6+vZ*w7),
			new(x4*x5+y4*x6+z4*x7+x8,x4*y5+y4*y6+z4*y7+y8,x4*z5+y4*z6+z4*z7+z8,x4*w5+y4*w6+z4*w7+w8)));
			// int location = GL.GetUniformLocation(shader.Handle, "texOffset");
			// GL.Uniform2(location, Vector2.Zero);
			// shader.SetMatrix4("projection", game._camera.Projection);
			GL.BindVertexArray(cubeVAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
			int i = 0;
			// Vector3i[] pos = new Vector3i[Text.BulkDrawConst];
#if MINING_GAME_PROFILELOG
			int test = 0;
			long sdfjkl0, sdfjkl, sdfjkl1, sdfjkl2, sdfjkl3, sdfjkl4, sdfjklsjd = Stopwatch.GetTimestamp();
			double testt = 0, testt2 = 0, testt3 = 0, testt4, testt5 = 0, testt6 = 0, testt10 = 0, nonrendertime = 0;
#endif
			foreach (KeyValuePair<Vector3i, ushort[]> d in bData) {
				ushort[] B = d.Value;
				// ulong[] F = fData[d.Key];
				ulong[] F = new ulong[cSzCb >> 3];
				int[] realF = fData[d.Key];
#if MINING_GAME_PROFILELOG
				sdfjkl0 = Stopwatch.GetTimestamp();
#endif
				(int chunkPosX, int chunkPosY, int chunkPosZ) = d.Key;
				(int basePosX, int basePosY, int basePosZ) = (chunkPosX << cBSN, chunkPosY << cBSN, chunkPosZ << cBSN);
				// Console.WriteLine("1184lengths: " + _d.Length + ", " + _f.Length);
				int j, ind, tmp2, j1, k, j2, j1xcs, j1xcssq, _i = 0;
				uint tmp;
#if MINING_GAME_PER_FACE_CULL
				uint tmp1;
#endif
				// Console.WriteLine("1185");
#if MINING_GAME_PROFILELOG
				sdfjkl = Stopwatch.GetTimestamp();
#endif
				if (CDT[d.Key] > 0) {
					Array.Fill(F, 0ul);
					ushort[] ocfd = new ushort[cSzSq * 6]; // other chunk face deeta
#if MINING_GAME_PROFILELOG
					test++;
					testt10 += Stopwatch.GetElapsedTime(sdfjkl).TotalMilliseconds;
					sdfjkl1 = Stopwatch.GetTimestamp();
#endif
					{
						if (bData.TryGetValue((chunkPosX, chunkPosY, chunkPosZ - 1), out ushort[] _B)) {
							Array.Copy(_B, cSzCb-cSzSq, ocfd, cSzSq*4, cSzSq); }
						if (bData.TryGetValue((chunkPosX, chunkPosY, chunkPosZ + 1), out _B)) {
							Array.Copy(_B, 0, ocfd, cSzSq*5, cSzSq); }
						int Y1, Y2;
						if (bData.TryGetValue((chunkPosX - 1, chunkPosY, chunkPosZ), out _B)) {
							for(Y2=0;Y2<cSzCb;Y2+=cSzSq){Y1=Y2>>cBSN;
								ocfd[Y1]=_B[Y2|31];ocfd[Y1|1]=_B[Y2|63];ocfd[Y1|2]=_B[Y2|95];ocfd[Y1|3]=_B[Y2|127];
								ocfd[Y1|4]=_B[Y2|159];ocfd[Y1|5]=_B[Y2|191];ocfd[Y1|6]=_B[Y2|223];ocfd[Y1|7]=_B[Y2|255];
								ocfd[Y1|8]=_B[Y2|287];ocfd[Y1|9]=_B[Y2|319];ocfd[Y1|10]=_B[Y2|351];ocfd[Y1|11]=_B[Y2|383];
								ocfd[Y1|12]=_B[Y2|415];ocfd[Y1|13]=_B[Y2|447];ocfd[Y1|14]=_B[Y2|479];ocfd[Y1|15]=_B[Y2|511];
								ocfd[Y1|16]=_B[Y2|543];ocfd[Y1|17]=_B[Y2|575];ocfd[Y1|18]=_B[Y2|607];ocfd[Y1|19]=_B[Y2|639];
								ocfd[Y1|20]=_B[Y2|671];ocfd[Y1|21]=_B[Y2|703];ocfd[Y1|22]=_B[Y2|735];ocfd[Y1|23]=_B[Y2|767];
								ocfd[Y1|24]=_B[Y2|799];ocfd[Y1|25]=_B[Y2|831];ocfd[Y1|26]=_B[Y2|863];ocfd[Y1|27]=_B[Y2|895];
								ocfd[Y1|28]=_B[Y2|927];ocfd[Y1|29]=_B[Y2|959];ocfd[Y1|30]=_B[Y2|991];ocfd[Y1|31]=_B[Y2|1023];}}
						if (bData.TryGetValue((chunkPosX + 1, chunkPosY, chunkPosZ), out _B)) {
							// for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;
							// 	YP = cSzSq|YB1;
							for(Y2=0;Y2<cSzCb;Y2+=cSzSq){Y1=cSzSq|(Y2>>cBSN);
								ocfd[Y1]=_B[Y2];ocfd[Y1|1]=_B[Y2|32];ocfd[Y1|2]=_B[Y2|64];ocfd[Y1|3]=_B[Y2|96];
								ocfd[Y1|4]=_B[Y2|128];ocfd[Y1|5]=_B[Y2|160];ocfd[Y1|6]=_B[Y2|192];ocfd[Y1|7]=_B[Y2|224];
								ocfd[Y1|8]=_B[Y2|256];ocfd[Y1|9]=_B[Y2|288];ocfd[Y1|10]=_B[Y2|320];ocfd[Y1|11]=_B[Y2|352];
								ocfd[Y1|12]=_B[Y2|384];ocfd[Y1|13]=_B[Y2|416];ocfd[Y1|14]=_B[Y2|448];ocfd[Y1|15]=_B[Y2|480];
								ocfd[Y1|16]=_B[Y2|512];ocfd[Y1|17]=_B[Y2|544];ocfd[Y1|18]=_B[Y2|576];ocfd[Y1|19]=_B[Y2|608];
								ocfd[Y1|20]=_B[Y2|640];ocfd[Y1|21]=_B[Y2|672];ocfd[Y1|22]=_B[Y2|704];ocfd[Y1|23]=_B[Y2|736];
								ocfd[Y1|24]=_B[Y2|768];ocfd[Y1|25]=_B[Y2|800];ocfd[Y1|26]=_B[Y2|832];ocfd[Y1|27]=_B[Y2|864];
								ocfd[Y1|28]=_B[Y2|896];ocfd[Y1|29]=_B[Y2|928];ocfd[Y1|30]=_B[Y2|960];ocfd[Y1|31]=_B[Y2|992];}}
						if (bData.TryGetValue((chunkPosX, chunkPosY - 1, chunkPosZ), out _B)) {
							// for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;
							// 	YP = cSzSq*2|YB1;
							for(Y2=0;Y2<cSzCb;Y2+=cSzSq){Y1=(cSzSq*2)|(Y2>>cBSN);
								ocfd[Y1]=_B[Y2|992];ocfd[Y1|1]=_B[Y2|993];ocfd[Y1|2]=_B[Y2|994];ocfd[Y1|3]=_B[Y2|995];
								ocfd[Y1|4]=_B[Y2|996];ocfd[Y1|5]=_B[Y2|997];ocfd[Y1|6]=_B[Y2|998];ocfd[Y1|7]=_B[Y2|999];
								ocfd[Y1|8]=_B[Y2|1000];ocfd[Y1|9]=_B[Y2|1001];ocfd[Y1|10]=_B[Y2|1002];ocfd[Y1|11]=_B[Y2|1003];
								ocfd[Y1|12]=_B[Y2|1004];ocfd[Y1|13]=_B[Y2|1005];ocfd[Y1|14]=_B[Y2|1006];ocfd[Y1|15]=_B[Y2|1007];
								ocfd[Y1|16]=_B[Y2|1008];ocfd[Y1|17]=_B[Y2|1009];ocfd[Y1|18]=_B[Y2|1010];ocfd[Y1|19]=_B[Y2|1011];
								ocfd[Y1|20]=_B[Y2|1012];ocfd[Y1|21]=_B[Y2|1013];ocfd[Y1|22]=_B[Y2|1014];ocfd[Y1|23]=_B[Y2|1015];
								ocfd[Y1|24]=_B[Y2|1016];ocfd[Y1|25]=_B[Y2|1017];ocfd[Y1|26]=_B[Y2|1018];ocfd[Y1|27]=_B[Y2|1019];
								ocfd[Y1|28]=_B[Y2|1020];ocfd[Y1|29]=_B[Y2|1021];ocfd[Y1|30]=_B[Y2|1022];ocfd[Y1|31]=_B[Y2|1023];}}
						if (bData.TryGetValue((chunkPosX, chunkPosY + 1, chunkPosZ), out _B)) {
							// for(YB1=0;YB1<cSzSq;YB1+=cSz){YB2=YB1<<cBSN;
							// 	YP = cSzSq*3|YB1;
							for(Y2=0;Y2<cSzCb;Y2+=cSzSq){Y1=(cSzSq*3)|(Y2>>cBSN);
								ocfd[Y1]=_B[Y2];ocfd[Y1|1]=_B[Y2|1];ocfd[Y1|2]=_B[Y2|2];ocfd[Y1|3]=_B[Y2|3];
								ocfd[Y1|4]=_B[Y2|4];ocfd[Y1|5]=_B[Y2|5];ocfd[Y1|6]=_B[Y2|6];ocfd[Y1|7]=_B[Y2|7];
								ocfd[Y1|8]=_B[Y2|8];ocfd[Y1|9]=_B[Y2|9];ocfd[Y1|10]=_B[Y2|10];ocfd[Y1|11]=_B[Y2|11];
								ocfd[Y1|12]=_B[Y2|12];ocfd[Y1|13]=_B[Y2|13];ocfd[Y1|14]=_B[Y2|14];ocfd[Y1|15]=_B[Y2|15];
								ocfd[Y1|16]=_B[Y2|16];ocfd[Y1|17]=_B[Y2|17];ocfd[Y1|18]=_B[Y2|18];ocfd[Y1|19]=_B[Y2|19];
								ocfd[Y1|20]=_B[Y2|20];ocfd[Y1|21]=_B[Y2|21];ocfd[Y1|22]=_B[Y2|22];ocfd[Y1|23]=_B[Y2|23];
								ocfd[Y1|24]=_B[Y2|24];ocfd[Y1|25]=_B[Y2|25];ocfd[Y1|26]=_B[Y2|26];ocfd[Y1|27]=_B[Y2|27];
								ocfd[Y1|28]=_B[Y2|28];ocfd[Y1|29]=_B[Y2|29];ocfd[Y1|30]=_B[Y2|30];ocfd[Y1|31]=_B[Y2|31];}}}
					// woah this probably belongs in horriblecode.cs
#if MINING_GAME_PROFILELOG
					testt += Stopwatch.GetElapsedTime(sdfjkl1).TotalMicroseconds;
					sdfjkl2 = Stopwatch.GetTimestamp();
#endif
					for (j1=0,k=0;j1<cSz;j1++){
						j1xcs=j1<<cBSN;
						j1xcssq=j1<<cBS2;
						for(j2=0;j2<cSz;j2++,k+=cSz,_i++){
							tmp=(B[k]==0?0:1u)|(B[k|1]==0?0:2u)|(B[k|2]==0?0:4u)|(B[k|3]==0?0:8u)|(B[k|4]==0?0:0x10u)|(B[k|5]==0?0:0x20u)|(B[k|6]==0?0:0x40u)|
							(B[k|7]==0?0:0x80u)|(B[k|8]==0?0:0x100u)|(B[k|9]==0?0:0x200u)|(B[k|10]==0?0:0x400u)|(B[k|11]==0?0:0x800u)|(B[k|12]==0?0:0x1000u)|
							(B[k|13]==0?0:0x2000u)|(B[k|14]==0?0:0x4000u)|(B[k|15]==0?0:0x8000u)|(B[k|16]==0?0:0x10000u)|(B[k|17]==0?0:0x20000u)|(B[k|18]==0?0:0x40000u)|
							(B[k|19]==0?0:0x80000u)|(B[k|20]==0?0:0x100000u)|(B[k|21]==0?0:0x200000u)|(B[k|22]==0?0:0x400000u)|(B[k|23]==0?0:0x800000u)|
							(B[k|24]==0?0:0x1000000u)|(B[k|25]==0?0:0x2000000u)|(B[k|26]==0?0:0x4000000u)|(B[k|27]==0?0:0x8000000u)|(B[k|28]==0?0:0x10000000u)|
							(B[k|29]==0?0:0x20000000u)|(B[k|30]==0?0:0x40000000u)|(B[k|31]==0?0:0x80000000);
							//tmp=(tmp&~(tmp<<1))|(tmp&~(tmp>>1));
							ind=(j2<<cBSN)+j1xcssq;
#if MINING_GAME_PER_FACE_CULL
							tmp1=tmp&~((tmp<<1)|(ocfd[_i]==0?0:1u));
							tmp&=~((tmp>>1)|(ocfd[_i|cSzSq]==0?0:0x80000000));
							//ind=(y<<cBSN)+(z<<cBS2);
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);
							while(tmp2<32) {
								F[(ind|tmp2)>>3]|=0b00000010ul<<((tmp2&7)<<3);
								tmp^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp); }
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp1);
							while(tmp2<32) {
								F[(ind|tmp2)>>3]|=0b00000001ul<<((tmp2&7)<<3); // BRUH ONE CHARACTER MISSING LOL (THE L CHARACTER)
								tmp1^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp1); }
#else
							tmp&=~((tmp<<1)|(tmp>>1)|(ocfd[_i]==0?0:1u)|(ocfd[_i|cSzSq]==0?0:0x80000000));
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);
							while(tmp2<32) {
								F[(ind|tmp2)>>3]|=0b00111111ul<<((tmp2&7)<<3);
								tmp^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp); }
#endif
							int irs3 = _i >> 3; // i right shifted by 3. INTERNAL REVENUE SERVICE MENTION!?!?1?!?!1?!1?!!?1?!1?3991680010395?!1!?1?10395?!?!1?1039511?
							tmp=(B[_i]==0?0:1u)|(B[_i|cSzSq]==0?0:2u)|(B[_i|(cSzSq*2)]==0?0:4u)|(B[_i|(cSzSq*3)]==0?0:8u)|(B[_i|(cSzSq*4)]==0?0:16u)|(B[_i|(cSzSq*5)]==0?0:32u)|(B[_i|(cSzSq*6)]==0?0:64u)|
							(B[_i|(cSzSq*7)]==0?0:128u)|(B[_i|(cSzSq*8)]==0?0:256u)|(B[_i|(cSzSq*9)]==0?0:512u)|(B[_i|(cSzSq*10)]==0?0:1024u)|(B[_i|(cSzSq*11)]==0?0:2048u)|(B[_i|(cSzSq*12)]==0?0:4096u)|
							(B[_i|(cSzSq*13)]==0?0:8192u)|(B[_i|(cSzSq*14)]==0?0:16384u)|(B[_i|(cSzSq*15)]==0?0:32768u)|(B[_i|(cSzSq*16)]==0?0:65536u)|(B[_i|(cSzSq*17)]==0?0:131072u)|(B[_i|(cSzSq*18)]==0?0:262144u)|
							(B[_i|(cSzSq*19)]==0?0:524288u)|(B[_i|(cSzSq*20)]==0?0:1048576u)|(B[_i|(cSzSq*21)]==0?0:2097152u)|(B[_i|(cSzSq*22)]==0?0:4194304u)|(B[_i|(cSzSq*23)]==0?0:8388608u)|
							(B[_i|(cSzSq*24)]==0?0:16777216u)|(B[_i|(cSzSq*25)]==0?0:33554432u)|(B[_i|(cSzSq*26)]==0?0:67108864u)|(B[_i|(cSzSq*27)]==0?0:0x8000000u)|(B[_i|(cSzSq*28)]==0?0:0x10000000u)|
							(B[_i|(cSzSq*29)]==0?0:0x20000000u)|(B[_i|(cSzSq*30)]==0?0:0x40000000u)|(B[_i|(cSzSq*31)]==0?0:0x80000000);
#if MINING_GAME_PER_FACE_CULL
							ulong bsamtish = 0b00100000ul<<((_i&7)<<3); // bitshift amount ish
							// tmp=(tmp&~(tmp<<1))|(tmp&~(tmp>>1));
							// tmp&=~(tmp<<1)|~(tmp>>1);
							// tmp&=~((tmp<<1)&(tmp>>1));
							tmp1=tmp&~((tmp<<1)|(ocfd[cSzSq*4|_i]==0?0:1u));
							tmp&=~((tmp>>1)|(ocfd[cSzSq*5|_i]==0?0:0x80000000));
							// x = basePosX | j2;
							// y = basePosY | j1;
							// ind = x | (y<<cBSN);
							// ind = j2 | j1xcs;
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);
							// while (tmp2 < 32) {
							// 	// vdataBF[i] = (x, y, basePosZ+tmp2);
							// 	_f[ind | tmp2<<cBS2] |= 0b00110000;
							// 	tmp ^= 1u<<tmp2;
							// 	tmp2 = System.Numerics.BitOperations.TrailingZeroCount(tmp); }
							while(tmp2<32){
								// vdataBF[i] = (basePosX + tmp2, y, z);
								F[irs3|(tmp2<<(cBS2-3))]|=bsamtish;
								tmp^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);}
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp1);
							bsamtish >>= 1;
							while(tmp2<32){
								// vdataBF[i] = (basePosX + tmp2, y, z);
								F[irs3|(tmp2<<(cBS2-3))]|=bsamtish;
								tmp1^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp1);}
#else
							ulong bsamtish = 0b00111111ul<<((_i&7)<<3); // bitshift amount ish
							tmp&=~((tmp<<1)|(tmp>>1)|(ocfd[cSzSq*4|_i]==0?0:1u)|(ocfd[cSzSq*5|_i]==0?0:0x80000000));
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);
							while(tmp2<32){
								// vdataBF[i] = (basePosX + tmp2, y, z);
								F[irs3|(tmp2<<(cBS2-3))]|=bsamtish;
								tmp^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);}
#endif
							j=j1xcssq|j2;
							tmp=(B[j]==0?0:1u)|(B[j|cSz]==0?0:2u)|(B[j|cSz*2]==0?0:4u)|(B[j|cSz*3]==0?0:8u)|(B[j|cSz*4]==0?0:16u)|(B[j|cSz*5]==0?0:32u)|(B[j|cSz*6]==0?0:64u)|
							(B[j|cSz*7]==0?0:128u)|(B[j|cSz*8]==0?0:256u)|(B[j|cSz*9]==0?0:512u)|(B[j|cSz*10]==0?0:1024u)|(B[j|cSz*11]==0?0:2048u)|(B[j|cSz*12]==0?0:4096u)|
							(B[j|cSz*13]==0?0:8192u)|(B[j|cSz*14]==0?0:16384u)|(B[j|cSz*15]==0?0:32768u)|(B[j|cSz*16]==0?0:65536u)|(B[j|cSz*17]==0?0:131072u)|(B[j|cSz*18]==0?0:262144u)|
							(B[j|cSz*19]==0?0:524288u)|(B[j|cSz*20]==0?0:1048576u)|(B[j|cSz*21]==0?0:2097152u)|(B[j|cSz*22]==0?0:4194304u)|(B[j|cSz*23]==0?0:8388608u)|
							(B[j|cSz*24]==0?0:16777216u)|(B[j|cSz*25]==0?0:33554432u)|(B[j|cSz*26]==0?0:67108864u)|(B[j|cSz*27]==0?0:0x8000000u)|(B[j|cSz*28]==0?0:0x10000000u)|
							(B[j|cSz*29]==0?0:0x20000000u)|(B[j|cSz*30]==0?0:0x40000000u)|(B[j|cSz*31]==0?0:0x80000000);
							irs3 = (j2|j1xcssq) >> 3; // i right shifted by 3. INTERNAL REVENUE SERVICE MENTION!?!?1?!?!1?!1?!!?1?!1?3991680010395?!1!?1?10395?!?!1?1039511?
#if MINING_GAME_PER_FACE_CULL
							tmp1=tmp&~((tmp<<1)|(ocfd[cSzSq*2|_i]==0?0:1u));
							tmp&=~((tmp>>1)|(ocfd[cSzSq*3|_i]==0?0:0x80000000));
							// ind=j2|j1xcssq;
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);
							bsamtish >>= 1;
							while(tmp2<32){
								F[irs3|(tmp2<<(cBSN-3))]|=bsamtish;
								tmp^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);}
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp1);
							bsamtish >>= 1;
							while(tmp2<32){
								F[irs3|(tmp2<<(cBSN-3))]|=bsamtish;
								tmp1^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp1);}
#else
							tmp&=~((tmp<<1)|(tmp>>1)|(ocfd[cSzSq*3|_i]==0?0:0x80000000)|(ocfd[cSzSq*2|_i]==0?0:1u));
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);
							while(tmp2<32){
								F[irs3|(tmp2<<(cBSN-3))]|=bsamtish;
								tmp^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);}
#endif
					}}
					CDT[d.Key] = 0;
#if MINING_GAME_PROFILELOG
					testt3 +=Stopwatch.GetElapsedTime(sdfjkl2).TotalMicroseconds;
					testt2+=Stopwatch.GetElapsedTime(sdfjkl).TotalMicroseconds;
					/*Console.WriteLine("culling took " + Stopwatch.GetElapsedTime(asdasda).TotalMilliseconds + "ms");*/
					sdfjkl3 = Stopwatch.GetTimestamp();
#endif
					ulong a;
					j = 0;
					int y, z;
					// realF = fData[d.Key];
					// (ulong rx, ulong ry, ulong rz) = ((basePosX>0?(ulong)basePosX:(ulong)(basePosX+1048576))<<43,(basePosY>0?(ulong)basePosY:(ulong)(basePosY+2097152))<<21,basePosZ>0?(ulong)basePosZ:(ulong)(basePosZ+1048576));
					// (uint rx, uint ry, uint rz) = ((uint)basePosX,(uint)basePosY,(uint)basePosZ);
					int count = 0;
					for (z=basePosZ;z<basePosZ+cSz;z++){
						for(y=basePosY;y<basePosY+cSz;y++,j+=4){
							a=F[j];if(a>0){if((a&255)>0){realF[count]=basePosX;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF00)>0){realF[count]=basePosX|1;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF0000)>0){realF[count]=basePosX|2;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF000000)>0){realF[count]=basePosX|3;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF00000000)>0){realF[count]=basePosX|4;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF0000000000)>0){realF[count]=basePosX|5;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF000000000000)>0){realF[count]=basePosX|6;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a>>56)>0){realF[count]=basePosX|7;realF[count+1]=y;realF[count+2]=z;count+=3;}}
							a=F[j|1];if(a>0){if((a&255)>0){realF[count]=basePosX|8;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF00)>0){realF[count]=basePosX|9;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF0000)>0){realF[count]=basePosX|10;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF000000)>0){realF[count]=basePosX|11;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF00000000)>0){realF[count]=basePosX|12;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF0000000000)>0){realF[count]=basePosX|13;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF000000000000)>0){realF[count]=basePosX|14;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a>>56)>0){realF[count]=basePosX|15;realF[count+1]=y;realF[count+2]=z;count+=3;}}
							a=F[j|2];if(a>0){if((a&255)>0){realF[count]=basePosX|16;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF00)>0){realF[count]=basePosX|17;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF0000)>0){realF[count]=basePosX|18;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF000000)>0){realF[count]=basePosX|19;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF00000000)>0){realF[count]=basePosX|20;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF0000000000)>0){realF[count]=basePosX|21;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF000000000000)>0){realF[count]=basePosX|22;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a>>56)>0){realF[count]=basePosX|23;realF[count+1]=y;realF[count+2]=z;count+=3;}}
							a=F[j|3];if(a>0){if((a&255)>0){realF[count]=basePosX|24;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF00)>0){realF[count]=basePosX|25;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF0000)>0){realF[count]=basePosX|26;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF000000)>0){realF[count]=basePosX|27;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF00000000)>0){realF[count]=basePosX|28;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a&0xFF0000000000)>0){realF[count]=basePosX|29;realF[count+1]=y;realF[count+2]=z;count+=3;}
								if((a&0xFF000000000000)>0){realF[count]=basePosX|30;realF[count+1]=y;realF[count+2]=z;count+=3;}if((a>>56)>0){realF[count]=basePosX|31;realF[count+1]=y;realF[count+2]=z;count+=3;}} }}
					realF[cSzCb*3-1] = count/3;
#if MINING_GAME_PROFILELOG
					testt5 += Stopwatch.GetElapsedTime(sdfjkl3).TotalMicroseconds; }nonrendertime += Stopwatch.GetElapsedTime(sdfjkl0).TotalMilliseconds;
#else
				}
#endif
					int amt = realF[cSzCb*3-1];
				if (amt > 0){
					if (i+amt > Text.BulkDrawConst) {
#if MINING_GAME_PROFILELOG
						sdfjkl4 = Stopwatch.GetTimestamp();
						Console.WriteLine("1308bruh"+i);
						GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(int)*3*i, pos);
						GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, i); i = 0;
						testt6 += Stopwatch.GetElapsedTime(sdfjkl4).TotalMilliseconds;
#else
						GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(int)*3*i, pos);
						GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, i); i = 0;
#endif
					}// realF.CopyTo(pos, i);
					Array.Copy(realF, 0, pos, i*3, amt*3);
					i += amt;}}
#if MINING_GAME_PROFILELOG
			// Console.WriteLine("1313:" + i);
			Console.WriteLine("1493," + i);if (i > 0) {
				sdfjkl4 = Stopwatch.GetTimestamp();
				// Console.WriteLine("1316bruh"+i);
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(int)*3*i, pos);
				GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, i);
				testt6 += Stopwatch.GetElapsedTime(sdfjkl4).TotalMilliseconds;}
			testt4 = Stopwatch.GetElapsedTime(sdfjklsjd).TotalMilliseconds;
			Console.WriteLine("updated " + test + " chunks/"+bData.Count+". took " + testt10 + "ms to fill," + testt + "μs for other side loading; " + (testt / test) + "μsper for othersides, "+(testt10/test)+"msper for fill. chunkasm "+testt3+"μs;"+(testt3/test)+"μsper; full asm time:"+testt2+"μs,"+(testt2/test)+"μsper. total time was "+testt4+"ms;rendertime(?):"+testt6+"ms.(that's "+(100*testt6/testt4)+"%.)datagathering:"+testt5+"μs,"+(testt5/bData.Count)+"μsper. nonrender time: "+nonrendertime+"ms (that's "+(100*nonrendertime/testt4)+"%.)");
#else
			if (i > 0) {
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(int)*3*i, pos);
				GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, i);
			}
#endif
		}// 1434
		public override void OnUpdateFrame(Game game, double dt)
		{
			base.OnUpdateFrame(game, dt);
			(int x, int y, int z) = (Vector3i)game._player.RootPosition;
			x >>= 2; y = -(y-2)>>2; z >>= 2;
			if (game.MouseState[MouseButton.Right]) {
				SetBlock(x, y, z, 1);
			} else if (game.MouseState[MouseButton.Left]) {
				SetBlock(x, y, z, 0);
			}
		}
		public override void OnKeyDown(KeyboardKeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Key == Keys.L) GL.PolygonMode(MaterialFace.Front, PolygonMode.Point + Random.Shared.Next(3));
		}
		public override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			Console.WriteLine("mining game closing");
#if MINING_GAME_DEBUG
				string write0 = "\n\nyo this mining game is closing i guess\n\n";
				string write1 = "";
				// int skipAmount = bData.Count - 2;
				uint thisVal;
				foreach (var v in bData) {
					// if (skipAmount > 0) { skipAmount--; continue; } // this will probably perform terribly but oh well
					uint currentVal = v.Value[0];
					write0 += v.Key + "; "+currentVal;
					uint repeatCount = 1;
					for (int i = 1; i < cSzCb; i++) {
						if (v.Value[i] == currentVal) repeatCount++;
						else {
							thisVal = v.Value[i];
							if (repeatCount > 1) {write0 += "x" + repeatCount + "," + thisVal; repeatCount = 1;}
							else write0 += "," + thisVal;
							currentVal = thisVal;
							if (write0.Length > 1024) { write1 += write0; write0 = ""; }}}
					if (repeatCount > 1) {write0 += "x" + repeatCount;}
					write0 += "\n";
					Console.Write(write1 + write0);
					write1 = write0 = "";}
				Console.Write("\n\nhas like a lot of voxels, like " + bData.Count + " chunks of them, which is(n't) " + bData.Count * 36 + " verts, or " + bData.Count * 12 + " faces.\n");
#endif
		}}
	public struct MMGChunk
	{
		public short[] data;
	}
	public class Animate : IMinigame
	{
		public static Dictionary<string, Action<Game>> InGameConstructorthings = new() {
			["animate"] = delegate (Game game) {
				game._currentMinigames.Add(new Animate());
				game._gameModes.Add(GameIdentifier);}
		};
		public const string GameIdentifier = "animate";
		// public static void StartInit() {
			
		// }
		long frameamt = 0;
		Mesh _mesh;
		Mesh _mesh2;
		Matrix4 _groundmatrix;
		Texture _textureSheet;
		Shader _shader;
		const float _tickSpeed = 60;
		const float _tickSpeedInv = 1/_tickSpeed;
		public static class DeetaStuff
		{
			public static readonly float[] objv = [
				0,0,0, 0,0, 16/2048f,16/2048f, 16/2048f, 240/2048f,
				1,0,0, 64,0, 16/2048f,16/2048f, 16/2048f, 240/2048f,
				0,0,1, 0,64, 16/2048f,16/2048f, 16/2048f, 240/2048f,
				1,0,1, 64,64, 16/2048f,16/2048f, 16/2048f, 240/2048f,
			];
			public static readonly uint[] obji = [
				0,1,2,1,3,2
			];
			public static readonly float[] bluebgv = [
				0,0,0, 0,0, 2/2048f,2/2048f, 32/2048f, 254/2048f,
				1,0,0, 10,0, 2/2048f,2/2048f, 32/2048f, 254/2048f,
				0,0,1, 3,10, 2/2048f,2/2048f, 32/2048f, 254/2048f,
				1,0,1, 13,10, 2/2048f,2/2048f, 32/2048f, 254/2048f,
			];
			public static readonly uint[] bluebgi = [
				0,1,2,1,3,2
			];
		}
		public class Mesh
		{
			public static int MeshCount;
			public static List<Mesh> Meshes = [];
			public float[] Vertices;
			public uint[] Indices;
			public int MeshIndex;
			public int VertexArrayObject;
			public int VertexBufferObject;
			public int ElementBufferObject;
			public int ICount;
			public Mesh(float[] vertices, uint[] indices, BufferUsageHint VBOHint, BufferUsageHint EBOHint) {
				if (vertices.Clone() is float[] a) Vertices = a; else throw new Exception("huh????? what.");
				if (indices.Clone() is uint[] b) Indices = b; else throw new Exception("huh????? what2.");
				MeshIndex = MeshCount;
				VertexArrayObject = GL.GenVertexArray();
				GL.BindVertexArray(VertexArrayObject);

				VertexBufferObject = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
				GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, VBOHint);

				ICount = indices.Length;
				ElementBufferObject = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
				GL.BufferData(BufferTarget.ElementArrayBuffer, ICount * sizeof(uint), indices, EBOHint);

				GL.EnableVertexAttribArray(0); // position
				GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);
				GL.EnableVertexAttribArray(1); // texture (before mod)
				GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));
				GL.EnableVertexAttribArray(2); // scale after texture mod
				GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 5 * sizeof(float));
				GL.EnableVertexAttribArray(3); // offset after texture mod
				GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 7 * sizeof(float));

				Meshes.Add(this);
				MeshCount++; }
			public void Bind() { GL.BindVertexArray(VertexArrayObject); }
			public void UpdMeshV() {
				GL.BindVertexArray(VertexArrayObject);
				GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, Vertices.Length * sizeof(float), Vertices); }
			public void UpdMeshI() {
				GL.BindVertexArray(VertexArrayObject);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
				GL.BufferSubData(BufferTarget.ElementArrayBuffer, 0, Indices.Length * sizeof(float), Indices); }
			public void UpdMeshVI() {
				GL.BindVertexArray(VertexArrayObject);
				GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, Vertices.Length * sizeof(float), Vertices);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
				GL.BufferSubData(BufferTarget.ElementArrayBuffer, 0, Indices.Length * sizeof(float), Indices); }
			public void UpdMesh(float[] vertices) {
				GL.BindVertexArray(VertexArrayObject);
				GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, vertices.Length * sizeof(float), vertices); }
			public void UpdMesh(uint[] indices) {
				GL.BindVertexArray(VertexArrayObject);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
				GL.BufferSubData(BufferTarget.ElementArrayBuffer, 0, indices.Length * sizeof(float), indices); }
			public void UpdMesh(float[] vertices, uint[] indices) {
				GL.BindVertexArray(VertexArrayObject);
				GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, vertices.Length * sizeof(float), vertices);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
				GL.BufferSubData(BufferTarget.ElementArrayBuffer, 0, indices.Length * sizeof(float), indices); }
			public static void UpdCurrentMesh(float[] vertices) {
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, vertices.Length * sizeof(float), vertices); }
			public static void UpdCurrentMesh(uint[] indices) {
				GL.BufferSubData(BufferTarget.ElementArrayBuffer, 0, indices.Length * sizeof(float), indices); }
			public static void UpdCurrentMesh(float[] vertices, uint[] indices) {
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, vertices.Length * sizeof(float), vertices);
				GL.BufferSubData(BufferTarget.ElementArrayBuffer, 0, indices.Length * sizeof(float), indices); }
			public void Draw(Shader shader, Matrix4 model) {
				shader.SetMatrix4("model", model);
				GL.DrawElements(PrimitiveType.Triangles, ICount, DrawElementsType.UnsignedInt, 0); }
		}
		public static class PlrCam
		{
			public static float Speed = 3f;
			public static bool FirstPerson = true;
			public static bool Freecam = true;
			public static Vector3 CamToTargetOffset = (-1,0,0);
			/// <summary>
			/// so yeah this should be normalized so this is just direction i think?
			/// </summary>
			public static Vector3 CamLookAt = (1,0,0);
			public static Vector3 Right = (0,0,1);
			public static float CamToTargetDist = 3f;
			public static Vector3 Pos = (1,1,1);
			public static Vector3 Up = (0,1,0);
			public static Matrix4 View;
			public static float Pitch;
			public static float Yaw;
			public static float MouseSensitivity = 0.005f;
			public static Matrix4 Projection;
			public static float fovy=45f*DataStuff.D2RConst, aspect=400f/600f, depthNear=.1f, depthFar=10000f;
			public static void UpdProjection()
			{
				float num = MathF.Tan(0.5f * fovy);
				Projection = new(1f / (num * aspect),0f,0f,0f,
					0f,1f / num,0f,0f,
					0f,0f,(depthFar + depthNear) / (depthNear - depthFar),-1f,
					0f,0f,2f * depthFar * depthNear / (depthNear - depthFar),0f);
			}
			static PlrCam() {
				// Vector3 Direction = Vector3.Normalize(CamLookAt);
				// Vector3 Right = Vector3.Normalize(Vector3.Cross(Up, Direction));
				(float x, float x2, float x3) = Right = Vector3.Normalize(Vector3.Cross(Up, CamLookAt));
				// Projection = Matrix4.CreatePerspectiveFieldOfView(45f*DataStuff.D2RConst, 800f / 600f, .1f, 10000f);
				// public static void CreatePerspectiveFieldOfView(float fovy, float aspect, float depthNear, float depthFar, out Matrix4 result){
				//     if (fovy <= 0f || fovy > (float)Math.PI) {throw new ArgumentOutOfRangeException("fovy");}
				//     if (aspect <= 0f) {throw new ArgumentOutOfRangeException("aspect");}
				//     if (depthNear <= 0f) {throw new ArgumentOutOfRangeException("depthNear");}
				//     if (depthFar <= 0f) {throw new ArgumentOutOfRangeException("depthFar");}
				//     float num = depthNear * MathF.Tan(0.5f * fovy);
				//     float num2 = 0f - num;
				//     float left = num2 * aspect;
				//     float right = num * aspect;
				//     CreatePerspectiveOffCenter(left, right, num2, num, depthNear, depthFar, out result);}
				// public static void CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float depthNear, float depthFar, out Matrix4 result){
				//     if (depthNear <= 0f){throw new ArgumentOutOfRangeException("depthNear");}
				//     if (depthFar <= 0f){throw new ArgumentOutOfRangeException("depthFar");}
				//     if (depthNear >= depthFar){throw new ArgumentOutOfRangeException("depthNear");}
				//     float x = 2f * depthNear / (right - left);
				//     float y = 2f * depthNear / (top - bottom);
				//     float x2 = (right + left) / (right - left);
				//     float y2 = (top + bottom) / (top - bottom);
				//     float z = (0f - (depthFar + depthNear)) / (depthFar - depthNear);
				//     float z2 = (0f - 2f * depthFar * depthNear) / (depthFar - depthNear);
				//     result.Row0.X = x;
				//     result.Row0.Y = 0f;
				//     result.Row0.Z = 0f;
				//     result.Row0.W = 0f;
				//     result.Row1.X = 0f;
				//     result.Row1.Y = y;
				//     result.Row1.Z = 0f;
				//     result.Row1.W = 0f;
				//     result.Row2.X = x2;
				//     result.Row2.Y = y2;
				//     result.Row2.Z = z;
				//     result.Row2.W = -1f;
				//     result.Row3.X = 0f;
				//     result.Row3.Y = 0f;
				//     result.Row3.Z = z2;
				//     result.Row3.W = 0f;
				// }
				float num = MathF.Tan(0.5f * fovy);
				// Projection = new(2f * depthNear / (right - left),0f,0f,0f,
				// 	0f,2f * depthNear / (num - num2),0f,0f,
				// 	(right + left) / (right - left),(num + num2) / (num - num2),(depthFar + depthNear) / (depthNear - depthFar),-1f,
				// 	0f,0f,2f * depthFar * depthNear / (depthNear - depthFar),0f);
				Projection = new(1f / (num * aspect),0f,0f,0f,
					0f,1f / num,0f,0f,
					0f,0f,(depthFar + depthNear) / (depthNear - depthFar),-1f,
					0f,0f,2f * depthFar * depthNear / (depthNear - depthFar),0f);
				(float y, float y2, float y3) = Vector3.Normalize(Vector3.Cross(CamLookAt, Right));
				(float z, float z2, float z3) = CamLookAt;
				float x4 = x * Pos.X + x2 * Pos.Y + x3 * Pos.Z;
				float y4 = y * Pos.X + y2 * Pos.Y + y3 * Pos.Z;
				float z4 = z * Pos.X + z2 * Pos.Y + z3 * Pos.Z;
				float x5 = Projection.Row0.X;
				float y6 = Projection.Row1.Y;
				float z7 = Projection.Row2.Z;
				float z8 = Projection.Row3.Z;
				View = new(x * x5,y * y6,z * z7,-z,
				x2 * x5,y2 * y6,z2 * z7,-z2,
				x3 * x5,y3 * y6,z3 * z7,-z3,
				-x4 * x5,-y4 * y6,z8 - z4 * z7,z4);}
			public static void UpdateVectors() {
				(float x, float x2, float x3) = Right = Vector3.Normalize(Vector3.Cross(Up, CamLookAt));
				(float y, float y2, float y3) = Vector3.Normalize(Vector3.Cross(CamLookAt, Right));
				// Vector3.Cross function:
				// result.X = left.Y * right.Z - left.Z * right.Y;
				// result.Y = left.Z * right.X - left.X * right.Z;
				// result.Z = left.X * right.Y - left.Y * right.X;
				// Vector3.Normalize function:
				// float num = 1f / vec.Length;
				// vec.X *= num;
				// vec.Y *= num;
				// vec.Z *= num;
				(float z, float z2, float z3) = CamLookAt;
				float x4 = x * Pos.X + x2 * Pos.Y + x3 * Pos.Z;
				float y4 = y * Pos.X + y2 * Pos.Y + y3 * Pos.Z;
				float z4 = z * Pos.X + z2 * Pos.Y + z3 * Pos.Z;
				float x5 = Projection.Row0.X;
				float y6 = Projection.Row1.Y;
				float z7 = Projection.Row2.Z;
				float z8 = Projection.Row3.Z;
				View = new(x * x5,y * y6,z * z7,-z,
				x2 * x5,y2 * y6,z2 * z7,-z2,
				x3 * x5,y3 * y6,z3 * z7,-z3,
				-x4 * x5,-y4 * y6,z8 - z4 * z7,z4);}}
		public override void OnLoad(Game game)
		{
			base.OnLoad(game);
			game.UpdateFrequency = 60;
			game.renderPlayer = false;
			_mesh = new Mesh(DeetaStuff.objv,DeetaStuff.obji,BufferUsageHint.DynamicDraw,BufferUsageHint.DynamicDraw);
			_mesh2 = new Mesh(DeetaStuff.bluebgv,DeetaStuff.bluebgi,BufferUsageHint.DynamicDraw,BufferUsageHint.DynamicDraw);
			_shader = new Shader("Shaders/animate/Shader.vert", "Shaders/animate/Shader.frag");
			_shader.Use();
			GL.Enable(EnableCap.DepthTest);
			_textureSheet = Texture.LoadFromFile("Textures/texturesheet.png", false, false);
			_textureSheet.Use(TextureUnit.Texture0);
			_shader.SetInt("texture0",0);
			_groundmatrix = Matrix4.CreateScale(1024,1,1024) * Matrix4.CreateTranslation(1,-2,0);
		}
		public override void OnRenderFrame(Game game, double dt)
		{
			base.OnRenderFrame(game, dt);
			_textureSheet.Use(TextureUnit.Texture0);
			_shader.Use();
			_shader.SetMatrix4("view", PlrCam.View);
			_mesh.Bind();
			_mesh.Draw(_shader,Matrix4.CreateTranslation(MathF.Sin(frameamt*0.001f),0,0));
			_mesh.Draw(_shader,_groundmatrix);
			for (int i = 0; i < 30; i++) _mesh.Draw(_shader,DataStuff.CreateRotationXYZ(Random.Shared.NextSingle(),Random.Shared.NextSingle(),Random.Shared.NextSingle()) * Matrix4.CreateTranslation(Vector3.Normalize((Random.Shared.Next(-100,100),Random.Shared.Next(-100,100),Random.Shared.Next(-100,100)))*10));
			_mesh2.Bind();
			_mesh2.Draw(_shader,Matrix4.CreateTranslation(0,-5,0));
			_mesh2.Draw(_shader,Matrix4.CreateTranslation(-0.5f,0,-0.5f)*Matrix4.CreateScale(20,1,20)*DataStuff.CreateRotationXYZ(90*DataStuff.D2RConst,30*DataStuff.D2RConst,frameamt*.0001f)*Matrix4.CreateTranslation(-30,5,30));
			game._textRenderer.RenderText(game, game._textShader, "cam properties:\nSpeed: "+PlrCam.Speed.ToString("N4")+"\nFirstPerson: "
+PlrCam.FirstPerson.ToString()+"\nFreecam: "
+PlrCam.Freecam.ToString()+"\nCamToTargetOffset: "
+PlrCam.CamToTargetOffset.ToString("N4")+"\nCamLookAt: "
+PlrCam.CamLookAt.ToString("N4")+"\nRight: "
+PlrCam.Right.ToString("N4")+"\nCamToTargetDist: "
+PlrCam.CamToTargetDist.ToString("N4")+"\nPos: "
+PlrCam.Pos.ToString("N4")+"\nUp: "
+PlrCam.Up.ToString("N4")+"\nView: "
+PlrCam.View.ToString("N4")+"\nPitch: "
+PlrCam.Pitch.ToString("N4")+"\nYaw: "
+PlrCam.Yaw.ToString("N4")+"\nMouseSensitivity: "
+PlrCam.MouseSensitivity.ToString("N4")+"\nProjection: "
+PlrCam.Projection.ToString("N4")+"\nfovy: "
+PlrCam.fovy.ToString("N4")+"\n",(0,-200),(-.5f,.5f),(2,2),(.75f,.9f,.9f),10f,game._clientSize,FontCharFillerThing.FontCharDeeta);
		}
		public override void OnUpdateFrame(Game game, double dt)
		{
			base.OnUpdateFrame(game, dt);
			frameamt++;
			bool UpdCamVecs = false;
			MouseState ms = game.MouseState;
			(float deltaX, float deltaY) = ms.Position - ms.PreviousPosition;
			if ((ms[MouseButton.Middle] || ms[MouseButton.Right]) && (deltaX != 0 || deltaY != 0)) {
				float Yaw = PlrCam.Yaw = (PlrCam.Yaw + deltaX * PlrCam.MouseSensitivity) % MathF.Tau;
				float Pitch = PlrCam.Pitch = Math.Max(-89f*DataStuff.D2RConst, Math.Min(89f*DataStuff.D2RConst, PlrCam.Pitch + deltaY * PlrCam.MouseSensitivity));
				// float NCosPitch = -(float)Math.Cos(Pitch);
				float CosPitch = MathF.Cos(Pitch);
				// PlrCam.CamToTargetOffset = Vector3.Normalize(new Vector3(
				// 	NCosPitch * (float)Math.Cos(Yaw),
				// 	-(float)Math.Sin(Pitch),
				// 	NCosPitch * (float)Math.Sin(Yaw)));/*if (IsNotEngineTick) {*/PlrCam.UpdateVectors();// return;}
				PlrCam.CamLookAt = Vector3.Normalize(new Vector3(
					CosPitch * MathF.Cos(Yaw),
					MathF.Sin(Pitch),
					CosPitch * MathF.Sin(Yaw)));UpdCamVecs = true;
			}
			if (ms.ScrollDelta.Y != 0f) {PlrCam.Speed = Math.Min(Math.Max(PlrCam.Speed * (1+ms.ScrollDelta.Y*0.1f),0.1f),1000f);}
			KeyboardState ks = game.KeyboardState;
			float moveAmount = PlrCam.Speed / _tickSpeed;
			// Player player = _player;
			// float movement = (ks.IsKeyDown(Keys.W) ? -moveAmount : 0) + (ks.IsKeyDown(Keys.S) ? moveAmount : 0);
			// Vector3 plrMovement = PlrCam.CamLookAt * movement;
			// movement = (ks.IsKeyDown(Keys.A) ? -moveAmount : 0) + (ks.IsKeyDown(Keys.D) ? moveAmount : 0);
			// plrMovement += PlrCam.Right * movement;
			// movement = ((ks.IsKeyDown(Keys.Space) || ks.IsKeyDown(Keys.E)) ? moveAmount : 0) + ((ks.IsKeyDown(Keys.LeftShift) || ks.IsKeyDown(Keys.Q)) ? -moveAmount : 0);
			// plrMovement += PlrCam.Up * movement;
			Vector3 plrMovement = PlrCam.CamLookAt * ((ks[Keys.W] ? -moveAmount : 0) + (ks[Keys.S] ? moveAmount : 0)) +
			PlrCam.Right * ((ks[Keys.A] ? -moveAmount : 0) + (ks[Keys.D] ? moveAmount : 0)) +
			PlrCam.Up * (((ks[Keys.Space] || ks[Keys.E]) ? moveAmount : 0) + ((ks[Keys.LeftShift] || ks[Keys.Q]) ? -moveAmount : 0));
			if (plrMovement != Vector3.Zero) {
				PlrCam.Pos += plrMovement;
				UpdCamVecs = true;
			}
			if (frameamt % 2 == 0) {
				PlrCam.fovy = MathF.Sin(frameamt*.0025f)*4f;
				PlrCam.UpdProjection();
				UpdCamVecs = true;
			}
			if (UpdCamVecs) PlrCam.UpdateVectors();
			float[] mesh = _mesh.Vertices;
			if (ks[Keys.RightControl]) {DeetaStuff.objv.CopyTo(mesh);Console.WriteLine("resetted");}
			else{float n;
			if (ks[Keys.R]) {n=MathF.Sin(frameamt*.01f)*(16/2048f);
				mesh[5]=mesh[14]=mesh[23]=mesh[32]=n;Console.WriteLine("thing:r"); }
			if (ks[Keys.T]) {n=MathF.Cos(frameamt*.01f)*(16/2048f);
				mesh[6]=mesh[15]=mesh[24]=mesh[33]=n;Console.WriteLine("thing:t"); }
			if (ks[Keys.U]) {n=MathF.Sin(frameamt*.012f)*(16/2048f);
				mesh[7]=mesh[16]=mesh[25]=mesh[34]=n;Console.WriteLine("thing:u"); }
			if (ks[Keys.I]) {n=MathF.Cos(frameamt*.0134f)*(16/2048f)+240/2048f;
				mesh[8]=mesh[17]=mesh[26]=mesh[35]=n;Console.WriteLine("thing:i"); }
			if (ks[Keys.O]) {n=MathF.Sin(frameamt*.0026f)*96f;
				mesh[12]=mesh[30]=n;Console.WriteLine("thing:o"); }
			if (ks[Keys.P]) {n=MathF.Cos(frameamt*.00232f)*64f;
				mesh[22]=mesh[31]=n;Console.WriteLine("thing:p"); }
			if (ks[Keys.LeftBracket]) {n=MathF.Sin(frameamt*.0046f)*96f;
				mesh[3]=n;mesh[12]=n+64;mesh[21]=n;mesh[30]=n+64;Console.WriteLine("thing:["); }
			if (ks[Keys.K]) {n=MathF.Cos(frameamt*.00432f)*96f;
				mesh[4]=n;mesh[13]=n;mesh[22]=mesh[31]=n+64;Console.WriteLine("thing:k"); }}
			_mesh.UpdMeshV();
			mesh = DeetaStuff.bluebgv;
			float offsetthing = frameamt*.005f+MathF.Sin(frameamt*.0025f);
			mesh[3]=offsetthing;
			mesh[12]=offsetthing+10;
			mesh[21]=offsetthing+3;
			mesh[30]=offsetthing+13;
			offsetthing = frameamt*(-.0025f)-MathF.Cos(frameamt*.0025f)*.5f;
			mesh[4]=mesh[13]=offsetthing;
			mesh[22]=mesh[31]=offsetthing+10;
			_mesh2.UpdMesh(mesh);
		}
		public override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);
			PlrCam.aspect =(float)e.Width/e.Height;
			PlrCam.UpdProjection();
			PlrCam.UpdateVectors();
		}
		public override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
		}
	}
	public class RandomDemo1 : IMinigame
	{
		public const string GameIdentifier = "randomdemo1";
		public static Dictionary<string, Action<Game>> InGameConstructorthings = new() {
			["randomdemo1"] = delegate (Game game) {
				game._currentMinigames.Add(new RandomDemo1());
				game._gameModes.Add(GameIdentifier);
			}
		};
		public ObjectMesh _tetrahedron, _cube, _plane;
		public override void OnLoad(Game game) {
			base.OnLoad(game);
			_tetrahedron = new ObjectMesh(new Vector3(0f, 10f, 0f), Vector3.Zero, Vector3.One, DataStuff.TetrahedronV, DataStuff.TetrahedronI);
			_cube = new ObjectMesh((2,0,0), Vector3.Zero, Vector3.One, DataStuff.CubeV, DataStuff.CubeI);
			_plane = new ObjectMesh(Vector3.Zero, Vector3.Zero, Vector3.One, DataStuff.PlaneV, DataStuff.PlaneI);
		}
		public override void OnRenderFrame(Game game, double dt)
		{
			base.OnRenderFrame(game, dt);
			_tetrahedron.Draw(game._shader, true);
			_cube.Draw(game._shader, true);
			float sinGameTime = (float)Math.Sin(game._gameTime);
			Matrix4 Scale = Matrix4.CreateScale(new Vector3(sinGameTime + 1f));
			Matrix4[] models = [
				Scale * Matrix4.CreateTranslation(-10f, 0f, 0f),
				Scale * Matrix4.CreateTranslation(10f, 0f, 0f),
				Scale * Matrix4.CreateTranslation(0f, 10f, 0f),
				Scale * Matrix4.CreateTranslation(0f, -10f, 0f),
				Scale * Matrix4.CreateTranslation(0f, 0f, 10f),
				Scale * Matrix4.CreateTranslation(0f, 0f, -10f),
			];
			_cube.DrawWithModels(game._shader, models, false);
			// for (int i = 0; i < models.Length; i++) game._cube.DrawWithModel(game._shader, models[i], false);
			Text _tr = game._textRenderer;
			FontCharacterData fcd = FontCharFillerThing.FontCharDeeta;
			_tr.RenderText(game, game._textShader, "FPS: "+(1 / game._dT).ToString("N4"), (0,0), (-.9f,.9f), (5f,5f), (1f,1f,1f), 10f, game._clientSize, fcd, true);
			_tr.RenderText(game, game._textShader, "FPS: {1 / _dT:N4}", (0,0), (-.5f,.9f), (5f,5f), (1f,1f,1f), 10f, game._clientSize, fcd, true);
			_tr.RenderText(game, game._textShader, "FPS: "+(1 / game._dT), (0,0), (-.9f,.9f), (2f,2f), (1f,1f,1f), 10f, game._clientSize, fcd, true);
			_tr.RenderText(game, game._textShader, "FPS: "+(1 / game._dT).ToString("N4"), (0,0), (-.9f,.9f), (1f,1f), (1f,1f,1f), 10f, game._clientSize, fcd, true);
			_tr.RenderText(game, game._textShader, "abcdefghijklmnopqrstuvwxyz a b      d!? b", (0,0), (-.9f,-.9f), (10f,10f), (1f,.5f,1f), 10f, game._clientSize, fcd, true);
			_tr.RenderText(game, game._textShader, "abcdefghijklmnopqrstuvwxyz a b      d!? b", (0,0), ((float)Math.Sin(game._gameTime),-.9f), (10f,10f), (1f,.5f,1f), 10f, game._clientSize, fcd, true);
			_tr.RenderText(game, game._textShader, "FPS", (0,0), (-.9f,-.4f), (10f,10f), (1f,.5f,1f), 10f, game._clientSize, fcd, true);
			_tr.RenderText(game, game._textShader, """
The FitnessGram(TM) Pacer Test is an aerobic capacity test that progressively gets harder as it continues.
The thirty meter pacer test begins in 20 seconds.
When you hear this signal a lap is completed if you don't complete the lap in time you get a strike if you get two strikes you are out
blah blah when you hear this sound it starts on your mark get ready start
the quick brown fox jumped over the lazy dog THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG 0123456789
?!?!?![]{}-=_+`~!@#$%^&*();':",.<>/\|        cabbage
""", (0,0), (-.9f,-.6f), (1f,1f), (1f,.5f,1f), 10f, game._clientSize, fcd, true);
			float xOffset = (float)Math.Sin(game._gameTime) * .025f - .48f;
			for (int i = 20; i > 0; i--) _tr.RenderText(game, game._textShader,i + "FPS: "+1 / game._dT, (0,0), (xOffset,i * .05f - .25f), (i*.5f,i*.5f), new(Random.Shared.Next(0, 100) / 100f), 10f, game._clientSize, fcd, true);
		}
		public override void OnUpdateFrame(Game game, double dt)
		{
			base.OnUpdateFrame(game, dt);
			if (game.IsFocused && !game._isChatting) {
				_cube.Update(0, (2,0,0), (0f, (float)game._gameTime, (float)Math.Sin(game._gameTime) * 2f), Vector3.One);
				_tetrahedron.Update(0, (0f, 3f, 0f), (0f, (float)game._gameTime, (float)Math.Sin(game._gameTime) * 2f), Vector3.One);
			}
		}
		public override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			_cube?.Dispose();
			_tetrahedron?.Dispose();
			_plane?.Dispose();
		}
	}
	public class BABFTE : IMinigame {
		public const string GameIdentifier = "babfte";
		public static Dictionary<string, Action<Game>> InGameConstructorthings = new() {
			["bab"] = delegate (Game game) {
				game._currentMinigames = [new BABFTE()];
				game._gameModes.Add(GameIdentifier);
			}
		};
		public static void StartInit() {
			InGameConstructorthings["babft"]=
			InGameConstructorthings["babfte"]=
			InGameConstructorthings["buildaboat"]=
			InGameConstructorthings["buildaboatfortreasure"]=
			InGameConstructorthings["buildaboatemulator"]=
			InGameConstructorthings["buildaboatfortreasureemulator"]=
			InGameConstructorthings["babemulator"]=
			InGameConstructorthings["babftemulator"]=
			InGameConstructorthings["babgates"]=
			InGameConstructorthings["babftgates"]=
			InGameConstructorthings["logicgates"]=
			InGameConstructorthings["logic gates"]=
			InGameConstructorthings["build a boat for treasure logic gates"]=InGameConstructorthings["bab"];
			DataStuff.chatCommands["bab "] = delegate (Game g, string s) {
				long randomTimestamplol = Stopwatch.GetTimestamp();
				StringBuilder writeString = new("bab command lol\n");
				if (s.Length > 5) {
					switch (s[..5]) {
						case "save ": {
								writeString.Append("attempting to save..\n");
								string path = "data/babfte/saves/"+s[5..];
								ulong tryNum = 0;
								if (Directory.Exists(path)) {
									writeString.Append("path exists; all files:\n");
									string[] allFilesInPath = Directory.GetFiles(path);
									int pathlenp1 = path.Length+1;
									HashSet<ulong> nums = [];
									foreach (string _s in allFilesInPath) {
										writeString.Append(_s+'\n');
										try {ulong thisNum = Convert.ToUInt64(_s[pathlenp1..]); nums.Add(thisNum);} catch {}
									}
									if (nums.Contains(tryNum)) {
										do {
											tryNum = ((ulong)Random.Shared.NextInt64()<<1)^((ulong)Random.Shared.NextInt64());
										} while (nums.Contains(tryNum));
									}
									writeString.Append("will try with filename "+tryNum);
								} else {
									writeString.Append("path doesn't exist; will save as 0");
									Directory.CreateDirectory(path);
								}
								Dictionary<ulong,uint> IndexTranslator = []; // The reason for this is because some items may have been deleted and stuff and the ids are spaced out so like uh.
								uint indextranslatorindex = 0;
								uint bindableblockcount = 0;
								uint gatecount = 0;
								List<BiBS.Gate> CleanGatesList = [];
								foreach (IBlock block in BlockStuff.AllBlocks)
									if (block != null && block.ID > 0) {
										IndexTranslator[block.ID] = indextranslatorindex++;
										if (block is IBindableBlock bindableBlock) {
											bindableblockcount++;
											if (bindableBlock is BiBS.Gate gate) {
												gatecount++;
												CleanGatesList.Add(gate);
											}
										}
									}
								List<uint> thingy = [
									4294967295, // save version
									indextranslatorindex, // amount of blocks
									bindableblockcount, // amount of bindable blocks
									gatecount, // amount of gates
								];
								int thingyPtr = 4;
								float[] vecs = new float[9];
								uint[] vectouint = new uint[9];
								for (int i = 0; i < gatecount; i++) {
									BiBS.Gate gate = CleanGatesList[i];
									thingy.Add(IndexTranslator[gate.ID]);
									thingy.Add((uint)gate.Type|((uint)gate.State<<3)|((uint)gate.OutlineState<<5));
									thingy.Add(gate.Clr);
									(vecs[0], vecs[1], vecs[2]) = gate.Pos;
									(vecs[3], vecs[4], vecs[5]) = gate.Rot;
									(vecs[6], vecs[7], vecs[8]) = gate.Size;
									System.Buffer.BlockCopy(vecs, 0, vectouint, 0, 36);
									thingy.AddRange(vectouint);
									thingyPtr += 12;
								}
								for (int i = 0; i < gatecount; i++) {
									BiBS.Gate gate = CleanGatesList[i];
									// int countPtr = thingyPtr;
									thingy.Add(0);
									thingy.Add(0);// thingyPtr += 2;
									uint inpCount = 0, outCount = 0;
									foreach (IBindableBlock _gate in gate.InputsArray)
										if (_gate != null && _gate.ID > 0) {
											thingy.Add(IndexTranslator[_gate.ID]); inpCount++;
										}
									foreach (IBindableBlock _gate in gate.OrigOrderOutputs)
										if (_gate != null && _gate.ID > 0) {
											thingy.Add(IndexTranslator[_gate.ID]); outCount++;
										}
									thingy[thingyPtr] = inpCount;
									thingy[thingyPtr+1] = outCount;
									thingyPtr += (int)(inpCount+outCount+2u);
								}
								byte[] actualData = new byte[thingy.Count<<2];
								System.Buffer.BlockCopy(thingy.ToArray(), 0, actualData, 0, actualData.Length);
								writeString.Append("\nsaving uint list len: "+actualData.Length+"\ntime so far: "+((double)(Stopwatch.GetTimestamp()-randomTimestamplol)/Stopwatch.Frequency));
								File.WriteAllBytes(path+'/'+tryNum, actualData);
								writeString.Append("\ntime: "+((double)(Stopwatch.GetTimestamp()-randomTimestamplol)/Stopwatch.Frequency)+'\n');
								break;
							}
						case "load ": {
								writeString.Append("attempting to load..\n");
								string path = "data/babfte/saves/"+s[5..];
								if (File.Exists(path)) {
									System.Threading.Tasks.Task<byte[]> deetaGetter = File.ReadAllBytesAsync(path);
									writeString.Append("file ("+path+") exists!\n");
									int pathlen = path.Length;
									foreach (IBindableBlock b in BiBS.AllBindableBlocks) {
										b.Inputs = null;
										b.InputsArray = null;
										b.OutputsHashSet = null;
										b.OrigOrderOutputs = null;
										b.CorrectedOrderOutputs = null;
									}
									BiBS.Gate.AllGates = [Tools.GhostBlockGateIshIDK];
									BiBS.AllBindableBlocks.Clear();
									BiBS.CalcQueue.Clear();
									BiBS.NextCalcQueue.Clear();
									BiBS.SubCalcQueue = [];
									BlockStuff.AllBlocks.Clear();
									Tools.BindToolBindList.Clear();
									Tools.BindToolSelList.Clear();
									Tools.SelectedBlocks.Clear();
									GC.Collect();

									deetaGetter.Wait();
									byte[] data = deetaGetter.Result; deetaGetter.Dispose();
									uint[] dAsUint = new uint[data.Length>>2];
									System.Buffer.BlockCopy(data,0,dAsUint,0,data.Length);
									// Dictionary<ulong,uint> IndexTranslator = []; // The reason for this is because some items may have been deleted and stuff and the ids are spaced out so like uh.
									// uint indextranslatorindex = 1;
									// uint bindableblockcount = 0;
									// uint gatecount = 0;
									// List<Gate> CleanGatesList = [];
									// foreach (IBlock block in BlockStuff.AllBlocks) {
									// 	if (block != null && block.ID > 0) {
									// 		IndexTranslator[block.ID] = indextranslatorindex++;
									// 		if (block is IBindableBlock bindableBlock) {
									// 			bindableblockcount++;
									// 			if (bindableBlock is Gate gate) {
									// 				gatecount++;
									// 				CleanGatesList.Add(gate);
									// 			}
									// 		}
									// 	}
									// }
									uint Ver = dAsUint[0],
									BlockCount = dAsUint[1],
									BindableBlockCount = dAsUint[2],
									GateCount = dAsUint[3];
									IBlock.IDPtr = BlockCount;
									int thingyPtr = 4;
									float[] floats = new float[9];
									BiBS.Gate[] gatesArrayThing = new BiBS.Gate[GateCount];
									for (int i = 0; i < GateCount; i++) {
										uint deetaThingy = dAsUint[thingyPtr+1];
										System.Buffer.BlockCopy(dAsUint, (thingyPtr<<2)+12, floats, 0, 36);
										uint id = dAsUint[thingyPtr];
										BiBS.Gate gate = gatesArrayThing[id] = new() {
											ID = id,
											Type = (GateType)(deetaThingy&7u),
											TypeXorOn = (GateType)(deetaThingy&7u),
											State = (BlockState)((deetaThingy>>3)&3),
											OutlineState = (OutlineType)((deetaThingy>>5)&3),
											Clr = dAsUint[thingyPtr+2],
											Pos = (floats[0],floats[1],floats[2]),
											Rot = (floats[3],floats[4],floats[5]),
											Size = (floats[6],floats[7],floats[8]),
										};
										BlockStuff.AllBlocks.Add(gate);
										BiBS.AllBindableBlocks.Add(gate);
										BiBS.Gate.AllGates.Add(gate);
										thingyPtr += 12;
									}
									for (int i = 0; i < GateCount; i++) {
										BiBS.Gate gate = gatesArrayThing[i];
										uint inpCount = dAsUint[thingyPtr], outCount = dAsUint[++thingyPtr];
										HashSet<IBindableBlock> gateInps = gate.Inputs;
										for (int j = (int)inpCount; j > 0; j--) gateInps.Add(gatesArrayThing[dAsUint[++thingyPtr]]);
										for (int j = 0; j < (int)outCount; j++) {
											BiBS.Gate outputGate = gatesArrayThing[dAsUint[++thingyPtr]];
											gate.OrigOrderOutputs.Add(outputGate);
											gate.OutputsHashSet.Add(outputGate);
										}
										thingyPtr++;
									}
									BiBS.RefreshAllBindOrderStuffs();
									writeString.Append("\nloaded hopefully...\ntime: "+((double)(Stopwatch.GetTimestamp()-randomTimestamplol)/Stopwatch.Frequency)+'\n');
								}
								break;
							}
						case "svrl ": {
								writeString.Append("attempting to save..\n");
								string path = "data/babfte/saves/"+s[5..];
								ulong tryNum = 0;
								if (Directory.Exists(path)) {
									writeString.Append("path exists; all files:\n");
									string[] allFilesInPath = Directory.GetFiles(path);
									int pathlenp1 = path.Length+1;
									HashSet<ulong> nums = [];
									foreach (string _s in allFilesInPath) {
										writeString.Append(_s+'\n');
										try {ulong thisNum = Convert.ToUInt64(_s[pathlenp1..]); nums.Add(thisNum);} catch {}
									}
									if (nums.Contains(tryNum)) {
										do {
											tryNum = ((ulong)Random.Shared.NextInt64()<<1)^((ulong)Random.Shared.NextInt64());
										} while (nums.Contains(tryNum));
									}
									writeString.Append("will try with filename "+tryNum);
								} else {
									writeString.Append("path doesn't exist; will save as 0");
									Directory.CreateDirectory(path);
								}
								Dictionary<ulong,uint> IndexTranslator = []; // The reason for this is because some items may have been deleted and stuff and the ids are spaced out so like uh.
								uint indextranslatorindex = 0;
								uint bindableblockcount = 0;
								uint gatecount = 0;
								List<BiBS.Gate> CleanGatesList = [];
								foreach (IBlock block in BlockStuff.AllBlocks)
									if (block != null && block.ID > 0) {
										IndexTranslator[block.ID] = indextranslatorindex++;
										if (block is IBindableBlock bindableBlock) {
											bindableblockcount++;
											if (bindableBlock is BiBS.Gate gate) {
												gatecount++;
												CleanGatesList.Add(gate);
											}
										}
									}
								List<uint> thingy = [
									4294967295, // save version
									indextranslatorindex, // amount of blocks
									bindableblockcount, // amount of bindable blocks
									gatecount, // amount of gates
								];
								int thingyPtr = 4;
								float[] vecs = new float[9];
								uint[] vectouint = new uint[9];
								for (int i = 0; i < gatecount; i++) {
									BiBS.Gate gate = CleanGatesList[i];
									thingy.Add(IndexTranslator[gate.ID]);
									thingy.Add((uint)gate.Type|((uint)gate.State<<3)|((uint)gate.OutlineState<<5));
									thingy.Add(gate.Clr);
									(vecs[0], vecs[1], vecs[2]) = gate.Pos;
									(vecs[3], vecs[4], vecs[5]) = gate.Rot;
									(vecs[6], vecs[7], vecs[8]) = gate.Size;
									System.Buffer.BlockCopy(vecs, 0, vectouint, 0, 36);
									thingy.AddRange(vectouint);
									thingyPtr += 12;
								}
								for (int i = 0; i < gatecount; i++) {
									BiBS.Gate gate = CleanGatesList[i];
									// int countPtr = thingyPtr;
									thingy.Add(0);
									thingy.Add(0);// thingyPtr += 2;
									uint inpCount = 0, outCount = 0;
									foreach (IBindableBlock _gate in gate.InputsArray)
										if (_gate != null && _gate.ID > 0) {
											thingy.Add(IndexTranslator[_gate.ID]); /*thingyPtr++;*/ inpCount++;
										}
									foreach (IBindableBlock _gate in gate.OrigOrderOutputs)
										if (_gate != null && _gate.ID > 0) {
											thingy.Add(IndexTranslator[_gate.ID]); /*thingyPtr++;*/ outCount++;
										}
									thingy[thingyPtr] = inpCount;
									thingy[thingyPtr+1] = outCount;
									thingyPtr += (int)(inpCount+outCount+2u);
								}
								byte[] actualData = new byte[thingy.Count<<2];
								System.Buffer.BlockCopy(thingy.ToArray(), 0, actualData, 0, actualData.Length);
								writeString.Append("\nsaving uint list len: "+actualData.Length+"\ntime so far: "+((double)(Stopwatch.GetTimestamp()-randomTimestamplol)/Stopwatch.Frequency));
								File.WriteAllBytes(path+'/'+tryNum, actualData);
								writeString.Append("\ntime: "+((double)(Stopwatch.GetTimestamp()-randomTimestamplol)/Stopwatch.Frequency)+'\n');



								writeString.Append("attempting to reload..\n");
								foreach (IBindableBlock b in BiBS.AllBindableBlocks) {
									b.Inputs = null;
									b.InputsArray = null;
									b.OutputsHashSet = null;
									b.OrigOrderOutputs = null;
									b.CorrectedOrderOutputs = null;
								}
								BiBS.Gate.AllGates = [Tools.GhostBlockGateIshIDK];
								BiBS.AllBindableBlocks.Clear();
								BiBS.CalcQueue.Clear();
								BiBS.NextCalcQueue.Clear();
								BiBS.SubCalcQueue = [];
								BlockStuff.AllBlocks.Clear();
								Tools.BindToolBindList.Clear();
								Tools.BindToolSelList.Clear();
								Tools.SelectedBlocks.Clear();
								GC.Collect();

								uint[] dAsUint = [.. thingy];
								// uint Ver = dAsUint[0],
								// BlockCount = dAsUint[1],
								// BindableBlockCount = dAsUint[2],
								// GateCount = dAsUint[3];
								IBlock.IDPtr = dAsUint[1];
								thingyPtr = 4;
								float[] floats = new float[9];
								BiBS.Gate[] gatesArrayThing = new BiBS.Gate[gatecount];
								for (int i = 0; i < gatecount; i++) {
									uint deetaThingy = dAsUint[thingyPtr+1];
									System.Buffer.BlockCopy(dAsUint, (thingyPtr<<2)+12, floats, 0, 36);
									uint id = dAsUint[thingyPtr];
									BiBS.Gate gate = gatesArrayThing[id] = new() {
										ID = id,
										Type = (GateType)(deetaThingy&7u),
										TypeXorOn = (GateType)(deetaThingy&7u),
										State = (BlockState)((deetaThingy>>3)&3),
										OutlineState = (OutlineType)((deetaThingy>>5)&3),
										Clr = dAsUint[thingyPtr+2],
										Pos = (floats[0],floats[1],floats[2]),
										Rot = (floats[3],floats[4],floats[5]),
										Size = (floats[6],floats[7],floats[8]),
									};
									BlockStuff.AllBlocks.Add(gate);
									BiBS.AllBindableBlocks.Add(gate);
									BiBS.Gate.AllGates.Add(gate);
									thingyPtr += 12;
								}
								for (int i = 0; i < gatecount; i++) {
									BiBS.Gate gate = gatesArrayThing[i];
									uint inpCount = dAsUint[thingyPtr], outCount = dAsUint[++thingyPtr];
									HashSet<IBindableBlock> gateInps = gate.Inputs;
									for (int j = (int)inpCount; j > 0; j--) gateInps.Add(gatesArrayThing[dAsUint[++thingyPtr]]);
									for (int j = (int)outCount; j > 0; j--) {
										BiBS.Gate outputGate = gatesArrayThing[dAsUint[++thingyPtr]];
										gate.OrigOrderOutputs.Add(outputGate);
										gate.OutputsHashSet.Add(outputGate);
									}
									thingyPtr++;
								}
								BiBS.RefreshAllBindOrderStuffs();
								writeString.Append("\nloaded hopefully...\ntotal time: "+((double)(Stopwatch.GetTimestamp()-randomTimestamplol)/Stopwatch.Frequency)+'\n');
								break;
							}
						case "setg ":
							if (s.Length > 10) {
								switch (s[5..10]) {
									case "bupf ":
										try {
											int a = Convert.ToInt32(s[10..]);
											BiBS.BlockUpdatesPerFrame = a;
											BiBS.ProcessMode = 0;
											writeString.Append("should have set block updates/frame to "+a+" if i've done this right..\n");
										} catch {writeString.Append("bruh you did smth wrong..\n");} break;
									case "stpf ":
										try {
											int a = Convert.ToInt32(s[10..]);
											BiBS.BlockUpdatesPerFrame = a;
											BiBS.ProcessMode = 1;
											writeString.Append("should have set subtick updates/frame to "+a+" if i've done this right..\n");
										} catch {writeString.Append("bruh you did smth wrong..\n");} break;
									case "tupf ":
										try {
											int a = Convert.ToInt32(s[10..]);
											BiBS.BlockUpdatesPerFrame = a;
											BiBS.ProcessMode = 2;
											writeString.Append("should have set ticks/frame to "+a+" if i've done this right..\n");
										} catch {writeString.Append("bruh you did smth wrong..\n");} break;
									case "efpu ":
										try {
											int a = Convert.ToInt32(s[10..]);
											BiBS.AdditionalFramesPerUpdate = a;
											writeString.Append("should have set extra frames per update to "+a+" if i've done this right..\n");
										} catch {writeString.Append("bruh you did smth wrong..\n");} break;
								}
							}
							break;
					}
				}
				Console.Write(writeString.ToString());
			};
		}
		public static int frame;
		public BABFTE() {
			Tools.BABs.Add(this);
		}
		public static class BlockStuff {
			public const int BulkDrawBS = 12;
			public const int BulkDrawConst = 1<<BulkDrawBS; // 4096
			public static List<IBlock> AllBlocks = [];
			public static Type[] AllBlockTypes = [typeof(BiBS.Gate)];
			public static int[] BlockVAOs;
			public static int[] BlockVBOs;
			public static int[] InstanceVBOs;
			public static Dictionary<Type, Action> BlockRenderBehavior = new() {
				[typeof(BiBS.Gate)] = BiBS.Gate.R
			};
			public static Action[] BlockRenderBehaviors = [
				BiBS.Gate.R
			];
		}
		public abstract class IBlock {
			public Vector3 Pos, Rot, Size;
			public uint Clr;
			public BlockState State; // by, like, tools.
			public OutlineType OutlineState;
			public static int VAO, VBO, instanceVBO;
			public static ulong IDPtr = 1;
			// um uh please don't change this after creation thank you uh.
			public ulong ID;
		}
		[Flags]
		public enum BlockState {
			None = 0,
			Selected = 1, // selected, e.g. drag sel'd or shift-sel'd or clicked or whv.
			On = 2, // only for ibindableblock idk
		}
		public class BiBS {
			public static HashSet<IBindableBlock> AllBindableBlocks = [];
			public static List<IBindableBlock[]> CalcQueue = [], NextCalcQueue = [];
			public static IBindableBlock[] SubCalcQueue = [];
			public static int CalcQueueLenMinusTwo, ProcessMode = 0;
			public static int[] ToggleQueue = [];
			public static int BlockUpdatesPerFrame = 16;
			/// <summary>
			/// 0: each frame;
			/// 1: each other frame;
			/// 2: each third frame; etc.
			/// </summary>
			public static int AdditionalFramesPerUpdate = 0;
			public static int LastUpdateFrame = 0;
			public static int QueuePtr = 0, QueueSubPtr = 0, thingy = 0;
			public static long Tick = 0;
			public static void QueueBlock(IBindableBlock block) {
				NextCalcQueue.Add([block]);
			}
			public static void QueueSet(IBindableBlock block) {
				NextCalcQueue.Add(block.CorrectedOrderOutputs);
			}
			public static void RefreshAllBindOrderStuffs() {
				int MaxAmt = 0;
				foreach (IBindableBlock a in AllBindableBlocks) {
					int amt = a.OrigOrderOutputs.Count;
					if (amt > MaxAmt) MaxAmt = amt;
				}
				IBindableBlock[] Deprioritizeds = new IBindableBlock[MaxAmt];
				ToggleQueue = new int[MaxAmt];
				foreach (IBindableBlock a in AllBindableBlocks) {
					int amt = a.OrigOrderOutputs.Count;
					a.CorrectedOrderOutputs = new IBindableBlock[amt];
					if (a.HasAnyOutputBlock = amt > 0) {
						int i = 0, j = 0;
						foreach (IBindableBlock b in a.OrigOrderOutputs)
							if (a.OutputsHashSet.Overlaps(b.OutputsHashSet)) {Deprioritizeds[j]=b; j++;} else
								{a.CorrectedOrderOutputs[i]=b; i++;}
						if (j > 0) Array.Copy(Deprioritizeds, 0, a.CorrectedOrderOutputs, i, j);
					}
					a.Inputs.CopyTo(a.InputsArray = new IBindableBlock[(a.InpsCountMinusOne = (a.InpsCount = a.Inputs.Count)-1)+1]);
				}
			}
			public static void NewTick() {
				Tick++;
				QueuePtr = 0;
				CalcQueueLenMinusTwo = NextCalcQueue.Count-2;
				CalcQueue.Clear();
				(CalcQueue, NextCalcQueue) = (NextCalcQueue, CalcQueue);
				SubCalcQueue = CalcQueue[0];
				QueueSubPtr = 0;
				for (thingy = SubCalcQueue.Length-1; thingy > -1; thingy--) SubCalcQueue[thingy].AddToTQIfCanUpdate();
				if (QueueSubPtr == 0) NextSubTick();
			}
			public static int NextSubTick() {
				QueueSubPtr = 0;
				do {
					if (QueuePtr > CalcQueueLenMinusTwo) {
						Tick++;
						QueuePtr = 0;
						CalcQueue.Clear();
						if ((CalcQueueLenMinusTwo = NextCalcQueue.Count-2) == -2) return 1;
						(CalcQueue, NextCalcQueue) = (NextCalcQueue, CalcQueue);
					} else QueuePtr++;
					SubCalcQueue = CalcQueue[QueuePtr];
					for (thingy = SubCalcQueue.Length-1; thingy > -1; thingy--) SubCalcQueue[thingy].AddToTQIfCanUpdate();
				} while (QueueSubPtr == 0);
				return 0;
			}
			public static void OnTickUpdateSST() {
				if (CalcQueue.Count == 0) {if (NextCalcQueue.Count > 0) NewTick(); return;} // :3
				LastUpdateFrame = frame;
				// int thingy = SubCalcQueueRealLen - QueueSubPtr - BlockUpdatesPerFrame;
				// if (thingy < 0) { // queuecount-queueptr is amt of items that have yet to be processed this subtick. that minus BUPF will be the amount left if the normal amt of blocks is processed this subtick.
				// 	int todo = BlockUpdatesPerFrame - SubCalcQueueRealLen + QueueSubPtr; // me when i count my progresses before they progress
				// int thingy = SubCalcQueueRealLen - QueueSubPtr - BlockUpdatesPerFrame;
				// if (thingy < 0) {
				// 	int todo = -thingy; // these 2 are equivalent lol
				int amtLeft = QueueSubPtr - BlockUpdatesPerFrame; // amount left in this subtick if bupf things are to be processed rn
				if (amtLeft < 0) {
					do {
						do {QueueSubPtr--;SubCalcQueue[ToggleQueue[QueueSubPtr]].ForceToggle();} while (QueueSubPtr > 0);
						do {
							if (QueuePtr > CalcQueueLenMinusTwo) {
								Tick++;
								QueuePtr = 0;
								CalcQueue.Clear();
								if ((CalcQueueLenMinusTwo = NextCalcQueue.Count-2) == -2) return;
								(CalcQueue, NextCalcQueue) = (NextCalcQueue, CalcQueue);
								SubCalcQueue = CalcQueue[0];
							} else {QueuePtr++; SubCalcQueue = CalcQueue[QueuePtr];}
							for (thingy = SubCalcQueue.Length-1; thingy > -1; thingy--) SubCalcQueue[thingy].AddToTQIfCanUpdate();
						} while (QueueSubPtr == 0);
						amtLeft += QueueSubPtr;
					} while (amtLeft < 0);
				}
				if (amtLeft == 0) {
					do {QueueSubPtr--;SubCalcQueue[ToggleQueue[QueueSubPtr]].ForceToggle();} while (QueueSubPtr > 0);
					NextSubTick();}
				else do {QueueSubPtr--;SubCalcQueue[ToggleQueue[QueueSubPtr]].ForceToggle();} while (QueueSubPtr > amtLeft);
			}
			public static void OnTickUpdateST() {
				if (CalcQueue.Count == 0) {if (NextCalcQueue.Count > 0) NewTick(); return;} // :3
				LastUpdateFrame = frame;
				for (int todo = BlockUpdatesPerFrame; todo > 0; todo--){
					do {QueueSubPtr--;SubCalcQueue[ToggleQueue[QueueSubPtr]].ForceToggle();} while (QueueSubPtr > 0);
					do {
						if (QueuePtr > CalcQueueLenMinusTwo) {
							Tick++;
							QueuePtr = 0;
							CalcQueue.Clear();
							if ((CalcQueueLenMinusTwo = NextCalcQueue.Count-2) == -2) return;
							(CalcQueue, NextCalcQueue) = (NextCalcQueue, CalcQueue);
							SubCalcQueue = CalcQueue[0];
						} else {QueuePtr++; SubCalcQueue = CalcQueue[QueuePtr];}
						for (thingy = SubCalcQueue.Length-1; thingy > -1; thingy--) SubCalcQueue[thingy].AddToTQIfCanUpdate();
					} while (QueueSubPtr == 0);
				}
			}
			public static void OnTickUpdateT() {
				int calcqueuelenminusone = CalcQueue.Count-1;
				if (calcqueuelenminusone == -1) {if (NextCalcQueue.Count > 0) {
						Tick++;
						QueuePtr = 0;
						(CalcQueue, NextCalcQueue) = (NextCalcQueue, CalcQueue);
						SubCalcQueue = CalcQueue[0];
						QueueSubPtr = 0;
						// for (int i = SubCalcQueue.Length-1; i > -1; i--) if (SubCalcQueue[i].GetUpdateResult()) ToggleQueue[QueueSubPtr++] = i;
						for (thingy = SubCalcQueue.Length-1; thingy > -1; thingy--) SubCalcQueue[thingy].AddToTQIfCanUpdate();
					} return;} // :3
				LastUpdateFrame = frame;
				for (int todo = BlockUpdatesPerFrame; todo > 0; todo--){
					while (QueuePtr < calcqueuelenminusone) {
						while (QueueSubPtr > 0) {QueueSubPtr--;SubCalcQueue[ToggleQueue[QueueSubPtr]].ForceToggle();}
						QueuePtr++; SubCalcQueue = CalcQueue[QueuePtr];
						for (thingy = SubCalcQueue.Length-1; thingy > -1; thingy--) SubCalcQueue[thingy].AddToTQIfCanUpdate();
					}
					while (QueueSubPtr > 0) {QueueSubPtr--;SubCalcQueue[ToggleQueue[QueueSubPtr]].ForceToggle();}
					Tick++;
					QueuePtr = 0;
					CalcQueue.Clear();
					if ((calcqueuelenminusone = NextCalcQueue.Count-1) == -1) return;
					(CalcQueue, NextCalcQueue) = (NextCalcQueue, CalcQueue);
					SubCalcQueue = CalcQueue[0];
					for (thingy = SubCalcQueue.Length-1; thingy > -1; thingy--) SubCalcQueue[thingy].AddToTQIfCanUpdate();
				}
			}
			public static void OnUpdateFrame() {
				if (frame > LastUpdateFrame + AdditionalFramesPerUpdate) {
					switch (ProcessMode) {
						case 0:
							OnTickUpdateSST();
							break;
						case 1:
							OnTickUpdateST();
							break;
						case 2:
							OnTickUpdateT();
							break;
						default:
							throw new Exception("invalid");
					}
				} // block updates
			}
			public class Gate : IBindableBlock {
				public static List<Gate> AllGates = [];
				public GateType Type;
				public GateType TypeXorOn;
				// /// <summary>
				// /// True: Not enabled. False: Not disabled.
				// /// </summary>
				// public bool Not;
				/// <summary>
				/// creates a gate and does absolutely NOTHING else, doesn't even add it to the lists or add an ID.
				/// </summary>
				public Gate() {}
				public Gate(Vector3 position) {
					Pos = position; Size = (1f,.4f,1f); ID = IDPtr++;
					BlockStuff.AllBlocks.Add(this);
					BiBS.AllBindableBlocks.Add(this);
					AllGates.Add(this);
				}
				public Gate(Vector3 position, Vector3 rotation) {
					Pos = position; Rot = rotation; Size = (1f,.4f,1f); ID = IDPtr++;
					BlockStuff.AllBlocks.Add(this);
					BiBS.AllBindableBlocks.Add(this);
					AllGates.Add(this);
				}
				public Gate(bool ghostIsh) {
					Size = (1f,.4f,1f);
					AllGates.Add(this);
					if (!ghostIsh) { ID = IDPtr++;
						BlockStuff.AllBlocks.Add(this);
						BiBS.AllBindableBlocks.Add(this);
					} else ID = 0;
				}
				public static float[] verts = [
					-.5f,-.5f,.5f, 1/4096f,1/4096f,  .5f,-.5f,.5f, 1/4096f,1/4096f,  -.5f,.5f,.5f, 1/4096f,1/4096f,   .5f,-.5f,.5f, 1/4096f,1/4096f,  .5f,.5f,.5f, 1/4096f,1/4096f,  -.5f,.5f,.5f, 1/4096f,1/4096f, // front
					-.5f,-.5f,-.5f, 1/4096f,1/4096f,  -.5f,.5f,-.5f, 1/4096f,1/4096f,  .5f,-.5f,-.5f, 1/4096f,1/4096f,   .5f,-.5f,-.5f, 1/4096f,1/4096f,  -.5f,.5f,-.5f, 1/4096f,1/4096f,  .5f,.5f,-.5f, 1/4096f,1/4096f, // back
					.5f,-.5f,-.5f, 1/4096f,1/4096f,  .5f,.5f,-.5f, 1/4096f,1/4096f,  .5f,-.5f,.5f, 1/4096f,1/4096f,   .5f,-.5f,.5f, 1/4096f,1/4096f,  .5f,.5f,-.5f, 1/4096f,1/4096f,  .5f,.5f,.5f, 1/4096f,1/4096f, // right
					-.5f,-.5f,-.5f, 1/4096f,1/4096f,  -.5f,-.5f,.5f, 1/4096f,1/4096f,  -.5f,.5f,-.5f, 1/4096f,1/4096f,   -.5f,-.5f,.5f, 1/4096f,1/4096f,  -.5f,.5f,.5f, 1/4096f,1/4096f,  -.5f,.5f,-.5f, 1/4096f,1/4096f, // left
					-.5f,.5f,.5f, 0,1/64f,  -.5f,.5f,-.5f, 0,0,  .5f,.5f,.5f, 1/64f,1/64f,   .5f,.5f,.5f, 1/64f,1/64f,  -.5f,.5f,-.5f, 0,0,  .5f,.5f,-.5f, 1/64f,0, // top
					-.5f,-.5f,.5f, 1/4096f,1/4096f,  .5f,-.5f,.5f, 1/4096f,1/4096f,  -.5f,-.5f,-.5f, 1/4096f,1/4096f,   .5f,-.5f,.5f, 1/4096f,1/4096f,  .5f,-.5f,-.5f, 1/4096f,1/4096f,  -.5f,-.5f,-.5f, 1/4096f,1/4096f, // bottom
				];
				public static void L(Game game) {
					VAO = GL.GenVertexArray();
					GL.BindVertexArray(VAO);

					VBO = GL.GenBuffer();
					GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
					GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

					GL.EnableVertexAttribArray(0);
					GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
					GL.EnableVertexAttribArray(1);
					GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

					instanceVBO = GL.GenBuffer();
					GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
					GL.BufferData(BufferTarget.ArrayBuffer, (sizeof(float)*12+sizeof(uint)*2)*BlockStuff.BulkDrawConst, 0, BufferUsageHint.DynamicDraw);

					GL.EnableVertexAttribArray(2);
					GL.EnableVertexAttribArray(3);
					GL.EnableVertexAttribArray(4);
					GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, sizeof(float)*12+sizeof(uint)*2, 0);
					GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, sizeof(float)*12+sizeof(uint)*2, 4*sizeof(float));
					GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, sizeof(float)*12+sizeof(uint)*2, 8*sizeof(float));
					GL.EnableVertexAttribArray(5);
					GL.EnableVertexAttribArray(6);
					GL.VertexAttribPointer(5, 4, VertexAttribPointerType.UnsignedByte, false, sizeof(float)*12+sizeof(uint)*2, sizeof(float)*12);
					GL.VertexAttribPointer(6, 1, VertexAttribPointerType.UnsignedByte, false, sizeof(float)*12+sizeof(uint)*2, sizeof(float)*12+sizeof(uint));
					GL.VertexAttribDivisor(2, 1);
					GL.VertexAttribDivisor(3, 1);
					GL.VertexAttribDivisor(4, 1);
					GL.VertexAttribDivisor(5, 1);
					GL.VertexAttribDivisor(6, 1);

					// it'll be fineeee without unbind here lol
				}
				[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
				public struct Mat3ColAndU32Color {
					public float m0,m1,m2,m3,m4,m5,m6,m7,m8,m9,m10,m11;
					public uint clr, state;
				}
				public static Mat3ColAndU32Color[] blockDataBuffer = new Mat3ColAndU32Color[BlockStuff.BulkDrawConst];
				public static void R() {
					GL.BindVertexArray(VAO);
					int count = AllGates.Count;
					if ((frame&255)==0)Console.WriteLine("Instance count: "+count);
					if (count > BlockStuff.BulkDrawConst) {
						int amtThing = count>>BlockStuff.BulkDrawBS;
						int i2 = 0;
						for (int j = 0; j < amtThing; j++) {
							for (int i3=0; i3<BlockStuff.BulkDrawConst;i2++,i3++) {
								Gate gate = AllGates[i2];
								uint clr = gate.Clr;
								ref Mat3ColAndU32Color gateData = ref blockDataBuffer[i3];
								gateData.clr = (((((clr&0xff000000u)>>24)+255u)>>1)<<24)|((((clr&0x00ff0000u)+16711680u)>>17)<<16)|(((((clr&0x0000ff00u)*3)+65280u)>>10)<<8)|(clr&0xffu);
								Vector3 r = gate.Rot;
								float sx = gate.Size.X, sy = gate.Size.Y, sz = gate.Size.Z;
								float num = MathF.Cos(r.X), num2 = MathF.Sin(r.X),
								num3 = MathF.Cos(r.Y), num4 = MathF.Sin(r.Y),
								num5 = MathF.Cos(r.Z), num6 = MathF.Sin(r.Z);
								float _x2 = num2 * num4, _x3 = num * num4;
								gateData.m0 = sx*num3*num5;
								gateData.m1 = sy*(_x2*num5-num*num6);
								gateData.m2 = sz*(_x3*num5+num2*num6);
								gateData.m3 = gate.Pos.X;
								gateData.m4 = sx*num3*num6;
								gateData.m5 = sy*(_x2*num6+num*num5);
								gateData.m6 = sz*(_x3*num6-num2*num5);
								gateData.m7 = gate.Pos.Y;
								gateData.m8 = sx*-num4;
								gateData.m9 = sy*num2*num3;
								gateData.m10 = sz*num*num3;
								gateData.m11 = gate.Pos.Z;
								gateData.state = (((uint)gate.Type)|(gate.On?8u:0u))&255u;
							}
							GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
							GL.BufferSubData(BufferTarget.ArrayBuffer, 0, (sizeof(float)*12+sizeof(uint)*2)*BlockStuff.BulkDrawConst, blockDataBuffer);
							GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, BlockStuff.BulkDrawConst);
						}
						int amt = count&(BlockStuff.BulkDrawConst-1);
						if (amt > 0) {
							for (int i3=0; i2 < count; i2++,i3++) {
								Gate gate = AllGates[i2];
								uint clr = gate.Clr;
								ref Mat3ColAndU32Color gateData = ref blockDataBuffer[i3];
								gateData.clr = (((((clr&0xff000000u)>>24)+255u)>>1)<<24)|((((clr&0x00ff0000u)+16711680u)>>17)<<16)|(((((clr&0x0000ff00u)*3)+65280u)>>10)<<8)|(clr&0xffu);
								(float rx, float ry, float rz) = gate.Rot;
								(float sx, float sy, float sz) = gate.Size;
								(float tx, float ty, float tz) = gate.Pos;
								float num = MathF.Cos(rx), num2 = MathF.Sin(rx),
								num3 = MathF.Cos(ry), num4 = MathF.Sin(ry),
								num5 = MathF.Cos(rz), num6 = MathF.Sin(rz);
								float _x2 = num2 * num4, _x3 = num * num4;
								gateData.m0 = sx*num3*num5;
								gateData.m1 = sy*(_x2*num5-num*num6);
								gateData.m2 = sz*(_x3*num5+num2*num6);
								gateData.m3 = tx;
								gateData.m4 = sx*num3*num6;
								gateData.m5 = sy*(_x2*num6+num*num5);
								gateData.m6 = sz*(_x3*num6-num2*num5);
								gateData.m7 = ty;
								gateData.m8 = sx*-num4;
								gateData.m9 = sy*num2*num3;
								gateData.m10 = sz*num*num3;
								gateData.m11 = tz;
								gateData.state = (((uint)gate.Type)|(gate.On?8u:0u))&255u;
							}
							GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
							GL.BufferSubData(BufferTarget.ArrayBuffer, 0, (sizeof(float)*12+sizeof(uint)*2)*amt, blockDataBuffer);
							GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, amt);}
					} else {
						for (int i = 0, i2=0; i2 < count; i+=12,i2++) {
							Gate gate = AllGates[i2];
							uint clr = gate.Clr;
							ref Mat3ColAndU32Color gateData = ref blockDataBuffer[i2];
							gateData.clr = (((((clr&0xff000000u)>>24)+255u)>>1)<<24)|((((clr&0x00ff0000u)+16711680u)>>17)<<16)|(((((clr&0x0000ff00u)*3)+65280u)>>10)<<8)|(clr&0xffu);
							(float rx, float ry, float rz) = gate.Rot;
							(float sx, float sy, float sz) = gate.Size;
							(float tx, float ty, float tz) = gate.Pos;
							float num = MathF.Cos(rx), num2 = MathF.Sin(rx),
							num3 = MathF.Cos(ry), num4 = MathF.Sin(ry),
							num5 = MathF.Cos(rz), num6 = MathF.Sin(rz);
							float _x2 = num2 * num4, _x3 = num * num4;
							gateData.m0 = sx*num3*num5;
							gateData.m1 = sy*(_x2*num5-num*num6);
							gateData.m2 = sz*(_x3*num5+num2*num6);
							gateData.m3 = tx;
							gateData.m4 = sx*num3*num6;
							gateData.m5 = sy*(_x2*num6+num*num5);
							gateData.m6 = sz*(_x3*num6-num2*num5);
							gateData.m7 = ty;
							gateData.m8 = sx*-num4;
							gateData.m9 = sy*num2*num3;
							gateData.m10 = sz*num*num3;
							gateData.m11 = tz;
							gateData.state = (byte)(((int)gate.Type)|(gate.On?8:0));
						}
						GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
						GL.BufferSubData(BufferTarget.ArrayBuffer, 0, (sizeof(float)*12+sizeof(uint)*2)*count, blockDataBuffer);
						GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, count);
					}
				}
				public override bool OnUpdate() {
					bool Changes;
					switch (TypeXorOn) {
						case GateType.And: // this is either off and and or on and nand
							for (int i = InpsCountMinusOne; i > -1; i--) {if (!InputsArray[i].On) {Changes = false; goto thingy;}}
							Changes = true; break;
						case GateType.Or:
							for (int i = InpsCountMinusOne; i > -1; i--) {if (InputsArray[i].On) {Changes = true; goto thingy;}}
							Changes = false; break;
						case GateType.Xor: // off xor or on xnor
							Changes = false;
							for (int i = InpsCountMinusOne; i > -1; i--) Changes ^= InputsArray[i].On;
							break;
						case GateType.Nand:
							for (int i = InpsCountMinusOne; i > -1; i--) {if (!InputsArray[i].On) {Changes = true; goto thingy;}}
							Changes = false; break;
						case GateType.Nor:
							for (int i = InpsCountMinusOne; i > -1; i--) {if (InputsArray[i].On) {Changes = false; goto thingy;}}
							Changes = true; break;
						case GateType.Xnor:
							Changes = true;
							for (int i = InpsCountMinusOne; i > -1; i--) Changes ^= InputsArray[i].On;
							break;
						default: throw new Exception();
					}thingy:
					if (Changes) {
						On ^= true;
						TypeXorOn ^= (GateType)4;
						ForceEvent();
						return true;
					}
					return false;
				}
				public override bool GetUpdateResult() {
					switch (TypeXorOn) {
						case GateType.And: // this is either off and and or on and nand
							for (int i = InpsCountMinusOne; i > -1; i--) if (!InputsArray[i].On) return false;
							return true;
						case GateType.Or:
							for (int i = InpsCountMinusOne; i > -1; i--) if (InputsArray[i].On) return true;
							return false;
						case GateType.Xor: // off xor or on xnor
							bool StateChanges = false;
							for (int i = InpsCountMinusOne; i > -1; i--) StateChanges ^= InputsArray[i].On;
							return StateChanges;
						case GateType.Nand:
							for (int i = InpsCountMinusOne; i > -1; i--) if (!InputsArray[i].On) return true;
							return false;
						case GateType.Nor:
							for (int i = InpsCountMinusOne; i > -1; i--) if (InputsArray[i].On) return false;
							return true;
						case GateType.Xnor:
							bool _StateChanges = true;
							for (int i = InpsCountMinusOne; i > -1; i--) _StateChanges ^= InputsArray[i].On;
							return _StateChanges;
						default: throw new Exception();
					}
				}
				public override void AddToTQIfCanUpdate() {
					switch (TypeXorOn) {
						case GateType.And: // this is either off and and or on and nand
							for (int i = InpsCountMinusOne; i > -1; i--) if (!InputsArray[i].On) return;
							goto add;
						case GateType.Or:
							for (int i = InpsCountMinusOne; i > -1; i--) if (InputsArray[i].On) goto add;
							return;
						case GateType.Xor: // off xor or on xnor
							bool StateChanges = false;
							for (int i = InpsCountMinusOne; i > -1; i--) StateChanges ^= InputsArray[i].On;
							if (StateChanges) goto add; return;
						case GateType.Nand:
							for (int i = InpsCountMinusOne; i > -1; i--) if (!InputsArray[i].On) goto add;
							return;
						case GateType.Nor:
							for (int i = InpsCountMinusOne; i > -1; i--) if (InputsArray[i].On) return;
							goto add;
						case GateType.Xnor:
							bool _StateChanges = true;
							for (int i = InpsCountMinusOne; i > -1; i--) _StateChanges ^= InputsArray[i].On;
							if (_StateChanges) goto add; return;
						default: throw new Exception();
					}
					add:ToggleQueue[QueueSubPtr] = thingy; QueueSubPtr++;
				}
				public override void CleanInputs() {
					foreach (IBindableBlock block in InputsArray) {if(block==null||block.ID==0)Inputs.Remove(block);}
					Inputs.CopyTo(InputsArray = new IBindableBlock[Inputs.Count]);
				}
				public override void ForceEvent() {
					//foreach (IBindableBlock block in OrigOrderOutputs) BindableBlockStuff.QueueBlock(block);
					BiBS.NextCalcQueue.Add(CorrectedOrderOutputs);
				}
				public override void ForceSet(bool state) {
					if (On ^ state) {
						On^=true;
						TypeXorOn ^= (GateType)4;
						BiBS.NextCalcQueue.Add(CorrectedOrderOutputs);
					}
				}
				public override void ForceToggle() {
					On ^= true;
					TypeXorOn ^= (GateType)4;
					BiBS.NextCalcQueue.Add(CorrectedOrderOutputs);
				}
				public override void QuietForceSet(bool state) {
					// if (On^state)On^=true;
					On=state;
					TypeXorOn = Type^(On?(GateType)4:0);
				}
				public override void QuietForceToggle() {
					On ^= true;
					TypeXorOn ^= (GateType)4;
				}
			}
		}
		public abstract class IBindableBlock : IBlock {
			// public List<IBindableBlock> Inputs, OrigOrderOutputs;
			public HashSet<IBindableBlock> Inputs = [], OutputsHashSet = [];
			public List<IBindableBlock> OrigOrderOutputs = [];
			public IBindableBlock[] CorrectedOrderOutputs = [], InputsArray = [];
			public int InpsCount = 0, InpsCountMinusOne = -1;
			public bool HasAnyOutputBlock = false;
			// public bool State;
			// public long IsScheduled = -1L;
			public bool On;
			public abstract bool OnUpdate();
			public abstract bool GetUpdateResult();
			public abstract void AddToTQIfCanUpdate();
			public abstract void CleanInputs();
			public abstract void ForceToggle();
			public abstract void ForceSet(bool state);
			public abstract void QuietForceToggle();
			public abstract void QuietForceSet(bool state);
			public abstract void ForceEvent();
		}
		[Flags]
		public enum OutlineType {
			None = 0,
			Selected = 1,
			Highlighted = 2,
		}
		/// <summary>
		/// all 7 tools lol, babfte probably won't work if there are multiple. lol.
		/// </summary>
		public static class Tools {
			public static List<BABFTE> BABs = [];
			// public static Shader shader;
			// public static CompShader compShader;
			public static int ToolEquipped = 0;
			// public static IBlock[] SelectedBlocks;
			public static Type BuildToolBlock = typeof(BiBS.Gate);
			public static Vector2 DragStart;
			public static bool Dragging;
			// public const int ptsListElementLen = BlockStuff.BulkDrawConst+4;
			// public static List<Vector3[]> ptsList = [new Vector3[ptsListElementLen]];
			// public static int CSVAO,CSSSBO,VAO,VBO;
			public static Shader BiToolShader, OutlBoxShader;
			public static int BiToolVAO, BiToolVBO;//, BiToolEBO;
			public static int OutlBoxVAO, OutlBoxVBO1, OutlBoxVBO2, OutlInstVBO;
			// public static Dictionary<IBlock, OutlineType> OutlinedObjects = [];
			public static SelBoxDataStruct[] SelBoxesBuffer = new SelBoxDataStruct[BlockStuff.BulkDrawConst];
			public static float[] SelBoxPointData = [ // 3x pt s, then 3x pt o
// GO GO GADGET LUAU SCRIPT!
// // COMMENTS HERE WERE MADE AFTER.   ALR SO TO UNDERSTAND THIS,   EACH LINE IS ONE VERTEX. EACH LINE MAKES ONE TRIANGLE.
// // THERE ARE FOUR BASE TRIANGLES.   THE REST IS AUTO-GENERATED THROUGH TERRIBLE Luau CODE.
// // THE FIRST 2 TRIANGLES MAKE THE FRONT LOWER OUTER TRAPEZOID.   TRI 1: LDF LDF, LDF RUF, RDF LUF   TRI 2: LDF LDF, RDF LUF, RDF RDF
// // TRIs 3&4 MAKE THE FRONT LOWER INNER RECTANGLE.   TRI 3: LDF RUF, LDF RUB, RDF LUB   TRI 4: LDF RUF, RDF LUB, RDF LUF
// //  IT IS THEN COPIED BUT ROTATED XY 90 CCW X3 TO MAKE THE FRONT FACE.

// AFTER THAT, IT IS ROTATED XZ 90 CCW X3 TO MAKE THE RIGHT, BACK, AND LEFT SIDES.

// THEN IT ROTATES BACK AND THEN ROTATES YZ 90 CCW TO MAKE TOP.

// THEN IT IS ROTATED YZ 180 TO MAKE THE BOTTOM.
// /* THE FULL OLD BROKEN Luau SCRIPT IS:
// for i=1,1 do
// local inp = {-.5,-.5,-.5,-.02,-.02,-.02,-.5,-.5,-.5,.02,.02,-.02,.5,-.5,-.5,-.02,.02,-.02,-.5,-.5,-.5,-.02,-.02,-.02,.5,-.5,-.5,-.02,.02,-.02,.5,-.5,-.5,.02,-.02,-.02,-.5,-.5,-.5,.02,.02,-.02,-.5,-.5,-.5,.02,.02,.02,.5,-.5,-.5,-.02,.02,.02,-.5,-.5,-.5,.02,.02,-.02,.5,-.5,-.5,-.02,.02,.02,.5,-.5,-.5,-.02,.02,-.02}
// local num=#inp local numx4 = num*4
// local function printinp()for i=1,numx4,6 do print(inp[i].."f, "..inp[i+1].."f, "..inp[i+2].."f, "..inp[i+3].."f, "..inp[i+4].."f, "..inp[i+5].."f,")end end
// for i = 1,numx4,6 do inp[i+num], inp[i+num+1] = -inp[i+1], inp[i] inp[i+num+2] = inp[i+2] inp[i+num+3], inp[i+num+4] = -inp[i+4], inp[i+3] inp[i+num+5] = inp[i+5] end
// print("\n// FRONT SIDE")printinp()
// for i = 1,numx4,6 do inp[i+num],inp[i+num+1],inp[i+num+2]=-inp[i+2],inp[i+1],inp[i] inp[i+num+3],inp[i+num+4],inp[i+num+5]=-inp[i+5],inp[i+4],inp[i+3] end
// print("\n//RIGHT SIDE")printinp()
// for i = 1,numx4,6 do inp[i+num],inp[i+num+1],inp[i+num+2]=-inp[i+2],inp[i+1],inp[i] inp[i+num+3],inp[i+num+4],inp[i+num+5]=-inp[i+5],inp[i+4],inp[i+3] end
// print("\n//BACK SIDE")printinp()
// for i = 1,numx4,6 do inp[i+num],inp[i+num+1],inp[i+num+2]=-inp[i+2],inp[i+1],inp[i] inp[i+num+3],inp[i+num+4],inp[i+num+5]=-inp[i+5],inp[i+4],inp[i+3] end
// print("\n//LEFT SIDE")printinp()
// for i = 1,numx4,6 do inp[i+num],inp[i+num+1],inp[i+num+2]=-inp[i+2],inp[i+1],inp[i] inp[i+num+3],inp[i+num+4],inp[i+num+5]=-inp[i+5],inp[i+4],inp[i+3] end
// for i = 1,numx4,6 do inp[i+num],inp[i+num+1],inp[i+num+2]=inp[i],-inp[i+2],inp[i+1] inp[i+num+3],inp[i+num+4],inp[i+num+5]=inp[i+3],-inp[i+5],inp[i+4] end
// print("\n//TOP SIDE")printinp()
// for i = 1,numx4,6 do inp[i+num],inp[i+num+1],inp[i+num+2]=inp[i],-inp[i+1],-inp[i+2] inp[i+num+3],inp[i+num+4],inp[i+num+5]=inp[i+3],-inp[i+4],-inp[i+5] end
// print("\n//BOTTOM SIDE")printinp()end*/
// THAT OLD SCRIPT HAD SOME SERIOUS ISSUES AND WAS BROKEN LOL.
/*NEW SCRIPT:
for i=1,1 do
local inp={
-.5,-.5,-.5,-.02,-.02,-.02,-.5,-.5,-.5,.02,.02,-.02,.5,-.5,-.5,-.02,.02,-.02,
-.5,-.5,-.5,-.02,-.02,-.02,.5,-.5,-.5,-.02,.02,-.02,.5,-.5,-.5,.02,-.02,-.02,
-.5,-.5,-.5,.02,.02,-.02,-.5,-.5,-.5,.02,.02,.02,.5,-.5,-.5,-.02,.02,.02,
-.5,-.5,-.5,.02,.02,-.02,.5,-.5,-.5,-.02,.02,.02,.5,-.5,-.5,-.02,.02,-.02}
local num=#inp local numx4=num*4
local function printinp()for i=1,numx4,18 do print(inp[i].."f,"..inp[i+1].."f,"..inp[i+2].."f,"..inp[i+3].."f,"..inp[i+4].."f,"..inp[i+5].."f,   "..inp[i+6].."f,"..inp[i+7].."f,"..inp[i+8].."f,"..inp[i+9].."f,"..inp[i+10].."f,"..inp[i+11].."f,   "..inp[i+12].."f,"..inp[i+13].."f,"..inp[i+14].."f,"..inp[i+15].."f,"..inp[i+16].."f,"..inp[i+17].."f,") end end
for i=1,numx4,3 do inp[i+num]=-inp[i+1] inp[i+num+1]=inp[i] inp[i+num+2]=inp[i+2]end
print("\n// FRONT SIDE")printinp()
local function rot() for i = 1,numx4,3 do inp[i],inp[i+2]=-inp[i+2],inp[i]end end rot()
print("\n//RIGHT SIDE")printinp()rot()
print("\n//BACK SIDE")printinp()rot()
print("\n//LEFT SIDE")printinp()rot()
for i=2,numx4,3 do inp[i],inp[i+1]=-inp[i+1],inp[i]end
print("\n//TOP SIDE")printinp()
for i=2,numx4,3 do inp[i],inp[i+1]=-inp[i],-inp[i+1]end
print("\n//BOTTOM SIDE")printinp()end*/
// FRONT SIDE
-.5f,-.5f,-.5f,-.02f,-.02f,-.02f,   -.5f,-.5f,-.5f,.02f,.02f,-.02f,   .5f,-.5f,-.5f,-.02f,.02f,-.02f,
-.5f,-.5f,-.5f,-.02f,-.02f,-.02f,   .5f,-.5f,-.5f,-.02f,.02f,-.02f,   .5f,-.5f,-.5f,.02f,-.02f,-.02f,
-.5f,-.5f,-.5f,.02f,.02f,-.02f,   -.5f,-.5f,-.5f,.02f,.02f,.02f,   .5f,-.5f,-.5f,-.02f,.02f,.02f,
-.5f,-.5f,-.5f,.02f,.02f,-.02f,   .5f,-.5f,-.5f,-.02f,.02f,.02f,   .5f,-.5f,-.5f,-.02f,.02f,-.02f,
.5f,-.5f,-.5f,.02f,-.02f,-.02f,   .5f,-.5f,-.5f,-.02f,.02f,-.02f,   .5f,.5f,-.5f,-.02f,-.02f,-.02f,
.5f,-.5f,-.5f,.02f,-.02f,-.02f,   .5f,.5f,-.5f,-.02f,-.02f,-.02f,   .5f,.5f,-.5f,.02f,.02f,-.02f,
.5f,-.5f,-.5f,-.02f,.02f,-.02f,   .5f,-.5f,-.5f,-.02f,.02f,.02f,   .5f,.5f,-.5f,-.02f,-.02f,.02f,
.5f,-.5f,-.5f,-.02f,.02f,-.02f,   .5f,.5f,-.5f,-.02f,-.02f,.02f,   .5f,.5f,-.5f,-.02f,-.02f,-.02f,
.5f,.5f,-.5f,.02f,.02f,-.02f,   .5f,.5f,-.5f,-.02f,-.02f,-.02f,   -.5f,.5f,-.5f,.02f,-.02f,-.02f,
.5f,.5f,-.5f,.02f,.02f,-.02f,   -.5f,.5f,-.5f,.02f,-.02f,-.02f,   -.5f,.5f,-.5f,-.02f,.02f,-.02f,
.5f,.5f,-.5f,-.02f,-.02f,-.02f,   .5f,.5f,-.5f,-.02f,-.02f,.02f,   -.5f,.5f,-.5f,.02f,-.02f,.02f,
.5f,.5f,-.5f,-.02f,-.02f,-.02f,   -.5f,.5f,-.5f,.02f,-.02f,.02f,   -.5f,.5f,-.5f,.02f,-.02f,-.02f,
-.5f,.5f,-.5f,-.02f,.02f,-.02f,   -.5f,.5f,-.5f,.02f,-.02f,-.02f,   -.5f,-.5f,-.5f,.02f,.02f,-.02f,
-.5f,.5f,-.5f,-.02f,.02f,-.02f,   -.5f,-.5f,-.5f,.02f,.02f,-.02f,   -.5f,-.5f,-.5f,-.02f,-.02f,-.02f,
-.5f,.5f,-.5f,.02f,-.02f,-.02f,   -.5f,.5f,-.5f,.02f,-.02f,.02f,   -.5f,-.5f,-.5f,.02f,.02f,.02f,
-.5f,.5f,-.5f,.02f,-.02f,-.02f,   -.5f,-.5f,-.5f,.02f,.02f,.02f,   -.5f,-.5f,-.5f,.02f,.02f,-.02f,

//RIGHT SIDE
.5f,-.5f,-.5f,.02f,-.02f,-.02f,   .5f,-.5f,-.5f,.02f,.02f,.02f,   .5f,-.5f,.5f,.02f,.02f,-.02f,
.5f,-.5f,-.5f,.02f,-.02f,-.02f,   .5f,-.5f,.5f,.02f,.02f,-.02f,   .5f,-.5f,.5f,.02f,-.02f,.02f,
.5f,-.5f,-.5f,.02f,.02f,.02f,   .5f,-.5f,-.5f,-.02f,.02f,.02f,   .5f,-.5f,.5f,-.02f,.02f,-.02f,
.5f,-.5f,-.5f,.02f,.02f,.02f,   .5f,-.5f,.5f,-.02f,.02f,-.02f,   .5f,-.5f,.5f,.02f,.02f,-.02f,
.5f,-.5f,.5f,.02f,-.02f,.02f,   .5f,-.5f,.5f,.02f,.02f,-.02f,   .5f,.5f,.5f,.02f,-.02f,-.02f,
.5f,-.5f,.5f,.02f,-.02f,.02f,   .5f,.5f,.5f,.02f,-.02f,-.02f,   .5f,.5f,.5f,.02f,.02f,.02f,
.5f,-.5f,.5f,.02f,.02f,-.02f,   .5f,-.5f,.5f,-.02f,.02f,-.02f,   .5f,.5f,.5f,-.02f,-.02f,-.02f,
.5f,-.5f,.5f,.02f,.02f,-.02f,   .5f,.5f,.5f,-.02f,-.02f,-.02f,   .5f,.5f,.5f,.02f,-.02f,-.02f,
.5f,.5f,.5f,.02f,.02f,.02f,   .5f,.5f,.5f,.02f,-.02f,-.02f,   .5f,.5f,-.5f,.02f,-.02f,.02f,
.5f,.5f,.5f,.02f,.02f,.02f,   .5f,.5f,-.5f,.02f,-.02f,.02f,   .5f,.5f,-.5f,.02f,.02f,-.02f,
.5f,.5f,.5f,.02f,-.02f,-.02f,   .5f,.5f,.5f,-.02f,-.02f,-.02f,   .5f,.5f,-.5f,-.02f,-.02f,.02f,
.5f,.5f,.5f,.02f,-.02f,-.02f,   .5f,.5f,-.5f,-.02f,-.02f,.02f,   .5f,.5f,-.5f,.02f,-.02f,.02f,
.5f,.5f,-.5f,.02f,.02f,-.02f,   .5f,.5f,-.5f,.02f,-.02f,.02f,   .5f,-.5f,-.5f,.02f,.02f,.02f,
.5f,.5f,-.5f,.02f,.02f,-.02f,   .5f,-.5f,-.5f,.02f,.02f,.02f,   .5f,-.5f,-.5f,.02f,-.02f,-.02f,
.5f,.5f,-.5f,.02f,-.02f,.02f,   .5f,.5f,-.5f,-.02f,-.02f,.02f,   .5f,-.5f,-.5f,-.02f,.02f,.02f,
.5f,.5f,-.5f,.02f,-.02f,.02f,   .5f,-.5f,-.5f,-.02f,.02f,.02f,   .5f,-.5f,-.5f,.02f,.02f,.02f,

//BACK SIDE
.5f,-.5f,.5f,.02f,-.02f,.02f,   .5f,-.5f,.5f,-.02f,.02f,.02f,   -.5f,-.5f,.5f,.02f,.02f,.02f,
.5f,-.5f,.5f,.02f,-.02f,.02f,   -.5f,-.5f,.5f,.02f,.02f,.02f,   -.5f,-.5f,.5f,-.02f,-.02f,.02f,
.5f,-.5f,.5f,-.02f,.02f,.02f,   .5f,-.5f,.5f,-.02f,.02f,-.02f,   -.5f,-.5f,.5f,.02f,.02f,-.02f,
.5f,-.5f,.5f,-.02f,.02f,.02f,   -.5f,-.5f,.5f,.02f,.02f,-.02f,   -.5f,-.5f,.5f,.02f,.02f,.02f,
-.5f,-.5f,.5f,-.02f,-.02f,.02f,   -.5f,-.5f,.5f,.02f,.02f,.02f,   -.5f,.5f,.5f,.02f,-.02f,.02f,
-.5f,-.5f,.5f,-.02f,-.02f,.02f,   -.5f,.5f,.5f,.02f,-.02f,.02f,   -.5f,.5f,.5f,-.02f,.02f,.02f,
-.5f,-.5f,.5f,.02f,.02f,.02f,   -.5f,-.5f,.5f,.02f,.02f,-.02f,   -.5f,.5f,.5f,.02f,-.02f,-.02f,
-.5f,-.5f,.5f,.02f,.02f,.02f,   -.5f,.5f,.5f,.02f,-.02f,-.02f,   -.5f,.5f,.5f,.02f,-.02f,.02f,
-.5f,.5f,.5f,-.02f,.02f,.02f,   -.5f,.5f,.5f,.02f,-.02f,.02f,   .5f,.5f,.5f,-.02f,-.02f,.02f,
-.5f,.5f,.5f,-.02f,.02f,.02f,   .5f,.5f,.5f,-.02f,-.02f,.02f,   .5f,.5f,.5f,.02f,.02f,.02f,
-.5f,.5f,.5f,.02f,-.02f,.02f,   -.5f,.5f,.5f,.02f,-.02f,-.02f,   .5f,.5f,.5f,-.02f,-.02f,-.02f,
-.5f,.5f,.5f,.02f,-.02f,.02f,   .5f,.5f,.5f,-.02f,-.02f,-.02f,   .5f,.5f,.5f,-.02f,-.02f,.02f,
.5f,.5f,.5f,.02f,.02f,.02f,   .5f,.5f,.5f,-.02f,-.02f,.02f,   .5f,-.5f,.5f,-.02f,.02f,.02f,
.5f,.5f,.5f,.02f,.02f,.02f,   .5f,-.5f,.5f,-.02f,.02f,.02f,   .5f,-.5f,.5f,.02f,-.02f,.02f,
.5f,.5f,.5f,-.02f,-.02f,.02f,   .5f,.5f,.5f,-.02f,-.02f,-.02f,   .5f,-.5f,.5f,-.02f,.02f,-.02f,
.5f,.5f,.5f,-.02f,-.02f,.02f,   .5f,-.5f,.5f,-.02f,.02f,-.02f,   .5f,-.5f,.5f,-.02f,.02f,.02f,

//LEFT SIDE
-.5f,-.5f,.5f,-.02f,-.02f,.02f,   -.5f,-.5f,.5f,-.02f,.02f,-.02f,   -.5f,-.5f,-.5f,-.02f,.02f,.02f,
-.5f,-.5f,.5f,-.02f,-.02f,.02f,   -.5f,-.5f,-.5f,-.02f,.02f,.02f,   -.5f,-.5f,-.5f,-.02f,-.02f,-.02f,
-.5f,-.5f,.5f,-.02f,.02f,-.02f,   -.5f,-.5f,.5f,.02f,.02f,-.02f,   -.5f,-.5f,-.5f,.02f,.02f,.02f,
-.5f,-.5f,.5f,-.02f,.02f,-.02f,   -.5f,-.5f,-.5f,.02f,.02f,.02f,   -.5f,-.5f,-.5f,-.02f,.02f,.02f,
-.5f,-.5f,-.5f,-.02f,-.02f,-.02f,   -.5f,-.5f,-.5f,-.02f,.02f,.02f,   -.5f,.5f,-.5f,-.02f,-.02f,.02f,
-.5f,-.5f,-.5f,-.02f,-.02f,-.02f,   -.5f,.5f,-.5f,-.02f,-.02f,.02f,   -.5f,.5f,-.5f,-.02f,.02f,-.02f,
-.5f,-.5f,-.5f,-.02f,.02f,.02f,   -.5f,-.5f,-.5f,.02f,.02f,.02f,   -.5f,.5f,-.5f,.02f,-.02f,.02f,
-.5f,-.5f,-.5f,-.02f,.02f,.02f,   -.5f,.5f,-.5f,.02f,-.02f,.02f,   -.5f,.5f,-.5f,-.02f,-.02f,.02f,
-.5f,.5f,-.5f,-.02f,.02f,-.02f,   -.5f,.5f,-.5f,-.02f,-.02f,.02f,   -.5f,.5f,.5f,-.02f,-.02f,-.02f,
-.5f,.5f,-.5f,-.02f,.02f,-.02f,   -.5f,.5f,.5f,-.02f,-.02f,-.02f,   -.5f,.5f,.5f,-.02f,.02f,.02f,
-.5f,.5f,-.5f,-.02f,-.02f,.02f,   -.5f,.5f,-.5f,.02f,-.02f,.02f,   -.5f,.5f,.5f,.02f,-.02f,-.02f,
-.5f,.5f,-.5f,-.02f,-.02f,.02f,   -.5f,.5f,.5f,.02f,-.02f,-.02f,   -.5f,.5f,.5f,-.02f,-.02f,-.02f,
-.5f,.5f,.5f,-.02f,.02f,.02f,   -.5f,.5f,.5f,-.02f,-.02f,-.02f,   -.5f,-.5f,.5f,-.02f,.02f,-.02f,
-.5f,.5f,.5f,-.02f,.02f,.02f,   -.5f,-.5f,.5f,-.02f,.02f,-.02f,   -.5f,-.5f,.5f,-.02f,-.02f,.02f,
-.5f,.5f,.5f,-.02f,-.02f,-.02f,   -.5f,.5f,.5f,.02f,-.02f,-.02f,   -.5f,-.5f,.5f,.02f,.02f,-.02f,
-.5f,.5f,.5f,-.02f,-.02f,-.02f,   -.5f,-.5f,.5f,.02f,.02f,-.02f,   -.5f,-.5f,.5f,-.02f,.02f,-.02f,

//TOP SIDE
-.5f,.5f,-.5f,-.02f,.02f,-.02f,   -.5f,.5f,-.5f,.02f,.02f,.02f,   .5f,.5f,-.5f,-.02f,.02f,.02f,
-.5f,.5f,-.5f,-.02f,.02f,-.02f,   .5f,.5f,-.5f,-.02f,.02f,.02f,   .5f,.5f,-.5f,.02f,.02f,-.02f,
-.5f,.5f,-.5f,.02f,.02f,.02f,   -.5f,.5f,-.5f,.02f,-.02f,.02f,   .5f,.5f,-.5f,-.02f,-.02f,.02f,
-.5f,.5f,-.5f,.02f,.02f,.02f,   .5f,.5f,-.5f,-.02f,-.02f,.02f,   .5f,.5f,-.5f,-.02f,.02f,.02f,
.5f,.5f,-.5f,.02f,.02f,-.02f,   .5f,.5f,-.5f,-.02f,.02f,.02f,   .5f,.5f,.5f,-.02f,.02f,-.02f,
.5f,.5f,-.5f,.02f,.02f,-.02f,   .5f,.5f,.5f,-.02f,.02f,-.02f,   .5f,.5f,.5f,.02f,.02f,.02f,
.5f,.5f,-.5f,-.02f,.02f,.02f,   .5f,.5f,-.5f,-.02f,-.02f,.02f,   .5f,.5f,.5f,-.02f,-.02f,-.02f,
.5f,.5f,-.5f,-.02f,.02f,.02f,   .5f,.5f,.5f,-.02f,-.02f,-.02f,   .5f,.5f,.5f,-.02f,.02f,-.02f,
.5f,.5f,.5f,.02f,.02f,.02f,   .5f,.5f,.5f,-.02f,.02f,-.02f,   -.5f,.5f,.5f,.02f,.02f,-.02f,
.5f,.5f,.5f,.02f,.02f,.02f,   -.5f,.5f,.5f,.02f,.02f,-.02f,   -.5f,.5f,.5f,-.02f,.02f,.02f,
.5f,.5f,.5f,-.02f,.02f,-.02f,   .5f,.5f,.5f,-.02f,-.02f,-.02f,   -.5f,.5f,.5f,.02f,-.02f,-.02f,
.5f,.5f,.5f,-.02f,.02f,-.02f,   -.5f,.5f,.5f,.02f,-.02f,-.02f,   -.5f,.5f,.5f,.02f,.02f,-.02f,
-.5f,.5f,.5f,-.02f,.02f,.02f,   -.5f,.5f,.5f,.02f,.02f,-.02f,   -.5f,.5f,-.5f,.02f,.02f,.02f,
-.5f,.5f,.5f,-.02f,.02f,.02f,   -.5f,.5f,-.5f,.02f,.02f,.02f,   -.5f,.5f,-.5f,-.02f,.02f,-.02f,
-.5f,.5f,.5f,.02f,.02f,-.02f,   -.5f,.5f,.5f,.02f,-.02f,-.02f,   -.5f,.5f,-.5f,.02f,-.02f,.02f,
-.5f,.5f,.5f,.02f,.02f,-.02f,   -.5f,.5f,-.5f,.02f,-.02f,.02f,   -.5f,.5f,-.5f,.02f,.02f,.02f,

//BOTTOM SIDE
-.5f,-.5f,.5f,-.02f,-.02f,.02f,   -.5f,-.5f,.5f,.02f,-.02f,-.02f,   .5f,-.5f,.5f,-.02f,-.02f,-.02f,
-.5f,-.5f,.5f,-.02f,-.02f,.02f,   .5f,-.5f,.5f,-.02f,-.02f,-.02f,   .5f,-.5f,.5f,.02f,-.02f,.02f,
-.5f,-.5f,.5f,.02f,-.02f,-.02f,   -.5f,-.5f,.5f,.02f,.02f,-.02f,   .5f,-.5f,.5f,-.02f,.02f,-.02f,
-.5f,-.5f,.5f,.02f,-.02f,-.02f,   .5f,-.5f,.5f,-.02f,.02f,-.02f,   .5f,-.5f,.5f,-.02f,-.02f,-.02f,
.5f,-.5f,.5f,.02f,-.02f,.02f,   .5f,-.5f,.5f,-.02f,-.02f,-.02f,   .5f,-.5f,-.5f,-.02f,-.02f,.02f,
.5f,-.5f,.5f,.02f,-.02f,.02f,   .5f,-.5f,-.5f,-.02f,-.02f,.02f,   .5f,-.5f,-.5f,.02f,-.02f,-.02f,
.5f,-.5f,.5f,-.02f,-.02f,-.02f,   .5f,-.5f,.5f,-.02f,.02f,-.02f,   .5f,-.5f,-.5f,-.02f,.02f,.02f,
.5f,-.5f,.5f,-.02f,-.02f,-.02f,   .5f,-.5f,-.5f,-.02f,.02f,.02f,   .5f,-.5f,-.5f,-.02f,-.02f,.02f,
.5f,-.5f,-.5f,.02f,-.02f,-.02f,   .5f,-.5f,-.5f,-.02f,-.02f,.02f,   -.5f,-.5f,-.5f,.02f,-.02f,.02f,
.5f,-.5f,-.5f,.02f,-.02f,-.02f,   -.5f,-.5f,-.5f,.02f,-.02f,.02f,   -.5f,-.5f,-.5f,-.02f,-.02f,-.02f,
.5f,-.5f,-.5f,-.02f,-.02f,.02f,   .5f,-.5f,-.5f,-.02f,.02f,.02f,   -.5f,-.5f,-.5f,.02f,.02f,.02f,
.5f,-.5f,-.5f,-.02f,-.02f,.02f,   -.5f,-.5f,-.5f,.02f,.02f,.02f,   -.5f,-.5f,-.5f,.02f,-.02f,.02f,
-.5f,-.5f,-.5f,-.02f,-.02f,-.02f,   -.5f,-.5f,-.5f,.02f,-.02f,.02f,   -.5f,-.5f,.5f,.02f,-.02f,-.02f,
-.5f,-.5f,-.5f,-.02f,-.02f,-.02f,   -.5f,-.5f,.5f,.02f,-.02f,-.02f,   -.5f,-.5f,.5f,-.02f,-.02f,.02f,
-.5f,-.5f,-.5f,.02f,-.02f,.02f,   -.5f,-.5f,-.5f,.02f,.02f,.02f,   -.5f,-.5f,.5f,.02f,.02f,-.02f,
-.5f,-.5f,-.5f,.02f,-.02f,.02f,   -.5f,-.5f,.5f,.02f,.02f,-.02f,   -.5f,-.5f,.5f,.02f,-.02f,-.02f,

// -.5f,-.5f,-.5f,0f,0f,0f,   -.5f,.5f,-.5f,0f,0f,0f,   .5f,.5f,-.5f,0f,0f,0f,
// -.5f,-.5f,-.5f,0f,0f,0f,   .5f,.5f,-.5f,0f,0f,0f,   .5f,-.5f,-.5f,0f,0f,0f,
// actually screw it i have the script i'm gonna get my bang for my buck
/*script:
for i=1,1 do
local inp={
-.5,-.5,-.5,-.001,-.001,-.001,   -.5,.5,-.5,-.001,.001,-.001,   .5,.5,-.5,.001,.001,-.001,
-.5,-.5,-.5,-.001,-.001,-.001,   .5,.5,-.5,.001,.001,-.001,   .5,-.5,-.5,.001,-.001,-.001,}
local num=#inp
local function printinp()for i=1,num,18 do print(inp[i].."f,"..inp[i+1].."f,"..inp[i+2].."f,"..inp[i+3].."f,"..inp[i+4].."f,"..inp[i+5].."f,   "..inp[i+6].."f,"..inp[i+7].."f,"..inp[i+8].."f,"..inp[i+9].."f,"..inp[i+10].."f,"..inp[i+11].."f,   "..inp[i+12].."f,"..inp[i+13].."f,"..inp[i+14].."f,"..inp[i+15].."f,"..inp[i+16].."f,"..inp[i+17].."f,") end end
print("\n// FRONT SIDE")printinp()
local function rot()for i=1,num,3 do inp[i],inp[i+2]=-inp[i+2],inp[i]end end rot()
print("\n//RIGHT SIDE")printinp()rot()
print("\n//BACK SIDE")printinp()rot()
print("\n//LEFT SIDE")printinp()rot()
for i=2,num,3 do inp[i],inp[i+1]=-inp[i+1],inp[i]end
print("\n//TOP SIDE")printinp()
for i=2,num,3 do inp[i],inp[i+1]=-inp[i],-inp[i+1]end
print("\n//BOTTOM SIDE")printinp()end*/
// FRONT SIDE
-.5f,-.5f,-.5f,-.001f,-.001f,-.001f,   -.5f,.5f,-.5f,-.001f,.001f,-.001f,   .5f,.5f,-.5f,.001f,.001f,-.001f,
-.5f,-.5f,-.5f,-.001f,-.001f,-.001f,   .5f,.5f,-.5f,.001f,.001f,-.001f,   .5f,-.5f,-.5f,.001f,-.001f,-.001f,

//RIGHT SIDE
.5f,-.5f,-.5f,.001f,-.001f,-.001f,   .5f,.5f,-.5f,.001f,.001f,-.001f,   .5f,.5f,.5f,.001f,.001f,.001f,
.5f,-.5f,-.5f,.001f,-.001f,-.001f,   .5f,.5f,.5f,.001f,.001f,.001f,   .5f,-.5f,.5f,.001f,-.001f,.001f,

//BACK SIDE
.5f,-.5f,.5f,.001f,-.001f,.001f,   .5f,.5f,.5f,.001f,.001f,.001f,   -.5f,.5f,.5f,-.001f,.001f,.001f,
.5f,-.5f,.5f,.001f,-.001f,.001f,   -.5f,.5f,.5f,-.001f,.001f,.001f,   -.5f,-.5f,.5f,-.001f,-.001f,.001f,

//LEFT SIDE
-.5f,-.5f,.5f,-.001f,-.001f,.001f,   -.5f,.5f,.5f,-.001f,.001f,.001f,   -.5f,.5f,-.5f,-.001f,.001f,-.001f,
-.5f,-.5f,.5f,-.001f,-.001f,.001f,   -.5f,.5f,-.5f,-.001f,.001f,-.001f,   -.5f,-.5f,-.5f,-.001f,-.001f,-.001f,

//TOP SIDE
-.5f,.5f,-.5f,-.001f,.001f,-.001f,   -.5f,.5f,.5f,-.001f,.001f,.001f,   .5f,.5f,.5f,.001f,.001f,.001f,
-.5f,.5f,-.5f,-.001f,.001f,-.001f,   .5f,.5f,.5f,.001f,.001f,.001f,   .5f,.5f,-.5f,.001f,.001f,-.001f,

//BOTTOM SIDE
-.5f,-.5f,.5f,-.001f,-.001f,.001f,   -.5f,-.5f,-.5f,-.001f,-.001f,-.001f,   .5f,-.5f,-.5f,.001f,-.001f,-.001f,
-.5f,-.5f,.5f,-.001f,-.001f,.001f,   .5f,-.5f,-.5f,.001f,-.001f,-.001f,   .5f,-.5f,.5f,.001f,-.001f,.001f,
			];
			public static uint[] SelBoxColorData = [ // 1x color
0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,
0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,
0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,
0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,
0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,
0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,
0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,
0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,
0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,
0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,
0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,
0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,
0x7f7f7f7f,0x7f7f7f7f,0x7f7f7f7f,0x7f7f7f7f,0x7f7f7f7f,0x7f7f7f7f,0x7f7f7f7f,0x7f7f7f7f,0x7f7f7f7f];
			public struct SelBoxDataStruct {
				public uint color;
				public float sx,sy,sz,rx,ry,rz,px,py,pz;
			}
			public static void OnLoad(Game game) {
				// shader = new("Shaders/babfte/debug.vert","Shaders/babfte/debug.frag");
				// GL.UseProgram(shader.Handle);
				// GL.PointSize(8);
				// VAO = GL.GenVertexArray();
				// GL.BindVertexArray(VAO);

				// VBO = GL.GenBuffer();
				// GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
				// GL.BufferData(BufferTarget.ArrayBuffer, BlockStuff.BulkDrawConst * sizeof(float) * 3, 0, BufferUsageHint.DynamicDraw);

				// GL.EnableVertexAttribArray(0);
				// GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

				// compShader = new("Shaders/babfte/Shader.comp");
				// CSVAO = GL.GenVertexArray();
				// GL.BindVertexArray(CSVAO);

				// CSSSBO = GL.GenBuffer();
				// GL.BindBuffer(BufferTarget.ShaderStorageBuffer, CSSSBO);
				// GL.BufferData(BufferTarget.ShaderStorageBuffer, (BlockStuff.BulkDrawConst + 4) * sizeof(float) * 3, 0, BufferUsageHint.DynamicDraw);
				BiToolShader = new("Shaders/babfte/BindTool.vert","Shaders/babfte/BindTool.frag");
				GL.UseProgram(BiToolShader.Handle);
				BiToolVAO = GL.GenVertexArray();
				GL.BindVertexArray(BiToolVAO);

				BiToolVBO = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ArrayBuffer, BiToolVBO);
				// GL.BufferData(BufferTarget.ArrayBuffer, BlockStuff.BulkDrawConst * sizeof(float) * 3, 0, BufferUsageHint.DynamicDraw);
				GL.BufferData(BufferTarget.ArrayBuffer, BlockStuff.BulkDrawConst * sizeof(float) * 6, 0, BufferUsageHint.DynamicDraw);

				// BiToolEBO = GL.GenBuffer();
				// GL.BindBuffer(BufferTarget.ElementArrayBuffer, BiToolEBO);
				// GL.BufferData(BufferTarget.ArrayBuffer, BlockStuff.BulkDrawConst * sizeof(int), 0, BufferUsageHint.DynamicDraw);

				GL.EnableVertexAttribArray(0);
				GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
				GL.EnableVertexAttribArray(1);
				GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
				GL.VertexAttribDivisor(0, 1);
				GL.VertexAttribDivisor(1, 1);

				OutlBoxShader = new("Shaders/babfte/OutlineBox.vert", "Shaders/babfte/OutlineBox.frag");
				GL.UseProgram(OutlBoxShader.Handle);
				OutlBoxVAO = GL.GenVertexArray();
				GL.BindVertexArray(OutlBoxVAO);

				OutlBoxVBO1 = GL.GenBuffer(); // vbo for general selection box pt data
				GL.BindBuffer(BufferTarget.ArrayBuffer, OutlBoxVBO1);
				GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * /*6 * 16 * 6*/SelBoxPointData.Length+sizeof(uint)*SelBoxColorData.Length, SelBoxPointData, BufferUsageHint.StaticDraw);
				GL.BufferSubData(BufferTarget.ArrayBuffer, sizeof(float)*SelBoxPointData.Length, sizeof(uint)*SelBoxColorData.Length, SelBoxColorData);
				GL.EnableVertexAttribArray(0);
				GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0); // position scale
				GL.EnableVertexAttribArray(1);
				GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float)); // position offset
				GL.EnableVertexAttribArray(2);
				GL.VertexAttribPointer(2, 1, VertexAttribPointerType.UnsignedByte, false, sizeof(byte), sizeof(float)*SelBoxPointData.Length); // transparency
				OutlInstVBO = GL.GenBuffer(); // vbo for the selection boxes
				GL.BindBuffer(BufferTarget.ArrayBuffer, OutlInstVBO);
				GL.BufferData(BufferTarget.ArrayBuffer, BlockStuff.BulkDrawConst * (sizeof(float) * 9+sizeof(uint)), 0, BufferUsageHint.DynamicDraw);

				GL.EnableVertexAttribArray(3);
				GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, false, 40, 0); // box color
				GL.EnableVertexAttribArray(4);
				GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 40, 4); // box size
				GL.EnableVertexAttribArray(5);
				// GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, 64, 16); // transformation mat col 0
				GL.VertexAttribPointer(5, 3, VertexAttribPointerType.Float, false, 40, 16); // rotation
				GL.EnableVertexAttribArray(6);
				// GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, 64, 32); // transformation mat col 1
				GL.VertexAttribPointer(6, 3, VertexAttribPointerType.Float, false, 40, 28); // position
				// GL.EnableVertexAttribArray(7);
				// GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, 64, 48); // transformation mat col 2
				GL.VertexAttribDivisor(3, 1);
				GL.VertexAttribDivisor(4, 1);
				GL.VertexAttribDivisor(5, 1);
				GL.VertexAttribDivisor(6, 1);
				// GL.VertexAttribDivisor(7, 1);
			}
			public static void OnRenderFrame(Game game) {
				// int amt = ptsList.Count;
				// switch (amt) {
				// 	case 0: break;
				// 	case 1:
				// 		if((frame&127)==0)Console.Write("rendering thing\n");
				// 		GL.UseProgram(compShader.Handle);
				// 		GL.BindVertexArray(CSVAO);
				// 		GL.BindBuffer(BufferTarget.ShaderStorageBuffer, CSSSBO);
				// 		Vector3[] arr = ptsList[0];
				// 		GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, 0, (BlockStuff.BulkDrawConst+4) * sizeof(float) * 3, arr);
				// 		if (ASJFDKJDFKSDJ){StringBuilder s = new("\n\nbuffer after:\n");
				// 			for (int i = 0; i < 128; i++) s.Append(arr[i]); s.Append('\n');
				// 			Console.Write(s.ToString());ASJFDKJDFKSDJ=false;}
				// 		GL.UseProgram(shader.Handle);
				// 		GL.BindVertexArray(VAO);
				// 		GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
				// 		GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float)*3*BlockStuff.BulkDrawConst, arr[4..]);
				// 		GL.DrawArrays(PrimitiveType.Points, 0, BlockStuff.BulkDrawConst);
				// 		// Vector3[] list = new Vector3[BlockStuff.BulkDrawConst];
				// 		// for (int i = 0; i < 20; i++) list[i]=(Random.Shared.NextSingle()*2f-1f,Random.Shared.NextSingle()*4f-2f,Random.Shared.NextSingle()*2f-1f);
				// 		// GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float)*3*16, list);
				// 		// GL.DrawArrays(PrimitiveType.Points, 0, 16);
				// 		break;
				// 	default:
				// 		throw new Exception("oopsies, not made yet :3");
				// 		// break;
				// }
				switch (ToolEquipped) {
					case 4:
						GL.UseProgram(BiToolShader.Handle);
						GL.BindVertexArray(BiToolVAO);
						GL.BindBuffer(BufferTarget.ArrayBuffer, BiToolVBO);
						BiToolShader.SetMatrix4("view", currentView);
						HashSet<IBindableBlock> AllBiBlocks = BiBS.AllBindableBlocks;
						// int amt = AllBiBlocks.Count;
						const int BDCX2 = BlockStuff.BulkDrawConst<<1; // because i don't want VSCODE COLORING ALMOST EVERYTHING AFTER THAT SWITCH STATEMENT IN YELLOW DFJSFJWEJFWIOFJ
						Vector3[] list = new Vector3[BDCX2];

						int i = 1;
						foreach (IBindableBlock block in AllBiBlocks) {
							HashSet<IBindableBlock> bOuts = block.OutputsHashSet;
							int count = bOuts.Count;
							if (count > 0) {
								int countThing = count<<1;
								switch (i + countThing) {
									case > BDCX2+1:
										// throw new Exception("oopsies too many binds, i haven't implemented bulk drawing binds yet :3");
										// remaining of the first one
										Vector3 pos = block.Pos;
										int amtThingy = BDCX2-(i-1);
										Array.Fill(list, pos, i-1, amtThingy);
										IBindableBlock[] arr = new IBindableBlock[count];
										bOuts.CopyTo(arr);
										for (int j = 0; j < amtThingy>>1; j++, i+=2) list[i] = arr[j].Pos;
										// render
										GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float)*6*BlockStuff.BulkDrawConst, list);
										GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, BlockStuff.BulkDrawConst);

										// repeat full array until done or smth
										Array.Fill(list, pos, 0, BDCX2-amtThingy);
										int k = amtThingy;
										for (; k < count-BDCX2; k+=BDCX2) {
											i = 1;
											for (int L = k; L < k+BDCX2; L++, i+=2) list[i] = arr[L].Pos;
											GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float)*6*BlockStuff.BulkDrawConst, list);
											GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, BlockStuff.BulkDrawConst);
										}
										for (i = 1; k < count; k++, i+=2) list[i] = arr[k].Pos;
										break;
									case BDCX2+1:
										Array.Fill(list, block.Pos, i-1, countThing);
										foreach (IBindableBlock b in bOuts) {
											list[i] = b.Pos;
											i += 2;
										}
										GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float)*6*BlockStuff.BulkDrawConst, list);
										GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, BlockStuff.BulkDrawConst);
										i = 1;
										break;
									default:
										Array.Fill(list, block.Pos, i-1, countThing);
										foreach (IBindableBlock b in bOuts) {
											list[i] = b.Pos;
											i += 2;
										}
										break;
								}
							}
						}
						if (i > 1) { if ((frame&127)==0) Console.WriteLine("rendering binds");
							GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float)*3*(i-1), list);
							GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, i>>1);
						}
						break;
				}
				// int OutlObjCount = OutlinedObjects.Count;
				// if (OutlObjCount > 0) {Console.Write("rendering the selection boxes.");
				// 	GL.UseProgram(OutlBoxShader.Handle);
				// 	GL.BindVertexArray(OutlBoxVAO);
				// 	GL.BindBuffer(BufferTarget.ArrayBuffer, OutlInstVBO);
				// 	OutlBoxShader.SetMatrix4("v", currentView);
				// 	switch (OutlObjCount) {
				// 		case > BlockStuff.BulkDrawConst: {
				// 			float[] array = new float[BlockStuff.BulkDrawConst*15];
				// 			uint[] colors = new uint[BlockStuff.BulkDrawConst];
				// 			int i = 0, i2 = 0;
				// 			foreach (KeyValuePair<IBlock, OutlineType> a in OutlinedObjects) {
				// 				IBlock block = a.Key;
				// 				// Matrix4 Mat = DataStuff.CreateRotationXYZ(block.Rot) * Matrix4.CreateTranslation(block.Pos);
				// 				// (float sizeX, float sizeY, float sizeZ) = block.Size;
				// 				// new float[] {sizeX, sizeY, sizeZ, Mat.Row0.X, Mat.Row1.X, Mat.Row2.X, Mat.Row3.X, Mat.Row0.Y, Mat.Row1.Y, Mat.Row2.Y, Mat.Row3.Y, Mat.Row0.Z, Mat.Row1.Z, Mat.Row2.Z, Mat.Row3.Z, Mat.Row0.W, Mat.Row1.W, Mat.Row2.W, Mat.Row3.W}.CopyTo(array, i);
				// 				colors[i] = a.Value switch {
				// 					OutlineType.None => 0xff00007fu,
				// 					OutlineType.Selected => 0xffffbf00u,
				// 					OutlineType.Highlighted => 0xff00ff00u,
				// 					OutlineType.Highlighted | OutlineType.Selected => 0xffffff00u,
				// 					_ => 0xff0000ffu
				// 				};
				// 				(float rx, float ry, float rz) = block.Rot;
				// 				(float tx, float ty, float tz) = block.Pos;
				// 				float num = MathF.Cos(rx), num2 = MathF.Sin(rx),
				// 				num3 = MathF.Cos(ry), num4 = MathF.Sin(ry),
				// 				num5 = MathF.Cos(rz), num6 = MathF.Sin(rz);
				// 				float _x2 = num2 * num4, _x3 = num * num4;
				// 				// return new Matrix4(num3*num5,num3*num6,-num4,0,
				// 				// _x2*num5-num*num6,_x2*num6+num*num5,num2*num3,0,
				// 				// _x3*num5+num2*num6,_x3*num6-num2*num5,num*num3,0,
				// 				// tx,ty,tz,1);
				// 				(array[i2],array[i2+1],array[i2+2])=block.Size;
				// 				array[i2+3]=num3*num5;
				// 				array[i2+4]=_x2*num5-num*num6;
				// 				array[i2+5]=_x3*num5+num2*num6;
				// 				array[i2+6]=tx;
				// 				array[i2+7]=num3*num6;
				// 				array[i2+8]=_x2*num6+num*num5;
				// 				array[i2+9]=_x3*num6-num2*num5;
				// 				array[i2+10]=ty;
				// 				array[i2+11]=-num4;
				// 				array[i2+12]=num2*num3;
				// 				array[i2+13]=num*num3;
				// 				array[i2+14]=tz;
				// 				i++; i2+=15;
				// 				if (i == OutlObjCount) {
				// 					GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * BlockStuff.BulkDrawConst * 15, array);
				// 					GL.BufferSubData(BufferTarget.ArrayBuffer, sizeof(float)*BlockStuff.BulkDrawConst*15, sizeof(uint)*BlockStuff.BulkDrawConst, colors);
				// 					GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 324, BlockStuff.BulkDrawConst);
				// 					i = 0; i2 = 0;
				// 				}
				// 			}
				// 			if (i > 0) {
				// 				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * i2, array);
				// 				GL.BufferSubData(BufferTarget.ArrayBuffer, sizeof(float)*BlockStuff.BulkDrawConst*15, sizeof(uint)*i, colors);
				// 				GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 324, i);
				// 			}
				// 		} break;
				// 		default: {
				// 			float[] array = new float[OutlObjCount*15];
				// 			uint[] colors = new uint[OutlObjCount];
				// 			int i = 0, i2 = 0;
				// 			foreach (KeyValuePair<IBlock, OutlineType> a in OutlinedObjects) {
				// 				IBlock block = a.Key;
				// 				// Matrix4 Mat = DataStuff.CreateRotationXYZ(block.Rot) * Matrix4.CreateTranslation(block.Pos);
				// 				// (float sizeX, float sizeY, float sizeZ) = block.Size;
				// 				// new float[] {sizeX, sizeY, sizeZ, Mat.Row0.X, Mat.Row1.X, Mat.Row2.X, Mat.Row3.X, Mat.Row0.Y, Mat.Row1.Y, Mat.Row2.Y, Mat.Row3.Y, Mat.Row0.Z, Mat.Row1.Z, Mat.Row2.Z, Mat.Row3.Z, Mat.Row0.W, Mat.Row1.W, Mat.Row2.W, Mat.Row3.W}.CopyTo(array, i);
				// 				colors[i] = a.Value switch {
				// 					OutlineType.None => 0xff00007fu,
				// 					OutlineType.Selected => 0xffffbf00u,
				// 					OutlineType.Highlighted => 0xff00ff00u,
				// 					OutlineType.Highlighted | OutlineType.Selected => 0xffffff00u,
				// 					_ => 0xff0000ffu
				// 				};
				// 				(float rx, float ry, float rz) = block.Rot;
				// 				(float tx, float ty, float tz) = block.Pos;
				// 				float num = MathF.Cos(rx), num2 = MathF.Sin(rx),
				// 				num3 = MathF.Cos(ry), num4 = MathF.Sin(ry),
				// 				num5 = MathF.Cos(rz), num6 = MathF.Sin(rz);
				// 				float _x2 = num2 * num4, _x3 = num * num4;
				// 				// return new Matrix4(num3*num5,num3*num6,-num4,0,
				// 				// _x2*num5-num*num6,_x2*num6+num*num5,num2*num3,0,
				// 				// _x3*num5+num2*num6,_x3*num6-num2*num5,num*num3,0,
				// 				// tx,ty,tz,1);
				// 				(array[i2],array[i2+1],array[i2+2])=block.Size;
				// 				array[i2+3]=num3*num5;
				// 				array[i2+4]=_x2*num5-num*num6;
				// 				array[i2+5]=_x3*num5+num2*num6;
				// 				array[i2+6]=tx;
				// 				array[i2+7]=num3*num6;
				// 				array[i2+8]=_x2*num6+num*num5;
				// 				array[i2+9]=_x3*num6-num2*num5;
				// 				array[i2+10]=ty;
				// 				array[i2+11]=-num4;
				// 				array[i2+12]=num2*num3;
				// 				array[i2+13]=num*num3;
				// 				array[i2+14]=tz;
				// 				i++; i2+=15;
				// 			}
				// 			GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * i2, array);
				// 			GL.BufferSubData(BufferTarget.ArrayBuffer, sizeof(float)*BlockStuff.BulkDrawConst*15, sizeof(uint)*OutlObjCount, colors);
				// 			GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 324, OutlObjCount);
				// 		} break;
				// 	}
				// }
				// int m = 0;
				// foreach (IBlock block in BlockStuff.AllBlocks) {
				// 	if ((block.State&(BlockState.Selected|BlockState.Highlighted))>0) {
				// 		SelBoxesBuffer[m] = new SelBoxDataStruct();
				// 	}
				// }
				{
					GL.UseProgram(OutlBoxShader.Handle);
					GL.BindVertexArray(OutlBoxVAO);
					GL.BindBuffer(BufferTarget.ArrayBuffer, OutlInstVBO);
					OutlBoxShader.SetMatrix4("v", currentView);
					// float[] array = new float[BlockStuff.BulkDrawConst*9];
					// uint[] colors = new uint[BlockStuff.BulkDrawConst];
					int i = 0/*, i2 = 0*/;
					foreach (IBlock block in BlockStuff.AllBlocks) {
						if ((block.State&BlockState.Selected)==0) continue;
						// Matrix4 Mat = DataStuff.CreateRotationXYZ(block.Rot) * Matrix4.CreateTranslation(block.Pos);
						// (float sizeX, float sizeY, float sizeZ) = block.Size;
						// new float[] {sizeX, sizeY, sizeZ, Mat.Row0.X, Mat.Row1.X, Mat.Row2.X, Mat.Row3.X, Mat.Row0.Y, Mat.Row1.Y, Mat.Row2.Y, Mat.Row3.Y, Mat.Row0.Z, Mat.Row1.Z, Mat.Row2.Z, Mat.Row3.Z, Mat.Row0.W, Mat.Row1.W, Mat.Row2.W, Mat.Row3.W}.CopyTo(array, i);
						// colors[i] = block.OutlineState switch {
						// 	OutlineType.None => 0xff00007fu,
						// 	OutlineType.Selected => 0xffffbf00u,
						// 	OutlineType.Highlighted => 0xff00ff00u,
						// 	OutlineType.Highlighted | OutlineType.Selected => 0xffffff00u,
						// 	_ => 0xff0000ffu
						// };
						// (array[i2],array[i2+1],array[i2+2])=block.Size;
						// (array[i2+3],array[i2+4],array[i2+5])=block.Rot;
						// (array[i2+6],array[i2+7],array[i2+8])=block.Pos;
						ref SelBoxDataStruct thisSelBoxDataStruct = ref SelBoxesBuffer[i];
						thisSelBoxDataStruct.color = block.OutlineState switch {
							OutlineType.None => 0xff00007fu,
							OutlineType.Selected => 0xffffbf00u,
							OutlineType.Highlighted => 0xff00ff00u,
							OutlineType.Highlighted | OutlineType.Selected => 0xffffff00u,
							_ => 0xff0000ffu
						};
						// (array[i2],array[i2+1],array[i2+2])=block.Size;
						// (array[i2+3],array[i2+4],array[i2+5])=block.Rot;
						// (array[i2+6],array[i2+7],array[i2+8])=block.Pos;
						(thisSelBoxDataStruct.sx,thisSelBoxDataStruct.sy,thisSelBoxDataStruct.sz) = block.Size;
						(thisSelBoxDataStruct.rx,thisSelBoxDataStruct.ry,thisSelBoxDataStruct.rz) = block.Rot;
						(thisSelBoxDataStruct.px,thisSelBoxDataStruct.py,thisSelBoxDataStruct.pz) = block.Pos;
						i++;// i2+=9;
						// if (i == BlockStuff.BulkDrawConst) {
						// 	GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * BlockStuff.BulkDrawConst * 9, array);
						// 	GL.BufferSubData(BufferTarget.ArrayBuffer, sizeof(float)*BlockStuff.BulkDrawConst*9, sizeof(uint)*BlockStuff.BulkDrawConst, colors);
						// 	GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 324, BlockStuff.BulkDrawConst);
						// 	i = 0; i2 = 0;
						// }
						if (i == BlockStuff.BulkDrawConst) {
							GL.BufferSubData(BufferTarget.ArrayBuffer, 0, BlockStuff.BulkDrawConst * 40, SelBoxesBuffer);
							GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 324, BlockStuff.BulkDrawConst);
							i = 0;// i2 = 0;
						}
					}
					if (i > 0) {
						GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * 40 * i, SelBoxesBuffer);
						// GL.BufferSubData(BufferTarget.ArrayBuffer, sizeof(float)*BlockStuff.BulkDrawConst*15, sizeof(uint)*i, colors);
						GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 324, i);
					}
				}
			}
			// public static bool ASJFDKJDFKSDJ = false;
			public static List<IBindableBlock> BindToolSelList = [];
			public static List<IBindableBlock> BindToolBindList = [];
			public static IBindableBlock HighlightedBlock = null;
			public static HashSet<IBindableBlock> SelectedBlocks = [];
			public static readonly BiBS.Gate GhostBlockGateIshIDK = new BiBS.Gate(true);
			public static readonly Dictionary<Type, Action<Vector3>> PlaceBlockDict = new() {
				[typeof(BiBS.Gate)] = delegate(Vector3 pos) {BiBS.Gate newGate = new(pos) { Clr = (((uint)Random.Shared.Next())<<1)|255 };}
			};
			public static void OnUpdateFrame(Game game) {
				MouseState ms = game.MouseState;
				Vector2 mousePos = ms.Position;
				KeyboardState ks = game.KeyboardState;
				Vector2i CliSz = game._clientSize;
				bool ShiftSelect = ks[Keys.LeftShift] || ks[Keys.RightShift];
				// Matrix4 view = game._camera.View;
				Matrix4 view = currentView;
				switch (ToolEquipped) { // none, del, build, paint, bind, scale, prop, trow; DTool, BTool, PTool, BiTool, STool, PTool, TTool, MTool lol
					case 0: // none
						break;
					case 1: // del
						if (ms[MouseButton.Left]) {
							if (Dragging) {
								// Matrix4 otherview = game._camera.otherview;
								// Matrix4 projection = game._camera.Projection;
								// List<IBlock> AllBlocks = BlockStuff.AllBlocks;
								// int blockCount = AllBlocks.Count;
								// Vector3[] positions = new Vector3[blockCount];
								// Vector3[] matThingy = [(view.Row0.X, view.Row1.X, view.Row2.X), (view.Row3.X, view.Row0.Y, view.Row1.Y), (view.Row2.Y, view.Row3.Y, view.Row0.Z), (view.Row1.Z, view.Row2.Z, view.Row3.Z)];
								float z0=view.Row0.Z, w0=view.Row0.W,rawX0=view.Row0.X,rawY0=view.Row0.Y,
								x1=view.Row1.X/rawX0, y1=view.Row1.Y/rawY0, z1=view.Row1.Z, w1=view.Row1.W,
								x2=view.Row2.X/rawX0, y2=view.Row2.Y/rawY0, z2=view.Row2.Z, w2=view.Row2.W,
								x3=view.Row3.X/rawX0, y3=view.Row3.Y/rawY0, z3=view.Row3.Z, w3=view.Row3.W,
								// (float sx, float sy) = DragStart/CliSz;
								// (float ex, float ey) = mousePos/CliSz;
								// sx = (sx*2-1)/x0;sy = (sy*-2+1)/y0;ex = (ex*2-1)/x0;ey = (ey*-2+1)/y0; // idk for some reason the y axis is flipped
								sx = MathF.FusedMultiplyAdd(DragStart.X/CliSz.X,2,-1)/rawX0,
								ex = MathF.FusedMultiplyAdd(mousePos.X/CliSz.X,2,-1)/rawX0,
								sy = MathF.FusedMultiplyAdd(DragStart.Y/CliSz.Y,-2,1)/rawY0,
								ey = MathF.FusedMultiplyAdd(mousePos.Y/CliSz.Y,-2,1)/rawY0; // idk for some reason the y axis is flipped
								List<IBlock> AllBlocks = BlockStuff.AllBlocks;
								int AmtOfBlocks = AllBlocks.Count;
								// int diff = ((AmtOfBlocks+(BlockStuff.BulkDrawConst-1))>>BlockStuff.BulkDrawBS)-ptsList.Count;
								// if (diff>0) {do{ptsList.Add(new Vector3[BlockStuff.BulkDrawConst]);diff--;}while(diff>0);}
								if (ShiftSelect) {
									for (int i = AmtOfBlocks-1; i > -1; i--) {
										IBlock block = AllBlocks[i];
										// //result = new Vector4(vec.X * mat.Row0.X + vec.Y * mat.Row1.X + vec.Z * mat.Row2.X + vec.W * mat.Row3.X,
										// // vec.X * mat.Row0.Y + vec.Y * mat.Row1.Y + vec.Z * mat.Row2.Y + vec.W * mat.Row3.Y,
										// // vec.X * mat.Row0.Z + vec.Y * mat.Row1.Z + vec.Z * mat.Row2.Z + vec.W * mat.Row3.Z,
										// // vec.X * mat.Row0.W + vec.Y * mat.Row1.W + vec.Z * mat.Row2.W + vec.W * mat.Row3.W);
										// // (float x, float y, float z, float w) = new Vector4(block.Pos, 1) * view;
										// (float x, float y, float z) = block.Pos;
										// float w = x*w0+y*w1+z*w2+w3, wi = 1f/w;
										// (x,y,z)=((x+y*x1+z*x2+x3)*wi,
										// (x+y*y1+z*y2+y3)*wi,
										// x*z0+y*z1+z*z2+z3);
										// // x /= w; y /= w; z /= w; // OH, OH, OH MY GOD YEAAAH BOIIIIIIIIIIIII YEUUREUUUEUREUEEEAAEAAAAHHHSHSSSS HOLY [__] YES OMG FINALLY HOLY COW WOW SDJFKJSDF AAAAAAAAAAAA
										// // // sdfjsdfjsjdflkdjks
										Vector3 pos = block.Pos;
										float z=MathF.FusedMultiplyAdd(pos.X,z0,MathF.FusedMultiplyAdd(pos.Y,z1,MathF.FusedMultiplyAdd(pos.Z,z2,z3))),
										w=MathF.FusedMultiplyAdd(pos.X,w0,MathF.FusedMultiplyAdd(pos.Y,w1,MathF.FusedMultiplyAdd(pos.Z,w2,w3))), wi = 1f/w,
										x=(MathF.FusedMultiplyAdd(pos.Y,x1,pos.X)+MathF.FusedMultiplyAdd(pos.Z,x2,x3))*wi,
										y=(MathF.FusedMultiplyAdd(pos.Y,y1,pos.X)+MathF.FusedMultiplyAdd(pos.Z,y2,y3))*wi;
										if (z > -w && (x > sx ^ x > ex) && (y > sy ^ y > ey)) {
											block.State |= BlockState.Selected;
											block.OutlineState |= OutlineType.Selected;
										}
									}
								} else {
									for (int i = AmtOfBlocks-1; i > -1; i--) {
										IBlock block = AllBlocks[i];
										// (float x, float y, float z) = block.Pos;
										// float w = x*w0+y*w1+z*w2+w3, wi = 1f/w;
										// (x,y,z)=((x+y*x1+z*x2+x3)*wi,
										// (x+y*y1+z*y2+y3)*wi,
										// x*z0+y*z1+z*z2+z3);
										// // x /= w; y /= w; z /= w; // OH, OH, OH MY GOD YEAAAH BOIIIIIIIIIIIII YEUUREUUUEUREUEEEAAEAAAAHHHSHSSSS HOLY [__] YES OMG FINALLY HOLY COW WOW SDJFKJSDF AAAAAAAAAAAA
										// // // sdfjsdfjsjdflkdjks
										Vector3 pos = block.Pos;
										float w=MathF.FusedMultiplyAdd(pos.X,w0,MathF.FusedMultiplyAdd(pos.Y,w1,MathF.FusedMultiplyAdd(pos.Z,w2,w3))), wi = 1f/w;
										float x=(MathF.FusedMultiplyAdd(pos.Y,x1,pos.X)+MathF.FusedMultiplyAdd(pos.Z,x2,x3))*wi,
										y=(MathF.FusedMultiplyAdd(pos.Y,y1,pos.X)+MathF.FusedMultiplyAdd(pos.Z,y2,y3))*wi,
										z=MathF.FusedMultiplyAdd(pos.X,z0,MathF.FusedMultiplyAdd(pos.Y,z1,MathF.FusedMultiplyAdd(pos.Z,z2,z3)));

										// block.IsSelected = z > -w && (x > sx ^ x > ex) && (y > sy ^ y > ey);
										if (z > -w && (x > sx ^ x > ex) && (y > sy ^ y > ey)){
											block.OutlineState |= OutlineType.Selected;
											block.State |= BlockState.Selected;
										} else {
											block.OutlineState &= ~OutlineType.Selected;
											if (block.OutlineState == 0) block.State &= ~BlockState.Selected;
										}
									}
								}
								// for (int i = 0; i < blockCount; i++) {
								// 	positions[i] = AllBlocks[i].Pos;
								// }
								// int amtthing = (blockCount+(BlockStuff.BulkDrawConst-1))>>BlockStuff.BulkDrawBS;
								// if (amtthing > ptsList.Count) for (int i = ptsList.Count; i < amtthing; i++) ptsList[i] = new Vector3[ptsListElementLen];
								// for (int i = 0; i < amtthing; i++) {
								// 	Vector3[] listToDo = ptsList[i];
								// 	matThingy.CopyTo(listToDo, 0);
								// 	positions.CopyTo(listToDo, 4);
								// }
								// GL.UseProgram(compShader.Handle);
								// GL.BindVertexArray(CSVAO);
								// GL.BindBuffer(BufferTarget.ShaderStorageBuffer, CSSSBO);
								// if (amtthing > 1) {
								// 	throw new Exception("oops too many, haven't gotten around to this yet. probably should.");
								// } else {
								// 	// Vector3[] arr = ptsList[0];
								// 	// StringBuilder s = new("buffer before:\n");
								// 	// foreach (Vector3 vec in arr)if (vec != (0,0,0)) s.Append(vec);
								// 	// GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, (BlockStuff.BulkDrawConst+4), arr);
								// 	// GL.DispatchCompute(BlockStuff.BulkDrawConst, 1, 1);
								// 	// GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, 0, (BlockStuff.BulkDrawConst+4) * sizeof(float) * 3, arr);
								// 	// s.Append("\n\nbuffer after:\n");
								// 	// foreach (Vector3 vec in arr)if (vec != (0,0,0)) s.Append(vec);
								// 	// s.Append('\n');
								// 	// Console.Write(s.ToString());
								// 	Vector3[] arr = ptsList[0];
								// 	if ((frame&3)==0){
								// 		StringBuilder s = new("buffer before:\n");
								// 		for (int i = 0; i < 128; i++)s.Append(arr[i]);
								// 		s.Append('\n');
								// 		Console.Write(s.ToString());ASJFDKJDFKSDJ = true;}
								// 	GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, (BlockStuff.BulkDrawConst+4), arr);
								// 	GL.DispatchCompute(BlockStuff.BulkDrawConst, 1, 1);
								// }
							} else {
								DragStart = mousePos;
								Dragging = true;
								Console.WriteLine("del drag start");
							}
						} else {
							Dragging = false;
						}
						if (ks[Keys.Enter]) {
							{ List<IBlock> BlockList = BlockStuff.AllBlocks;IBlock b;
							for (int i = BlockList.Count-1; i > -1; i--) {b=BlockList[i];if((b.State&BlockState.Selected)>0&&(b.OutlineState&OutlineType.Selected)>0) {b.ID=0;BlockList.RemoveAt(i);}} }
							{ HashSet<IBindableBlock> BlockList = BiBS.AllBindableBlocks;
							foreach (IBindableBlock b in BlockList) {if((b.State&BlockState.Selected)>0&&(b.OutlineState&OutlineType.Selected)>0) BlockList.Remove(b);} }
							{ List<BiBS.Gate> BlockList = BiBS.Gate.AllGates;IBlock b;
							for (int i = BlockList.Count-1; i > -1; i--) {b=BlockList[i];if((b.State&BlockState.Selected)>0&&(b.OutlineState&OutlineType.Selected)>0) BlockList.RemoveAt(i);} }
							foreach (IBindableBlock b in BiBS.AllBindableBlocks) b.CleanInputs();
							GC.Collect();
						}
					break;
					case 2: // build
						{float mx = mousePos.X/CliSz.X*2-1, my = mousePos.Y/CliSz.Y*-2+1;
						// Vector4 frontVecTester = (mx, my, -1, 1), backVecTester = (mx, my, 1, 1);
						Matrix4 viewInv = Matrix4.Invert(view);
						// (float x1, float y1, float z1, float w1) = frontVecTester * viewInv;
						// (float x2, float y2, float z2, float w2) = backVecTester * viewInv;
						// x1 /= w1; y1 /= w1; z1 /= w1;
						// x2 /= w2; y2 /= w2; z2 /= w2;
						float shared0 = MathF.FusedMultiplyAdd(mx, viewInv.Row0.X, MathF.FusedMultiplyAdd(my, viewInv.Row1.X, viewInv.Row3.X));
						float shared1 = MathF.FusedMultiplyAdd(mx, viewInv.Row0.Y, MathF.FusedMultiplyAdd(my, viewInv.Row1.Y, viewInv.Row3.Y));
						float shared2 = MathF.FusedMultiplyAdd(mx, viewInv.Row0.Z, MathF.FusedMultiplyAdd(my, viewInv.Row1.Z, viewInv.Row3.Z));
						float shared3 = MathF.FusedMultiplyAdd(mx, viewInv.Row0.W, MathF.FusedMultiplyAdd(my, viewInv.Row1.W, viewInv.Row3.W));
						float s1 = shared3 - viewInv.Row2.W;
						// (float x1, float y1, float z1) = (
						// 	(shared0 - viewInv.Row2.X)*s1,
						// 	(shared1 - viewInv.Row2.Y)*s1,
						// 	(shared2 - viewInv.Row2.Z)*s1);
						float x1=(shared0 - viewInv.Row2.X)/s1,
						y1=(shared1 - viewInv.Row2.Y)/s1,
						z1=(shared2 - viewInv.Row2.Z)/s1;
						float n = y1/(-shared1/shared3+y1); // it's big brain time - markiplier or sm
						// GhostBlockGateIshIDK.Pos = (MathF.FusedMultiplyAdd(n,shared0/shared3-x1,x1), 0, MathF.FusedMultiplyAdd(n,shared2/shared3-z1,z1));
						ref Vector3 ghostGatePos = ref GhostBlockGateIshIDK.Pos;
						ghostGatePos.X = MathF.FusedMultiplyAdd(n,shared0/shared3-x1,x1);
						ghostGatePos.Y = 0;
						ghostGatePos.Z = MathF.FusedMultiplyAdd(n,shared2/shared3-z1,z1);
						GhostBlockGateIshIDK.Clr ^= (uint)Random.Shared.Next();
						Console.Write("ghost block pos: "+ghostGatePos+'\n');
						break;}
					case 3: // paint
						break;
					case 4: {// bind
							(float x0,float y0,float z0,float w0,float x1,float y1,float z1,float w1,float x2,float y2,float z2,float w2,float x3,float y3,float z3,float w3)=
							(view.Row0.X,view.Row0.Y,view.Row0.Z,view.Row0.W,view.Row1.X,view.Row1.Y,view.Row1.Z,view.Row1.W,view.Row2.X,view.Row2.Y,view.Row2.Z,view.Row2.W,view.Row3.X,view.Row3.Y,view.Row3.Z,view.Row3.W);
							// (float mx, float my) = mousePos/CliSz;
							// mx = MathF.FusedMultiplyAdd(mx,2,-1);my = MathF.FusedMultiplyAdd(my,-2,1); // idk for some reason the y axis is flipped
							float mx = mousePos.X/CliSz.X*2-1, my = mousePos.Y/CliSz.Y*-2+1;
							// float maxDist = 0.03125f; maxDist *= maxDist;
							const float maxDist = 0.03125f*0.03125f;
							HashSet<IBindableBlock> BlockList = BiBS.AllBindableBlocks;
							IBindableBlock selBlock = null;
							int AmtOfBlocks = BlockList.Count;
							float best = float.PositiveInfinity;
							foreach (IBindableBlock block in BlockList) {
								block.OutlineState &= ~OutlineType.Highlighted;
								if (block.OutlineState == 0) block.State &= ~BlockState.Selected;
								(float x, float y, float z) = block.Pos;
								// float w = x*w0+y*w1+z*w2+w3, wi = 1f/w;
								// (x,y,z)=((x*x0+y*x1+z*x2+x3)*wi,
								// (x*y0+y*y1+z*y2+y3)*wi,
								// (x*z0+y*z1+z*z2+z3)*wi);
								// x /= w; y /= w; z /= w; // OH, OH, OH MY GOD YEAAAH BOIIIIIIIIIIIII YEUUREUUUEUREUEEEAAEAAAAHHHSHSSSS HOLY [__] YES OMG FINALLY HOLY COW WOW SDJFKJSDF AAAAAAAAAAAA
								// // sdfjsdfjsjdflkdjks
								float w = MathF.FusedMultiplyAdd(x,w0,MathF.FusedMultiplyAdd(y,w1,MathF.FusedMultiplyAdd(z,w2,w3))), wi = 1f/w;
								(x,y,z)=(MathF.FusedMultiplyAdd(x,x0,MathF.FusedMultiplyAdd(y,x1,MathF.FusedMultiplyAdd(z,x2,x3)))*wi,
								MathF.FusedMultiplyAdd(x,y0,MathF.FusedMultiplyAdd(y,y1,MathF.FusedMultiplyAdd(z,y2,y3)))*wi,
								MathF.FusedMultiplyAdd(x,z0,MathF.FusedMultiplyAdd(y,z1,MathF.FusedMultiplyAdd(z,z2,z3)))*wi);

								// block.IsSelected = z > -w && (x > sx ^ x > ex) && (y > sy ^ y > ey);
								if (z > -1) {
									float a = x - mx; a *= a;
									float b = y - my; b *= b;
									float c = a + b;
									if (c<maxDist) {
										// float v = (.0625f*z*z)+c;
										float v = MathF.FusedMultiplyAdd(.0625f*z,z,c);
										if (v < best) {
											selBlock = block;
											best = v;
										}
									}
								}
							}
							if (selBlock != null) {
								selBlock.OutlineState |= OutlineType.Highlighted;
								selBlock.State |= BlockState.Selected;
								// Console.WriteLine("block in highlight!");
								if (ms.IsButtonPressed(MouseButton.Left)) {
									if (ShiftSelect) BindToolBindList.Add(selBlock); else BindToolSelList.Add(selBlock);
									selBlock.OutlineState |= OutlineType.Selected;
								}
								if (ms.IsButtonPressed(MouseButton.Right)) {
									selBlock.ForceToggle();
								}
								if (ms.IsButtonPressed(MouseButton.Middle)) {
									if (selBlock is BiBS.Gate gate) {
										gate.Type = gate.Type switch {
											GateType.And => GateType.Nand,
											GateType.Or => GateType.Nor,
											GateType.Xor => GateType.Xnor,
											GateType.Nand => GateType.Or,
											GateType.Nor => GateType.Xor,
											_ => GateType.And};
										gate.TypeXorOn = gate.Type^(gate.On?(GateType)4:0);
									}
									BiBS.QueueBlock(selBlock);
								}
							}
						}
						break;
					case 5: // scale
						break;
					case 6: // prop
						break;
					case 7: // trow
						break;
					case 8: // game mods
						break;
					default: throw new Exception();
				}
			}
			public static void ToolToggle(int index) {
				switch (ToolEquipped) { // unequip
					case 0: break;
					case 1: break;
					case 2: GhostBlockGateIshIDK.Pos = (float.NaN, float.NaN, float.NaN); break;
					case 3: break;
					case 4: BindToolSelList = []; BindToolBindList = [];
						foreach (IBindableBlock block in BiBS.AllBindableBlocks) {
							block.OutlineState &= ~(OutlineType.Highlighted|OutlineType.Selected);
							block.State &= ~BlockState.Selected;
						}
						break;
					case 5: break;
					case 6: break;
					case 7: break;
					case 8: break;
				}
				if (index == ToolEquipped) { // unequip
					ToolEquipped = 0;
				} else { // equip
					switch (index) {
						case 0: break;
						case 1: break;
						case 2: break;
						case 3: break;
						case 4: BindToolSelList = []; BindToolBindList = []; break;
						case 5: break;
						case 6: break;
						case 7: break;
						case 8: break;
					}
					ToolEquipped = index;
				}
			}
			public static void OnKeyDown(KeyboardKeyEventArgs e) {
				switch (e.Key) {
					case Keys.D1: ToolToggle(1); return;
					case Keys.D2: ToolToggle(2); return;
					case Keys.D3: ToolToggle(3); return;
					case Keys.D4: ToolToggle(4); return;
					case Keys.D5: ToolToggle(5); return;
					case Keys.D6: ToolToggle(6); return;
					case Keys.D7: ToolToggle(7); return;
					case Keys.D8: ToolToggle(8); return;
				}
				switch (ToolEquipped) {
					case 0: break;
					case 1: break;
					case 2:
						if (e.Alt) {
							switch (e.Key) {
								case Keys.Q:
									BuildToolBlock = typeof(BiBS.Gate);
									break;
							}
						}
						break;
					case 3: break;
					case 4:
						switch (e.Key) {
							case Keys.Enter:
								Console.Write("binding thing\nsel list count: "+BindToolSelList.Count+"\nbind list count: "+BindToolBindList.Count+'\n');
								if (e.Control) {
									// b1 is SelList, b2 is BindList; b1 -> b2
									HashSet<IBindableBlock> b1 = [], b2 = [];
									foreach (IBindableBlock a in BindToolSelList) b1.Add(a);
									foreach (IBindableBlock a in BindToolBindList) b2.Add(a);

									foreach (IBindableBlock b in b1) {
										foreach (IBindableBlock ibindable in b2) {
											if (!b.OutputsHashSet.Contains(ibindable)) {
												b.OrigOrderOutputs.Add(ibindable);
												b.OutputsHashSet.Add(ibindable);
											}
										}
									}
									foreach (IBindableBlock b in b2) b.Inputs.UnionWith(b1);
								} else {
									int SelListCount = BindToolSelList.Count, BindListCount = BindToolBindList.Count;
									for (int i = 0; i < Math.Min(SelListCount, BindListCount); i++) {
										IBindableBlock block1 = BindToolSelList[i], block2 = BindToolBindList[i];
										if (!block1.OutputsHashSet.Contains(block2)) {
											block1.OrigOrderOutputs.Add(block2);
											block1.OutputsHashSet.Add(block2);
										}
										block2.Inputs.Add(block1);
									}
								}
								foreach (IBindableBlock b in BiBS.AllBindableBlocks) {b.State&=~BlockState.Selected;b.OutlineState&=~OutlineType.Selected;}
								BindToolSelList = []; BindToolBindList = [];
								BiBS.RefreshAllBindOrderStuffs();
								break;
						}
						break;
					case 5: break;
					case 6: break;
					case 7: break;
					case 8: break;
				}
			}
			public static void OnMouseDown(MouseButtonEventArgs e, Game game) {
				switch (ToolEquipped) {
					case 0: break;
					case 1: break;
					case 2:
						if (e.Button != MouseButton.Left) break;
						Vector2 mPos = game.MouseState.Position;
						// for some reason the y axis is flipped
						PlaceBlockDict[BuildToolBlock](DataStuff.RaycastToXZPlane_Vec3(Matrix4.Invert(currentView),MathF.FusedMultiplyAdd(mPos.X/game._clientSize.X,2,-1),MathF.FusedMultiplyAdd(mPos.Y/game._clientSize.Y,-2,1)));
						break;
					case 3: break;
					case 4: break;
					case 5: break;
					case 6: break;
					case 7: break;
					case 8: break;
				}
			}
		}
		public enum GateType {
			And = 0, Or = 1, Xor = 2,
			Nand = 4, Nor = 5, Xnor = 6,
			NotBitmask = 4,
		}
		/*public class DisplayBlock : IBindableBlock {
			public static List<DisplayBlock> AllDisplayBlocks = [];
			public GateType Type;
			/// <summary>
			/// creates a DisplayBlock and does absolutely NOTHING else, doesn't even add it to the lists or add an ID.
			/// </summary>
			public DisplayBlock() {}
			public DisplayBlock(Vector3 position) {
				Pos = position; Size = (1f,1f,1f); ID = IDPtr++;
				BlockStuff.AllBlocks.Add(this);
				BindableBlockStuff.AllBindableBlocks.Add(this);
				AllDisplayBlocks.Add(this);
			}
			public DisplayBlock(Vector3 position, Vector3 rotation) {
				Pos = position; Rot = rotation; Size = (1f,1f,1f); ID = IDPtr++;
				BlockStuff.AllBlocks.Add(this);
				BindableBlockStuff.AllBindableBlocks.Add(this);
				AllDisplayBlocks.Add(this);
			}
			public DisplayBlock(bool ghostIsh) {
				Size = (1f,1f,1f);
				AllDisplayBlocks.Add(this);
				if (!ghostIsh) { ID = IDPtr++;
					BlockStuff.AllBlocks.Add(this);
					BindableBlockStuff.AllBindableBlocks.Add(this);
				} else ID = 0;
			}

			// public static float[] verts = [
			// 	-.5f,-.5f,.5f, 0,0,  .5f,-.5f,.5f, 1,0,  -.5f,.5f,.5f, 0,1,   .5f,-.5f,.5f, 1,0,  .5f,.5f,.5f, 1,1,  -.5f,.5f,.5f, 0,1, // front
			// 	-.5f,-.5f,-.5f, 0,0,  -.5f,.5f,-.5f, 0,1,  .5f,-.5f,-.5f, 1,0,   .5f,-.5f,-.5f, 1,0,  -.5f,.5f,-.5f, 0,1,  .5f,.5f,-.5f, 1,1, // back
			// 	.5f,-.5f,-.5f, 0,0,  .5f,.5f,-.5f, 0,1,  .5f,-.5f,.5f, 1,0,   .5f,-.5f,.5f, 1,0,  .5f,.5f,-.5f, 0,1,  .5f,.5f,.5f, 1,1, // right
			// 	-.5f,-.5f,-.5f, 1,0,  -.5f,-.5f,.5f, 0,0,  -.5f,.5f,-.5f, 1,1,   -.5f,-.5f,.5f, 0,0,  -.5f,.5f,.5f, 0,1,  -.5f,.5f,-.5f, 1,1, // left
			// 	-.5f,.5f,.5f, 0,1,  -.5f,.5f,-.5f, 0,0,  .5f,.5f,.5f, 1,1,   .5f,.5f,.5f, 1,1,  -.5f,.5f,-.5f, 0,0,  .5f,.5f,-.5f, 1,0, // top
			// 	-.5f,-.5f,.5f, 0,0,  .5f,-.5f,.5f, 1,0,  -.5f,-.5f,-.5f, 0,1,   .5f,-.5f,.5f, 1,0,  .5f,-.5f,-.5f, 1,1,  -.5f,-.5f,-.5f, 0,1, // bottom
			// ];
			public static float[] verts = [
				-.5f,-.5f,.5f, .5f,-.5f,.5f, -.5f,.5f,.5f,  .5f,-.5f,.5f, .5f,.5f,.5f, -.5f,.5f,.5f,// front
				-.5f,-.5f,-.5f, -.5f,.5f,-.5f, .5f,-.5f,-.5f,  .5f,-.5f,-.5f, -.5f,.5f,-.5f, .5f,.5f,-.5f,// back
				.5f,-.5f,-.5f, .5f,.5f,-.5f, .5f,-.5f,.5f,  .5f,-.5f,.5f, .5f,.5f,-.5f, .5f,.5f,.5f,// right
				-.5f,-.5f,-.5f, -.5f,-.5f,.5f, -.5f,.5f,-.5f,  -.5f,-.5f,.5f, -.5f,.5f,.5f, -.5f,.5f,-.5f,// left
				-.5f,.5f,.5f, -.5f,.5f,-.5f, .5f,.5f,.5f,  .5f,.5f,.5f, -.5f,.5f,-.5f, .5f,.5f,-.5f,// top
				-.5f,-.5f,.5f, .5f,-.5f,.5f, -.5f,-.5f,-.5f,  .5f,-.5f,.5f, .5f,-.5f,-.5f, -.5f,-.5f,-.5f,// bottom
			];
			public static void L(Game game) {
				VAO = GL.GenVertexArray();
				GL.BindVertexArray(VAO);

				VBO = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
				GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

				GL.EnableVertexAttribArray(0);
				GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

				instanceVBO = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
				GL.BufferData(BufferTarget.ArrayBuffer, (sizeof(float)*12+sizeof(uint)+1)*BlockStuff.BulkDrawConst, 0, BufferUsageHint.DynamicDraw);

				GL.EnableVertexAttribArray(2);
				GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 12 * sizeof(float), 0);
				GL.EnableVertexAttribArray(3);
				GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 12 * sizeof(float), 4 * sizeof(float));
				GL.EnableVertexAttribArray(4);
				GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, 12 * sizeof(float), 8 * sizeof(float));
				GL.VertexAttribDivisor(2, 1);
				GL.VertexAttribDivisor(3, 1);
				GL.VertexAttribDivisor(4, 1);

				GL.EnableVertexAttribArray(5);
				GL.VertexAttribPointer(5, 4, VertexAttribPointerType.UnsignedByte, false, sizeof(uint), sizeof(float)*12*BlockStuff.BulkDrawConst);
				GL.VertexAttribDivisor(5, 1);

				GL.EnableVertexAttribArray(6);
				GL.VertexAttribPointer(6, 1, VertexAttribPointerType.UnsignedByte, false, 1, (sizeof(float)*12+sizeof(uint))*BlockStuff.BulkDrawConst);
				GL.VertexAttribDivisor(6, 1);

				// it'll be fineeee without unbind here lol
			}
			public static void R() {
				GL.BindVertexArray(VAO);
				float[] blockdata = new float[BlockStuff.BulkDrawConst*12];
				uint[] colordata = new uint[BlockStuff.BulkDrawConst];
				byte[] statedata = new byte[BlockStuff.BulkDrawConst];
				int count = AllDisplayBlocks.Count;
				if ((frame&255)==0)Console.WriteLine("Instance count: "+count);
				if (count > BlockStuff.BulkDrawConst) {
					int amtThing = count>>BlockStuff.BulkDrawBS;
					for (int j = 0; j < amtThing; j++) {
						for (int i=0, i2=j<<BlockStuff.BulkDrawBS, i3=0; i3<BlockStuff.BulkDrawConst;i+=12,i2++,i3++) {
							DisplayBlock dispBlock = AllDisplayBlocks[i2];
							uint clr = dispBlock.Clr;
							colordata[i3] = (((((clr&0xff000000u)>>24)+255u)>>1)<<24)|((((clr&0x00ff0000u)+16711680u)>>17)<<16)|(((((clr&0x0000ff00u)*3)+65280u)>>10)<<8)|(clr&0xffu);
							(float rx, float ry, float rz) = dispBlock.Rot;
							(float sx, float sy, float sz) = dispBlock.Size;
							(float tx, float ty, float tz) = dispBlock.Pos;
							float num = MathF.Cos(rx), num2 = MathF.Sin(rx),
							num3 = MathF.Cos(ry), num4 = MathF.Sin(ry),
							num5 = MathF.Cos(rz), num6 = MathF.Sin(rz);
							float _x2 = num2 * num4, _x3 = num * num4;
							blockdata[i] = sx*num3*num5;
							blockdata[i+1] = sy*(_x2*num5-num*num6);
							blockdata[i+2] = sz*(_x3*num5+num2*num6);
							blockdata[i+3] = tx;
							blockdata[i+4] = sx*num3*num6;
							blockdata[i+5] = sy*(_x2*num6+num*num5);
							blockdata[i+6] = sz*(_x3*num6-num2*num5);
							blockdata[i+7] = ty;
							blockdata[i+8] = sx*-num4;
							blockdata[i+9] = sy*num2*num3;
							blockdata[i+10] = sz*num*num3;
							blockdata[i+11] = tz;
							statedata[i3] = (byte)(((int)dispBlock.Type)|(dispBlock.On?8:0));
						}
						GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
						GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float)*12*BlockStuff.BulkDrawConst, blockdata);
						GL.BufferSubData(BufferTarget.ArrayBuffer, sizeof(float)*12*BlockStuff.BulkDrawConst, sizeof(uint)*BlockStuff.BulkDrawConst, colordata);
						GL.BufferSubData(BufferTarget.ArrayBuffer, (sizeof(float)*12+sizeof(uint))*BlockStuff.BulkDrawConst, BlockStuff.BulkDrawConst, statedata);
						GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, BlockStuff.BulkDrawConst);
					}
					int amt = count&(BlockStuff.BulkDrawConst-1);
					if (amt > 0) {
						for (int i=0,i2=amtThing<<BlockStuff.BulkDrawBS,i3=0; i2 < count; i+=12,i2++,i3++) {
							// (blockdata[i], blockdata[i+1], blockdata[i+2]) = AllDisplayBlocks[i2].Pos;
							// (blockdata[i+3], blockdata[i+4], blockdata[i+5]) = AllDisplayBlocks[i2].Rot;
							// (blockdata[i+6], blockdata[i+7], blockdata[i+8]) = AllDisplayBlocks[i2].Clr;
							DisplayBlock dispBlock = AllDisplayBlocks[i2];
							uint clr = dispBlock.Clr;
							colordata[i3] = (((((clr&0xff000000u)>>24)+255u)>>1)<<24)|((((clr&0x00ff0000u)+16711680u)>>17)<<16)|(((((clr&0x0000ff00u)*3)+65280u)>>10)<<8)|(clr&0xffu);
							(float rx, float ry, float rz) = dispBlock.Rot;
							(float sx, float sy, float sz) = dispBlock.Size;
							(float tx, float ty, float tz) = dispBlock.Pos;
							float num = MathF.Cos(rx), num2 = MathF.Sin(rx),
							num3 = MathF.Cos(ry), num4 = MathF.Sin(ry),
							num5 = MathF.Cos(rz), num6 = MathF.Sin(rz);
							float _x2 = num2 * num4, _x3 = num * num4;
							blockdata[i] = sx*num3*num5;
							blockdata[i+1] = sy*(_x2*num5-num*num6);
							blockdata[i+2] = sz*(_x3*num5+num2*num6);
							blockdata[i+3] = tx;
							blockdata[i+4] = sx*num3*num6;
							blockdata[i+5] = sy*(_x2*num6+num*num5);
							blockdata[i+6] = sz*(_x3*num6-num2*num5);
							blockdata[i+7] = ty;
							blockdata[i+8] = sx*-num4;
							blockdata[i+9] = sy*num2*num3;
							blockdata[i+10] = sz*num*num3;
							blockdata[i+11] = tz;
							statedata[i3] = (byte)(((int)dispBlock.Type)|(dispBlock.On?8:0));
						}
						GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
						GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float)*12*amt, blockdata);
						GL.BufferSubData(BufferTarget.ArrayBuffer, sizeof(float)*12*BlockStuff.BulkDrawConst, sizeof(uint)*amt, colordata);
						GL.BufferSubData(BufferTarget.ArrayBuffer, (sizeof(float)*12+sizeof(uint))*BlockStuff.BulkDrawConst, amt, statedata);
						GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, amt);}
				} else {
					for (int i = 0, i2=0; i2 < count; i+=12,i2++) {
						DisplayBlock dispBlock = AllDisplayBlocks[i2];
						uint clr = dispBlock.Clr;
						colordata[i2] = (((((clr&0xff000000u)>>24)+255u)>>1)<<24)|((((clr&0x00ff0000u)+16711680u)>>17)<<16)|(((((clr&0x0000ff00u)*3)+65280u)>>10)<<8)|(clr&0xffu);
						(float rx, float ry, float rz) = dispBlock.Rot;
						(float sx, float sy, float sz) = dispBlock.Size;
						(float tx, float ty, float tz) = dispBlock.Pos;
						float num = MathF.Cos(rx), num2 = MathF.Sin(rx),
						num3 = MathF.Cos(ry), num4 = MathF.Sin(ry),
						num5 = MathF.Cos(rz), num6 = MathF.Sin(rz);
						float _x2 = num2 * num4, _x3 = num * num4;
						blockdata[i] = sx*num3*num5;
						blockdata[i+1] = sy*(_x2*num5-num*num6);
						blockdata[i+2] = sz*(_x3*num5+num2*num6);
						blockdata[i+3] = tx;
						blockdata[i+4] = sx*num3*num6;
						blockdata[i+5] = sy*(_x2*num6+num*num5);
						blockdata[i+6] = sz*(_x3*num6-num2*num5);
						blockdata[i+7] = ty;
						blockdata[i+8] = sx*-num4;
						blockdata[i+9] = sy*num2*num3;
						blockdata[i+10] = sz*num*num3;
						blockdata[i+11] = tz;
						statedata[i2] = (byte)(((int)dispBlock.Type)|(dispBlock.On?8:0));
					}
					GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
					GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float)*12*count, blockdata);
					GL.BufferSubData(BufferTarget.ArrayBuffer, sizeof(float)*12*BlockStuff.BulkDrawConst, sizeof(uint)*count, colordata);
					GL.BufferSubData(BufferTarget.ArrayBuffer, (sizeof(float)*12+sizeof(uint))*BlockStuff.BulkDrawConst, count, statedata);
					GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, count);
				}
			}
			public override bool OnUpdate() {
				bool Changes;
				switch (Type^((GateType)(On?4:0))) {
					case GateType.And: // this is either off and and or on and nand
						foreach (IBindableBlock block in Inputs) {if (!block.On) {Changes = false; break;}}
						Changes = true; break;
					case GateType.Or:
						foreach (IBindableBlock block in Inputs) {if (block.On) {Changes = true; break;}}
						Changes = false; break;
					case GateType.Xor: // off xor or on xnor
						Changes = false;
						foreach (IBindableBlock block in Inputs) Changes ^= block.On;
						break;
					case GateType.Nand:
						foreach (IBindableBlock block in Inputs) {if (!block.On) {Changes = true; break;}}
						Changes = false; break;
					case GateType.Nor:
						foreach (IBindableBlock block in Inputs) {if (block.On) {Changes = false; break;}}
						Changes = true; break;
					case GateType.Xnor:
						Changes = true;
						foreach (IBindableBlock block in Inputs) Changes ^= block.On;
						break;
					default: throw new Exception();
				}
				if (Changes) {
					// State ^= true;
					On ^= true;
					ForceEvent();
					return true;
				}
				return false;
			}
			public override bool GetUpdateResult() {
				switch (Type^((GateType)(On?4:0))) {
					case GateType.And: // this is either off and and or on and nand
						foreach (IBindableBlock block in Inputs) if (!block.On) return false;
						return true;
					case GateType.Or:
						foreach (IBindableBlock block in Inputs) if (block.On) return true;
						return false;
					case GateType.Xor: // off xor or on xnor
						bool StateChanges = false;
						foreach (IBindableBlock block in Inputs) StateChanges ^= block.On;
						return StateChanges;
					case GateType.Nand:
						foreach (IBindableBlock block in Inputs) if (!block.On) return true;
						return false;
					case GateType.Nor:
						foreach (IBindableBlock block in Inputs) if (block.On) return false;
						return true;
					case GateType.Xnor:
						bool _StateChanges = true;
						foreach (IBindableBlock block in Inputs) _StateChanges ^= block.On;
						return _StateChanges;
					default: throw new Exception();
				}
			}
			public override void CleanInputs() {
				foreach (IBindableBlock block in Inputs) {if(block==null||block.ID==0)Inputs.Remove(block);}
			}
			public override void ForceEvent() {
				//foreach (IBindableBlock block in OrigOrderOutputs) BindableBlockStuff.QueueBlock(block);
				BindableBlockStuff.QueueSet(this);
			}
			public override void ForceSet(bool state) {
				if (On ^ state) {
					On^=true;
					ForceEvent();
				}
			}
			public override void ForceToggle() {
				On ^= true;
				ForceEvent();
			}
			public override void QuietForceSet(bool state) {
				if (On^state)On^=true;
			}
			public override void QuietForceToggle() {
				On ^= true;
			}
		}*/
		public Shader shader;
		public int cubeVAO, cubeVBO, instanceVBO;
		public static float[] cubeVerts = [
			0,0,1, 0,0,  1,0,1, 1,0,  0,1,1, 0,1,   1,0,1, 1,0,  1,1,1, 1,1,  0,1,1, 0,1, // front
			0,0,0, 0,0,  0,1,0, 0,1,  1,0,0, 1,0,   1,0,0, 1,0,  0,1,0, 0,1,  1,1,0, 1,1, // back
			1,0,0, 0,0,  1,1,0, 0,1,  1,0,1, 1,0,   1,0,1, 1,0,  1,1,0, 0,1,  1,1,1, 1,1, // right
			0,0,0, 1,0,  0,0,1, 0,0,  0,1,0, 1,1,   0,0,1, 0,0,  0,1,1, 0,1,  0,1,0, 1,1, // left
			0,1,1, 0,1,  0,1,0, 0,0,  1,1,1, 1,1,   1,1,1, 1,1,  0,1,0, 0,0,  1,1,0, 1,0, // top
			0,0,1, 0,0,  1,0,1, 1,0,  0,0,0, 0,1,   1,0,1, 1,0,  1,0,0, 1,1,  0,0,0, 0,1, // bottom
		];
		public override void OnLoad(Game game) {
			base.OnLoad(game);
			this.game = game;
			frame = 0;
			// me when i steal from myself
			// game._camera.IsFlying = true;
			game._camera.MaxDist = 1024;
			shader = new("Shaders/babfte/Shader.vert", "Shaders/babfte/Shader.frag");
			GL.UseProgram(shader.Handle);
			shader.SetInt("texture0", 0);
			// GenDefaultWorld();
			foreach (Type type in BlockStuff.AllBlockTypes) type.GetMethod("L")?.Invoke(null, [game]);
			Tools.OnLoad(game);
			// cubeVAO = GL.GenVertexArray();
			// GL.BindVertexArray(cubeVAO);

			// cubeVBO = GL.GenBuffer();
			// GL.BindBuffer(BufferTarget.ArrayBuffer, cubeVBO);
			// GL.BufferData(BufferTarget.ArrayBuffer, cubeVerts.Length * sizeof(float), cubeVerts, BufferUsageHint.StaticDraw);

			// GL.EnableVertexAttribArray(0);
			// GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
			// GL.EnableVertexAttribArray(1);
			// GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

			// instanceVBO = GL.GenBuffer();
			// GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
			// GL.BufferData(BufferTarget.ArrayBuffer, sizeof(int) * Text.BulkDrawConst * 3, /*pos*/ 0, BufferUsageHint.DynamicDraw);

			// GL.EnableVertexAttribArray(2);
			// GL.VertexAttribIPointer(2, 3, VertexAttribIntegerType.Int, 3 * sizeof(int), 0);
			// GL.VertexAttribDivisor(2, 1);
		}
		Game game;
		public static Matrix4 currentView;
		public override void OnRenderFrame(Game game, double dt) {
			base.OnRenderFrame(game, dt);
			game._textureSheet.Use(TextureUnit.Texture0);
			GL.UseProgram(shader.Handle);
			currentView = game._camera.View;
			GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "view"), true, ref currentView);
			foreach (var BlockRenderAction in BlockStuff.BlockRenderBehaviors) BlockRenderAction();
			Tools.OnRenderFrame(game);
		}
		public override void OnUpdateFrame(Game game, double dt) {
			base.OnUpdateFrame(game, dt);
			BiBS.OnUpdateFrame();
			Tools.OnUpdateFrame(game);
			frame++;
		}
		public override void OnKeyDown(KeyboardKeyEventArgs e) {
			base.OnKeyDown(e);
			Tools.OnKeyDown(e);
		}
		public override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			Tools.OnMouseDown(e, game);
		}
	}
	public class CPSTest : IMinigame {
		public const string GameIdentifier = "cpstester";
		public static Dictionary<string, Action<Game>> InGameConstructorthings = new() {
			["cpstest"] = delegate (Game game) {
				game._currentMinigames.Add(new CPSTest());
				game._gameModes.Add(GameIdentifier);
			}
		};
		public static void StartInit() {
			InGameConstructorthings["cpstester"]=
			InGameConstructorthings["cpstest"];
			DataStuff.chatCommands["cpstest "] = delegate (Game game, string path) {
				Console.Write("cps test thing.\n");
				if (path.Length > 6 && path[..5] == "time ") {
					try {
						long duration = (long)(Convert.ToDouble(path[5..])*Stopwatch.Frequency);
						Duration = duration;
					} catch (Exception e) {
						Console.Write("smth didn't work bruh\n"+e.Message+'\n');
					}
				}
			};
		}
		public CPSTest() {StartTime = Stopwatch.GetTimestamp();}
		public long StartTime;
		public static long Duration = Stopwatch.Frequency * 5;
		public long Clicks = 0;
        public override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
			if (e.Key == Keys.Enter) {
				StartTime = Stopwatch.GetTimestamp();
				Clicks = 0;
			}
        }
        public override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
			if (Stopwatch.GetTimestamp() < StartTime+Duration) {
				Clicks++;
			} else {
				double dDur = Duration/(double)Stopwatch.Frequency;
				Console.WriteLine("done; time: "+dDur+"; clicks: "+Clicks+"; cps: "+(Clicks / dDur)+";\n");
				StartTime = Stopwatch.GetTimestamp();
				Clicks = 0;
			}
        }

	}
	public class Minesweeper : IMinigame {
		public const string GameIdentifier = "minesweeper";
		public static Dictionary<string, Action<Game>> InGameConstructorThings = new() {
			["minesweeper"] = delegate (Game game) {
				game._currentMinigames.Add(new Minesweeper());
				game._gameModes.Add(GameIdentifier);
			}
		};
        public override void OnLoad(Game game)
        {
            base.OnLoad(game);
			game.UpdateFrequency = 60;
			throw new Exception("no i haven't made this yet bruh");
        }
        public override void OnRenderFrame(Game game, double dt)
        {
            base.OnRenderFrame(game, dt);
        }
	}
	public class RandomProgramStuff : IMinigame {
		public const string GameIdentifier = "randomprogramstuff";
		public static Dictionary<string, Action<Game>> InGameConstructorthings = new() {
			["randomprogramstuff"] = delegate (Game game) {
				game._currentMinigames.Add(new RandomProgramStuff());
				game._gameModes.Add(GameIdentifier);
			}
		};
		public Dictionary<string, Action> Things = new() {
			["infrpgthing"] = delegate() {
				Console.Write("Infinity RPG Weapon Loadout Calculator\n");
				List<string> weaponnames = [];
				// List<double> weaponlowstats = [];
				// List<double> weaponhighstats = [];
				List<double> weaponavgstats = [];
				while (true) {
					Console.Write("Another weapon, nothing to stop or input name... ");
					string thing = Console.ReadLine() ?? "";
					if (thing == "") break;
					Console.Write('\n'+thing+"\nLow dmg: ");
					double lowdmg = Convert.ToDouble(Console.ReadLine());
					double highdmg = Convert.ToDouble(Console.ReadLine());
					weaponnames.Add(thing);
					weaponavgstats.Add((lowdmg+highdmg)*.5);
				}
				int amt = weaponavgstats.Count;
				if (amt == 0) {Console.Write("nothing listed...\n"); return;}
				int thingyy = Math.Min(10, amt);
				string[] weaponlist = new string[thingyy];
				double[] avgdmglist = new double[thingyy];
				for (int bruh = 0; bruh < thingyy; bruh++) {
					double highest = weaponavgstats[0];
					int ind = 0;
					for (int i = 1; i < weaponavgstats.Count; i++) {
						double tmp = weaponavgstats[i];
						if (tmp > highest) {
							ind = i;
							highest = tmp;
						}
					}
					weaponlist[bruh] = weaponnames[ind];
					avgdmglist[bruh] = weaponavgstats[ind];
					weaponnames.RemoveAt(ind);
					weaponavgstats.RemoveAt(ind);
				}
				Console.Write("\nWeapon order:\n");
				for (int i = 0; i < thingyy; i++) Console.Write(weaponlist[i]+", avg dmg: "+avgdmglist[i]);
				Console.Write("Done! :3\n");
			},
			["divmultbench"] = delegate() {
				Console.Write("Division and multiplication benchmark thing\n");
				for (int _i = 0; _i < 10; _i++){
					long t0 = Stopwatch.GetTimestamp();
					float[] numbers = new float[10000000];
					for (int i = 0; i < 10000000; i++) numbers[i] = Random.Shared.NextSingle()*16-8;
					float[] numbers0 = new float[10000000];
					for (int i = 0; i < 10000000; i++) numbers0[i] = Random.Shared.NextSingle()*16-8;
					float[] numbers1 = (float[])numbers.Clone(), numbers2 = (float[])numbers.Clone();
					long t1 = Stopwatch.GetTimestamp();
					Console.Write("\nTime to prepare: "+((t1-t0)/(double)Stopwatch.Frequency));
					long t2 = Stopwatch.GetTimestamp();
					for (int i = 0; i < 10000000; i++) {
						numbers1[i] /= numbers0[i];
					}
					long t3 = Stopwatch.GetTimestamp();
					Console.Write("\nDivision: "+((t3-t2)/(double)Stopwatch.Frequency));
					long t4 = Stopwatch.GetTimestamp();
					for (int i = 0; i < 10000000; i++) {
						numbers2[i] *= numbers0[i];
					}
					long t5 = Stopwatch.GetTimestamp();
					Console.Write("\nMult: "+((t5-t4)/(double)Stopwatch.Frequency)+'\n');
					double total = 1;
					foreach (float n in numbers0) {total = total*n+Random.Shared.NextSingle();}
					Console.Write(total+'\n');
				}
				Console.Write("done\nenter to continue\n");
			}
		};
		public override void OnLoad(Game game) {
			base.OnLoad(game);
			Console.WriteLine("random program stuff thing. yeah.");
			while (true) {
				foreach (KeyValuePair<string, Action> act in Things) Console.Write("Thing: \""+act.Key+"\"\n");
				Console.WriteLine("Input the thing you want, or smth that isn't to stop.");
				string thing = Console.ReadLine() ?? "\n";
				if (Things.TryGetValue(thing, out Action action)) {Console.Write("thing found...\n");try{action();}catch(Exception a){Console.Write("oops, smth happened and it broke... . w .\n"+a.Message+'\n'+a.StackTrace+'\n');}Console.Write("done...\n\n");}
				else {game.WillReopen=true;game.Close();return;}
			}
		}
	}
	public class DefaultGameBehavior : IMinigame
	{
		public const string GameIdentifier = "DEFAULT_BEHAVIOR";
		public static Dictionary<string, Action<Game>> InGameConstructorthings = new() {
			["DEFAULT_BEHAVIOR"] = delegate (Game game) {
				game._currentMinigames.Add(new DefaultGameBehavior());
				game._gameModes.Add(GameIdentifier);
			}
		};
		// public override void OnLoad(Game game)
		// {
		//     base.OnLoad(game);
		// }
		// public override void OnRenderFrame(Game game, double dt)
		// {
		//     base.OnRenderFrame(game, dt);
		// }
	}
}