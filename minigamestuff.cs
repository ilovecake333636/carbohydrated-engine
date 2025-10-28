using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;

namespace GameEngineThing {
	public class Pong {
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
		public void Loss(byte Player1Side) {}}
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
	public class ManiaRG
	{
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
		public uint[] noteHitAccAmts = new uint[Enum.GetValues<ManiaAccs>().Length]; // an array containing how many times the player has hit each of the accuracies including misses.
		private ManiaChart chart;
		private float jBasePosX;
		private float jBasePosY;
		private Vector2[] jPosSOffs;
		private uint training = 0;
		public ManiaRG(Text renderer, uint keyAmount = 4, Keys[] binds = null, float JBasePosX = float.NaN, float JBasePosY = float.NaN, Vector2[] JPosSOffs = null)
		{
			keybinds = binds ?? [Keys.E, Keys.R, Keys.U, Keys.I];
			KeyCount = keyAmount;
			gameRenderer = renderer;
			currentKeys = new uint[KeyCount];
			jBasePosX = float.IsNaN(JBasePosX) ? 0 : JBasePosX;
			jBasePosY = float.IsNaN(JBasePosY) ? -0.8f : JBasePosY;
			if (JPosSOffs == null || JPosSOffs.Length != KeyCount) {
				jPosSOffs = new Vector2[KeyCount];
				for (uint i = KeyCount; i-- > 0;) {
					jPosSOffs[i] = new((float)i / KeyCount * 0.5f - 0.25f, 0);
				}
			}
			else jPosSOffs = JPosSOffs;
			// Console.WriteLine("mania rhythm game thing " + keybinds + "," + keyCount + "," + renderer + "," + currentKeys.Length + "," + jBasePosX + "," + jBasePosY+","+jPosSOffs+","+jPosSOffs == null ? -1 : jPosSOffs.Length+","+JPosSOffs+","+JPosSOffs == null ? -1 : JPosSOffs.Length);
			Console.WriteLine("mania rhythm game thing");
			Console.WriteLine(keybinds+","+KeyCount);
			Console.WriteLine(renderer+","+currentKeys.Length);
			Console.WriteLine(jBasePosX+","+jBasePosY);
			Console.WriteLine(jPosSOffs);
			Console.WriteLine(jPosSOffs==null?-1:jPosSOffs.Length);
			if (jPosSOffs != null)
			{
				Console.WriteLine("jPosSOffs is not null.");
				for (int i = 0; i < jPosSOffs.Length; i++)
				{
					(float jx, float jy) = (jPosSOffs[i].X, jPosSOffs[i].Y);
					Console.WriteLine("lane " + i + " thing: " + jx + "," + jy);
				}
			}
			Console.WriteLine(JPosSOffs);
			Console.WriteLine(JPosSOffs==null?-1:JPosSOffs.Length);
		}
		public void LoadMap(ManiaChart chart)
		{
			currentKeys = new uint[KeyCount];
			// lastDisplayedKey = 0;
			this.chart = chart;
		}
		public void StartMap() {
			if (!Playing)
			{
				Playing = true;
				// time that has passed is the current time minus the time at pause
				timeOffset += Stopwatch.GetTimestamp() - timeAtPause;
			} }
		public void RestartMap()
		{
			if (training == 0)
			{
				timeOffset = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 3;
				currentKeys = new uint[KeyCount];
				// lastDisplayedKey = 0;
			}
			else
			{
				timeOffset = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 3;
				currentKeys = new uint[KeyCount];
				// lastDisplayedKey = 0;
				RestartTraining();
			}
		}
		public void TogglePauseMap()
		{
			if (Playing)
			{
				Playing = false; timeAtPause = Stopwatch.GetTimestamp();
			}
			else
			{
				Playing = true;
				// time that has passed is the current time minus the time at pause
				timeOffset += Stopwatch.GetTimestamp() - timeAtPause;
			}
		}
		public void PauseMap()
		{
			if (Playing)
			{
				Playing = false;
				timeAtPause = Stopwatch.GetTimestamp();
			}
		}
		public float[][] CalcVertices(Vector2 posScale, Vector2 noteScale, GlyphData noteGlyphData, Vector2i windowSize, Vector2i? posOffset = null)
		{
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
				// 	vertices.Add(vt);
				// }
			}
			if (I != 0)
			{
				float[] vt = new float[I];
				Array.Copy(v, vt, I);
				vertices.Add(vt);
			}
			float[][] V = new float[vertices.Count][];
			for (int i = vertices.Count; i-- > 0;) { V[i] = vertices[i]; }
			return V;
		}
		public void Update()
		{
			if (!Playing)
			{
				time = (timeAtPause - timeOffset) / (double)Stopwatch.Frequency;
			}
			for (int i = 0; i < KeyCount; i++)
			{
				ManiaKey[] laneData = chart.KeyData[i];
				uint currentKey = currentKeys[i];
				uint currentKeyNow = currentKey;
				while (currentKey < laneData.Length && laneData[currentKey].time - time <= -0.3) { currentKey++; } // calculates miss amounts
				currentKeys[i] = currentKey;
				uint missAmount = currentKey - currentKeyNow;
				if (missAmount != 0) { Console.WriteLine("Missed " + missAmount + " notes this frame."); noteHitAccAmts[(int)ManiaAccs.miss] += missAmount; }
			}
			if (training == 1)
			{
				bool firstRefresh = true;
				uint currentKey;
				for (int i = 0; i < KeyCount; i++)
				{
					if ((currentKey = currentKeys[i]) > 64)
					{
						ManiaKey[] laneData = chart.KeyData[i];
						if (firstRefresh)
						{
							firstRefresh = false;
							Console.Write("refreshing lane " + i);
						}
						else { Console.Write(", " + i); }
						// current key is greater than 64; at least 64 notes were hit or missed or smth
						// so anyways generate more notes
						ManiaKey[] newLaneData = new ManiaKey[4096];
						Array.Copy(laneData, currentKey, newLaneData, 0, 4096 - currentKey);
						double lastNoteTime = laneData[4095].time;
						uint startingIndex = 4096 - currentKey;
						for (uint j = 0; j < currentKey;)
						{
							newLaneData[j + startingIndex] = new ManiaKey(lastNoteTime + 1.8 / Math.Sqrt(++j + 80));
						}
						chart.KeyData[i] = newLaneData;
						currentKeys[i] = 0;
					}
				}
			}
		}
		public void Render(Game game, bool sdfjskls = false, Vector3? color = null)
		{
			GlyphData noteGlyphData = FontCharFillerThing.FontCharDeeta.SChars["note"];
			float[][] data = CalcVertices(new((float)Math.Sin(time) * .4f, (float)(Math.Cos(time) * .05f)), new(12), noteGlyphData, game._clientSize);
			if (data.Length > 0) { gameRenderer.RenderWithPrecalculatedVertices(data, game._textShader, color ?? new(1));}
			gameRenderer.RenderText(game, game._textShader, "Map time: " + time + "\n" + lingeringTxt, new(0), new(-.5f, 0.3f), new(8), color ?? new(1, 0, 1), 10f, game._clientSize, FontCharFillerThing.FontCharDeeta, false);
			if (sdfjskls)
			{
				Console.WriteLine("data thing: " + data);
				Console.WriteLine("data length: " + data.Length);
				foreach (float[] d in data) { Console.WriteLine("data: " + d+","+d.Length); }
			}
			if (DisplayFullInfo)
			{
				string FullInfoText = "Ratings:";
				foreach (ManiaAccs s in Enum.GetValues<ManiaAccs>()) FullInfoText += "\n" + Enum.GetName(s) + ": " + noteHitAccAmts[(int)s];
				gameRenderer.RenderText(game, game._textShader, FullInfoText, new(0), new(0.5f, 0.3f), new(2), color ?? new(1, 0, 1), 10f, game._clientSize, FontCharFillerThing.FontCharDeeta, false);
			}
		}
		public void Render2(Shader shader, Game game, Vector3? _color = null)
		{
			if (!Playing)
			{
				time = (timeAtPause - timeOffset) / (double)Stopwatch.Frequency;
			}
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
					I = 0;
				} else I += 16;
			}
			if (I != 0) {
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * I, v);
				GL.DrawElements(PrimitiveType.Triangles, I * 3 / 8, DrawElementsType.UnsignedInt, 0);
			}
			// GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			// GL.BindVertexArray(0);

			gameRenderer.RenderText(game, game._textShader, "Map time: " + time + "\n" + lingeringTxt, new(0), new(-.5f, 0.3f), new(8), new(1, 0, 1), 10f, game._clientSize, FontCharFillerThing.FontCharDeeta, false);
			if (DisplayFullInfo)
			{
				string FullInfoText = "Ratings:";
				foreach (ManiaAccs s in Enum.GetValues<ManiaAccs>()) FullInfoText += "\n" + Enum.GetName(s) + ": " + noteHitAccAmts[(int)s];
				gameRenderer.RenderText(game, game._textShader, FullInfoText, new(0), new(0.5f, 0.3f), new(2), new(1, 0, 1), 10f, game._clientSize, FontCharFillerThing.FontCharDeeta, false);
			}
		}
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
					default: break;}
			} else if (Playing) {
				Console.Write("lane: " + laneNumberThing + " ");
				// key is one of the keybinds.
				uint currentKey = currentKeys[laneNumberThing];
				ManiaKey[] laneData = chart.KeyData[laneNumberThing];
				uint currentKeyNow = currentKey;
				while (currentKey < laneData.Length && laneData[currentKey].time - time <= -0.3) { currentKey++; }
				uint missAmount = currentKey - currentKeyNow;
				if (missAmount != 0) { Console.WriteLine("missed " + missAmount + " notes this frame."); noteHitAccAmts[(int)ManiaAccs.miss] += missAmount; }
				if (currentKey >= laneData.Length) { Console.WriteLine("There are no more notes on lane " + laneNumberThing); return; }
				ManiaKey CurrentKey = laneData[currentKey];
				double keyTime = CurrentKey.time;
				double timeDiff = keyTime - time; // the amount of time before the note would be perfectly at the place.
				string DiffMSStr = timeDiff*1000+"MS"; // the amount of time before the note would be perfectly at the place.
				bool Hit = true;
				switch (timeDiff) { // big beautiful wall of text
					case > 0.5: Hit = false; noteHitAccAmts[(int)ManiaAccs.noNotePressed]++;/* lingeringText="No note was hit. " + timeDiff * 1000 + "MS";*/ Console.Write("No note was hit."); break;
					case > 0.3: noteHitAccAmts[(int)ManiaAccs.tooEarly]++; lingeringTxt = "TOO early... " + DiffMSStr; Console.Write("Too early."); break;
					case 0: noteHitAccAmts[(int)ManiaAccs.PERFECT]++; lingeringTxt = "PERFECT " + DiffMSStr; Console.Write("PERFECT TIMING?? :O;"); break;
					case > -.0000005 and < .0000005: noteHitAccAmts[(int)ManiaAccs.perfecthmcs]++; lingeringTxt = "HALF MICROSECOND PERFECT " + DiffMSStr; Console.Write("+-0.5μs TIMING??????;"); break;
					case > -.000001 and < .000001: noteHitAccAmts[(int)ManiaAccs.perfect1mcs]++; lingeringTxt = "MICROSECOND PERFECT " + DiffMSStr; Console.Write("+-1μs TIMING??????;"); break;
					case > -.000002 and < .000002: noteHitAccAmts[(int)ManiaAccs.perfect2mcs]++; lingeringTxt = "2MCS PERFECT " + DiffMSStr; Console.Write("+-2μs???;"); break;
					case > -.000003 and < .000003: noteHitAccAmts[(int)ManiaAccs.perfect3mcs]++; lingeringTxt = "3MCS PERFECT " + DiffMSStr; Console.Write("+-3μs???;"); break;
					case > -.000005 and < .000005: noteHitAccAmts[(int)ManiaAccs.perfect5mcs]++; lingeringTxt = "5MCS PERFECT " + DiffMSStr; Console.Write("+-5μs???;"); break;
					case > -.00001 and < .00001: noteHitAccAmts[(int)ManiaAccs.perfect10mcs]++; lingeringTxt = "10MCS PERFECT " + DiffMSStr; Console.Write("+-10μs???;"); break;
					case > -.000025 and < .000025: noteHitAccAmts[(int)ManiaAccs.perfect25mcs]++; lingeringTxt = "25MCS PERFECT " + DiffMSStr; Console.Write("+-25μs???;"); break;
					case > -.00005 and < .00005: noteHitAccAmts[(int)ManiaAccs.perfect50mcs]++; lingeringTxt = ".05MS Perfect! " + DiffMSStr; Console.Write("+-50μs???;"); break;
					case > -.0001 and < .0001: noteHitAccAmts[(int)ManiaAccs.perfect100mcs]++; lingeringTxt = ".1MS Perfect! " + DiffMSStr; Console.Write("+-100μs!!;"); break;
					case > -.00025 and < .00025: noteHitAccAmts[(int)ManiaAccs.qmsPerfect]++; lingeringTxt = ".25ms perfect! " + DiffMSStr; Console.Write("Hit within +-0.25ms!!;"); break;
					case > -.0005 and < .0005: noteHitAccAmts[(int)ManiaAccs.hmsPerfect]++; lingeringTxt = ".5ms perfect! " + DiffMSStr; Console.Write("Hit within +-0.5ms!!;"); break;
					case > -.001 and < .001: noteHitAccAmts[(int)ManiaAccs.msPerfect]++; lingeringTxt = "Millisecond perfect! " + DiffMSStr; Console.Write("Hit within +-1ms!!;"); break;
					case > -1d / 480d and < 1d / 480d: noteHitAccAmts[(int)ManiaAccs.fp240]++; lingeringTxt = "Frame perfect at 240fps! " + DiffMSStr; Console.Write("Frame perfect at 240fps!;"); break;
					case > -1d / 240d and < 1d / 240d: noteHitAccAmts[(int)ManiaAccs.fp120]++; lingeringTxt = "Frame perfect at 120fps! " + DiffMSStr; Console.Write("Frame perfect at 120fps!;"); break;
					case > -1d / 120d and < 1d / 120d: noteHitAccAmts[(int)ManiaAccs.fp60]++; lingeringTxt = "Frame perfect at 60fps! " + DiffMSStr; Console.Write("Frame perfect at 60fps!;"); break;
					case > -.01 and < .01: noteHitAccAmts[(int)ManiaAccs.excellent]++; lingeringTxt = "Excellent! " + DiffMSStr; Console.Write("Excellent +-10ms!;"); break;
					case > -1d / 60d and < 1d / 60d: noteHitAccAmts[(int)ManiaAccs.fp30]++; lingeringTxt = "Frame perfect at 30fps! " + DiffMSStr; Console.Write("Frame perfect at 30fps! (+-1/60);"); break;
					case > -.03 and < .03: noteHitAccAmts[(int)ManiaAccs.sick]++; lingeringTxt = "Sick! " + DiffMSStr; Console.Write("Sick! (+-30ms);"); break;
					case > -.05 and < .05: noteHitAccAmts[(int)ManiaAccs.great]++; lingeringTxt = "Great! " + DiffMSStr; Console.Write("Great (+-50ms);"); break;
					case > -.08 and < .08: noteHitAccAmts[(int)ManiaAccs.good]++; lingeringTxt = "Good. " + DiffMSStr; Console.Write("Good. (+-80ms);"); break;
					case > -.11 and < .11: noteHitAccAmts[(int)ManiaAccs.okay]++; lingeringTxt = "Okay. " + DiffMSStr; Console.Write("Okay. (+-110ms);"); break;
					case > -.15 and < .15: noteHitAccAmts[(int)ManiaAccs.bad]++; lingeringTxt = "Bad. " + DiffMSStr; Console.Write("Bad. (+-150ms);"); break;
					case > -.3 and < .3: noteHitAccAmts[(int)ManiaAccs.yikes]++; lingeringTxt = "Yikes. " + DiffMSStr; Console.Write("Yikes. (+-300ms);"); break;
					default: Hit = false; break;}
				Console.WriteLine(" TimeDiff: " + timeDiff);
				if (Hit) currentKeys[laneNumberThing]++; }
		}
		public bool TryLoadMapFromString(string chart)
		{
			string[] processedS1 = chart.Split('\n');
			if (processedS1.Length < 2) return false; // must have at least 2 lines; first is version number
			Console.WriteLine("Version number: " + processedS1[0]);
			ManiaKey[][] KeyData = new ManiaKey[KeyCount][];
			for (int j = 1; j < Math.Min(processedS1.Length, KeyCount); j++)
			{
				string[] processedS2 = processedS1[j].Split(',');
				KeyData[j] = new ManiaKey[processedS2.Length];
				ManiaKey[] keyData1 = KeyData[j];
				for (int i = 0; i < processedS2.Length; i++)
				{
					string[] stringThingy = processedS2[i].Split(' ');
					switch (stringThingy.Length)
					{
						case 1: // not a hold note
							keyData1[i] = new ManiaKey(Convert.ToDouble(stringThingy[0])); break;
						case 2: // hold note
							keyData1[i] = new ManiaKey(Convert.ToDouble(stringThingy[0]), Convert.ToSingle(stringThingy[1])); break;
						default: return false;
					}
				}
			}
			ManiaChart NewChart = new(KeyData);
			LoadMap(NewChart);
			return true;
		}
		public void StartTraining(uint difficulty = 1)
		{
			Console.WriteLine("starting training dksflsfd");
			ManiaKey[][] keyData = new ManiaKey[KeyCount][];
			for (int i = 0; i < KeyCount; i++)
			{
				keyData[i] = new ManiaKey[4096];
				for (int j = 0; j < 4096; j++)
				{
					keyData[i][j] = new ManiaKey(0.2d * (j + (double)i / KeyCount));
				}
			}
			chart = new ManiaChart(keyData);
			training = difficulty;
			Console.WriteLine(training);
			Console.WriteLine("keydata length: " + keyData.Length);
			for (int i = 0; i < keyData.Length; i++)
			{
				Console.WriteLine("lane " + i + " length: " + keyData[i].Length);
			}
		}
		public void RestartTraining()
		{
			ManiaKey[][] keyData = new ManiaKey[KeyCount][];
			for (int i = 0; i < KeyCount; i++)
			{
				double iOverkC = (double)i / KeyCount; // i divided by key count; i over key count.
				ManiaKey[] laneKeyData = new ManiaKey[4096];
				for (int j = 0; j < 4096; j++)
				{
					laneKeyData[j] = new ManiaKey(0.2d*(j+iOverkC));
				}
				keyData[i] = laneKeyData;
			}
			chart = new ManiaChart(keyData);
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
	public enum ManiaAccs {
		miss,
		noNotePressed,
		tooEarly, // 300 to 500ms early
		yikes, // +-300ms
		bad, // +-150ms
		okay, // +-110ms
		good, // +-80ms
		great, // +-50ms
		sick, // +-30ms
		fp30, // +-1/60fps; the fp here stands for frame perfect.
		excellent, // 10ms
		fp60, // 8ms
		fp120, // 4ms
		fp240, // 2ms
		msPerfect, // 1ms
		hmsPerfect, // .5ms
		qmsPerfect, // .25ms
		perfect100mcs,
		perfect50mcs,
		perfect25mcs,
		perfect10mcs,
		perfect5mcs,
		perfect3mcs,
		perfect2mcs,
		perfect1mcs,
		perfecthmcs,
		PERFECT
 }
	public class VerticalOneKey {
		public Keys keybind = Keys.E;
		public Text gameRenderer;
		public Stopwatch stopwatch = new();
		public VerticalOneKey(Text renderer) { gameRenderer = renderer; }
		private string lingeringText = "No note hit yet";
		public float timeOffset = 0;
		private uint currentKey = 0;
		// private uint lastDisplayedKey = 0; // might be used to cull the notes at some point.
		public double time = 0;
		// public double dt = 0;
		public float scrollSpeed = 1.0f; // what fraction of a second does it take for a note to go from the top to the bottom.
		public static bool DisplayFullInfo = false;
		public uint[] noteHitAccAmounts = new uint[Enum.GetValues<ManiaAccs>().Length]; // an array containing how many times the player has hit each of the accuracies including misses.
		private V1KChart chart;
		public void LoadMap(V1KChart chart) {
			currentKey = 0;
			// lastDisplayedKey = 0;
			stopwatch.Reset();
			this.chart = chart;}
		public void StartMap() {
			stopwatch.Start(); }
		public void RestartMap() {
			timeOffset = -3;
			currentKey = 0;
			// lastDisplayedKey = 0;
			stopwatch.Restart(); }
		public void TogglePauseMap() {
			if (stopwatch.IsRunning) stopwatch.Stop(); else stopwatch.Start(); }
		public void PauseMap() {
			stopwatch.Stop(); }
		public float[][] CalcVertices(Vector2i posOffset, Vector2 posScale, Vector2 noteScale, GlyphData noteGlyphData, Vector2i windowSize) {
			float ftexW = gameRenderer.TextTexture.Width;
			float ftexH = gameRenderer.TextTexture.Height;
			float halfOfCeilTrueSizeX = MathF.Ceiling(noteGlyphData.size.X * noteScale.X) * 0.5f;
			float halfOfCeilTrueSizeY = MathF.Ceiling(noteGlyphData.size.Y * noteScale.Y) * 0.5f;
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
			float baseSX = (MathF.Floor(oX - halfOfCeilTrueSizeX) + .5f) / WinSX; // the base value for the start X
			float baseSY = (MathF.Floor(oY - halfOfCeilTrueSizeY) + .5f) / WinSY; // the base value for the start Y
			float baseNX = (MathF.Floor(oX + halfOfCeilTrueSizeX) + .5f) / WinSX; // the base value for the end X
			float baseNY = (MathF.Floor(oY + halfOfCeilTrueSizeY) + .5f) / WinSY; // the base value for the end Y
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
			float halfOfCeilTrueJudgementSizeX = MathF.Ceiling(noteGlyphData.size.X * noteScale.X) * 0.625f;
			float halfOfCeilTrueJudgementSizeY = MathF.Ceiling(noteGlyphData.size.Y * noteScale.Y) * 0.5f;
			float SX = (MathF.Floor(oX - halfOfCeilTrueJudgementSizeX) + .5f) / WinSX;
			float SY = (MathF.Floor(oY - halfOfCeilTrueJudgementSizeY) + .5f) / WinSY;
			float NX = (MathF.Floor(oX + halfOfCeilTrueJudgementSizeX) + .5f) / WinSX;
			float NY = (MathF.Floor(oY + halfOfCeilTrueJudgementSizeY) + .5f) / WinSY;
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
            if (missAmount != 0) { Console.WriteLine("Missed " + missAmount + " notes this frame."); noteHitAccAmounts[(int)ManiaAccs.miss] += missAmount; }
        }
		public void Render(Game game) {
			GlyphData noteGlyphData = FontCharFillerThing.FontCharDeeta.SChars["note"];
			float[][] data = CalcVertices(Vector2i.Zero, new((float)Math.Sin(time)*.4f,(float)(-.2+Math.Cos(time)*.05f)), new(16), noteGlyphData, game._clientSize);
			if (data.Length > 0) gameRenderer.RenderWithPrecalculatedVertices(data, game._textShader, new(1));
			gameRenderer.RenderText(game, game._textShader, "Map time: " + time + "\n" + lingeringText, new(0), new(-.5f,0.3f), new(8), new(1,0,1), 10f, game._clientSize, FontCharFillerThing.FontCharDeeta, false);
			if (DisplayFullInfo) {
				string FullInfoText = "Ratings:";
				foreach (ManiaAccs s in Enum.GetValues<ManiaAccs>()) FullInfoText += "\n" + Enum.GetName(s) + ": " + noteHitAccAmounts[(int)s];
				gameRenderer.RenderText(game, game._textShader, FullInfoText, new(0), new(0.5f,0.3f), new(2), new(1,0,1), 10f, game._clientSize, FontCharFillerThing.FontCharDeeta, false); }}
		public void KeyDown(Keys key) {
			if (key == keybind) { // do this only if it is the keybind.
				double time = stopwatch.Elapsed.TotalSeconds + timeOffset;
				uint currentKeyNow = currentKey;
				while (currentKey < chart.KeyData.Length && chart.KeyData[currentKey].time - time <= -0.3) { currentKey++; }
				uint missAmount = currentKey - currentKeyNow;
				if (missAmount != 0) { Console.WriteLine("Missed " + missAmount + " notes this frame."); noteHitAccAmounts[(int)ManiaAccs.miss] += missAmount; }
				if (currentKey >= chart.KeyData.Length) { Console.WriteLine("There are no more notes; the chart has ended ig"); return; }
				V1KKey CurrentKey = chart.KeyData[currentKey];
				double keyTime = CurrentKey.time;
				double timeDiff = keyTime - time; // the amount of time before the note would be perfectly at the place.
				bool Hit = true;
				switch (timeDiff) {
					case > 0.5:
						Hit = false;
						noteHitAccAmounts[(int)ManiaAccs.noNotePressed]++;
						// lingeringText = "No note was hit. " + timeDiff * 1000 + "MS";
						Console.Write("No note was hit.");
						break;
					case > 0.3:
						noteHitAccAmounts[(int)ManiaAccs.tooEarly]++;
						lingeringText = "TOO early... " + timeDiff * 1000 + "MS";
						Console.Write("Too early.");
						break;
					// case > -.0000000000001 and < .0000000000001: Console.WriteLine("+-100femptoseconds??????"); break;
					// case > -.000000000001 and < .000000000001: Console.WriteLine("+-1ps??????"); break;
					// case > -.00000000001 and < .00000000001: Console.WriteLine("+-10ps??????"); break;
					// case > -.0000000001 and < .0000000001: Console.WriteLine("+-100ps TIMING??????"); break;
					// case > -.000000001 and < .000000001: Console.WriteLine("+-1ns TIMING??????"); break;
					// case > -.00000001 and < .00000001: Console.WriteLine("+-10ns TIMING??????"); break;
					// case > -.0000001 and < .0000001: Console.WriteLine("+-100ns TIMING??????"); break;
					case 0:
						noteHitAccAmounts[(int)ManiaAccs.PERFECT]++;
						lingeringText = "PERFECT " + timeDiff * 1000 + "MS";
						Console.Write("PERFECT TIMING?? :O;"); break;
					case > -.0000005 and < .0000005:
						noteHitAccAmounts[(int)ManiaAccs.perfecthmcs]++;
						lingeringText = "HALF MICROSECOND PERFECT " + timeDiff * 1000 + "MS";
						Console.Write("+-0.5μs TIMING??????;"); break;
					case > -.000001 and < .000001:
						noteHitAccAmounts[(int)ManiaAccs.perfect1mcs]++;
						lingeringText = "MICROSECOND PERFECT " + timeDiff * 1000 + "MS";
						Console.Write("+-1μs TIMING??????;"); break;
					case > -.000002 and < .000002:
						noteHitAccAmounts[(int)ManiaAccs.perfect2mcs]++;
						lingeringText = "2MCS PERFECT " + timeDiff * 1000 + "MS";
						Console.Write("+-2μs???;"); break;
					case > -.000003 and < .000003:
						noteHitAccAmounts[(int)ManiaAccs.perfect3mcs]++;
						lingeringText = "3MCS PERFECT " + timeDiff * 1000 + "MS";
						Console.Write("+-3μs???;"); break;
					case > -.000005 and < .000005:
						noteHitAccAmounts[(int)ManiaAccs.perfect5mcs]++;
						lingeringText = "5MCS PERFECT " + timeDiff * 1000 + "MS";
						Console.Write("+-5μs???;"); break;
					case > -.00001 and < .00001:
						noteHitAccAmounts[(int)ManiaAccs.perfect10mcs]++;
						lingeringText = "10MCS PERFECT " + timeDiff * 1000 + "MS";
						Console.Write("+-10μs???;"); break;
					case > -.000025 and < .000025:
						noteHitAccAmounts[(int)ManiaAccs.perfect25mcs]++;
						lingeringText = "25MCS PERFECT " + timeDiff * 1000 + "MS";
						Console.Write("+-25μs???;"); break;
					case > -.00005 and < .00005:
						noteHitAccAmounts[(int)ManiaAccs.perfect50mcs]++;
						lingeringText = ".05MS Perfect! " + timeDiff * 1000 + "MS";
						Console.Write("+-50μs???;"); break;
					case > -.0001 and < .0001:
						noteHitAccAmounts[(int)ManiaAccs.perfect100mcs]++;
						lingeringText = ".1MS Perfect! " + timeDiff * 1000 + "MS";
						Console.Write("+-100μs!!;"); break;
					case > -.00025 and < .00025:
						noteHitAccAmounts[(int)ManiaAccs.qmsPerfect]++;
						lingeringText = ".25ms perfect! " + timeDiff * 1000 + "MS";
						Console.Write("Hit within +-0.25ms!!;"); break;
					case > -.0005 and < .0005:
						noteHitAccAmounts[(int)ManiaAccs.hmsPerfect]++;
						lingeringText = ".5ms perfect! " + timeDiff * 1000 + "MS";
						Console.Write("Hit within +-0.5ms!!;"); break;
					case > -.001 and < .001:
						noteHitAccAmounts[(int)ManiaAccs.msPerfect]++;
						lingeringText = "Millisecond perfect! " + timeDiff * 1000 + "MS";
						Console.Write("Hit within +-1ms!!;"); break;
					case > -1d / 480d and < 1d / 480d:
						noteHitAccAmounts[(int)ManiaAccs.fp240]++;
						lingeringText = "Frame perfect at 240fps! " + timeDiff * 1000 + "MS";
						Console.Write("Frame perfect at 240fps!;"); break;
					case > -1d / 240d and < 1d / 240d:
						noteHitAccAmounts[(int)ManiaAccs.fp120]++;
						lingeringText = "Frame perfect at 120fps! " + timeDiff * 1000 + "MS";
						Console.Write("Frame perfect at 120fps!;"); break;
					case > -1d / 120d and < 1d / 120d:
						noteHitAccAmounts[(int)ManiaAccs.fp60]++;
						lingeringText = "Frame perfect at 60fps! " + timeDiff * 1000 + "MS";
						Console.Write("Frame perfect at 60fps!;"); break;
					case > -.01 and < .01:
						noteHitAccAmounts[(int)ManiaAccs.excellent]++;
						lingeringText = "Excellent! " + timeDiff * 1000 + "MS";
						Console.Write("Excellent +-10ms!;"); break;
					case > -1d / 60d and < 1d / 60d:
						noteHitAccAmounts[(int)ManiaAccs.fp30]++;
						lingeringText = "Frame perfect at 30fps! " + timeDiff * 1000 + "MS";
						Console.Write("Frame perfect at 30fps! (+-1/60);"); break;
					case > -.03 and < .03:
						noteHitAccAmounts[(int)ManiaAccs.sick]++;
						lingeringText = "Sick! " + timeDiff * 1000 + "MS";
						Console.Write("Sick! (+-30ms);"); break;
					case > -.05 and < .05:
						noteHitAccAmounts[(int)ManiaAccs.great]++;
						lingeringText = "Great! " + timeDiff * 1000 + "MS";
						Console.Write("Great (+-50ms);"); break;
					case > -.08 and < .08:
						noteHitAccAmounts[(int)ManiaAccs.good]++;
						lingeringText = "Good. " + timeDiff * 1000 + "MS";
						Console.Write("Good. (+-80ms);"); break;
					case > -.11 and < .11:
						noteHitAccAmounts[(int)ManiaAccs.okay]++;
						lingeringText = "Okay. " + timeDiff * 1000 + "MS";
						Console.Write("Okay. (+-110ms);"); break;
					case > -.15 and < .15:
						noteHitAccAmounts[(int)ManiaAccs.bad]++;
						lingeringText = "Bad. " + timeDiff * 1000 + "MS";
						Console.Write("Bad. (+-150ms);"); break;
					case > -.3 and < .3:
						noteHitAccAmounts[(int)ManiaAccs.yikes]++;
						lingeringText = "Yikes. " + timeDiff * 1000 + "MS";
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
        public bool TryLoadMapFromString(string chart)
        {
            string[] processedS1 = chart.Split('\n');
            if (processedS1.Length < 2) return false; // must have at least 2 lines; first is version number
            Console.WriteLine("Version number: " + processedS1[0]);
            string[] processedS2 = processedS1[1].Split(',');
            V1KKey[] keyData1 = new V1KKey[processedS2.Length];
            for (int i = 0; i < processedS2.Length; i++)
            {
                string[] stringThingy = processedS2[i].Split(' ');
                switch (stringThingy.Length)
                {
                    case 1: // not a hold note
                        keyData1[i] = new V1KKey(Convert.ToDouble(stringThingy[0])); break;
                    case 2: // hold note
                        keyData1[i] = new V1KKey(Convert.ToDouble(stringThingy[0]), Convert.ToSingle(stringThingy[1])); break;
                    default: return false;
                }
            }
            V1KChart NewChart = new(keyData1);
            LoadMap(NewChart);
            return true;
        }
    }}