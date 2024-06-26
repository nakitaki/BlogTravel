﻿using BlogTravel.Data;
using BlogTravel.Interfaces;
using BlogTravel.Models;
using BlogTravel.Repository;
using BlogTravel.Services;
using BlogTravel.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogTravel.Controllers
{
    public class AdventureController : Controller
    {

        private readonly IAdventureRepository _adventureRepository;
        private readonly IPhotoService _photoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdventureController(IAdventureRepository adventureRepository, IPhotoService photoService, IHttpContextAccessor httpContextAccessor)
        {
            _adventureRepository = adventureRepository;
            _photoService = photoService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<Adventure> adventures = await _adventureRepository.GetAll();
            return View(adventures);
        }

        public async Task<IActionResult> Detail(int id)
        {
            Adventure adventure = await _adventureRepository.GetByIdAsync(id);
            return View(adventure);
        }

        public IActionResult Create()
        {
            var curUserId = _httpContextAccessor.HttpContext?.User.GetUserId();
            var createdAdventureViewModel = new CreateAdventureViewModel
            {
                AppUserId = curUserId
            };
            return View(createdAdventureViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAdventureViewModel adventureVM)
        {
            if (ModelState.IsValid)
            {
                var result = await _photoService.AddPhotoAsync(adventureVM.Image);
                var adventure = new Adventure
                {
                    Title = adventureVM.Title,
                    Description = adventureVM.Description,
                    Image = result.Url.ToString(),
                    AppUserId = adventureVM.AppUserId,
                    Address = new Address
                    {
                        City = adventureVM.Address.City,
                        Street = adventureVM.Address.Street
                    }
                };
                _adventureRepository.Add(adventure);
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Photo upload failed");
            }

            return View(adventureVM);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var adventure = await _adventureRepository.GetByIdAsync(id);
            if (adventure == null) return View("Error");
            var adventureVM = new EditAdventureViewModel
            {
                Title = adventure.Title,
                Description = adventure.Description,
                AddressId = adventure.AddressId,
                Address = adventure.Address,
                URL = adventure.Image,
                AdventureCategory = adventure.AdventureCategory
            };
            return View(adventureVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditAdventureViewModel adventureVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit adventure");
                return View("Edit", adventureVM);
            }

            var userAdventure = await _adventureRepository.GetByIdAsyncNoTracking(id);

            if (userAdventure != null)
            {
                try
                {
                    await _photoService.DeletePhotoAsync(userAdventure.Image);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Could not delete photo");
                    return View(adventureVM);
                }

                var photoResult = await _photoService.AddPhotoAsync(adventureVM.Image);

                var adventure = new Adventure
                {
                    Id = id,
                    Title = adventureVM.Title,
                    Description = adventureVM.Description,
                    Image = photoResult.Url.ToString(),
                    AddressId = adventureVM.AddressId,
                    Address = adventureVM.Address
                };

                _adventureRepository.Update(adventure);
                return RedirectToAction("Index");
            }
            else
            {
                return View(adventureVM);
            }

        }


        public async Task<IActionResult> Delete(int id)
        {
            var adventureDetails = await _adventureRepository.GetByIdAsync(id);
            if (adventureDetails == null) return View("Error");
            return View(adventureDetails);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteAdventure(int id)
        {
            var adventureDetails = await _adventureRepository.GetByIdAsync(id);
            if (adventureDetails == null) return View("Error");

            _adventureRepository.Delete(adventureDetails);
            return RedirectToAction("Index");
        }
    }
}
