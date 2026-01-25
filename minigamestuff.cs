#undef MINING_GAME_DEBUG
#undef MINING_GAME_PROFILELOG
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using System.ComponentModel;

namespace GameEngineThing {
	public class IMinigame {
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
		}
	public class Pong : IMinigame {
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
					if (e.Key == P1DownBind) { P1DownDown = 1; }
					if (e.Key == P1UpBind) { P1UpDown = 1; }
					if (e.Key == P2DownBind) { P2DownDown = 1; }
					if (e.Key == P2UpBind) { P2UpDown = 1; }}
				else {
					if (e.Key == P1DownBind) { P1DownDown = 0; }
					if (e.Key == P1UpBind) { P1UpDown = 0; }
					if (e.Key == P2DownBind) { P2DownDown = 0; }
					if (e.Key == P2UpBind) { P2UpDown = 0; }}}
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
		public void Render(Shader shader, ObjectMesh CubeMesh, ObjectMesh Background, bool bind) {
			CubeMesh.DrawWithModels(shader, [
				Matrix4.CreateScale(new Vector3(HittingWidth*2,PaddleHeight,1f)) * Matrix4.CreateTranslation(Paddle1X,Paddle1Y,0) * Transformation,
				Matrix4.CreateScale(new Vector3(HittingWidth*2,PaddleHeight,1f)) * Matrix4.CreateTranslation(Paddle2X,Paddle2Y,0) * Transformation,
				Matrix4.CreateScale(new Vector3(BallRadius*2)) * Matrix4.CreateTranslation(BallX,BallY,0) * Transformation,
			], bind);
			Background.DrawWithModel(shader, Matrix4.CreateScale(new Vector3(5.5f, 5.5f, 1f)) * Matrix4.CreateTranslation(0f, 0f, -1f) * Transformation, true);}
		public string ScoreText => "Score: " + P1Score + " - " + P2Score;
		public void UpdateTransformation(Matrix4 Transformation) {
			this.Transformation = Transformation;}
		public void UpdateTransformation() {
			Transformation = Matrix4.CreateRotationX(Rot.X) * Matrix4.CreateRotationY(Rot.Y) * Matrix4.CreateRotationZ(Rot.Z) * Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Pos);}
		public void UpdatePos(Vector3 pos) {
			Pos = pos;
			Transformation = Matrix4.CreateRotationX(Rot.X) * Matrix4.CreateRotationY(Rot.Y) * Matrix4.CreateRotationZ(Rot.Z) * Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Pos);}
		public void UpdateRot(Vector3 rot) {
			if (AngleMode) Rot = rot * ((float)Math.PI / 180f);
			else Rot = rot;
			Transformation = Matrix4.CreateRotationX(Rot.X) * Matrix4.CreateRotationY(Rot.Y) * Matrix4.CreateRotationZ(Rot.Z) * Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Pos);}
		public void UpdateScale(Vector3 scale) {
			Scale = scale;
			Transformation = Matrix4.CreateRotationX(Rot.X) * Matrix4.CreateRotationY(Rot.Y) * Matrix4.CreateRotationZ(Rot.Z) * Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Pos);}
		public void Loss(byte Player1Side) {}
		public override void OnRenderFrame(Game game, double dt) {
			base.OnRenderFrame(game, dt);
			Render(game._shader, game._cube, game._plane, true);
			// _textRenderer.RenderText(this, _textShader, _pongGame.ScoreText, Vector2i.Zero, new(0f, .45f), new(6f), new(0f, 1f, 0f), 10f, _clientSize, FontCharFillerThing.FontCharDeeta, false);
			game._textRenderer.Render(new TxtOptions(Vector2i.Zero, new(0f, .45f), new(4f), new(0f, 1f, 0f), 10f, FontCharFillerThing.FontCharDeeta, false), game, ScoreText); }
		public override void OnUpdateFrame(Game game, double dt)
		{
			base.OnUpdateFrame(game, dt);
			UpdateRot(new Vector3(270f, (float)game._gameTime * 15f, 0f));
			Update(1 / game._gameTickSpeed);
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
	// 	public uint KeyCount { get; private set; }
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
		public Keys[] keybinds;
		public uint KeyCount { get; private set; }
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
			float halfOfCeilTrueSizeX = MathF.Ceiling(noteGlyphData.size.X * noteScale.X) * 0.5f;
			float halfOfCeilTrueSizeY = MathF.Ceiling(noteGlyphData.size.Y * noteScale.Y) * 0.5f;
			// int WinSX = windowSize.X; int WinSY = windowSize.Y;
			(int WinSX, int WinSY) = (windowSize.X, windowSize.Y);
			float oX = posOffset?.X??0 + posScale.X * WinSX; // offset x
			float oY = posOffset?.Y??0 + posScale.Y * WinSY; // offset y
			List<float[]> vertices = []; int I = 0; // I is vertex index
			float[] v = new float[Text.BulkDrawFloats];
			Vector2i texStart = noteGlyphData.textureStart;
			float tSX = texStart.X / ftexW; // texture start x
			float tSY = texStart.Y / ftexH; // texture start y
			float tNX = (texStart.X + noteGlyphData.textureSize.X) / ftexW; // texture end x
			float tNY = (texStart.Y + noteGlyphData.textureSize.Y) / ftexH; // texture end y
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
				float halfOfCeilTrueJudgementSizeX = MathF.Ceiling(noteGlyphData.size.X * noteScale.X) * 0.55f; // scaled wider slightly
				float halfOfCeilTrueJudgementSizeY = MathF.Ceiling(noteGlyphData.size.Y * noteScale.Y) * 0.5f;
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
			float halfOfCeilTrueSizeX = MathF.Ceiling(noteGlyphData.size.X * noteScaleX) * 0.5f;
			float halfOfCeilTrueSizeY = MathF.Ceiling(noteGlyphData.size.Y * noteScaleY) * 0.5f;
			(int WinSX, int WinSY) = (windowSize.X, windowSize.Y);
			float oX = posScaleX * WinSX; // offset x
			float oY = posScaleY * WinSY; // offset y
			int I = 0; // I is vertex index
			float[] v = new float[Text.BulkDrawFloats];
			Vector2i texStart = noteGlyphData.textureStart;
			float tSX = texStart.X / ftexW; // texture start x
			float tSY = texStart.Y / ftexH; // texture start y
			float tNX = (texStart.X + noteGlyphData.textureSize.X) / ftexW; // texture end x
			float tNY = (texStart.Y + noteGlyphData.textureSize.Y) / ftexH; // texture end y
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
				float halfOfCeilTrueJudgementSizeX = MathF.Ceiling(noteGlyphData.size.X * noteScaleX) * 0.55f; // scaled wider slightly
				float halfOfCeilTrueJudgementSizeY = MathF.Ceiling(noteGlyphData.size.Y * noteScaleY) * 0.5f;
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
			// GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			// GL.BindVertexArray(0);

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
		public Keys keybind = Keys.E;
		public Text gameRenderer;
		public Stopwatch stopwatch = new();
		public VerticalOneKey(Text renderer) { gameRenderer = renderer; LoadMap(chart); RestartMap(); }
		public VerticalOneKey(Text renderer, V1KChart chart) { gameRenderer = renderer; }
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
			float halfCeilTrueSzX = MathF.Ceiling(noteGlyphData.size.X * noteScale.X) * 0.5f; // the x value for half of the ceiling of the true size
			float halfCeilTrueSzY = MathF.Ceiling(noteGlyphData.size.Y * noteScale.Y) * 0.5f; // the y value for half of the ceiling of the true size
			int WinSX = windowSize.X; int WinSY = windowSize.Y;
			float oX = posOffset.X + posScale.X * WinSX; // offset x
			float oY = posOffset.Y + posScale.Y * WinSY; // offset y
			List<float[]> vertices = [];    int I = 0; // I is vertex index
			float[] v = new float[Text.BulkDrawFloats];
			Vector2i texStart = noteGlyphData.textureStart;
			float tSX = texStart.X / ftexW; // texture start x
			float tSY = texStart.Y / ftexH; // texture start y
			float tNX = (texStart.X + noteGlyphData.textureSize.X) / ftexW; // texture end x
			float tNY = (texStart.Y + noteGlyphData.textureSize.Y) / ftexH; // texture end y
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
			float halfCeilTrueJudgementSzX = MathF.Ceiling(noteGlyphData.size.X * noteScale.X) * 0.625f;
			float halfCeilTrueJudgementSzY = MathF.Ceiling(noteGlyphData.size.Y * noteScale.Y) * 0.5f;
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
	public class MiningGame : IMinigame {
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
		int cubeVAO;
		int cubeVBO;
		int instanceVBO;
		float[] cubeVerts = [
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
			game._camera.IsFlying = true;
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
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.VertexAttribDivisor(2, 1);

			// unbind to prevent later code from accidentally modifying this VAO/EBO
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
			// GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
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
				uint tmp, tmp1;
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
							tmp1=tmp&~((tmp<<1)|(ocfd[_i]==0?0:1u));
							tmp&=~((tmp>>1)|(ocfd[_i|cSzSq]==0?0:0x80000000));
							//ind=(y<<cBSN)+(z<<cBS2);
							ind=(j2<<cBSN)+j1xcssq;
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
							tmp=(B[_i]==0?0:1u)|(B[_i|cSzSq]==0?0:2u)|(B[_i|(cSzSq*2)]==0?0:4u)|(B[_i|(cSzSq*3)]==0?0:8u)|(B[_i|(cSzSq*4)]==0?0:16u)|(B[_i|(cSzSq*5)]==0?0:32u)|(B[_i|(cSzSq*6)]==0?0:64u)|
							(B[_i|(cSzSq*7)]==0?0:128u)|(B[_i|(cSzSq*8)]==0?0:256u)|(B[_i|(cSzSq*9)]==0?0:512u)|(B[_i|(cSzSq*10)]==0?0:1024u)|(B[_i|(cSzSq*11)]==0?0:2048u)|(B[_i|(cSzSq*12)]==0?0:4096u)|
							(B[_i|(cSzSq*13)]==0?0:8192u)|(B[_i|(cSzSq*14)]==0?0:16384u)|(B[_i|(cSzSq*15)]==0?0:32768u)|(B[_i|(cSzSq*16)]==0?0:65536u)|(B[_i|(cSzSq*17)]==0?0:131072u)|(B[_i|(cSzSq*18)]==0?0:262144u)|
							(B[_i|(cSzSq*19)]==0?0:524288u)|(B[_i|(cSzSq*20)]==0?0:1048576u)|(B[_i|(cSzSq*21)]==0?0:2097152u)|(B[_i|(cSzSq*22)]==0?0:4194304u)|(B[_i|(cSzSq*23)]==0?0:8388608u)|
							(B[_i|(cSzSq*24)]==0?0:16777216u)|(B[_i|(cSzSq*25)]==0?0:33554432u)|(B[_i|(cSzSq*26)]==0?0:67108864u)|(B[_i|(cSzSq*27)]==0?0:0x8000000u)|(B[_i|(cSzSq*28)]==0?0:0x10000000u)|
							(B[_i|(cSzSq*29)]==0?0:0x20000000u)|(B[_i|(cSzSq*30)]==0?0:0x40000000u)|(B[_i|(cSzSq*31)]==0?0:0x80000000);
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
							ulong bsamtish = 0b00100000ul<<((_i&7)<<3); // bitshift amount ish
							int irs3 = _i >> 3; // i right shifted by 3. INTERNAL REVENUE SERVICE MENTION!?!?1?!?!1?!1?!!?1?!1?3991680010395?!1!?1?10395?!?!1?1039511?
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
							j=j1xcssq|j2;
							tmp=(B[j]==0?0:1u)|(B[j|cSz]==0?0:2u)|(B[j|cSz*2]==0?0:4u)|(B[j|cSz*3]==0?0:8u)|(B[j|cSz*4]==0?0:16u)|(B[j|cSz*5]==0?0:32u)|(B[j|cSz*6]==0?0:64u)|
							(B[j|cSz*7]==0?0:128u)|(B[j|cSz*8]==0?0:256u)|(B[j|cSz*9]==0?0:512u)|(B[j|cSz*10]==0?0:1024u)|(B[j|cSz*11]==0?0:2048u)|(B[j|cSz*12]==0?0:4096u)|
							(B[j|cSz*13]==0?0:8192u)|(B[j|cSz*14]==0?0:16384u)|(B[j|cSz*15]==0?0:32768u)|(B[j|cSz*16]==0?0:65536u)|(B[j|cSz*17]==0?0:131072u)|(B[j|cSz*18]==0?0:262144u)|
							(B[j|cSz*19]==0?0:524288u)|(B[j|cSz*20]==0?0:1048576u)|(B[j|cSz*21]==0?0:2097152u)|(B[j|cSz*22]==0?0:4194304u)|(B[j|cSz*23]==0?0:8388608u)|
							(B[j|cSz*24]==0?0:16777216u)|(B[j|cSz*25]==0?0:33554432u)|(B[j|cSz*26]==0?0:67108864u)|(B[j|cSz*27]==0?0:0x8000000u)|(B[j|cSz*28]==0?0:0x10000000u)|
							(B[j|cSz*29]==0?0:0x20000000u)|(B[j|cSz*30]==0?0:0x40000000u)|(B[j|cSz*31]==0?0:0x80000000);
							tmp1=tmp&~((tmp<<1)|(ocfd[cSzSq*2|_i]==0?0:1u));
							tmp&=~((tmp>>1)|(ocfd[cSzSq*3|_i]==0?0:0x80000000));
							ind=j2|j1xcssq;
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);
							bsamtish >>= 1;
							irs3 = ind >> 3; // i right shifted by 3. INTERNAL REVENUE SERVICE MENTION!?!?1?!?!1?!1?!!?1?!1?3991680010395?!1!?1?10395?!?!1?1039511?
							while(tmp2<32){
								F[irs3|(tmp2<<(cBSN-3))]|=bsamtish;
								tmp^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp);}
							tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp1);
							bsamtish >>= 1;
							while(tmp2<32){
								F[irs3|(tmp2<<(cBSN-3))]|=bsamtish;
								tmp1^=1u<<tmp2;
								tmp2=System.Numerics.BitOperations.TrailingZeroCount(tmp1);}}}
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
	public struct MMGChunk
	{
		public short[] data;
	}
	public class DefaultGameBehavior : IMinigame
	{
        public override void OnRenderFrame(Game game, double dt)
        {
            base.OnRenderFrame(game, dt);
			game._tetrahedron.Draw(game._shader, true);

			game._cube.Draw(game._shader, true);

			Matrix4 Scale = Matrix4.CreateScale(new Vector3((float)Math.Sin(game._gameTime) + 1f, (float)Math.Sin(game._gameTime) + 1f, (float)Math.Sin(game._gameTime) + 1f));
			Matrix4[] models = [
				Scale * Matrix4.CreateTranslation(new Vector3(-10f, 0f, 0f)),
				Scale * Matrix4.CreateTranslation(new Vector3(10f, 0f, 0f)),
				Scale * Matrix4.CreateTranslation(new Vector3(0f, 10f, 0f)),
				Scale * Matrix4.CreateTranslation(new Vector3(0f, -10f, 0f)),
				Scale * Matrix4.CreateTranslation(new Vector3(0f, 0f, 10f)),
				Scale * Matrix4.CreateTranslation(new Vector3(0f, 0f, -10f)),
			];

			// // Draw multiple cubes with different texture layers
			// for (int i = 0; i < models.Length; i++) game._cube.DrawWithModel(game._shader, models[i], false);
			game._cube.DrawWithModels(game._shader, models, false);
        }
	}
}