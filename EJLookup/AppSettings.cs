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
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Collections.Generic;

namespace EJLookup
{
    public class AppSettings
    {
        private IsolatedStorageSettings settings;

        private List<string> fontSizeList = new List<string>() { "Нормальный", "Крупный", "Максимальный" };
        private List<string> maxResList = new List<string>() { "100", "250", "500", "1000" };
        private List<string> maxSugList = new List<string>() { "10", "25", "50", "100" };

        public List<string> FontSizeList
        {
            get { return fontSizeList; }
            set { }
        }

        public List<string> MaxResList
        {
            get { return maxResList; }
            set { }
        }

        public List<string> MaxSugList
        {
            get { return maxSugList; }
            set { }
        }

        public AppSettings()
        {
            try
            {
                settings = IsolatedStorageSettings.ApplicationSettings;
            }
            catch (System.IO.IsolatedStorage.IsolatedStorageException e)
            {
                Console.WriteLine(e.Message);
                // handle exception
            }
        }

        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            if (settings.Contains(Key))
            {
                if (settings[Key] != value)
                {
                    settings[Key] = value;
                    valueChanged = true;
                }
            }
            else
            {
                settings.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            return (settings.Contains(Key) ? (T)settings[Key] : defaultValue);
        }

        public bool Romaji
        {
            get { return GetValueOrDefault<bool>("romaji", true); }
            set { if (AddOrUpdateValue("romaji", value)) Save(); }
        }

        public int FontSize
        {
            get { return GetValueOrDefault<int>("fontsize", 0); }
            set { if (AddOrUpdateValue("fontsize", value)) Save(); }
        }

        public int MaxResults
        {
            get { return GetValueOrDefault<int>("maxres", 0); }
            set { if (AddOrUpdateValue("maxres", value)) Save(); }
        }

        public int MaxSuggest
        {
            get { return GetValueOrDefault<int>("maxsug", 0); }
            set { if (AddOrUpdateValue("maxsug", value)) Save(); }
        }

        public bool jredict
        {
            get { return GetValueOrDefault<bool>("jr-edict", true); }
            set { if (AddOrUpdateValue("jr-edict", value)) Save(); }
        }

        public bool warodai
        {
            get { return GetValueOrDefault<bool>("warodai", true); }
            set { if (AddOrUpdateValue("warodai", value)) Save(); }
        }

        public bool edict
        {
            get { return GetValueOrDefault<bool>("edict", true); }
            set { if (AddOrUpdateValue("edict", value)) Save(); }
        }

        public bool kanjidic
        {
            get { return GetValueOrDefault<bool>("kanjidic", true); }
            set { if (AddOrUpdateValue("kanjidic", value)) Save(); }
        }

        public bool ediclsd4
        {
            get { return GetValueOrDefault<bool>("ediclsd4", true); }
            set { if (AddOrUpdateValue("ediclsd4", value)) Save(); }
        }

        public bool classical
        {
            get { return GetValueOrDefault<bool>("classical", true); }
            set { if (AddOrUpdateValue("classical", value)) Save(); }
        }

        public bool compverb
        {
            get { return GetValueOrDefault<bool>("compverb", true); }
            set { if (AddOrUpdateValue("compverb", value)) Save(); }
        }

        public bool compdic
        {
            get { return GetValueOrDefault<bool>("compdic", true); }
            set { if (AddOrUpdateValue("compdic", value)) Save(); }
        }

        public bool lingdic
        {
            get { return GetValueOrDefault<bool>("lingdic", true); }
            set { if (AddOrUpdateValue("lingdic", value)) Save(); }
        }

        public bool jddict
        {
            get { return GetValueOrDefault<bool>("jddict", true); }
            set { if (AddOrUpdateValue("jddict", value)) Save(); }
        }

        public bool jword3
        {
            get { return GetValueOrDefault<bool>("4jword3", true); }
            set { if (AddOrUpdateValue("4jword3", value)) Save(); }
        }

        public bool aviation
        {
            get { return GetValueOrDefault<bool>("aviation", true); }
            set { if (AddOrUpdateValue("aviation", value)) Save(); }
        }

        public bool buddhdic
        {
            get { return GetValueOrDefault<bool>("buddhdic", true); }
            set { if (AddOrUpdateValue("buddhdic", value)) Save(); }
        }

        public bool engscidic
        {
            get { return GetValueOrDefault<bool>("engscidic", true); }
            set { if (AddOrUpdateValue("engscidic", value)) Save(); }
        }

        public bool envgloss
        {
            get { return GetValueOrDefault<bool>("envgloss", true); }
            set { if (AddOrUpdateValue("envgloss", value)) Save(); }
        }

        public bool findic
        {
            get { return GetValueOrDefault<bool>("findic", true); }
            set { if (AddOrUpdateValue("findic", value)) Save(); }
        }

        public bool forsdic_e
        {
            get { return GetValueOrDefault<bool>("forsdic_e", true); }
            set { if (AddOrUpdateValue("forsdic_e", value)) Save(); }
        }

        public bool forsdic_s
        {
            get { return GetValueOrDefault<bool>("forsdic_s", true); }
            set { if (AddOrUpdateValue("forsdic_s", value)) Save(); }
        }

        public bool geodic
        {
            get { return GetValueOrDefault<bool>("geodic", true); }
            set { if (AddOrUpdateValue("geodic", value)) Save(); }
        }

        public bool lawgledt
        {
            get { return GetValueOrDefault<bool>("lawgledt", true); }
            set { if (AddOrUpdateValue("lawgledt", value)) Save(); }
        }

        public bool manufdic
        {
            get { return GetValueOrDefault<bool>("manufdic", true); }
            set { if (AddOrUpdateValue("manufdic", value)) Save(); }
        }

        public bool mktdic
        {
            get { return GetValueOrDefault<bool>("mktdic", true); }
            set { if (AddOrUpdateValue("mktdic", value)) Save(); }
        }

        public bool pandpdic
        {
            get { return GetValueOrDefault<bool>("pandpdic", true); }
            set { if (AddOrUpdateValue("pandpdic", value)) Save(); }
        }

        public bool stardict
        {
            get { return GetValueOrDefault<bool>("stardict", true); }
            set { if (AddOrUpdateValue("stardict", value)) Save(); }
        }

        public bool concrete
        {
            get { return GetValueOrDefault<bool>("concrete", true); }
            set { if (AddOrUpdateValue("concrete", value)) Save(); }
        }


        public void Save()
        {
            settings.Save();
        }
    }
}
