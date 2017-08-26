using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpi_rgb_led_matrix_sharp
{
    public class Font
    {
        public string Version { get; private set; }
        public string Name { get; private set; }
        public int Points { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        private int descent;
        private int ascent;
        private int defaultChar;
        private int totalChars;
        private BoundingBox boundingBox;
        private Dictionary<char, Glyph> glyphs;

        public Font(string filepath)
        {
            glyphs = new Dictionary<char, Glyph>();
            Glyph currentGlyph =null;            

            var lines = File.ReadAllLines(filepath);
            for (var i= 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var data = line.Split(' ');
                if (data.Length > 0)
                {
                    var declaration = data[0];
                    switch (declaration)
                    {
                        case "STARTFONT":
                            this.Version = data[1];
                            break;
                        case "FONT":
                            this.Name = data[1];
                            break;
                        case "FONTBOUNDINGBOX":
                            this.boundingBox = new BoundingBox() {                             
                                Width= int.Parse(data[1]),
                                Height= int.Parse(data[2]),
                                X= int.Parse(data[3]),
                                Y= int.Parse(data[4])
                            };
                            break;
                        case "SIZE":
                            this.Points = int.Parse(data[1]);
                            this.Width = int.Parse(data[2]);
                            this.Height = int.Parse(data[3]);
                            break;
                        case "FONT_DESCENT":
                            this.descent = int.Parse(data[1]);
                            break;
                        case "FONT_ASCENT":
                            this.ascent = int.Parse(data[1]);
                            break;
                        case "DEFAULT_CHAR":
                            this.defaultChar = int.Parse(data[1]);
                            break;
                        case "CHARS":
                            this.totalChars = int.Parse(data[1]);
                            break;
                        case "STARTCHAR":
                            var name = data[1];
                            currentGlyph = new Glyph(name);
                            break;
                        case "ENCODING":
                            currentGlyph.Code = int.Parse(data[1]);
                            currentGlyph.Char = (char)currentGlyph.Code;
                            glyphs[currentGlyph.Char] = currentGlyph;
                            break;
                        case "SWIDTH":
                            currentGlyph.ScaleWidthX = int.Parse(data[1]);
                            currentGlyph.ScaleWidthY = int.Parse(data[2]);
                            break;
                        case "DWIDTH":
                            currentGlyph.DeviceWidthX = int.Parse(data[1]);
                            currentGlyph.DeviceWidthY = int.Parse(data[2]);
                            break;
                        case "BBX":
                            currentGlyph.BoundingBox = new BoundingBox()
                            {
                                X = int.Parse(data[3]),
                                Y = int.Parse(data[4]),
                                Width = int.Parse(data[1]),
                                Height = int.Parse(data[2])
                            };
                            break;
                        case "BITMAP":
                            i++;
                            var buffer = new List<byte>();
                            while (lines[i] != "ENDCHAR")                            
                            {
                                var barray = convertToByteArray(lines[i]);
                                currentGlyph.BitWidth = barray.Length * 8;
                                foreach (var b in barray)
                                    buffer.Add(b);
                                i++;
                            }
                            currentGlyph.Data = new BitArray(buffer.ToArray());
                            break;
                    }
                }
            }
        }

        public static byte[] convertToByteArray(string hexData)
        {
            hexData = hexData.Trim();
            byte[] ret = new byte[(int)Math.Ceiling((double)hexData.Length /2)];
            string h = null;
            int r = 0;
            byte b = 0;
            int o = 0;
            for (var i=0; i<hexData.Length; i += 2)
            {                
                r = hexData.Length - (i + 1);
                if (r > 1) h=hexData.Substring(i, 2);
                else h=hexData.Substring(i, 1);
                b = byte.Parse(h, NumberStyles.HexNumber);
                ret[o++] = b;
            }
            return ret;
        }

        internal IEnumerable<Point> RenderText(int x, int y, string text, int kerningBias=0)
        {
            int width = 0;
            int height = 0;
            for (var i = 0; i < text.Length; i++)
            {
                var charCode = text[i];
                if (charCode == '\n')
                {
                    width = 0;
                    height += boundingBox.Height;
                    continue;
                }
                var glyphData = this.glyphs[charCode];

                for (var yi = 0; yi < boundingBox.Height; yi++)
                {
                    for (var xi = 0; xi < glyphData.BitWidth; xi++)
                    {                      
                        var offset = (yi+1) * glyphData.BitWidth - xi;
                        if (offset < glyphData.Data.Count) {
                            if (glyphData.Data[offset]) {                                
                                yield return new Point(
                                    x + xi + glyphData.BoundingBox.X + width - glyphData.BoundingBox.Width,
                                    y + yi + glyphData.BoundingBox.Y + descent + height);
                            }
                        }
                    }
                }
                width += glyphData.BoundingBox.Width+ glyphData.BoundingBox.X + kerningBias;
            }
        }
    }

    internal class Glyph
    {
        public Glyph(string name)
        {
            this.Name = name;
        }

        public string Name { get; internal set; }
        public int Code { get; internal set; }
        public char Char { get; internal set; }
        public int ScaleWidthX { get; internal set; }
        public int ScaleWidthY { get; internal set; }
        public int DeviceWidthX { get; internal set; }
        public int DeviceWidthY { get; internal set; }
        public BoundingBox BoundingBox { get; internal set; }
        public BitArray Data { get; internal set; }
        public int BitWidth { get; internal set; }
    }

    public struct BoundingBox
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    public struct Point
    {
        public Point (int x, int y)
        {
            X = x; Y = y;
        }
        public int X;
        public int Y;
    }
}
