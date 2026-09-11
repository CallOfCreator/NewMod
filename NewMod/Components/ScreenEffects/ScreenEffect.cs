using UnityEngine;

namespace NewMod.Components.ScreenEffects;

public abstract class ScreenEffect
{
    public ScreenEffectSystem Owner;
    public Material _mat;
    public bool Active = true;

    public virtual void Initialize() { }
    public virtual void Tick() { }
    public abstract void Render(RenderTexture source, RenderTexture destination);

    public void Remove()
    {
        Owner.Remove(this);
    }

    public virtual void Dispose()
    {
        Active = false;
        if (_mat)
            Object.Destroy(_mat);
        _mat = null;
    }
}
