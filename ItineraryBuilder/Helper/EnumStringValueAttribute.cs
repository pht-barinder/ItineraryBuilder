using System;

namespace ItineraryBuilder.Util
{
    /*Author: Ankit*/
    public class UtilEnumStringValueAttribute : Attribute
    {
        public string LargeDescription { get; protected set; }

        public string SmallDescription { get; protected set; }

        public string KeyValue { get; protected set; }

        public string KeyValue2 { get; protected set; }

        public string Key { get; protected set; }

        public UtilEnumStringValueAttribute(string largeDescription)
        {
            LargeDescription = largeDescription;
        }

        public UtilEnumStringValueAttribute(string largeDescription, string smallDescription)
        {
            LargeDescription = largeDescription;
            SmallDescription = smallDescription;
        }
    }
}
