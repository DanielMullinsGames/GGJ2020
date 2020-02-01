using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Kilt.Extensions
{
    public static class StringExtension
    {
        public static string SplitCamelCase(this string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.CultureInvariant).Trim();
        }

        public static string ReplaceFirst(this string p_text, string p_oldText, string p_newText)
        {
            try
            {
                p_oldText = p_oldText != null ? p_oldText : "";
                p_newText = p_newText != null ? p_newText : "";
                p_text = p_text != null ? p_text : "";

                int v_pos = string.IsNullOrEmpty(p_oldText) ? -1 : p_text.IndexOf(p_oldText);
                if (v_pos < 0)
                    return p_text;
                string v_result = p_text.Substring(0, v_pos) + p_newText + p_text.Substring(v_pos + p_oldText.Length);
                return v_result;
            }
            catch { }
            return p_text;
        }

        public static string ReplaceLast(this string p_text, string p_oldText, string p_newText)
        {
            try
            {
                p_oldText = p_oldText != null ? p_oldText : "";
                p_newText = p_newText != null ? p_newText : "";
                p_text = p_text != null ? p_text : "";

                int v_pos = string.IsNullOrEmpty(p_oldText) ? -1 : p_text.LastIndexOf(p_oldText);
                if (v_pos < 0)
                    return p_text;

                string v_result = p_text.Remove(v_pos, p_oldText.Length).Insert(v_pos, p_newText);
                return v_result;
            }
            catch { }
            return p_text;
        }

        public static bool IsUrl(string p_string)
        {
            if (!string.IsNullOrEmpty(p_string))
            {
                string v_pattern = @"((ht|f)tp(s?)\:\/\/)?www[.][0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?";
                if (Regex.IsMatch(p_string, v_pattern))
                    return true;
            }
            return false;
        }

        public static string FirstCharToUpper(this string p_input)
        {
            if (p_input == null)
                return "";

            if (p_input.Length > 1)
                return char.ToUpper(p_input[0]) + p_input.Substring(1);

            return p_input.ToUpper();
        }
    }
}
