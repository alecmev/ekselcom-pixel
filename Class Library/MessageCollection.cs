using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace MyFa
{
    [Serializable()]
    public class MessageCollection : ISerializable
    {
        private string displayType;
        public string DisplayType
        {
            get { return displayType; }
            set { displayType = value; }
        }

        private List<MessageStorage> messages;
        public List<MessageStorage> Messages
        {
            get { return messages; }
            set { messages = value; }
        }

        public MessageCollection(string newDisplayType, int newCapacity)
        {
            DisplayType = newDisplayType;
            Messages = new List<MessageStorage>(newCapacity);
            Messages.AddRange(new MessageStorage[newCapacity]);
        }

        public MessageCollection(MessageCollection source)
        {
            DisplayType = source.DisplayType;
            Messages = new List<MessageStorage>(source.Messages);
        }

        public MessageCollection(SerializationInfo info, StreamingContext context)
        {
            DisplayType = (string)info.GetValue("DisplayType", typeof(string));
            Messages = (List<MessageStorage>)info.GetValue("Messages", typeof(List<MessageStorage>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("DisplayType", DisplayType);
            info.AddValue("Messages", Messages);
        }
    }
}