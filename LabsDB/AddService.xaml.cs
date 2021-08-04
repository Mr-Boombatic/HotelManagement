using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SqlClient;
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
    /// Логика взаимодействия для AddService.xaml
    /// </summary>
    public partial class AddService : Window
    {
        public AddService()
        {
            InitializeComponent(); 
            GetServicesTypes();
        }

        public int IdClient;
        string connectionString = @"Data Source=DESKTOP-H0BRIQV\SQLEXPRESS01;Initial Catalog=Hotel;Integrated Security=True";
        List<ServiceType> servicesInfo;
        public void GetServicesTypes() 
        {
            DataContext db = new DataContext(connectionString);
            servicesInfo = new List<ServiceType>();
            var services = from s in db.GetTable<ServiceType>()
                           select s;

            foreach (var seriveceType in services)
            {
                this.ServicesTypes.Items.Add(seriveceType.ServiceName);
                servicesInfo.Add(new ServiceType() { Id = seriveceType.Id, Price = seriveceType.Price, ServiceName = seriveceType.ServiceName });
            }
        }

        private void AddServiceForClient(object sender, RoutedEventArgs e)
        {
            if (CountService.Text != "")
            {
                var service = servicesInfo.Where(s => (s.ServiceName == ServicesTypes.Text)).First();
                string sqlExpression = "INSERT INTO tblService VALUES ( "
                    + service.Id + ", "
                    + IdClient.ToString() + ", "
                    + CountService.Text + ", "
                    + int.Parse(CountService.Text) * service.Price + ", "
                    + "\'" + calendar.SelectedDate.Value.Year + "-" + calendar.SelectedDate.Value.Month + "-" + calendar.SelectedDate.Value.Day + "\'" + " )";
                
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    int number = command.ExecuteNonQuery();
                    Console.WriteLine("Добавлено объектов: {0}", number);
                }
            }    
        }
    }
}
