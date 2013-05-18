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
using System.IO;

namespace EJLookup
{
    public class Nihongo
    {
        public static Nihongo global;

        private string[] kana = new string[512];
        private string[] roma = new string[512];
        private int kana_count = 0;
        private int roma_count = 0;
        private char[] hashtab = new char[65536];

        public Nihongo()
        {
            for (int i = 0; i < 65536; i++)
                if ((i >= 'A' && i <= 'Z') ||
                    (i >= 0x0410 && i <= 0x042F))
                    hashtab[i] = (char)(i + 0x20);
                else if (i == 0x0451 || i == 0x0401)
                    hashtab[i] = (char)0x0435;
                else if (i == 0x040E || i == 0x045E)
                    hashtab[i] = (char)0x0443;
                else if (i == 0x3000)
                    hashtab[i] = (char)0x0020;
                else if (i >= 0x30A1 && i <= 0x30F4)
                    hashtab[i] = (char)(i - 0x60);
                else if (i >= 0xFF01 && i <= 0xFF20)
                    hashtab[i] = (char)(i - 0xFEE0);
                else if (i >= 0xFF21 && i <= 0xFF3A)
                    hashtab[i] = (char)(i - 0xFEC0);
                else if (i >= 0xFF3B && i <= 0xFF5E)
                    hashtab[i] = (char)(i - 0xFEE0);
                else
                    hashtab[i] = (char)i;

            var myStream = Application.GetResourceStream(new Uri("Data/kanatab.txt", UriKind.Relative));
            if (myStream != null)
            {
                Stream myFileStream = myStream.Stream;
                if (myFileStream.CanRead)
                {
                    StreamReader myStreamReader = new StreamReader(myFileStream);
                    while (!myStreamReader.EndOfStream)
                        kana[kana_count++] = myStreamReader.ReadLine();
                }
            }

            myStream = Application.GetResourceStream(new Uri("Data/romatab.txt", UriKind.Relative));
            if (myStream != null)
            {
                Stream myFileStream = myStream.Stream;
                if (myFileStream.CanRead)
                {
                    StreamReader myStreamReader = new StreamReader(myFileStream);
                    while (!myStreamReader.EndOfStream)
                        roma[roma_count++] = myStreamReader.ReadLine();
                }
            }
        }

        public static bool Jaiueoy(char c)
        {
            return (c >= 0x3041 && c <= 0x304A) || (c >= 0x30A1 && c <= 0x30AA) ||
            (c >= 0x3083 && c <= 0x3088) || (c >= 0x30E3 && c <= 0x30E8);
        }

        public static char jtolower(char c)
        {
            if ((c >= 'A' && c <= 'Z') || (c >= 0x0410 && c <= 0x042F))
                return (char)(c + 0x20);
            return c;
        }

        public static bool aiueo(char c)
        {
            return c == 'a' || c == 'i' || c == 'u' || c == 'e' || c == 'o';
        }

        public static bool letter(char c)
        {
            return ((c >= '0' && c <= '9') ||
			        (c >= 'A' && c <= 'Z') ||
			        (c >= 'a' && c <= 'z') ||
			        (c >= 0x00C0 && c <= 0x02A8) ||
			        (c >= 0x0401 && c <= 0x0451) ||
			        c == 0x3005 ||
			        (c >= 0x3041 && c <= 0x30FA) ||
			        (c >= 0x4E00 && c <= 0xFA2D) ||
			        (c >= 0xFF10 && c <= 0xFF19) ||
			        (c >= 0xFF21 && c <= 0xFF3A) ||
			        (c >= 0xFF41 && c <= 0xFF5A) ||
			        (c >= 0xFF66 && c <= 0xFF9F));
        }

        private int findsub(char[] str, int offset)
        {
            int a = 0, b = roma_count - 1, cur;
            int psub, pstr;
    
            while (b-a > 1)
            {
                cur = (a+b)/2;
                psub = 0;
                pstr = offset;

                string sroma = roma[cur];

                while (pstr < str.Length && sroma[psub] != '=')
                {
                    if (jtolower(str[pstr]) < sroma[psub])
                    {	b = cur;	break;	}
                    else if (jtolower(str[pstr]) > sroma[psub])
                    {	a = cur;	break;	}
                    pstr++;	psub++;
                }

                if (sroma[psub++] == '=') return cur;
                else if (pstr >= str.Length) return -1;
            }
    
            psub = 0;
            pstr = offset;
            string aroma = roma[a];
            while (pstr < str.Length && aroma[psub] != '=')
            {
                if (jtolower(str[pstr]) != aroma[psub]) break;
                pstr++;	psub++;
            }
            if (aroma[psub++] == '=') return a;
            else if (pstr >= str.Length) return -1;
    
            if (a != b)
            {
                psub = 0;
                pstr = offset;
                string broma = roma[b];
                while (pstr <= str.Length && broma[psub] != '=')
                {
                    if (jtolower(str[pstr]) != broma[psub]) break;
                    pstr++;	psub++;
                }
                if (broma[psub++] == '=') return b;
            }
    
            return -1;
        }

