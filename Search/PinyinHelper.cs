using System.Text;
using NPinyin;

namespace RecipaediaEX.Search {
    public static class PinyinHelper {
        public static string ToFullPinyin(string text) {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            StringBuilder builder = new();
            foreach (char c in text) {
                if (c <= 127) {
                    if (char.IsLetterOrDigit(c)) builder.Append(char.ToLowerInvariant(c));
                    continue;
                }
                string py = Pinyin.GetPinyin(c);
                if (string.IsNullOrEmpty(py)) continue;
                foreach (char pc in py) {
                    if (char.IsLetter(pc)) builder.Append(char.ToLowerInvariant(pc));
                }
            }
            return builder.ToString();
        }

        public static string ToInitials(string text) {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            StringBuilder builder = new();
            foreach (char c in text) {
                if (c <= 127) {
                    if (char.IsLetter(c)) builder.Append(char.ToLowerInvariant(c));
                    continue;
                }
                string py = Pinyin.GetInitials(c.ToString());
                if (string.IsNullOrEmpty(py)) continue;
                builder.Append(char.ToLowerInvariant(py[0]));
            }
            return builder.ToString();
        }

        public static bool IsAsciiLetters(string text) {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text) {
                if (c > 127 || !char.IsLetter(c)) return false;
            }
            return true;
        }
    }
}
