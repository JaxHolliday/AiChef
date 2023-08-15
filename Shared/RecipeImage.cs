using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiChef.Shared
{
    //creating model to grab data from open AI
    public class RecipeImage
    {
        public int created { get; set; }
        public ImageData[] data { get; set; }

    }

    public class ImageData
    {
        public string url { get; set; }
    }
}
