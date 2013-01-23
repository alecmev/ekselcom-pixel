using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MyFa
{
    [Serializable()]
    public class SymbolStorage : ISerializable
    {
        private int width;
        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        private int height;
        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        private bool[][] map;
        public bool[][] Map
        {
            get { return map; }
            set { map = value; }
        }

        private byte code;
        public byte Code
        {
            get { return code; }
            set { code = value; }
        }

        private int ascii;
        public int Ascii
        {
            get { return ascii; }
            set { ascii = value; }
        }

        private string font;
        public string Font
        {
            get { return font; }
            set { font = value; }
        }

        private float size;
        public float Size
        {
            get { return size; }
            set { size = value; }
        }

        public SymbolStorage(int widthNew, int heightNew, byte newCode, int newAscii, string newFont, float newSize)
        {
            Width = widthNew;
            Height = heightNew;
            Code = newCode;
            Ascii = newAscii;
            Font = newFont;
            Size = newSize;
            UpdateMap();
        }

        public SymbolStorage(SymbolStorage source)
        {
            Width = source.Width;
            Height = source.Height;
            Code = source.Code;
            Ascii = source.Ascii;
            Font = source.Font;
            Size = source.Size;
            UpdateMap();
            for (int i = 0; i < source.Height; ++i) Array.Copy(source.Map[i], Map[i], source.Map[i].Length);
        }

        public SymbolStorage(SerializationInfo info, StreamingContext context)
        {
            Width = (int)info.GetValue("Width", typeof(int));
            Height = (int)info.GetValue("Height", typeof(int));
            Map = (bool[][])info.GetValue("Map", typeof(bool[][]));
            Code = (byte)info.GetValue("Code", typeof(byte));
            Ascii = (int)info.GetValue("Ascii", typeof(int));
            Font = (string)info.GetValue("Font", typeof(string));
            Size = (float)info.GetValue("Size", typeof(float));
        }

        private void UpdateMap()
        {
            Map = new Boolean[Height][];
            for (Int32 i = 0; i < Height; ++i) Map[i] = new Boolean[Width];
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Width", Width);
            info.AddValue("Height", Height);
            info.AddValue("Map", Map);
            info.AddValue("Code", Code);
            info.AddValue("Ascii", Ascii);
            info.AddValue("Font", Font);
            info.AddValue("Size", Size);
        }
    }
}
