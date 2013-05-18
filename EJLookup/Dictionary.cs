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

namespace EJLookup
{
    public class Dictionary
    {
        private static List<string> fileList = new List<string> ()
        {
			"jr-edict",
			"warodai",
			"edict",
			"kanjidic",
			"ediclsd4",
			"classical",
			"compverb",
			"compdic",
			"lingdic",
			"jddict",
			"4jword3",
			"aviation",
			"buddhdic",
			"engscidic",
			"envgloss",
			"findic",
			"forsdic_e",
			"forsdic_s",
			"geodic",
			"lawgledt",
			"manufdic",
			"mktdic",
			"pandpdic",
			"stardict",
			"concrete"
		};

        private static int maxres = 100;

        public static List<ResultLine> Lookup(string request)
        {
            List<ResultLine> result = null;

            char[] text = request.ToCharArray();
            string kanareq = Nihongo.global.Kanate(text);
            char[] kanatext = kanareq.ToCharArray();

            int qlen = Nihongo.global.Normalize(text);
            int klen = Nihongo.global.Normalize(kanatext);

            result = new List<ResultLine>();

            List<string> sexact = new List<string>();
            List<string>[] spartial = new List<string>[fileList.Count];

            AppSettings settings = new AppSettings();
            int[] limit = { 100, 250, 500, 1000 };
            maxres = limit[settings.MaxResults];

            int i = 0, etotal = 0, ptotal = 0;
            foreach (string fileName in fileList)
            {
                spartial[i] = new List<string>();

                if (etotal < maxres && settings.GetValueOrDefault<bool>(fileName, true))
                {
                    LookupDict(fileName, sexact, ((etotal + ptotal) < maxres ? spartial[i] : null), text, qlen, kanatext, klen);

                    ptotal += spartial[i].Count;
                    etotal += sexact.Count;

                    sexact.Sort();
                    foreach (string st in sexact)
                    {
                        if (result.Count >= maxres) break;
                        result.Add(new ResultLine(fileName, st));
                    }
                    sexact.Clear();
                }

                i++;
            }

            i = 0;
            foreach (string fileName in fileList)
            {
                if (result.Count >= maxres) break;
                spartial[i].Sort();
                foreach (string st in spartial[i])
                {
                    if (result.Count >= maxres) break;
                    result.Add(new ResultLine(fileName + " (partial)", st));
                }
                i++;
            }

            return result;
        }

        private static void LookupDict(string fileName, List<string> sexact, List<string> spartial, char[] text, int qlen, char[] kanatext, int klen)
	    {
            Stream idx = OpenFile(fileName + ".idx");
            Stream dic = OpenFile(fileName + ".utf");

            if (idx == null || dic == null) return;
//            if (!EJLookupActivity.preferences.getbool(fileName, true) || !exists) return;

            BinaryReader fileIdx = new BinaryReader(idx);

            Dictionary<int, int> elines = new Dictionary<int, int>();
			Dictionary<int, int> plines = null;
			if (spartial != null)
				plines = new Dictionary<int, int>();

			int qwnum = Tokenize(text, qlen, fileIdx, elines, plines);
			if (!text.Equals(kanatext))
            {
				int kwnum = Tokenize(kanatext, klen, fileIdx, elines, plines);
				if (qwnum < kwnum) qwnum = kwnum;
			}

			List<int> spos = new List<int>();
            foreach (KeyValuePair<int, int> kvp in elines)
            {
				int line = kvp.Key;
				int mask = kvp.Value;
				if (mask + 1 == 1 << qwnum)
                {
					spos.Add(line);
					if (plines != null)
						plines.Remove(line);
				}
				else if (plines != null)
                {
					int pmask;
                    if (plines.TryGetValue(line, out pmask) && ((mask | pmask) != pmask))
						plines[line] = (pmask | mask);
				}
			}

            StreamReader fileDic = new StreamReader(dic);

            int lastpos = -1;
            spos.Sort();
            foreach (int pos in spos)
            {
                if (sexact.Count >= maxres)
                    break;

                if (pos != lastpos)
                {
                    fileDic.DiscardBufferedData();
                    fileDic.BaseStream.Seek(pos, SeekOrigin.Begin);
                    sexact.Add(fileDic.ReadLine());
                    lastpos = pos;
                }
            }

            if (plines != null && sexact.Count < maxres)
            {
				spos.Clear();
                foreach (KeyValuePair<int, int> kvp in plines)
                {
					int line = kvp.Key;
					int mask = kvp.Value;
					if (mask + 1 == 1 << qwnum)
						spos.Add(line);
				}

                lastpos = -1;
                spos.Sort();
                foreach (int pos in spos)
                {
                    if (sexact.Count + spartial.Count >= maxres)
                        break;

                    if (pos != lastpos)
                    {
                        fileDic.DiscardBufferedData();
                        fileDic.BaseStream.Seek(pos, SeekOrigin.Begin);
                        spartial.Add(fileDic.ReadLine());
                        lastpos = pos;
                    }
                }
			}
	    }

