﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fungus
{
	
	public class Dialog : MonoBehaviour 
	{
		public static Character speakingCharacter;
		public static string prevStoryText;

		public float writingSpeed = 60;
		public AudioClip writingSound;
		public bool loopWritingSound = true;
		public float slowBeepsAt;
		public float fastBeepsAt;
		public float punctuationPause = 0.25f;
		public float fadeBoxDuration = 1f;
		public LeanTweenType fadeEaseType;

		[Tooltip("Click anywhere on screen to continue when set to true, or only on dialog when false.")]
		public bool clickAnywhere = true;
		
		public Canvas dialogCanvas;
		public Text nameText;
		public Text storyText;
		public Image characterImage;
		public AudioClip characterTypingSound;

		protected float currentSpeed;
		protected float currentPunctuationPause;
		protected bool boldActive;
		protected bool italicActive;
		protected bool colorActive;
		protected string colorText;
		protected float clickCooldownTimer;
		protected bool slowBeeps;

		protected bool wasPointerClicked;

		protected AudioSource voiceOverAudio;

		protected virtual void LateUpdate()
		{
			wasPointerClicked = false;

			if (clickCooldownTimer > 0f)
			{
				clickCooldownTimer -= Time.deltaTime;
				clickCooldownTimer = Mathf.Max(0, clickCooldownTimer); 
			}

			if (clickCooldownTimer == 0f &&
			    clickAnywhere &&
			    Input.GetMouseButtonDown(0))
			{
				wasPointerClicked = true;
				clickCooldownTimer = 0.2f;
			}
		}

		public virtual void ShowDialog(bool visible)
		{
			if (dialogCanvas != null)
			{
				dialogCanvas.gameObject.SetActive(visible);
			}
		}

		public virtual void FadeInDialog()
		{
			dialogCanvas.GetComponent<CanvasGroup>().alpha = 0;
			dialogCanvas.gameObject.SetActive(true);
			if (fadeBoxDuration == 0) fadeBoxDuration = float.Epsilon;
			LeanTween.value(dialogCanvas.gameObject,0,1,fadeBoxDuration).setEase(fadeEaseType).setOnUpdate(
				(float fadeAmount)=>{
				dialogCanvas.GetComponent<CanvasGroup>().alpha = fadeAmount;
			}
			);
		}
		
		public virtual void FadeOutDialog()
		{
			if (fadeBoxDuration == 0) fadeBoxDuration = float.Epsilon;
			LeanTween.value(dialogCanvas.gameObject,1,0,fadeBoxDuration).setEase(fadeEaseType).setOnComplete(OnCompleteHideDialog).setOnUpdate(
				(float fadeAmount)=>{
				dialogCanvas.GetComponent<CanvasGroup>().alpha = fadeAmount;
			}
			);
		}

		protected void OnCompleteHideDialog()
		{
			dialogCanvas.gameObject.SetActive(false);
		}
		
		public virtual void SetCharacter(Character character, FungusScript fungusScript = null)
		{
			if (character == null)
			{
				if (characterImage != null)
					characterImage.enabled = false;
				if (nameText != null)
					nameText.text = "";
				characterTypingSound = null;
			}
			else
			{
				Character prevSpeakingCharacter = speakingCharacter;
				speakingCharacter = character;

				// Dim portraits of non-speaking characters
				foreach (PortraitStage ps in PortraitStage.activePortraitStages)
				{
					if (ps.dimPortraits)
					{
						foreach (Character c in PortraitStage.charactersOnStage)
						{
							if (prevSpeakingCharacter != speakingCharacter)
							{
								if (c != speakingCharacter)
								{
									Portrait.Dim(c,ps);
								}
								else
								{
									Portrait.Undim(c,ps);
								}
							}
						}
					}
				}

				string characterName = character.nameText;
				if (characterName == "")
				{
					// Use game object name as default
					characterName = character.name;
				}

				if (fungusScript != null)
				{
					characterName = fungusScript.SubstituteVariables(characterName);
				}

				characterTypingSound = character.soundEffect;

				SetCharacterName(characterName, character.nameColor);
			}
		}
		
		public virtual void SetCharacterImage(Sprite image)
		{
			if (characterImage != null)
			{
				if (image != null)
				{
					characterImage.sprite = image;
					characterImage.enabled = true;
				}
				else
				{
					characterImage.enabled = false;
				}
			}
		}
		
		public virtual void SetCharacterName(string name, Color color)
		{
			if (nameText != null)
			{
				nameText.text = name;
				nameText.color = color;
			}
		}

		protected virtual IEnumerator WriteText(string text, Action onWritingComplete, Action onExitTag)
		{
			storyText.text = "";

			// Parse the story text & tag markup to produce a list of tokens for processing
			DialogParser parser = new DialogParser();

			text += " "; // Hack to fix audio not beeping on last character.
			parser.Tokenize(text);

			if (parser.tokens.Count == 0)
			{
				if (onWritingComplete != null)
				{
					onWritingComplete();
				}
				yield break;
			}

			DialogText dialogText = new DialogText();
			dialogText.speed = writingSpeed;
			dialogText.punctuationPause = punctuationPause;

			GameObject typingAudio = null;
			if (characterTypingSound != null || writingSound != null)
			{
				typingAudio = new GameObject("WritingSound");
				typingAudio.AddComponent<AudioSource>();

				if (characterTypingSound != null)
				{
					typingAudio.audio.clip = characterTypingSound;
				}
				else if (writingSound != null)
				{
					typingAudio.audio.clip = writingSound;
				}

				typingAudio.audio.loop = loopWritingSound;
				typingAudio.audio.Play();

				dialogText.typingAudio = typingAudio.audio;
			}

			foreach (Token token in parser.tokens)
			{
				switch (token.type)
				{
				case TokenType.Words:
					dialogText.Append(token.param);
					break;

				case TokenType.BoldStart:
					dialogText.boldActive = true;
					break;

				case TokenType.BoldEnd:
					dialogText.boldActive = false;
					break;

				case TokenType.ItalicStart:
					dialogText.italicActive = true;
					break;

				case TokenType.ItalicEnd:
					dialogText.italicActive = false;
					break;

				case TokenType.ColorStart:
					dialogText.colorActive = true;
					dialogText.colorText = token.param;
					break;

				case TokenType.ColorEnd:
					dialogText.colorActive = false;
					break;

				case TokenType.Wait:
					float duration = 1f;
					if (!Single.TryParse(token.param, out duration))
					{
						duration = 1f;
					}
					yield return StartCoroutine(WaitForSecondsOrInput(duration));
					break;

				case TokenType.WaitForInputNoClear:
					OnWaitForInputTag(true);
					yield return StartCoroutine(WaitForInput(null));
					OnWaitForInputTag(false);
					break;
					
				case TokenType.WaitForInputAndClear:
					OnWaitForInputTag(true);
					yield return StartCoroutine(WaitForInput(null));
					OnWaitForInputTag(false);
					currentSpeed = writingSpeed;
					dialogText.Clear();
					StopVoiceOver();
					break;

				case TokenType.WaitOnPunctuationStart:
					float newPunctuationPause = 0f;
					if (!Single.TryParse(token.param, out newPunctuationPause))
					{
						newPunctuationPause = 0f;
					}
					dialogText.punctuationPause = newPunctuationPause;
					break;
				case TokenType.WaitOnPunctuationEnd:
					dialogText.punctuationPause = punctuationPause;
					break;

				case TokenType.Clear:
					dialogText.Clear();
					break;

				case TokenType.SpeedStart:
					float newSpeed = 0;
					if (!Single.TryParse(token.param, out newSpeed))
					{
						newSpeed = 0f;
					}
					dialogText.speed = newSpeed;
					break;
					
				case TokenType.SpeedEnd:
					dialogText.speed = writingSpeed;
					break;
					
				case TokenType.Exit:
					
					if (onExitTag != null)
					{
						Destroy(typingAudio);
						onExitTag();
					}
					
					yield break;

				case TokenType.Message:
					FungusScript.BroadcastFungusMessage(token.param);
					break;
				}

				// Update text writing
				while (!dialogText.UpdateGlyphs(wasPointerClicked))
				{
					storyText.text = dialogText.GetDialogText();
					yield return null;
				}
				storyText.text = dialogText.GetDialogText();
				wasPointerClicked = false;

				// Now process next token
			}

			prevStoryText = storyText.text;

			Destroy(typingAudio);

			if (onWritingComplete != null)
			{
				onWritingComplete();
			}
			
			yield break;
		}
	
		public virtual void Clear()
		{
			ClearStoryText();

			// Reset control variables
			currentSpeed = 60;
			currentPunctuationPause = 0.25f;
			boldActive = false;
			italicActive = false;
			colorActive = false;
			colorText = "";

			// Kill any active write coroutine
			StopAllCoroutines();
		}
		
		protected virtual void ClearStoryText()
		{
			if (storyText != null)
			{
				storyText.text = "";
			}
		}

		protected virtual IEnumerator WaitForInput(Action onInput)
		{
			while (!wasPointerClicked)
			{
				yield return null;
			}
			wasPointerClicked = false;

			if (onInput != null)
			{
				// Stop all tweening portraits
				foreach( Character c in Character.activeCharacters )
				{
					if (c.state.portraitImage != null)
					{
						if (LeanTween.isTweening(c.state.portraitObj))
						{
							LeanTween.cancel(c.state.portraitObj, true);
							c.state.portraitImage.material.SetFloat( "_Fade", 1 );
							Portrait.SetRectTransform(c.state.portraitImage.rectTransform, c.state.position);
							if (c.state.dimmed == true)
							{
								c.state.portraitImage.material.SetColor("_Color", new Color(0.5f,0.5f,0.5f,1f));
							}
							else
							{
								c.state.portraitImage.material.SetColor("_Color", new Color(1f,1f,1f,1f));
							}
						}
					}
				}
				onInput();
			}
		}

		protected virtual IEnumerator WaitForSecondsOrInput(float duration)
		{
			float timer = duration;
			while (timer > 0 && !wasPointerClicked)
			{
				timer -= Time.deltaTime;
				yield return null;
			}
			
			wasPointerClicked = false;
		}

		protected virtual void OnWaitForInputTag(bool waiting)
		{}

		public virtual void OnPointerClick()
		{
			if (clickCooldownTimer == 0f)
			{
				wasPointerClicked = true;
			}
		}

		public virtual void PlayVoiceOver(AudioClip voiceOverSound)
		{
			if (voiceOverAudio == null)
			{
				voiceOverAudio = gameObject.AddComponent<AudioSource>();
			}
			voiceOverAudio.clip = voiceOverSound;
			voiceOverAudio.Play();
		}

		public virtual void StopVoiceOver()
		{
			if (voiceOverAudio)
			{
				Destroy(voiceOverAudio);
			}
		}
	}
	
}
