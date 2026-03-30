using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class DbSeeder
    {

        private readonly IOutageReportService _service;

        public DbSeeder(IOutageReportService service)
        {
            _service = service;
        }
        public async Task SeedAsync(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            AppDbContext db)
        {

            foreach (var roleName in new[] { "Admin", "User" })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
            }

            var admin = await CreateUserIfNotExist(userManager, "ilknuradmin@gmail.com", "ilknuradmin", "Admin123!", "Admin");

       
            var user = await CreateUserIfNotExist(userManager, "ilknuruser@gmail.com", "ilknuruser", "User123!", "User");

          
            var furkan = await CreateUserIfNotExist(userManager, "furkan@gmail.com", "furkan", "Furkan123!", "User");

         
            var reportsToSeed = new List<OutageReport>
            {
                _service.Create("Trafo Arızası", "Ana trafo ünitesi devre dışı kaldı.", "Ankara/Çankaya", Priority.High, admin.Id),
                _service.Create("Kablo Hasarı", "Yer altı ana besleme hattında kopma mevcut.", "İstanbul/Kadıköy", Priority.High, user.Id),
                _service.Create("Sayaç Arızası", "Akıllı sayaç merkezi sisteme yanıt vermiyor.", "İzmir/Bornova", Priority.Medium, user.Id),
                _service.Create("Sokak Aydınlatma Sorunu", "10 adet sokak lambası yanmıyor.", "Bursa/Nilüfer", Priority.Low, furkan.Id),
                _service.Create("Voltaj Dalgalanması", "Bölge genelinde stabil olmayan gerilim tespit edildi.", "Ankara/Keçiören", Priority.High, furkan.Id),
                _service.Create("Sigorta Patlaması", "Binadaki ana bina sigortası attı.", "İstanbul/Beşiktaş", Priority.Medium, admin.Id),
                _service.Create("Genel Elektrik Kesintisi", "Tüm blok genelinde enerji yok.", "İzmir/Konak", Priority.High, user.Id),
                _service.Create("Havai Hat Hasarı", "Fırtına nedeniyle havai hatlarda kopma meydana geldi.", "Antalya/Muratpaşa", Priority.High, furkan.Id),
                _service.Create("Planlı Bakım", "Planlı şebeke iyileştirme ve bakım çalışması.", "Bursa/Osmangazi", Priority.Low, admin.Id),
                _service.Create("Trafo Merkezi Arızası", "İkincil trafo merkezi çalışmıyor.", "Ankara/Mamak", Priority.Medium, user.Id),
                _service.Create("Kaçak Kullanım Şüphesi", "Tespit edilen şüpheli yasadışı bağlantı.", "İstanbul/Fatih", Priority.Medium, admin.Id),
                _service.Create("Jeneratör Arızası", "Yedek jeneratör devreye girmiyor.", "İzmir/Karşıyaka", Priority.Low, furkan.Id),
            };

            foreach (var report in reportsToSeed)
            {
           
                if (!await db.OutageReports.AnyAsync(r => r.Title == report.Title && r.Description == report.Description))
                {
                    await db.OutageReports.AddAsync(report);
                }
            }

            await db.SaveChangesAsync();
        }


        private async Task<AppUser> CreateUserIfNotExist(
            UserManager<AppUser> userManager,
            string email,
            string userName,
            string password,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new AppUser
                {
                    Id = Guid.NewGuid(),
                    UserName = userName,
                    Email = email,
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
            return user;
        }
    }
}