using CSM.Bataan.Web.Infrastructure.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.Bataan.Web.Infrastructure.Data.Models
{
    public class Post : BaseModel
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public string Description { get; set; }

        public string TemplateName { get; set; }

        public bool IsPublished { get; set; }

        public DateTime PostExpiry { get; set; }
    }
}
