using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows;

namespace MyFa
{
    [Serializable()]
    public class StringStorage : ISerializable
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private Int32Rect bounds;
        public Int32Rect Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }

        public int X
        {
            get { return bounds.X; }
            set { bounds.X = value; }
        }
        public int Y
        {
            get { return bounds.Y; }
            set { bounds.Y = value; }
        }
        public int Width
        {
            get { return bounds.Width; }
            set { bounds.Width = value; }
        }
        public int Height
        {
            get { return bounds.Height; }
            set { bounds.Height = value; }
        }

        private string text;
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        private string font;
        public string Font
        {
            get { return font; }
            set { font = value; }
        }

        private int shift;
        public int Shift
        {
            get { return shift; }
            set { shift = value; }
        }

        private int vShift;
        public int VShift
        {
            get { return vShift; }
            set { vShift = value; }
        }

        private int indent;
        public int Indent
        {
            get { return indent; }
            set { indent = value > 0 ? value : 0; }
        }

        private bool running;
        public bool Running
        {
            get { return running; }
            set { running = value; }
        }

        public StringStorage(string newName, int newX, int newY, int newWidth, int newHeight, string newText, string newFont, int newShift, int newVShift, int newIndent, bool newRunning)
        {
            Name = newName;
            Bounds = new Int32Rect(newX, newY, newWidth, newHeight);
            Text = newText;
            Font = newFont;
            Shift = newShift;
            VShift = newVShift;
            Indent = newIndent;
            Running = newRunning;
        }

        public StringStorage(StringStorage source)
        {
            Name = source.Name;
            Bounds = new Int32Rect(source.X, source.Y, source.Width, source.Height);
            Text = "";
            Font = source.Font;
            Shift = source.Shift;
            VShift = source.VShift;
            Indent = source.Indent;
            Running = source.Running;
        }

        public StringStorage(SerializationInfo info, StreamingContext context)
        {
            Name = (string)info.GetValue("Name", typeof(string));
            Bounds = (Int32Rect)info.GetValue("Bounds", typeof(Int32Rect));
            Text = (string)info.GetValue("Text", typeof(string));
            Font = (string)info.GetValue("Font", typeof(string));
            Shift = (int)info.GetValue("Shift", typeof(int));
            VShift = (int)info.GetValue("VShift", typeof(int));
            Indent = (int)info.GetValue("Indent", typeof(int));
            Running = (bool)info.GetValue("Running", typeof(bool));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("Bounds", Bounds);
            info.AddValue("Text", Text);
            info.AddValue("Font", Font);
            info.AddValue("Shift", Shift);
            info.AddValue("VShift", VShift);
            info.AddValue("Indent", Indent);
            info.AddValue("Running", Running);
        }
    }
}
