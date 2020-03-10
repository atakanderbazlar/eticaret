using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopApp.Business.Abstract;
using ShopApp.Business.Concrete;
using ShopApp.DataAccess.Abstract;
using ShopApp.DataAccess.Concrete.EfCore;
using ShopApp.WebUI.EmailServices;
using ShopApp.WebUI.Identity;
using ShopApp.WebUI.Middlewares;

namespace ShopApp.WebUI
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationIdentityDbContext>(options =>
             options.UseSqlServer(Configuration.GetConnectionString("IdentityConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // password

                options.Password.RequireDigit = true; //sayısal değer
                options.Password.RequireLowercase = true; //küçük harf
                options.Password.RequiredLength = 6; //min kaç karakter
                options.Password.RequireNonAlphanumeric = true; //alfanumerik numara zorunlu değil
                options.Password.RequireUppercase = true; //büyük harf

                options.Lockout.MaxFailedAccessAttempts = 5; //yanlış parola girme hakkı
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // 5 dk kilit
                options.Lockout.AllowedForNewUsers = true; //yeni kullanıcı içinde geçerli

                // options.User.AllowedUserNameCharacters = ""; //User name içerisine alınmayan harfler
                options.User.RequireUniqueEmail = true; //aynı mail adresi engelleme

                options.SignIn.RequireConfirmedEmail = true; //mail onayı yapmak zorunda
                options.SignIn.RequireConfirmedPhoneNumber = false; //telefon onayı ..
            });


            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/account/login"; //login sayfası locasyonu
                options.LogoutPath = "/account/logout"; //logout say...
                options.AccessDeniedPath = "/account/accessdenied"; //yetkisi olunmayan yere girerse buraya yönlendir
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Cookie süresi
                options.SlidingExpiration = true; // Cookie süresi tutma 
                options.Cookie = new CookieBuilder
                {
                    HttpOnly = true, //scriptler cookieye ulaşamaz
                    Name = ".ShopApp.Security.Cookie", //cookie ismi belirttik
                    SameSite = SameSiteMode.Strict //başka bir kullanıcı bizim cookie'yi alıp server'a gönderemez.
                };

            });

            services.AddScoped<IProductDal, EfCoreProductDal>();
            services.AddScoped<ICategoryDal, EfCoreCategoryDal>();
            services.AddScoped<ICartDal, EfCoreCartDal>();
            services.AddScoped<IOrderDal, EfCoreOrderDal>();

            services.AddScoped<IProductService, ProductManager>();
            services.AddScoped<ICategoryService, CategoryManager>();
            services.AddScoped<ICartService, CartManager>();
            services.AddScoped<IOrderService, OrderManager>();

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                SeedDatabase.Seed();
            }
            app.UseStaticFiles();
            app.CustomStaticFiles();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                 name: "adminProducts",
                 template: "admin/products",
                 defaults: new { controller = "Admin", action = "ProductList" }
               );

                routes.MapRoute(
                    name: "adminProducts",
                    template: "admin/products/{id?}",
                    defaults: new { controller = "Admin", action = "EditProduct" }
                );
                routes.MapRoute(
                    name: "adminCategories",
                    template: "admin/categories",
                    defaults: new { controller = "Admin", action = "CategoryList" }
                );

                routes.MapRoute(
                    name: "adminCategories",
                    template: "admin/categories/{id?}",
                    defaults: new { controller = "Admin", action = "EditCategory" }
                );

                routes.MapRoute(
                    name: "cart",
                    template: "cart",
                    defaults: new { controller = "Cart", action = "Index" }
                );
                routes.MapRoute(
                     name: "cart",
                     template: "products",
                     defaults: new { controller = "Cart", action = "AddToCart" }
                );

                routes.MapRoute(
                    name: "orders",
                    template: "orders",
                    defaults: new { controller = "Cart", action = "GetOrders" }
                );

                routes.MapRoute(
                   name: "checkout",
                   template: "checkout",
                   defaults: new { controller = "Cart", action = "Checkout" }
               );

                routes.MapRoute(
                  name: "products",
                  template: "products/{category?}",
                  defaults: new { controller = "Shop", action = "List" }
                );

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}"
                );

            });

            SeedIdentity.Seed(userManager, roleManager, Configuration).Wait();

        }
    }
}
