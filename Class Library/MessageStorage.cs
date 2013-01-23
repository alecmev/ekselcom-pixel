using System;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace MyFa
{
    [Serializable()]
    public class MessageStorage : ISerializable
    {
        private bool loop;
        public bool Loop
        {
            get { return loop; }
            set { loop = value; }
        }

        private List<PageStorage> pages;
        public List<PageStorage> Pages
        {
            get { return pages; }
            set { pages = value; }
        }

        public MessageStorage(bool newLoop, int newCapacity)
        {
            Loop = newLoop;
            Pages = new List<PageStorage>(newCapacity);
            Pages.AddRange(new PageStorage[newCapacity]);
        }

        public MessageStorage(MessageStorage source)
        {
            Loop = source.Loop;
            Pages = new List<PageStorage>(source.Pages);
        }

        public MessageStorage(SerializationInfo info, StreamingContext context)
        {
            Loop = (bool)info.GetValue("Loop", typeof(bool));
            Pages = (List<PageStorage>)info.GetValue("Pages", typeof(List<PageStorage>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Loop", Loop);
            info.AddValue("Pages", Pages);
        }
    }
}
