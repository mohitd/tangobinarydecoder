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
        }

        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: (mono) TangoBinaryDecoder.exe inputFile outputFile");
                return;
            }

            List<PointXYZ> points;

            using (BinaryReader reader = new BinaryReader(File.OpenRead(args[0])))
            {
                ReadPoseFromFile(reader);
                ReadDepthFromFile(reader, out points);
            }
                
            PrintCoordinates(points);
            PrintCoordinatesToFile(args[1], points);
        }

        private static void ReadPoseFromFile(BinaryReader reader)
        {
            if (reader == null)
            {
                return;
            }

            string frameMarker;
            try {
                frameMarker = reader.ReadString();
            } catch (EndOfStreamException x) {
                Console.WriteLine(x.StackTrace);
                return;
            }

            if (frameMarker.CompareTo("poseframe\n") != 0)
            {
                Console.WriteLine("Failed to read pose");
                return;
            }

            Console.WriteLine("timestamp: " + double.Parse(reader.ReadString()));

            Console.WriteLine("BaseFrame: " + ((TangoEnums.TangoCoordinateFrameType)reader.ReadInt32()).ToString());
            Console.WriteLine("TargetFrame: " + ((TangoEnums.TangoCoordinateFrameType)reader.ReadInt32()).ToString());

            Console.WriteLine("Status: " + ((TangoEnums.TangoPoseStatusType)reader.ReadInt32()).ToString());
            Console.WriteLine("(" + reader.ReadDouble().ToString() + "," + reader.ReadDouble() + "," + reader.ReadDouble() + ")");
            Console.WriteLine("(" + reader.ReadDouble() + "," + reader.ReadDouble() + "," + reader.ReadDouble() + "," + reader.ReadDouble() + ")");
        }

        private static void ReadDepthFromFile(BinaryReader reader, out List<PointXYZ> points)
        {
            // need to initialize this here in case the method returns
            points = new List<PointXYZ>();

            string frameMarker;
            try {
                frameMarker = reader.ReadString();
            } catch (EndOfStreamException x) {
                reader.BaseStream.Position = 0;
                frameMarker = reader.ReadString();
                Console.WriteLine(x.StackTrace);
                return;
            }

            if (frameMarker.CompareTo("depthframe\n") != 0) {
                Console.WriteLine("Failed to read depth");
                return;
            }

            Console.WriteLine("timestamp: " + double.Parse(reader.ReadString()));
            int pointCount = int.Parse(reader.ReadString());
            Console.WriteLine("pointCount: " + pointCount);

            //load up the data
            for (int i = 0; i < pointCount; i++)
            {
                PointXYZ pt = new PointXYZ();
                pt.x = reader.ReadSingle();
                pt.y = reader.ReadSingle();
                pt.z = reader.ReadSingle();
                points.Add(pt);
            }

            return;
        }

        private static void PrintCoordinates(List<PointXYZ> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                float x = points[i].x;
                float y = points[i].y;
                float z = points[i].z;

                if (x == 0 && y == 0 && z == 0)
                    continue;

                Console.WriteLine("(" + x + "," + y + "," + z + ")");
            }
        }

        private static void PrintCoordinatesToFile(string path, List<PointXYZ> points)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                for (int i = 0; i < points.Count; i++)
                {
                    float x = points[i].x;
                    float y = points[i].y;
                    float z = points[i].z;

                    if (x == 0 && y == 0 && z == 0)
                        continue;

                    writer.WriteLine(x + " " + y + " " + z);
                    writer.Flush();
                }
            }
        }
    }
}
