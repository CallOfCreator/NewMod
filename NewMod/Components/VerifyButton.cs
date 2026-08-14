using System;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public class VerifyButton(IntPtr ptr) : MonoBehaviour(ptr)
{
    public SpriteRenderer Renderer;
    public Sprite NormalSprite;
    public Sprite HoverSprite;
}