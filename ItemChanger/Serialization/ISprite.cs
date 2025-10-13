using Newtonsoft.Json;
using UnityEngine;

namespace ItemChanger.Serialization;

/// <summary>
/// Interface which allows representing functions to define sprites in a serializable manner.
/// </summary>
public interface ISprite
{
    /// <summary>
    /// Gets the defined sprite.
    /// </summary>
    Sprite Value { get; }

    /// <summary>
    /// Creates a deep clone of this sprite.
    /// </summary>
    ISprite Clone();
}

/// <summary>
/// An <see cref="ISprite"/> which retrieves its sprite from a <see cref="ItemChanger.SpriteManager"/>.
/// </summary>
public abstract class EmbeddedSprite : ISprite
{
    /// <summary>
    /// The key of the sprite in the SpriteManager
    /// </summary>
    public string key;

    /// <summary>
    /// The sprite manager which will provide the sprite implementation.
    /// </summary>
    [JsonIgnore]
    public abstract SpriteManager SpriteManager { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public Sprite Value => SpriteManager.GetSprite(key);

    /// <inheritdoc/>
    public ISprite Clone() => (ISprite)MemberwiseClone();
}

/// <summary>
/// A sprite with no content
/// </summary>
public class EmptySprite : ISprite
{
    private Sprite? cachedSprite;

    /// <inheritdoc/>
    [JsonIgnore]
    public Sprite Value
    {
        get
        {
            if (cachedSprite == null)
            {
                Texture2D tex = new Texture2D(1, 1);
                byte[] data = [0, 0, 0, 0];
                tex.LoadRawTextureData(data);
                tex.Apply();
                cachedSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
            }
            return cachedSprite;
        }
    }

    /// <inheritdoc/>
    public ISprite Clone() => new EmptySprite();
}

/// <summary>
/// A sprite that can be changed based on an <see cref="IBool"/>
/// </summary>
public class DualSprite(IBool test, ISprite trueSprite, ISprite falseSprite) : ISprite
{
    public IBool Test => test;
    public ISprite TrueSprite => trueSprite;
    public ISprite FalseSprite => falseSprite;

    /// <inheritdoc/>
    [JsonIgnore]
    public Sprite Value => Test.Value ? TrueSprite.Value : FalseSprite.Value;

    /// <inheritdoc/>
    public ISprite Clone() => (ISprite)MemberwiseClone();
}
