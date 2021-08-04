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
using System.Windows.Shapes;

namespace LabsDB
{
    /// <summary>
    /// Логика взаимодействия для Reports.xaml
    /// </summary>
    public partial class Reports : Window
    {
        public Reports()
        {
            InitializeComponent();
            Services.Source = new Uri(Directory.GetCurrentDirectory() + "/Reports/report_services.html", UriKind.RelativeOrAbsolute);
            Clients.Source = new Uri(Directory.GetCurrentDirectory() + "/Reports/report_clients.html", UriKind.RelativeOrAbsolute);
            Flats.Source = new Uri(Directory.GetCurrentDirectory() + "/Reports/report_rooms.html", UriKind.RelativeOrAbsolute);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            var selectedItem = (ComboBoxItem)comboBox.SelectedItem;
            var numberRoom = int.Parse(selectedItem.Content.ToString().Substring(1));
            HTMLReport.GenerateHtmlReport(HTMLReport.Report.ReportOnRoom, numberRoom);
            Flats.Source = new Uri(Directory.GetCurrentDirectory() + "/Reports/report_rooms.html", UriKind.RelativeOrAbsolute);
        }
    }
}
