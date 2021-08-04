using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Linq;
using System.Data.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data.SqlTypes;

namespace LabsDB
{
    class HTMLReport
    {
        public enum Report
        {
            ReportOnClient,
            ReportOnRoom,
            ReportOnService
        }

        static string connectionString = @"Data Source=DESKTOP-H0BRIQV\SQLEXPRESS01;Initial Catalog=Hotel;Integrated Security=True";
        public Dictionary<int, Tuple<string, float>> TypesOfServices = new Dictionary<int, Tuple<string, float>>();

        static public void GenerateHtmlReport(Report report, int numberRoom = -1)
        {
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\Reports");

            switch ((int)report)
            {
                case 0:
                    GetReportOnClient();
                    break;
                case 1:
                    GetRoomReport(numberRoom);
                    break;
                case 2:
                    GetServiceReport();
                    break;
            }
        }

        static private async Task GetReportOnClient()
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var serviservicesRendered = new Dictionary<string, IElement>();

            var document = await context.
                OpenAsync(req => req.Content(@"<!DOCTYPE html><html><meta content='text/html;charset=UTF-8' ><head></head><body></body></html>"));

            string sqlCommand =
            @"SELECT tblServiceType.txtServiceTypeName, 
	            txtClientSurname, 
	            txtClientName, 
	            txtClientSecondName, 
	            txtClientAddress, 
	            txtClientPassportNumber,
	            SUM(tblService.intServiceCount),
	            SUM(tblService.fltServiceSum)
            FROM tblService 
	            JOIN tblVisit 
	            ON tblVisit.intVisitId = tblService.intVisitId
	            JOIN tblClient 
	            ON tblClient.intClientId = tblVisit.intClientId
	            JOIN tblServiceType
	            ON tblServiceType.intServiceTypeId = tblService.intServiceTypeId
            GROUP BY txtClientSurname, tblServiceType.txtServiceTypeName, txtClientName, txtClientSecondName, txtClientAddress, txtClientPassportNumber
            ORDER BY tblServiceType.txtServiceTypeName";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlCommand, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {

                        var table = document.CreateElement("table");
                        var headerTable = document.CreateElement("caption");
                        headerTable.TextContent = reader.GetValue(0).ToString();
                        table.AppendChild(headerTable);

                        var tr = document.CreateElement("tr");

                        var tdFullName = document.CreateElement("td");
                        tdFullName.TextContent = reader.GetValue(1).ToString() + " " + reader.GetValue(2).ToString() + " " + reader.GetValue(3).ToString();
                        var tdAdress = document.CreateElement("td");
                        tdAdress.TextContent = reader.GetValue(4).ToString();
                        var tdPasssportNumber = document.CreateElement("td");
                        tdPasssportNumber.TextContent = reader.GetValue(5).ToString();
                        var tdServiceCount = document.CreateElement("td");
                        tdServiceCount.TextContent = reader.GetValue(6).ToString();
                        var tdServiceSum = document.CreateElement("td");
                        tdServiceSum.TextContent = reader.GetValue(7).ToString();

                        tr.AppendChild(tdFullName);
                        tr.AppendChild(tdAdress);
                        tr.AppendChild(tdPasssportNumber);
                        tr.AppendChild(tdServiceCount);
                        tr.AppendChild(tdServiceSum);

