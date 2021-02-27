using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    public class ContentMessage : IMessage
    {

        public ContentType Type { get; set; }

        public ResourceType ResourceType { get; set; }

        public byte[] Id { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }


        override public string ToString()
        {
            if (Type == ContentType.RESSOURCE)
            {
                return ResourceType + "";
            }
            else if (Type == ContentType.PLAYER)
            {
                if (Name != null)
                {
                    return "Player: " + " " + Name;
                }
                else
                {
                    return "Player: " + " unknown Name";
                }
            }
            else if (Type == ContentType.RESSOURCE)
            {
                if (Name != null)
                {
                    return "Quest: " + " " + Name;
                }
                else
                {
                    return "Quest: " + " unknown Name";
                }

            }
            return null;
        }
    }
}
