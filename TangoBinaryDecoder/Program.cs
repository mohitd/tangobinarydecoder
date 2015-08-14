//
// Program.cs
//
// Author:
//       mohitd <mohitd2000@gmail.com>
//
// Copyright (c) 2015 mohitd
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace TangoBinaryDecoder
{
	class MainClass
	{
		struct PointXYZ
		{
			public float x, y, z;
		};

		public static void Main (string[] args)
		{
			if (args.Length < 2) {
				Console.WriteLine ("Usage: (mono) TangoBinaryDecoder.exe inputFile outputFile");
				return;
			}

			List<PointXYZ> points = new List<PointXYZ> ();

            if (!File.Exists(args[0])) {
                Console.WriteLine("Input file does not exist!");
                return;
            }

			Console.WriteLine ("Reading from " + args[0] + "...");
			DateTime tic = DateTime.Now;
			using (BinaryReader reader = new BinaryReader(File.OpenRead(args[0]))) {
                // Keep reading until EOF
				while (reader.BaseStream.Position != reader.BaseStream.Length) {
					ReadPoseFromFile (reader);
					ReadDepthFromFile (reader, points);
				}
			}
			TimeSpan toc = DateTime.Now - tic;
			Console.WriteLine ("[done " + toc.Milliseconds + "ms]");

			Console.WriteLine ("Writing " + points.Count + " points to " + args[1] + "...");
            Console.WriteLine ("Writing " + points.Count + " points to " + args[1].Substring(0, args[1].IndexOf(".")) + ".csv" + "...");
			DateTime tic2 = DateTime.Now;
			WriteDepthToFile (args[1], points);
			TimeSpan toc2 = DateTime.Now - tic2;
			Console.WriteLine ("[done " + toc2.Milliseconds + "ms]");
		}

		private static void ReadPoseFromFile (BinaryReader reader)
		{
			if (reader == null) {
				return;
			}

			string frameMarker;
			try {
				frameMarker = reader.ReadString ();
			} catch (EndOfStreamException x) {
				Console.WriteLine (x.StackTrace);
				return;
			}

			if (frameMarker.CompareTo ("poseframe\n") != 0) {
				Console.WriteLine ("Failed to read pose");
				return;
			}

			reader.ReadString ();   // timestamp

			reader.ReadInt32 ();    // BaseFrame
			reader.ReadInt32 ();    // TargetFrame
			reader.ReadInt32 ();    // Status

			reader.ReadDouble ();   // ---\
            reader.ReadDouble ();   //    |---- Translation (x,y,z)
			reader.ReadDouble ();   // ---/ 

			reader.ReadDouble ();   // ---\
            reader.ReadDouble ();   //    |\
            reader.ReadDouble ();   //    || --- Orientation (a,b,c,d)
			reader.ReadDouble ();   // ---/
		}

		private static void ReadDepthFromFile (BinaryReader reader, List<PointXYZ> points)
		{
			string frameMarker;
			try {
				frameMarker = reader.ReadString ();
			} catch (EndOfStreamException x) {
				reader.BaseStream.Position = 0;
				frameMarker = reader.ReadString ();
				Console.WriteLine (x.StackTrace);
				return;
			}

			if (frameMarker.CompareTo ("depthframe\n") != 0) {
				Console.WriteLine ("Failed to read depth");
				return;
			}

			reader.ReadString ();    // timestamp
			int pointCount = int.Parse (reader.ReadString ());

			//load up the data
			for (int i = 0; i < pointCount; i++) {
				PointXYZ pt = new PointXYZ ();
				pt.x = reader.ReadSingle ();
				pt.y = reader.ReadSingle ();
				pt.z = reader.ReadSingle ();
				points.Add (pt);
			}

			return;
		}

		private static void WriteDepthToFile (string path, List<PointXYZ> points)
        {
            string csvPath = path.Substring(0, path.IndexOf(".")) + ".csv";
            using (StreamWriter csvWriter = new StreamWriter(csvPath))
            {
                // ParaView CSV column markers
                csvWriter.WriteLine("x coord,y coord,z coord");
                csvWriter.Flush();
                using (StreamWriter writer = new StreamWriter(path))
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        float x = points[i].x;
                        float y = points[i].y;
                        float z = points[i].z;
                
                        if (x == 0 && y == 0 && z == 0)
                            continue;
                
                        // for Intrepid algorithm
                        writer.WriteLine (x + " " + y + " " + z);
                        writer.Flush ();

                        // for Paraview viewing 
                        csvWriter.WriteLine(x + "," + y + "," + z);
                        csvWriter.Flush();
                    }
                }
            }
		}
	}
}
