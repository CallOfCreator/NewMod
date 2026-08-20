using System;
using NewMod.GeneralEvents.Season1;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components.ScreenEffects
{
    [RegisterInIl2Cpp]
    public class SystemOverrideEffect(nint ptr) : MonoBehaviour(ptr)
    {
        public float _startedAt;

        public void OnEnable()
        {
            _startedAt = Time.time;
        }

        public void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (!SystemOverrideGE.Active || MeetingHud.Instance || ExileController.Instance)
            {
                Graphics.Blit(src, dst);
                return;
            }

            float time = Time.time - _startedAt;

            if (time < 0.09f || time is >= 0.16f and < 0.27f || Mathf.Repeat(time, 2.7f) < 0.05f)
            {
                Graphics.Blit(Texture2D.blackTexture, dst);
                return;
            }

            int phase = Mathf.FloorToInt(time / 1.7f) % 5;

            switch (phase)
            {
                case 0:
                    Graphics.Blit(src, dst);
                    break;
                case 1:
                    Graphics.Blit(src, dst, new Vector2(-1f, 1f), new Vector2(1f, 0f));
                    break;
                case 2:
                    Graphics.Blit(src, dst, new Vector2(1f, -1f), new Vector2(0f, 1f));
                    break;
                case 3:
                    Graphics.Blit(src, dst, new Vector2(-1f, -1f), Vector2.one);
                    break;
                default:
                    float offset = Mathf.Sin(time * 43f) * 0.018f;
                    Graphics.Blit(src, dst, Vector2.one, new Vector2(offset, 0f));
                    break;
            }
        }
    }
}