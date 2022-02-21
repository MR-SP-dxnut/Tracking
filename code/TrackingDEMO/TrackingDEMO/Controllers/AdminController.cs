using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server;
using MySql.Data.MySqlClient;
using TrackingDEMO.Models;

using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Globalization;

namespace TrackingDEMO.Controllers
{

    public class AdminController : Controller
    {
        // connect mysql
        MySqlConnection con = new MySqlConnection("server=localhost;user=root;database=dbtracking;port=3306;password=123456");

        // dateTime
        DateTime dateNow = DateTime.Now;

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - Tracking - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public IActionResult Tracking()
        {
            
            string host = "https://" + HttpContext.Request.Host + 
                          "" + HttpContext.Request.Path + 
                          "" + HttpContext.Request.QueryString;
            Uri myUri = new Uri(host);
            string TrackingID = HttpUtility.ParseQueryString(myUri.Query).Get("TrackingID");

            // show view
            ViewBag.session = HttpContext.Session.GetString("username");
            ViewBag.Actionbar = "Tracking";
            ViewBag.Action = "Tracking";
            ViewBag.TrackingAction = "";
            ViewBag.TrackingID = TrackingID;
            return View();
        }

        [HttpPost]
        public IActionResult Tracking(TrackingProduct trackingProduct)
        {
            ViewBag.session = HttpContext.Session.GetString("username");
            string create_date = dateNow.ToString("yyyy/MM/dd HH:mm:ss");

            if (ModelState.IsValid)
            {
                if (trackingProduct.name_Status.Equals("เลือกสถานะ"))
                {
                    con.Open();
                    string sql = "select product.name , track.id from track " +
                        "inner join product on track.Product_id = product.id" +
                        " where track.id = @id";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@id", trackingProduct.id);
                    cmd.ExecuteNonQuery();

                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        trackingProduct.id = reader["id"].ToString();
                        trackingProduct.name_Product = reader["name"].ToString();
                    }

                    ViewBag.Actionbar = "Tracking";
                    ViewBag.Action = "Tracking";
                    ViewBag.TrackingAction = "UpdateTracking";
                    ViewBag.message = "* กรุณาเลือกสถานะ";
                    return View(trackingProduct);
                }
                else
                {
                    con.Open();
                    // check location 
                    string check_name_location = trackingProduct.name_Location + " ประเทศไทย";
                    MySqlCommand check_location = new MySqlCommand("select * from location where name = @name", con);
                    check_location.Parameters.AddWithValue("@name", check_name_location);
                    check_location.ExecuteNonQuery();

                    int id_location;
                    MySqlDataReader reader = check_location.ExecuteReader();
                    if (reader.Read())
                    {
                        id_location = Convert.ToInt32(reader["id"].ToString());

                    }
                    else
                    {
                        reader.Close();
                        // create location
                        MySqlCommand num_location = new MySqlCommand("select count(id) from location", con);
                        object result_num_location = num_location.ExecuteScalar();
                        int create_id_location = Convert.ToInt32(result_num_location) + 1;
                        string create_name_location = trackingProduct.name_Location + " ประเทศไทย";
                        string sql_location = "insert into location(id,name,create_date)" +
                                            " value(@id,@name,@create_date) ";
                        MySqlCommand cmdlocation = new MySqlCommand(sql_location, con);
                        cmdlocation.Parameters.AddWithValue("@id", create_id_location);
                        cmdlocation.Parameters.AddWithValue("@name", create_name_location);
                        cmdlocation.Parameters.AddWithValue("@create_date", create_date);
                        cmdlocation.ExecuteNonQuery();

                        id_location = create_id_location;
                    }
                    reader.Close();

                    // check id track
                    MySqlCommand check_id_track = new MySqlCommand("select * from statustrack where Track_id = @Track_id and status = @status", con);
                    check_id_track.Parameters.AddWithValue("@Track_id", trackingProduct.id);
                    check_id_track.Parameters.AddWithValue("@status", trackingProduct.name_Status);
                    check_id_track.ExecuteNonQuery();
                    MySqlDataReader reader_check_id_track = check_id_track.ExecuteReader();
                    if (reader_check_id_track.Read())
                    {
                        // update statustrack
                        int id_status = Convert.ToInt32(reader_check_id_track["id"].ToString());
                        reader_check_id_track.Close();
                        string sql_status = "update statustrack" +
                                            " set " +
                                            " Location_id = (select id from location where id = @Location_id)" +
                                            " where id = @id and status = @status ";
                        MySqlCommand cmdstatus = new MySqlCommand(sql_status, con);
                        cmdstatus.Parameters.AddWithValue("@id", id_status);
                        cmdstatus.Parameters.AddWithValue("@Location_id", id_location);
                        cmdstatus.Parameters.AddWithValue("@status", trackingProduct.name_Status);
                        cmdstatus.Parameters.AddWithValue("@create_date", create_date);
                        cmdstatus.ExecuteNonQuery();

                        // show view
                        ViewBag.Actionbar = "Tracking";
                        ViewBag.Action = "Tracking";
                        ViewBag.TrackingAction = "Success";
                        ViewBag.message = "อัพเดตสถานะ สำเร็จ";
                        return View();
                    }
                    else
                    {
                        reader_check_id_track.Close();
                        // create statustrack
                        MySqlCommand num_status = new MySqlCommand("select count(id) from statustrack", con);
                        object result_num_status = num_status.ExecuteScalar();
                        int id_status = Convert.ToInt32(result_num_status) + 1;
                        string sql_status = "insert into statustrack(id,Track_id,Location_id,status,create_date)" +
                                            " value(@id," +
                                            "(select id from track where id =@Track_id)," +
                                            "(select id from location where id =@Location_id)," +
                                            "@status,@create_date) ";
                        MySqlCommand cmdstatus = new MySqlCommand(sql_status, con);
                        cmdstatus.Parameters.AddWithValue("@id", id_status);
                        cmdstatus.Parameters.AddWithValue("@Track_id", trackingProduct.id);
                        cmdstatus.Parameters.AddWithValue("@Location_id", id_location);
                        cmdstatus.Parameters.AddWithValue("@status", trackingProduct.name_Status);
                        cmdstatus.Parameters.AddWithValue("@create_date", create_date);
                        cmdstatus.ExecuteNonQuery();

                        // show view
                        ViewBag.Actionbar = "Tracking";
                        ViewBag.Action = "Tracking";
                        ViewBag.TrackingAction = "Success";
                        ViewBag.message = "เพิ่มสถานะ สำเร็จ";
                        return View();
                    }
                }
            }
            else
            {
                con.Open();
                string sql = "select product.name , track.id from track "+
                    "inner join product on track.Product_id = product.id"+
                    " where track.id = @id";
                MySqlCommand cmd = new MySqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", trackingProduct.id);
                cmd.ExecuteNonQuery();

                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    trackingProduct.id = reader["id"].ToString();
                    trackingProduct.name_Product = reader["name"].ToString();
                    // show view
                    ViewBag.Actionbar = "Tracking";
                    ViewBag.Action = "Tracking";
                    ViewBag.TrackingAction = "UpdateTracking";
                    return View(trackingProduct);
                }
                else
                {
                    // show view
                    ViewBag.Actionbar = "Tracking";
                    ViewBag.Action = "Tracking";
                    ViewBag.TrackingAction = "";
                    ViewBag.Erorr = "* ไม่มีข้อมูล";
                    return View();
                }
            }
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - MyQRcode - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public IActionResult MyQRcode()
        {
            List<CreateTracking> list_ShowTracking = new List<CreateTracking>();
            con.Open();

            string sql_track = "select product.name ,track.id,track.track_qr_code, track.create_date from track" +
                                " inner join product on track.Product_id = product.id "+
                                " order by track.create_date asc";
            MySqlCommand cmd_track = new MySqlCommand(sql_track, con);
            MySqlDataReader reader = cmd_track.ExecuteReader();

            while (reader.Read())
            {
                CreateTracking list = new CreateTracking();
                list.name_Product = reader["name"].ToString();
                list.id_Tracking = reader["id"].ToString();
                list.QRCodes = string.Format("data:image/png;base64,{0}", Convert.ToBase64String((byte[])reader["track_qr_code"]));
                list_ShowTracking.Add(list);
            }
            
            reader.Close();
            // show view
            ViewBag.session = HttpContext.Session.GetString("username");
            ViewBag.Actionbar = "MyQRcode";
            ViewBag.Action = "MyQRcode";
            ViewBag.MyQRcodeAction = "MyQRcode";
            ViewBag.list_ShowTracking = list_ShowTracking;
            return View();
        }

