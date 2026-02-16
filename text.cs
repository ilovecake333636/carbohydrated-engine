using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace GameEngineThing {
	public class Text {
		public Texture TextTexture { get; set; }
		public static readonly char[] CharSearchThingy = ['|', '\\', '\n'];
		public const int BulkDrawConst = 1048576;
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
			Render(TextThingies[i], game, text, shader ?? game._textShader, windowSize ?? game._clientSize); }
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
				// if (vI != 0) { float[] verts = new float[vI]; Array.Copy(vertices, verts, vI); v.Add(verts);}
				if (vI != 0) { v.Add(vertices[..vI]); }
			} else {
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
				if (vI != 0) { float[] verts = new float[vI]; Array.Copy(vertices, verts, vI); v.Add(verts);}}

			float[][] V = new float[v.Count][];
			for (int i = 0; i < v.Count; i++) { V[i] = v[i]; }
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
		public void RenderBarGraph(Game game, Vector2 position, (float, float) size, Vector3 color, double[] data) {
			(int windowSizeX, int windowSizeY) = (game._clientSize.X, game._clientSize.Y);
			(float sizeX, float sizeY) = size;
			Shader shader = game._textShader;
			shader.Use();
			shader.SetVector3("textColor", color);
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

			float absPosX = MathF.Floor(position.X * windowSizeX) + 0.5f;
			float absPosY = position.Y * windowSizeY;

			float[] vertices = new float[BulkDrawFloats];
			int vI = 0;
			// int tRSX = 0;
			int tRSY = 32;
			// float tX = tRSX / (float)TextTexture.Width;
			float tX = 0;
			float tY = tRSY / (float)TextTexture.Height;
			float startY = (0.5f + MathF.Floor(absPosY)) / windowSizeY;

			if (data.Length > BulkDrawFloats / 4) { // if it takes more than one array to store all of the data
				vertices[1] = startY;
				vertices[2] = vertices[6] = tX;
				vertices[3] = vertices[7] = tY;
				int i = 8;
				// for (; i < BulkDrawFloats/2+1; i *= 2)
				do { Array.Copy(vertices, 1, vertices, i + 1, i - 1); i *= 2; } while (i < BulkDrawFloats / 2 + 1);
				if (i < BulkDrawFloats) Array.Copy(vertices, 1, vertices, i + 1, BulkDrawFloats - i - 1);
				// please work
				vertices.CopyTo(vertices, 0);
				
				for (i = 0; i < data.Length; i++) {
					// float xPos = (0.5f + MathF.Floor(absPosX)) / windowSizeX;
					// float endY = (float)((0.5 + Math.Floor(absPosY + data[i] / sizeY)) / windowSizeY);

					// vertices[vI] = vertices[vI + 4] = xPos;
					vertices[vI] = vertices[vI + 4] = absPosX / windowSizeX;
					vertices[vI + 5] = (float)((0.5 + Math.Floor(absPosY + data[i] / sizeY)) / windowSizeY);
					// vertices[vI + 5] = endY;
					if (vI == BulkDrawFloats - 8) {
						vI = 0;
						GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * BulkDrawFloats, vertices);
						GL.DrawArrays(PrimitiveType.Lines, 0, BulkDrawFloats); /*it's still bulkdrawfloats because i have an array thing of some length and i'll use ALL of it.*/ }
					else vI += 8;
					// absPos += new Vector2i((int)(Chr.advance.X * o.textScaleX), (int)(Chr.advance.Y * o.textScaleY));
					absPosX += sizeX * 2; } }
			// vertices[1] = startY;
			// vertices[2] = vertices[6] = tX;
			// vertices[3] = vertices[7] = tY;

			else for (int i = 0; i < data.Length; i++) {
				// float xPos = (0.5f + MathF.Floor(absPosX)) / windowSizeX;
				// float endY = (float)((0.5 + Math.Floor(absPosY + data[i] / sizeY)) / windowSizeY);

				// vertices[vI] = vertices[vI + 4] = xPos;
				vertices[vI] = vertices[vI + 4] = absPosX / windowSizeX;
				vertices[vI + 1] = startY;
				vertices[vI + 2] = vertices[vI + 6] = tX;
				vertices[vI + 3] = vertices[vI + 7] = tY;
				vertices[vI + 5] = (float)((0.5 + Math.Floor(absPosY + data[i] / sizeY)) / windowSizeY);
				// vertices[vI + 5] = endY;
				if (vI == BulkDrawFloats - 8) { vI = 0;
					GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * BulkDrawFloats, vertices);
					GL.DrawArrays(PrimitiveType.Lines, 0, BulkDrawFloats); /*it's still bulkdrawfloats because i have an array thing of some length and i'll use ALL of it.*/ }
				else vI += 8;
				// absPos += new Vector2i((int)(Chr.advance.X * o.textScaleX), (int)(Chr.advance.Y * o.textScaleY));
				absPosX += sizeX * 2; }
			if (vI != 0) {
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * vI, vertices);
				GL.DrawArrays(PrimitiveType.Lines, 0, vI); /*it's still bulkdrawfloats because i have an array thing of some length and i'll use ALL of it.*/ }
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);}
		public void ProfilerRender(Game game) {
			(float posX, float posY) = (1f, 1f);
			double[] data = game.profilerFrameTimes;
			int startIndex = game.profilerIndex;
			const float sizeX = -2f;
			const double sizeY = -1f / 60f;
			(int windowSizeX, int windowSizeY) = game._clientSize;
			(float WSXInv, float WSYInv) = (1f/windowSizeX, 1f/windowSizeY);
			
			int dataLen = data.Length;
			Shader shader = game._textShader;
			shader.Use();
			shader.SetVector3("textColor", new(0.8f, 0.4f, 0f));
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			
			float absPosX = (MathF.Floor(posX * windowSizeX) + 0.5f)*WSXInv;
			float XIncrementAmt = sizeX*WSXInv;
			float absPosY = MathF.Floor(posY * windowSizeY) + 0.5f;

			float[] v = game.profilerVD;
			int vI = 0;
			int i;
			int k = startIndex % dataLen;
			int dlm1 = dataLen - 1;
			for (int j = startIndex; j > startIndex - dataLen; j--, vI += 8, absPosX += XIncrementAmt, k = (k + dlm1) % dataLen) {
				v[vI]=v[vI+4]=absPosX;
				v[vI+5]=(0.5f+MathF.Floor((float)(absPosY+data[k]/sizeY)))*WSYInv;}
			GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * vI, v);
			GL.DrawArrays(PrimitiveType.Lines, 0, vI >> 2);
			int pl = dataLen;
			int pi = game.profilerIndex + pl - 1;
			double a = data[pi%pl];
			string txt = "FPS1: " + (1000d / a);
			for(i=pi-1;i>pi-10;i--){a+=data[i%pl];}txt+="\nFPS10: "+(10000d/a);
			for(i=pi-10;i>pi-30;i--){a+=data[i%pl];}txt+="\nFPS30: "+(30000d/a);
			for(i=pi-30;i>pi-50;i--){a+=data[i%pl];}txt+="\nFPS50: "+(50000d/a);
			for(i=pi-50;i>pi-100;i--){a+=data[i%pl];}txt+="\nFPS100: "+(100000d/a);
			ReadOnlySpan<char> text = txt.AsSpan();
			const int posOffsetX = 0;
			const int posOffsetY = -40;
			const float posScaleX = -1f;
			const float posScaleY = 1f;
			const float textScaleX = 4;
			const float textScaleY = 4;
			shader.SetVector3("textColor", new Vector3(1,1,0));
			const float lineHeight = 10f;
			const float lineHeightScaled = lineHeight * textScaleY;
			FontCharacterData fontCharData = FontCharFillerThing.FontCharDeeta;
			// Vector2 absPos = new(posOffsetX + posScaleX * windowSizeX, posOffsetY + posScaleY * windowSizeY);
			float startPosX = absPosX = posOffsetX + posScaleX * windowSizeX; absPosY = posOffsetY + posScaleY * windowSizeY;
			const float spaceSize = textScaleX * 3;
			// const float tabSize = spaceSize * 4;
			float[] vertices = new float[BulkDrawFloats];
			vI = 0;
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			// int incorrectSpecialChar = 0;
			int txtLen = text.Length;
			int lm1 = txtLen - 1;
			float TWInv = 1f / TextTexture.Width;
			float THInv = 1f / TextTexture.Height;
			GlyphData Chr;
			char c;
			for (i = 0; i < txtLen; i++) {
				c = text[i];
				// if (incorrectSpecialChar > 0 && --incorrectSpecialChar == 0 && c == '|') continue;
				switch (c) {
					case ' ': absPosX += spaceSize; continue;
					// case '	': absPosX += tabSize; continue;
					case '\n': absPosX = startPosX; absPosY -= lineHeightScaled; continue;
					case '\\':
						int ip1 = i + 1;
						char nextChar = text[ip1];
						if (ip1 > lm1 || nextChar == '\\') { Chr = fontCharData.Chars['\\']; break; } // if this is the last char or the next char is another '\\' then show a '\\' char.
						if (nextChar == '|') { Chr = fontCharData.Chars['\\']; i++; break; } // if the next char is a | char (my format is \| for '\\' chars) then show a '\\', then increment i so the '|' isn't shown.
						if (nextChar == '\n') { i++; goto case '\n'; } // if the line goes to a new line then increment i and do the next line stuff.
						int IsStacking = 0;
						int j = txt.IndexOfAny(CharSearchThingy, ip1);
						if (j == -1) j = txtLen; else if (text[j] != '|') IsStacking = 1;
						int len = j - ip1;
						// string s;
						// if (len == -1) s = txt[ip1..];
						// else s = txt.Substring(ip1, len);
						// string s = len == -1 ? txt[ip1..] : txt.Substring(ip1, len);
						// if (!fontCharData.SChars.TryGetValue(s, out Chr)) // if it can find the special character then jump to the index of the last chr in the special character, and the next char will be a new one.
						if (!fontCharData.SChars.TryGetValue(len==-1?txt[ip1..]:txt.Substring(ip1,len),out Chr)) // if it cant find the special character then uhh idk
							/*incorrectSpecialChar = len + 1; Chr = fontCharData.Chars['\\'];*/Chr=fontCharData.SChars["unknown"]; i = j - IsStacking;
						break;
					default: if (!fontCharData.Chars.TryGetValue(c, out Chr)) Chr = fontCharData.Chars['?']; break;}
				float startX = (MathF.Floor(absPosX + Chr.bearing.X * textScaleX) + .5f) * WSXInv;
				float startY = (MathF.Floor(absPosY + Chr.bearing.Y * textScaleY) + .5f) * WSYInv;
				float endX = (MathF.Floor(absPosX + Chr.bearing.X * textScaleX + MathF.Ceiling(Chr.size.X * textScaleX)) + .5f) * WSXInv;
				float endY = (MathF.Floor(absPosY + Chr.bearing.Y * textScaleY + MathF.Ceiling(Chr.size.Y * textScaleY)) + .5f) * WSYInv;

				float tStartX = Chr.textureStart.X * TWInv;
				float tStartY = Chr.textureStart.Y * THInv;
				float tEndX = (Chr.textureStart.X + Chr.textureSize.X) * TWInv;
				float tEndY = (Chr.textureStart.Y + Chr.textureSize.Y) * THInv;
				vertices[vI]=vertices[vI+4]=startX;vertices[vI+2]=vertices[vI+6]=tStartX;
				vertices[vI+1]=vertices[vI+13]=startY;vertices[vI+3]=vertices[vI+15]=tStartY;
				vertices[vI+5]=vertices[vI+9]=endY;vertices[vI+7]=vertices[vI+11]=tEndY;
				vertices[vI+8]=vertices[vI+12]=endX;vertices[vI+10]=vertices[vI+14]=tEndX;
				if (vI == BulkDrawFloats - 16) {
					GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * BulkDrawFloats, vertices);
					GL.DrawElements(PrimitiveType.Triangles, MTILen, DrawElementsType.UnsignedInt, 0);
					vI = 0;} else vI += 16;
				absPosX += (int)(Chr.advance.X * textScaleX); absPosY += (int)(Chr.advance.Y * textScaleY);}
			if (vI != 0) {
				GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * vI, vertices);
				GL.DrawElements(PrimitiveType.Triangles, (vI * 3) >> 3, DrawElementsType.UnsignedInt, 0);}
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);}
		public void AnnouncementsRender(List<Announcement> announcements, Game game, Shader shader, float lineHeight, Vector2i windowSize)
		{
			// Game game, Shader shader, string text, Vector2i posOffset, Vector2 posScale, Vector2 textScale, Vector3 color, float lineHeight, Vector2i windowSize, FontCharacterData fontCharData, bool useSpecialChar = false

			shader.Use();
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			FontCharacterData fontCharData = FontCharFillerThing.FontCharDeeta;
			int vI = 0;
			const float posScaleX = -0.6f;
			const float posScaleY = -0.9f;
			const float textScaleX = 2;
			const float textScaleY = 2;
			for (int index = announcements.Count-1; index > -1; index--){
				Announcement tA = announcements[0];
				shader.SetVector3("textColor", tA.TextColor);

				// Vector2 absPos = new(posScaleX * windowSize.X,posScaleY * windowSize.Y);
				float absPosX = posScaleX * windowSize.X;
				float absPosY = posScaleY * windowSize.Y;
				float spaceSize = textScaleX * 3;
				float tabSize = spaceSize * 4;

				float[] vertices = new float[BulkDrawFloats];

				if (tA.SpecialText) {
					int incorrectSpecialChar = 0;
					for (int i = 0; i < tA.Message.Length; i++) {
						char c = tA.Message[i];
						GlyphData Chr;
						if (incorrectSpecialChar > 0 && --incorrectSpecialChar == 0 && c == '|') continue;
						switch (c) {
							// case ' ': absPos = new(absPos.X + spaceSize, absPos.Y); continue;
							// case '	': absPos = new(absPos.X + tabSize, absPos.Y); continue;
							// case '\n': absPos = new(posScaleX * windowSize.X, absPos.Y - textScaleY * lineHeight); continue;
							case ' ': absPosX += spaceSize; continue;
							case '	': absPosX += tabSize; continue;
							case '\n': absPosX = posScaleX * windowSize.X; absPosY -= textScaleY * lineHeight; continue;
							case '\\':
								int ip1 = i + 1;
								if (ip1 >= tA.Message.Length || tA.Message[ip1] == '\\') { Chr = fontCharData.Chars['\\']; break; } // if this is the last char or the next char is another '\\' then show a '\\' char.
								char nextChar = tA.Message[ip1];
								if (nextChar == '|') { Chr = fontCharData.Chars['\\']; i++; break; } // if the next char is a | char (my format is \| for '\\' chars) then show a '\\', then increment i so the '|' isn't shown.
								if (nextChar == '\n') { i++; goto case '\n'; } // if the line goes to a new line then increment i and do the next line stuff.
								byte IsStacking = 0;
								int j = tA.Message.IndexOfAny(CharSearchThingy, ip1);
								if (j == -1) j = tA.Message.Length; else if (tA.Message[j] != '|') IsStacking = 1;
								int len = j - ip1;
								string s;
								if (len == -1) s = tA.Message[ip1..];
								else s = tA.Message.Substring(ip1, len);
								if (fontCharData.SChars.TryGetValue(s, out Chr)) { i = j - IsStacking; } // if it can find the special character then jump to the index of the last chr in the special character, and the next char will be a new one.
								else { // if it cant find the special character then color the characters red, if there is at least one character.
									shader.SetVector3("textColor", new(.5f, 0, 1f));
									incorrectSpecialChar = len + 1;
									Chr = fontCharData.Chars['\\'];}
								break;
							default: if (!fontCharData.Chars.TryGetValue(c, out Chr)) Chr = fontCharData.Chars['?']; break;}
						float startX = (MathF.Floor(absPosX + Chr.bearing.X * textScaleX) + .5f) / windowSize.X;
						float startY = (MathF.Floor(absPosY + Chr.bearing.Y * textScaleY) + .5f) / windowSize.Y;
						float endX = (MathF.Floor(absPosX + Chr.bearing.X * textScaleX + MathF.Ceiling(Chr.size.X * textScaleX)) + .5f) / windowSize.X;
						float endY = (MathF.Floor(absPosY + Chr.bearing.Y * textScaleY + MathF.Ceiling(Chr.size.Y * textScaleY)) + .5f) / windowSize.Y;

						float tStartX = Chr.textureStart.X / (float)TextTexture.Width;
						float tStartY = Chr.textureStart.Y / (float)TextTexture.Height;
						float tEndX = (Chr.textureStart.X + Chr.textureSize.X) / (float)TextTexture.Width;
						float tEndY = (Chr.textureStart.Y + Chr.textureSize.Y) / (float)TextTexture.Height;
						vertices[vI]=vertices[vI+4]=startX;vertices[vI+1]=vertices[vI+13]=startY;
						vertices[vI+2]=vertices[vI+6]=tStartX;vertices[vI+3]=vertices[vI+15]=tStartY;
						vertices[vI+5]=vertices[vI+9]=endY;vertices[vI+7]=vertices[vI+11]=tEndY;
						vertices[vI+8]=vertices[vI+12]=endX;vertices[vI+10]=vertices[vI+14]=tEndX;
						
						if (vI == BulkDrawFloats - 16) {
							GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * BulkDrawFloats, vertices);
							GL.DrawElements(PrimitiveType.Triangles, MTILen, DrawElementsType.UnsignedInt, 0); vI = 0;} else vI += 16;
						// absPos += new Vector2i((int)(Chr.advance.X * textScaleX), (int)(Chr.advance.Y * textScaleY));}
						absPosX += (int)(Chr.advance.X * textScaleX); absPosY += (int)(Chr.advance.Y * textScaleY);}
				} else {
					for (int i = 0; i < tA.Message.Length; i++) {
						char c = tA.Message[i];
						GlyphData Chr;
						switch (c) {
							case ' ': absPosX += spaceSize; continue;
							case '	': absPosX += tabSize; continue;
							case '\n': absPosX = posScaleX * windowSize.X; absPosY -= textScaleY * lineHeight; continue;
							default: if (!fontCharData.Chars.TryGetValue(c, out Chr)) Chr = fontCharData.Chars['?']; break;}
						float startX = (MathF.Floor(absPosX + Chr.bearing.X * textScaleX) + .5f) / windowSize.X;
						float startY = (MathF.Floor(absPosY + Chr.bearing.Y * textScaleY) + .5f) / windowSize.Y;
						float endX = (MathF.Floor(absPosX + Chr.bearing.X * textScaleX + MathF.Ceiling(Chr.size.X * textScaleX)) + .5f) / windowSize.X;
						float endY = (MathF.Floor(absPosY + Chr.bearing.Y * textScaleY + MathF.Ceiling(Chr.size.Y * textScaleY)) + .5f) / windowSize.Y;

						float tStartX = Chr.textureStart.X / (float)TextTexture.Width;
						float tStartY = Chr.textureStart.Y / (float)TextTexture.Height;
						float tEndX = (Chr.textureStart.X + Chr.textureSize.X) / (float)TextTexture.Width;
						float tEndY = (Chr.textureStart.Y + Chr.textureSize.Y) / (float)TextTexture.Height;

						vertices[vI]=vertices[vI+4]=startX;vertices[vI+1]=vertices[vI+13]=startY;
						vertices[vI+2]=vertices[vI+6]=tStartX;vertices[vI+3]=vertices[vI+15]=tStartY;
						vertices[vI+5]=vertices[vI+9]=endY;vertices[vI+7]=vertices[vI+11]=tEndY;
						vertices[vI+8]=vertices[vI+12]=endX;vertices[vI+10]=vertices[vI+14]=tEndX;
						if (vI == BulkDrawFloats - 16) {
							GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * BulkDrawFloats, vertices);

							GL.DrawElements(PrimitiveType.Triangles, MTILen, DrawElementsType.UnsignedInt, 0);
							vI = 0;} else vI += 16;
						// absPos += new Vector2i((int)(Chr.advance.X * textScaleX), (int)(Chr.advance.Y * textScaleY));}
						absPosX += (int)(Chr.advance.X * textScaleX); absPosY += (int)(Chr.advance.Y * textScaleY);}}
				if (vI != 0) {
					GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * vI, vertices);
					GL.DrawElements(PrimitiveType.Triangles, (vI * 3) >> 3, DrawElementsType.UnsignedInt, 0);}
			}
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
		}
		public int NewTxtThing(TxtOptions O) {
			TextThingies.Add(O);
			return TextThingies.Count - 1;}
		public void Dispose() {
			GL.DeleteBuffer(VBO);
			GL.DeleteBuffer(EBO);
			GL.DeleteVertexArray(VAO);
			// Reset static initialization flag for next instance
			IsInitialized = false;}}}