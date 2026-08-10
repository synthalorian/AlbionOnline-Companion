using StatisticsAnalysisTool.Network;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.EventValidations;

public static class EventValidator
{
    public static void IsEventValid(EventCodes eventCode, Dictionary<byte, object> parameters)
    {
        // Minimal validation - just ensure parameters exist
        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }
    }
}
