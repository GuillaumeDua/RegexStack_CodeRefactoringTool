using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GCL
{
    [Serializable]
    [AttributeUsage(AttributeTargets.All)]
    class MetaConfiguration : System.Attribute
    {
        public static readonly string CONFIGURATION_FILE_PATH = @".\config.txt";

        public MetaConfiguration()
        { }
        ~MetaConfiguration()
        {
            // Commented for convenient tests
            Serialize();
        }

        public override string ToString()
        {
            string ret = "";

            foreach (var field in this.GetType().GetFields())
                ret += "[" + field.Name + "]=[" + field.GetValue(this) + "]\n";
            return ret;
        }
        public string[] ToStringArray()
        {
            List<string> ret = new List<string>();

            foreach (var field in this.GetType().GetFields())
                ret.Add("[" + field.Name + "]=[" + field.GetValue(this) + "]");
            return ret.ToArray();
        }

        private void FieldTextBoxChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var textBox = sender as TextBox;
                this.GetType().GetField(textBox.Name).SetValue(this, textBox.Text);
            }
            catch (System.Exception ex)
            {
                Logger.instance.Write("[Error]::[System.Exception] : Configuration.FieldTextBoxChanged : " + ex.ToString());
            }
        }

        public void Load()
        {
            if ((this.DeSerialize()) == false)
                this.GetFromWindowForm();
        }

        //public static Configuration GetInstance()
        //{
        //    Configuration config;
        //    if ((config = DeSerialize()) == null)
        //        config = GetFromWindowForm();
        //    return config;
        //}
        protected void Serialize()
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(CONFIGURATION_FILE_PATH, FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, this);
                stream.Close();
            }
            catch (System.Exception ex)
            {
                Logger.instance.Write("[Error]::[System.Exception] : Configuration.Serialize : [" + ex.ToString() + "]");
            }
        }
        protected bool DeSerialize()
        {
            try
            {
                MetaConfiguration configuration;
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(CONFIGURATION_FILE_PATH, FileMode.Open, FileAccess.Read, FileShare.Read);
                configuration = (MetaConfiguration)formatter.Deserialize(stream);

                foreach (var field in this.GetType().GetFields())
                {
                    if (field.IsPublic && !field.IsStatic)
                    {
                        field.SetValue(this, configuration.GetType().GetField(field.Name).GetValue(configuration));
                    }
                }

                stream.Close();

                return true;
            }
            catch (System.Exception)
            {
                // Logger.Write("[Error]::[System.Exception] : Configuration.DeSerialize : [" + ex.ToString() + "]");
                return false;
            }
        }
        public void GetFromWindowForm()
        {
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };

            foreach (var field in this.GetType().GetFields())
            {
                if (field.IsPublic && !field.IsStatic)
                {
                    Grid grid = new Grid { HorizontalAlignment = HorizontalAlignment.Stretch };
                    grid.Children.Add(new Label { Content = field.Name });
                    var value = field.GetValue(this);
                    var textBox = new TextBox { Text = (value == null ? "" : value).ToString(), Width = 300, Name = field.Name };
                    textBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.FieldTextBoxChanged);
                    grid.Children.Add(textBox);

                    stackPanel.Children.Add(grid);
                }
            }

            Window window = new Window { Height = this.GetType().GetFields().Length * 40, Width = 600 }; // Background="#FF1B1A1A"
            window.Content = stackPanel;

            window.ShowDialog();
        }
    }
}
