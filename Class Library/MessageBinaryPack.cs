using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyFa.Properties;
using System.IO;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyFa
{
    public class FAT
    {
        public List<UInt32> Addresses;

        public UInt32 Weight
        {
            get { return (UInt32)Addresses.Count * 4; }
        }

        public FAT()
        {
            Addresses = new List<uint>();
        }

        public void Write(Stream stream)
        {
            BinaryWriter binary = new BinaryWriter(stream);
            foreach (UInt32 tmp in Addresses) binary.Write(tmp);
        }
    }

    public class Description
    {
		public UInt32 FileLength = 116;
        public UInt16 DescriptionLength = 84;
        public Byte DisplayType = 0;
        public Byte DisplayWidth = 128;
        public Byte DisplayHeight = 8;
        public UInt32 FlashWriteAddress = 0;
        public UInt32 FlashSize = 1081344;
        public Byte[] CreationDate; // must be 8
        public Byte[] Comment; // must be 28
        public Byte[] Revision; // must be 8
        public Byte[] Extra = new Byte[17];

        public FAT GeneralFAT; // 4 entries

        public FAT FontFAT; // max 16 entries
        public FAT LineFAT; // max 1000 entries
        public FAT DestinationFAT; // max 1000 entries
        public FAT StopFAT; // max 10000 entries

        public List<Font> Fonts;
        public List<Message> Lines;
        public int LastLine = 0;
        public int CountLine = 0;
        public List<Message> Destinations;
        public int LastDestination = 0;
        public int CountDestination = 0;
        public List<Message> Stops;
        public int LastStop = 0;
        public int CountStop = 0;

        public Description(MessageCollection lines, MessageCollection destinations, MessageCollection stops)
        {
            CreationDate = System.Text.Encoding.ASCII.GetBytes(DateTime.Now.ToString("ddMMyyyy"));
            Comment = System.Text.Encoding.ASCII.GetBytes("VALDS & EKSELCOM RIGA LATVIA");
            Revision = System.Text.Encoding.ASCII.GetBytes("REV 1.01");

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFaPixel\Config\Extra.txt";
            if (File.Exists(path))
            {
                StreamReader stream = new StreamReader(path);
                String str = stream.ReadLine();
                stream.Close();
                if (str.Length == (Extra.Length * 3 - 1)) for (int i = 0; i < Extra.Length; ++i) byte.TryParse(str.Substring(i * 3, 2), System.Globalization.NumberStyles.HexNumber, null, out Extra[i]);
            }

            GeneralFAT = new FAT();

            FontFAT = new FAT();
            LineFAT = new FAT();
            DestinationFAT = new FAT();
            StopFAT = new FAT();

            Fonts = new List<Font>();
            Lines = new List<Message>(1000);
            for (int i = 0; i < 1000; ++i) Lines.Add(null);
            Destinations = new List<Message>(1000);
            for (int i = 0; i < 1000; ++i) Destinations.Add(null);
            Stops = new List<Message>(2000);
            for (int i = 0; i < 2000; ++i) Stops.Add(null);

            Message tmpMessage;
            int tmpFont;
            
            foreach (MessageStorage message in lines.Messages)
            {
                if (message != null)
                {
                    ++CountLine;
                    tmpMessage = new Message();
                    foreach (PageStorage page in message.Pages)
                    {
                        if (page != null)
                        {
                            tmpFont = FontByName(page.Strings[0].Font);
                            if (tmpFont < 0)
                            {
                                Fonts.Add(new Font(page.Strings[0].Font));
                                tmpFont = Fonts.Count - 1;
                            }
                            tmpMessage.Items.Add(new Page(tmpFont, page.Strings[0].Indent, page.Strings[0].X, page.Strings[0].Width, page.Strings[0].Shift, page.Strings[0].Text, Fonts[tmpFont].Ascii, message.Loop, 0));
                        }
                    }
                    LastLine = lines.Messages.IndexOf(message);
                    Lines[LastLine] = tmpMessage;
                }
            }

            foreach (MessageStorage message in destinations.Messages)
            {
                if (message != null)
                {
                    ++CountDestination;
                    tmpMessage = new Message();
                    foreach (PageStorage page in message.Pages)
                    {
                        if (page != null)
                        {
                            tmpFont = FontByName(page.Strings[0].Font);
                            if (tmpFont < 0)
                            {
                                Fonts.Add(new Font(page.Strings[0].Font));
                                tmpFont = Fonts.Count - 1;
                            }
                            tmpMessage.Items.Add(new Page(tmpFont, page.Strings[0].Indent, page.Strings[0].X, page.Strings[0].Width, page.Strings[0].Shift, page.Strings[0].Text, Fonts[tmpFont].Ascii, (tmpMessage.Items.Count > 0 ? message.Loop : false), (tmpMessage.Items.Count > 0 || message.Pages.Last() != null ? page.Time : 0)));
                        }
                    }
                    LastDestination = destinations.Messages.IndexOf(message);
                    Destinations[LastDestination] = tmpMessage;
                }
            }

            foreach (MessageStorage message in stops.Messages)
            {
                if (message != null)
                {
                    ++CountStop;
                    tmpMessage = new Message();
                    foreach (PageStorage page in message.Pages)
                    {
                        if (page != null)
                        {
                            tmpFont = FontByName(page.Strings[0].Font);
                            if (tmpFont < 0)
                            {
                                Fonts.Add(new Font(page.Strings[0].Font));
                                tmpFont = Fonts.Count - 1;
                            }
                            tmpMessage.Items.Add(new Page(tmpFont, page.Strings[0].Indent, page.Strings[0].X, page.Strings[0].Width, page.Strings[0].Shift, page.Strings[0].Text, Fonts[tmpFont].Ascii, (tmpMessage.Items.Count > 0 ? message.Loop : false), (tmpMessage.Items.Count > 0 || message.Pages.Last() != null ? page.Time : 0)));
                        }
                    }
                    LastStop = stops.Messages.IndexOf(message);
                    Stops[LastStop] = tmpMessage;
                }
            }
			
			GeneralFAT.Addresses.Add(FileLength);
			FileLength += (UInt32)(Fonts.Count * 4);
			GeneralFAT.Addresses.Add(FileLength);
            FileLength += (UInt32)((LastLine + 1) * 4);
			GeneralFAT.Addresses.Add(FileLength);
            FileLength += (UInt32)((LastDestination + 1) * 4);
			GeneralFAT.Addresses.Add(FileLength);
            FileLength += (UInt32)((LastStop + 1) * 4);
			
			foreach (Font tmp in Fonts)
            {
				FontFAT.Addresses.Add(FileLength);
                FileLength += tmp.Weight;
			}
			
            foreach (Message tmp in Lines)
            {
                if (tmp != null)
                {
                    LineFAT.Addresses.Add(FileLength);
                    FileLength += tmp.Weight;
                    if (Lines.IndexOf(tmp) == LastLine) break;
                }
                else LineFAT.Addresses.Add(0);
            }
			
            foreach (Message tmp in Destinations)
            {
                if (tmp != null)
                {
                    DestinationFAT.Addresses.Add(FileLength);
                    FileLength += tmp.Weight;
                    if (Destinations.IndexOf(tmp) == LastDestination) break;
                }
                else DestinationFAT.Addresses.Add(0);
            }
			
            foreach (Message tmp in Stops)
            {
                if (tmp != null)
                {
                    StopFAT.Addresses.Add(FileLength);
                    FileLength += tmp.Weight;
                    if (Stops.IndexOf(tmp) == LastStop) break;
                }
                else StopFAT.Addresses.Add(0);
            }
        }

        public void Write(Stream stream)
        {
            BinaryWriter binary = new BinaryWriter(stream);
			
            binary.Write(FileLength);
            binary.Write(DescriptionLength);
            binary.Write((UInt32)DescriptionLength);
            binary.Write((UInt16)16);
            binary.Write(DisplayType);
            binary.Write(DisplayWidth);
            binary.Write(DisplayHeight);
            binary.Write(FlashWriteAddress);
            binary.Write(FlashSize);
            binary.Write(CreationDate);
            binary.Write(Comment);
            binary.Write(Revision);
            binary.Write(Extra);

            binary.Write(GeneralFAT.Addresses[0]);
			binary.Write((UInt32)Fonts.Count * 4);
			binary.Write(GeneralFAT.Addresses[1]);
            binary.Write((UInt32)(LastLine + 1) * 4);
			binary.Write(GeneralFAT.Addresses[2]);
            binary.Write((UInt32)(LastDestination + 1) * 4);
			binary.Write(GeneralFAT.Addresses[3]);
            binary.Write((UInt32)(LastStop + 1) * 4);
			
            FontFAT.Write(stream);
            LineFAT.Write(stream);
            DestinationFAT.Write(stream);
            StopFAT.Write(stream);

            foreach (Font tmp in Fonts) tmp.Write(stream);
            foreach (Message tmp in Lines)
            {
                if (tmp != null)
                {
                    tmp.Write(stream);
                    if (Lines.IndexOf(tmp) == LastLine) break;
                }
            }
            foreach (Message tmp in Destinations)
            {
                if (tmp != null)
                {
                    tmp.Write(stream);
                    if (Destinations.IndexOf(tmp) == LastDestination) break;
                }
            }
            foreach (Message tmp in Stops)
            {
                if (tmp != null)
                {
                    tmp.Write(stream);
                    if (Stops.IndexOf(tmp) == LastStop) break;
                }
            }

            //binary.Write((Byte)255);
        }

        public int FontByName(string name)
        {
            foreach (Font tmp in Fonts) if (tmp.Name == name) return Fonts.IndexOf(tmp);
            return -1;
        }
    }

    public class Font
    {
        public string Name;
        public Byte Width;
        public Byte Height = 8;
        public Byte Type = 0;
        public Byte Header = 2;
        public UInt16 Ascii;
        public Byte[,] Map;

        public UInt32 Weight
        {
            get { return (UInt32)(Width + Header) * 256 + 6; }
        }

        public Font(string name)
        {
            Name = name;
            Stream stream = File.Open(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFaPixel\Fonts\" + Name + @".mfpf", FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();
            Collection<SymbolStorage> font = (Collection<SymbolStorage>)bformatter.Deserialize(stream);
            stream.Close();

            Width = (Byte)font[0].Width;
            Ascii = (UInt16)font[0].Ascii;
            Map = new Byte[256, Width + Header];
            Byte tmpFirst, tmpLast;

            for (int i = 0; i < 256; ++i)
            {
                tmpFirst = 255;
                tmpLast = 0;
                for (Byte x = 0; x < Width; ++x)
                {
                    for (Byte y = 0; y < Height; ++y)
                    {
                        if (font[i].Map[y][x])
                        {
                            Map[i, x] += (Byte)Math.Pow(2, y);
                            if (x < tmpFirst) tmpFirst = x;
                            if (x > tmpLast) tmpLast = x;
                        }
                    }
                }
                if (tmpFirst <= tmpLast)
                {
                    Map[i, Width] = tmpFirst;
                    Map[i, Width + 1] = (Byte)(tmpLast - tmpFirst + 1);
                }
                else
                {
                    Map[i, Width] = 0;
                    Map[i, Width + 1] = 0;
                }
            }
        }

        public void Write(Stream stream)
        {
            BinaryWriter binary = new BinaryWriter(stream);
            binary.Write(Width);
            binary.Write(Height);
            binary.Write(Type);
            binary.Write(Header);
            binary.Write(Ascii);
            for (int i = 0; i < 256; ++i)
            {
                binary.Write(Map[i, Width]);
                binary.Write(Map[i, Width + 1]);
                for (int x = 0; x < Width; ++x)
                {
                    binary.Write(Map[i, x]);
                }
            }
        }
    }

    public class Page
    {
        public Byte Font;
        public Byte Indent;
        public Byte LocationX;
        public Byte SizeX;
        public Byte ShiftX;
        public Byte Length
        {
            get { return (Byte)Text.Length; }
        }
        public Byte[] Text;
        public Byte Loop;
        public Byte Time;

        public Page(int font, int indent, int locationX, int sizeX, int shiftX, string text, int ascii, bool loop, int time)
        {
            Font = (Byte)font;
            Indent = (Byte)indent;
            LocationX = (Byte)locationX;
            SizeX = (Byte)sizeX;
            ShiftX = (Byte)shiftX;
            Text = System.Text.Encoding.GetEncoding(ascii).GetBytes(text);
            Loop = (Byte)(loop ? 1 : 0);
            Time = (Byte)time;
        }

        public void Write(Stream stream)
        {
            BinaryWriter binary = new BinaryWriter(stream);
            binary.Write(Font);
            binary.Write(Indent);
            binary.Write(LocationX);
            binary.Write(SizeX);
            binary.Write(ShiftX);
            binary.Write(Length);
            binary.Write(Text);
            binary.Write(Time);
            binary.Write(Loop);
        }
    }

    public class Message
    {
        public List<Page> Items;

        public UInt32 Weight
        {
            get
            {
                UInt32 tmp = (UInt32)Items.Count * 9;
                foreach (Page tmpPage in Items) tmp += tmpPage.Length;
                return tmp;
            }
        }

        public Message()
        {
            Items = new List<Page>();
        }

        public void Write(Stream stream)
        {
            BinaryWriter binary = new BinaryWriter(stream);
            for (int i = 0; i < Items.Count - 1; ++i)
            {
                Items[i].Write(stream);
                binary.Write((Byte)10);
            }
            Items[Items.Count - 1].Write(stream);
            binary.Write((Byte)13);
        }
    }
}