        [HttpPost]
        public IActionResult MyQRcode(CreateTracking createTracking)
        {
            string create_date = dateNow.ToString("yyyy/MM/dd HH:mm:ss");
            string sessionuser = HttpContext.Session.GetString("username");
            ViewBag.session = sessionuser;

            if (ModelState.IsValid)
            {
                con.Open();
                string id_Product = "P" + createTracking.id_Product;
                // check id product 
                MySqlCommand checkproduct = new MySqlCommand("select count(id) from product where id=@id", con);
                checkproduct.Parameters.AddWithValue("@id", id_Product);
                object result = checkproduct.ExecuteScalar();
                int checkproductid = Convert.ToInt32(result) ;


                if (checkproductid >= 1)
                {
                    // show view
                    ViewBag.Actionbar = "MyQRcode";
                    ViewBag.Action = "MyQRcode";
                    ViewBag.MyQRcodeAction = "Create";
                    ViewBag.Erorr = "* มีรหัสสินค้าเเล้ว";
                    return View();
                }
                else
                {
                    // create product
                    string sql_product = "insert into product(id,name,create_date)"+
                                        " value(@id,@name,@create_date) ";
                    MySqlCommand cmdproduct = new MySqlCommand(sql_product, con);
                    cmdproduct.Parameters.AddWithValue("@id", id_Product);
                    cmdproduct.Parameters.AddWithValue("@name", createTracking.name_Product);
                    cmdproduct.Parameters.AddWithValue("@create_date", create_date);
                    cmdproduct.ExecuteNonQuery();

                    // Create QRcode 
                    QRCodeGenerator QrGenerator = new QRCodeGenerator();
                    string id_track = "T" + dateNow.ToString("ddMM") + createTracking.id_Product + dateNow.ToString("yy");
                    string urlQRcode = "https://" + HttpContext.Request.Host + "/Admin/Tracking?TrackingID="+ id_track;
                    QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(urlQRcode, QRCodeGenerator.ECCLevel.Q);
                    QRCode QrCode = new QRCode(QrCodeInfo);
                    Bitmap QrBitmap = QrCode.GetGraphic(60);
                    byte[] BitmapArray = QrBitmap.BitmapToByteArray();
                    string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
                    ViewBag.QrCodeUri = QrUri;

                    // Get id user
                    string id_usersession = HttpContext.Session.GetString("id_user");

                    // create track
                    string sql_track = "insert into track(id,User_id,Product_id,track_qr_code,url_track,create_date)"+
                                        "values (@id,"+
                                        "(select id from user where id =@User_id)," +
                                        "(select id from product where id =@Product_id)," +
                                        "@track_qr_code,@url_track,@create_date)";
                    MySqlCommand cmdtrack = new MySqlCommand(sql_track, con);
                    cmdtrack.Parameters.AddWithValue("@id", id_track);
                    cmdtrack.Parameters.AddWithValue("@User_id", id_usersession);
                    cmdtrack.Parameters.AddWithValue("@Product_id", id_Product);
                    cmdtrack.Parameters.AddWithValue("@track_qr_code", BitmapArray);
                    cmdtrack.Parameters.AddWithValue("@url_track", urlQRcode);
                    cmdtrack.Parameters.AddWithValue("@create_date", create_date);
                    cmdtrack.ExecuteNonQuery();

                    // set view 
                    createTracking.id_Product = "P" + createTracking.id_Product;
                    createTracking.url_Product = urlQRcode;
                    createTracking.id_Tracking = id_track;

                    // show view
                    ViewBag.Actionbar = "MyQRcode";
                    ViewBag.Action = "MyQRcode";
                    ViewBag.MyQRcodeAction = "Success";
                    return View(createTracking);
                }

            }
            else
            {
                
                // show view
                ViewBag.Actionbar = "MyQRcode";
                ViewBag.Action = "MyQRcode";
                ViewBag.MyQRcodeAction = "Create";
                return View();
            }
        }

