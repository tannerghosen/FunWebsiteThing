using FunWebsiteThing.SQL;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace FunWebsiteThing
{
    public class AccountController : Controller
    {
        public SessionManager _s = new SessionManager();
        IHttpContextAccessor _h = new HttpContextAccessor();
        public async Task<IActionResult> Login(string Username, string Password)
        {
            if ((!string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(Password)) && _h.HttpContext.Session.GetInt32("IsLoggedIn") != 1)
            {
                int sid = _s.SID();
                bool isusernameemail = Regex.IsMatch(Username, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                Username = isusernameemail == true ? SQL.Accounts.GetUsername(Username) : Username;
                (bool result, bool error) = await SQL.Accounts.Login(Username, Password, sid);

                if (result == true)
                {
                    _s.Login(Username, SQL.Accounts.GetUserID(Username), sid);
                    return Ok("Login successful. Logged in as: " + Username + ".");
                }
                else if (result == false && error != true)
                {
                    return BadRequest("Invalid login");
                }
                else if (error == true)
                {
                    return StatusCode(500, "An error occurred while logging in");
                }
            }
            return BadRequest("Either username or password is blank, or you're already logged in.");
        }
        public async Task<IActionResult> Register(string Email, string Username, string Password, string? SecurityQuestion = null, string? Answer = null)
        {
            int sid = _s.SID();
            if (_h.HttpContext.Session.GetInt32("IsLoggedIn") != 1) // If we get a result, return the results
            {
                (bool result, bool error) = await SQL.Accounts.Register(Email, Username, Password, sid); // Registers our account, hopefully
                if (result == true)
                {
                    _s.Login(Username, SQL.Accounts.GetUserID(Username), sid);
                    //Logger.Write(_h.HttpContext.Session.GetString("Username") + " " + _h.HttpContext.Session.GetInt32("UserId") + " " + _h.HttpContext.Session.GetInt32("SessionId") + " " + _h.HttpContext.Session.GetInt32("IsLoggedIn"), "REGISTER");
                    await SQL.Accounts.CreateSecurityQuestion(SQL.Accounts.GetUserID(Username), SecurityQuestion, Answer);
                    return Ok("Account Registered. Logged into " + Username + ".");
                }
                else if (result == false && error != true)
                {
                    BadRequest("Duplicate account.");
                }
                else if (error == true)
                {
                    StatusCode(500, "An error occured while registering the account.");
                }
            }
            return BadRequest("You're already logged in, no need to register an account!");
        }

        public async Task<IActionResult> Logout()
        {
            if (_s.IsUserLoggedIn() && (_h.HttpContext.Session.GetString("Username") != null || _h.HttpContext.Session.GetString("Username") != ""))
            {
                _s.Logout();
                return Ok("Logged out of "+ _h.HttpContext.Session.GetString("Username"));
            }
            return BadRequest("You're not logged in.");
        }
    }
}
