using System;
using WinSuperResolution.Models;

namespace WinSuperResolution.Services
{
    internal sealed class ResolutionPlanService
    {
        internal ResolutionPlan Build(DisplayConfigurationRecord record, int magnification)
        {
            if (record == null)
            {
                throw new InvalidOperationException("Select a display configuration first.");
            }

            if (!record.CanApplyVirtualCapability)
            {
                throw new InvalidOperationException("Virtual-resolution capability changes are disabled because multiple registry configurations match the same active Windows display.");
            }

            if (magnification < 100 || magnification > 350 || magnification % 10 != 0)
            {
                throw new InvalidOperationException("Magnification must be between 100% and 350% in 10% increments.");
            }

            int baseWidth;
            int baseHeight;
            CalculationBasis basis;
            if (record.HasActiveSignal)
            {
                baseWidth = record.ActiveSignalWidth;
                baseHeight = record.ActiveSignalHeight;
                basis = CalculationBasis.ActiveSize;
            }
            else if (record.HasPrimarySurface)
            {
                baseWidth = record.PrimarySurfaceWidth;
                baseHeight = record.PrimarySurfaceHeight;
                basis = CalculationBasis.PrimSurfSize;
            }
            else
            {
                throw new InvalidOperationException("The selected configuration has no valid calculation basis.");
            }

            ResolutionPlan plan = new ResolutionPlan();
            plan.Record = record;
            plan.Magnification = magnification;
            plan.Basis = basis;
            plan.BaseWidth = baseWidth;
            plan.BaseHeight = baseHeight;
            plan.TargetWidth = CalculateScaled(baseWidth, magnification);
            plan.TargetHeight = CalculateScaled(baseHeight, magnification);

            foreach (RegistryTarget target in record.RegistryTargets)
            {
                RegistryMutation mutation = new RegistryMutation();
                mutation.RelativePath = target.RelativePath;
                mutation.OriginalWidth = target.PrimarySurfaceWidth;
                mutation.OriginalHeight = target.PrimarySurfaceHeight;
                mutation.TargetWidth = plan.TargetWidth;
                mutation.TargetHeight = plan.TargetHeight;
                plan.Mutations.Add(mutation);
            }

            return plan;
        }

        private static int CalculateScaled(int value, int magnification)
        {
            double scaled = value * magnification / 100.0;
            if (scaled > int.MaxValue)
            {
                throw new InvalidOperationException("Calculated resolution exceeds the supported range.");
            }

            return (int)Math.Floor(scaled + 0.5);
        }
    }
}