        public IActionResult DetailQRcode(string idQRcode)
        {
            con.Open();
            List<CreateTracking> list_ShowTracking = new List<CreateTracking>();

            string sql_check_idQRcode = "select product.name , track.Product_id , track.id , track.track_qr_code , track.url_track from track" +
                                " inner join product on track.Product_id = product.id " +
                                " where track.id = @track_id";
          
            MySqlCommand check_idQRcode = new MySqlCommand(sql_check_idQRcode, con);
            check_idQRcode.Parameters.AddWithValue("@track_id", idQRcode);
            check_idQRcode.ExecuteNonQuery();

            MySqlDataReader reader = check_idQRcode.ExecuteReader();
            string QrUri = null ;
            reader.Read();
                CreateTracking list_detail = new CreateTracking();
                list_detail.id_Product = reader["Product_id"].ToString();
                list_detail.name_Product = reader["name"].ToString();
                list_detail.url_Product = reader["url_track"].ToString();
                list_detail.id_Tracking = reader["id"].ToString();
                list_detail.QRCode = (byte[])reader["track_qr_code"];
                QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(list_detail.QRCode));
                list_ShowTracking.Add(list_detail);
                ViewBag.QrCodeUri = QrUri;

            reader.Close();

            // show view
            ViewBag.session = HttpContext.Session.GetString("username");
            ViewBag.Actionbar = "MyQRcode";
            ViewBag.list_ShowTracking = list_ShowTracking;
            return View();
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - HistoryTracking - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public IActionResult HistoryTracking()
        {
            con.Open();
            List<TrackingProduct> list_trackingProducts = new List<TrackingProduct>();
            
            // set show table HistoryTracking
            string sql_history_tracking = "select statustrack.Track_id, product.name, statustrack.status from statustrack" +
                                 " inner join track on statustrack.Track_id = track.id"+
                                 " inner join product on track.Product_id = product.id";
            MySqlCommand cmd_history_tracking = new MySqlCommand(sql_history_tracking, con);
            cmd_history_tracking.ExecuteNonQuery();

            MySqlDataReader reader = cmd_history_tracking.ExecuteReader();
            while (reader.Read())
            {
                TrackingProduct trackingProduct = new TrackingProduct();
                trackingProduct.id = reader["Track_id"].ToString();
                trackingProduct.name_Product = reader["name"].ToString();
                trackingProduct.name_Status = reader["status"].ToString();
                list_trackingProducts.Add(trackingProduct);
            }
            reader.Close();

            // show view
            ViewBag.session = HttpContext.Session.GetString("username");
            ViewBag.Actionbar = "HistoryTracking";
            ViewBag.Action = "HistoryTracking";
            ViewBag.showTable = list_trackingProducts;
            return View();
        }

        // btn Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }

    //Extension method to convert Bitmap to Byte Array
    public static class BitmapExtension
    {
        public static byte[] BitmapToByteArray(this Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}