using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class BasketController : BaseAPIController
    {
        public IBasketRepository BasketRepository { get; }
        public IMapper Mapper { get; }

        public BasketController(IBasketRepository basketRepository, IMapper mapper)
        {
            BasketRepository = basketRepository;
            Mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CustomerBasket>> GetBasketById(string basketId)
        {
            var basket = await BasketRepository.GetBasketAsync(basketId);
            return Ok(basket ?? new CustomerBasket(basketId));
        }

        [HttpPost]
        public async Task<ActionResult<CustomerBasket>> UpdateBasket(CustomerBasketDTO basket)
        {
            var customerBasket = Mapper.Map<CustomerBasketDTO, CustomerBasket>(basket);
            var updatedBasket = await BasketRepository.UpdateBasketAsync(customerBasket);

            return Ok(updatedBasket);
        }

        [HttpDelete]
        public async Task DeleteBasketAsync(string basketId)
        {
            await BasketRepository.DeleteBasketAsync(basketId);
        }



        
    }
}