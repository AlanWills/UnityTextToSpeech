using Google.Cloud.TextToSpeech.V1;
using Google.Protobuf;
using System;
using System.IO;
using UnityEngine;

public class TextToSpeech : MonoBehaviour
{
    private const string CredentialFileName = "gcp_credentials.json";

    // Start is called before the first frame update
    private void Start()
    {
        string credentialsPath = Path.Combine(Application.streamingAssetsPath, CredentialFileName);
        if (!File.Exists(credentialsPath))
        {
            Debug.LogError("Could not find StreamingAssets/gcp_credentials.json. Please create a Google service account key for a Google Cloud Platform project with the Speech-to-Text API enabled, then download that key as a JSON file and save it as StreamingAssets/gcp_credentials.json in this project. For more info on creating a service account key, see Google's documentation: https://cloud.google.com/speech-to-text/docs/quickstart-client-libraries#before-you-begin");
            return;
        }

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

        // Instantiates a client
        TextToSpeechClient textToSpeechClient = TextToSpeechClient.Create();

        // Set the text input to be synthesized
        SynthesisInput input = new SynthesisInput()
        {
            Text = "Hello, World!"
        };

        // Build the voice request, select the language code ("en-US") and the ssml voice gender
        // ("neutral")
        VoiceSelectionParams voice = new VoiceSelectionParams()
        {
            LanguageCode = "en-US",
            SsmlGender = SsmlVoiceGender.Neutral
        };

        // Select the type of audio file you want returned
        AudioConfig audioConfig = new AudioConfig()
        {
            AudioEncoding = AudioEncoding.Mp3
        };

        // Perform the text-to-speech request on the text input with the selected voice parameters and
        // audio file type
        SynthesizeSpeechResponse response =
            textToSpeechClient.SynthesizeSpeech(input, voice, audioConfig);

        // Get the audio contents from the response
        ByteString audioContents = response.AudioContent;

        // Write the response to the output file.
        File.WriteAllBytes(Path.Combine(Application.streamingAssetsPath, "output.mp3"), audioContents.ToByteArray());
    }
}
