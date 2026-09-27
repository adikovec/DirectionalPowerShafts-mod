using System.Collections.Generic;
using System.Linq;
using Timberborn.Coordinates;

namespace DirectionalPowerShafts;

public static class ShaftLineDirections
{
    /// <summary>
    /// The game's two-segment line picker rotates shafts to follow the drag.
    /// Use the selected tool rotation instead, as terrain blocks do, so the
    /// arrow shown on a single hovered shaft stays put when dragging begins.
    /// </summary>
    public static IReadOnlyList<Placement> UseToolOrientation(
        IEnumerable<Placement> source, Orientation toolOrientation)
    {
        return source.Select(placement => new Placement(placement.Coordinates,
            toolOrientation, placement.FlipMode)).ToList();
    }
}
