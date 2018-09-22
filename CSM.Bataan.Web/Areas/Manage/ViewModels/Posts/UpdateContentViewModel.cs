using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.Bataan.Web.Areas.Manage.ViewModels.Posts
{
    public class UpdateContentViewModel
    {
        public Guid? PostId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }
    }
}
