using InventoryDesign.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace InventoryDesign.Controllers
{
    public class InventoryController : Controller
    {
        //SHOW ALL ITEMS

        public ActionResult item_list(string campus_name)
        {
            Modelall mymodel = new Modelall();
            mymodel.Itemlist = Allitems(campus_name);
            mymodel.Allbrand = Brands(campus_name);
            mymodel.Roomlist = Allrooms(campus_name);
            ViewBag.campus_name = campus_name;
            return View("~/Views/Inventory/item_list.cshtml", mymodel);
        }

        public IEnumerable<Items> Allitems(string campus_name)
        {

            IEnumerable<Items> ec = null;
            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/item/");

            var consumedata = hc.GetAsync("all?campus_name=" + campus_name);
            consumedata.Wait();


            var dataread = consumedata.Result;
            if (dataread.IsSuccessStatusCode)
            {
                var results = dataread.Content.ReadAsAsync<IList<Items>>();
                results.Wait();
                ec = results.Result;
            }
            else if (dataread.StatusCode == HttpStatusCode.NotFound)
            {
                ec = Enumerable.Empty<Items>();
                ViewBag.Message = "No Locality Found.";
            }
            else
            {
                ec = Enumerable.Empty<Items>();
                ViewBag.Message = "Unable to display locality, please email to it@golden-success.com for help.";
            }
            return ec;
        }

        //ADD ITEMS 

        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult createitems(string add_item_name, string add_model, string add_brand, string add_serial_number, string add_item_type, string add_item_status, string add_item_department, string add_date, string add_room, string add_campus)
        {
            try
            {
                string msg;
                try
                {
                    WebRequest req;
                    WebResponse res;
                    string postData = "item_name=" + add_item_name + "&model=" + add_model + "&brand=" + add_brand + "&serial_number=" + add_serial_number + "&item_type=" + add_item_type + "&status=" + add_item_status + "&department=" + add_item_department + "&date_now=" + add_date + "&room_name=" + add_room + "&campus_name=" + add_campus;
                    req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/add/newitems?" + postData);
                    Byte[] data = Encoding.UTF8.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    req.ContentLength = data.Length;

                    Stream stream = req.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();

                    using (res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        msg = reader.ReadToEnd();

                        // Redirect back to the referring page
                        return Redirect(Request.UrlReferrer.ToString());
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse res = (HttpWebResponse)ex.Response;
                    Stream receiveStream = res.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content(readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
        }



        //UPDATE ITEMS 

        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult updateitems(string update_item_name, string update_model, string update_brand, string update_serial_number, string update_item_type, string update_item_status, string update_department, string update_room_name)
        {

            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(User.Identity.Name);
            try
            {
                string msg;
                try
                {
                    WebRequest req;
                    WebResponse res;
                    string postData = "item_name=" + update_item_name + "&model=" + update_model + "&brand=" + update_brand + "&serial_number=" + update_serial_number + "&item_type=" + update_item_type + "&status=" + update_item_status + "&department=" + update_department + "&room_name=" + update_room_name;
                    req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/update/items?" + postData);
                    Byte[] data = Encoding.UTF8.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    req.ContentLength = data.Length;

                    Stream stream = req.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();

                    using (res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        msg = reader.ReadToEnd();

                        // Redirect back to the referring page
                        return Redirect(Request.UrlReferrer.ToString());
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse res = (HttpWebResponse)ex.Response;
                    Stream receiveStream = res.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content(readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
        }

        // SHOW ALL BRANDS

        public ActionResult brandlist(string campus_name)
        {
            Modelall mymodel = new Modelall();
            mymodel.Allbrand = Brands(campus_name);
            ViewBag.campus_name = campus_name;
            return View("~/Views/Inventory/brandlist.cshtml", mymodel);
        }

        public IEnumerable<Brand> Brands(string campus_name)
        {

            IEnumerable<Brand> ec = null;
            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/brand/");

            var consumedata = hc.GetAsync("list?campus_name=" + campus_name);
            consumedata.Wait();


            var dataread = consumedata.Result;
            if (dataread.IsSuccessStatusCode)
            {
                var results = dataread.Content.ReadAsAsync<IList<Brand>>();
                results.Wait();
                ec = results.Result;
            }
            else if (dataread.StatusCode == HttpStatusCode.NotFound)
            {
                ec = Enumerable.Empty<Brand>();
                ViewBag.Message = "No Locality Found.";
            }
            else
            {
                ec = Enumerable.Empty<Brand>();
                ViewBag.Message = "Unable to display locality, please email to it@golden-success.com for help.";
            }
            return ec;
        }

        // ADD BRAND

        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult addbrand(string add_brand, string add_campus)
        {

            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(User.Identity.Name);
            try
            {
                string msg;
                try
                {
                    WebRequest req;
                    WebResponse res;
                    string postData = "brandname=" + add_brand + "&campus_name=" + add_campus;
                    req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/add/brand?" + postData);
                    Byte[] data = Encoding.UTF8.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    req.ContentLength = data.Length;

                    Stream stream = req.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();
                    using (res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        msg = reader.ReadToEnd();

                        // Redirect back to the referring page
                        return Redirect(Request.UrlReferrer.ToString());
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse res = (HttpWebResponse)ex.Response;
                    Stream receiveStream = res.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content(readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
        }


        // SHOW ALL ROOMS 

        public ActionResult rooms(string campus_name)
        {
            Modelall mymodel = new Modelall();
            mymodel.Roomlist = Allrooms(campus_name);
            ViewBag.campus_name = campus_name;

            return View("~/Views/Inventory/rooms.cshtml", mymodel);
        }

        public IEnumerable<Rooms> Allrooms(string campus_name)
        {
            try
            {
                IEnumerable<Rooms> ec = null;
                HttpClient hc = new HttpClient();
                hc.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/room/");

                var consumedata = hc.GetAsync("campus?campus_name=" + campus_name);
                consumedata.Wait();

                var dataread = consumedata.Result;
                if (dataread.IsSuccessStatusCode)
                {
                    var results = dataread.Content.ReadAsAsync<IList<Rooms>>();
                    results.Wait();
                    ec = results.Result;
                }
                else if (dataread.StatusCode == HttpStatusCode.NotFound)
                {
                    ec = Enumerable.Empty<Rooms>();
                    ViewBag.Message = "No Rooms Found for the selected campus.";
                }
                else
                {
                    ec = Enumerable.Empty<Rooms>();
                    ViewBag.Message = "Unable to retrieve room data. Please contact support for assistance.";
                }
                return ec;
            }
            catch (Exception ex)
            {

                ViewBag.Message = "An error occurred: " + ex.Message;
                return Enumerable.Empty<Rooms>();
            }
        }


        //ADD ROOMS

        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult createrooms(string add_room_name, string add_floor_level, string add_room_description, string add_campus_name)
        {
            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(User.Identity.Name);
            try
            {
                string msg;
                try
                {
                    WebRequest req;
                    WebResponse res;
                    string postData = "room_name=" + add_room_name + "&floorlevel=" + add_floor_level + "&room_description=" + add_room_description + "&campus_name=" + add_campus_name;
                    req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/add/newroom?" + postData);
                    Byte[] data = Encoding.UTF8.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    req.ContentLength = data.Length;

                    Stream stream = req.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();

                    using (res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        msg = reader.ReadToEnd();

                        // Get the URL of the referring page and redirect to it
                        string referrerUrl = Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : "/";
                        return Redirect(referrerUrl);
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse res = (HttpWebResponse)ex.Response;
                    Stream receiveStream = res.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content(readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
        }



        //UPDATE ROOM

        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult updaterooms(string update_room_name, string update_floorlevel, string update_room_description, string update_room_id)
        {

            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(User.Identity.Name);
            try
            {
                string msg;
                try
                {
                    WebRequest req;
                    WebResponse res;
                    string postData = "room_name=" + update_room_name + "&floorlevel=" + update_floorlevel + "&room_description=" + update_room_description + "&room_id=" + update_room_id;
                    req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/update/room?" + postData);
                    Byte[] data = Encoding.UTF8.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    req.ContentLength = data.Length;

                    Stream stream = req.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();

                    using (res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        msg = reader.ReadToEnd();

                        // Get the URL of the referring page and redirect to it
                        string referrerUrl = Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : "/";
                        return Redirect(referrerUrl);
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse res = (HttpWebResponse)ex.Response;
                    Stream receiveStream = res.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content(readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
        }


        //ITEMS INSIDE THE ROOM
        public ActionResult insideroom(string room_name, string campus_name)
        {
            Modelall mymodel = new Modelall();
            mymodel.Itemlist = roomname(room_name, campus_name);
            mymodel.Roomlist = Allrooms(campus_name);
            mymodel.Allemployee = employee_acc(campus_name);
            mymodel.Allbrand = Brands(campus_name);
            mymodel.Allcampus = campuses();
            ViewBag.room_name = room_name;
            ViewBag.campus_name = campus_name;

            return View("~/Views/Inventory/insideroom.cshtml", mymodel);
        }

        public IEnumerable<Items> roomname(string room_name, string campus_name)
        {
            IEnumerable<Items> ec = null;
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    hc.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/inside/");

                    // Construct the URI with query parameters
                    string uri = $"room?room_name={room_name}&campus_name={campus_name}";

                    var consumedata = hc.GetAsync(uri);
                    consumedata.Wait();

                    var dataread = consumedata.Result;
                    if (dataread.IsSuccessStatusCode)
                    {
                        var results = dataread.Content.ReadAsAsync<IList<Items>>();
                        results.Wait();
                        ec = results.Result;
                    }
                    else if (dataread.StatusCode == HttpStatusCode.NotFound)
                    {
                        ec = Enumerable.Empty<Items>();
                        ViewBag.Message = "No Locality Found.";
                    }
                    else
                    {
                        ec = Enumerable.Empty<Items>();
                        ViewBag.Message = "Unable to display locality, please email to it@golden-success.com for help.";
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately (e.g., log them)
                ViewBag.Message = "An error occurred: " + ex.Message;
                ec = Enumerable.Empty<Items>();
            }

            return ec;
        }



        //TRANSFER ITEMS

        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult transfer(string transfer_serial_no, string transfer_room_name, string transfer_requested_by, string transfer_by, string transfer_campus_name)
        {
            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(User.Identity.Name);
            try
            {
                string msg;
                try
                {
                    WebRequest req;
                    WebResponse res;
                    string postData = "serial_number=" + transfer_serial_no + "&room_name=" + transfer_room_name + "&transfer_by=" + transfer_by + "&requested_by=" + transfer_requested_by + "&campus_name=" + transfer_campus_name;
                    req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/transfer/items?" + postData);
                    Byte[] data = Encoding.UTF8.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    req.ContentLength = data.Length;

                    Stream stream = req.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();

                    using (res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        msg = reader.ReadToEnd();

                        // Get the referring URL (current page)
                        string referringUrl = Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : "";

                        // Redirect back to the referring URL (current page)
                        return Redirect(referringUrl);
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse res = (HttpWebResponse)ex.Response;
                    Stream receiveStream = res.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content(readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
        }



        // SHOW ALL EMPLOYEE

        public ActionResult employeelist(string campus_name)
        {
            Modelall mymodel = new Modelall();
            mymodel.Allemployee = employee_acc(campus_name);
            ViewBag.campus_name = campus_name;

            return View("~/Views/Inventory/employeelist.cshtml", mymodel);
        }

        public IEnumerable<Employee> employee_acc(string campus_name)
        {

            IEnumerable<Employee> ec = null;
            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/employee/");

            var consumedata = hc.GetAsync("list?campus_name=" + campus_name);
            consumedata.Wait();


            var dataread = consumedata.Result;
            if (dataread.IsSuccessStatusCode)
            {
                var results = dataread.Content.ReadAsAsync<IList<Employee>>();
                results.Wait();
                ec = results.Result;
            }
            else if (dataread.StatusCode == HttpStatusCode.NotFound)
            {
                ec = Enumerable.Empty<Employee>();
                ViewBag.Message = "No Locality Found.";
            }
            else
            {
                ec = Enumerable.Empty<Employee>();
                ViewBag.Message = "Unable to display locality, please email to it@golden-success.com for help.";
            }
            return ec;
        }



        //ADD EMPLOYEE

        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult addemployee(string add_employee, string add_campus_name)
        {

            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(User.Identity.Name);
            try
            {
                string msg;
                try
                {
                    WebRequest req;
                    WebResponse res;
                    string postData = "employee_name=" + add_employee + "&campus_name=" + add_campus_name;
                    req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/add/employeename?" + postData);
                    Byte[] data = Encoding.UTF8.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    req.ContentLength = data.Length;

                    Stream stream = req.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();

                    using (res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        msg = reader.ReadToEnd();

                        // Get the URL of the referring page and redirect to it
                        string referrerUrl = Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : "/";
                        return Redirect(referrerUrl);
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse res = (HttpWebResponse)ex.Response;
                    Stream receiveStream = res.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content(readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
        }


        // SHOW ALL Clothes Apparel List

        public ActionResult uniform_stock(string campus_name)
        {
            Modelall mymodel = new Modelall();
            mymodel.Alluniform_stock = uniformstock(campus_name);
            mymodel.Allgrades = grades(campus_name);
            mymodel.Allsize = sizes(campus_name);
            mymodel.Allclothes = clothes_type(campus_name);
            ViewBag.campus_name = campus_name;
            return View("~/Views/Inventory/uniform_stock.cshtml", mymodel);
        }

        public IEnumerable<uniform_stock> uniformstock(string campus_name)
        {

            IEnumerable<uniform_stock> ec = null;
            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/uniform/");

            var consumedata = hc.GetAsync("stock?campus_name=" + campus_name);
            consumedata.Wait();


            var dataread = consumedata.Result;
            if (dataread.IsSuccessStatusCode)
            {
                var results = dataread.Content.ReadAsAsync<IList<uniform_stock>>();
                results.Wait();
                ec = results.Result;
            }
            else if (dataread.StatusCode == HttpStatusCode.NotFound)
            {
                ec = Enumerable.Empty<uniform_stock>();
                ViewBag.Message = "No Locality Found.";
            }
            else
            {
                ec = Enumerable.Empty<uniform_stock>();
                ViewBag.Message = "Unable to display locality, please email to it@golden-success.com for help.";
            }
            return ec;
        }


        // ADD UNIFORM

        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult adduniform(string add_grade_level, string add_uniform_type, string add_size, string add_stock, string add_campus_name)
        {

            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(User.Identity.Name);
            try
            {
                string msg;
                try
                {
                    WebRequest req;
                    WebResponse res;
                    string postData = "grade_level=" + add_grade_level + "&clothes_type=" + add_uniform_type + "&size=" + add_size + "&quantity=" + add_stock + "&campus_name=" + add_campus_name;
                    req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/add/uniform?" + postData);
                    Byte[] data = Encoding.UTF8.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    req.ContentLength = data.Length;

                    Stream stream = req.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();

                    using (res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        msg = reader.ReadToEnd();


                        string referringUrl = Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : "";


                        return Redirect(referringUrl);
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse res = (HttpWebResponse)ex.Response;
                    Stream receiveStream = res.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content(readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
        }


        // ADD Quantity

        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult addstock(string add_uniform_id, string add_stock, string add_transaction, string add_grade_level, string add_clothes_type, string add_size, string add_campus_name)
        {

            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(User.Identity.Name);
            try
            {
                string msg;
                try
                {
                    WebRequest req;
                    WebResponse res;
                    string postData = "uniform_id=" + add_uniform_id + "&stockChange=" + add_stock + "&transaction=" + add_transaction + "&grade_level=" + add_grade_level + "&clothes_type=" + add_clothes_type + "&size=" + add_size + "&campus_name=" + add_campus_name;
                    req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/add/stock?" + postData);
                    Byte[] data = Encoding.UTF8.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    req.ContentLength = data.Length;

                    Stream stream = req.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();

                    using (res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        msg = reader.ReadToEnd();

                        // Get the referring URL (current page)
                        string referringUrl = Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : "";

                        // Redirect back to the referring URL (current page)
                        return Redirect(referringUrl);
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse res = (HttpWebResponse)ex.Response;
                    Stream receiveStream = res.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content(readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
        }


        //SHOW ALL ITEM HISTORY

        public ActionResult itemhistory(string campus_name)
        {
            Modelall mymodel = new Modelall();
            mymodel.Historydata = allhistory(campus_name);
            ViewBag.campus_name = campus_name;
            return View("~/Views/Inventory/itemhistory.cshtml", mymodel);
        }

        public IEnumerable<History> allhistory(string campus_name)
        {

            IEnumerable<History> ec = null;
            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/history/");

            var consumedata = hc.GetAsync("list?campus_name=" + campus_name);
            consumedata.Wait();


            var dataread = consumedata.Result;
            if (dataread.IsSuccessStatusCode)
            {
                var results = dataread.Content.ReadAsAsync<IList<History>>();
                results.Wait();
                ec = results.Result;
            }
            else if (dataread.StatusCode == HttpStatusCode.NotFound)
            {
                ec = Enumerable.Empty<History>();
                ViewBag.Message = "No Locality Found.";
            }
            else
            {
                ec = Enumerable.Empty<History>();
                ViewBag.Message = "Unable to display locality, please email to it@golden-success.com for help.";
            }
            return ec;
        }

        // SHOW ALL GRADES

        public ActionResult grade_list(string campus_name)
        {
            Modelall mymodel = new Modelall();
            mymodel.Allgrades = grades(campus_name);
            ViewBag.campus_name = campus_name;


            return View("~/Views/Inventory/grade_list.cshtml", mymodel);
        }

        public IEnumerable<Grade> grades(string campus_name)
        {

            IEnumerable<Grade> ec = null;
            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/grade/");

            var consumedata = hc.GetAsync("list?campus_name=" + campus_name);
            consumedata.Wait();


            var dataread = consumedata.Result;
            if (dataread.IsSuccessStatusCode)
            {
                var results = dataread.Content.ReadAsAsync<IList<Grade>>();
                results.Wait();
                ec = results.Result;
            }
            else if (dataread.StatusCode == HttpStatusCode.NotFound)
            {
                ec = Enumerable.Empty<Grade>();
                ViewBag.Message = "No Locality Found.";
            }
            else
            {
                ec = Enumerable.Empty<Grade>();
                ViewBag.Message = "Unable to display locality, please email to it@golden-success.com for help.";
            }
            return ec;
        }

        //ADD Grade level
        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult addgradelevel(string add_grade_level, string add_campus_name)
        {

            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(User.Identity.Name);
            try
            {
                string msg;
                try
                {
                    WebRequest req;
                    WebResponse res;
                    string postData = "grade_level=" + add_grade_level + "&campus_name=" + add_campus_name;
                    req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/add/gradelevel?" + postData);
                    Byte[] data = Encoding.UTF8.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    req.ContentLength = data.Length;

                    Stream stream = req.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();

                    using (res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        msg = reader.ReadToEnd();

                        // Get the referring URL (current page)
                        string referringUrl = Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : "";

                        // Redirect back to the referring URL (current page)
                        return Redirect(referringUrl);
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse res = (HttpWebResponse)ex.Response;
                    Stream receiveStream = res.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content(readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
        }


        //LIST OF ACCOUNT

        public ActionResult Login()
        {
            Modelall mymodel = new Modelall();
            mymodel.loginsystem = loginaccount();
            mymodel.Allcampus = campuses();
            return View("~/Views/Shared/Login.cshtml", mymodel);
        }

        public IEnumerable<login> loginaccount()
        {

            IEnumerable<login> ec = null;
            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/account/");

            var consumedata = hc.GetAsync("list");
            consumedata.Wait();


            var dataread = consumedata.Result;
            if (dataread.IsSuccessStatusCode)
            {
                var results = dataread.Content.ReadAsAsync<IList<login>>();
                results.Wait();
                ec = results.Result;
            }
            else if (dataread.StatusCode == HttpStatusCode.NotFound)
            {
                ec = Enumerable.Empty<login>();
                ViewBag.Message = "No Locality Found.";
            }
            else
            {
                ec = Enumerable.Empty<login>();
                ViewBag.Message = "Unable to display locality, please email to it@golden-success.com for help.";
            }
            return ec;
        }



        //LOG IN
        [HttpPost]

        public ActionResult loginaccount(string add_user_name, string add_password)
        {
            try
            {
                WebRequest req;
                WebResponse res;
                string postData = "employee_user_name=" + add_user_name + "&employee_password=" + add_password;
                req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/Login/all?" + postData);
                Byte[] data = Encoding.UTF8.GetBytes(postData);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                req.ContentLength = data.Length;

                Stream stream = req.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();

                using (res = req.GetResponse())
                using (var reader = new StreamReader(res.GetResponseStream()))
                {
                    string msg = reader.ReadToEnd();

                    if (msg.Contains("Successfully Logged In"))
                    {

                        return RedirectToAction("Dashboard");
                    }
                    else if (msg.Contains("Username and password are incorrect"))
                    {

                        return RedirectToAction("Login");
                    }
                    else
                    {

                        return Content("Unhandled API response: " + msg, "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions here, if needed
                return Content("Unhandled exception: " + ex.Message, "text/plain", Encoding.UTF8);
            }
        }


        //Create account

        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult createaccount(string employee_firstname, string employee_last_name, string employee_user_name, string employee_password, string employee_assign_campus)
        {

            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(User.Identity.Name);
            try
            {
                string msg;
                try
                {
                    WebRequest req;
                    WebResponse res;
                    string postData = "employee_name=" + employee_firstname + "&employee_last_name=" + employee_last_name + "&employee_user_name=" + employee_user_name + "&employee_password=" + employee_password + "&campus_name=" + employee_assign_campus;
                    req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/create/account?" + postData);
                    Byte[] data = Encoding.UTF8.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    req.ContentLength = data.Length;

                    Stream stream = req.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();

                    using (res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        msg = reader.ReadToEnd();

                        // Get the referring URL (current page)
                        string referringUrl = Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : "";

                        // Redirect back to the referring URL (current page)
                        return Redirect(referringUrl);
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse res = (HttpWebResponse)ex.Response;
                    Stream receiveStream = res.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content(readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
        }



        //DASHBOARD
        public ActionResult Dashboard()
        {
             return View();
        }




        //SHOW ALL SIZES

        public ActionResult size_list(string campus_name)
        {
            Modelall mymodel = new Modelall();
            mymodel.Allsize = sizes(campus_name);
            ViewBag.campus_name = campus_name;
            return View("~/Views/Inventory/size_list.cshtml", mymodel);
        }

        public IEnumerable<sizes> sizes(string campus_name)
        {

            IEnumerable<sizes> ec = null;
            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/size/");

            var consumedata = hc.GetAsync("list?campus_name=" + campus_name);
            consumedata.Wait();


            var dataread = consumedata.Result;
            if (dataread.IsSuccessStatusCode)
            {
                var results = dataread.Content.ReadAsAsync<IList<sizes>>();
                results.Wait();
                ec = results.Result;
            }
            else if (dataread.StatusCode == HttpStatusCode.NotFound)
            {
                ec = Enumerable.Empty<sizes>();
                ViewBag.Message = "No Locality Found.";
            }
            else
            {
                ec = Enumerable.Empty<sizes>();
                ViewBag.Message = "Unable to display locality, please email to it@golden-success.com for help.";
            }
            return ec;
        }


        //ADD Size

        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult addsizes(string add_size, string add_campus_name)
        {

            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(User.Identity.Name);
            try
            {
                string msg;
                try
                {
                    WebRequest req;
                    WebResponse res;
                    string postData = "size=" + add_size + "&campus_name=" + add_campus_name;
                    req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/add/sizes?" + postData);
                    Byte[] data = Encoding.UTF8.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    req.ContentLength = data.Length;

                    Stream stream = req.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();


                    using (res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        msg = reader.ReadToEnd();

                        // Get the referring URL (current page)
                        string referringUrl = Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : "";

                        // Redirect back to the referring URL (current page)
                        return Redirect(referringUrl);
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse res = (HttpWebResponse)ex.Response;
                    Stream receiveStream = res.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content(readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
        }


        //SHOW ALL CLOTHES

        public ActionResult Clothes_list(string campus_name)
        {
            Modelall mymodel = new Modelall();
            mymodel.Allclothes = clothes_type(campus_name);
            ViewBag.campus_name = campus_name;


            return View("~/Views/Inventory/Clothes_list.cshtml", mymodel);
        }

        public IEnumerable<clothes_list> clothes_type(string campus_name)
        {

            IEnumerable<clothes_list> ec = null;
            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/clothes/");

            var consumedata = hc.GetAsync("list?campus_name=" + campus_name);
            consumedata.Wait();


            var dataread = consumedata.Result;
            if (dataread.IsSuccessStatusCode)
            {
                var results = dataread.Content.ReadAsAsync<IList<clothes_list>>();
                results.Wait();
                ec = results.Result;
            }
            else if (dataread.StatusCode == HttpStatusCode.NotFound)
            {
                ec = Enumerable.Empty<clothes_list>();
                ViewBag.Message = "No Locality Found.";
            }
            else
            {
                ec = Enumerable.Empty<clothes_list>();
                ViewBag.Message = "Unable to display locality, please email to it@golden-success.com for help.";
            }
            return ec;
        }

        //ADD APPAREL TYPE

        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult addClothestype(string add_clothes_type, string add_campus_name)
        {

            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(User.Identity.Name);
            try
            {
                string msg;
                try
                {
                    WebRequest req;
                    WebResponse res;
                    string postData = "clothes_type=" + add_clothes_type + "&campus_name=" + add_campus_name;
                    req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/add/clothestype?" + postData);
                    Byte[] data = Encoding.UTF8.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    req.ContentLength = data.Length;

                    Stream stream = req.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();

                    using (res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        msg = reader.ReadToEnd();

                        // Get the referring URL (current page)
                        string referringUrl = Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : "";

                        // Redirect back to the referring URL (current page)
                        return Redirect(referringUrl);
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse res = (HttpWebResponse)ex.Response;
                    Stream receiveStream = res.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content(readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
        }

        //SHOW ALL CLOTHES LOG

        public ActionResult clotheslog(string campus_name)
        {
            Modelall mymodel = new Modelall();
            mymodel.Allclotheslog = clothes_log(campus_name);
            ViewBag.campus_name = campus_name;
            return View("~/Views/Inventory/clotheslog.cshtml", mymodel);
        }

        public IEnumerable<clothes_log> clothes_log(string campus_name)
        {

            IEnumerable<clothes_log> ec = null;
            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/clothes/");

            var consumedata = hc.GetAsync("log?campus_name=" + campus_name);
            consumedata.Wait();


            var dataread = consumedata.Result;
            if (dataread.IsSuccessStatusCode)
            {
                var results = dataread.Content.ReadAsAsync<IList<clothes_log>>();
                results.Wait();
                ec = results.Result;
            }
            else if (dataread.StatusCode == HttpStatusCode.NotFound)
            {
                ec = Enumerable.Empty<clothes_log>();
                ViewBag.Message = "No Locality Found.";
            }
            else
            {
                ec = Enumerable.Empty<clothes_log>();
                ViewBag.Message = "Unable to display locality, please email to it@golden-success.com for help.";
            }
            return ec;
        }


        //SHOW ALL Campus

        public ActionResult Campus()
        {
            Modelall mymodel = new Modelall();
            mymodel.Allcampus = campuses();
            mymodel.loginsystem = loginaccount();

            return View("~/Views/Inventory/Campus.cshtml", mymodel);
        }

        public IEnumerable<campus> campuses()
        {

            IEnumerable<campus> ec = null;
            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/campus/");

            var consumedata = hc.GetAsync("list");
            consumedata.Wait();


            var dataread = consumedata.Result;
            if (dataread.IsSuccessStatusCode)
            {
                var results = dataread.Content.ReadAsAsync<IList<campus>>();
                results.Wait();
                ec = results.Result;
            }
            else if (dataread.StatusCode == HttpStatusCode.NotFound)
            {
                ec = Enumerable.Empty<campus>();
                ViewBag.Message = "No Locality Found.";
            }
            else
            {
                ec = Enumerable.Empty<campus>();
                ViewBag.Message = "Unable to display locality, please email to it@golden-success.com for help.";
            }
            return ec;
        }



        // ADD CAMPUS
        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult AddCampus(string add_campus)
        {


            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(User.Identity.Name);

            try
            {
                WebRequest req;
                WebResponse res;
                string postData = "campus_name=" + add_campus;
                req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/add/campus?" + postData);
                Byte[] data = Encoding.UTF8.GetBytes(postData);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                req.ContentLength = data.Length;

                Stream stream = req.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();

                using (res = req.GetResponse())
                using (var reader = new StreamReader(res.GetResponseStream()))
                {
                    // Check the response for success or failure
                    string responseText = reader.ReadToEnd();
                    if (responseText == "Successfully added campus")
                    {
                        // Redirect to the "Campus" page on success
                        return RedirectToAction("Campus");
                    }
                    else
                    {
                        // Display an error message on failure
                        TempData["ErrorMessage"] = "An error occurred while adding the campus.";
                        // Add JavaScript to show an alert with the error message and a redirect
                        return Content("An error occurred while adding the campus!");
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    // Display an error message in case of a WebException
                    TempData["ErrorMessage"] = "An error occurred while adding the campus.";
                    // Add JavaScript to show an alert with the error message and a redirect
                    return Content("Campus Name is Already Exist");
                }
            }
        }




        // CLAIM STUB

        public ActionResult Claim_stub(string campus_name)
        {
            Modelall mymodel = new Modelall();
            mymodel.Allclaimitems = claimstub(campus_name);
            mymodel.Allclothes = clothes_type(campus_name);
            mymodel.Allgrades = grades(campus_name);
            mymodel.Allsize = sizes(campus_name);
            ViewBag.campus_name = campus_name;

            return View("~/Views/Inventory/Claim_stub.cshtml", mymodel);
        }

        public IEnumerable<claim_stub> claimstub(string campus_name)
        {

            IEnumerable<claim_stub> ec = null;
            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/claim/");

            var consumedata = hc.GetAsync("list?campus_name=" + campus_name);
            consumedata.Wait();


            var dataread = consumedata.Result;
            if (dataread.IsSuccessStatusCode)
            {
                var results = dataread.Content.ReadAsAsync<IList<claim_stub>>();
                results.Wait();
                ec = results.Result;
            }
            else if (dataread.StatusCode == HttpStatusCode.NotFound)
            {
                ec = Enumerable.Empty<claim_stub>();
                ViewBag.Message = "No Locality Found.";
            }
            else
            {
                ec = Enumerable.Empty<claim_stub>();
                ViewBag.Message = "Unable to display locality, please email to it@golden-success.com for help.";
            }
            return ec;
        }




        // CLAIM FORM


        [HttpPost]
        [Route("insert/feedback/view2")]
        public ActionResult claimitem(string add_firstname, string add_lastname, string add_apparel, string add_reciept_number, string add_date_receive, string add_campusname, string add_gradelevel, string add_quantity, string add_size)
        {

            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(User.Identity.Name);
            try
            {
                string msg;
                try
                {
                    WebRequest req;
                    WebResponse res;
                    string postData = "first_name=" + add_firstname + "&last_name=" + add_lastname + "&apparel_name=" + add_apparel + "&reciept_number=" + add_reciept_number + "&date_recieve=" + add_date_receive + "&campus_name=" + add_campusname + "&grade_level=" + add_gradelevel + "&quantity=" + add_quantity + "&size=" + add_size;
                    req = WebRequest.Create(ConfigurationManager.AppSettings["API_Path"] + "api/inventory/add/claimitems?" + postData);
                    Byte[] data = Encoding.UTF8.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    req.ContentLength = data.Length;

                    Stream stream = req.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();

                    using (res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        msg = reader.ReadToEnd();

                        
                        string referringUrl = Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : "";

                        
                        return Redirect(referringUrl);
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse res = (HttpWebResponse)ex.Response;
                    Stream receiveStream = res.GetResponseStream();
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        return Content("Catch: " + readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream receiveStream = res.GetResponseStream();
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return Content(readStream.ReadToEnd(), "text/plain", Encoding.UTF8);
                }
            }
        }

        public ActionResult UserProfile()
        {

            return View();
        }


    }
}