////////////////////////////////////////////////////////////////////////////////////////////////////
/// @file   MainWindow.xaml.cs
/// @author Guillaume Dua
/// @brief  Implements the main window.xaml class.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
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
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;

using GCL;
using System.Diagnostics;

class XML_Serialization
{
    static public void SerializeObject<T>(T serializableObject, string fileName)
    {
        if (serializableObject == null) { return; }

        try
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, serializableObject);
                stream.Position = 0;
                xmlDocument.Load(stream);
                xmlDocument.Save(fileName);
                stream.Close();
            }
        }
        catch (Exception ex)
        {
            GCL.Logger.instance.Write("XML_Serialization::SerializeObject : " + ex);
        }
    }

    static public T DeSerializeObject<T>(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) { return default(T); }

        T objectOut = default(T);

        try
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(fileName);
            string xmlString = xmlDocument.OuterXml;

            using (StringReader read = new StringReader(xmlString))
            {
                Type outType = typeof(T);

                XmlSerializer serializer = new XmlSerializer(outType);
                using (XmlReader reader = new XmlTextReader(read))
                {
                    objectOut = (T)serializer.Deserialize(reader);
                    reader.Close();
                }

                read.Close();
            }
        }
        catch (Exception ex)
        {
            GCL.Logger.instance.Write("XML_Serialization::DeSerializeObject : " + ex);
        }

        return objectOut;
    }
}

