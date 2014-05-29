using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Flashcards.ContentManager.Models
{
    public class PackageModel
    {
        public string ProductName
        {
            get;
            set;
        }

        public HttpPostedFileBase PackageFile
        {
            get;
            set;
        }
    }

    public class ContentModel
    {
        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public DateTime? DateUploaded
        {
            get;
            set;
        }

        public string UploadedBy
        {
            get;
            set;
        }
    }
}