using Fly.Blackbox.Console.Riff;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fly.Blackbox.Console.FileFixer
{
    public class FileFixer
    {
        int JVIDINDEX = -1;

        public void Fix(string input, string output)
        {
            var realoutname = output;

            if(File.Exists(realoutname))
            {
                File.Delete(realoutname);
                Thread.Sleep(1000);
            }

            File.Copy(input, realoutname);

            var parser = new RiffParser();
            parser.OpenFile(realoutname);

            bool continueReading = true;
            while (continueReading)
            {
                continueReading = parser.ReadElement((a, b, c, d) => ProcessChunk(a, b, c, d), (a, b, c) => ProcessElement(a, b, c));
            }

            parser.CloseFile();

            using (var f = File.OpenWrite(realoutname))
            {
                foreach (var item in parser.Bookmarks)
                {
                    if (item.Key.StartsWith("JVID"))
                    {
                        f.Position = item.Value;
                        f.WriteByte((byte)'H');
                        f.WriteByte((byte)'2');
                        f.WriteByte((byte)'6');
                        f.WriteByte((byte)'4');
                    }
                    else if (item.Key.StartsWith("JAVE"))
                    {
                        f.Position = item.Value;
                        f.WriteByte((byte)'a');
                        f.WriteByte((byte)'u');
                        f.WriteByte((byte)'d');
                        f.WriteByte((byte)'s');
                        f.WriteByte((byte)'W');
                    }
                }
            }
        }

        private void ProcessElement(RiffParser p, int fourCC, int length)
        {
            bool continueReading = true;
            while (continueReading)
            {
                continueReading = p.ReadElement((a, b, c, d) => ProcessChunk(a, b, c, d), (a, b, c) => ProcessElement(a, b, c));
            }
        }

        bool readNextSTRF = false;

        void ProcessChunk(RiffParser p, int fourcCC, int length, int paddedLength)
        {
            var type = RiffParser.FromFourCC(fourcCC);
            

            if (type == "strh")
            {
                var dataType = new byte[4];
                dataType[0] = p.ReadOneByte();
                dataType[1] = p.ReadOneByte();
                dataType[2] = p.ReadOneByte();
                dataType[3] = p.ReadOneByte();
                var dataTypeStr = System.Text.Encoding.ASCII.GetString(dataType);

                var dataHandler = new byte[4];
                dataHandler[0] = p.ReadOneByte();
                dataHandler[1] = p.ReadOneByte();
                dataHandler[2] = p.ReadOneByte();
                dataHandler[3] = p.ReadOneByte();
                var dataHandlerStr = System.Text.Encoding.ASCII.GetString(dataHandler);
                                
                if (dataTypeStr == "vids" && dataHandlerStr == "JVID")
                {
                    JVIDINDEX++;
                    p.AddBookmark("JVID" + JVIDINDEX, -4);
                    readNextSTRF = true;
                }
                else if (dataTypeStr == "vids" && dataHandlerStr == "JAVE")
                {
                    JVIDINDEX++;
                    p.AddBookmark("JAVE" + JVIDINDEX, -8);
                    readNextSTRF = false;
                }
                else
                {
                    readNextSTRF = false;
                }

                p.SkipData(paddedLength - 8);
            }
            else if ((type == "strf") && (readNextSTRF))
            {
                var biSize = p.ReadOneInt();
                var biWidth = p.ReadOneInt();
                var biHeight = p.ReadOneInt();
                var biPlanes1 = p.ReadOneByte();
                var biPlanes2 = p.ReadOneByte();
                var biBitCount1 = p.ReadOneByte();
                var biBitCount2 = p.ReadOneByte();
                var biCompression = p.ReadOneInt();

                JVIDINDEX++;
                p.AddBookmark("JVID" + JVIDINDEX, -4);

                var biSizeImage = p.ReadOneInt();
                var biXPelsPerMeter = p.ReadOneInt();
                var biYPelsPerMeter = p.ReadOneInt();
                var biClrUsed = p.ReadOneInt();
                var biClrImportant = p.ReadOneInt();

                readNextSTRF = false;
            }
            else
            {
                p.SkipData(paddedLength);

                readNextSTRF = false;
            }
        }
    }
}
