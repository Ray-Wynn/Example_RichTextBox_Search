﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Example_RichTextBox_Search
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            StringComparisonComboBox.ItemsSource = Enum.GetValues(typeof(StringComparison));
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MemoryStream memLicStream = new(Encoding.Default.GetBytes(Resource1.test));
            rtb.Selection.Load(memLicStream, DataFormats.Rtf);            
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Rich Text files (*.rtf)|*.rtf",
                CheckFileExists = true, // True is the default
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadRtfScriptFile(openFileDialog.FileName);
            }
        }

        void LoadRtfScriptFile(string _fileName)
        {
            TextRange range;
            FileStream fStream;

            if (File.Exists(_fileName))
            {
                range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                fStream = new FileStream(_fileName, FileMode.OpenOrCreate);
                range.Load(fStream, DataFormats.Rtf);
                fStream.Close();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FindTextBox.Text))
                return;

            StringComparison stringComparison = (StringComparison)StringComparisonComboBox.SelectedItem;
            SearchInRichTextBox(rtb, FindTextBox.Text, stringComparison);
        }

        // Derived from https://social.msdn.microsoft.com/Forums/silverlight/en-US/a81df766-be43-4292-9924-6ec669cf25a3/richtextbox-search-how-to-scroll-found-text-into-view-select-and-put-cursor-to-it?forum=silverlightcontrols
        private static void SearchInRichTextBox(RichTextBox rtb, string searchFor, StringComparison stringComparison)
        {
            TextRange searchRange = new(rtb.Document.ContentStart, rtb.Document.ContentEnd);

            foreach (Block block in rtb.Document.Blocks)
            {
                searchRange.Select(block.ContentStart, block.ContentEnd);
                int index = searchRange.Text.IndexOf(searchFor, 0, stringComparison);

                if (index >= 0)
                {
                    TextPointer current = searchRange.Start.GetPositionAtOffset(index, LogicalDirection.Forward);
                    Rect startCharRect = current.GetCharacterRect(LogicalDirection.Forward);

                    // Attempt to scroll searchFor into midpoint (rtb.ActualHeight / 2.0) of view
                    rtb.ScrollToVerticalOffset(startCharRect.Top - rtb.ActualHeight / 2.0);
                    return;
                }
            }
        }        
    }
}