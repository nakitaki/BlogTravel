﻿using BlogTravel.Data;
using BlogTravel.Interfaces;
using BlogTravel.Repository;
using BlogTravel.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BlogTravel.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _dashboardRepository;
        public DashboardController(IDashboardRepository dashboardRepository) 
        {
            _dashboardRepository = dashboardRepository;
        }
        public async Task<IActionResult> Index()
        {
            var userAdventures = await _dashboardRepository.GetAllUserAdventures();
            var userHolidays = await _dashboardRepository.GetAllUserHolidays();
            var dashboardViewModel = new DashboardViewModel()
            {
                Adventures = userAdventures,
                Holidays = userHolidays
            };
            return View(dashboardViewModel);
        }
    }
}
