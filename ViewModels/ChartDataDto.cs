using System.Collections.Generic;

namespace AgencyFlow.ViewModels
{
    public class ChartDataDto
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<ChartDatasetDto> Datasets { get; set; } = new List<ChartDatasetDto>();
    }

    public class ChartDatasetDto
    {
        public string Label { get; set; } = string.Empty;
        public List<decimal> Data { get; set; } = new List<decimal>();
        public string BorderColor { get; set; } = "rgba(78, 115, 223, 1)";
    }
}
