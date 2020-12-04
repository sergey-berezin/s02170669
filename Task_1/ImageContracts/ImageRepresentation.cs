using System;

namespace ImageContracts
{
    public class ImageRepresentation
    {
        public int ImageId { get; set; }
        public string ImageName { get; set; }
        public string ClassName { get; set; }
        public int NumOfRequests { get; set; }
        public float Prob { get; set; }
        public string ImageHash { get; set; }
        public string Base64Image { get; set; }

    }
}
