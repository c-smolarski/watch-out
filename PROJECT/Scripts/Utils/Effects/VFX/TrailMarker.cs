using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.Utils.Effects
{
    public partial class TrailMarker : Marker2D
    {
        [Export] public Trail TrailLine { get; private set; }
    }
}
