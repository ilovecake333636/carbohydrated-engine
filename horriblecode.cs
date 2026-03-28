using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace GameEngineThing
{
	public struct GlyphData
	{
		// public Vector2i textureStart;
		// public Vector2i textureSize;
		// public Vector2 bearing;
		// public Vector2 size;
		// public Vector2 advance;
		public float tStartX, tStartY,
		// tSizeX, tSizeY,
		tEndX, tEndY,
		bearingX, bearingY,
		sizeX, sizeY,
		spbX, spbY,
		advanceX, advanceY;
		public GlyphData(GlyphAdvType advType, Vector2 textureStart, Vector2 textureSize, Vector2 adv, Vector2 bearing)
		{
			bearingX = bearing.X; bearingY = bearing.Y;
			sizeX = Math.Abs(textureSize.X); sizeY = Math.Abs(textureSize.Y);
			spbX = sizeX+bearingX; spbY = sizeY+bearingY;
			switch (advType) {
				case GlyphAdvType.SizeXPlusA: advanceX = adv.X + sizeX; advanceY = adv.Y; break;
				case GlyphAdvType.SizeYPlusA: advanceX = adv.X; advanceY = sizeY + adv.Y; break;
				case GlyphAdvType.SizeXYPlusA: advanceX = sizeX + adv.X; advanceY = sizeY + adv.Y; break;
				default: advanceX = adv.X; advanceY = adv.Y; break;
			};
			tStartX = textureStart.X; tStartY = textureStart.Y;
			// tSizeX = textureSize.X; tSizeY = textureSize.Y;
			tEndX = tStartX+textureSize.X; tEndY = tStartY+textureSize.Y;
		}
		public GlyphData(GlyphAdvType advType, Vector2 textureStart, Vector2 textureSize, Vector2 adv)
		{
			bearingX = 0; bearingY = 0;
			sizeX = Math.Abs(textureSize.X); sizeY = Math.Abs(textureSize.Y);
			spbX = sizeX+bearingX; spbY = sizeY+bearingY;
			switch (advType) {
				case GlyphAdvType.SizeXPlusA: advanceX = adv.X + sizeX; advanceY = adv.Y; break;
				case GlyphAdvType.SizeYPlusA: advanceX = adv.X; advanceY = sizeY + adv.Y; break;
				case GlyphAdvType.SizeXYPlusA: advanceX = sizeX + adv.X; advanceY = sizeY + adv.Y; break;
				default: advanceX = adv.X; advanceY = adv.Y; break;
			};
			tStartX = textureStart.X; tStartY = textureStart.Y;
			// tSizeX = textureSize.X; tSizeY = textureSize.Y;
			tEndX = tStartX + textureSize.X; tEndY = tStartY + textureSize.Y;
		}
		public GlyphData(GlyphAdvType advType, float tStX, float tStY, float tSzX, float tSzY, float advX, float advY, float bearingX, float bearingY)
		{
			this.bearingX = bearingX; this.bearingY = bearingY;
			sizeX = Math.Abs(tSzX); sizeY = Math.Abs(tSzY);
			spbX = sizeX+bearingX; spbY = sizeY+bearingY;
			switch (advType) {
				case GlyphAdvType.SizeXPlusA: advanceX = advX + sizeX; advanceY = advY; break;
				case GlyphAdvType.SizeYPlusA: advanceX = advX; advanceY = sizeY + advY; break;
				case GlyphAdvType.SizeXYPlusA: advanceX = sizeX + advX; advanceY = sizeY + advY; break;
				default: advanceX = advX; advanceY = advY; break;
			};
			tStartX = tStX; tStartY = tStY;
			// tSizeX = tSzX; tSizeY = tSzY;
			tEndX = tStX+tSzX; tEndY = tStY+tSzY;
		}
		public GlyphData(GlyphAdvType advType, float tStX, float tStY, float tSzX, float tSzY, float advX, float advY)
		{
			bearingX = bearingY = 0;
			sizeX = spbX = Math.Abs(tSzX); sizeY = spbY = Math.Abs(tSzY);
			switch (advType) {
				case GlyphAdvType.SizeXPlusA: advanceX = advX + sizeX; advanceY = advY; break;
				case GlyphAdvType.SizeYPlusA: advanceX = advX; advanceY = sizeY + advY; break;
				case GlyphAdvType.SizeXYPlusA: advanceX = sizeX + advX; advanceY = sizeY + advY; break;
				default: advanceX = advX; advanceY = advY; break;
			};
			tStartX = tStX; tStartY = tStY;
			// tSizeX = tSzX; tSizeY = tSzY;
			tEndX = tStX+tSzX; tEndY = tStY+tSzY;
		}
		public GlyphData(float sX, float sY, float tStX, float tStY, float tEnX, float tEnY, float advX, float advY, float bX, float bY)
		{
			sizeX = sX; bearingX = bX; spbX = bX+sX;
			sizeY = sY; bearingY = bY; spbY = bY+sY;
			tStartX = tStX; tStartY = tStY;
			tEndX = tEnX; tEndY = tEnY;
			advanceX = advX; advanceY = advY;
		}
	}
	public enum GlyphAdvType
	{
		AdvOnly = 0,
		SizeXPlusA = 1,
		SizeYPlusA = 2,
		SizeXYPlusA = 3,
	}
	public static class FontCharFillerThing
	{
		private static readonly GlyphAdvType XPA = GlyphAdvType.SizeXPlusA;
		// private static readonly Vector2 Adv = new(1f, 0f);
		// private static readonly float AdvX = 1f;
		// private static readonly float AdvY = 0f;
		public static FontCharacterData FontCharDeeta = new();
		static FontCharFillerThing()
		{
			// initialization
			foreach (KeyValuePair<string, string> item in SCharsDuplicates) { SChars[item.Key] = SChars[item.Value]; }

			// now some more things like this
			FillFontChar(FontCharDeeta, 1f/1024, 1f/1024);
		}
		private static readonly Dictionary<char, GlyphData> Chars = new()
		{
			['a'] = new(XPA, 0, 0, 5f, 7f, 1f, 0f),
			['b'] = new(XPA, 7, 0, 4f, 7f, 1f, 0f),
			['c'] = new(XPA, 0, 0, 3f, 4f, 1f, 0f),
			['d'] = new(XPA, 11, 0, -4f, 7f, 1f, 0f),
			['e'] = new(XPA, 11, 0, 4f, 5f, 1f, 0f),
			['f'] = new(XPA, 16, 0, 4f, 7f, 1f, 0f),
			['g'] = new(XPA, 20, 0, 4f, 7f, 1f, 0f, 0f, -3f),
			['h'] = new(XPA, 24, 0, 4f, 7f, 1f, 0f),
			['i'] = new(XPA, 31, 1, 1f, 6f, 1f, 0f),
			['j'] = new(XPA, 28, 0, 4f, 7f, 1f, 0f, 0f, -3f),
			['k'] = new(XPA, 32, 0, 3f, 7f, 1f, 0f),
			['l'] = new(XPA, 35, 0, 2f, 7f, 1f, 0f),
			['m'] = new(XPA, 37, 0, 5f, 4f, 1f, 0f),
			['n'] = new(XPA, 42, 0, 4f, 4f, 1f, 0f),
			['o'] = new(XPA, 46, 0, 4f, 4f, 1f, 0f),
			['p'] = new(XPA, 7, 7, 4f, -7f, 1f, 0f, 0f, -3f),
			['q'] = new(XPA, 11, 7, -6f, -7f, 1f, 0f, 0f, -3f),
			['r'] = new(XPA, 50, 0, 4f, 4f, 1f, 0f),
			['s'] = new(XPA, 54, 0, 4f, 5f, 1f, 0f),
			['t'] = new(XPA, 58, 0, 3f, 7f, 1f, 0f),
			['u'] = new(XPA, 61, 0, 4f, 4f, 1f, 0f),
			['v'] = new(XPA, 49, 3, 3f, 3f, 1f, 0f),
			['w'] = new(XPA, 49, 3, 5f, 3f, 1f, 0f),
			['x'] = new(XPA, 51, 2, 3f, 3f, 1f, 0f),
			['y'] = new(XPA, 65, 0, 4f, 5f, 1f, 0f, 0f, -3f),
			['z'] = new(XPA, 69, 0, 4f, 5f, 1f, 0f),

			['A'] = new(XPA, 0, 7, 4f, 7f, 1f, 0f),
			['B'] = new(XPA, 11, 7, 4f, 7f, 1f, 0f),
			['C'] = new(XPA, 4, 7, 4f, 7f, 1f, 0f),
			['D'] = new(XPA, 12, 7, -4f, 7f, 1f, 0f),
			['E'] = new(XPA, 18, 7, 4f, 7f, 1f, 0f),
			['F'] = new(XPA, 19, 7, -4f, 7f, 1f, 0f),
			['G'] = new(XPA, 22, 7, 5f, 7f, 1f, 0f),
			['H'] = new(XPA, 27, 7, 5f, 7f, 1f, 0f),
			['I'] = new(XPA, 32, 7, 4f, 7f, 1f, 0f),
			['J'] = new(XPA, 35, 7, 6f, 7f, 1f, 0f, 0f, -1f),
			['K'] = new(XPA, 52, 7, 5f, 7f, 1f, 0f),
			['L'] = new(XPA, 43, 14, -4f, -7f, 1f, 0f),
			['M'] = new(XPA, 42, 7, 7f, 7f, 1f, 0f),
			['N'] = new(XPA, 48, 7, 5f, 7f, 1f, 0f),
			['O'] = new(XPA, 4, 7, 5f, 7f, 1f, 0f),
			['P'] = new(XPA, 7, 7, 4f, -7f, 1f, 0f),
			['Q'] = new(XPA, 57, 7, 7f, 7f, 1f, 0f),
			['R'] = new(XPA, 64, 7, 5f, 7f, 1f, 0f),
			['S'] = new(XPA, 68, 7, 5f, 7f, 1f, 0f),
			['T'] = new(XPA, 9, 20, 5f, -7f, 1f, 0f),
			['U'] = new(XPA, 73, 7, 5f, 7f, 1f, 0f),
			['V'] = new(XPA, 78, 7, 5f, 7f, 1f, 0f),
			['W'] = new(XPA, 42, 14, 7f, -7f, 1f, 0f),
			['X'] = new(XPA, 83, 7, 5f, 7f, 1f, 0f),
			['Y'] = new(XPA, 88, 7, 5f, 7f, 1f, 0f),
			['Z'] = new(XPA, 93, 7, 5f, 7f, 1f, 0f),

			// [' '] = new(XPA, Vec2Z,    new(1f, 0f), new(0, 0), new(0, 0)),
			['?'] = new(XPA, 73, 0, 2f, 6f, 1f, 0f),
			['!'] = new(XPA, 31, 7, 1f, -6f, 1f, 0f),

			['0'] = new(XPA, 100, 0, 4f, 7f, 1f, 0f), //[Vec2Z,new(2f, 0f),new(103,0),new(4,7)]
			['1'] = new(XPA, 75, 0, 4f, 7f, 1f, 0f),
			['2'] = new(XPA, 79, 0, 4f, 7f, 1f, 0f),
			['3'] = new(XPA, 83, 0, 4f, 7f, 1f, 0f),
			['4'] = new(XPA, 90, 0, 4f, 7f, 1f, 0f),
			['5'] = new(XPA, 96, 0, 4f, 7f, 1f, 0f),
			['6'] = new(XPA, 106, 0, 4f, 7f, 1f, 0f),
			['7'] = new(XPA, 110, 0, 4f, 7f, 1f, 0f),
			['8'] = new(XPA, 86, 0, 4f, 7f, 1f, 0f),
			['9'] = new(XPA, 114, 0, 4f, 7f, 1f, 0f),
			['.'] = new(XPA, 0, 1, 1f, 1f, 1f, 0f),
			[','] = new(XPA, 5, 0, -2f, 3f, 1f, 0f, 0, -2),
			['<'] = new(XPA, 53, 8, 3f, 5f, 1f, 0f),
			['>'] = new(XPA, 56, 8, -3f, 5f, 1f, 0f),
			['/'] = new(XPA, 111, 0, 3f, 6f, 1f, 0f),
			[':'] = new(XPA, 0, 5, 1f, 3f, 1f, 0f, 0, 2),
			[';'] = new(XPA, 4, 9, 2f, -4f, 1f, 0f),
			['\''] = new(XPA, 0, 1, 1f, 2f, 1f, 0f),
			['"'] = new(XPA, 49, 4, 3f, 2f, 1f, 0f),
			['\\'] = new(XPA, 114, 0, -3f, 6f, 1f, 0f),
			['|'] = new(XPA, 7, 0, 1f, 8f, 1f, 0f, 0f, -.5f),
			['['] = new(XPA, 93, 1, 3f, 7f, 1f, 0f),
			[']'] = new(XPA, 96, 1, -3f, 7f, 1f, 0f),
			['{'] = new(XPA, 118, 0, 3f, 7f, 1f, 0f),
			['}'] = new(XPA, 121, 0, -3f, 7f, 1f, 0f),
			['-'] = new(XPA, 1, 0, 2f, 1f, 1f, 0f, 0f, 4f),
			['_'] = new(XPA, 0, 10, 4f, 1f, 1f, 0f),
			['+'] = new(XPA, 17, 9, 3f, 3f, 1f, 0f, 0f, 2f),
			['='] = new(XPA, 66, 0, 2f, 3f, 1f, 0f),
			['@'] = new(XPA, 121, 0, 8f, 8f, 1f, 0f),
			['#'] = new(XPA, 129, 0, 5f, 5f, 1f, 0f),
			['$'] = new(XPA, 134, 0, 5f, 9f, 1f, 0f),//[Vec2Z,new(2f,0f),new(143,0),new(7,11)],[Vec2Z,new(2f, 0f),new(143,0),new(-5,9)]
			['%'] = new(XPA, 150, 0, 7f, 7f, 1f, 0f),
			['^'] = new(XPA, 0, 2, 3f, 2f, 1f, 0f, 0f, 4f),
			['&'] = new(XPA, 157, 0, 7f, 9f, 1f, 0f),
			['*'] = new(XPA, 164, 0, 4f, 3f, 1f, 0f),
			['('] = new(XPA, 4, 7, 2f, 7f, 1f, 0f),
			[')'] = new(XPA, 6, 7, -2f, 7f, 1f, 0f),
			['`'] = new(XPA, 49, 3, 2f, 2f, 1f, 0f),
			['~'] = new(XPA, 50, 3, 4f, 2f, 1f, 0f),

			['零'] = new(XPA, 512, 31, 11f, 18f, 1f, 0f),
			['一'] = new(XPA, 512, 20, 7f, 1f, 1f, 0f, 0f, 3f),
			['二'] = new(XPA, 512, 20, 7f, 3f, 1f, 0f, 0f, 2f),
			['三'] = new(XPA, 512, 16, 7f, 5f, 1f, 0f, 0f, 1f),
			['四'] = new(XPA, 512, 10, 7f, 7f, 1f, 0f),
			['五'] = new(XPA, 512, 4, 7f, 7f, 1f, 0f),
			['六'] = new(XPA, 512 + 6, 1, 7f, 7f, 1f, 0f),
			['七'] = new(XPA, 512 + 7, 8, 7f, 7f, 1f, 0f),
			['八'] = new(XPA, 512 + 13, 9, 7f, 7f, 1f, 0f, 1f, 2f),
			['九'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['十'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['百'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['千'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['万'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['亿'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['白'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['上'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['下'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['中'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['国'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['文'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['口'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['回'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['日'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['自'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['己'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['早'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['午'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['晚'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['凌'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['晨'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['我'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['的'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['你'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['他'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['她'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
			['它'] = new(XPA, 512, 0, 7f, 7f, 1f, 0f),
		};
		private static readonly Dictionary<string, GlyphData> SChars = new()
		{
			["blinker"] = new(XPA, 164, 3, 4f, 7f, 1f, 0f),
			["loaf"] = new(XPA, 256, 10, 11f, 10f, 1f, 0f),
			["loaf2"] = new(XPA, 256, 11, 11f, -11f, 1f, 0f),
			["loafBIG"] = new(XPA, 295, 0, 46f, 42f, 1f, 0f),
			["loafLegacy"] = new(XPA, 267, 0, 11f, 9f, 1f, 0f),
			["loafLegacyNoR"] = new(XPA, 277, 0, 10f, 9f, 2f, 0f),
			["loafLegacyNoL"] = new(XPA, 287, 0, -10f, 9f, 1f, 0f, 1f, 0f),
			["loafLegacyNoLR"] = new(XPA, 286, 0, 9f, 8f, 2f, 0f, 1f, 0f),
			["unknown"] = new(XPA, 0, 32, 16f, 16f, 0, 1),
			["note"] = new(XPA, 16, 32, 16f, 16f, 0, 1),
		};
		private static readonly Dictionary<string, string> SCharsDuplicates = new()
		{
			["loafLegacyNoRL"] = "loafLegacyNoLR",
			["loafLegacyNoEars"] = "loafLegacyNoLR",
		};
		public static void FillFontChar(FontCharacterData FontChrDeeta)
		{
			foreach (KeyValuePair<char, GlyphData> Deeta in Chars) FontChrDeeta.Chars[Deeta.Key] = Deeta.Value;
			foreach (KeyValuePair<string, GlyphData> Deeta in SChars) FontChrDeeta.SChars[Deeta.Key] = Deeta.Value;
		}
		public static void FillFontChar(FontCharacterData FontChrDeeta, float scaleX, float scaleY)
		{
			GlyphData d;
			foreach (KeyValuePair<char, GlyphData> Deeta in Chars) {
				d = Deeta.Value;
				FontChrDeeta.Chars[Deeta.Key] = new(d.sizeX,d.sizeY,d.tStartX*scaleX,d.tStartY*scaleY,d.tEndX*scaleX,d.tEndY*scaleY,d.advanceX,d.advanceY,d.bearingX,d.bearingY);
			}
			foreach (KeyValuePair<string, GlyphData> Deeta in SChars) {
				d = Deeta.Value;
				FontChrDeeta.SChars[Deeta.Key] = new(d.sizeX,d.sizeY,d.tStartX*scaleX,d.tStartY*scaleY,d.tEndX*scaleX,d.tEndY*scaleY,d.advanceX,d.advanceY,d.bearingX,d.bearingY);
			}
		}
	}
	/// <summary>
	/// this is probably not the correct way to store this data but idk what the correct way is. :p
	/// </summary>
	public static class DataStuff
	{
        public const float D2RConst = (float)(Math.PI / 180d);
        public const float R2DConst = (float)(180d / Math.PI);
		public static readonly float[] CubeV = [
		//   positions            texture coords
			// first 3 faces            btm -> bottom, top -> top, r -> right, l -> left, bk -> back, fr -> front
			 .5f, -.5f, -.5f,   0f, 0f,  //  btm - r - bk
			 .5f, -.5f,  .5f,   0f, 1f,  //  top - r - bk
			-.5f, -.5f, -.5f,   1f, 0f,  //  btm - l - bk
			-.5f, -.5f,  .5f,   1f, 1f,  //  top - l - bk

			-.5f,  .5f, -.5f,   2f, 0f,  //  btm - l - fr
			-.5f,  .5f,  .5f,   2f, 1f,  //  top - l - fr
			 .5f,  .5f, -.5f,   3f, 0f,  //  btm - r - fr
			 .5f,  .5f,  .5f,   3f, 1f,  //  top - r - fr

			// second 3 faces
			-.5f, -.5f, -.5f,   0f, 0f,  //  btm - l - bk
			-.5f,  .5f, -.5f,   0f, 1f,  //  btm - l - fr
			 .5f, -.5f, -.5f,   1f, 0f,  //  btm - r - bk
			 .5f,  .5f, -.5f,   1f, 1f,  //  btm - r - fr
			
			 .5f, -.5f,  .5f,   2f, 0f,  //  top - r - bk
			 .5f,  .5f,  .5f,   2f, 1f,  //  top - r - fr
			-.5f, -.5f,  .5f,   3f, 0f,  //  top - l - bk
			-.5f,  .5f,  .5f,   3f, 1f,  //  top - l - fr
		];
		public static readonly uint[] CubeI = [ // 0, 1, 2,  5, 6, 7...
		   0,1,2, 1,2,3,
		   2,3,4, 3,4,5,
		   4,5,6, 5,6,7,

		   8,9,10, 9,10,11,
		   10,11,12, 11,12,13,
		   12,13,14, 13,14,15,
		];
		public static readonly float[] TetrahedronV = [
			 MathF.Sqrt(8f/9f), -1f/3f,  0f,                 0f,  0f, // vertex 0
			-MathF.Sqrt(2f/9f), -1f/3f,  MathF.Sqrt(2f/3f),  0f,  1f, // vertex 1
			-MathF.Sqrt(2f/9f), -1f/3f, -MathF.Sqrt(2f/3f), .5f,  1f, // vertex 2
			 0f,                 1f,     0f,                .5f, .5f, // vertex 3
		];
		public static readonly uint[] TetrahedronI = [0, 1, 2, 0, 1, 3, 0, 2, 3, 1, 2, 3];
		public static readonly float[] PlrTorsoV = [
			-.5f, -.5f, -.5f,   60/2048f, 128/2048f, -.5f, -.3f, -.5f,   64/2048f, 128/2048f,
			 .5f, -.5f, -.5f,   60/2048f, 148/2048f,  .5f, -.3f, -.5f,   64/2048f, 148/2048f,
			 .5f, -.5f,  .5f,   60/2048f, 128/2048f,  .5f, -.3f,  .5f,   64/2048f, 128/2048f,
			-.5f, -.5f,  .5f,   60/2048f, 148/2048f, -.5f, -.3f,  .5f,   64/2048f, 148/2048f,

			-.5f, -.5f, -.5f,   40/2048f, 128/2048f,  .5f, -.5f, -.5f,   60/2048f, 128/2048f,
			 .5f, -.5f,  .5f,   60/2048f, 148/2048f, -.5f, -.5f,  .5f,   40/2048f, 148/2048f,
			-.5f, -.3f, -.5f,   40/2048f, 128/2048f,  .5f, -.3f, -.5f,   60/2048f, 128/2048f,
			 .5f, -.3f,  .5f,   60/2048f, 148/2048f, -.5f, -.3f,  .5f,   40/2048f, 148/2048f,


			-.5f,  .5f, -.5f,   60/2048f, 128/2048f, -.5f,  .3f, -.5f,   64/2048f, 128/2048f,
			 .5f,  .5f, -.5f,   60/2048f, 148/2048f,  .5f,  .3f, -.5f,   64/2048f, 148/2048f,
			 .5f,  .5f,  .5f,   60/2048f, 128/2048f,  .5f,  .3f,  .5f,   64/2048f, 128/2048f,
			-.5f,  .5f,  .5f,   60/2048f, 148/2048f, -.5f,  .3f,  .5f,   64/2048f, 148/2048f,

			-.5f,  .5f, -.5f,   40/2048f, 128/2048f,  .5f,  .5f, -.5f,   60/2048f, 128/2048f,
			 .5f,  .5f,  .5f,   60/2048f, 148/2048f, -.5f,  .5f,  .5f,   40/2048f, 148/2048f,
			-.5f,  .3f, -.5f,   40/2048f, 128/2048f,  .5f,  .3f, -.5f,   60/2048f, 128/2048f,
			 .5f,  .3f,  .5f,   60/2048f, 148/2048f, -.5f,  .3f,  .5f,   40/2048f, 148/2048f,


			-.375f, -.35f, -.375f,   64/2048f, 128/2048f, -.375f,  .35f, -.375f,   64/2048f, 142/2048f,
			 .375f, -.35f, -.375f,   79/2048f, 128/2048f,  .375f,  .35f, -.375f,   79/2048f, 142/2048f,
			 .375f, -.35f,  .375f,   64/2048f, 128/2048f,  .375f,  .35f,  .375f,   64/2048f, 142/2048f,
			-.375f, -.35f,  .375f,   79/2048f, 128/2048f, -.375f,  .35f,  .375f,   79/2048f, 142/2048f,
		];
		public static readonly uint[] PlrTorsoI = [
			0,1,2, 1,2,3,  2,3,4, 3,4,5,  4,5,6, 5,6,7,  6,7,0, 7,0,1,
			8,9,10, 8,10,11,  12,13,14, 12,14,15,
			16,17,18, 17,18,19,  18,19,20, 19,20,21,  20,21,22, 21,22,23,  22,23,16, 23,16,17,
			24,25,26, 24,26,27,  28,29,30, 28,30,31,
			32,33,34, 33,34,35,  34,35,36, 35,36,37,  36,37,38, 37,38,39,  38,39,32, 39,32,33,
		];
		public static readonly float[] PlrArmV = [
			-.25f,  .2f, -.25f,   40/2048f, 188/2048f,   -.25f, -1f, -.25f,   40/2048f, 148/2048f,
			 .25f,  .2f, -.25f,   60/2048f, 188/2048f,    .25f, -1f, -.25f,   60/2048f, 148/2048f,
			 .25f,  .2f,  .25f,   40/2048f, 188/2048f,    .25f, -1f,  .25f,   40/2048f, 148/2048f,
			-.25f,  .2f,  .25f,    0/2048f, 188/2048f,   -.25f, -1f,  .25f,    0/2048f, 148/2048f,
			-.25f,  .2f,  .25f,   60/2048f, 188/2048f,   -.25f, -1f,  .25f,   60/2048f, 148/2048f,

			-.25f,  .2f, -.25f,   20/2048f, 128/2048f,    .25f,  .2f, -.25f,   40/2048f, 128/2048f,
			 .25f,  .2f,  .25f,   40/2048f, 148/2048f,   -.25f,  .2f,  .25f,   20/2048f, 148/2048f,
			-.25f, -1f, -.25f,    0/2048f, 128/2048f,    .25f, -1f, -.25f,   20/2048f, 128/2048f,
			 .25f, -1f,  .25f,   20/2048f, 148/2048f,   -.25f, -1f,  .25f,    0/2048f, 148/2048f,
		];
		public static readonly uint[] PlrArmI = [0, 1, 2, 1, 2, 3, 2, 3, 4, 3, 4, 5, 4, 5, 6, 5, 6, 7, 8, 9, 0, 9, 0, 1, 10, 11, 12, 10, 12, 13, 14, 15, 16, 14, 16, 17,];
		public static readonly float[] PlrLegV = [
			-.25f,  .3f, -.25f,   40/2048f, 188/2048f,   -.25f, -1f, -.25f,   40/2048f, 148/2048f,
			 .25f,  .3f, -.25f,   60/2048f, 188/2048f,    .25f, -1f, -.25f,   60/2048f, 148/2048f,
			 .25f,  .3f,  .25f,   40/2048f, 188/2048f,    .25f, -1f,  .25f,   40/2048f, 148/2048f,
			-.25f,  .3f,  .25f,    0/2048f, 188/2048f,   -.25f, -1f,  .25f,    0/2048f, 148/2048f,
			-.25f,  .3f,  .25f,   60/2048f, 188/2048f,   -.25f, -1f,  .25f,   60/2048f, 148/2048f,

			-.25f,  .3f, -.25f,   20/2048f, 128/2048f,    .25f,  .3f, -.25f,   40/2048f, 128/2048f,
			 .25f,  .3f,  .25f,   40/2048f, 148/2048f,   -.25f,  .3f,  .25f,   20/2048f, 148/2048f,
			-.25f, -1f, -.25f,    0/2048f, 128/2048f,    .25f, -1f, -.25f,   20/2048f, 128/2048f,
			 .25f, -1f,  .25f,   20/2048f, 148/2048f,   -.25f, -1f,  .25f,    0/2048f, 148/2048f,
		];
		public static readonly uint[] PlrLegI = [0, 1, 2, 1, 2, 3, 2, 3, 4, 3, 4, 5, 4, 5, 6, 5, 6, 7, 8, 9, 0, 9, 0, 1, 10, 11, 12, 10, 12, 13, 14, 15, 16, 14, 16, 17,];
		/// <summary>
		/// basically just (float)(.7-Math.Sqrt(.286443))
		/// </summary>
		private const float PlrEarW = .164796300461216916300019080474685438614673176572592385930755643349352698f;
		// private static readonly float PlrEarC = (float)(.7-Math.Sqrt(.07161075));
		private static readonly float PlrEarC = (float)(.7 - Math.Sqrt(.031827));
		public static readonly float[] PlrHeadV = [ // loaf
			// =====Loaf's Crust=====
			// 0-3 bottom
			-.7f,-.5f,-1f,        393f/2048f, 15f/2048f,
			 .7f,-.5f,-1f,        393f/2048f, 15f/2048f,
			 .7f,-.5f, 1f,        393f/2048f, 15f/2048f,
			-.7f,-.5f, 1f,        393f/2048f, 15f/2048f,
			// 4-9 right ear (side)
			-PlrEarW, .5f,-1f,     393f/2048f, 15f/2048f,
			-PlrEarW, .5f, 1f,     393f/2048f, 15f/2048f,
			-.7f,  .809f,-1f,     393f/2048f, 15f/2048f, // y is .5f ± .309f
			-.7f,  .809f, 1f,     393f/2048f, 15f/2048f,
			-.7f,  .191f,-1f,     393f/2048f, 15f/2048f,
			-.7f,  .191f, 1f,     393f/2048f, 15f/2048f,
			// 10-12 right ear (back)
			-PlrEarW,  .5f,-1f,    392f/2048f, 22f/2048f,
			-.7f,   .809f,-1f,    392f/2048f, 22f/2048f,
			-.7f,   .191f,-1f,    392f/2048f, 22f/2048f,
			// 13-15 right ear (front)
			-PlrEarW,  .5f, 1f,    393f/2048f, 15f/2048f,
			-.7f,   .809f, 1f,    393f/2048f, 15f/2048f,
			-.7f,   .191f, 1f,    393f/2048f, 15f/2048f,
				
			// 16-21 left ear (side)
			 PlrEarW, .5f,-1f,     393f/2048f, 15f/2048f,
			 PlrEarW, .5f, 1f,     393f/2048f, 15f/2048f,
			 .7f,   .809f,-1f,     393f/2048f, 15f/2048f,
			 .7f,   .809f, 1f,     393f/2048f, 15f/2048f,
			 .7f,   .191f,-1f,     393f/2048f, 15f/2048f,
			 .7f,   .191f, 1f,     393f/2048f, 15f/2048f,
			// 22-24 left ear (back)
			 PlrEarW,  .5f,-1f,     392f/2048f, 22f/2048f,
			 .7f,   .809f,-1f,     392f/2048f, 22f/2048f,
			 .7f,   .191f,-1f,     392f/2048f, 22f/2048f,
			// 25-27 left ear (front)
			 PlrEarW,  .5f, 1f,     393f/2048f, 15f/2048f,
			 .7f,   .809f, 1f,     393f/2048f, 15f/2048f,
			 .7f,   .191f, 1f,     393f/2048f, 15f/2048f,

			// =======================================
			// actual textured (not crust) part next!!
			// =======================================

			// 28-31 bottom
			-.7f,-.5f,-1f,        210f/2048f, 128f/2048f,
			 .7f,-.5f,-1f,        154f/2048f, 128f/2048f,
			 .7f,-.5f, 1f,        384f/2048f, 126f/2048f,
			-.7f,-.5f, 1f,        566f/2048f, 126f/2048f,
			// 32-35 right ear-ish
			-PlrEarW, .5f,-1f,     (182f+40f*PlrEarW)/2048f, 168f/2048f,
			-PlrEarW, .5f, 1f,     (475f+130f*PlrEarW)/2048f, 256f/2048f,
			-.7f,  .191f,-1f,     210f/2048f, 155.64f/2048f,
			// -.7f,  .191f, 1f,     566f/2048f, 155.64f/2048f,
			-.7f,  .191f, 1f,     566f/2048f, 215.83f/2048f,
			// 36-39 left ear-ish
			 PlrEarW, .5f,-1f,     (182f-40f*PlrEarW)/2048f, 168f/2048f,
			 PlrEarW, .5f, 1f,     (475f-130f*PlrEarW)/2048f, 256f/2048f,
			 .7f,  .191f,-1f,     154f/2048f, 155.64f/2048f,
			//  .7f,  .191f, 1f,     384f/2048f, 155.64f/2048f,
			 .7f,  .191f, 1f,     384f/2048f, 215.83f/2048f,
			
			// 40-43 ear centers
			-PlrEarC, .5f,-1f,     392f/2048f, 15f/2048f, // right-back
			-PlrEarC, .5f, .8f, /*the ear is watching*/     393f/2048f, 22f/2048f, // right-front
			PlrEarC, .5f,-1f,     392f/2048f, 15f/2048f, // left-back
			PlrEarC, .5f, .8f,     393f/2048f, 22f/2048f, // left-front
		];
		public static readonly uint[] PlrHeadI = [
			0,1,2, 0,2,3,    // bottom of the loaf
			// sides:
			0,3,8, 3,8,9,    // right
			1,2,20, 2,20,21, // left
			4,5,16, 5,16,17, // top
			28,29,34, 29,34,38, // back
			34,38,32, 38,32,36, // back
			30,31,39, 31,39,35, // front
			39,35,33, 39,33,37, // front
			//=======ears======//
			// right ear:
			4,5,6, 5,6,7,                 // side
			6,7,8, 7,8,9,                 // side
			10,11,40, 11,12,40, 12,10,40, // back
			13,14,41, 14,15,41, 15,13,41, // front
			// left ear:
			16,17,18, 17,18,19,           // side
			18,19,20, 19,20,21,           // side
			22,23,42, 23,24,42, 24,22,42, // back
			25,26,43, 26,27,43, 27,25,43, // front
			//=======/ears======//
		];
		public static readonly float[] PlaneV = [
			-1f, -1f, 0f, 385f, 15f,
			1f, -1f, 0f, 385f, 15f,
			-1f, 1f, 0f, 385f, 15f,
			1f, 1f, 0f, 385f, 15f,
		];
		public static readonly uint[] PlaneI = [0, 1, 2, 1, 2, 3];
		// public static readonly float[] v = [];
		// public static readonly uint[] i = [];
		// public static readonly float[] v = [];
		// public static readonly uint[] i = [];
		public static (float, float, float) HSVToRGB(float h, float sv, float v)
		{
			sv *= v;
			// 0 is red, 1/3 is green, 2/3 is blue, in-betweens are a mix and values where 2/3<h<1 it is a mix of red and blue.
			// 1/6 is yellow, 1/2 is cyan, 5/6 is magenta.
			float r = Math.Abs((h *= 6) - 3); // raw red; before processing.
			float g = Math.Abs(h - 2); // raw green; before processing.
			float b = Math.Abs(h - 4); // raw blue; before processing.

			return (r switch { < 2 and > 1 => v + (r - 2) * sv, < 2 => v - sv, _ => v },
			g switch { < 2 and > 1 => v + (1 - g) * sv, < 2 => v, _ => v - sv },
			b switch { < 2 and > 1 => v + (1 - b) * sv, < 2 => v, _ => v - sv }); // good luck trying to figure this out lol. also idk it this even is optimized..
		}
		public static Vector3 HueToRGB(float hue)
		{
			// 0 is red, 1/3 is green, 2/3 is blue, in-betweens are a mix and values where 2/3<h<1 it is a mix of red and blue.
			// 1/6 is yellow, 1/2 is cyan, 5/6 is magenta.
			hue *= 6;
			(float rr, float rg, float rb) = (Math.Abs(hue - 3), Math.Abs(hue - 2), Math.Abs(hue - 4)); // raw red, raw green, raw blue
			return new(
				(rr < 2) ? ((rr > 1) ? (rr - 1) : 0) : 1,
				(rg < 2) ? ((rg > 1) ? (2 - rg) : 1) : 0,
				(rb < 2) ? ((rb > 1) ? (2 - rb) : 1) : 0);
		}
		// public static (float, float, float) HueToRGB2(float hue)
		// {
		// 	// 0 is red, 1/3 is green, 2/3 is blue, in-betweens are a mix and values where 2/3<h<1 it is a mix of red and blue.
		// 	// 1/6 is yellow, 1/2 is cyan, 5/6 is magenta.
		// 	hue *= 6;
		// 	(float rr, float rg, float rb) = (Math.Abs(hue-3),Math.Abs(hue-2),Math.Abs(hue-4)); // raw red, raw green, raw blue
		// 	return (
		// 		(rr < 2) ? ((rr > 1) ? (rr - 1) : 0) : 1,
		// 		(rg < 2) ? ((rg > 1) ? (2 - rg) : 1) : 0,
		// 		(rb < 2) ? ((rb > 1) ? (2 - rb) : 1) : 0);
		// }
		public static Matrix4 CreateRotationXYZ(Vector3 input)
		{
			// Matrix4 result;
			(float x, float y, float z) = input;
			float num = MathF.Cos(x),
			num2 = MathF.Sin(x),
			num3 = MathF.Cos(y),
			num4 = MathF.Sin(y),
			num5 = MathF.Cos(z),
			num6 = MathF.Sin(z);
			// Matrix4 result = new(1, 0, 0, 0,
			// 0, num, num2, 0,
			// 0, -num2, num, 0,
			// 0, 0, 0, 1);
			// Matrix4 result = new(num3,0,-num4,0,
			// num2 * num4,num,num2 * num3,0,
			// num * num4,-num2,num * num3,0,
			// 0,0,0,1);
			float x2 = num2 * num4, x3 = num * num4;
			// Matrix4 result = new(num3 * num5,num3 * num6,-num4,0,
			// x2 * num5 - num * num6,x2 * num6 + num * num5,num2*num3,0,
			// x3 * num5 + num2 * num6,x3 * num6 - num2 * num5,num*num3,0,
			// 0,0,0,1);

			// result *= new Matrix4(num3, 0, -num4, 0,
			// 0, 1, 0, 0,
			// num4, 0, num3, 0,
			// 0, 0, 0, 1);
			// result *= new Matrix4(num5, num6, 0, 0,
			// -num6, num5, 0, 0,
			// 0, 0, 1, 0,
			// 0, 0, 0, 1);
			// return result;
			return new(num3*num5,num3*num6,-num4,0,
			x2*num5-num*num6,x2*num6+num*num5,num2*num3,0,
			x3*num5+num2*num6,x3*num6-num2*num5,num*num3,0,
			0,0,0,1);
		}
		public static Matrix4 CreateRotationXYZ(float x, float y, float z)
		{
			float num = MathF.Cos(x),
			num2 = MathF.Sin(x),
			num3 = MathF.Cos(y),
			num4 = MathF.Sin(y),
			num5 = MathF.Cos(z),
			num6 = MathF.Sin(z);
			float x2 = num2 * num4, x3 = num * num4;
			return new(num3*num5,num3*num6,-num4,0,
			x2*num5-num*num6,x2*num6+num*num5,num2*num3,0,
			x3*num5+num2*num6,x3*num6-num2*num5,num*num3,0,
			0,0,0,1);
		}
		public static Dictionary<string, Action<Game, string>> chatCommands = [];
		public static Dictionary<string, Action<Game>> noInputChatCommands = [];
		/// <summary>
		/// this also includes invalid ones i think
		/// </summary>
		public static readonly List<Type> EverySingleMinigame = [];
		public static readonly List<(Type, string)> AllMinigames = [];
		public static readonly Dictionary<string, Action<Game>> MinigameInitializers = [];
		// public static Dictionary<Type, string> MinigameIdentifiers = [];
		static DataStuff() {
			noInputChatCommands["exit"] = noInputChatCommands["quit"] = noInputChatCommands["cabbage"] = delegate (Game game) {
				game.WillReopen = false; game.Close(); };
			noInputChatCommands["pong"] = noInputChatCommands["snake"] = noInputChatCommands["fnf"] = delegate (Game game) {
				game.ReopenData = game._chattingText;
				game.WillReopen = true;
				game.Close(); };
			noInputChatCommands["debugtxt"] = noInputChatCommands["debugtext"] = noInputChatCommands["dbtxt"] = delegate (Game game) {
				Console.WriteLine("debug txt entered debugging thing idk\nPrevious thing: " + game._debugFlags.HasFlag(DebugFlags.debugText));
				// if (game._debugFlags.HasFlag(DebugFlags.debugText))
				// 	game._debugFlags &= ~DebugFlags.debugText; else game._debugFlags |= DebugFlags.debugText;
					game._debugFlags ^= DebugFlags.debugText;
				Console.WriteLine("Now: " + game._debugFlags.HasFlag(DebugFlags.debugText)); };
			noInputChatCommands["debuglog"] = delegate (Game game) {
				Console.WriteLine("debug logging entered debugging thing idk\nPrevious: " + game._debugFlags.HasFlag(DebugFlags.debugLogging));
				// if (game._debugFlags.HasFlag(DebugFlags.debugLogging))
				// 	game._debugFlags &= ~DebugFlags.debugLogging; else game._debugFlags |= DebugFlags.debugLogging;
				game._debugFlags ^= DebugFlags.debugLogging;
				Console.WriteLine("Now: " + game._debugFlags.HasFlag(DebugFlags.debugText)); };
			noInputChatCommands["showvsync"] = delegate (Game game) {
				Console.WriteLine("Vsync mode right now: " + game.VSync); };
			noInputChatCommands["vsyncon"] = delegate (Game game) { game.VSync = VSyncMode.On; };
			noInputChatCommands["vsyncoff"] = delegate (Game game) { game.VSync = VSyncMode.Off; };
			noInputChatCommands["vsyncadapt"] = delegate (Game game) { game.VSync = VSyncMode.Adaptive; };
			noInputChatCommands["stoprecording"] = delegate (Game game) {
				Console.WriteLine("Stopping recording hopefully."); game.StopRecording(); Console.WriteLine("Stopped recording hopefully..."); };
			noInputChatCommands["reopen"] = delegate (Game game) {  game.WillReopen = true; game.Close(); };
			noInputChatCommands["playerrendertoggle"] = delegate (Game game) { game.renderPlayer = !game.renderPlayer; };

			chatCommands["reopen"] = delegate (Game game, string str) {
				game.WillReopen = true;
				if (str.Length > 1 && str[0] == ' ') {
					game.ReopenData = str[1..];
					Console.WriteLine("ReopenData: \"" + game.ReopenData + "\""); } game.Close(); };
			chatCommands["help"] = delegate (Game game, string str) {
				switch (str) {
					case " record":
						Console.WriteLine("""
chatCommands["record "] = delegate (Game game, string str) { game.StartRecording(str); Console.WriteLine("Recording with file path " + str); };
chatCommands["record_"] = delegate (Game game, string str) {
	int breakcharpos = str.IndexOf(',');
	if (breakcharpos == -1) { Console.WriteLine("file path never specified..."); return; }
	int fps = Convert.ToInt32(str[..breakcharpos++]);
	game.StartRecording(str[breakcharpos..], fps: fps); Console.WriteLine("Recording at " + fps + " fps with file path " + str[breakcharpos..]);
};
chatCommands["recordf"] = chatCommands["records"] = delegate (Game game, string str) {
	int breakcharpos1 = str.IndexOf(',');
	if (breakcharpos1 == -1) { Console.WriteLine("speed + file location never specified..."); return; }
	int inputfps = Convert.ToInt32(str[..breakcharpos1++]);
	int breakcharpos2 = str.IndexOf(',', breakcharpos1);
	if (breakcharpos2 == -1) { Console.WriteLine("file path never specified..."); return; }
	float recordingSpeed = Convert.ToSingle(str[breakcharpos1..breakcharpos2]);
	string newFilePath = str[(breakcharpos2+1)..];
	game.StartRecording(newFilePath, fps: inputfps, speed: recordingSpeed); Console.WriteLine("Recording at " + inputfps + " fps at " + recordingSpeed + "x speed with file path " + newFilePath);
};

yeah it does that. actually maybe not anymore bc this is outdated quite a bit.
"record " records with whatever filepath after it, e.g. "record filethingy.mp4" will record with file name "filethingy.mp4".

"record_" records with a specified framerate, where the first is fps(?) and the second is the file path. formatted like "{framerate},{filepath}" or something idk

"recordf" records with "{fps},{recordingspeed},{filepath}" or something n stuff idk

""");
						game.Close();
						break;
					case "":
						Console.WriteLine("""
hi this is the help command idk. This is carbohydrated-engine, a "game engine" written in c# that uses OpenTK and StbImageSharp and some tutorials and stuff, made by the person with usernames that include "@ilovecake333636" and also maybe(?) "carbohydrated".
"carbohydrated-engine" on github (no quotation marks)

""");break;
					case " record args":
						Console.WriteLine("""
args are made by:\n
"-n -f rawvideo -pix_fmt bgra -s " + w + 'x' + h + " -r " + resfps.ToString("N4")) + " -i - -vf \"vflip\" -an -c:v libx265 -preset slow -crf 25 -pix_fmt yuv420p \" + p + '\"'

so, with 1920x1080 display at 60 output fps and 30 input fps and path "miwocivsnvafd.mp4" it would be:

-n -f rawvideo -pix_fmt bgra -s 1920x1080 -r 60.0000 -i - -vf "vflip" -an -c:v libx265 -preset slow -crf 25 -pix_fmt yuv420p "miwocivsnvafd.mp4"

yeah use that as an example i guess or something. how you input it is: using recordc, the secret 4th argument is formatted like all the others and is the exact arguments i think.

yep
""");break;
					default:
						Console.WriteLine("idk");
						break; } };
			// chatCommands["record"] = delegate (Game game, string str) {
			// 	if (str.Length > 7) {
			// 		switch (str[6]) {
			// 			case ' ': // normal recording behavior
			// 				game.StartRecording(str[7..]); Console.WriteLine("Recording with file path " + str[7..]);
			// 				break;
			// 			case '_': // record at some specified fps
			// 				int breakcharpos = str.IndexOf(',', 7);
			// 				if (breakcharpos == -1) { Console.WriteLine("fps not found..."); return; }
			// 				int fps = Convert.ToInt32(str[7..breakcharpos++]);
			// 				game.StartRecording(str[breakcharpos..], fps: fps); Console.WriteLine("Recording at " + fps + " fps with file path " + str[breakcharpos..]);
			// 				break;
			// 			case 'f' or 's': // stands for either fast, slow, or speed i guess idk
			// 				int breakcharpos1 = str.IndexOf(',', 7);
			// 				if (breakcharpos1 == -1) { Console.WriteLine("fps not found..."); return; }
			// 				int inputfps = Convert.ToInt32(str[7..breakcharpos1++]);
			// 				int breakcharpos2 = str.IndexOf(',', breakcharpos1);
			// 				if (breakcharpos2 == -1) { Console.WriteLine("speed not found..."); return; }
			// 				float recordingSpeed = Convert.ToSingle(str[breakcharpos1..breakcharpos2]);
			// 				string newFilePath = str[(breakcharpos2+1)..];
			// 				game.StartRecording(newFilePath, fps: inputfps, speed: recordingSpeed); Console.WriteLine("Recording at " + inputfps + " fps with file path " + newFilePath);
			// 				break; }} };
			chatCommands["record "] = delegate (Game game, string str) { game.StartRecording(str); Console.WriteLine("Recording with file path " + str); };
			chatCommands["record_"] = delegate (Game game, string str) {
				int breakcharpos = str.IndexOf(',');
				if (breakcharpos == -1) { Console.WriteLine("file path never specified..."); return; }
				int fps = Convert.ToInt32(str[..breakcharpos++]);
				game.StartRecording(str[breakcharpos..], fps: fps); Console.WriteLine("Recording at " + fps + " fps with file path " + str[breakcharpos..]); };
			chatCommands["recordf"] = chatCommands["records"] = delegate (Game game, string str) {
				int breakcharpos1 = str.IndexOf(',');
				if (breakcharpos1 == -1) { Console.WriteLine("speed + file location never specified..."); return; }
				int inputfps = Convert.ToInt32(str[..breakcharpos1++]);
				int breakcharpos2 = str.IndexOf(',', breakcharpos1);
				if (breakcharpos2 == -1) { Console.WriteLine("file path never specified..."); return; }
				float recordingSpeed = Convert.ToSingle(str[breakcharpos1..breakcharpos2]);
				string newFilePath = str[(breakcharpos2+1)..];
				game.StartRecording(newFilePath, fps: inputfps, speed: recordingSpeed); Console.WriteLine("Recording at " + inputfps + " fps at " + recordingSpeed + "x speed with file path " + newFilePath); };
			chatCommands["recordc"] = delegate (Game game, string str) {
				int breakcharpos1 = str.IndexOf(',');
				if (breakcharpos1 == -1) { Console.WriteLine("inFPS + file location never specified..."); return; }
				float outFPS = Convert.ToSingle(str[..breakcharpos1++]);
				int breakcharpos2 = str.IndexOf(',', breakcharpos1);
				if (breakcharpos2 == -1) { Console.WriteLine("file path never specified..."); return; }
				float inFPS = Convert.ToSingle(str[breakcharpos1..breakcharpos2++]);
				breakcharpos1 = str.IndexOf(',', breakcharpos2);
				if (breakcharpos1 == -1) // if there's no other additonal control info,
					{string newFilePath = str[breakcharpos2..];
					game.StartRecording(newFilePath, resfps: outFPS, inpfps: inFPS); Console.WriteLine("Inputting at " + inFPS + " fps, output file has " + outFPS + " fps with file path " + newFilePath);}
				else {
					string newFilePath = str[breakcharpos2..breakcharpos1++];
					string parameters = str[breakcharpos1..];
					game.StartRecording(newFilePath, outFPS, inFPS, parameters); Console.WriteLine("Inputting at " + inFPS + " fps, output file has " + outFPS + " fps with file path " + newFilePath + ".\nargs are \""+parameters+'\"');
				}};
			// recordc format is "{outputfps},{inputfps},{outputpath}"
			chatCommands["loadmap "] = delegate (Game game, string path) {
				List<VerticalOneKey> v1kl = [];
				List<ManiaRG> mrgl = [];
				foreach (IMinigame minigame in game._currentMinigames) { if (minigame is VerticalOneKey m0) v1kl.Add(m0); else if (minigame is ManiaRG m1) mrgl.Add(m1); }
				bool cont = v1kl.Count + mrgl.Count > 0;
				if (cont) {
					if (File.Exists(path)) {
						string data = File.ReadAllText(path);
						foreach (VerticalOneKey _m in v1kl)
						if (_m.TryLoadMapFromString(data)) Console.WriteLine("Loadmap success!"); else Console.WriteLine("Failed to load map.");
						foreach (ManiaRG _m in mrgl)
						if (_m.TryLoadMapFromString(data)) Console.WriteLine("Loadmap success!"); else Console.WriteLine("Failed to load map."); }
					else Console.WriteLine("Path does not exist!"); } };
			chatCommands["profamt "] = chatCommands["proflen "] = delegate (Game game, string str) {
				try { game.profilerFrameTimes = new double[Math.Min(Convert.ToUInt32(str), 1048576)]; game.profilerIndex = 0; }
				catch (FormatException ex) { Console.WriteLine("Incorrect formatting. " + ex.Message); }
				catch (OverflowException ex) { Console.WriteLine("Overflow. ARE YOU TRYING TO CRASH YOUR COMPUTER OR SOMETHING??? (automatically capped at 1048576, but this error only appears above ~4.2B) " + ex.Message); } };
			// code based off https://stackoverflow.com/questions/73003523/how-to-get-all-inherited-classes by Gec
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
				foreach (Type type in assembly.GetTypes())
					if (typeof(IMinigame).IsAssignableFrom(type)) {
						EverySingleMinigame.Add(type);
						// type.GetMethod("StartInit").Invoke(null,null);
						type.GetMethod("StartInit")?.Invoke(null,null);
						if (type.GetField("GameIdentifier")?.GetValue(null) is string s && !string.IsNullOrEmpty(s) && type.GetField("InGameConstructorthings")?.GetValue(null) is Dictionary<string, Action<Game>> dict && !(dict == null || dict.Count < 1)) {
							AllMinigames.Add((type, s));
							foreach (var a in dict) {MinigameInitializers[a.Key] = a.Value;}
						} else Console.WriteLine("well this minigame has screwed up at least one thing.."+type.FullName);
					}
			StringBuilder sb = new("MinigameInitializers list:\n");
			foreach (var a in MinigameInitializers) {
				sb.Append(a.Key+'\n');
			}
			sb.Append("EverySingleMinigame:\n");
			foreach (var a in EverySingleMinigame) {
				sb.Append(a.FullName+'\n');
			}
			sb.Append("AllMinigames:\n");
			foreach ((Type type, string name) in AllMinigames) {
				sb.Append(name+"; FullName: "+type.FullName+'\n');
			}
			sb.Append("\ndone\n");
			Console.Write(sb);sb.Clear();
			

			// chatCommands["what"] = delegate (Game game, string str) { Console.WriteLine("Hello, World!"); };
			// noInputChatCommands["what"] = delegate (Game game) { Console.WriteLine("Hello, World!"); };
			// /*template*/
		}
	}
	public struct Announcement(string msg, long dsts, Vector3 textColor, Vector3 bgColor, float bgTransparency, bool st = false, float fot = 1)
    {
		public string Message = msg;
		public long DisappearTimestamp = dsts;
		public Vector3 TextColor = textColor;
		public Vector3 BGColor = bgColor;
		public float BGTransparency = bgTransparency;
		public bool SpecialText = st;
		public float FadeOutTime = fot;
    }
}