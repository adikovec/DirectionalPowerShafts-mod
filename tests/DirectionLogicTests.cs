using System;
using System.Collections.Generic;
using System.Linq;
using DirectionalPowerShafts;
using Timberborn.Coordinates;
using UnityEngine;

static Placement At(int x, int y, Orientation orientation = Orientation.Cw0) =>
    new(new Vector3Int(x, y, 0), orientation, FlipMode.Unflipped);

static void Check(IEnumerable<Placement> input, Orientation chosen)
{
    var source = input.ToList();
    var result = ShaftLineDirections.UseToolOrientation(source, chosen);
    if (result.Count != source.Count || result.Any(placement =>
        placement.Orientation != chosen))
    {
        throw new Exception($"Expected every arrow to keep {chosen}.");
    }
    for (var i = 0; i < source.Count; i++)
    {
        if (result[i].Coordinates != source[i].Coordinates
            || result[i].FlipMode != source[i].FlipMode)
        {
            throw new Exception("Placement coordinates or flip mode changed.");
        }
    }
}

var straight = new[] { At(0, 0), At(1, 0, Orientation.Cw270),
    At(2, 0, Orientation.Cw270), At(3, 0, Orientation.Cw270) };
var corner = new[] { At(0, 0), At(1, 0, Orientation.Cw270),
    At(1, 1, Orientation.Cw180) };

foreach (var orientation in new[] { Orientation.Cw0, Orientation.Cw90,
    Orientation.Cw180, Orientation.Cw270 })
{
    Check(straight, orientation);
    Check(corner, orientation);
    Check(new[] { At(0, 0, orientation) }, orientation);
    Check(Array.Empty<Placement>(), orientation);
}

// Hovering one shaft and then dragging must not rotate its arrow.
foreach (var orientation in new[] { Orientation.Cw0, Orientation.Cw90,
    Orientation.Cw180, Orientation.Cw270 })
{
    var hovered = ShaftLineDirections.UseToolOrientation(
        new[] { At(0, 0) }, orientation)[0];
    var dragged = ShaftLineDirections.UseToolOrientation(straight, orientation)[0];
    if (hovered.Orientation != dragged.Orientation)
    {
        throw new Exception("Arrow changed when drag began.");
    }
}

Console.WriteLine("Shaft placement directions: OK");
