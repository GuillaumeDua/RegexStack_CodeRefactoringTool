////////////////////////////////////////////////////////////////////////////////////////////////////
/// @file   CheckFiles.xaml.cs
/// @author Guillaume Dua
/// @brief  Implements the check files.xaml class. 
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
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;

namespace RegexStack_CodeRefactoringTool
{
    /// <summary>
    /// Logique d'interaction pour CheckFiles.xaml
    /// </summary>
    public partial class CheckFiles : Window
    {
        public CheckFiles(String DirPath)
        {
            InitializeComponent();
            LoadCheckRows(DirPath);
        }

        private void LoadCheckRows(String DirPath)
        {
            string[] files = Directory.GetFiles(DirPath);
            foreach (var file in files)
            {
                int strPos;
                if ((strPos = file.LastIndexOf('.')) == -1)
                    continue;
                string fileExtension = file.Substring(strPos, file.Length - strPos);
                if (fileExtension != ".cpp" && fileExtension != ".h")
                    continue;

                if (File.Exists(file + "_thot_refactoring"))
                {
                    AddCheckRow(file);
                }
            }
        }
        private int GetFilesDiff(String filePath)
        {
            string[] fileContent = File.ReadAllLines(filePath);
            string[] fileContentThotRefactoring = File.ReadAllLines(filePath + "_thot_refactoring");

            int diffSize = fileContent.Count() - fileContentThotRefactoring.Count();
            diffSize = diffSize < 0 ? diffSize * -1 : diffSize;
            int maxSize = (fileContentThotRefactoring.Length < fileContent.Length ? fileContentThotRefactoring.Length : fileContent.Length) - 1;
            for (int i = 0; i < maxSize; ++i)
            {
                if (fileContentThotRefactoring[i] != fileContent[i])
                    ++diffSize;
            }
            return diffSize;
        }
        private void AddCheckRow(string filePath)
        {
            try
            {
                int rowIndex = this.grid_FileCheck.RowDefinitions.Count;

                var rowDefinition = new RowDefinition();
                rowDefinition.SetValue(RowDefinition.HeightProperty, new GridLength(30));

                this.grid_FileCheck.RowDefinitions.Add(rowDefinition);

                var fileColElem = new TextBox();
                fileColElem.Text = filePath;
                fileColElem.IsReadOnly = true;

                Grid.SetRow(fileColElem, rowIndex);
                Grid.SetColumn(fileColElem, 0);

                this.grid_FileCheck.Children.Add(fileColElem);

                var checkButton = new Button();

                int diffSize = GetFilesDiff(filePath);
                if (diffSize != 0)
                {
                    checkButton.Content = String.Format("Merge ({0})", diffSize);
                    checkButton.Click += new System.Windows.RoutedEventHandler((sender, e) =>
                    {
                        Process meldCheckProcess = Process.Start
                            (
                                @"C:\Program Files (x86)\Meld\meld\meld.exe"
                            , String.Format("\"{0}\" \"{1}\"", filePath, filePath + "_thot_refactoring")
                            );
                        //meldCheckProcess.Exited += new EventHandler((a, b) =>
                        //    {
                        //        GCL.Logger.instance.Write("[DEBUG] : meldCheckProcess.Exited : Called");
                        //    });
                        meldCheckProcess.WaitForExit();
                        int newDiffSize = GetFilesDiff(filePath);
                        checkButton.Content = String.Format("Merge ({0})", newDiffSize);
                        if (newDiffSize > 1000)
                            fileColElem.Background = Brushes.Red;
                        else if (newDiffSize > 500)
                            fileColElem.Background = Brushes.OrangeRed;
                        else if (newDiffSize > 100)
                            fileColElem.Background = Brushes.Orange;
                        else if (newDiffSize > 50)
                            fileColElem.Background = Brushes.Pink;
                        else if (newDiffSize > 1)
                            fileColElem.Background = Brushes.LightPink;
                        else
                        {
                            checkButton.Content = "OK";
                            checkButton.IsEnabled = false;
                            fileColElem.Background = Brushes.Green;
                        }
                    });

                    if (diffSize > 1000)
                        fileColElem.Background = Brushes.Red;
                    else if (diffSize > 500)
                        fileColElem.Background = Brushes.OrangeRed;
                    else if (diffSize > 100)
                        fileColElem.Background = Brushes.Orange;
                    else if (diffSize > 50)
                        fileColElem.Background = Brushes.Pink;
                    else
                        fileColElem.Background = Brushes.LightPink;
                }
                else
                {
                    checkButton.Content = "OK";
                    checkButton.IsEnabled = false;
                    fileColElem.Background = Brushes.Green;
                }

                Grid.SetRow(checkButton, rowIndex);
                Grid.SetColumn(checkButton, 1);

                this.grid_FileCheck.Children.Add(checkButton);
            }
            catch (System.Exception ex)
            {
                GCL.Logger.instance.Write(ex.ToString());
            }
        }
    }
}
