using System;
using System.Collections.Generic;

namespace MES.API.Models;

public static class ProductionConfig
{
    public static readonly HashSet<string> FinalProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "AOI",
        "Packing",
        "FCT",
        "ICT"
    };
}
