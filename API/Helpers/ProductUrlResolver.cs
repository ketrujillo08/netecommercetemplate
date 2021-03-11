using API.DTOs;
using AutoMapper;
using Core.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class ProductUrlResolver : IValueResolver<Product, ProductToReturnDto, string>
    {
        public ProductUrlResolver(IConfiguration config)
        {
            Config = config;
        }

        public IConfiguration Config { get; }

        public string Resolve(Product source, 
            ProductToReturnDto destination, 
            string destMember, 
            ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.PictureUrl))
            {
                return Config["ApiUrl"] + source.PictureUrl;
            }
            return null;
        }
    }
}