namespace RegexStack_CodeRefactoringTool
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class Regex_Replacement
        {
            public Regex_Replacement()
            { }
            public Regex_Replacement(string regex, string replacement, bool? isActive = true)
            {
                _regex = regex;
                _replacement = replacement;
                _isActive = isActive;
            }

            public string _regex = "";
            public string _replacement = "";
            public bool? _isActive = true;
        }

        const string configurationFilePath = "./_configuration_file.xml";

        public MainWindow()
        {
            InitializeComponent();

            GCL.Logger.instance = new GCL.Logger.TextBoxLogger(this.txtBx_Log);
            this.LoadFromFile(configurationFilePath);
        }

        private List<Regex_Replacement> GetRegexReplacementAsList()
        {
            var list_regex_replacement = new List<Regex_Replacement>();

            int colCount = this.grid_Regex.ColumnDefinitions.Count;

            if (this.grid_Regex.Children.Count != colCount)    // Not only the "header-row"
            {
                for (int i = colCount - 1; i < this.grid_Regex.Children.Count; i += colCount)
                {
                    var chckBox_isValid = this.grid_Regex.Children[i] as CheckBox;
                    var txtBx_regex = this.grid_Regex.Children[i + 1] as TextBox;
                    var txtBx_replacement = this.grid_Regex.Children[i + 3] as TextBox;
                    try
                    {
                        list_regex_replacement.Add(new Regex_Replacement(txtBx_regex.Text, txtBx_replacement.Text, chckBox_isValid.IsChecked));
                    }
                    catch (System.Exception)
                    { }
                }
            }

            return list_regex_replacement;
        }

        private void LoadFromFile(string filePath)
        {
            if (!File.Exists(configurationFilePath))
            {
                GCL.Logger.instance.Write(String.Format("Loading configuration file failed : file not found path=[{0}]", configurationFilePath));
                return;
            }

            // Clear existing content
            this.grid_Regex.Children.RemoveRange(3, this.grid_Regex.Children.Count);
            // this.grid_Regex.Children.Clear();

            var list_regex_replacement = new List<Regex_Replacement>();
            list_regex_replacement = XML_Serialization.DeSerializeObject<List<Regex_Replacement>>(configurationFilePath);

            foreach (var elem in list_regex_replacement)
                AddRegexRow(elem);
            GCL.Logger.instance.Write(String.Format("Loaded successfuly from [{0}]", configurationFilePath));
        }

        private void SaveToFile(string filePath)
        {
            List<Regex_Replacement> list_regex_replacement = GetRegexReplacementAsList();
            if (list_regex_replacement.Count == 0)
                GCL.Logger.instance.Write("Nothing to save");
            else
                XML_Serialization.SerializeObject(list_regex_replacement, configurationFilePath);
            GCL.Logger.instance.Write(String.Format("Saved successfuly to [{0}]", configurationFilePath));
        }

        private void AddRegexRow(Regex_Replacement elem)
        {
            // AddRegexRow(elem._regex, elem._replacement, elem._isActive == true);
            try
            {
                int rowIndex = this.grid_Regex.RowDefinitions.Count;

                var rowDefinition = new RowDefinition();
                rowDefinition.SetValue(RowDefinition.HeightProperty, new GridLength(30));

                this.grid_Regex.RowDefinitions.Add(rowDefinition);

                var regexColElem = new TextBox();
                var replacementColElem = new TextBox();
                var emptyTxt = new TextBlock();
                var chckBx_toApply = new CheckBox();

                emptyTxt.Text = "";
                regexColElem.Text = elem._regex;
                replacementColElem.Text = elem._replacement;
                chckBx_toApply.IsChecked = elem._isActive;

                // Checked
                chckBx_toApply.Click += new System.Windows.RoutedEventHandler((sender, e) =>
                {
                    var _this_checkBox = sender as CheckBox;
                    if (_this_checkBox.IsChecked == true)
                    {
                        regexColElem.Background = Brushes.White;
                        replacementColElem.Background = Brushes.White;
                    }
                    else
                    {
                        regexColElem.Background = Brushes.Gray;
                        replacementColElem.Background = Brushes.Gray;
                    }
                });

                if (elem._isActive == true)
                {
                    regexColElem.Background = Brushes.White;
                    replacementColElem.Background = Brushes.White;
                }
                else
                {
                    regexColElem.Background = Brushes.Gray;
                    replacementColElem.Background = Brushes.Gray;
                }

                Grid.SetRow(chckBx_toApply, rowIndex);
                Grid.SetRow(regexColElem, rowIndex);
                Grid.SetRow(emptyTxt, rowIndex);
                Grid.SetRow(replacementColElem, rowIndex);

                Grid.SetColumn(chckBx_toApply, 0);
                Grid.SetColumn(regexColElem, 1);
                Grid.SetColumn(emptyTxt, 2);
                Grid.SetColumn(replacementColElem, 3);

                this.grid_Regex.Children.Add(chckBx_toApply);
                this.grid_Regex.Children.Add(regexColElem);
                this.grid_Regex.Children.Add(emptyTxt);
                this.grid_Regex.Children.Add(replacementColElem);
            }
            catch (System.Exception ex)
            {
                GCL.Logger.instance.Write(ex.ToString());
            }
        }

        private void btn_addRegex_Click(object sender, RoutedEventArgs e)
        {
            AddRegexRow(new Regex_Replacement("",  "", false));
        }
        
        private void btn_Load_Click(object sender, RoutedEventArgs e)
        {
            LoadFromFile(configurationFilePath);
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            SaveToFile(configurationFilePath);
        }
        private String StrInterpretLitterals(string str)
        {
            str = str.Replace(@"\t", "\t");
            str = str.Replace(@"\n", "\n");
            str = str.Replace(@"\r", "\r");

            return str;
        }
        private void btn_analyse_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(txtBx_SourcesPath.Text))
            {
                GCL.Logger.instance.Write(String.Format("[Error] : Analyse failed because [{0}] is not a valid directory", txtBx_SourcesPath.Text));
                return;
            }
            List<Regex_Replacement> list_regex_replacement = GetRegexReplacementAsList();
            if (list_regex_replacement.Count == 0)
            {
                GCL.Logger.instance.Write("No regex to analyse");
                return;
            }

            string[] files = Directory.GetFiles(txtBx_SourcesPath.Text);
            int rowIndex = grid_Regex.ColumnDefinitions.Count - 1;
            int countLinesToModify = 0;
            foreach (var elem in list_regex_replacement)
            {
                var textblock = this.grid_Regex.Children[rowIndex + 2] as TextBlock;
                if (elem._isActive == false)
                {
                    textblock.Text = "";
                    textblock.Background = Brushes.Black;
                }
                else
                {
                    int count = 0;
                    foreach (var file in files)
                    {
                        var fileContent = File.ReadAllText(file);

                        Regex regex = new Regex(StrInterpretLitterals(elem._regex), RegexOptions.IgnoreCase);
                        count += regex.Matches(fileContent).Count;
                    }
                    textblock.Text = count.ToString();
                    countLinesToModify += count;

                    if (count > 50)
                        textblock.Background = Brushes.Green;
                    else if (count > 20)
                        textblock.Background = Brushes.LightGreen;
                    else if (count > 10)
                        textblock.Background = Brushes.Pink;
                    else if (count > 1)
                        textblock.Background = Brushes.Orange;
                    else
                        textblock.Background = Brushes.Red;
                }

                rowIndex += grid_Regex.ColumnDefinitions.Count;
            }

            GCL.Logger.instance.Write(String.Format("Analyse of [{0}] successful. {1} lines to modify", txtBx_SourcesPath.Text, countLinesToModify));
        }
        private void btn_ApplyRegex_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(txtBx_SourcesPath.Text))
            {
                GCL.Logger.instance.Write(String.Format("[Error] : Regex application failed because [{0}] is not a valid directory", txtBx_SourcesPath.Text));
                return;
            }
            List<Regex_Replacement> list_regex_replacement = GetRegexReplacementAsList();
            if (list_regex_replacement.Count == 0)
            {
                GCL.Logger.instance.Write("No regex to analyse");
                return;
            }

            string[] files = Directory.GetFiles(txtBx_SourcesPath.Text);

            int count = 0;
            foreach (var file in files) 
            {
                int strPos;
                if ((strPos = file.LastIndexOf('.')) == -1)
                    continue;
                string fileExtension = file.Substring(strPos, file.Length - strPos);
                if (fileExtension != ".cpp" && fileExtension != ".h")
                    continue;

                var fileContent = File.ReadAllText(file);
                var newFileContent = fileContent;
                foreach (var elem in list_regex_replacement)
                {
                    if (elem._isActive == false)
                        continue;

                    Regex regex = new Regex(StrInterpretLitterals(elem._regex), RegexOptions.IgnoreCase);
                    count += regex.Matches(newFileContent).Count;
                    newFileContent = regex.Replace(newFileContent, StrInterpretLitterals(elem._replacement));
                }
                if (File.Exists(file + "_refactoring"))
                    File.Delete(file + "_refactoring");
                //if (fileContent != newFileContent)
                    File.WriteAllText(file + "_refactoring", newFileContent);
            }

            GCL.Logger.instance.Write(String.Format("Modification of [{0}] successful. {1} lines modified", txtBx_SourcesPath.Text, count));
            CheckRegexApplication(txtBx_SourcesPath.Text);
        }

        private void txtBx_SourcesPath_LostFocus(object sender, RoutedEventArgs e)
        {
            var this_txtBx_sourcePath = sender as TextBox;
            if (!this_txtBx_sourcePath.IsFocused && this_txtBx_sourcePath.Text == "")
                this_txtBx_sourcePath.Text = "Sources path ...";
        }

        private void txtBx_SourcesPath_GotFocus(object sender, RoutedEventArgs e)
        {
            var this_txtBx_sourcePath = sender as TextBox;
            if (this_txtBx_sourcePath.IsFocused && this_txtBx_sourcePath.Text == "Sources path ...")
                this_txtBx_sourcePath.Text = "";
        }
        private void CheckRegexApplication(string dirPath)
        {
            CheckFiles checkThotFileWindow = new CheckFiles(dirPath);
            checkThotFileWindow.Show();
            checkThotFileWindow.Focus();
        }
    }
}
