using Fly.Blackbox.Console.Riff;
using System;
using System.Collections.Generic;

namespace Fly.Blackbox.Console.ExtractCoordinates
{
    public class CoordinatesExtractor
    {
        public IEnumerable<CameraSensors> Extract(string input)
        {
            var parser = new RiffParser();
            parser.OpenFile(input);

            bool continueReading = true;
            while (continueReading)
            {
                continueReading = parser.ReadElement((a, b, c, d) => ProcessChunk(a, b, c, d),
                    (a, b, c) => ProcessElement(a, b, c));
            }

            var list = new List<CameraSensors>();

            int frame = 0;
            foreach (var item in parser.GetStream(2))
            {
                parser.SetPosition(item.Offset);
                parser.ReadElement((p, fourCC, length, paddedLength) =>
                {
                    var buffer = new Byte[paddedLength];
                    p.ReadData(buffer, 0, paddedLength);

                    var g1 = BitConverter.ToSingle(buffer, 40);
                    var g2 = BitConverter.ToSingle(buffer, 44);
                    var g3 = BitConverter.ToSingle(buffer, 48);

                    var gps1 = BitConverter.ToSingle(buffer, 60);
                    var gps2 = BitConverter.ToSingle(buffer, 64);

                    var coordinate = new Coordinate();
                    coordinate.ParseIsoString("-" + gps1.ToString() + "-0" + gps2.ToString() + "/");

                    var sensors = new CameraSensors()
                    {
                        Time = frame / 30.0f,
                        Frame = frame,
                        Gx = g1,
                        Gy = g2,
                        Gz = g3,
                        Latitude = coordinate.Latitude,
                        Longitude = coordinate.Longitude
                    };

                    frame++;

                    list.Add(sensors);
                }, null);
            }

            return list;
        }

        private void ProcessElement(RiffParser p, int fourCC, int length)
        {
            bool continueReading = true;
            while (continueReading)
            {
                continueReading = p.ReadElement((a, b, c, d) => ProcessChunk(a, b, c, d),
                    (a, b, c) => ProcessElement(a, b, c));
            }
        }

        void ProcessChunk(RiffParser p, int fourcCC, int length, int paddedLength)
        {
            var type = RiffParser.FromFourCC(fourcCC);

            if (type == "idx1")
            {
                ReadIndexChunk(p, paddedLength);
            }
            else
            {
                p.SkipData(paddedLength);
            }
        }

        private void ReadIndexChunk(RiffParser p, int paddedLength)
        {
            while (paddedLength > 0)
            {
                var Identifier = new byte[4];
                Identifier[0] = p.ReadOneByte();
                Identifier[1] = p.ReadOneByte();
                Identifier[2] = p.ReadOneByte();
                Identifier[3] = p.ReadOneByte();

                var Flags = p.ReadOneInt();
                var Offset = p.ReadOneInt();
                var Length = p.ReadOneInt();

                var idstr = System.Text.Encoding.ASCII.GetString(Identifier);

                paddedLength -= 16;

                p.AddIndex(idstr, Flags, Offset, Length);
            }
        }
    }
}
