using Newtonsoft.Json;

namespace Tests
{
    public class CustomResolution
    {
        public CustomResolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        [JsonProperty]
        public readonly int Width;
        [JsonProperty]
        public readonly int Height;
    }
}