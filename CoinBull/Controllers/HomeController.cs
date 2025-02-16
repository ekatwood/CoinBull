﻿using CoinBull.Models;
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
        private const string coinsList = "AAVE,ADA,ALGO,ATOM,AVAX,BAT,BCH,BSV,BTC,CEL,CHSB,COMP,CRV,DASH,DCR,DGB,DOGE,DOT,EGLD,EOS,ETC,ETH,FIL,FTT,GRT,HBAR,KSM,LINK,LRC,LTC,LUNA,MIOTA,MKR,NANO,NEAR,NEO,OMG,ONEINCH,ONT,QNT,REN,RENBTC,RUNE,SC,SNX,SOL,SUSHI,THETA,TRX,UMA,UNI,VET,WAVES,WBTC,XEM,XLM,XMR,XRP,XTZ,YFI,ZEC,ZEN,ZIL,ZRX";
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

                using (var command = new SqlCommand("GetAllJobsForUser", connection))
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
            ViewBag.Coins = coinsList;

            Debug.WriteLine(coinsList.Split(',').Length);
            return View();
        }

        public ActionResult RegisterConfirmation()
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

            Debug.WriteLine(m);
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

        public async Task<ActionResult> EditAlert(string alertId)
        {

            //get job
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("GetJob", connection))
                {

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@id", SqlDbType.Int).Value = Convert.ToInt32(alertId);


                    SqlDataReader r = await command.ExecuteReaderAsync();

                    while (r.Read())
                    {
                        ViewBag.Id = alertId;
                        ViewBag.Coins = r["ListOfCoins"];
                        ViewBag.Percent = r["PercentChange"];
                        ViewBag.Minutes = r["Minutes"];
                        ViewBag.AllCoins = coinsList;
                    }

                }
                connection.Close();
            }

            return View();
        }

        [HttpPost]
        public async Task<bool> EditAlert(string Id, string Percent, string Minutes, Dictionary<string, int> Coins)
        {
            
            //convert to string
            string m = "";
            foreach (KeyValuePair<string, int> c in Coins)
            {
                if (c.Value == 1)
                {
                    if (m == "")
                        m = c.Key;
                    else
                        m = m + "," + c.Key;
                }
            }

            //update alert
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("UpdateAlert", connection))
                {

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@id", SqlDbType.Int).Value = Convert.ToInt32(Id);
                    command.Parameters.Add("@coins", SqlDbType.VarChar).Value = m;
                    command.Parameters.Add("@percent", SqlDbType.Int).Value = Convert.ToInt32(Percent);
                    command.Parameters.Add("@minutes", SqlDbType.Int).Value = Convert.ToInt32(Minutes);

                    await command.ExecuteNonQueryAsync();

                }
                connection.Close();
            }


            return true;
        }

        public ActionResult AddAlert()
        {
            ViewBag.Coins = coinsList;

            return View();
        }

        [HttpPost]
        public async Task<bool> AddAlert(string Percent, string Minutes, Dictionary<string, int> Coins)
        {

            //convert to string
            string m = "";
            foreach (KeyValuePair<string, int> c in Coins)
            {
                if (c.Value == 1)
                {
                    if (m == "")
                        m = c.Key;
                    else
                        m = m + "," + c.Key;
                }
            }

            //update alert
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("AddJob", connection))
                {

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@user", SqlDbType.VarChar).Value = User.Identity.GetUserName();
                    command.Parameters.Add("@coins", SqlDbType.VarChar).Value = m;
                    command.Parameters.Add("@percent", SqlDbType.Int).Value = Convert.ToInt32(Percent);
                    command.Parameters.Add("@minutes", SqlDbType.Int).Value = Convert.ToInt32(Minutes);

                    await command.ExecuteNonQueryAsync();

                }
                connection.Close();
            }


            return true;
        }

        [HttpPost]
        public async Task<bool> DeleteAlert(string Id)
        {
            //delete alert
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("DeleteAlert", connection))
                {

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@id", SqlDbType.Int).Value = Convert.ToInt32(Id);

                    await command.ExecuteNonQueryAsync();

                }
                connection.Close();
            }


            return true;
        }

        public ActionResult TermsOfService()
        {
            
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}