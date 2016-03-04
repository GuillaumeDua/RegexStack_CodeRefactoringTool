using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GCL
{
    public class Logger
    {
        public static Interface instance = null;

        public interface Interface
        {
            void Write(string msg);
            void Write(string[] msg);
        }
        public class TextBoxLogger : GCL.Logger.Interface
        {
            public TextBoxLogger(TextBox textBox)
            {
                logTextBox = textBox;
            }

            public TextBox logTextBox = null;

            //void Interface.Write(string msg)
            public void Write(string msg)
            {
                if (logTextBox == null)
                    throw new System.NullReferenceException("Logger.logTextBox is null");
                if (msg.Length == 0)
                    return;

                logTextBox.Text += "[" + System.DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + "] : " + msg + "\n";
                logTextBox.Focus();
                logTextBox.CaretIndex = logTextBox.Text.Length;
                logTextBox.ScrollToEnd();
            }
            public void Write(string[] msg)
            {
                foreach (var line in msg)
                {
                    Write(line);
                }
            }
        }
    }
}