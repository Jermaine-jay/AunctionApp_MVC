﻿using AunctionApp.BLL.Interfaces;
using AunctionApp.BLL.Models;
using AunctionApp.DAL.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;

namespace AunctionAppMVC.Controllers
{
    [Route("[controller]/[action]/{productId?}")]
    public class AuctionController : Controller
    {
        private readonly IProductService _ProductService;

        public AuctionController( IProductService productService)
        {       
            _ProductService = productService;      
        }

        public async Task<IActionResult> GetAuction(int productId)
        {
            var model = await _ProductService.GetAuction(productId);
            return View(model);
        }


        public async Task<IActionResult> MakeBid(int productId)
        {
            return View(new AddOrUpdateBidVM { ProductId = productId });   
        }
       
        public async Task<IActionResult> Home()
        {       
            var model = await _ProductService.GetAuctionsWithBidsAsync();
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> SaveBid(AddOrUpdateBidVM model)
        {
            if (ModelState.IsValid)
            {
                var (successful, msg) = await _ProductService.AddOrUpdateAsync(model);
                if (successful)
                {

                    TempData["SuccessMsg"] = msg;
                    return RedirectToAction("Home");
                }
                TempData["ErrMsg"] = msg;
                return View("Home");
            }
            return View("Home");
        }
    }
}
