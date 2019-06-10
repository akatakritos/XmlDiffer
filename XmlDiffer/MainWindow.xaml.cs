using System;
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
using System.Xml.Linq;

namespace XmlDiffer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string leftTemp = System.IO.Path.GetTempFileName();
            string rightTemp = System.IO.Path.GetTempFileName();

            FormatAndSave(txtLeft.Text, leftTemp);
            FormatAndSave(txtRight.Text, rightTemp);

            ShowDiff(leftTemp, rightTemp);
        }

        private void FormatAndSave(string xml, string outputPath)
        {
            XElement xe = XElement.Parse(xml);
            SortElement(xe);
            Save(xe, outputPath);
        }

        private void Save(XElement xe, string outputPath)
        {
            using (var sw = new StreamWriter(outputPath))
            {
                SaveRecursive(xe, sw, 0);
            }

        }

        private void SaveRecursive(XElement xe, StreamWriter sw, int indendation)
        {
            Indent(sw, indendation);
            sw.Write("<" + xe.Name);


            foreach(var attr in xe.Attributes())
            {
                sw.WriteLine();
                Indent(sw, indendation + 2);
                sw.Write($"{attr.Name}=\"{attr.Value}\"");
            }

            var children = xe.Nodes().OfType<XElement>().ToList();


            if (children.Any())
            {
                sw.WriteLine(">");

                foreach(var child in xe.Nodes().OfType<XElement>())
                {
                    SaveRecursive(child, sw, indendation + 4);
                }

                Indent(sw, indendation);
                sw.WriteLine("</" + xe.Name + ">");
            }
            else
            {
                sw.WriteLine("/>");
            }
        }

        private void Indent(StreamWriter sw, int indendation)
        {
            for (int i = 0; i < indendation; i++)
                sw.Write(' ');
        }

        private void SortElement(XElement xe)
        {
            IEnumerable<XNode> NodesToBePreserved = xe.Nodes().Where(P => P.GetType() != typeof(XElement));
            xe.ReplaceAttributes(xe.Attributes().OrderBy(x => x.Name.LocalName));
            xe.ReplaceNodes((xe.Elements().OrderBy(x => x.Name.LocalName).Union((NodesToBePreserved).OrderBy(P => P.ToString()))).OrderBy(N => N.NodeType.ToString()));

            foreach (XElement xc in xe.Elements())
            {
                SortElement(xc);
            }
        }

        private void ShowDiff(string leftTemp, string rightTemp)
        {
            string diffMerge = @"C:\Program Files\SourceGear\Common\DiffMerge\sgdm.exe";
            string arguments = $"\"{leftTemp}\" \"{rightTemp}\"";
            Process.Start(diffMerge, arguments);
        }
    }
}
