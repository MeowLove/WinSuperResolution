using System;
using System.Collections.Generic;

namespace WinSuperResolution.Models
{
    public sealed class RegistryMutation
    {
        public string RelativePath { get; set; }
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }
        public int TargetWidth { get; set; }
        public int TargetHeight { get; set; }
    }

    public sealed class ResolutionPlan
    {
        public ResolutionPlan()
        {
            Mutations = new List<RegistryMutation>();
        }

        public DisplayConfigurationRecord Record { get; set; }
        public int Magnification { get; set; }
        public CalculationBasis Basis { get; set; }
        public int BaseWidth { get; set; }
        public int BaseHeight { get; set; }
        public int TargetWidth { get; set; }
        public int TargetHeight { get; set; }
        public IList<RegistryMutation> Mutations { get; private set; }

        public string Summary
        {
            get
            {
                return string.Format("{0}% using {1}: {2} x {3} -> {4} x {5}; {6} registry target(s).",
                    Magnification,
                    Basis,
                    BaseWidth,
                    BaseHeight,
                    TargetWidth,
                    TargetHeight,
                    Mutations.Count);
            }
        }
    }
}
