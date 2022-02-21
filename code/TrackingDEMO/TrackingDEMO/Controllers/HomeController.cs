using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using TrackingDEMO.Models;

namespace TrackingDemo.Controllers
{
    public class HomeController : Controller
    {
        // connect mysql
        MySqlConnection con = new MySqlConnection("server=localhost;user=root;database=dbtracking;port=3306;password=123456");

        public IActionResult Index()
        {
            ViewBag.TrackSuces = "";
            return View();
        }

        [HttpPost]
        public IActionResult Index(TrackingProduct trackingProduct)
        {
            con.Open();

            //check Tracking number
            MySqlCommand cmd_check_tracking = new MySqlCommand("select * from track where id=@id", con);
            cmd_check_tracking.Parameters.AddWithValue("@id", trackingProduct.id);
            cmd_check_tracking.ExecuteNonQuery();

            MySqlDataReader reader_check_tracking = cmd_check_tracking.ExecuteReader();
            if (reader_check_tracking.Read())
            {
                reader_check_tracking.Close();
                string sql = "select location.name , statustrack.status , statustrack.create_date from statustrack " +
                             " inner join location on statustrack.Location_id = location.id" +
                             " where statustrack.Track_id=@id";
                MySqlCommand cmd = new MySqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", trackingProduct.id);
                cmd.ExecuteNonQuery();
                List<TrackingProduct> list_Tracking = new List<TrackingProduct>();

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    TrackingProduct list = new TrackingProduct();
                    list.date_time_Status = Convert.ToDateTime(reader["create_date"]).AddYears(-543).ToString();
                    list.name_Location = reader["name"].ToString();
                    list.name_Status = reader["status"].ToString();
                    list_Tracking.Add(list);
                }
                ViewBag.list_Tracking = list_Tracking;
                ViewBag.TrackSuces = "True";
                return View();
            }
            else
            {
                ViewBag.TrackSuces = "Fales";
                ViewBag.message = "* Tracking Number ไม่ถูกต้อง!!";
                return View();
            }
        }

    }
}