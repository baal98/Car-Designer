using Newtonsoft.Json;

namespace AdvertisingAgency.Web.ViewModels.DTOs
{
    public class CanvasObjectDto
    {

        [JsonProperty("id")]
        public int? Id { get; set; }
        [JsonProperty("name")]
        public string? Name { get; set; }
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("price")]
        public decimal? Price { get; set; }
        [JsonProperty("version")]
        public string? Version { get; set; }
        [JsonProperty("originX")]
        public string? OriginX { get; set; }
        [JsonProperty("originY")]
        public string? OriginY { get; set; }
        [JsonProperty("left")]
        public double? Left { get; set; }
        [JsonProperty("top")]
        public double? Top { get; set; }
        [JsonProperty("width")]
        public double? Width { get; set; }
        [JsonProperty("height")]
        public double? Height { get; set; }
        [JsonProperty("fill")]
        public string? Fill { get; set; }
        [JsonProperty("stroke")]
        public string? Stroke { get; set; }
        [JsonProperty("strokeWidth")]
        public int? StrokeWidth { get; set; }
        [JsonProperty("strokeDashArray")]
        public string? StrokeDashArray { get; set; }
        [JsonProperty("strokeLineCap")]
        public string? StrokeLineCap { get; set; }
        [JsonProperty("strokeDashOffset")]
        public int? StrokeDashOffset { get; set; }
        [JsonProperty("strokeLineJoin")]
        public string? StrokeLineJoin { get; set; }
        [JsonProperty("strokeUniform")]
        public bool? StrokeUniform { get; set; }
        [JsonProperty("strokeMiterLimit")]
        public int? StrokeMiterLimit { get; set; }
        [JsonProperty("scaleX")]
        public double? ScaleX { get; set; }
        [JsonProperty("scaleY")]
        public double? ScaleY { get; set; }
        [JsonProperty("angle")]
        public double? Angle { get; set; }
        [JsonProperty("flipX")]
        public bool? FlipX { get; set; }
        [JsonProperty("flipY")]
        public bool? FlipY { get; set; }
        [JsonProperty("opacity")]
        public double? Opacity { get; set; }
        [JsonProperty("shadow")]
        public string? Shadow { get; set; }
        [JsonProperty("visible")]
        public bool? Visible { get; set; }
        [JsonProperty("backgroundColor")]
        public string? BackgroundColor { get; set; }
        [JsonProperty("fillRule")]
        public string? FillRule { get; set; }
        [JsonProperty("paintFirst")]
        public string? PaintFirst { get; set; }
        [JsonProperty("globalCompositeOperation")]
        public string? GlobalCompositeOperation { get; set; }
        [JsonProperty("skewX")]
        public double? SkewX { get; set; }
        [JsonProperty("skewY")]
        public double? SkewY { get; set; }
        [JsonProperty("cropX")]
        public double? CropX { get; set; }
        [JsonProperty("cropY")]
        public double? CropY { get; set; }
        [JsonProperty("src")]
        public string? Src { get; set; }
        [JsonProperty("crossOrigin")]
        public string? CrossOrigin { get; set; }

        [JsonProperty("filters")]
        public List<object>? Filters { get; set; }

        [JsonProperty("path")]
        public List<List<object>>? Path { get; set; }
    }
}
