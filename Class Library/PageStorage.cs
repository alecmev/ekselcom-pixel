using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace MyFa
{
    [Serializable()]
    public class PageStorage : ISerializable
    {
        private int time;
        public int Time
        {
            get { return time; }
            set { time = value > 1 ? value : 1; }
        }

        private List<StringStorage> strings;
        public List<StringStorage> Strings
        {
            get { return strings; }
            set { strings = value; }
        }

        public PageStorage(int newTime, int newCapacity)
        {
            Time = newTime;
            Strings = new List<StringStorage>(newCapacity);
            Strings.AddRange(new StringStorage[newCapacity]);
        }

        public PageStorage(PageStorage source)
        {
            Time = source.Time;
            Strings = new List<StringStorage>(source.Strings);
        }

        public PageStorage(SerializationInfo info, StreamingContext context)
        {
            Time = (int)info.GetValue("Time", typeof(int));
            Strings = (List<StringStorage>)info.GetValue("Strings", typeof(List<StringStorage>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Time", Time);
            info.AddValue("Strings", Strings);
        }
    }
}