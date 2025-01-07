﻿using FunWebsiteThing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

public class SessionManager
{
    private readonly IHttpContextAccessor _h;
    //private SQLStuff _sq = new SQLStuff();

    public SessionManager(IHttpContextAccessor h)
    {
        _h = h;
    }

    public void Login(string username, int id, int sessionid)
    {
        _h.HttpContext.Session.SetString("Username", username);
        _h.HttpContext.Session.SetInt32("UserId", SQLStuff.GetUserID(username));
        _h.HttpContext.Session.SetInt32("SessionId", sessionid);
        _h.HttpContext.Session.SetInt32("IsLoggedIn", 1);
        _h.HttpContext.Session.SetInt32("IsAdmin", SQLStuff.IsAdmin(_h.HttpContext.Session.GetInt32("UserId")) == true ? 1 : 0);
        Logger.Write("Username: " + username + " id: " + SQLStuff.GetUserID(username) + " ses id: " + sessionid + " is admin?: " + SQLStuff.IsAdmin(_h.HttpContext.Session.GetInt32("UserId")), "LOGIN");

    }

    public void Logout()
    {
        if (IsUserLoggedIn() && (_h.HttpContext.Session.GetString("Username") != null || _h.HttpContext.Session.GetString("Username") != ""))
        {
            Logger.Write("Username " + _h.HttpContext.Session.GetString("Username"), "LOGOUT");
            _h.HttpContext.Session.SetString("Username", "");
            _h.HttpContext.Session.SetInt32("IsLoggedIn", 0);
            _h.HttpContext.Session.SetInt32("UserId", -1);
            _h.HttpContext.Session.SetInt32("SessionId", -1);
            _h.HttpContext.Session.SetInt32("IsAdmin", 0);
        }
    }

    public int SID()
    {
        Random r = new Random();
        int sid = r.Next(999999999);
        return sid;
    }

    public bool IsUserLoggedIn()
    {
        if (_h.HttpContext.Session.GetInt32("IsLoggedIn") == 1)
        {
            return true;
        }
        return false;
    }
}