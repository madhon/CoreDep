﻿namespace CoreDep.Controllers;

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;

    public HomeController(ILogger<HomeController> logger)
    {
        this.logger = logger;
    }

    [HttpGet()]
    [AllowAnonymous()]
    public IActionResult Index()
    {
        using var act = Program.ActivitySource.StartActivity("IndexActionResult");
        return View();
    }

    [HttpGet()]
    [AllowAnonymous()]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet()]
    [AllowAnonymous()]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}