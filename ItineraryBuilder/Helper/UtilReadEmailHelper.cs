using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Text.RegularExpressions;
namespace ItineraryBuilder.Helpers
{
    public class UtilReadEmailHelper
    {
        #region Fields/Properties

        string _template;

        public enum TemplateTags
        {
            From = 1,
            Subject,
            Body
        }

        /// <summary>The Tag and Value collection that need to set on the email Template</summary>
        public Dictionary<string, string> TagAndValues { get; set; }

        ///<summary><![CDATA[Get: Value in <from/>...<from/> tag.]]></summary>
        public string From
        {
            get
            {
                return ReadTemplate(_template, TemplateTags.From);
            }
        }

        ///<summary><![CDATA[Get: Value in <subject/>...<subject/> tag.]]></summary>
        public string Subject
        {
            get
            {
                return ReadTemplate(_template, TemplateTags.Subject);
            }
        }

        string Body
        {
            get
            {
                return ReadTemplate(_template, TemplateTags.Body);
            }
        }

        #endregion

        //ctor
        public UtilReadEmailHelper(string templateData)
        {
            _template = templateData;
            TagAndValues = new Dictionary<string, string>();
        }

        //Public methods.
        public string GetSubject()
        {
            return this.ReplaceDynamicSubject(TagAndValues, true);
        }

        /// <summary><![CDATA[Return the <body>...</body> tag text with replaced {{tag..}} with dynamic data.]]></summary>
        public string GetEmailTemplateBodyTxt()
        {
            return this.ReplaceDynamicData(TagAndValues, true);
        }

        //Private methods.
        string ReadTemplate(string templateText, TemplateTags tagToRead)
        {
            Regex rgx = null;
            string tagStr = tagToRead.ToString().ToLower();
            string data = string.Empty;
            switch (tagToRead)
            {
                case TemplateTags.From:
                case TemplateTags.Subject:
                    rgx = new Regex(string.Format(@"(\<{0}\>).*?(\</{0}\>)", tagStr), RegexOptions.IgnoreCase);
                    data = rgx.Match(templateText, 0).ToString();
                    break;
                case TemplateTags.Body:
                    int start = templateText.IndexOf(string.Format("<{0}>", tagStr));
                    int end = templateText.IndexOf(string.Format("</{0}>", tagStr));
                    data = templateText.Substring(start, end - start);
                    break;
                default:
                    data = string.Empty;
                    break;
            }

            return data
                .Replace(string.Format("<{0}>", tagToRead), "").Replace(string.Format("</{0}>", tagToRead), "")
                .Replace(string.Format("<{0}>", tagToRead.ToString().ToLower()), "").Replace(string.Format("</{0}>", tagToRead.ToString().ToLower()), "");
        }

        string ReplaceDynamicSubject(Dictionary<string, string> tagValuePair, bool autoAddTheBracesToTag)
        {
            string returnValue = Subject;
            foreach (var item in tagValuePair)
            {
                returnValue = ReplaceDynamicData(returnValue, item.Key, item.Value, autoAddTheBracesToTag);

            }
            return returnValue;
        }

        string ReplaceDynamicData(Dictionary<string, string> tagValuePair, bool autoAddTheBracesToTag)
        {
            string returnValue = Body;
            foreach (var item in tagValuePair)
            {
                returnValue = ReplaceDynamicData(returnValue, item.Key, item.Value, autoAddTheBracesToTag);

            }
            return returnValue;
        }

        string ReplaceDynamicData(string rawData, string tag, string value, bool autoAddTheBracesToTag)
        {
            if (autoAddTheBracesToTag)
            {
                tag = "{{" + tag + "}}";
            }

            Regex rgx = new Regex(@"(\{{2}).*?(\}{2})");
            return rawData.Replace(tag, value);
        }
    }
}
