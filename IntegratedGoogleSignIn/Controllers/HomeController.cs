using IntegratedGoogleSignIn.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace IntegratedGoogleSignIn.Controllers
{
    public class HomeController : Controller
    {

        private string ClientId = "686433165749-l7almv15pjmptsbmnm8k35sa8ck8tvb3.apps.googleusercontent.com";
        private string SecretKey = "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";
        private string RedirectUrl = "https://localhost:44310/Home/SaveGoogleUser";

        /// <summary>    
        /// Returns login page if user is not logged in else return user profile    
        /// </summary>    
        /// <returns>return page</returns>  
        public async Task<ActionResult> Index()
        {
            string token = (string)Session["user"];
            if (string.IsNullOrEmpty(token))
            {
                return View();
            }
            else
            {
                return View("UserProfile", await GetuserProfile(token));
            }
        }

        /// <summary>  
        /// Hit Google API to get access code  
        /// </summary>  
        public void LoginUsingGoogle()
        {
            Response.Redirect($"https://accounts.google.com/o/oauth2/v2/auth?client_id={ClientId}&response_type=code&scope=openid%20email%20profile&redirect_uri={RedirectUrl}&state=abcdef");
        }

        [HttpGet]
        public ActionResult SignOut()
        {
            Session["user"] = null;
            return View("Index");
        }

        /// <summary>  
        /// Listen response from Google API after user authorization  
        /// </summary>  
        /// <param name="code">access code returned from Google API</param>  
        /// <param name="state">A value passed by application to prevent Cross-site request forgery attack</param>  
        /// <param name="session_state">session state</param>  
        /// <returns></returns>  
        [HttpGet]
        public async Task<ActionResult> SaveGoogleUser(string code, string state, string session_state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return View("Error");
            }

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.googleapis.com")
            };
            var requestUrl = $"oauth2/v4/token?code={code}&client_id={ClientId}&client_secret={SecretKey}&redirect_uri={RedirectUrl}&grant_type=authorization_code";

            var dict = new Dictionary<string, string>
            {
                { "Content-Type", "application/x-www-form-urlencoded" }
            };
            var req = new HttpRequestMessage(HttpMethod.Post, requestUrl) { Content = new FormUrlEncodedContent(dict) };
            var response = await httpClient.SendAsync(req);
            var token = JsonConvert.DeserializeObject<GmailToken>(await response.Content.ReadAsStringAsync());
            Session["user"] = token.AccessToken;
            var obj = await GetuserProfile(token.AccessToken);

            //IdToken property stores user data in Base64Encoded form  
            //var data = Convert.FromBase64String(token.IdToken.Split('.')[1]);  
            //var base64Decoded = System.Text.ASCIIEncoding.ASCII.GetString(data);  

            return View("UserProfile", obj);
        }

        /// <summary>  
        /// To fetch User Profile by access token  
        /// </summary>  
        /// <param name="accesstoken">access token</param>  
        /// <returns>User Profile page</returns>  
        public async Task<UserProfile> GetuserProfile(string accesstoken)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.googleapis.com")
            };
            string url = $"https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token={accesstoken}";
            var response = await httpClient.GetAsync(url);
            return JsonConvert.DeserializeObject<UserProfile>(await response.Content.ReadAsStringAsync());
        }
    }
}