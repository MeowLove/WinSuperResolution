using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using WinSuperResolution.Models;

namespace WinSuperResolution.Services
{
    // Windows does not publish a supported per-monitor scale setter. This service deliberately
    // refuses writes until a hardware- and build-specific profile has been verified and embedded.
    internal sealed class ExperimentalScaleService
    {
        private readonly JournalService _journals;

        internal ExperimentalScaleService(JournalService journals)
        {
            _journals = journals;
        }

        internal string GetAvailability(DisplayConfigurationRecord record)
        {
            if (record == null)
                return "Experimental scaling: select a display record first.";
            if (!record.CanManageCurrentState)
                return "Experimental scaling is disabled: it requires an Active + Exact live display match.";
            if (record.LiveDisplay.CurrentScalePercent <= 0)
                return "Experimental scaling is disabled: current per-monitor scale could not be read.";
            if (!IsWindows11OrLater())
                return "Experimental scaling is disabled: this Windows version is not in the compatibility range.";
            return "Experimental scaling is disabled: no verified compatibility profile matches this display mapping yet. Open Windows Display Settings to change scale manually.";
        }

        internal OperationResult Apply(DisplayConfigurationRecord record, int targetScalePercent)
        {
            string availability = GetAvailability(record);
            if (availability.IndexOf("disabled", StringComparison.OrdinalIgnoreCase) >= 0)
                return new OperationResult { Succeeded = false, Message = availability };
            return new OperationResult { Succeeded = false, Message = "No embedded scale profile is currently approved for this mapping." };
        }

        private static bool IsWindows11OrLater()
        {
            OperatingSystem version = Environment.OSVersion;
            return version.Version.Major >= 10;
        }
    }
}
