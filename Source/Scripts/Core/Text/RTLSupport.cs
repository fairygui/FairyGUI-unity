/********************************************************************
				Copyright (c) 2018, Tadpole Studio
					All rights reserved
 
	文件名称：	RTL.cs
	说	明：		RTL字符转换
    
	版	本：		1.00
	时	间：		2018/2/24 9:02:12
	作	者：		AQ
	概	述：		新建

*********************************************************************/
using System.Collections.Generic;
using System.Text;

namespace FairyGUI
{
    internal enum CharState
    {
        init,
        middle,
        final,
        isolated,
        number
    }

    public class RTLSupport
    {
        private static Dictionary<int, char> final;
        private static Dictionary<int, char> init;
        private static bool isCharsInitialized = false;
        private static Dictionary<int, char> isolate;
        private static Dictionary<int, char> middle;
        private static Dictionary<int, char> numbers;

        public static bool IsArabicLetter(char ch)
        {
            if (ch >= 0x600 && ch <= 0x6ff)
                return true;

            if (ch >= 0x750 && ch <= 0x77f)
                return true;

            if (ch >= 0xfb50 && ch <= 0xfc3f)
                return true;

            if (ch >= 0xfe70 && ch <= 0xfefc)
                return true;

            // ﷲ 添加真主字符 [2018/3/13 19:03:35 --By aq_1000]
//             if (ch == 0xfdf2)
//                 return true;

            return false;
        }

        public static bool ContainsArabicLetters(string text)
        {
            foreach (char character in text.ToCharArray())
            {
                if (character >= 0x600 && character <= 0x6ff)
                    return true;

                if (character >= 0x750 && character <= 0x77f)
                    return true;

                if (character >= 0xfb50 && character <= 0xfc3f)
                    return true;

                if (character >= 0xfe70 && character <= 0xfefc)
                    return true;

                // ﷲ 添加真主字符 [2018/3/13 19:03:35 --By aq_1000]
//                 if (character == 0xfdf2)
//                     return true;
            }
            return false;
        }

        private static bool CheckSeparator(char input)
        {
            if ((((input != ' ') && (input != '!')) && ((input != '.') && (input != '،'))) && ((input != '?') && (input != '؟')))
            {
                return false;
            }
            return true;
        }

        private static bool CheckSpecific(char input)
        {
            int num = input;
            if ((((num != 0x622) && (num != 0x623)) && ((num != 0x627) && (num != 0x62f))) && (((num != 0x630) && (num != 0x631)) && (((num != 0x632) && (num != 0x698)) && (num != 0x648))))
            {
                return false;
            }
            return true;
        }

