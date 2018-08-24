using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSM.Bataan.Web.Infrastructure.Data.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CSM.Bataan.Web.Controllers
{
    public class PostsController : Controller
    {
        private readonly DefaultDbContext _context;

        public PostsController(DefaultDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            var posts = this._context.Posts.ToList();

            return View();
        }

        public IActionResult Post(Guid? postId)
        {
            return View();
        }

        public IActionResult Carousel()
        {
            return View();
        }

        public IActionResult Gallery()
        {
            return View();
        }
    }
}