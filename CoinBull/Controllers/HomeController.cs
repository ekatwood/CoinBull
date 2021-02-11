using CoinBull.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace CoinBull.Controllers
{
    public class HomeController : Controller
    {
        private const string connectionString = "Server=tcp:btcwidgetserver.database.windows.net,1433;Initial Catalog=btcwidgetdb;Persist Security Info=False;User ID=ekatwood;Password=ek@132EKA;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public async Task<ActionResult> Index()
        {
            if (!Request.IsAuthenticated)
            {
                return Redirect("~/Home/NewUser");

            }

            //id, coins, percent, minutes, isactive
            var jobs = new List<(int, string, int, int, bool)> { };

            //get jobs for the user
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("GetJobsForUser", connection))
                {

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@user", SqlDbType.VarChar).Value = User.Identity.GetUserName();


                    SqlDataReader r = await command.ExecuteReaderAsync();

                    while (r.Read())
                    {
                        jobs.Add(((int)r["Id"], (string)r["ListOfCoins"], (int)r["PercentChange"], (int)r["Minutes"], (bool)r["IsActive"]));
                    }

                }
                connection.Close();
            }

            ViewBag.Alerts = jobs;

            return View();
        }

        public ActionResult NewUser()
        {
            return View();
        }

        [HttpPost]
        public void UpdateCoins(Dictionary<string, int> Coins)
        {
            //convert to string
            string m = "";
            foreach(KeyValuePair<string, int> c in Coins)
            {
                if (c.Value == 1)
                {
                    if (m == "")
                        m = c.Key;
                    else
                        m = m + "," + c.Key;
                }
            }

            TempData["Coins"] = m;

            return;
        }

        [HttpPost]
        public void SaveAlertSettings(string percent, string minutes)
        {
            TempData["Percent"] = percent;
            TempData["Minutes"] = minutes;

            return;
        }

        [HttpPost]
        public async Task<bool> UpdateAlertState(string state, string id)
        {

            Debug.WriteLine("state: " + state);
            Debug.WriteLine("id: " + id);
            //update the IsActive state
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("UpdateAlertState", connection))
                {

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@id", SqlDbType.Int).Value = Convert.ToInt32(id);

                    if(state == "false")
                        command.Parameters.Add("@isactive", SqlDbType.Bit).Value = 0;
                    else
                        command.Parameters.Add("@isactive", SqlDbType.Bit).Value = 1;

                    await command.ExecuteNonQueryAsync();

                    

                }
                connection.Close();
            }


            return true;
        }

        public ActionResult SetAlert()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}