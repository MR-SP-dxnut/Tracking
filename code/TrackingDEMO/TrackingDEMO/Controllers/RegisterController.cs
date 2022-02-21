using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using TrackingDEMO.Models;

namespace TrackingDemo.Controllers
{
    public class RegisterController : Controller
    {
        // connect mysql
        MySqlConnection con = new MySqlConnection("server=localhost;user=root;database=dbtracking;port=3306;password=123456");
        // dateTime
        DateTime dateNow = DateTime.Now;

        public IActionResult Index()
        {
            ViewBag.RegisterSuccess = "False";
            return View();
        }

        [HttpPost]
        public IActionResult Index(RegisterUser registerUser)
        {
            if (ModelState.IsValid)
            {
                con.Open();
                // check user
                MySqlCommand check_user_cmd = new MySqlCommand("select * from user where email = @email ", con);
                check_user_cmd.Parameters.AddWithValue("@email", registerUser.Email.ToString());
                check_user_cmd.ExecuteNonQuery();

                MySqlDataReader reader = check_user_cmd.ExecuteReader();
                if (reader.Read())
                {
                    ViewBag.RegisterSuccess = "False";
                    ViewBag.EmailErorr = "* มีข้อมูลเเล้ว";
                    return View();
                }
                else
                {
                    reader.Close();
                    string create_date = dateNow.ToString("yyyy/MM/dd HH:mm:ss");
                    string id = "U" + dateNow.ToString("yyyyMMddHHmmss");


                    MySqlCommand cmd = new MySqlCommand("insert into user(id,firstname,lastname,phone,email,password,create_date) " +
                                                        " values (@id,@firstname,@lastname,@phone,@email,@password,@create_date)", con);

                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@firstname", registerUser.FirstName.ToString());
                    cmd.Parameters.AddWithValue("@lastname", registerUser.LastName.ToString());
                    cmd.Parameters.AddWithValue("@phone", registerUser.Phone.ToString());
                    cmd.Parameters.AddWithValue("@email", registerUser.Email.ToString());
                    cmd.Parameters.AddWithValue("@password", registerUser.PassWord.ToString());
                    cmd.Parameters.AddWithValue("@create_date", create_date);

                    cmd.ExecuteNonQuery();

                    ViewBag.RegisterSuccess = "True";
                    ViewBag.Email = registerUser.Email.ToString();
                    return View();
                }
            }
            else
            {
                ViewBag.RegisterSuccess = "False";
                return View();
            }
            
        }

    }
}
