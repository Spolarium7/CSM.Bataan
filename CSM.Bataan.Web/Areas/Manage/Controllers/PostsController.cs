using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSM.Bataan.Web.Areas.Manage.ViewModels.Posts;
using CSM.Bataan.Web.Infrastructure.Data.Helpers;
using CSM.Bataan.Web.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace CSM.Bataan.Web.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class PostsController : Controller
    {
        private readonly DefaultDbContext _context;

        public PostsController(DefaultDbContext context)
        {
            _context = context;
        }

        [HttpGet, Route("manage/posts/index")]
        [HttpGet, Route("manage/posts")]
        public IActionResult Index(int pageIndex = 1, int pageSize = 2, string keyword = "")
        {
            Page<Post> result = new Page<Post>();

            if (pageSize < 1)
            {
                pageSize = 1;
            }

            IQueryable<Post> postQuery = (IQueryable<Post>)this._context.Posts;

            if (string.IsNullOrEmpty(keyword) == false)
            {
                postQuery = postQuery.Where(u => u.Title.ToLower().Contains(keyword.ToLower()));
            }

            long queryCount = postQuery.Count();

            int pageCount = (int)Math.Ceiling((decimal)(queryCount / pageSize));
            long mod = (queryCount % pageSize);

            if (mod > 0)
            {
                pageCount = pageCount + 1;
            }

            int skip = (int)(pageSize * (pageIndex - 1));
            List<Post> users = postQuery.ToList();

            result.Items = users.Skip(skip).Take((int)pageSize).ToList();
            result.PageCount = pageCount;
            result.PageSize = pageSize;
            result.QueryCount = queryCount;
            result.CurrentPage = pageIndex;

            return View(new IndexViewModel() {
                Posts = result
            });
        }

        [HttpGet, Route("manage/posts/create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, Route("manage/posts/create")]
        public IActionResult Create(CreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Post post = new Post()
            {
                Id = Guid.NewGuid(),
                Content = model.Content,
                Description = model.Description,
                IsPublished = true,
                PostExpiry = model.PostExpiry,
                TemplateName = "post1",
                Title = model.Title
            };

            this._context.Posts.Add(post);
            this._context.SaveChanges();

            return View();
        }


    }
}