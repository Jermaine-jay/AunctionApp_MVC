﻿using AunctionApp.BLL.Interfaces;
using AunctionApp.BLL.Models;
using AunctionApp.DAL.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using TodoList.DAL.Repository;


namespace AunctionApp.BLL.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Product> _ProductRepo;

        public AdminService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _ProductRepo = _unitOfWork.GetRepository<Product>();
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<(bool successful, string msg)> CreateAuctionAsync(AuctionVM model)
        {

            var fileName = model.ProductImagePath.FileName;
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "Auctions");

            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }
            string picPath = Path.Combine(imagePath, fileName);
            using (var stream = new FileStream(picPath, FileMode.Create))
            {
                await model.ProductImagePath.CopyToAsync(stream);
            }

            AuctionVMForm form = new()
            {
                ProductImagePath = fileName, 
                ProductName = model.ProductName,
                Description = model.Description,
                ActualAmount = model.ActualAmount,
            };

            var product = _mapper.Map<Product>(form);
            var rowChanges = await _ProductRepo.AddAsync(product);

            return rowChanges != null ? (true, "Auction created successfully!") : (false, "Failed to create Auction");
        }

        public async Task<(bool successful, string msg)> UpdateAuctionAsync(AuctionVM model)
        {
            var product = await _ProductRepo.GetSingleByAsync(u => u.Id == model.Id);
            if (product == null)
            {
                return (false, $"Product with ID:{model.Id} wasn't found");
            }

            var fileName = model.ProductImagePath.FileName;
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "Auctions");

            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }
            string picPath = Path.Combine(imagePath, fileName);
            using (var stream = new FileStream(picPath, FileMode.Create))
            {
                await model.ProductImagePath.CopyToAsync(stream);
            }

            AuctionVMForm form = new()
            {
                ProductImagePath = fileName, // <-- Use fileName instead of "/images/" + fileName
                ProductName = model.ProductName,
                Description = model.Description,
                ActualAmount = model.ActualAmount,
            };

            var updateproduct = _mapper.Map(form, product);
            var rowChanges = await _ProductRepo.UpdateAsync(updateproduct);

            return rowChanges != null ? (true, "Aunction created successfully!") : (false, "Failed to create Aunction");
        }

        public async Task<(bool successful, string msg)> DeleteAuctionAsync(int productId)
        {
            var aunction = await _ProductRepo.GetSingleByAsync(u => u.Id == productId);

            var fileName = aunction.ProductImagePath;
            var filePathToDelete = Path.Combine(_webHostEnvironment.WebRootPath, "img", fileName);

            if (aunction == null || File.Exists(filePathToDelete))
            {
                return (false, $"Aunction with user:{aunction.Id} wasn't found");
            }
           
            File.Delete(filePathToDelete);         
            await _ProductRepo.DeleteAsync(aunction);
            return await _unitOfWork.SaveChangesAsync() >= 0 ? (true, $"{aunction.ProductName} was deleted") : (false, $"Delete Failed");

        }

        public async Task<(bool Done, string msg)> ToggleProductStatus(int productId)
        {
            var aunction = await _ProductRepo.GetSingleByAsync(u => u.Id == productId, tracking: true);

            if (aunction != null)
            {
                aunction.IsSold = !aunction.IsSold;
                var row = _mapper.Map<Product>(aunction);
                var rowChanges = await _ProductRepo.UpdateAsync(row);
                return rowChanges != null ? (true, "Status updated") : (false, "Failed to update status");
            }
            return (false,"");
        }

    }
}