        public string Kanate(char[] text)
        {
            int pb, pk = 0, pls, prs, r;
            string sout = "";
            char[] kanabuf = new char[1024];
            bool tsu;
            char c;

            for (pb = 0; pb < text.Length; pb++)
            {
                tsu = false;
                if (pb + 1 < text.Length && jtolower(text[pb]) == jtolower(text[pb + 1]) && !aiueo(jtolower(text[pb])))
                {
                    if (pb + 2 < text.Length && jtolower(text[pb]) == 'n' && jtolower(text[pb + 1]) == 'n' && jtolower(text[pb + 2]) == 'n')
                    {
                        c = (char)0x3093;
                        sout += c;
                        pb++;
                        continue;
                    }
            
                    tsu = true;
                    pb++;
                }
        
                if (pb < text.Length && ((pls = findsub(text, pb)) >= 0))
                {
                    string sroma = roma[pls];

                    if (tsu)
                    {
                        if (jtolower(text[pb-1]) == 'n') kanabuf[pk++] = (char)0x3093;
                        else kanabuf[pk++] = (char)0x3063;
                    }
            
                    r = 0;
                    while (sroma[r++] != '=') pb++;
                    pb--;
            
                    prs = pk;
                    while (r < sroma.Length) kanabuf[prs++] = sroma[r++];
                    pk = prs;
                }
                else if (jtolower(text[pb]) == 'n' || jtolower(text[pb]) == 'm')
                    kanabuf[pk++] = (char)0x3093;
                else
                {
                    char[] tmp = new char[4];
                    pls = -1;

                    if (pb + 1 < text.Length && jtolower(text[pb]) == 't' && jtolower(text[pb + 1]) == 's')
                    {
                        tmp[0] = 't'; tmp[1] = 's'; tmp[2] = 'u'; tmp[3] = '\0';
                        pls = findsub(tmp, 0);
                    }

                    if (pb + 1 < text.Length && jtolower(text[pb]) == 's' && jtolower(text[pb + 1]) == 'h')
                    {
                        tmp[0] = 's'; tmp[1] = 'h'; tmp[2] = 'i'; tmp[3] = '\0';
                        pls = findsub(tmp, 0);
                    }

                    if (pls >= 0)
                    {
                        string sroma = roma[pls];

                        r = 0;
                        pb++;
                        while (sroma[r++] != '=') ;
                        prs = pk;
                        while (r < sroma.Length) kanabuf[prs++] = sroma[r++];
                        pk = prs;
                    }
                    else
                    {
                        if (tsu)
                            sout += text[pb-1];
                        sout += text[pb];
                    }
                }
        
                if (pk != 0)
                {
                    sout += new string(kanabuf, 0, pk);
                    pk = 0;
                }
            }
    
            return sout;
        }

