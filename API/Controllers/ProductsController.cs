using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Errors;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class ProductsController : BaseAPIController
    {
        public IGenericRepository<Product> ProductsRepo { get; }
        public IGenericRepository<ProductBrand> BrandRepo { get; }
        public IGenericRepository<ProductType> TypeRepo { get; }
        public IMapper Mapper { get; }

        public ProductsController(IGenericRepository<Product> productsRepo,
            IGenericRepository<ProductBrand> brandRepo, IGenericRepository<ProductType> typeRepo,
            IMapper mapper)
        {
            ProductsRepo = productsRepo;
            BrandRepo = brandRepo;
            TypeRepo = typeRepo;
            Mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts(
            [FromQuery]ProductSpecParams productParams)
        {

            var spec = new ProductsWithTypesAndBrandsSpecification(productParams);
            var countSpec = new ProductWithFiltersForCountSpecification(productParams);
            var totalItems = await ProductsRepo.CountAsync(spec);
            var products = await ProductsRepo.ListAsync(spec);
            var data = Mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);
            return Ok(new Pagination<ProductToReturnDto>(productParams.PageIndex,
                productParams.PageSize,totalItems, data));
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductToReturnDto>> GetProducts(int id)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(id);
            var product = await ProductsRepo.GetEntityWhitSpec(spec);
            if (product == null) return NotFound(new ApiResponse(404));
            return Mapper.Map<Product, ProductToReturnDto>(product);
        }

        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<List<ProductBrand>>>> GetProductBrands()
        {
            var brands = await BrandRepo.ListAllAsync();
            return Ok(brands);
        }
        [HttpGet("brands/{id}")]
        public async Task<ActionResult<ProductBrand>> GetProductBrands(int id)
        {
            return await BrandRepo.GetByIdAsync(id);
        }

        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<List<ProductType>>>> GetProductTypes()
        {
            var types = await TypeRepo.ListAllAsync();
            return Ok(types);
        }
        [HttpGet("types/{id}")]
        public async Task<ActionResult<ProductType>> GetProductTypes(int id)
        {
            return await TypeRepo.GetByIdAsync(id);
        }
    }
}