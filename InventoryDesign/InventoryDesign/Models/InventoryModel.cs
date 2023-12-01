using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryDesign.Models
{
    public class Items
    {
        public string item_name { get; set; }
        public string model { get; set; }
        public string brand { get; set; }
        public string serial_number { get; set; }
        public string item_type { get; set; }
        public string room_name { get; set; }
        public string status { get; set; }
        public string department { get; set; }
        public string date_now { get; set; }
    }
    public class Rooms
    {
        public string room_name { get; set; }
        public string floorlevel { get; set; }
        public string room_description { get; set; }
        public string room_id { get; set; }
        public string campus_name { get; set; }
    }
    public class History
    {
        public string item_name { get; set; }
        public string serial_number { get; set; }
        public string date_transfer { get; set; }
        public string last_location { get; set; }
        public string current_location { get; set; }
        public string transfer_by { get; set; }
        public string requested_by { get; set; }
        public string campus_name { get; set; }
    }
    public class Brand
    {
        public string brandname { get; set; }
        


    }
    public class campus
    {
        public string campus_name { get; set; }


    }
    public class Grade
    {
        public string grade_level { get; set; }


    }
    public class Employee
    {
        public string employee_id { get; set; }
        public string employee_name { get; set; }
        public string campus_name { get; set; }
        public string employee_last_name { get; set; }
        public string employee_user_name { get; set; }
        public string employee_password { get; set; }
        public string role { get; set; }

    }
    public class sizes
    {
        public string size { get; set; }


    }
    public class clothes_list
    {
        public string clothes_type { get; set; }


    }
    public class uniform_stock
    {
        public string grade_level { get; set; }
        public string clothes_type { get; set; }
        public string size { get; set; }
        public string quantity { get; set; }
        public string uniform_id { get; set; }
    }
    public class login
    {
        public string employee_id { get; set; }
        public string employee_name { get; set; }
        public string employee_last_name { get; set; }
        public string campus_name { get; set; }
        public string employee_user_name { get; set; }
        public string employee_password { get; set; }
        public string role { get; set; }

    }
    public class claim_stub
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string apparel_name { get; set; }
        public string reciept_number { get; set; }
        public string date_recieve { get; set; }
        public string claim_id { get; set; }
        public string campus_name { get; set; }
        public string grade_level { get; set; }
        public string type { get; set; }
        public string quantity { get; set; }
        public string size { get; set; }
    }
    public class clothes_log
    {
        public string transaction_date { get; set; }
        public string quantity { get; set; }
        public string size { get; set; }
        public string transaction { get; set; }
        public string grade_level { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string id { get; set; }
        public string clothes_type { get; set; }
      
    }
    public class Modelall
    {
        public IEnumerable<Items> Itemlist { get; set; }
        public IEnumerable<Rooms> Roomlist { get; set; }
        public IEnumerable<Brand> Allbrand { get; set; }
        public IEnumerable<History> Historydata { get; set; }
        public IEnumerable<Employee> Allemployee { get; set; }
        public IEnumerable<uniform_stock> Alluniform_stock { get; set; }
        public IEnumerable<Grade> Allgrades { get; set; }
        public IEnumerable<sizes> Allsize { get; set; }
        public IEnumerable<clothes_list> Allclothes { get; set; }
        public IEnumerable<login> loginsystem { get; set; }
        public IEnumerable<clothes_log> Allclotheslog { get; set; }
        public IEnumerable<campus> Allcampus { get; set; }
        public IEnumerable<claim_stub> Allclaimitems { get; set; }
    }

    public class CustomException : Exception
    {
        public CustomException() { }
        public CustomException(string message) : base(message) { }
        public CustomException(string message, Exception innerException) : base(message, innerException) { }
    }

}