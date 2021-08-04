using System;
using System.Collections.Generic;
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
    public partial class AddVisit : Window
    {
        public AddVisit()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = @"Data Source=DESKTOP-H0BRIQV\SQLEXPRESS01;Initial Catalog=Hotel;Integrated Security=True";
            string sqlExpression =
                String.Format("INSERT INTO tblClient VALUES (@surname, @name, @patronymic, @address, @pasportData)",
                    name.Text.ToString(), surname.Text, patronymic.Text, name.Text, address.Text);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.Parameters.AddWithValue("surname", surname.Text);
                command.Parameters.AddWithValue("patronymic", patronymic.Text);
                command.Parameters.AddWithValue("name", name.Text);
                command.Parameters.AddWithValue("address", address.Text);
                command.Parameters.AddWithValue("pasportData", pasportData.Text);
                command.ExecuteNonQuery();
                connection.Close();
            }

        }
    }
}
