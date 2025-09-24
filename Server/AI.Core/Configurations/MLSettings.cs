namespace AI.Core.Configurations;

public class MLSettings
{
    public string ModelFilePath { get; set; } = "";
    public string ImagesFolder { get; set; } = "";
    public int ImageWidth { get; set; } = 416;
    public int ImageHeight { get; set; } = 416;
    public float ScoreThreshold { get; set; } = 0.5f;
    public int MaxDetections { get; set; } = 5;
}