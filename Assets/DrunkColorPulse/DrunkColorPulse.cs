using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(DrunkColorPulseRenderer), PostProcessEvent.AfterStack, "SynapticResponse/DrunkColorPulse")]
public sealed class DrunkColorPulse : PostProcessEffectSettings
{
  [Tooltip("Overlay color")]
  public ColorParameter overlayColor = new ColorParameter { value = Color.red };
  [Range(0.0f, 1.0f), Tooltip("Overlay (pulse) maximum intensity")]
  public FloatParameter overlayMaxIntensity = new FloatParameter { value = 0.2f };
  [Tooltip("Pulse enabled")]
  public BoolParameter pulseEnabled = new BoolParameter { value = true };
  [Range(0.01f, 100.0f), Tooltip("Pulse speed")]
  public FloatParameter pulseSpeed = new FloatParameter { value = 4.0f };
  [Range(0.0f, 0.99f), Tooltip("Trail blur strength")]
  public FloatParameter trailBlurStrength = new FloatParameter { value = 0.875f };
}
public sealed class DrunkColorPulseRenderer : PostProcessEffectRenderer<DrunkColorPulse>
{
  Shader shader;
  private RenderTexture prevFrame;
  private UnityEngine.Rendering.RenderTargetIdentifier rtid;

  public override void Init()
  {
    int width = Mathf.CeilToInt(Screen.width * 1.0f);
    int height = Mathf.CeilToInt(Screen.height * 1.0f);
    prevFrame = new RenderTexture(width, height, 24, RenderTextureFormat.ARGBHalf);
    prevFrame.useMipMap = false;
    prevFrame.filterMode = FilterMode.Bilinear;
    prevFrame.name = "prev_frame";
    prevFrame.Create();
    rtid = new UnityEngine.Rendering.RenderTargetIdentifier(prevFrame);
    shader = Shader.Find("Hidden/SynapticResponse/DrunkColorPulse");
  }

  public override void Render(PostProcessRenderContext context)
  {
    var sheet = context.propertySheets.Get(shader);
    sheet.properties.SetColor("_OverlayColor", settings.overlayColor);
    sheet.properties.SetFloat("_OverLayMaxIntensity", settings.overlayMaxIntensity);
    sheet.properties.SetInteger("_PulseEnabled", settings.pulseEnabled ? 1 : 0);
    sheet.properties.SetFloat("_PulseSpeed", settings.pulseSpeed);    
    sheet.properties.SetFloat("_TrailBlurStrength", settings.trailBlurStrength);
    sheet.properties.SetTexture("_PrevFrameTex", prevFrame);
    context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    context.command.Blit(null, rtid);
  }
}
