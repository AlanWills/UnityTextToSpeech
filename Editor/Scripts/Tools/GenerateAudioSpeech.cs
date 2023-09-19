using Google.Cloud.TextToSpeech.V1;
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

        public string FullPathToLanguageFolder => Path.Combine(Application.dataPath, outputFolder, languageCode);

        public string gcpCredentialsPath;
        [HideInInspector] public string languageCode;
        [HideInInspector] public List<AudioSpeech> text;
        public string outputFolder;

        #endregion

        public string PathRelativeToProjectFolder(string outputName)
        {
            return Path.Combine("Assets", outputFolder, languageCode, $"{outputName}.mp3");
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
                    SsmlGender = SsmlVoiceGender.Neutral
                };

                AudioConfig audioConfig = new AudioConfig()
                {
                    AudioEncoding = AudioEncoding.Mp3
                };

                string fullOutputPath = Path.Combine(outputDirectory, $"{audioSpeech.outputName}.mp3");
                DirectoryInfo fullOutputDirectory = Directory.GetParent(fullOutputPath);

                if (!fullOutputDirectory.Exists)
                {
                    fullOutputDirectory.Create();
                }

                SynthesizeSpeechResponse response = textToSpeechClient.SynthesizeSpeech(input, voice, audioConfig);
                File.WriteAllBytes(fullOutputPath, response.AudioContent.ToByteArray());
            }

            AssetDatabase.Refresh();

            foreach (AudioSpeech audioSpeech in parameters.text)
            {
                string fullOutputPath = parameters.PathRelativeToProjectFolder(audioSpeech.outputName);
                AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(fullOutputPath);
                Debug.Assert(audioClip != null, $"Could not find audio clip for speech {audioSpeech.outputName}.");

                if (audioClip != null)
                {
                    audioClips.Add(audioClip);
                }
            }


            return audioClips;
        }
    }
}