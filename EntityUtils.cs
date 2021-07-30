using System;
using System.Linq;
using System.Text;

namespace EntityGen
{
    public static class EntityUtils
    {
        public static string CaptalizeString(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;
            
            StringBuilder builder = new StringBuilder(field.ToLower());

            int underlineIndex;
            while((underlineIndex = builder.IndexOf("_", 0, true)) != -1)
            {
                if (underlineIndex == builder.Length)
                    continue;

                builder[underlineIndex + 1] = char.ToUpper(builder[underlineIndex + 1]);
                builder.Replace("_", "", underlineIndex, 1);
            }

            builder[0] = char.ToUpper(builder[0]);

            return builder.ToString();
        }
        
        static int IndexOf(this StringBuilder sb, string value, int startIndex, bool ignoreCase)
        {            
            int index;
            int length = value.Length;
            int maxSearchLength = (sb.Length - length) + 1;

            if (ignoreCase)
            {
                for (int i = startIndex; i < maxSearchLength; ++i)
                {
                    if (char.ToLower(sb[i]) == char.ToLower(value[0]))
                    {
                        index = 1;
                        while ((index < length) && (char.ToLower(sb[i + index]) == char.ToLower(value[index])))
                            ++index;

                        if (index == length)
                            return i;
                    }
                }

                return -1;
            }

            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (sb[i] == value[0])
                {
                    index = 1;
                    while ((index < length) && (sb[i + index] == value[index]))
                        ++index;

                    if (index == length)
                        return i;
                }
            }

            return -1;
        }
    }
}