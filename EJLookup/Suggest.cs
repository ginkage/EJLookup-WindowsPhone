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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.ComponentModel;

namespace EJLookup
{
    public class Suggest
    {
        public Suggest()
        {
        }

        public static List<string> Lookup(string request, BackgroundWorker task, int maxsug, bool romanize)
        {
            List<string> result = null;

            char[] text = request.ToCharArray();
            string kanareq = Nihongo.global.Kanate(text);
            char[] kanatext = kanareq.ToCharArray();

            int qlen = Nihongo.global.Normalize(text);
            int klen = Nihongo.global.Normalize(kanatext);

            Dictionary<string, int> suggest = new Dictionary<string, int>();

            int last = -1;
            var myStream = Application.GetResourceStream(new Uri("Data/Dic/suggest.dat", UriKind.Relative));
            if (myStream != null)
            {
                Stream myFileStream = myStream.Stream;
                if (myFileStream.CanRead)
                {
                    BinaryReader fileIdx = new BinaryReader(myFileStream);
                    last = Tokenize(text, qlen, fileIdx, suggest, task);
                    if (!text.Equals(kanatext))
                        Tokenize(kanatext, klen, fileIdx, suggest, task);
                }
            }

            if (suggest.Count > 0 && !task.CancellationPending)
            {
                result = new List<string>();

                Dictionary<string, int> duplicate = new Dictionary<string, int>();
                string begin = (last >= 0 ? request.Substring(0, last) : "");

                foreach (KeyValuePair<string, int> kvp in (from pair in suggest
                                                           orderby pair.Value descending, pair.Key ascending
                                                           select pair))
                {
                    if (result.Count >= maxsug) break;
                    string str = kvp.Key;
                    string k = str;

                    if (romanize)
                    {
                        int i;
                        bool convert = true;
                        for (i = 0; i < str.Length; i++)
                            if (str[i] >= 0x3200)
                            {
                                convert = false;
                                break;
                            }

                        if (convert)
                        {
                            char[] txt = str.ToCharArray();
                            k = Nihongo.global.Romanate(txt, 0, str.Length - 1);
                        }
                    }

                    if (!duplicate.ContainsKey(k))
                    {
                        result.Add(begin + k);
                        duplicate[k] = 0;
                    }
                }
            }

            return result;
        }

        private static int Tokenize(char[] text, int len, BinaryReader fileIdx, Dictionary<string, int> suggest, BackgroundWorker task)
        {
            int p, last = -1;

            for (p = 0; p < len; p++)
                if (Nihongo.letter(text[p]) || (text[p] == '\'' && p > 0 && p + 1 < len && Nihongo.letter(text[p - 1]) && Nihongo.letter(text[p + 1])))
                {
                    if (last < 0)
                    {
                        last = p;
                    }
                }
                else if (last >= 0)
                {
                    last = -1;
                }

            if (last >= 0) // Only search for the last word entered
                Traverse(new string(text, last, p - last), fileIdx, 0, "", suggest, task);

            return last;
        }

        private static bool Traverse(string word, BinaryReader fidx, long pos, string str, Dictionary<string, int> suglist, BackgroundWorker task)
	    {
		    if (task.CancellationPending) return false;
            fidx.BaseStream.Seek(pos, SeekOrigin.Begin);

		    int tlen = fidx.ReadByte();
		    int c = fidx.ReadByte();
		    int freq = fidx.ReadInt32();
		    bool children = ((c & 1) != 0), unicode = ((c & 8) != 0), exact = !(word.Equals(""));
		    int match = 0, nlen = 0, wlen = word.Length, p;
		    char ch;

		    if (pos > 0) {
			    string nword = "";

			    if (tlen > 0) {
                    if (unicode)
                    {
                        char[] wbuf = new char[tlen];
                        for (c = 0; c < tlen; c++)
                            wbuf[c] = (char)fidx.ReadUInt16();
                        nword = new string(wbuf);
                    }
                    else
                    {
                        byte[] wbuf = fidx.ReadBytes(tlen);
                        nword = Encoding.UTF8.GetString(wbuf, 0, tlen);
                    }
			    }

			    nlen = nword.Length;
			    str += nword;

			    if (exact) {
				    word = word.Substring(1);
				    wlen--;
	
				    while (match < wlen && match < nlen) {
					    if (word[match] != nword[match])
						    break;
					    match++;
				    }
			    }
		    }

		    if (match == nlen || match == wlen) {
                Dictionary<int, char> cpos = new Dictionary<int, char>();
			    exact = exact && (match == nlen);

			    if (children) // One way or the other, we'll need a full children list
				    do { // Read it from this location once, save for later
					    ch = (char)fidx.ReadUInt16();
					    p = fidx.ReadInt32();
					    if (match < wlen) { // (match == nlen), Traverse children
						    if (ch == word[match]) {
							    string newWord = word.Substring(match);
							    return Traverse(newWord, fidx, (p & 0x7fffffff), str + ch, suglist, task); // Traverse children
						    }
					    }
					    else
						    cpos.Add(p & 0x7fffffff, ch);
				    } while ((p & 0x80000000) == 0);

			    if (match == wlen) {
				    if (freq > 0 && !exact) {
                        int v;
                        if (!suglist.TryGetValue(str, out v))
                            v = 0;
                        suglist[str] = v + freq;
				    }

                    foreach (KeyValuePair<int, char> kvp in cpos)
                        Traverse("", fidx, kvp.Key, str + kvp.Value, suglist, task); // Traverse everything that begins with this word

				    return true; // Got result
			    }
		    }

		    return false; // Nothing found
	    }
    }
}