        public static string Convert(string input)
        {
            if (!isCharsInitialized)
            {
                isCharsInitialized = true;
                InitChars();
            }

            // 伊斯兰教真主安拉在阿拉伯文里写作الله， ّ (shadda) 上面有一个短线，这是小艾里夫（短剑艾里夫）的一个特殊形式。
            // 键盘输入时输入 ل (lam) + ل (lam) + ه (ha) 后会自动转换成带记号的符号。 [2018/3/13 20:03:45 --By aq_1000]
            if (input.Contains("الله"))
            {
                input = input.Replace("الله", "ﷲ");
            }

            char[] chArray = input.ToCharArray();
            char[] chArray2 = new char[chArray.Length];
            List<char> list = new List<char>();
            for (int i = 0; i < chArray.Length; i++)
            {
                if (IsNumericChar(chArray[i]))
                {
                    //chArray2[i] = ReplaceChar(chArray[i], CharState.number);
                    chArray2[i] = chArray[i];
                }
                else if ((i + 1) == chArray.Length)
                {
                    if (chArray.Length == 1)
                    {
                        chArray2[i] = ReplaceChar(chArray[i], CharState.isolated);
                    }
                    else if (CheckSeparator(chArray[i - 1]) || CheckSpecific(chArray[i - 1]))
                    {
                        chArray2[i] = ReplaceChar(chArray[i], CharState.isolated);
                    }
                    else
                    {
                        chArray2[i] = ReplaceChar(chArray[i], CharState.final);
                    }
                }
                else if (i == 0)
                {
                    if (!CheckSeparator(chArray[i + 1]))
                    {
                        chArray2[i] = ReplaceChar(chArray[i], CharState.init);
                    }
                    else
                    {
                        chArray2[i] = ReplaceChar(chArray[i], CharState.isolated);
                    }
                }
                else if (CheckSeparator(chArray[i + 1]))
                {
                    if (CheckSeparator(chArray[i - 1]) || CheckSpecific(chArray[i - 1]))
                    {
                        chArray2[i] = ReplaceChar(chArray[i], CharState.isolated);
                    }
                    else
                    {
                        chArray2[i] = ReplaceChar(chArray[i], CharState.final);
                    }
                }
                else if (CheckSeparator(chArray[i - 1]))
                {
                    if (CheckSeparator(chArray[i + 1]))
                    {
                        chArray2[i] = ReplaceChar(chArray[i], CharState.isolated);
                    }
                    else
                    {
                        chArray2[i] = ReplaceChar(chArray[i], CharState.init);
                    }
                }
                else if (CheckSpecific(chArray[i + 1]))
                {
                    if (CheckSeparator(chArray[i - 1]) || CheckSpecific(chArray[i - 1]))
                    {
                        chArray2[i] = ReplaceChar(chArray[i], CharState.init);
                    }
                    else
                    {
                        chArray2[i] = ReplaceChar(chArray[i], CharState.middle);
                    }
                }
                else if (CheckSpecific(chArray[i - 1]))
                {
                    if (CheckSeparator(chArray[i + 1]))
                    {
                        chArray2[i] = ReplaceChar(chArray[i], CharState.isolated);
                    }
                    else
                    {
                        chArray2[i] = ReplaceChar(chArray[i], CharState.init);
                    }
                }
                else
                {
                    chArray2[i] = ReplaceChar(chArray[i], CharState.middle);
                }
            }
            List<string> list2 = new List<string>();
            string str = "";
            int num2 = 0;
            for (int j = 0; j < chArray2.Length; j++)
            {
                char item = chArray2[(chArray2.Length - j) - 1];
                int num4 = item;
                if (((num4 >= 0x20) && (num4 <= 0x7e) && !_IsBracket(item)) || 
                    ((num4 >= 0x660) && (num4 <= 0x669)) || 
                    char.IsSurrogate(item) || char.IsLowSurrogate(item))
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        list.Add('\x00bf');
                        num2++;
                    }
                    str = str + item.ToString();
                }
                else
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        list2.Add(str);
                    }
                    str = "";
		    item = _ProcessBracket(item);
                    list.Add(item);
                }
            }
            if (!string.IsNullOrEmpty(str))
            {
                list2.Add(str);
            }
            string str2 = "";
            for (int k = 0; k < list.Count; k++)
            {
                if (list[k] != '\0')
                {
                    str2 = str2 + list[k].ToString();
                }
            }
            for (int m = 0; m < num2; m++)
            {
                for (int n = 0; n < str2.Length; n++)
                {
                    if (str2[n] == '\x00bf')
                    {
                        string str3 = "";
                        string str4 = list2[0];
                        list2.RemoveAt(0);
                        for (int num8 = 0; num8 < str4.Length; num8++)
                        {
                            str3 = str3 + str4[(str4.Length - num8) - 1];
                        }
                        str2 = str2.Remove(n, 1).Insert(n, str3);
                        break;
                    }
                }
            }


			return Reverse(str2);
        }

		private static string Reverse(string source)
		{
			StringBuilder buffer = new StringBuilder();
			int len = source.Length;
			int i = len - 1;
			while (i >= 0)
			{
				char ch = source[i];
				if (ch == '\r' && i != len - 1 && source[i + 1] == '\n')
				{
					i--;
					continue;
				}

				if (char.IsLowSurrogate(ch)) //不要反向高低代理对
				{
					buffer.Append(source[i - 1]);
					buffer.Append(ch);
					i--;
				}
				else
					buffer.Append(ch);
				i--;
			}

			return buffer.ToString();
		}

        private static void InitChars()
        {
            numbers = new Dictionary<int, char>();
            init = new Dictionary<int, char>();
            final = new Dictionary<int, char>();
            middle = new Dictionary<int, char>();
            isolate = new Dictionary<int, char>();
            numbers.Add(0x660, '٠');
            numbers.Add(0x661, '١');
            numbers.Add(0x662, '٢');
            numbers.Add(0x663, '٣');
            numbers.Add(0x664, '٤');
            numbers.Add(0x665, '٥');
            numbers.Add(0x666, '٦');
            numbers.Add(0x667, '٧');
            numbers.Add(0x668, '٨');
            numbers.Add(0x669, '٩');
            numbers.Add(0x30, '٠');
            numbers.Add(0x31, '١');
            numbers.Add(50, '٢');
            numbers.Add(0x33, '٣');
            numbers.Add(0x34, '٤');
            numbers.Add(0x35, '٥');
            numbers.Add(0x36, '٦');
            numbers.Add(0x37, '٧');
            numbers.Add(0x38, '٨');
            numbers.Add(0x39, '٩');
            init.Add(0x622, (char)0xfe81);
            init.Add(0x627, (char)0xfe8d);
            init.Add(0x628, (char)0xfe91);
            init.Add(0x67e, (char)0xfb58);
            init.Add(0x62a, (char)0xfe97);
            init.Add(0x62b, (char)0xfe9b);
            init.Add(0x62c, (char)0xfe9f);
            init.Add(0x686, (char)0xfb7c);
            init.Add(0x62d, (char)0xfea3);
            init.Add(0x62e, (char)0xfea7);
            init.Add(0x62f, (char)0xfea9);
            init.Add(0x630, (char)0xfeab);
            init.Add(0x631, (char)0xfead);
            init.Add(0x632, (char)0xfeaf);
            init.Add(0x698, (char)0xfb8a);
            init.Add(0x633, (char)0xfeb3);
            init.Add(0x634, (char)0xfeb7);
            init.Add(0x635, (char)0xfebb);
            init.Add(0x636, (char)0xfebf);
            init.Add(0x637, (char)0xfec3);
            init.Add(0x638, (char)0xfec7);
            init.Add(0x639, (char)0xfecb);
            init.Add(0x63a, (char)0xfecf);
            init.Add(0x641, (char)0xfed3);
            init.Add(0x642, (char)0xfed7);
            init.Add(0x6a9, (char)0xfedb);
            init.Add(0x643, (char)0xfedb);
            init.Add(0x6af, (char)0xfb94);
            init.Add(0x644, (char)0xfedf);
            init.Add(0x645, (char)0xfee3);
            init.Add(0x646, (char)0xfee7);
            init.Add(0x648, (char)0xfeed);
            init.Add(0x647, (char)0xfeeb);
            init.Add(0x6be, (char)0xfeeb);
            init.Add(0x6cc, (char)0xfef3);
            init.Add(0x64a, (char)0xfef3);
            init.Add(0x623, (char)0xfe83);
            init.Add(0x621, (char)0xfe8b);
            init.Add(0x626, (char)0xfe8b);
            middle.Add(0x622, (char)0xfe81);
            middle.Add(0x627, (char)0xfe8e);
            middle.Add(0x628, (char)0xfe92);
            middle.Add(0x67e, (char)0xfb59);
            middle.Add(0x62a, (char)0xfe98);
            middle.Add(0x62b, (char)0xfe9c);
            middle.Add(0x62c, (char)0xfea0);
            middle.Add(0x686, (char)0xfb7d);
            middle.Add(0x62d, (char)0xfea4);
            middle.Add(0x62e, (char)0xfea8);
            middle.Add(0x62f, (char)0xfeaa);
            middle.Add(0x630, (char)0xfeac);
            middle.Add(0x631, (char)0xfeae);
            middle.Add(0x632, (char)0xfeb0);
            middle.Add(0x698, (char)0xfb8b);
            middle.Add(0x633, (char)0xfeb4);
            middle.Add(0x634, (char)0xfeb8);
            middle.Add(0x635, (char)0xfebc);
            middle.Add(0x636, (char)0xfec0);
            middle.Add(0x637, (char)0xfec4);
            middle.Add(0x638, (char)0xfec8);
            middle.Add(0x639, (char)0xfecc);
            middle.Add(0x63a, (char)0xfed0);
            middle.Add(0x641, (char)0xfed4);
            middle.Add(0x642, (char)0xfed8);
            middle.Add(0x6a9, (char)0xfedc);
            middle.Add(0x643, (char)0xfedc);
            middle.Add(0x6af, (char)0xfb95);
            middle.Add(0x644, (char)0xfee0);
            middle.Add(0x645, (char)0xfee4);
            middle.Add(0x646, (char)0xfee8);
            middle.Add(0x648, (char)0xfeee);
            middle.Add(0x647, (char)0xfeec);
            middle.Add(0x6be, (char)0xfeec);
            middle.Add(0x6cc, (char)0xfef4);
            middle.Add(0x64a, (char)0xfef4);
            middle.Add(0x623, (char)0xfe84);
            middle.Add(0x621, (char)0xfe8c);
            middle.Add(0x626, (char)0xfe8c);
            final.Add(0x622, (char)0xfe81);
            final.Add(0x627, (char)0xfe8e);
            final.Add(0x628, (char)0xfe90);
            final.Add(0x67e, (char)0xfb57);
            final.Add(0x62a, (char)0xfe96);
            final.Add(0x62b, (char)0xfe9a);
            final.Add(0x62c, (char)0xfe9e);
            final.Add(0x686, (char)0xfb7b);
            final.Add(0x62d, (char)0xfea2);
            final.Add(0x62e, (char)0xfea6);
            final.Add(0x62f, (char)0xfeaa);
            final.Add(0x630, (char)0xfeac);
            final.Add(0x631, (char)0xfeae);
            final.Add(0x632, (char)0xfeb0);
            final.Add(0x698, (char)0xfb8b);
            final.Add(0x633, (char)0xfeb2);
            final.Add(0x634, (char)0xfeb6);
            final.Add(0x635, (char)0xfeba);
            final.Add(0x636, (char)0xfebe);
            final.Add(0x637, (char)0xfec2);
            final.Add(0x638, (char)0xfec6);
            final.Add(0x639, (char)0xfeca);
            final.Add(0x63a, (char)0xfece);
            final.Add(0x641, (char)0xfed2);
            final.Add(0x642, (char)0xfed6);
            final.Add(0x6a9, (char)0xfb8f);
            final.Add(0x643, (char)0xfb8f);
            final.Add(0x6af, (char)0xfb93);
            final.Add(0x644, (char)0xfede);
            final.Add(0x645, (char)0xfee2);
            final.Add(0x646, (char)0xfee6);
            final.Add(0x648, (char)0xfeee);
            final.Add(0x647, (char)0xfeea);
            final.Add(0x6be, (char)0xfeea);
            final.Add(0x6cc, (char)0xfef0);
            final.Add(0x64a, (char)0xfef0);
            final.Add(0x623, (char)0xfe84);
            final.Add(0x621, (char)0xfe8a);
            final.Add(0x626, (char)0xfe80);
            isolate.Add(0x622, (char)0xfe81);
            isolate.Add(0x627, (char)0xfe8d);
            isolate.Add(0x628, (char)0xfe8f);
            isolate.Add(0x67e, (char)0xfb56);
            isolate.Add(0x62a, (char)0xfe95);
            isolate.Add(0x62b, (char)0xfe99);
            isolate.Add(0x62c, (char)0xfe9d);
            isolate.Add(0x686, (char)0xfb7a);
            isolate.Add(0x62d, (char)0xfea1);
            isolate.Add(0x62e, (char)0xfea5);
            isolate.Add(0x62f, (char)0xfea9);
            isolate.Add(0x630, (char)0xfeab);
            isolate.Add(0x631, (char)0xfead);
            isolate.Add(0x632, (char)0xfeaf);
            isolate.Add(0x698, (char)0xfb8a);
            isolate.Add(0x633, (char)0xfeb1);
            isolate.Add(0x634, (char)0xfeb5);
            isolate.Add(0x635, (char)0xfeb9);
            isolate.Add(0x636, (char)0xfebd);
            isolate.Add(0x637, (char)0xfec1);
            isolate.Add(0x638, (char)0xfec5);
            isolate.Add(0x639, (char)0xfec9);
            isolate.Add(0x63a, (char)0xfecd);
            isolate.Add(0x641, (char)0xfed1);
            isolate.Add(0x642, (char)0xfed5);
            isolate.Add(0x6a9, (char)0xfb8e);
            isolate.Add(0x643, (char)0xfb8e);
            isolate.Add(0x6af, (char)0xfb92);
            isolate.Add(0x644, (char)0xfedd);
            isolate.Add(0x645, (char)0xfee1);
            isolate.Add(0x646, (char)0xfee5);
            isolate.Add(0x648, (char)0xfeed);
            isolate.Add(0x647, (char)0xfee9);
            isolate.Add(0x6be, (char)0xfee9);
            isolate.Add(0x6cc, (char)0xfeef);
            isolate.Add(0x64a, (char)0xfeef);
            isolate.Add(0x623, (char)0xfe83);

            // 这两个字符相互兑换错了 [2018/3/13 17:03:20 --By aq_1000]
            //             isolate.Add(0x621, (char)0xfe89);
            //             isolate.Add(0x626, (char)0xfe80);
            isolate.Add(0x621, (char)0xfe80);
            isolate.Add(0x626, (char)0xfe89);
        }

        private static bool IsNumericChar(int unicode)
        {
            return numbers.ContainsKey(unicode);
        }

        private static char ReplaceChar(int unicode, CharState _state)
        {
            if (((ushort)unicode) == 0x200c)
            {
                return '\0';
            }
            switch (_state)
            {
                case CharState.init:
                    if (init.ContainsKey(unicode))
                    {
                        return init[unicode];
                    }
                    return (char)unicode;

                case CharState.middle:
                    if (middle.ContainsKey(unicode))
                    {
                        return middle[unicode];
                    }
                    return (char)unicode;

                case CharState.final:
                    if (final.ContainsKey(unicode))
                    {
                        return final[unicode];
                    }
                    return (char)unicode;

                case CharState.isolated:
                    if (isolate.ContainsKey(unicode))
                    {
                        return isolate[unicode];
                    }
                    return (char)unicode;

                case CharState.number:
                    if (numbers.ContainsKey(unicode))
                    {
                        return numbers[unicode];
                    }
                    return (char)unicode;
            }
            return '*';
        }
	    
	        // 是否括号
        private static bool _IsBracket(char uc)
        {
            return (uc == ')' || uc == '(' ||
                    uc == ']' || uc == '[' ||
                    uc == '}' || uc == '{' ||
                    uc == '>' || uc == '<');
        }

        private static char _ProcessBracket(char uc)
        {
            // 这些配对符,在从右至左排列中应该逆序显示
            if (uc == '[')
            {
                return ']';
            }
            else if (uc == ']')
            {
                return '[';
            }
            else if (uc == '{')
            {
                return '}';
            }
            else if (uc == '}')
            {
                return '{';
            }
            else if (uc == '(')
            {
                return ')';
            }
            else if (uc == ')')
            {
                return '(';
            }
            else if (uc == '<')
            {
                return '>';
            }
            else if (uc == '>')
            {
                return '<';
            }
            else return uc;
        }
    }
}

