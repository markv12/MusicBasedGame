using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class microphoneinput : MonoBehaviour {
	private const int conversionSampleRate = 22050;

	private float sensitivity = 100.0f;
	private float frequency = 0.0f;
	private int samplerate = 11024;
	private int frequencyBands = 8192;
	private float indexToHertz;

	private const int windowSize = 12;

	private Queue<float>window;

	public SpriteRenderer indicator;

	void Start() {
		audio.clip = Microphone.Start(null, true, 10, samplerate);
		audio.loop = true; // Set the AudioClip to loop
		audio.mute = true; // Mute the sound, we don't want the player to hear it
		while (!(Microphone.GetPosition("") > 0)){} // Wait until the recording has started
		audio.Play(); // Play the audio source!

		indexToHertz = ((float)conversionSampleRate) / ((float)frequencyBands);
		window = new Queue<float> ();

	}
	
	void Update(){
		//frequency = GetFundamentalFrequency();
		float[] results = GetMainFrequencies (40f, 1100f);

		Array.Sort (results);


		if(results.Length >0){
			Debug.Log(results[0]);
			setIndicatorColor (results[0]);
		}
		else{
			indicator.color = Color.white;
		}
	}

	private void setIndicatorColor(float frequency){
		window.Enqueue (frequency);
		if(window.Count> windowSize){
			window.Dequeue();
		}
		float sum = 0;

		foreach(float freq in window){
			sum+=freq;
		}
		float freqAverage = sum / window.Count;


		float period = 0.02f;

		float red   = (float)Math.Sin(period*freqAverage + 0);
		float green = (float)Math.Sin(period*freqAverage + 2);
		float blue  = (float)Math.Sin(period*freqAverage + 4);
		indicator.color = new Color (red, green, blue);			
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

	float[] GetMainFrequencies(float low, float high){
		int maxIndex = (int)(high / indexToHertz);
		int minIndex = (int)(low / indexToHertz);
		float[] data = new float[frequencyBands];
		audio.GetSpectrumData(data,0,FFTWindow.BlackmanHarris);
		List<KeyValuePair<int,float>> candidates = new List<KeyValuePair<int,float>> ();
		for (int j = minIndex; j < maxIndex; j++)
		{
			if (data[j] >  0.0006f)
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