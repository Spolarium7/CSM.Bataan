using CSM.Bataan.Web.Infrastructure.Data.Helpers;
using CSM.Bataan.Web.Infrastructure.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.Bataan.Web.Areas.Manage.ViewModels.Posts
{
    public class IndexViewModel
    {
       public Page<Post> Posts { get; set; }
    }
}
