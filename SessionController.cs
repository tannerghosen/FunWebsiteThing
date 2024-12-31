using FunWebsiteThing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

public class SessionController: Controller
{
    private readonly IHttpContextAccessor _h;
    private SQLStuff _sq = new SQLStuff();

    public SessionController(IHttpContextAccessor h)
    {
        _h = h;
    }

    public IActionResult Login(string username, int id, int sessionid)
    {
        _h.HttpContext.Session.SetString("Username", username);
        _h.HttpContext.Session.SetInt32("UserId", _sq.GetUserID(username));
        _h.HttpContext.Session.SetInt32("SessionId", sessionid);
        _h.HttpContext.Session.SetInt32("IsLoggedIn", 1);
        _h.HttpContext.Session.SetInt32("IsAdmin", _sq.IsAdmin(_h.HttpContext.Session.GetInt32("UserId")) == true ? 1 : 0);
        Logger.Write("Username: " + username + " id: " + _sq.GetUserID(username) + " ses id: " + sessionid + " is admin?: " + _sq.IsAdmin(_h.HttpContext.Session.GetInt32("UserId")), "LOGIN");

        return View();
    }

    public IActionResult Logout()
    {
        if (_h.HttpContext.Session.GetInt32("IsLoggedIn") == 1 && (_h.HttpContext.Session.GetString("Username") != null || _h.HttpContext.Session.GetString("Username") != ""))
        {
            Logger.Write("Username " + _h.HttpContext.Session.GetString("Username"), "LOGOUT");
            _h.HttpContext.Session.SetString("Username", "");
            _h.HttpContext.Session.SetInt32("IsLoggedIn", 0);
            _h.HttpContext.Session.SetInt32("UserId", -999999999);
            _h.HttpContext.Session.SetInt32("SessionId", -999999999);
            _h.HttpContext.Session.SetInt32("IsAdmin", 0);
        }

        return View();
    }

    public int SID()
    {
        Random r = new Random();
        int sid = r.Next(999999999);
        return sid;
    }
}