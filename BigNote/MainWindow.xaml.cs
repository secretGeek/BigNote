namespace BigNote
{
    using System.Linq;
    using System.Windows;
    using System.Reflection;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Collections.Generic;
    using BigNote.Models;
    using System.IO;
    using Microsoft.Win32;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string fileName = null;
        List<Replacement> replacements = null;
        List<string> snippets = new List<string>();
        int snippetIndex = 0;
        private int colorIndex = 0;
        private int fontIndex = 0;
        private PropertyInfo[] colorProperties;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindowLoaded;
            KeyDown += MainWindowKeyDown;
            colorProperties = typeof(Colors).GetProperties();
            settingsExpander.Collapsed += SettingsCollapsed;
        }

        private void SettingsCollapsed(object sender, RoutedEventArgs e)
        {
            mainTextBox.Focus();
        }

        protected override void OnActivated(System.EventArgs e)
        {
            base.OnActivated(e);
            mainTextBox.Focus();
            settingsExpander.Opacity = 100;
        }

        protected override void OnDeactivated(System.EventArgs e)
        {
            base.OnDeactivated(e);
            settingsExpander.Opacity = 0;
        }

        void MainWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (mainTextBox.Text == "")
                {
                    WindowState = System.Windows.WindowState.Minimized;
                }
                else
                {
                    ClearCurrentText();
                }
            }

            if (e.Key == Key.F2)
            {
                ToggleFont();
            }

            if (e.Key == Key.F3)
            {
                ToggleColor();
            }
        }

        private void ClearCurrentText()
        {
            mainTextBox.Text = "";
            mainTextBox.Focus();
        }

        private void ToggleColor()
        {
            if (colorIndex < colorProperties.Length - 1)
            {
                colorIndex++;
            }
            else
            {
                colorIndex = 0;
            }

            mainTextBox.Foreground = new SolidColorBrush((Color)colorProperties[colorIndex].GetValue(null, null));
        }

        private void ToggleFont()
        {
            mainTextBox.FontFamily = Fonts.SystemFontFamilies.Skip(fontIndex).First();
            fontIndex++;

            if (fontIndex >= Fonts.SystemFontFamilies.Count)
            {
                fontIndex = 0;
            }
        }

        void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                string defaultFileName = @"C:\users\leon\dropbox\secretgeek\all_someday_projects\ddd_linux_core\demo\scripts.md";
                var f = new OpenFileDialog();

                if (File.Exists(defaultFileName))
                {
                    f.InitialDirectory = Path.GetDirectoryName(defaultFileName);
                    f.FileName = Path.GetFileName(defaultFileName);
                }

                f.Title = "Select snippet file";
                var result = f.ShowDialog();

                if (result == true
                    && File.Exists(f.FileName))
                {
                    fileName = f.FileName;
                }
                else
                {
                    // Failed to pick a file to load snippets from?
                    // Time to die, application.
                    Application.Current.Shutdown();
                    //Application.Exit();
                    return;
                }
            }

            //this.replacements = replacements ?? Replacement.CreateList("45.55.151.240`45.55.151.239,ddd-test-4`ddd-test-3");
            this.replacements = replacements ?? Replacement.CreateList("45.55.151.240`45.55.151.239,138.197.25.125`138.197.29.193,ddd-test-4`ddd-test-3");

            LoadSnippets(fileName);
            LoadReplacements(replacements);
            GetSnippet(snippetIndex);
        }

        private void LoadReplacements(List<Replacement> replacements)
        {
            //contextMenuStrip1.Items.Clear();
            cm.Items.Clear();
            //var pMenu = new ContextMenu();
            
            foreach (var replacement in replacements)
            {
                MenuItem m = new MenuItem { Header = replacement.ReplaceThis + " -> " + replacement.WithThis };
                m.Click += editItemToolStripMenuItem_Click;
                var addedId = cm.Items.Add(m);
                m.Tag = replacement;
                
                //var added = contextMenuStrip1.Items.Add(replacement.ReplaceThis + " -> " + replacement.WithThis, null, editItemToolStripMenuItem_Click);
                //added.Tag = replacement;
            }

            MenuItem addItem = new MenuItem { Header = "Add {ReplaceThis}`{WithThis} token..." };
            addItem.Click += addItemToolStripMenuItem_Click;
            cm.Items.Add(addItem);


            //contextMenuStrip1.Items.Add("Add {ReplaceThis}`{WithThis} token...", null, addItemToolStripMenuItem_Click);
            mainTextBox.Focus();
        }

        private void addItemToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = Prompt.ShowDialog("Enter {ReplaceThis}`{WithThis} Token pair (separate with `)", "Add {ReplaceThis}`{WithThis} Token pair");

            if (!string.IsNullOrWhiteSpace(item))
            {
                if (item.IndexOf("`") <= 0)
                {
                    MessageBox.Show("Please enter a string in the form\r\n\"{ReplaceThis}`{WithThis}\"\r\n... note backtick separator.", "Missing backtick in Replace`With string");
                }
                else
                {
                    var replacement = new Replacement(item);
                    replacements.Add(replacement);
                    LoadReplacements(replacements);
                }
            }
        }

        private void editItemToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            var replacement = menuItem.Tag as Replacement;
            var defaultText = replacement.ReplaceThis + "`" + replacement.WithThis;

            var item = Prompt.ShowDialog("Enter {ReplaceThis}`{WithThis} Token pair (separate with `)", "Edit {ReplaceThis}`{WithThis} Token pair", defaultText);

            if (!string.IsNullOrWhiteSpace(item))
            {
                if (item.IndexOf("`") <= 0)
                {
                    MessageBox.Show("Please enter a string in the form\r\n\"{ReplaceThis}`{WithThis}\"\r\n... note backtick separator.", "Missing backtick in Replace`With string");
                }
                else
                {
                    replacements.Remove(replacement);
                    var newReplacement = new Replacement(item);
                    replacements.Add(newReplacement);
                    LoadReplacements(replacements);
                }
            }
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            Prev();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void Next()
        {
            snippetIndex++;
            if (snippetIndex >= snippets.Count)
                snippetIndex = 0;

            GetSnippet(snippetIndex);
        }

        private void Prev()
        {
            snippetIndex--;
            if (snippetIndex < 0)
                snippetIndex = snippets.Count - 1;

            GetSnippet(snippetIndex);
        }

        private void GetSnippet(int snippetIndex)
        {
            var rawSnippet = snippets[snippetIndex];
            var updatedSnippet = rawSnippet;

            foreach (var r in replacements)
            {
                updatedSnippet = updatedSnippet.Replace(r.ReplaceThis, r.WithThis);
            }

            mainTextBox.Text = updatedSnippet;
            Clipboard.SetDataObject(updatedSnippet, true);
        }

        private void LoadSnippets(string fileName)
        {
            var lines = File.ReadAllLines(fileName);
            var currentSnippet = string.Empty;

            foreach (var l in lines)
            {
                if (l.TrimStart().StartsWith("#"))
                {
                    if (currentSnippet != string.Empty)
                    {
                        snippets.Add(currentSnippet.TrimEnd("\n".ToCharArray()));
                    }

                    currentSnippet = string.Empty;
                    continue;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(l))
                    {
                        currentSnippet += l + "\n";
                    }
                }
            }
        }
    }
}
