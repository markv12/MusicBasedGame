using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class microphoneinput : MonoBehaviour {
	private const int conversionSampleRate = 22050;

	public float sensitivity = 100.0f;
	public float loudness = 0.0f;
	public float frequency = 0.0f;
	public int samplerate = 11024;
	private int frequencyBands = 8192;
	private float indexToHertz;

	private int[] positions;


	void Start() {
		audio.clip = Microphone.Start(null, true, 10, samplerate);
		audio.loop = true; // Set the AudioClip to loop
		audio.mute = true; // Mute the sound, we don't want the player to hear it
		while (!(Microphone.GetPosition("") > 0)){} // Wait until the recording has started
		audio.Play(); // Play the audio source!

		indexToHertz = ((float)conversionSampleRate) / ((float)frequencyBands);
	}
	
	void Update(){
		loudness = GetAveragedVolume() * sensitivity;
		//frequency = GetFundamentalFrequency();
		float[] results = GetMainFrequencies (2000f);

		foreach(float i in results){
			Debug.Log(i);
		}
	}
	
	private float GetAveragedVolume()
	{
		float[] data = new float[256];
		float a = 0;
		audio.GetOutputData(data,0);
		foreach(float s in data)
		{
			a += Mathf.Abs(s);
		}
		return a/256;
	}
	
	private float GetFundamentalFrequency(){
		float fundamentalFrequency = 0.0f;
		float[] data = new float[frequencyBands];
		audio.GetSpectrumData(data,0,FFTWindow.BlackmanHarris);
		float s = 0.0f;
		int i = 0;
		for (int j = 1; j < frequencyBands/2; j++)
		{
			if ( s < data[j] )
			{
				s = data[j];
				i = j;
			}
		}
		fundamentalFrequency = i * conversionSampleRate / frequencyBands;

		return fundamentalFrequency;
	}

	float[] GetMainFrequencies(float frequencyCutoff){
		int maxIndex = (int)(frequencyCutoff / indexToHertz);

		float[] data = new float[frequencyBands];
		audio.GetSpectrumData(data,0,FFTWindow.BlackmanHarris);
		List<KeyValuePair<int,float>> candidates = new List<KeyValuePair<int,float>> ();
		for (int j = 1; j < maxIndex; j++)
		{
			if (data[j] >  0.02f)
			{
				candidates.Add(new KeyValuePair<int,float>(j, data[j]));
			}
		}

		candidates.Sort((firstPair,nextPair) =>
		    {
				return firstPair.Value.CompareTo(nextPair.Value);
			}
		);
		float[] results = new float[candidates.Count];
		for(int i = 0; i<candidates.Count; i++){
			results[i] = ((float)candidates[i].Key) * indexToHertz;
		}
		return results;
	}


	static int[] Clone(int[] array)
	{
		int[] result = new int[array.Length];
		Buffer.BlockCopy(array, 0, result, 0, array.Length * sizeof(int));
		return result;
	}

}