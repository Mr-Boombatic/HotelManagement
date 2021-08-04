using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LabsDB
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CreateMainTable();
        }

        SqlConnection sqlConnection;
        static string connectionString = @"Data Source=DESKTOP-H0BRIQV\SQLEXPRESS01;Initial Catalog=Hotel;Integrated Security=True";
        public async void CreateMainTable()
        {
            sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();
            SqlDataReader sqlReader = null;
            SqlCommand command = new SqlCommand("SELECT * FROM [tblVisit]", sqlConnection);

            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                List<ClientInfo> Customers = new List<ClientInfo>();
                while (await sqlReader.ReadAsync())
                {
                    Customers.Add(new ClientInfo
                    {
                        ArrivalDate = sqlReader["datBegin"].ToString(),
                        DepartureDate = sqlReader["datEnt"].ToString(),
                        NameCustomer = GetNameCustomer(connectionString, Convert.ToInt32(sqlReader["intClientId"])),
                        RoomNumber = sqlReader["intRoomNumber"].ToString(),
                        Floor = GetFloorOfClient(connectionString, Convert.ToInt32(sqlReader["intRoomNumber"])),
                        CostOfServices = sqlReader["fltServiceSum"].ToString()
                    }); ;
                }
                CustomersInformation.ItemsSource = Customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sqlReader != null)
                    sqlReader.Close();
            }

            sqlConnection.Close();
        }

        public string GetNameCustomer(string connectionString, int customerId)
        {
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM [tblClient]", connectionString);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            return ds.Tables[0].Rows[customerId - 1][1].ToString() + ' ' + ds.Tables[0].Rows[customerId - 1][2].ToString() + ' ' + ds.Tables[0].Rows[customerId - 1][3].ToString();
        }

        public string GetFloorOfClient(string connectionString, int roomNumber)
        {
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM [tblRoom]", connectionString);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            return ds.Tables[0].Rows[roomNumber - 1][2].ToString();
        }
        public class ClientInfo
        {
            public string ArrivalDate { get; set; }
            public string DepartureDate { get; set; }
            public string NameCustomer { get; set; }
            public string RoomNumber { get; set; }
            public string Floor { get; set; }
            public string CostOfServices { get; set; }

        }

        public class ClientService
        {
            public int ServiceType { get; set; }
            public string Count { get; set; }
            public string Sum { get; set; }
            public string Date { get; set; }
            public string Cost { get; set; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddVisit taskWindow = new AddVisit();
            taskWindow.Show();
        }

        int IdClient;
        private void SelectClient(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dgr;
            if (sender != null)
            {
                DataGrid grid = sender as DataGrid;
                if (grid != null && grid.SelectedItems != null && grid.SelectedItems.Count == 1)
                {
                    Dispatcher.BeginInvoke((Action)(() => Hotel.SelectedIndex = 1));

                    dgr = grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem) as DataGridRow;
                    DataContext db = new DataContext(connectionString);

                    var selectedRow = (ClientInfo)CustomersInformation.Items[dgr.GetIndex()];
                    var name = selectedRow.NameCustomer.Split(new char[] { ' ' })[1];
                    var surname = selectedRow.NameCustomer.Split(new char[] { ' ' })[0];
                    var secondname = selectedRow.NameCustomer.Split(new char[] { ' ' })[2];

                    var queryClietInfo = from s in db.GetTable<Client>()
                                         where s.Name == name && s.Surname == surname && s.SecondName == secondname
                                         select s;

                    this.fullname.Content = queryClietInfo.Single<Client>().Surname + " " + queryClietInfo.Single<Client>().Name + " " + queryClietInfo.Single<Client>().SecondName;
                    this.numberPassport.Content = queryClietInfo.Single<Client>().PassportNumber;
                    this.adress.Content = queryClietInfo.Single<Client>().ClientAddress;
                    var idClient = queryClietInfo.Single<Client>().Id;
                    string visitRoomNumber = selectedRow.RoomNumber;

                    var queryVisitsInfoClient = db.GetTable<Visit>().Where(v => v.ClientId == idClient).ToList();
                    var clientRenderedServices = new List<ClientService>();
                    foreach (var visit in queryVisitsInfoClient)
                    {
                        var queryGetServiceOfClient = from s in db.GetTable<Service>()
                                                      where s.VisitId == visit.VisitId
                                                      select s;
                        if (int.Parse(visitRoomNumber) == visit.RoomNumber)
                            IdClient = visit.VisitId;
                        foreach (var service in queryGetServiceOfClient)
                        {
                            clientRenderedServices.Add(new ClientService()
                            {
                                ServiceType = service.ServiceTypeId,
                                Cost = service.ServiceSum.ToString(),
                                Sum = service.ServiceSum.ToString(),
                                Count = service.ServiceCount.ToString(),
                                Date = service.ServiceDate.ToString(),
                            });
                        }
                    }
                    this.ClientServiceGrid.ItemsSource = clientRenderedServices;
                }
            }
        }
        private void AddServiceForClient(object sender, RoutedEventArgs e)
        {
            var page = new AddService() { IdClient =  this.IdClient};
            page.Owner = this;
            page.Show();
        }

        private void Update(object sender, RoutedEventArgs e)
        {
            this.SelectClient(this.CustomersInformation, null);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            HTMLReport.GenerateHtmlReport(HTMLReport.Report.ReportOnClient);
            HTMLReport.GenerateHtmlReport(HTMLReport.Report.ReportOnService);
            HTMLReport.GenerateHtmlReport(HTMLReport.Report.ReportOnRoom, 1);
            var page = new Reports();
            page.Owner = this;
            page.Show();
        }
    }
}