                        table.AppendChild(tr);
                        try
                        {
                            if (serviservicesRendered.ContainsKey(reader.GetValue(0).ToString()))
                                serviservicesRendered[reader.GetValue(0).ToString()].AppendChild(tr);
                            else
                                serviservicesRendered.Add(reader.GetValue(0).ToString(), table);
                        }
                        catch (Exception ex)
                        {
                            string str = ex.Message;
                        }
                    }
                }
                reader.Close();
            }

            StreamWriter save = new StreamWriter(Directory.GetCurrentDirectory() + @"\Reports\report_clients.html");
            save.Write(document.DocumentElement.OuterHtml);
            save.Close();
        }

        static private async Task GetServiceReport()
        {
            DataContext db = new DataContext(connectionString);
            var clients = db.GetTable<Client>();

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);

            var document = await context.
                OpenAsync(req => req.Content(@"<!DOCTYPE html><html><meta content='text/html;charset=UTF-8' ><head></head><body></body></html>"));

            foreach (var client in clients)
            {
                var h1 = document.CreateElement("h1"); // ФИО, номер паспорта, адрес
                h1.TextContent = client.Name;
                document.Body.AppendChild(h1);

                var visitsOfClient1 = db.ExecuteQuery<Visit>(
                                "Select intVisitId, intClientId, intRoomNumber, datBegin, datEnt, fltroomSum, fltServiceSum " +
                                "FROM tblVisit " +
                                "WHERE tblVisit.intClientId = {0}",
                                client.Id.ToString());
                var visitsOfClient = visitsOfClient1.ToList();

                try
                {
                    foreach (var visit in visitsOfClient)
                    {
                        var h2 = document.CreateElement("h2"); // дата приезда, отьзеда и номер комнаты
                        h2.TextContent = "Дата приезда: " + visit.Begin.ToString() + " Дата отьезда: " + visit.End.ToString() + " Номер комнаты: " + visit.RoomNumber;
                        document.Body.AppendChild(h2);

                        string DateEnd = visit.End.ToShortDateString().Replace(".", "-");
                        string DateBegin = visit.Begin.ToShortDateString().Replace(".", "-");
                        var services =
                            db.ExecuteQuery<Service>(
                                "Select intServiceId, intServiceTypeId, intVisitId, intServiceCount, fltServiceSum, datServiceDate " +
                                "FROM tblService " +
                                "WHERE intVisitId = {0} AND datServiceDate <= {1} AND datServiceDate >= {2}",
                                visit.VisitId, DateEnd, DateBegin);

                        var table = document.CreateElement("table"); // услуги окзанные за визит
                        foreach (var service in services)
                        {
                            var tr = document.CreateElement("tr");
                            var tdServiceType = document.CreateElement("td");
                            var tdDate = document.CreateElement("td");
                            var tdCount = document.CreateElement("td");
                            var tdSum = document.CreateElement("td");

                            tdServiceType.TextContent = service.ServiceTypeId.ToString();
                            tr.AppendChild(tdServiceType);
                            tdDate.TextContent = service.ServiceDate.ToLongDateString();
                            tr.AppendChild(tdDate);
                            tdCount.TextContent = service.ServiceCount.ToString();
                            tr.AppendChild(tdCount);
                            tdSum.TextContent = service.ServiceSum.ToString();
                            tr.AppendChild(tdSum);
                            table.AppendChild(tr);
                        }
                        document.Body.AppendChild(table);
                    }
                }
                catch (Exception ex)
                {
                    string str = ex.Message;
                }
            }

            StreamWriter save = new StreamWriter(Directory.GetCurrentDirectory() + @"\Reports\report_clients.html");
            save.Write(document.DocumentElement.OuterHtml);
            save.Close();
        }

        static private async Task GetRoomReport(int roomNumber)
        {
            DataContext db = new DataContext(connectionString);
            var clients = db.GetTable<Client>();

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);

            var document = await context.
                OpenAsync(req => req.Content(@"<!DOCTYPE html><html><meta content='text/html;charset=UTF-8' ><head></head><body></body></html>"));

            var room = db.ExecuteQuery<LabsDB.Room>(
                "Select intRoomNumber, txtRoomDescription, intFlor, fltRoomPrice " +
                "FROM tblRoom " +
                "WHERE tblRoom.intRoomNumber = {0}",
                roomNumber).ToList()[0];

            var h1 = document.CreateElement("h1"); // Номер комнаты и её стоимость
            h1.TextContent = "Номер комнаты: " + room.RoomNumber + " Этаж: " + room.Flor + " Цена: " + room.RoomPrice + " Описание: " + room.RoomDescription;
            document.Body.AppendChild(h1);

            var visits = db.ExecuteQuery<LabsDB.Visit>(
            "Select intClientId, intRoomNumber, datBegin, datEnt " +
            "FROM tblVisit " +
            "WHERE tblVisit.intRoomNumber = {0} " +
            "ORDER BY datBegin, datEnt",
            roomNumber.ToString()).ToList();

            try
            {
                foreach (var visit in visits)
                {
                    var client =
                        db.ExecuteQuery<Client>(
                            "Select txtClientSurname, txtClientName, txtClientSecondName, txtClientAddress, txtClientPassportNumber " +
                            "FROM tblClient " +
                            "WHERE intClientId = {0}",
                            visit.ClientId).ToList()[0];

                    var tableClients = document.CreateElement("table");

                    var tr = document.CreateElement("tr");
                    var tdFullName = document.CreateElement("td");
                    var tdDateBegin = document.CreateElement("td");
                    var tdDateEnd = document.CreateElement("td");
                    var tdPassportNumber = document.CreateElement("td");
                    var tdClientAddress = document.CreateElement("td");

                    tdFullName.TextContent = client.Name + " " + client.Surname + " " + client.SecondName;
                    tr.AppendChild(tdFullName);
                    tdDateBegin.TextContent = visit.Begin.ToLongDateString();
                    tr.AppendChild(tdDateBegin);
                    tdDateEnd.TextContent = visit.End.ToLongDateString();
                    tr.AppendChild(tdDateEnd);
                    tdPassportNumber.TextContent = "номер паспорта: " + client.PassportNumber;
                    tr.AppendChild(tdPassportNumber);
                    tdClientAddress.TextContent = client.ClientAddress;
                    tr.AppendChild(tdClientAddress);
                    tableClients.AppendChild(tr);

                    document.Body.AppendChild(tableClients);
                }
            }
            catch (Exception ex)
            {
                string stre = ex.Message;
            }

            var services = from s in db.GetTable<Service>()
                           orderby s.ServiceDate
                           select s;

            var tableServices = document.CreateElement("table");
            var headerTable = document.CreateElement("caption");
            headerTable.TextContent = "Услуги предоставленные отелем";
            tableServices.AppendChild(headerTable);
            var trh = document.CreateElement("tr");
            var thServiceType = document.CreateElement("th");
            thServiceType.TextContent = "Название услуги";
            var thSum = document.CreateElement("th");
            thSum.TextContent = "Стоимость";
            var thCount = document.CreateElement("th");
            thCount.TextContent = "Кол-во";
            var thDate = document.CreateElement("th");
            thDate.TextContent = "Дата оказания";
            trh.AppendChild(thServiceType);
            trh.AppendChild(thSum);
            trh.AppendChild(thCount);
            trh.AppendChild(thDate);
            tableServices.AppendChild(trh);

            foreach (var service in services)
            {
                var tr = document.CreateElement("tr");

                var tdServiceType = document.CreateElement("td");
                tdServiceType.TextContent = service.ServiceTypeId.ToString();
                var tdSum = document.CreateElement("td");
                tdSum.TextContent = service.ServiceSum.ToString();
                var tdCount = document.CreateElement("td");
                tdCount.TextContent = service.ServiceCount.ToString();
                var tdDate = document.CreateElement("td");
                tdDate.TextContent = service.ServiceDate.ToLongDateString();

                tr.AppendChild(tdServiceType);
                tr.AppendChild(tdSum);
                tr.AppendChild(tdCount);
                tr.AppendChild(tdDate);
                tableServices.AppendChild(tr);
            }
            document.Body.AppendChild(tableServices);

            StreamWriter save = new StreamWriter(Directory.GetCurrentDirectory() + @"\Reports\report_rooms.html");
            save.Write(document.DocumentElement.OuterHtml);
            save.Close();
        }


    }

}
