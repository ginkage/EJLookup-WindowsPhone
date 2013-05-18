using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace EJLookup
{
    public class ResultLine
    {
        public string group;
        public string data;

        public ResultLine(string _group, string _data)
        {
            group = _group;
            data = _data;
        }

	    static private String getSubstr(char[] text, int begin)
	    {
		    int i, len = text.Length, end = -1;
		    for (i = begin; i < len && text[i] != 0; i++)
			    end = i;
		    if (end < 0) return "";
		    return new String(text, begin, end - begin + 1);
	    }

	    static private int strchr(char[] text, int begin, char c)
	    {
		    int i, len = text.Length;
		    for (i = begin; i < len && text[i] != 0; i++)
			    if (text[i] == c)
				    return i;
		    return -1;
	    }

        public Paragraph FormatLine(int KanjiFont, int KanaFont)
	    {
		    int i, i0 = -1, i1 = -1, i2 = -1, i3 = -1, i4 = -1, i5 = -1;
		    int kanji = -1, kana = -1, trans = -1, roshi = -1, p;
            string sdict = "Default", skanji = "", skana = "", strans = "";
		    bool kd = false;

		    char[] text = data.ToCharArray();
		    int len = text.Length;

		    for (i = 0; i < len; i++)
			    if (i0 < 0 && text[i] == ')')
				    i0 = i;
			    else if (i1 < 0 && i3 < 0 && text[i] == '[')
				    i1 = i;
			    else if (i2 < 0 && i3 < 0 && text[i] == ']')
				    i2 = i;
			    else if (i4 < 0 && i3 < 0 && text[i] == '{')
				    i4 = i;
			    else if (i5 < 0 && i3 < 0 && text[i] == '}')
				    i5 = i;
			    else if (i3 < 0 && text[i] == '/' && (i == 0 || text[i-1] != '<'))
				    i3 = i;

		    i0 = -1;
		    sdict = group;

		    if (sdict.StartsWith("kanjidic")) {
			    kanji = i0 + 1;
			    while (kanji < len && text[kanji] == ' ') kanji++;
			    p = strchr(text, kanji, ' ');
			    if (p >= 0) {
				    text[p] = '\0';
				    kana = p + 1;
				    for (p = kana; p < len && text[p] != 0; p++)
					    if (text[p] > 127) {
						    kana = p;
						    break;
					    }

				    p = strchr(text, kana, '{');
				    if (p >= 0) {
					    text[p-1] = '\0';
					    trans = p;
				    }
			    }
			    kd = true;
		    }
		    else {
			    trans = i0 + 1;
			    if (i1 >= 0 && i2 >= 0 && i1 < i2) {
				    text[i1] = '\0';
				    text[i2] = '\0';
				    kana = i1 + 1;
				    trans = i2 + 1;
				    if (i0 < i1) {
					    kanji = i0 + 1;
					    while (kanji < len && text[kanji] == ' ')
						    kanji++;
				    }
			    }

			    if (i3 >= 0 && i3 > i0 && i3 > i1 && i3 > i2 && i3 > i4 && i3 > i5) {
				    if (kana < 0) kana = trans;
				    text[i3] = '\0';
				    trans = i3 + 1;
			    }

			    if (i4 >= 0 && i5 >= 0 && i4 < i5) {
				    text[i4] = '\0';
				    text[i5] = '\0';
				    roshi = i4 + 1;
			    }
		    }

		    if (kanji >= 0) {
			    int end = kanji;
			    while (end < len && text[end] != 0) end++;
			    if (end > kanji) {
				    end--;
				    while (end > kanji && text[end] == ' ')
					    text[end--] = '\0';
			    }
			    skanji = getSubstr(text, kanji);
		    }

		    if (kana >= 0) {
			    for (p = kana; p < len && text[p] != 0; ) {
				    while (p < len && (text[p] == ' ' || text[p] == ',')) p++;
				    if (p < len && text[p] != 0) {
					    int begin = p, end = p - 1;
					    while (p < len && text[p] != 0 && text[p] != ' ' && text[p] != ',') { end = p; p++; }
					    if (end >= begin) {
						    if (!skana.Equals("")) skana += "\n";
						    skana += "[" + new String(text, begin, end - begin + 1);
						    if (text[begin] > 127) {
							    skana += (kd ? " / " : "]\n[");
							    skana += Nihongo.global.Romanate(text, begin, end);
						    }
						    skana += "]";
					    }
				    }
			    }
		    }

		    if (roshi >= 0) {
			    for (p = roshi; p < len && text[p] != 0; ) {
				    while (p < len && (text[p] == ' ' || text[p] == ',')) p++;
				    if (p < len && text[p] != 0) {
					    int begin = p, end = p - 1;
					    while (p < len && text[p] != 0 && text[p] != ' ' && text[p] != ',') { end = p; p++; }
					    if (end >= begin) {
						    if (!skana.Equals("")) skana += "\n";
						    skana += "[" + new String(text, begin, end - begin + 1) + "]";
					    }
				    }
			    }
		    }

		    if (trans >= 0) {
			    if (kd) {
				    for (p = trans; p < len && text[p] != 0; ) {
					    while (p < len && (text[p] == '{' || text[p] == '}')) p++;
					    if (p < len && text[p] != 0) {
						    while (p < len && text[p] == ' ') p++;
						    int begin = p, end = p - 1;
						    while (p < len && text[p] != 0 && text[p] != '{' && text[p] != '}') { end = p; p++; }
						    if (end >= begin) {
							    if (!strans.Equals("")) strans += "\n";
							    strans += new String(text, begin, end - begin + 1);
						    }
					    }
				    }
			    }
			    else {
				    for (p = trans; p < len && text[p] != 0; p++) {
					    if (text[p] == '/' && (p == trans || text[p-1] != '<')) {
						    text[p] = '\0';
						    p++;
						    while (trans < len && text[trans] == ' ') trans++;
						    if (trans < len && text[trans] != 0) {
							    if (!strans.Equals("")) strans += "\n";
							    strans += getSubstr(text, trans);
						    }
						    trans = p;
					    }
				    }
				    if (trans >= 0) {
					    while (trans < len && text[trans] == ' ') trans++;
					    if (trans < len && text[trans] != 0) {
						    if (!strans.Equals("")) strans += "\n";
						    strans += getSubstr(text, trans);
					    }
				    }
			    }
		    }

            Paragraph paragraph = new Paragraph();

		    if (!skanji.Equals("")) {
                Run run = new Run();
                run.Text = skanji + "\n";
                run.FontSize = KanjiFont;
                run.Foreground = new SolidColorBrush(Color.FromArgb(255, 170, 127, 85));
                paragraph.Inlines.Add(run);
            }

            if (!skana.Equals("")) {
                Run run = new Run();
                run.Text = skana + "\n";
                run.FontSize = KanaFont;
                run.Foreground = new SolidColorBrush(Color.FromArgb(255, 42, 170, 170));
                paragraph.Inlines.Add(run);
            }

            if (!strans.Equals(""))
            {
                strans += "\n";

                int begin, end;
                while ((begin = strans.IndexOf("<i>")) >= 0)
                {
                    Run run = new Run();
                    run.Text = strans.Substring(0, begin);
                    run.FontSize = KanaFont;
                    paragraph.Inlines.Add(run);

                    end = strans.IndexOf("</i>", begin + 1);

                    Italic italic = new Italic();
                    italic.FontSize = KanaFont;
                    if (end < 0)
                    {
                        italic.Inlines.Add(strans.Substring(begin + 3));
                        strans = "";
                    }
                    else
                    {
                        italic.Inlines.Add(strans.Substring(begin + 3, end - begin - 3));
                        strans = strans.Substring(end + 4);
                    }
                    paragraph.Inlines.Add(italic);
                }

                if (!strans.Equals(""))
                {
                    Run run = new Run();
                    run.Text = strans;
                    run.FontSize = KanaFont;
                    paragraph.Inlines.Add(run);
                }
            }
            else
            {
                Run run = new Run();
                run.Text = "\n";
                run.FontSize = KanaFont;
                paragraph.Inlines.Add(run);
            }

            return paragraph;
        }
    }
}