        private static Stream OpenFile(string fileName)
        {
            var myStream = Application.GetResourceStream(new Uri("Data/Dic/" + fileName, UriKind.Relative));
            if (myStream != null)
            {
                Stream myFileStream = myStream.Stream;
                if (myFileStream.CanRead)
                    return myFileStream;
            }

            return null;
        }

        private static int Tokenize(char[] text, int len, BinaryReader fileIdx, Dictionary<int, int> exact, Dictionary<int, int> partial)
        {
            int p, last = -1, wnum = 0;
            bool kanji = false;

            for (p = 0; p < len; p++)
                if (Nihongo.letter(text[p]) || (text[p] == '\'' && p > 0 && p + 1 < len && Nihongo.letter(text[p - 1]) && Nihongo.letter(text[p + 1])))
                {
                    if (last < 0)
                        last = p;
                    if (text[p] >= 0x3200)
                        kanji = true;
                }
                else if (last >= 0)
                {
                    DoSearch(new string(text, last, p - last), wnum++, fileIdx, exact, partial, kanji);
                    kanji = false;
                    last = -1;
                }

            if (last >= 0)
                DoSearch(new string(text, last, p - last), wnum++, fileIdx, exact, partial, kanji);

            return wnum;
        }

        private static void DoSearch(string query, int wnum, BinaryReader fileIdx, Dictionary<int, int> exact, Dictionary<int, int> partial, bool kanji)
        {
            int mask = 1 << wnum;
            Dictionary<int, bool> lines = new Dictionary<int, bool>();
            Traverse(query, fileIdx, 0, (query.Length > 1 || kanji) && (partial != null), true, lines);

            foreach (KeyValuePair<int, bool> kvp in lines)
            {
                int k = kvp.Key, v = 0, val;
                bool e = kvp.Value;

                if (e)
                {
                    if (exact.TryGetValue(k, out val))
                        v = val;
                }
                else if (partial != null)
                {
                    if (partial.TryGetValue(k, out val))
                        v = val;
                }

                v |= mask;

                if (e)
                    exact[k] = v;
                else if (partial != null)
                    partial[k] = v;
            }
        }

        private static bool Traverse(string word, BinaryReader fidx, long pos, bool partial, bool child, Dictionary<int, bool> poslist)
	    {
            fidx.BaseStream.Seek(pos, SeekOrigin.Begin);

            int match = 0, nlen = 0, wlen = word.Length, p;
            int tlen = fidx.ReadByte();
		    int c = fidx.ReadByte();
            bool children = ((c & 1) != 0), filepos = ((c & 2) != 0), parents = ((c & 4) != 0), unicode = ((c & 8) != 0), exact = (wlen > 0);

            if (!exact)
                fidx.BaseStream.Seek(unicode ? (tlen * 2) : tlen, SeekOrigin.Current);
            else if (pos > 0)
            {
                word = word.Substring(1);
                wlen--;

                if (tlen > 0)
                {
                    string nword = null;

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

                    nlen = nword.Length;

                    while (match < wlen && match < nlen)
                    {
                        if (word[match] != nword[match])
                            break;
                        match++;
                    }
                }
            }

		    if (match == nlen || match == wlen) {
			    List<int> cpos = new List<int>();

			    if (children) // One way or the other, we'll need a full children list
				    do { // Read it from this location once, save for later
					    c = (char)fidx.ReadUInt16();
					    p = fidx.ReadInt32();
					    if (match < wlen) { // (match == nlen), Traverse children
						    if (c == word[match]) {
							    string newWord = word.Substring(match);
							    return Traverse(newWord, fidx, (p & 0x7fffffff), partial, true, poslist); // Traverse children
						    }
					    }
					    else if (partial && child)
						    cpos.Add(p & 0x7fffffff);
				    } while ((p & 0x80000000) == 0);

			    if (match == wlen) {
				    // Our search was successful, word ends here. We'll need all file positions and relatives
				    exact = exact && (match == nlen);
				    if (filepos && (match == nlen || partial)) { // Gather all results from this node
					    do {
						    p = fidx.ReadInt32();
						    int k = (p & 0x7fffffff);
                            bool v;

                            if (!poslist.TryGetValue(k, out v) || (!v && exact))
							    poslist[k] = exact;
					    } while ((p & 0x80000000) == 0);
				    }

				    if (partial) {
					    List<int> ppos = new List<int>();
					    if (parents) // One way or the other, we'll need a full parents list
						    do { // Read it from this location once, save for later
                                p = fidx.ReadInt32();
							    ppos.Add(p & 0x7fffffff);
						    } while ((p & 0x80000000) == 0);

					    if (child)
                            foreach (int it in cpos)
							    Traverse("", fidx, it, partial, true, poslist); // Traverse everything that begins with this word

                        foreach (int it in ppos)
						    Traverse("", fidx, it, partial, false, poslist); // Traverse everything that fully has this word in it
				    }

				    return true; // Got result
			    }
		    }

		    return false; // Nothing found
	    }
    }
}
