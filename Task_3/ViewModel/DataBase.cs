using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.IO;

namespace ViewModel
{
    public class ImageClass
    {
        public int ImageClassId { get; set; }
        public string ClassName { get; set; }
        public int Count { get; set; }

        public ICollection<ImageInfo> Images { get; set; }


    }

    public class ImageInfo
    {
        public int ImageInfoId { get; set; }
        public string ClassName { get; set; }

        public float Prob { get; set; }
        public int NumOfRequests { get; set; }
        public string Path { get; set; }
        public ImageFile ByteImage { get; set; }

        public ICollection<ImageClass> ImageClasses { get; set; }

    }

    public class ImageFile
    {
        public int ImageFileId { get; set; }
        public byte[] Img { get; set; }

    }

    public class LibraryContext : DbContext
    {
        public DbSet<ImageClass> ImageClasses { get; set; }
        public DbSet<ImageInfo> Images { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder o)
            => o.UseSqlite("Data Source=ImageDataBase.db");
    }
}
