using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSM.Bataan.Web.Areas.Manage.ViewModels.Posts;
using CSM.Bataan.Web.Infrastructure.Data.Helpers;
using CSM.Bataan.Web.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CSM.Bataan.Web.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class PostsController : Controller
    {
        private readonly DefaultDbContext _context;
        private IHostingEnvironment _env;

        public PostsController(DefaultDbContext context, IHostingEnvironment env)
        {
            _context = context;
            _env = env;
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
            result.PageIndex = pageIndex;
            result.Keyword = keyword;

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

        [HttpPost, Route("manage/posts/unpublish")]
        public IActionResult Unpublish(PostIdViewModel model)
        {
            var post = this._context.Posts.FirstOrDefault(p => p.Id == model.Id);

            if (post != null)
            {
                post.IsPublished = false;
                this._context.Posts.Update(post);
                this._context.SaveChanges();
                return Ok();
            }

            return null;
        }

        [HttpPost, Route("manage/posts/publish")]
        public IActionResult Publish(PostIdViewModel model)
        {
            var post = this._context.Posts.FirstOrDefault(p => p.Id == model.Id);

            if (post != null)
            {
                post.IsPublished = true;
                this._context.Posts.Update(post);
                this._context.SaveChanges();
                return Ok();
            }

            return null;
        }

        [HttpGet, Route("/manage/posts/update-title/{postId}")]
        public IActionResult UpdateTitle(Guid? postId)
        {
            var post = this._context.Posts.FirstOrDefault(p => p.Id == postId);

            if(post != null)
            {
                var model = new UpdateTitleViewModel()
                {
                    Id = post.Id,
                    Description = post.Description,
                    Title = post.Title,
                    PostExpiry = post.PostExpiry,
                    TemplateName = post.TemplateName
                };

                return View(model);
            }

            return RedirectToAction("Create");
        }

        [HttpPost, Route("/manage/posts/update-title")]
        public IActionResult UpdateTitle(UpdateTitleViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var post = this._context.Posts.FirstOrDefault(p => p.Id == model.Id);

            if(post != null)
            {
                post.Title = model.Title;
                post.Description = model.Description;
                post.PostExpiry = model.PostExpiry;
                post.TemplateName = model.TemplateName;

                this._context.Posts.Update(post);
                this._context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpGet, Route("/manage/posts/update-thumbnail/{postId}")]
        public IActionResult Thumbnail(Guid? postId)
        {
            return View(new ThumbnailViewModel() { PostId = postId });
        }

        [HttpPost, Route("/manage/posts/update-thumbnail")]
        public async Task<IActionResult> Thumbnail(ThumbnailViewModel model)
        {
            //Check file size of the uploaded thumbnail
            //reject if the file is greater than 2mb
            var fileSize = model.Thumbnail.Length;
            if ((fileSize / 1048576.0) > 2)
            {
                ModelState.AddModelError("", "The file you uploaded is too large. Filesize limit is 2mb.");
                return View(model);
            }

            //Check file type of the uploaded thumbnail
            //reject if the file is not a jpeg or png
            if (model.Thumbnail.ContentType != "image/jpeg" && model.Thumbnail.ContentType != "image/png")
            {
                ModelState.AddModelError("", "Please upload a jpeg or png file for the thumbnail.");
                return View(model);
            }

            //Formulate the directory where the file will be saved
            //create the directory if it does not exist
            var dirPath = _env.WebRootPath + "/posts/" + model.PostId.ToString();
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            //always name the file thumbnail.png
            var filePath = dirPath + "/thumbnail.png";

            if (model.Thumbnail.Length > 0)
            {

                //Open a file stream to read all the file data into a byte array
                byte[] bytes = await FileBytes(model.Thumbnail.OpenReadStream());

                //load the file into the third party (ImageSharp) Nuget Plugin                
                using (Image<Rgba32> image = Image.Load(bytes))
                {
                    //use the Mutate method to resize the image 150px wide by 150px long
                    image.Mutate(x => x.Resize(150, 150));

                    //save the image into the path formulated earlier
                    image.Save(filePath);
                }      
            }

            return RedirectToAction("Thumbnail", new { PostId = model.PostId });
        }


        //this method is used to load the file stream into 
        //a byte array
        public async Task<byte[]> FileBytes(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}