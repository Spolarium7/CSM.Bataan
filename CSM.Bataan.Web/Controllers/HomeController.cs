using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CSM.Bataan.Web.Models;
using CSM.Bataan.Web.Infrastructure.Data.Helpers;
using CSM.Bataan.Web.Infrastructure.Data.Models;
using System.IO;

namespace CSM.Bataan.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly DefaultDbContext _context;

        public HomeController(DefaultDbContext context)
        {
            _context = context;
        }

        [HttpGet, Route("")]
        [HttpGet, Route("home")]
        [HttpGet, Route("home/index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet, Route("home/initialize")]
        public IActionResult Init()
        {
            var post = this._context.Posts.FirstOrDefault();

            if (post == null)
            { 
                Post post1 = new Post()
                {
                    Id = Guid.Parse("3a88bea9-8a65-4c23-941a-972a6195b940"),
                    Description = @"The International 2018 was the eight iteration of 
                                Dota 2's flagship annual championship. Hosted by Valve Corporation,
                                it took place at the Rogers Arena in Vancouver,
                                British Columbia, being the first to be celebrated 
                                outside of the United States.",
                    Content = @"The tournament began on 14 June 2018 with the Open Qualifiers and closed on 25 August 2018 with the Grand Final, where fan favorites OG OG defeated Chinese team PSG.LGD PSG.LGD 3-2, becoming the first Grand Final since 2013 to feature five games.
                            With a prize pool of over US$25.3 million, The International 2018 is the largest tournament to ever take place in eSports, breaking the record held by its previous iteration, The International 2017, which accumulated over U$24 million. The majority of this sum is collected through several in-game items released by Valve, including the The International 2018 Battle Pass, a digital tournament pass and features bundle tied to the event that included many activities and cosmetic items. 2018 marks the fifth consecutive year that The International's prize pool outgrows its previous-year record.",
                    IsPublished = true,
                    PostExpiry = DateTime.UtcNow.AddDays(1),
                    TemplateName = "post1",
                    Timestamp = DateTime.UtcNow,
                    Title = "Lakad Matatag!!!"
                };
                this._context.Posts.Add(post1);

                Post post2 = new Post()
                {
                    Id = Guid.Parse("3a88bea9-8a65-4c23-941a-972a6195b941"),
                    Description = @"Monday is a holiday in the US – Memorial Day – so Wizards 
                                    is giving us three MTGO PTQs this weekend.   Saturday is Standard, beginning at 9am PT. Sunday’s PTQ is Sealed starting at 7am PT. Monday’s Modern PTQ starts at 9am PT.",
                    Content = @" Wizards Announcement Day was last Friday.   The article and video are here. Here’s the really short version of what was a pretty short set of announcements:
       *The superfriends are back – on Ravnica. 
       *Guilds of Ravnica set drops in fall, featuring the Golgari, Izzet, Dimir, Selesnya and Boros Guilds. 
       *Ravnica Allegiances comes out in January, with the other five Guilds.
       *Another Ravnica set will appear later, and will feature a big story arc conclusion.
       *Theme decks will appear in fall – one precon per Guild. These will likely be paper only.",
                    IsPublished = true,
                    PostExpiry = DateTime.UtcNow.AddDays(2),
                    TemplateName = "post1",
                    Timestamp = DateTime.UtcNow,
                    Title = "Three Online PTQs this Weekend"
                };
                this._context.Posts.Add(post2);

                Post post3 = new Post()
                {
                    Id = Guid.Parse("3a88bea9-8a65-4c23-941a-972a6195b942"),
                    Description = @"AVENGERS INFINITY WAR director Joe Russo just confirmed the painful truth behind Spider-Man's death. ",
                    Content = @"Spider-Man's death scene in Avengers: Infinity War originally wasn't as long. Marvel Studios did what many thought they wouldn't when it came to the end of Infinity War and the deaths it would have. Fans expected the older guard to say goodbye, but they're actually the last ones standing. This has led to some sidestepping from Marvel, as many of the 'dead' heroes are going to be returning for future films. Tom Holland's Spider-Man is the prime example, as he turned to dust at the end of Infinity War but has Spider-Man: Far From Home hitting theaters two months after Avengers 4.",
                    IsPublished = true,
                    PostExpiry = DateTime.UtcNow.AddDays(3),
                    TemplateName = "post1",
                    Timestamp = DateTime.UtcNow,
                    Title = "Spiderman Dies in Avengers: Infinity War"
                };
                this._context.Posts.Add(post3);

                this._context.SaveChanges();
            }

            var user = this._context.Users.FirstOrDefault();

            if(user == null)
            {
                var admin = new User()
                {
                    Id = Guid.Parse("b2e5a4fc-ca4e-4d3f-b9ac-d8a088cd6401"),
                    EmailAddress = "goshenjimenez@gmail.com",
                    FirstName = "Goshen",
                    LastName = "Jimenez",
                    Gender = Infrastructure.Data.Enums.Gender.Male,
                    LoginStatus = Infrastructure.Data.Enums.LoginStatus.Active,
                    LoginTrials =0,
                    RegistrationCode = RandomString(6),
                    Password = BCrypt.BCryptHelper.HashPassword("Accord605", BCrypt.BCryptHelper.GenerateSalt(8))
                };

                this._context.Users.Add(admin);
                this._context.SaveChanges();

                this._context.UserRoles.Add(new UserRole()
                {
                    Id = Guid.Parse("b2e5a4fc-ca4e-4d3f-b9ac-d8a088cd6401"),
                    Role = Infrastructure.Data.Enums.Role.Admin,
                    UserId = admin.Id
                });

                this._context.SaveChanges();
            }

            return RedirectToAction("index");

            //return RedirectPermanent("~/posts/index");
        }

        private Random random = new Random();
        private string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
