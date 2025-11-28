using FunWebsiteThing;
using Microsoft.AspNetCore.Authentication;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using MySqlX.XDevAPI;

#pragma warning disable CS8602

public struct Session
{
    public string? Username;
    public int? UserId;
    public int? SessionId;
    public int? IsLoggedIn;
    public int? IsAdmin;
}
public class SessionManager
{

    private IHttpContextAccessor _h;

    public SessionManager(IHttpContextAccessor h)
    {
        _h = h;
    }
    public void Login(string username, int id, int sessionid)
    {
        _h.HttpContext.Session.SetString("Username", username);
        _h.HttpContext.Session.SetInt32("UserId", FunWebsiteThing.SQL.Accounts.GetUserID(username));
        _h.HttpContext.Session.SetInt32("SessionId", sessionid);
        _h.HttpContext.Session.SetInt32("IsLoggedIn", 1);
        _h.HttpContext.Session.SetInt32("IsAdmin", FunWebsiteThing.SQL.Admin.IsAdmin(_h.HttpContext.Session.GetInt32("UserId")) == true ? 1 : 0);
        Logger.Write("Username: " + username + " id: " + FunWebsiteThing.SQL.Accounts.GetUserID(username) + " ses id: " + sessionid + " is admin?: " + FunWebsiteThing.SQL.Admin.IsAdmin(_h.HttpContext.Session.GetInt32("UserId")), "LOGIN");
    }

    public void Logout()
    {
        if (IsUserLoggedIn() && (_h.HttpContext.Session.GetString("Username") != null || _h.HttpContext.Session.GetString("Username") != ""))
        {
            Logger.Write("Username " + _h.HttpContext.Session.GetString("Username"), "LOGOUT");
            _h.HttpContext.Session.SetString("Username", "");
            _h.HttpContext.Session.SetInt32("UserId", -1);
            _h.HttpContext.Session.SetInt32("SessionId", -1);
            _h.HttpContext.Session.SetInt32("IsLoggedIn", 0);
            _h.HttpContext.Session.SetInt32("IsAdmin", 0);
        }
    }

    // Session ID generator
    public int SID()
    {
        int max = 999999999;
        byte[] bytes = new byte[4];
        using (var rng = RandomNumberGenerator.Create()) // create a random number generator
        {
            rng.GetBytes(bytes);
        }
        int result = BitConverter.ToInt32(bytes, 0) & int.MaxValue; // convert the bytes to an int. & int.MaxValue is an AND bitwise operation that ensures we only get positives
        result = result % (max + 1); // in case it goes over our max, we do a modulo to get the remainder. this does nothing if it's less than the max

        return result;
    }

    public bool IsUserLoggedIn()
    {
        if (_h.HttpContext?.Session.GetInt32("IsLoggedIn") == 1)
        {
            return true;
        }
        return false;
    }

    public string GetIP()
    {
        return _h.HttpContext.Connection.RemoteIpAddress.ToString();
    }

    public Session GetSession()
    {
        return new Session { Username = _h.HttpContext?.Session.GetString("Username"), UserId = _h.HttpContext?.Session.GetInt32("UserId"), SessionId = _h.HttpContext?.Session.GetInt32("SessionId"), IsLoggedIn = _h.HttpContext?.Session.GetInt32("IsLoggedIn"), IsAdmin = _h.HttpContext?.Session.GetInt32("IsAdmin") };
    }
}