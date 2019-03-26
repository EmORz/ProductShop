using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });

            var mapper = config.CreateMapper();

            var context = new ProductShopContext();

            var users = new
            {
                usersCount = context.Users.Count(),
                users = 
                     context
                    .Users
                    .OrderByDescending(x => x.ProductsSold.Count)
                    .ThenBy(l => l.LastName)
                    .Where(x => x.ProductsSold.Count>=1 && x.ProductsSold.Any(g => g.Buyer!=null))
                    .Select(x => new
                    {
                        firstName = x.FirstName,
                        lastName = x.LastName,
                        age = x.Age,
                        soldProducts = new
                        {
                            count = x.ProductsSold.Count,
                            products = x.ProductsSold.Select( d => new
                            {
                                name = d.Name,
                                price = d.Price
                            })
                        }
                    })
            };
                   
                

            var jsonProducts = JsonConvert.SerializeObject(users, new JsonSerializerSettings
            {
               Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            });

            //users-sold-products.json
            File.WriteAllText("E:\\ProductShop\\ProductShop\\Json\\users-and-products.json", jsonProducts);


        }

        public static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var result = new List<ValidationResult>();

            return Validator.TryValidateObject(obj, validationContext, result, true);
        }
    }
}