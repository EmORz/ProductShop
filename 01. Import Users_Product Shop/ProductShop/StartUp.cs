using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProductShop.Data;
using ProductShop.DTOs.Export;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        private static string reportMessage = "Successfully imported {0}";
        public static void Main(string[] args)
        {

            var context = new ProductShopContext();
            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            var path = $"C:\\Users\\User\\Desktop\\01. Import Users_Product Shop\\ProductShop\\Datasets\\categories-products.json";
            var userJson = File.ReadAllText(path);

            var result = GetUsersWithProducts(context);

            Console.WriteLine(result);;

            //Create Methods stricktly define pathern!!! Be carefully
        }
        //Methods
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var filteredUsers =
                context
                    .Users
                    .Where(u => u.ProductsSold.Any(ps => ps.Buyer != null))
                    .OrderByDescending(u => u.ProductsSold.Count(ps => ps.Buyer != null))
                    .Select(u =>
                        new
                        {
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            Age = u.Age,
                            SoldProducts = new
                            {
                                Count = u.ProductsSold.Count(ps => ps.Buyer != null),
                                Products = u.ProductsSold.Where(ps => ps.Buyer != null)
                                    .Select(ps => new
                                    {
                                        Name = ps.Name,
                                        Price = ps.Price
                                    }).ToArray()
                            }
                        }).ToArray();

            var result = new
            {
                UsersCount = filteredUsers.Length,
                Users = filteredUsers
            };

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var json = JsonConvert.SerializeObject(result,
                new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ContractResolver = contractResolver,
                    NullValueHandling = NullValueHandling.Ignore
                });
            return json;
        }
        public static string GetSoldProducts(ProductShopContext context)
        {
            var filteredUsers = context
                .Users
                .Where(x => x.ProductsSold.Any(ps => ps.Buyer != null))
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .Select(c =>
                    new
                    {
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        SoldProducts = c.ProductsSold
                            .Where(ps => ps.Buyer != null)
                            .Select(ps => new
                            {
                                Name = ps.Name,
                                Price = ps.Price,
                                BuyerFirstName = ps.Buyer.FirstName,
                                BuyerLastName = ps.Buyer.LastName
                            }).ToArray()
                    }).ToArray();

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy =  new CamelCaseNamingStrategy()
            };

            var json = JsonConvert.SerializeObject(filteredUsers, 
            new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver =  contractResolver
            });
            return json;
        }
        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            //todo its Broken must be refactoring!!!

            var validCategoriesId = context.Categories.Select(x => x.Id).ToHashSet();
            var validProductsId = context.Products.Select(x => x.Id).ToHashSet();

            var categoryProducts = JsonConvert.DeserializeObject<CategoryProduct[]>(inputJson);
            var validCategoriesProducts = new List<CategoryProduct>();

            foreach (var categoryProduct in categoryProducts)
            {
                var isValid = validCategoriesId.Contains(categoryProduct.CategoryId)
                              && validProductsId.Contains(categoryProduct.ProductId);
                if (isValid)
                {
                    validCategoriesProducts.Add(categoryProduct);
                }
            }
            context.CategoryProducts.AddRange(validCategoriesProducts);
            context.SaveChanges();

            return string.Format(reportMessage, validCategoriesProducts.Count);
        }
        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            var categories = JsonConvert.DeserializeObject<Category[]>(inputJson);

            var validCategories = new List<Category>();

            foreach (var category in categories)
            {
                //name (from 3 to 15 characters)
                if (category.Name==null|| category.Name.Length<3 || category.Name.Length>15 )
                {
                    continue;
                }
                validCategories.Add(category);
            }

            context.Categories.AddRange(validCategories);
            context.SaveChanges();



            return string.Format(reportMessage, validCategories.Count);
        }
        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            var users = JsonConvert.DeserializeObject<User[]>(inputJson)
                .Where(x => x.LastName != null && x.LastName.Length >= 3)
                .ToArray();

            var validResults = new List<User>();

            foreach (var user in users)
            {
                if (user.LastName ==null || user.LastName.Length<3)
                {
                    continue;
                }

                validResults.Add(user);
            }

            context.Users.AddRange(validResults);
           var lenght = context.SaveChanges();

            return string.Format(reportMessage, lenght);

            //return $"Successfully imported {lenght}";
        }

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var products = JsonConvert.DeserializeObject<Product[]>(inputJson)
                .Where(x => x.Name != null && x.Name.Trim().Length >= 3)
                .ToArray();


            context.Products.AddRange(products);
            context.SaveChanges();

            return string.Format(reportMessage, products.Length);
            //return $"Successfully imported {products.Length}";

        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .Select(x => new ProductDto
                {
                    Name = x.Name,
                    Price = x.Price,
                    Seller = $"{x.Seller.FirstName} {x.Seller.LastName}"
                })
                .OrderBy(x => x.Price)
                .ToList();
            var json = JsonConvert.SerializeObject(products, Formatting.Indented);
            return json;
        }






    }
}