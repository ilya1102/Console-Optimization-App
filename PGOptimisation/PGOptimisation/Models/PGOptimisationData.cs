namespace PGOptimisation.Models
{
    public class PGOptimisationData
    {
        public double[] BaseGas { get; set; } = Array.Empty<double>();
        public double[] MinGas { get; set; } = Array.Empty<double>();
        public double[] MaxGas { get; set; } = Array.Empty<double>();
        public double[] E { get; set; } = Array.Empty<double>();
        public double[] BaseCoke { get; set; } = Array.Empty<double>();
        public double[] BaseProduction { get; set; } = Array.Empty<double>();
        public double[] DeltaPgCoeff { get; set; } = Array.Empty<double>();
        public double[] DeltaCokeCoeff { get; set; } = Array.Empty<double>();
        public double[] SiCoeffPg { get; set; } = Array.Empty<double>();
        public double[] SiCoeffCoke { get; set; } = Array.Empty<double>();
        public double[] SiCoeffProd { get; set; } = Array.Empty<double>();
        public double[] SiBase { get; set; } = Array.Empty<double>();
        public double[] SiMin { get; set; } = Array.Empty<double>();
        public double[] SiMax { get; set; } = Array.Empty<double>();
        public double TotalGasLimit { get; set; }
        public double TotalCokeLimit { get; set; }
        public double RequiredProduction { get; set; }
    }
}