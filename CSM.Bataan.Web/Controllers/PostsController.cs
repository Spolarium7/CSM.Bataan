using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeKicker.BBCode;
using CSM.Bataan.Web.Infrastructure.Data.Helpers;
using CSM.Bataan.Web.Infrastructure.Data.Models;
using CSM.Bataan.Web.ViewModels;
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

        [HttpGet, Route("posts")]
        [HttpGet, Route("posts/index")]
        public IActionResult Index()
        {
            return View(new IndexViewModel()
            {
                Posts = Feed(1)
            });
        }

        [HttpGet, Route("posts/feed")]
        public List<Post> Feed(int pageIndex)
        { 
            int skip = (int)(3 * (pageIndex - 1));
            return this._context.Posts
                                .Where(p => p.IsPublished == true)
                                .OrderByDescending(p => p.Timestamp)
                                .Skip(skip)
                                .Take(3)
                                .ToList();
        }


        [HttpGet, Route("posts/{postId}")]
        public IActionResult Post(Guid? postId)
        {
            var post = this._context.Posts.FirstOrDefault(p => p.Id == postId);

            if (post != null)
            {
                return View(new PostViewModel()
                {
                    PostId = post.Id,
                    Title = post.Title,
                    Content = ParseBBCode(post.Content)
                });
            }

            return StatusCode(404);
        }

        public string ParseBBCode(string bbcode)
        {
            var parser = new BBCodeParser(new[]
                {
                    new BBTag("img", "<img src=\"${content}\" />", "", false, true),
                    new BBTag("b", "<strong>", "</strong>"),
                    new BBTag("color","<font  color=\"${color}\">","</font >", new BBAttribute("color", ""), new BBAttribute("color", "color")),
                    new BBTag("i", "<span style=\"font-style:italic;\">", "</span>"),
                    new BBTag("u", "<span style=\"text-decoration:underline;\">", "</span>"),
                    new BBTag("code", "<pre class=\"prettyprint\">", "</pre>"),
                    new BBTag("quote", "<blockquote>", "</blockquote>"),
                    new BBTag("list", "<ul>", "</ul>"),
                    new BBTag("*", "<li>", "</li>", true, false),
                    new BBTag("url", "<a href=\"${href}\">", "</a>", new BBAttribute("href", ""), new BBAttribute("href", "href")),
                    new BBTag("youtube", "<div class='video'><iframe width='550px' height='309px' src='//www.youtube.com/embed/${content}' allowFullScreen></iframe></div>","", false, true),
                });
            return parser.ToHtml(bbcode);
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