        public int Normalize(char[] buffer)
        {
            int p, unibuf;

            for (unibuf = p = 0; p < buffer.Length && buffer[p] != 0; p++)
            {
                if (buffer[p] >= 0xFF61 && buffer[p] <= 0xFF9F)
                {
                    switch ((int)buffer[p])
                    {
				        case 0xFF61: buffer[p] = (char)0x3002;	break;
                        case 0xFF62: buffer[p] = (char)0x300C; break;
                        case 0xFF63: buffer[p] = (char)0x300D; break;
                        case 0xFF64: buffer[p] = (char)0x3001; break;
                        case 0xFF65: buffer[p] = (char)0x30FB; break;
                        case 0xFF66: buffer[p] = (char)0x30F2; break;
                    
				        case 0xFF67: case 0xFF68: case 0xFF69: case 0xFF6A: case 0xFF6B:
					        buffer[p] = (char) ((buffer[p] - 0xFF67)*2 + 0x30A1);	break;
                    
				        case 0xFF6C: case 0xFF6D: case 0xFF6E:
					        buffer[p] = (char) ((buffer[p] - 0xFF6C)*2 + 0x30E3); break;

                        case 0xFF6F: buffer[p] = (char)0x30C3; break;
                        case 0xFF70: buffer[p] = (char)0x30FC; break;
                    
				        case 0xFF71: case 0xFF72: case 0xFF73: case 0xFF74: case 0xFF75:
					        buffer[p] = (char) ((buffer[p] - 0xFF71)*2 + 0x30A2); break;
                    
				        case 0xFF76: case 0xFF77: case 0xFF78: case 0xFF79: case 0xFF7A:
				        case 0xFF7B: case 0xFF7C: case 0xFF7D: case 0xFF7E: case 0xFF7F:
				        case 0xFF80: case 0xFF81:
					        buffer[p] = (char) ((buffer[p] - 0xFF76)*2 + 0x30AB); break;
                    
				        case 0xFF82: case 0xFF83: case 0xFF84:
					        buffer[p] = (char) ((buffer[p] - 0xFF82)*2 + 0x30C4); break;
                    
				        case 0xFF85: case 0xFF86: case 0xFF87: case 0xFF88: case 0xFF89:
					        buffer[p] = (char) ((buffer[p] - 0xFF85) + 0x30CA); break;
                    
				        case 0xFF8A: case 0xFF8B: case 0xFF8C: case 0xFF8D: case 0xFF8E:
					        buffer[p] = (char) ((buffer[p] - 0xFF8A)*3 + 0x30CF); break;
                    
				        case 0xFF8F: case 0xFF90: case 0xFF91: case 0xFF92: case 0xFF93:
					        buffer[p] = (char) ((buffer[p] - 0xFF8F) + 0x30DE); break;
                    
				        case 0xFF94: case 0xFF95: case 0xFF96:
					        buffer[p] = (char) ((buffer[p] - 0xFF94)*2 + 0x30E4); break;
                    
				        case 0xFF97: case 0xFF98: case 0xFF99: case 0xFF9A: case 0xFF9B:
					        buffer[p] = (char) ((buffer[p] - 0xFF97) + 0x30E9); break;

                        case 0xFF9C: buffer[p] = (char)0x30EF; break;
                        case 0xFF9D: buffer[p] = (char)0x30F3; break;
                    
				        case 0xFF9E: if (unibuf > 0) buffer[unibuf-1] = (char)(buffer[unibuf-1] + 1); break;
                        case 0xFF9F: if (unibuf > 0) buffer[unibuf-1] = (char)(buffer[unibuf-1] + 2); break;
                    }
                }

                if (buffer[p] != 0xFF9E && buffer[p] != 0xFF9F && buffer[p] != 0x0301)
                    buffer[unibuf++] = hashtab[buffer[p]];
            }
            return unibuf;
        }

        public string Romanate(char[] text, int begin, int end)
	    {
		    string sout = "";
		    int pkana, pk, pi, ps, pb;
		    bool tsu = false;

		    for (pb = begin; pb <= end; pb++) {
			    if ((text[pb] >= 0x3041 && text[pb] <= 0x3094) || (text[pb] >= 0x30A1 && text[pb] <= 0x30FC)) {
				    if (text[pb] == 0x3063 || text[pb] == 0x30C3) {
					    if (pb+1 <= end && ((text[pb+1] >= 0x3041 && text[pb+1] <= 0x3094) ||
						    (text[pb+1] >= 0x30A1 && text[pb+1] <= 0x30FC)))
						    tsu = true;
					    else
						    sout += "ltsu";
					    continue;
				    }

				    for (pkana = kana_count - 1; pkana >= 0; pkana--) {
					    for (pk = 0, pi = pb; pi <= end && pk < kana[pkana].Length && kana[pkana][pk] != '='; pk++, pi++)
						    if (kana[pkana][pk] != text[pi] && kana[pkana][pk] != (text[pi]-0x60)) break;

					    if (kana[pkana][pk] == '=') {
						    ps = pk + 1;

						    if (tsu) {
							    sout += kana[pkana][ps];
							    tsu = false;
						    }

						    sout += new string(kana[pkana].ToCharArray(), ps, kana[pkana].Length - ps);
						    if (text[pb] == 0x3093 && pb+1 <= end && Jaiueoy(text[pb+1])) sout += '\'';

						    pb = pi-1;
						    break;
					    }
				    }

				    if (pkana < 0) sout += text[pb];
			    }
			    else sout += text[pb];
		    }

		    return sout;
	    }
    }
}
