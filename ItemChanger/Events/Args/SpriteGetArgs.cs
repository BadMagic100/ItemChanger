using System;
using ItemChanger.Serialization;
using UnityEngine;

namespace ItemChanger.Events.Args;

public class SpriteGetArgs : EventArgs
{
    public ISprite Source { get; }
    public Sprite Orig { get; }
    public Sprite Current { get; set; }

    public SpriteGetArgs(ISprite source)
    {
        Source = source;
        Current = Orig = source.Value;
    }
}
