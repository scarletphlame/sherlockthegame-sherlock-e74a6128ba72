using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;

namespace Fungus
{
	/**
	 * Transitions a sprite from its current color to a target color.
	 * An offset can be applied to slide the sprite in while changing color.
	 */
	[RequireComponent (typeof (Image))]
	public class ImageFader : MonoBehaviour 
	{
		protected float fadeDuration;
		protected float fadeTimer;
		protected Color startColor;
		protected Color endColor;
		protected Vector2 slideOffset;
		protected Vector3 endPosition;
		
		protected Image image;
		
		protected Action onFadeComplete;
		
		/** 
		 * Attaches a ImageFader component to a sprite object to transition its color over time.
		 */
		public static void FadeSprite(Image image, Color targetColor, float duration, Vector2 slideOffset, Action onComplete = null)
		{
			if (image == null)
			{
				Debug.LogError("Image must not be null");
				return;
			}
			
			// Fade child sprite renderers
			Image[] children = image.gameObject.GetComponentsInChildren<Image>();
			foreach (Image child in children)
			{
				if (child == image)
				{
					continue;
				}
				
				FadeSprite(child, targetColor, duration, slideOffset);
			}
			
			// Destroy any existing fader component
			ImageFader oldImageFader = image.GetComponent<ImageFader>();
			{
				Destroy(oldImageFader);
			}
			
			// Early out if duration is zero
			if (duration == 0f)
			{
				image.color = targetColor;
				if (onComplete != null)
				{
					onComplete();
				}
				return;
			}
			
			// Set up color transition to be applied during update
			ImageFader imageFader = image.gameObject.AddComponent<ImageFader>();
			imageFader.fadeDuration = duration;
			imageFader.startColor = image.color;
			imageFader.endColor = targetColor;
			imageFader.endPosition = image.transform.position;
			imageFader.slideOffset = slideOffset;
			imageFader.onFadeComplete = onComplete;
		}
		
		protected virtual void Start()
		{
			//image = renderer as Image;
		}
		
		protected virtual void Update() 
		{
			fadeTimer += Time.deltaTime;
			if (fadeTimer > fadeDuration)
			{
				// Snap to final values
				image.color = endColor;
				if (slideOffset.magnitude > 0)
				{
					transform.position = endPosition;
				}
				
				// Remove this component when transition is complete
				Destroy(this);
				
				if (onFadeComplete != null)
				{
					onFadeComplete();
				}
			}
			else
			{
				float t = Mathf.SmoothStep(0, 1, fadeTimer / fadeDuration);
				image.color = Color.Lerp(startColor, endColor, t);
				if (slideOffset.magnitude > 0)
				{
					Vector3 startPosition = endPosition;
					startPosition.x += slideOffset.x;
					startPosition.y += slideOffset.y;
					transform.position = Vector3.Lerp(startPosition, endPosition, t);
				}
			}
		}		
	}
}
