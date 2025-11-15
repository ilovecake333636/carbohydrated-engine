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


namespace GameEngineThing
{
    public static class debuggingThingClass
    {
        public const bool IsDebugging = false;
        /// <summary>
        /// idk some debugging thing. it doesn't really do anything except exist and log some stuff if you pass true.
        /// </summary>
        /// <param name="isDebugging">self-explanatory. if you are debugging, set it to true. If you aren't, then either don't call this you idiot or pass false.</param>
        public static void someDebugThing(bool isDebugging)
        {
            if (!isDebugging) return;
            int a = 1;
            int b = 1;
            int c = 1;
            Console.WriteLine("a = 1; (a-- == 0) is " + (a-- == 0) + "; b = 1; (b-- == 1) is " + (b-- == 1) + "; c = 1; c-- is " + c--);
            for (int j = 0; j < 4; j++) /* debugging things */
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
                for (int i = 0; i < 25000000; i++)
                {
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
                // time = Stopwatch.GetTimestamp();
                // for (int i = 0; i < 25000000; i++) DataStuff.HSVToRGBUnoptimized(new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()));
                // dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
                // Console.WriteLine("somewhat 'unoptimized' code loop: " + dt + "ms.");
                // time = Stopwatch.GetTimestamp();
                // for (int i = 0; i < 25000000; i++) DataStuff.HSVToRGB(new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()));
                // dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
                // Console.WriteLine("somewhat 'optimized' code loop: " + dt + "ms.");
                // time = Stopwatch.GetTimestamp();
                // for (int i = 0; i < 25000000; i++) DataStuff.HSVToRGB2(new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()));
                // dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
                // Console.WriteLine("something 2: (hopefully this might be a bit faster) " + dt + "ms.");
                // time = Stopwatch.GetTimestamp();
                // for (int i = 0; i < 25000000; i++) DataStuff.HSVToRGB3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
                // dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
                // Console.WriteLine("something 3: " + dt + "ms.");
                // time = Stopwatch.GetTimestamp();
                // for (int i = 0; i < 25000000; i++) DataStuff.HSVToRGB4(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
                // dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
                // Console.WriteLine("something 4: " + dt + "ms.");
                // time = Stopwatch.GetTimestamp();
                // for (int i = 0; i < 25000000; i++) DataStuff.HSVToRGB5(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
                // dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
                // Console.WriteLine("something 5: " + dt + "ms.");
                time = Stopwatch.GetTimestamp();
                for (int i = 0; i < 25000000; i++) DataStuff.HueToRGB(Random.Shared.NextSingle());
                dt = Stopwatch.GetElapsedTime(time).TotalMilliseconds;
                Console.WriteLine("just hue, no sat or val code loop: " + dt + "ms.");
            }
        }
    }
}
