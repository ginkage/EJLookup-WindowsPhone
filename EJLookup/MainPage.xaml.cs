using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.ComponentModel;

namespace EJLookup
{
    public partial class MainPage : PhoneApplicationPage
    {
        private BackgroundWorker worker;
        private bool afterFill;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Nihongo.global = new Nihongo();
            afterFill = false;
        }

        private class SearchInfo
        {
            public string SearchText { get; set; }
            public List<ResultLine> Results { get; set; }
        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !SearchBox.Text.Equals(""))
            {
                if (worker != null)
                {
                    worker.CancelAsync();
                    worker = null;
                }

                ResultLB.Items.Clear();
                SearchBox.IsEnabled = false;
                WaitBar.Visibility = Visibility.Visible;
                WaitBar.IsIndeterminate = true;

                var searchInfo = new SearchInfo
                {
                    SearchText = SearchBox.Text,
                };

                worker = new BackgroundWorker();
                worker.WorkerReportsProgress = false;
                worker.WorkerSupportsCancellation = false;
                worker.DoWork += new DoWorkEventHandler(DictionaryLookup);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnSearchComplete);
                worker.RunWorkerAsync(searchInfo);
            }
        }

        private void DictionaryLookup(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            SearchInfo searchInfo = e.Argument as SearchInfo;
            searchInfo.Results = Dictionary.Lookup(searchInfo.SearchText);
            if (!worker.CancellationPending)
                e.Result = searchInfo;
        }

        private void AddCenteredText(string Text)
        {
            RichTextBox gtb = new RichTextBox();
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            run.Text = Text;
            run.FontSize = 18;
            run.Foreground = new SolidColorBrush(Color.FromArgb(255, 214, 214, 214));
            paragraph.Inlines.Add(run);
            gtb.HorizontalAlignment = HorizontalAlignment.Center;
            gtb.Blocks.Add(paragraph);
            ResultLB.Items.Add(gtb);
        }

        private void OnSearchComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            SearchInfo searchInfo = e.Result as SearchInfo;
            if (!e.Cancelled && searchInfo != null)
            {
                List<ResultLine> result = searchInfo.Results;
                string lastgroup = "";

                AppSettings settings = new AppSettings();
                int[] kanji = { 26, 35, 44 };
                int[] kana = { 20, 26, 33 };
                int kanjiSize = kanji[settings.FontSize];
                int kanaSize = kana[settings.FontSize];

                foreach (ResultLine line in result)
                {
                    if (!line.group.Equals(lastgroup))
                    {
                        AddCenteredText(line.group + "\n");
                        lastgroup = line.group;
                    }

                    RichTextBox tb = new RichTextBox();
                    tb.IsReadOnly = true;
                    tb.Blocks.Add(line.FormatLine(kanjiSize, kanaSize));
                    ContextMenu contextMenu = new ContextMenu();
                    MenuItem menuItem = new MenuItem() { Header = "Копировать текст", Tag = "Copy" };
                    contextMenu.Items.Add(menuItem);
                    menuItem.DataContext = tb;
                    menuItem.Click += ContextMenuItem_Click;
                    ContextMenuService.SetContextMenu(tb, contextMenu);

                    ResultLB.Items.Add(tb);
                }

                if (lastgroup.Equals(""))
                    AddCenteredText("\n\nНичего не найдено");
            }

            WaitBar.IsIndeterminate = false;
            WaitBar.Visibility = Visibility.Collapsed;
            SearchBox.IsEnabled = true;
            afterFill = true;
            worker = null;
        }

        private class PopulateInfo
        {
            public AutoCompleteBox AutoCompleteBox { get; set; }
            public string SearchText { get; set; }
            public IEnumerable<string> Results { get; set; }
            public int maxsug { get; set; }
            public bool romanize { get; set; }
        }

        private void SearchBox_Populating(object sender, PopulatingEventArgs e)
        {
            e.Cancel = true;

            if (worker != null)
            {
                worker.CancelAsync();
                worker = null;
            }

            AppSettings settings = new AppSettings();
            int[] limit = { 10, 25, 50, 100 };

            var populateInfo = new PopulateInfo
            {
                AutoCompleteBox = sender as AutoCompleteBox,
                SearchText = (sender as AutoCompleteBox).SearchText,
                maxsug = limit[settings.MaxSuggest],
                romanize = settings.Romaji
            };

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = false;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(SuggestLookup);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnPopulateComplete);
            worker.RunWorkerAsync(populateInfo);
        }

        private void SuggestLookup(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            PopulateInfo populateInfo = e.Argument as PopulateInfo;
            populateInfo.Results = Suggest.Lookup(populateInfo.SearchText, worker, populateInfo.maxsug, populateInfo.romanize);
            if (!worker.CancellationPending)
                e.Result = populateInfo;
        }

        private void OnPopulateComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            PopulateInfo populateInfo = e.Result as PopulateInfo;
            if (!e.Cancelled && populateInfo != null && populateInfo.SearchText == populateInfo.AutoCompleteBox.SearchText)
            {
                populateInfo.AutoCompleteBox.ItemsSource = populateInfo.Results;
                populateInfo.AutoCompleteBox.PopulateComplete();
            }
            worker = null;
        }

        private void ResultLB_LayoutUpdated(object sender, EventArgs e)
        {
            if (afterFill)
            {
                if (ResultLB.Items.Count > 0)
                {
                    ResultLB.ScrollIntoView(ResultLB.Items.First());
                    ResultLB.Focus();
                }
                afterFill = false;
            }
        }

        private void ApplicationBarSettings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void ApplicationBarAbout_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = false;
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = true;
        }

        private void ContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (sender as MenuItem);
            RichTextBox textbox = (item.DataContext as RichTextBox);
            TextPointer start = textbox.Selection.Start, end = textbox.Selection.End;
            textbox.SelectAll();
            string text = textbox.Selection.Text;
            textbox.Selection.Select(start, end);
            Clipboard.SetText(text);
        }
    }
}