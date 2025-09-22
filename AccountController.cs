using FunWebsiteThing.SQL;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI;
using System.Text.RegularExpressions;

namespace FunWebsiteThing
{
    public class AccountController : Controller
    {
        IHttpContextAccessor _h;
        SessionManager _s;
        public AccountController(SessionManager s, IHttpContextAccessor h)
        {
            _s = s;
            _h = h;
        }
        // To-do: change this to work with external login sources? (Google)
        public async Task<IActionResult> Login(string Username, string Password, bool External = false)
        {
            if (_h.HttpContext.Session.GetInt32("IsLoggedIn") != 1)
            {
                int sid = _s.SID(); // generate session id
                bool isusernameemail = Regex.IsMatch(Username, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                Username = isusernameemail == true ? SQL.Accounts.GetUsername(Username) : Username;
                string ip = _s.GetIP();
                (bool result, bool error) = (false, false);
                // If non-external login source (the website only)
                if (External == false)
                {
                    (result, error) = await SQL.Accounts.Login(Username, Password, sid, ip);
                }
                // If it's an external website and this method is being called, it's a successful result
                // So all we need to really prove here is the user does actually exist
                else
                {
                    (result, error) = (SQL.Accounts.DoesUserExist(Username), false);
                }

                if (result == true)
                {
                    _s.Login(Username, SQL.Accounts.GetUserID(Username), sid);
                    IncrementLogins();
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
            return StatusCode(409, "You're already logged in.");
        }
        public async Task<IActionResult> Register(string Email, string Username, string Password, string? SecurityQuestion = null, string? Answer = null, bool External = false)
        {
            int sid = _s.SID(); // generate session id
            if (_h.HttpContext.Session.GetInt32("IsLoggedIn") != 1) 
            {
                string ip = _s.GetIP();
                (bool result, bool error) = await SQL.Accounts.Register(Email, Username, Password, sid, ip); 
                if (result == true)
                {
                    // We have this if statement here in case it's an External login. This session manager is null on the callback page, so we handle the session stuff there.
                    if (External == false) 
                    {
                        _s.Login(Username, SQL.Accounts.GetUserID(Username), sid);
                    }
                    await SQL.Accounts.CreateSecurityQuestion(SQL.Accounts.GetUserID(Username), SecurityQuestion, Answer);
                    IncrementRegistrations();
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
            return StatusCode(409, "You're already logged in, no need to register an account!");
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

        public async void IncrementLogins()
        {
            Statistics.IncrementLogins();
        }

        public async void IncrementRegistrations()
        {
            Statistics.IncrementRegistrations();
        }
    }
}
