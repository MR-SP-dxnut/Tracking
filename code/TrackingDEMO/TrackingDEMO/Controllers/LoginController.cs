using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using TrackingDEMO.Models;

namespace TrackingDemo.Controllers
{
    public class LoginController : Controller
    {

        // connect mysql
        MySqlConnection con = new MySqlConnection("server=localhost;user=root;database=dbtracking;port=3306;password=123456");

        public IActionResult Index()
        {
            ViewBag.LoginSucess = "";
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginUser loginUser)
        {
            con.Open();
            // check user login
            MySqlCommand cmd = new MySqlCommand("select * from user where email=@email and password=@password", con);
            cmd.Parameters.AddWithValue("@email", loginUser.Email.ToString());
            cmd.Parameters.AddWithValue("@password", loginUser.PassWord.ToString());
            cmd.ExecuteNonQuery();

            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string id_user = reader["id"].ToString();
                string name = reader["firstname"].ToString() +"  "+ reader["lastname"].ToString();
                HttpContext.Session.SetString("id_user", id_user);
                HttpContext.Session.SetString("username", name);
                return RedirectToAction("Tracking","Admin");
            }
            else
            {
                ViewBag.LoginSucess = "False";
                return View();
            }
        }
    }
}
