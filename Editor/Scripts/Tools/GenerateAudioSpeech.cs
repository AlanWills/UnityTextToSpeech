﻿using Google.Cloud.TextToSpeech.V1;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CelesteEditor.Tools
{
    [Serializable]
    public struct AudioSpeech
    {
        public string text;
        public string outputName;
    }

    [Serializable]
    public struct GenerateAudioSpeechParameters
    {
        #region Properties and Fields

        public string FullPathToLanguageFolder => $"{Application.dataPath}/../{outputFolder}/{languageCode}";

        public string gcpCredentialsPath;
        [HideInInspector] public string languageCode;
		public string voiceName;
        [HideInInspector] public List<AudioSpeech> text;
        public string outputFolder;

        #endregion
		
		public void SetDefaultValues()
		{
			gcpCredentialsPath = Path.Combine(Application.streamingAssetsPath, "gcp_credentials.json");
			text = new List<AudioSpeech>();
			outputFolder = "Assets/Localisation/Audio";
		}

        public string PathRelativeToProjectFolder(string outputName)
        {
            return $"{outputFolder}/{languageCode}/{outputName}";
        }
    }

    public static class GenerateAudioSpeech
    {
        public static List<AudioClip> Generate(GenerateAudioSpeechParameters parameters)
        {
            List<AudioClip> audioClips = new List<AudioClip>();

            if (!File.Exists(parameters.gcpCredentialsPath))
            {
                Debug.LogError($"Could not find gcp credentials at path {parameters.gcpCredentialsPath}.");
                return audioClips;
            }

            string outputDirectory = parameters.FullPathToLanguageFolder;

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            Environment.SetEnvironmentVariable(GenerateAudioSpeechConstants.GOOGLE_APPLICATION_CREDENTIALS, parameters.gcpCredentialsPath);

            // Instantiate a client
            TextToSpeechClient textToSpeechClient = TextToSpeechClient.Create();

            foreach (AudioSpeech audioSpeech in parameters.text)
            {
                SynthesisInput input = new SynthesisInput()
                {
                    Text = audioSpeech.text
                };

                VoiceSelectionParams voice = new VoiceSelectionParams()
                {
                    LanguageCode = parameters.languageCode,
                    SsmlGender = SsmlVoiceGender.Neutral,
					Name = parameters.voiceName
                };

                AudioConfig audioConfig = new AudioConfig()
                {
                    AudioEncoding = AudioEncoding.Mp3
                };

				try
				{
					string fullOutputPath = Path.Combine(outputDirectory, audioSpeech.outputName);
					DirectoryInfo fullOutputDirectory = Directory.GetParent(fullOutputPath);

					if (!fullOutputDirectory.Exists)
					{
						fullOutputDirectory.Create();
					}

					SynthesizeSpeechResponse response = textToSpeechClient.SynthesizeSpeech(input, voice, audioConfig);
					File.WriteAllBytes(fullOutputPath, response.AudioContent.ToByteArray());
				}
				catch
				{
					Debug.LogError($"Serious error whilst trying to generate audio file for {audioSpeech.outputName}.");
				}
            }

            AssetDatabase.Refresh();

            foreach (AudioSpeech audioSpeech in parameters.text)
            {
                string fullOutputPath = parameters.PathRelativeToProjectFolder(audioSpeech.outputName);
                AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(fullOutputPath);
                Debug.Assert(audioClip != null, $"Could not find audio clip for speech {audioSpeech.outputName} at path {fullOutputPath}.");

                if (audioClip != null)
                {
                    audioClips.Add(audioClip);
                }
            }


            return audioClips;
        }
    }
}