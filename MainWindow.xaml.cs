﻿using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
using static System.Net.Mime.MediaTypeNames;

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
            string searchFor = MatchStringComparison(FindTextBox.Text, stringComparison);

            if (!SearchInRichTextBox(rtb, searchFor, stringComparison))
            {
                MessageBox.Show(string.Format("Searched for {0} not found!", searchFor));
            }            
        }
        
        // Derived from https://social.msdn.microsoft.com/Forums/silverlight/en-US/a81df766-be43-4292-9924-6ec669cf25a3/richtextbox-search-how-to-scroll-found-searchFor-into-view-select-and-put-cursor-to-it?forum=silverlightcontrols
        private bool SearchInRichTextBox(RichTextBox rtb, string searchFor, StringComparison stringComparison)
        {                      
            string searchForComparison = MatchStringComparison(searchFor, stringComparison); // Match searchFor to StringComparison
            TextRange searchRange = new(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            
            Debug.WriteLine(string.Format("Number of blocks = {0}", rtb.Document.Blocks.Count));

            foreach (Block block in rtb.Document.Blocks)
            {
                searchRange.Select(block.ContentStart, block.ContentEnd);                
                Debug.WriteLine(string.Format("Block text length = {0}", searchRange.Text.Length));

                if (searchRange.Text.Contains(searchForComparison, stringComparison))                
                {
                    if (FindTextInRange(searchRange, searchForComparison) is TextRange textRange)
                    {
                        textRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                        textRange.ApplyPropertyValue(TextElement.FontSizeProperty, 20.0);
                        textRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Green); // TextElement required for BackgroundProperty.
                        textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red); // TextElement not required for ForegroundProperty?

                        Rect startCharRect = textRange.Start.GetCharacterRect(LogicalDirection.Forward);
                        // Attempt to scroll searchForComparison into midpoint (rtb.ActualHeight / 2.0) of view
                        rtb.ScrollToVerticalOffset(startCharRect.Top - rtb.ActualHeight / 2.0);
                        
                        #region Flobbydust - Insert Button
                        Button b = new()
                        { // Unspecified width and button fills width
                            Content = searchForComparison,
                            FontSize = 20,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.Black,
                            Background = Brushes.Green,
                            IsEnabled = true,                            
                        };

                        b.Click += TestButtonClick;
                        b.MouseDown += B_MouseDown;
                        
                        // Add button to block, then insert before current block
                        BlockUIContainer blockUIContainer = new(b);
                        blockUIContainer.MouseDown += B_MouseDown;
                        rtb.Document.Blocks.InsertBefore(block, blockUIContainer);
                        #endregion

                        //rtb.Focus();
                    }
                    return true;
                }
            }

            return false;
        }

        private void B_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private static string MatchStringComparison(string searchFor, StringComparison stringComparison)
        {
            string compare;
            
            switch (stringComparison)
            {
                case StringComparison.Ordinal:
                case StringComparison.CurrentCulture:
                case StringComparison.InvariantCulture:
                    compare = searchFor;
                    break;
                case StringComparison.OrdinalIgnoreCase:
                case StringComparison.CurrentCultureIgnoreCase:
                case StringComparison.InvariantCultureIgnoreCase:
                    compare = searchFor.ToLower();
                    break;
                default: throw new ArgumentException("Unknown StringComparison");
            }

            return compare;
        }

        private static TextRange? FindTextInRange(TextRange searchRange, string searchText)
        {
            // The start position of the search
            TextPointer current = searchRange.Start.GetInsertionPosition(LogicalDirection.Forward);

            while (current != null)
            {
                // The TextRange that contains the current character
                TextRange text = new(current.GetPositionAtOffset(0, LogicalDirection.Forward), 
                    current.GetPositionAtOffset(1, LogicalDirection.Forward));

                // If the current character is the start of the searched searchFor
                if (text.Text == searchText[0].ToString())
                {
                    TextRange match = new(current, current.GetPositionAtOffset(searchText.Length, LogicalDirection.Forward));

                    if (match.Text == searchText)
                    {
                        // Return the match
                        return match;
                    }
                }

                // Move to the next character
                current = current.GetPositionAtOffset(1, LogicalDirection.Forward);
            }

            // Return null if no match found
            return null;
        }

        private static Block? GetBlock(RichTextBox rtb, TextPointer textPointer)
        {
            foreach (Block block in rtb.Document.Blocks)
            {
                TextRange blockRange = new(block.ContentStart, block.ContentEnd);

                if (blockRange.Contains(textPointer))
                {
                    return block;
                }
            }

            return null;
        }
        
        private static void TestButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You pressed that button didn't you!", "Don't Touch", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
