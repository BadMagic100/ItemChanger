using ItemChanger.Internal;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace ItemChanger;

public interface ISprite
{
    Sprite Value { get; }
    ISprite Clone();
}

/// <summary>
/// ISprite wrapper for Sprite. Use only for items created and disposed at runtime--it is not serializable.
/// </summary>
public class BoxedSprite : ISprite
{
    public Sprite Value { get; set; }

    public BoxedSprite(Sprite Value) => this.Value = Value;

    public ISprite Clone() => (ISprite)MemberwiseClone();
}

/// <summary>
/// An ISprite which retrieves its sprite from a SpriteManager.
/// </summary>
public abstract class EmbeddedSprite : ISprite
{
    public string key;
    [JsonIgnore] public abstract SpriteManager SpriteManager { get; }
    [JsonIgnore] public Sprite Value => SpriteManager.GetSprite(key);
    public ISprite Clone() => (ISprite)MemberwiseClone();
}

[Serializable]
public class EmptySprite : ISprite
{
    private Sprite? cachedSprite;
    [JsonIgnore] 
    public Sprite Value
    {
        get
        {
            if (cachedSprite == null)
            {
                Texture2D tex = new Texture2D(1, 1);
                byte[] date = [0, 0, 0, 0];
                tex.LoadRawTextureData(date);
                tex.Apply();
                cachedSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
            }
            return cachedSprite;
        }
    }
    public ISprite Clone() => new EmptySprite();
}

[Serializable]
public class DualSprite : ISprite
{
    public IBool Test;
    public ISprite TrueSprite;
    public ISprite FalseSprite;

    public DualSprite(IBool Test, ISprite TrueSprite, ISprite FalseSprite)
    {
        this.Test = Test;
        this.TrueSprite = TrueSprite;
        this.FalseSprite = FalseSprite;
    }

    [JsonIgnore] public Sprite Value => Test.Value ? TrueSprite.Value : FalseSprite.Value;
    public ISprite Clone() => (ISprite)MemberwiseClone();
}
