using System;
using System.Data.Linq.Mapping;

namespace LabsDB
{
    [Table(Name = "tblClient")]
    class Client
    {
        [Column(Name = "intClientId")]
        public int Id { get; set; }

        [Column(Name = "txtClientName")]
        public string Name { get; set; }

        [Column(Name = "txtClientSurname")]
        public string Surname { get; set; }

        [Column(Name = "txtClientSecondName")]
        public string SecondName { get; set; }

        [Column(Name = "txtClientAddress")]
        public string ClientAddress { get; set; }

        [Column(Name = "txtClientPassportNumber")]
        public string PassportNumber { get; set; }

    }

    [Table(Name = "tblVisit")]
    class Visit
    {
        [Column(Name = "intClientId")]
        public int ClientId { get; set; }

        [Column(Name = "intVisitId")]
        public int VisitId { get; set; }

        [Column(Name = "intRoomNumber")]
        public int RoomNumber { get; set; }

        [Column(Name = "datBegin")]
        public DateTime Begin { get; set; }

        [Column(Name = "datEnt")]
        public DateTime End { get; set; }

        [Column(Name = "fltroomSum")]
        public System.Data.SqlTypes.SqlMoney RoomSum { get; set; }

        [Column(Name = "fltServiceSum")]
        public System.Data.SqlTypes.SqlMoney ServiceSum { get; set; }
    }

    [Table(Name = "tblService")]
    class Service
    {
        [Column(Name = "intServiceId")]
        public int ServiceId { get; set; }

        [Column(Name = "intServiceTypeId")]
        public int ServiceTypeId { get; set; }

        [Column(Name = "intVisitId")]
        public int VisitId { get; set; }

        [Column(Name = "intServiceCount")]
        public int ServiceCount { get; set; }

        [Column(Name = "fltServiceSum")]
        public System.Data.SqlTypes.SqlMoney ServiceSum { get; set; }

        [Column(Name = "datServiceDate")]
        public DateTime ServiceDate { get; set; }
    }

    [Table(Name = "tblRoom")]
    class Room
    {
        [Column(Name = "intRoomNumber")]
        public int RoomNumber { get; set; }

        [Column(Name = "txtRoomDescription")]
        public string RoomDescription { get; set; }

        [Column(Name = "intFlor")]
        public int Flor { get; set; }

        [Column(Name = "fltRoomPrice")]
        public System.Data.SqlTypes.SqlMoney RoomPrice { get; set; }
    }

    [Table(Name = "tblServiceType")]
    class ServiceType
    {
        [Column(Name = "intServiceTypeId")]
        public int Id { get; set; }

        [Column(Name = "txtServiceTypeName")]
        public string ServiceName { get; set; }

        [Column(Name = "fltServiceTypePrice")]
        public System.Data.SqlTypes.SqlMoney Price { get; set; }
    }
}
