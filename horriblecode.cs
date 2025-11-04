using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GameEngineThing
{
	public struct GlyphData
	{
		public Vector2i textureStart;
		public Vector2i textureSize;
		public Vector2 bearing;
		public Vector2 size;
		public Vector2 advance;
		public GlyphData(GlyphAdvType advType, Vector2i textureStart, Vector2i textureSize, Vector2 adv, Vector2? bearing)
		{
			if (bearing.HasValue) this.bearing = (Vector2)bearing; // cursor keeps screaming when i dont cast this and the compiler doesnt compile for some reason idk why
			else this.bearing = Vector2.Zero;
			size = new(Math.Abs(textureSize.X), Math.Abs(textureSize.Y));
			advance = advType switch
			{
				GlyphAdvType.SizeXPlusA => new(adv.X + size.X, adv.Y),
				GlyphAdvType.SizeYPlusA => new(adv.X, size.Y + adv.Y),
				GlyphAdvType.SizeXYPlusA => size + adv,
				_ => adv,
			};
			this.textureStart = textureStart;
			this.textureSize = textureSize;
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
		private static readonly Vector2 Adv = new(1f, 0f);
		public static FontCharacterData FontCharDeeta = new();
		static FontCharFillerThing()
		{
			// initialization
			foreach (KeyValuePair<string, string> item in SCharsDuplicates) { SChars[item.Key] = SChars[item.Value]; }

			// now some more things like this
			FillFontChar(FontCharDeeta);
		}
		private static readonly Dictionary<char, GlyphData> Chars = new()
		{
			['a'] = new(XPA, new(0, 0), new(5, 7), Adv, null),
			['b'] = new(XPA, new(7, 0), new(4, 7), Adv, null),
			['c'] = new(XPA, new(0, 0), new(3, 4), Adv, null),
			['d'] = new(XPA, new(11, 0), new(-4, 7), Adv, null),
			['e'] = new(XPA, new(11, 0), new(4, 5), Adv, null),
			['f'] = new(XPA, new(16, 0), new(4, 7), Adv, null),
			['g'] = new(XPA, new(20, 0), new(4, 7), Adv, new(0f, -3f)),
			['h'] = new(XPA, new(24, 0), new(4, 7), Adv, null),
			['i'] = new(XPA, new(31, 1), new(1, 6), Adv, null),
			['j'] = new(XPA, new(28, 0), new(4, 7), Adv, new(0f, -3f)),
			['k'] = new(XPA, new(32, 0), new(3, 7), Adv, null),
			['l'] = new(XPA, new(35, 0), new(2, 7), Adv, null),
			['m'] = new(XPA, new(37, 0), new(5, 4), Adv, null),
			['n'] = new(XPA, new(42, 0), new(4, 4), Adv, null),
			['o'] = new(XPA, new(46, 0), new(4, 4), Adv, null),
			['p'] = new(XPA, new(7, 7), new(4, -7), Adv, new(0f, -3f)),
			['q'] = new(XPA, new(11, 7), new(-6, -7), Adv, new(0f, -3f)),
			['r'] = new(XPA, new(50, 0), new(4, 4), Adv, null),
			['s'] = new(XPA, new(54, 0), new(4, 5), Adv, null),
			['t'] = new(XPA, new(58, 0), new(3, 7), Adv, null),
			['u'] = new(XPA, new(61, 0), new(4, 4), Adv, null),
			['v'] = new(XPA, new(49, 3), new(3, 3), Adv, null),
			['w'] = new(XPA, new(49, 3), new(5, 3), Adv, null),
			['x'] = new(XPA, new(51, 2), new(3, 3), Adv, null),
			['y'] = new(XPA, new(65, 0), new(4, 5), Adv, new(0f, -3f)),
			['z'] = new(XPA, new(69, 0), new(4, 5), Adv, null),

			['A'] = new(XPA, new(0, 7), new(4, 7), Adv, null),
			['B'] = new(XPA, new(11, 7), new(4, 7), Adv, null),
			['C'] = new(XPA, new(4, 7), new(4, 7), Adv, null),
			['D'] = new(XPA, new(12, 7), new(-4, 7), Adv, null),
			['E'] = new(XPA, new(18, 7), new(4, 7), Adv, null),
			['F'] = new(XPA, new(19, 7), new(-4, 7), Adv, null),
			['G'] = new(XPA, new(22, 7), new(5, 7), Adv, null),
			['H'] = new(XPA, new(27, 7), new(5, 7), Adv, null),
			['I'] = new(XPA, new(32, 7), new(4, 7), Adv, null),
			['J'] = new(XPA, new(35, 7), new(6, 7), Adv, new(0f, -1f)),
			['K'] = new(XPA, new(52, 7), new(5, 7), Adv, null),
			['L'] = new(XPA, new(43, 14), new(-4, -7), Adv, null),
			['M'] = new(XPA, new(42, 7), new(7, 7), Adv, null),
			['N'] = new(XPA, new(48, 7), new(5, 7), Adv, null),
			['O'] = new(XPA, new(4, 7), new(5, 7), Adv, null),
			['P'] = new(XPA, new(7, 7), new(4, -7), Adv, null),
			['Q'] = new(XPA, new(57, 7), new(7, 7), Adv, null),
			['R'] = new(XPA, new(64, 7), new(5, 7), Adv, null),
			['S'] = new(XPA, new(68, 7), new(5, 7), Adv, null),
			['T'] = new(XPA, new(9, 20), new(5, -7), Adv, null),
			['U'] = new(XPA, new(73, 7), new(5, 7), Adv, null),
			['V'] = new(XPA, new(78, 7), new(5, 7), Adv, null),
			['W'] = new(XPA, new(42, 14), new(7, -7), Adv, null),
			['X'] = new(XPA, new(83, 7), new(5, 7), Adv, null),
			['Y'] = new(XPA, new(88, 7), new(5, 7), Adv, null),
			['Z'] = new(XPA, new(93, 7), new(5, 7), Adv, null),

			// [' '] = new(XPA, Vec2Z,    new(1f, 0f), new(0, 0), new(0, 0)),
			['?'] = new(XPA, new(73, 0), new(2, 6), Adv, null),
			['!'] = new(XPA, new(31, 7), new(1, -6), Adv, null),

			['0'] = new(XPA, new(100, 0), new(4, 7), Adv, null), //[Vec2Z,new(2f, 0f),new(103,0),new(4,7)]
			['1'] = new(XPA, new(75, 0), new(4, 7), Adv, null),
			['2'] = new(XPA, new(79, 0), new(4, 7), Adv, null),
			['3'] = new(XPA, new(83, 0), new(4, 7), Adv, null),
			['4'] = new(XPA, new(90, 0), new(4, 7), Adv, null),
			['5'] = new(XPA, new(96, 0), new(4, 7), Adv, null),
			['6'] = new(XPA, new(106, 0), new(4, 7), Adv, null),
			['7'] = new(XPA, new(110, 0), new(4, 7), Adv, null),
			['8'] = new(XPA, new(86, 0), new(4, 7), Adv, null),
			['9'] = new(XPA, new(114, 0), new(4, 7), Adv, null),
			['.'] = new(XPA, new(0, 1), new(1, 1), Adv, null),
			[','] = new(XPA, new(5, 0), new(-2, 3), Adv, new(0, -2)),
			['<'] = new(XPA, new(53, 8), new(3, 5), Adv, null),
			['>'] = new(XPA, new(56, 8), new(-3, 5), Adv, null),
			['/'] = new(XPA, new(111, 0), new(3, 6), Adv, null),
			[':'] = new(XPA, new(0, 5), new(1, 3), Adv, new(0, 2)),
			[';'] = new(XPA, new(4, 9), new(2, -4), Adv, null),
			['\''] = new(XPA, new(0, 1), new(1, 2), Adv, null),
			['"'] = new(XPA, new(49, 4), new(3, 2), Adv, null),
			['\\'] = new(XPA, new(114, 0), new(-3, 6), Adv, null),
			['|'] = new(XPA, new(7, 0), new(1, 8), Adv, new(0f, -.5f)),
			['['] = new(XPA, new(93, 1), new(3, 7), Adv, null),
			[']'] = new(XPA, new(96, 1), new(-3, 7), Adv, null),
			['{'] = new(XPA, new(118, 0), new(3, 7), Adv, null),
			['}'] = new(XPA, new(121, 0), new(-3, 7), Adv, null),
			['-'] = new(XPA, new(1, 0), new(2, 1), Adv, new(0f, 4f)),
			['_'] = new(XPA, new(0, 10), new(4, 1), Adv, null),
			['+'] = new(XPA, new(17, 9), new(3, 3), Adv, new(0f, 2f)),
			['='] = new(XPA, new(66, 0), new(2, 3), Adv, null),
			['@'] = new(XPA, new(121, 0), new(8, 8), Adv, null),
			['#'] = new(XPA, new(129, 0), new(5, 5), Adv, null),
			['$'] = new(XPA, new(134, 0), new(5, 9), Adv, null),//[Vec2Z,new(2f,0f),new(143,0),new(7,11)],[Vec2Z,new(2f, 0f),new(143,0),new(-5,9)]
			['%'] = new(XPA, new(150, 0), new(7, 7), Adv, null),
			['^'] = new(XPA, new(0, 2), new(3, 2), Adv, new(0f, 4f)),
			['&'] = new(XPA, new(157, 0), new(7, 9), Adv, null),
			['*'] = new(XPA, new(164, 0), new(4, 3), Adv, null),
			['('] = new(XPA, new(4, 7), new(2, 7), Adv, null),
			[')'] = new(XPA, new(6, 7), new(-2, 7), Adv, null),
			['`'] = new(XPA, new(49, 3), new(2, 2), Adv, null),
			['~'] = new(XPA, new(50, 3), new(4, 2), Adv, null),

			['零'] = new(XPA, new(512, 31), new(11, 18), Adv, null),
			['一'] = new(XPA, new(512, 20), new(7, 1), Adv, new(0f, 3f)),
			['二'] = new(XPA, new(512, 20), new(7, 3), Adv, new(0f, 2f)),
			['三'] = new(XPA, new(512, 16), new(7, 5), Adv, new(0f, 1f)),
			['四'] = new(XPA, new(512, 10), new(7, 7), Adv, null),
			['五'] = new(XPA, new(512, 4), new(7, 7), Adv, null),
			['六'] = new(XPA, new(512 + 6, 1), new(7, 7), Adv, null),
			['七'] = new(XPA, new(512 + 7, 8), new(7, 7), Adv, null),
			['八'] = new(XPA, new(512 + 13, 9), new(7, 7), Adv, new(1f, 2f)),
			['九'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['十'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['百'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['千'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['万'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['亿'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['白'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['上'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['下'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['中'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['国'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['文'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['口'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['回'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['日'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['自'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['己'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['早'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['午'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['晚'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['凌'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['晨'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['我'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['的'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['你'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['他'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['她'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
			['它'] = new(XPA, new(512, 0), new(7, 7), Adv, null),
		};
		private static readonly Dictionary<string, GlyphData> SChars = new()
		{
			["blinker"] = new(XPA, new(164, 3), new(4, 7), Adv, null),
			["loaf"] = new(XPA, new(256, 10), new(11, 10), Adv, null),
			["loaf2"] = new(XPA, new(256, 11), new(11, -11), Adv, null),
			["loafBIG"] = new(XPA, new(295, 0), new(46, 42), Adv, null),
			["loafLegacy"] = new(XPA, new(267, 0), new(11, 9), Adv, null),
			["loafLegacyNoR"] = new(XPA, new(277, 0), new(10, 9), new(2f, 0f), null),
			["loafLegacyNoL"] = new(XPA, new(287, 0), new(-10, 9), Adv, new(1f, 0f)),
			["loafLegacyNoLR"] = new(XPA, new(286, 0), new(9, 8), new(2f, 0f), new(1f, 0f)),
			["unknown"] = new(XPA, new(0, 32), new(16, 16), new(0, 1), null),
			["note"] = new(XPA, new(16, 32), new(16, 16), new(0, 1), null),
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
	}
	/// <summary>
	/// this is probably not the correct way to store this data but idk what the correct way is. :p
	/// </summary>
	public static class DataStuff
	{
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
			-.5f, -.5f, -.5f,   60/1024f, 128/256f, -.5f, -.4f, -.5f,   64/1024f, 128/256f,
			 .5f, -.5f, -.5f,   60/1024f, 148/256f,  .5f, -.4f, -.5f,   64/1024f, 148/256f,
			 .5f, -.5f,  .5f,   60/1024f, 128/256f,  .5f, -.4f,  .5f,   64/1024f, 128/256f,
			-.5f, -.5f,  .5f,   60/1024f, 148/256f, -.5f, -.4f,  .5f,   64/1024f, 148/256f,

			-.5f, -.5f, -.5f,   40/1024f, 128/256f,  .5f, -.5f, -.5f,   60/1024f, 128/256f,
			 .5f, -.5f,  .5f,   60/1024f, 148/256f, -.5f, -.5f,  .5f,   40/1024f, 148/256f,
			-.5f, -.4f, -.5f,   40/1024f, 128/256f,  .5f, -.4f, -.5f,   60/1024f, 128/256f,
			 .5f, -.4f,  .5f,   60/1024f, 148/256f, -.5f, -.4f,  .5f,   40/1024f, 148/256f,


			-.5f,  .5f, -.5f,   60/1024f, 128/256f, -.5f,  .4f, -.5f,   64/1024f, 128/256f,
			 .5f,  .5f, -.5f,   60/1024f, 148/256f,  .5f,  .4f, -.5f,   64/1024f, 148/256f,
			 .5f,  .5f,  .5f,   60/1024f, 128/256f,  .5f,  .4f,  .5f,   64/1024f, 128/256f,
			-.5f,  .5f,  .5f,   60/1024f, 148/256f, -.5f,  .4f,  .5f,   64/1024f, 148/256f,

			-.5f,  .5f, -.5f,   40/1024f, 128/256f,  .5f,  .5f, -.5f,   60/1024f, 128/256f,
			 .5f,  .5f,  .5f,   60/1024f, 148/256f, -.5f,  .5f,  .5f,   40/1024f, 148/256f,
			-.5f,  .4f, -.5f,   40/1024f, 128/256f,  .5f,  .4f, -.5f,   60/1024f, 128/256f,
			 .5f,  .4f,  .5f,   60/1024f, 148/256f, -.5f,  .4f,  .5f,   40/1024f, 148/256f,


			-.35f, -.45f, -.35f,   64/1024f, 128/256f, -.35f,  .45f, -.35f,   64/1024f, 146/256f,
			 .35f, -.45f, -.35f,   78/1024f, 128/256f,  .35f,  .45f, -.35f,   78/1024f, 146/256f,
			 .35f, -.45f,  .35f,   64/1024f, 128/256f,  .35f,  .45f,  .35f,   64/1024f, 146/256f,
			-.35f, -.45f,  .35f,   78/1024f, 128/256f, -.35f,  .45f,  .35f,   78/1024f, 146/256f,
		];
		public static readonly uint[] PlrTorsoI = [
			0,1,2, 1,2,3,  2,3,4, 3,4,5,  4,5,6, 5,6,7,  6,7,0, 7,0,1,
			8,9,10, 8,10,11,  12,13,14, 12,14,15,
			16,17,18, 17,18,19,  18,19,20, 19,20,21,  20,21,22, 21,22,23,  22,23,16, 23,16,17,
			24,25,26, 24,26,27,  28,29,30, 28,30,31,
			32,33,34, 33,34,35,  34,35,36, 35,36,37,  36,37,38, 37,38,39,  38,39,32, 39,32,33,
		];
		public static readonly float[] PlrArmV = [
			-.25f,  .2f, -.25f,   40/1024f, 188/256f,   -.25f, -1f, -.25f,   40/1024f, 148/256f,
			 .25f,  .2f, -.25f,   60/1024f, 188/256f,    .25f, -1f, -.25f,   60/1024f, 148/256f,
			 .25f,  .2f,  .25f,   40/1024f, 188/256f,    .25f, -1f,  .25f,   40/1024f, 148/256f,
			-.25f,  .2f,  .25f,    0/1024f, 188/256f,   -.25f, -1f,  .25f,    0/1024f, 148/256f,
			-.25f,  .2f,  .25f,   60/1024f, 188/256f,   -.25f, -1f,  .25f,   60/1024f, 148/256f,

			-.25f,  .2f, -.25f,   20/1024f, 128/256f,    .25f,  .2f, -.25f,   40/1024f, 128/256f,
			 .25f,  .2f,  .25f,   40/1024f, 148/256f,   -.25f,  .2f,  .25f,   20/1024f, 148/256f,
			-.25f, -1f, -.25f,    0/1024f, 128/256f,    .25f, -1f, -.25f,   20/1024f, 128/256f,
			 .25f, -1f,  .25f,   20/1024f, 148/256f,   -.25f, -1f,  .25f,    0/1024f, 148/256f,
		];
		public static readonly uint[] PlrArmI = [0, 1, 2, 1, 2, 3, 2, 3, 4, 3, 4, 5, 4, 5, 6, 5, 6, 7, 8, 9, 0, 9, 0, 1, 10, 11, 12, 10, 12, 13, 14, 15, 16, 14, 16, 17,];
		public static readonly float[] PlrLegV = [
			-.25f,  .3f, -.25f,   40/1024f, 188/256f,   -.25f, -1f, -.25f,   40/1024f, 148/256f,
			 .25f,  .3f, -.25f,   60/1024f, 188/256f,    .25f, -1f, -.25f,   60/1024f, 148/256f,
			 .25f,  .3f,  .25f,   40/1024f, 188/256f,    .25f, -1f,  .25f,   40/1024f, 148/256f,
			-.25f,  .3f,  .25f,    0/1024f, 188/256f,   -.25f, -1f,  .25f,    0/1024f, 148/256f,
			-.25f,  .3f,  .25f,   60/1024f, 188/256f,   -.25f, -1f,  .25f,   60/1024f, 148/256f,

			-.25f,  .3f, -.25f,   20/1024f, 128/256f,    .25f,  .3f, -.25f,   40/1024f, 128/256f,
			 .25f,  .3f,  .25f,   40/1024f, 148/256f,   -.25f,  .3f,  .25f,   20/1024f, 148/256f,
			-.25f, -1f, -.25f,    0/1024f, 128/256f,    .25f, -1f, -.25f,   20/1024f, 128/256f,
			 .25f, -1f,  .25f,   20/1024f, 148/256f,   -.25f, -1f,  .25f,    0/1024f, 148/256f,
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
			-.7f,-.5f,-1f,        393f/1024f, 15f/256f,
			 .7f,-.5f,-1f,        393f/1024f, 15f/256f,
			 .7f,-.5f, 1f,        393f/1024f, 15f/256f,
			-.7f,-.5f, 1f,        393f/1024f, 15f/256f,
			// 4-9 right ear (side)
			-PlrEarW, .5f,-1f,     393f/1024f, 15f/256f,
			-PlrEarW, .5f, 1f,     393f/1024f, 15f/256f,
			-.7f,  .809f,-1f,     393f/1024f, 15f/256f, // y is .5f ± .309f
			-.7f,  .809f, 1f,     393f/1024f, 15f/256f,
			-.7f,  .191f,-1f,     393f/1024f, 15f/256f,
			-.7f,  .191f, 1f,     393f/1024f, 15f/256f,
			// 10-12 right ear (back)
			-PlrEarW,  .5f,-1f,    392f/1024f, 22f/256f,
			-.7f,   .809f,-1f,    392f/1024f, 22f/256f,
			-.7f,   .191f,-1f,    392f/1024f, 22f/256f,
			// 13-15 right ear (front)
			-PlrEarW,  .5f, 1f,    393f/1024f, 15f/256f,
			-.7f,   .809f, 1f,    393f/1024f, 15f/256f,
			-.7f,   .191f, 1f,    393f/1024f, 15f/256f,
				
			// 16-21 left ear (side)
			 PlrEarW, .5f,-1f,     393f/1024f, 15f/256f,
			 PlrEarW, .5f, 1f,     393f/1024f, 15f/256f,
			 .7f,   .809f,-1f,     393f/1024f, 15f/256f,
			 .7f,   .809f, 1f,     393f/1024f, 15f/256f,
			 .7f,   .191f,-1f,     393f/1024f, 15f/256f,
			 .7f,   .191f, 1f,     393f/1024f, 15f/256f,
			// 22-24 left ear (back)
			 PlrEarW,  .5f,-1f,     392f/1024f, 22f/256f,
			 .7f,   .809f,-1f,     392f/1024f, 22f/256f,
			 .7f,   .191f,-1f,     392f/1024f, 22f/256f,
			// 25-27 left ear (front)
			 PlrEarW,  .5f, 1f,     393f/1024f, 15f/256f,
			 .7f,   .809f, 1f,     393f/1024f, 15f/256f,
			 .7f,   .191f, 1f,     393f/1024f, 15f/256f,

			// =======================================
			// actual textured (not crust) part next!!
			// =======================================

			// 28-31 bottom
			-.7f,-.5f,-1f,        188f/1024f, 128f/256f,
			 .7f,-.5f,-1f,        132f/1024f, 128f/256f,
			 .7f,-.5f, 1f,        78f/1024f, 128f/256f,
			-.7f,-.5f, 1f,        134f/1024f, 128f/256f,
			// 32-35 right ear-ish
			-PlrEarW, .5f,-1f,     (160f+40f*PlrEarW)/1024f, 168f/256f,
			-PlrEarW, .5f, 1f,     (106f+40f*PlrEarW)/1024f, 168f/256f,
			-.7f,  .191f,-1f,     188f/1024f, 155.64f/256f,
			-.7f,  .191f, 1f,     134f/1024f, 155.64f/256f,
			// 36-39 left ear-ish
			 PlrEarW, .5f,-1f,     (160f-40f*PlrEarW)/1024f, 168f/256f,
			 PlrEarW, .5f, 1f,     (106f-40f*PlrEarW)/1024f, 168f/256f,
			 .7f,  .191f,-1f,     132f/1024f, 155.64f/256f,
			 .7f,  .191f, 1f,     78f/1024f, 155.64f/256f,
			
			// 40-43 ear centers
			-PlrEarC, .5f,-1f,     392f/1024f, 15f/256f, // right-back
			-PlrEarC, .5f, .8f, /*the ear is watching*/     393f/1024f, 22f/256f, // right-front
			PlrEarC, .5f,-1f,     392f/1024f, 15f/256f, // left-back
			PlrEarC, .5f, .8f,     393f/1024f, 22f/256f, // left-front
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
		public static readonly V1KChart[] BuiltInV1KCharts;
		public static readonly ManiaKey[][][] BuiltInCharts = [
			[
				[

				],[

				],[

				],[

				],
			],

		];
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
			(float rr, float rg, float rb) = (Math.Abs(hue-3),Math.Abs(hue-2),Math.Abs(hue-4)); // raw red, raw green, raw blue
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
		static DataStuff()
		{
			BuiltInV1KCharts = new V1KChart[2];
			V1KKey[] V1KChart1KeyData = new V1KKey[4096];
			for (int i = 0; i < 4096; i++) { V1KChart1KeyData[i] = new V1KKey(Math.Pow(i, 0.5)); }
			V1KKey[] V1KChart2KeyData = new V1KKey[32768];
			for (int i = 0; i < 32768; i++) { V1KChart2KeyData[i] = new V1KKey(i / 694.20); }

			BuiltInV1KCharts[0] = new V1KChart(V1KChart1KeyData);
			BuiltInV1KCharts[1] = new V1KChart(V1KChart2KeyData);
		}
	}
}