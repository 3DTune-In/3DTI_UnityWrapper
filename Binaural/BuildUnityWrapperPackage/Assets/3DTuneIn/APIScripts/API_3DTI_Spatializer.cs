﻿using API_3DTI_Common;
using System;
using System.Collections.Generic;
using System.IO;            // Needed for FileStream
using System.Runtime.InteropServices;
using UnityEngine.Audio;

/**
*** API for 3D-Tune-In Toolkit Unity Wrapper ***
*
* version beta 1.0
* Created on: July 2016
* 
* Author: 3DI-DIANA Research Group / University of Malaga / Spain
* Contact: areyes@uma.es
* 
* Project: 3DTI (3D-games for TUNing and lEarnINg about hearing aids)
* Module: 3DTI Toolkit Unity Wrapper
**/



using UnityEngine;
using System.Linq;



[System.AttributeUsage(System.AttributeTargets.Field)]
public class SpatializerParameterAttribute : System.Attribute
{
    public Type type = typeof(float);
    // Label used in GUI
    public string label;
	// Tooltip for GUI
	public string description;
    // For numeric values, the units label, e.g. "dB"
    public string units;
    // For int/float parameters: limit to these discrete values. Leave as null for no limits.
    public float[] validValues;
	public float min;
	public float max;
	public float defaultValue;
	// If true then this parameter may be set individually on a specific source
	public bool isSourceParameter = false;
}


public enum TSampleRateEnum
{
    K44, K48, K96
}


public enum SpatializerBinaryRole
{
    // These values must match the C++ values
    HighPerformanceILD,
    HighQualityHRTF,
    HighQualityILD,
    ReverbBRIR,
}


public class API_3DTI_Spatializer : MonoBehaviour
{

	// Set this to the 3DTI mixer containing the SpatializerCore3DTI effect.
	public AudioMixer spatializereCoreMixer;

    public enum SpatializationMode : int
    {
        SPATIALIZATION_MODE_NONE = 0,
        SPATIALIZATION_MODE_HIGH_PERFORMANCE = 1,
        SPATIALIZATION_MODE_HIGH_QUALITY = 2,
    }

    public enum ReverbOrder : int
    {
        Adimensional,
        Bidimensional,
        ThreeDimensional,
    }


    // Note: The numbering of these parameters must be kept in sync with the C++ plugin source code. Per-source parameters must appear first for compatibility with the plugin.
    // The int value of these enums may change in future versions. For compatibility, always use the enum value name rather than the int value (i.e. use SptaializerParameter.PARAM_HRTF_INTERPOLATION instead of 0).
    public enum SpatializerParameter
	{
		[SpatializerParameter(/*pluginName="HRTFInterp",*/ label = "Enable HRTF interpolation", description = "Enable runtime interpolation of HRIRs, to allow for smoother transitions when moving listener and/or sources", min = 0, max = 1, type = typeof(bool), defaultValue = 1.0f, isSourceParameter = true)]
		EnableHRTFInterpolation = 0,

		[SpatializerParameter(/*pluginName = "MODfarLPF",*/ label = "Enable far distance effect", description = "Enable low pass filter to simulate sound coming from far distances", min = 0, max = 1, type = typeof(bool), defaultValue = 1.0f, isSourceParameter = true)]
		EnableFarDistanceEffect = 1,

		[SpatializerParameter(/*pluginName = "MODDistAtt",*/ label = "Enable anechoic distance attenuation", description = "Enable attenuation of sound depending on distance to listener for anechoic processing", min = 0, max = 1, type = typeof(bool), defaultValue = 1.0f, isSourceParameter = true)]
		EnableDistanceAttenuationAnechoic = 2,

		[SpatializerParameter(/*pluginName = "MODNFILD",*/ label = "Enable near distance ILD (High quality mode only)", description = "Enable near field filter for sources very close to the listener. Only available in high quality mode. Depends on the High Quality ILD binary being loaded.", min = 0, max = 1, type = typeof(bool), defaultValue = 1.0f, isSourceParameter = true)]
		EnableNearFieldILD = 3,

		[SpatializerParameter(/*pluginName = "SpatMode",*/ label = "Spatialization mode", description = "Set spatialization mode (0=High quality, 1=High performance, 2=None). Note, High quality depends on the HRTF binary being loaded and High Performance depends on the High Performance ILD binary being loaded.", min = 0, max = 2, type = typeof(API_3DTI_Spatializer.SpatializationMode), defaultValue = 0.0f, isSourceParameter = true)]
		SpatializationMode = 4,

        [SpatializerParameter(label = "Enable reverb processing", description = "Enable reverb processing", min = 0.0f, max = 1.0f, type = typeof(bool), defaultValue = 0, isSourceParameter = true)]
        EnableReverb = 5,

        [SpatializerParameter(label = "Enable reverb distance attenuation", description = "Enable attenuation of sound depending on distance to listener for reverb processing", min = 0.0f, max = 1.0f, type = typeof(bool), defaultValue = 0, isSourceParameter = true)]
        EnableDistanceAttenuationReverb = 6,

        [SpatializerParameter(label = "Head radius", description = "Set listener head radius", units = "m", min = 0.0f, max = 1e20f, defaultValue = 0.0875f)]
		HeadRadius = 7,

		// TODO: Add remaining default values
		[SpatializerParameter(label = "Scale factor", description = "Set the proportion between metres and Unity scale units", min = 1e-20f, max = 1e20f, defaultValue = 1.0f)]
		ScaleFactor = 8,

		[SpatializerParameter(label = "Enable custom ITD", description = "Enable Interaural Time Difference customization", type = typeof(bool), defaultValue = 0.0f)]
		EnableCustomITD = 9,

		[SpatializerParameter(label = "Anechoic distance attenuation", description = "Set attenuation in dB for each double distance", min = -30.0f, max = 0.0f, units = "dB", defaultValue = -1.0f)]
		AnechoicDistanceAttenuation = 10,

        [SpatializerParameter(label = "ILD Attenuation", description = "Attenuation in dB applied before ILD (interaural level difference) processing", min = 0.0f, max = 30.0f, units = "dB", defaultValue = -6.0f)]
        ILDAttenuation = 11,

        [SpatializerParameter(label = "Sound speed", description = "Set sound speed, used for custom ITD computation", units = "m/s", min = 10.0f, max = 1000.0f, defaultValue = 343.0f)]
		SoundSpeed = 12,

		[SpatializerParameter(label = "Anechoic directionality attenuation for left ear", description = "Set directionality extend for left ear. The value is the attenuation in decibels applied to sources placed behind the listener", units = "dB", min = 0.0f, max = 30.0f, defaultValue = 15.0f)]
		HearingAidDirectionalityAttenuationLeft = 13,

		[SpatializerParameter(label = "Anechoic directionality attenuation for right ear", description = "Set directionality extend for right ear. The value is the attenuation in decibels applied to sources placed behind the listener", units = "dB", min = 0.0f, max = 30.0f, defaultValue = 15.0f)]
		HearingAidDirectionalityAttenuationRight = 14,

		[SpatializerParameter(label = "Enable directionality simulation for left ear", type = typeof(bool), defaultValue = 0.0f)]
		EnableHearingAidDirectionalityLeft = 15,

		[SpatializerParameter(label = "Enable directionality simulation for right ear", type = typeof(bool), defaultValue = 0.0f)]
		EnableHearingAidDirectionalityRight = 16,

		[SpatializerParameter(label = "Enable limiter", description = "Enable dynamics limiter after spatialization, to avoid potential saturation", type = typeof(bool), defaultValue = 1.0f)]
		EnableLimiter = 17,

        [SpatializerParameter(label = "HRTF resampling step (High Quality only)", description = "HRTF resampling step; Lower values give better quality at the cost of more memory usage. Only affects High Quality mode.", min = 1, max = 90, type = typeof(int), defaultValue = 15)]
		HRTFResamplingStep = 18,

        [SpatializerParameter(label = "Reverb order", description = "Configures the number of channels of the first-order ambisonic reverb processing. The options are: W, X, Y and Z (ThreeDimensional); W, X and Y (Bidimensional); only W (Adimensional)", min = 0.0f, max = 2.0f, type = typeof(API_3DTI_Spatializer.ReverbOrder), defaultValue = (float)API_3DTI_Spatializer.ReverbOrder.Bidimensional, isSourceParameter = true)]
        ReverbOrder = 19,
    };
    public const int NumSourceParameters = (int)SpatializerParameter.EnableDistanceAttenuationReverb + 1;
	public const int NumParameters = 20;

	// Store the parameter values here for Unity to serialize. We initialize them to their default values. This is private and clients should use the accessor/getter methods below which will ensure the plugin is kept in sync with these values.
	// NB, per-source parameters may be set on individual sources but they are also set on the core which defines their initial value.
	[SerializeField]
	private float[] spatializerParameters = Enumerable.Range(0, NumParameters).Select(i => ((SpatializerParameter)i).GetAttribute<SpatializerParameterAttribute>().defaultValue).ToArray<float>();

    /// Array is for the three different sample rates
    [SerializeField]
    private string[] highQualityHRTFPaths =
	{
		"Assets/3DTuneIn/Resources/Data/HighQuality/HRTF/3DTI_HRTF_IRC1032_256s_44100Hz.3dti-hrtf.bytes",
		"Assets/3DTuneIn/Resources/Data/HighQuality/HRTF/3DTI_HRTF_IRC1032_256s_48000Hz.3dti-hrtf.bytes",
		"Assets/3DTuneIn/Resources/Data/HighQuality/HRTF/3DTI_HRTF_IRC1032_256s_96000Hz.3dti-hrtf.bytes",
	};
    [SerializeField]
    private string[] highQualityILDPaths = {
		"Assets/3DTuneIn/Resources/Data/HighQuality/ILD/NearFieldCompensation_ILD_44100.3dti-ild.bytes",
		"Assets/3DTuneIn/Resources/Data/HighQuality/ILD/NearFieldCompensation_ILD_48000.3dti-ild.bytes",
		"Assets/3DTuneIn/Resources/Data/HighQuality/ILD/NearFieldCompensation_ILD_96000.3dti-ild.bytes",
	};
    [SerializeField]
    private string[] highPerformanceILDPaths = {
        "Assets/3DTuneIn/Resources/Data/HighPerformance/ILD/HRTF_ILD_44100.3dti-ild.bytes",
        "Assets/3DTuneIn/Resources/Data/HighPerformance/ILD/HRTF_ILD_48000.3dti-ild.bytes",
        "Assets/3DTuneIn/Resources/Data/HighPerformance/ILD/HRTF_ILD_96000.3dti-ild.bytes",
    };
    [SerializeField]
    private string[] reverbBRIRPaths = {
		"Assets/3DTuneIn/Resources/Data/Reverb/BRIR/3DTI_BRIR_large_44100.3dti-brir.bytes",
		"Assets/3DTuneIn/Resources/Data/Reverb/BRIR/3DTI_BRIR_large_48000.3dti-brir.bytes",
		"Assets/3DTuneIn/Resources/Data/Reverb/BRIR/3DTI_BRIR_large_96000.3dti-brir.bytes",
	};


    // for convenience
    private string[] binaryPaths(SpatializerBinaryRole role)
    {
        switch (role)
        {
            case SpatializerBinaryRole.HighPerformanceILD: return highPerformanceILDPaths;
            case SpatializerBinaryRole.HighQualityHRTF: return highQualityHRTFPaths;
            case SpatializerBinaryRole.HighQualityILD: return highQualityILDPaths;
            case SpatializerBinaryRole.ReverbBRIR: return reverbBRIRPaths;
            default: throw new Exception("Invalid value of enum SpatializerBinaryRole");
        }
    }
    

#if UNITY_IPHONE
    [DllImport ("__Internal")]
#else
    [DllImport("AudioPlugin3DTIToolkit")]
#endif
    private static extern bool Load3DTISpatializerBinary(int role, string path);


#if UNITY_IPHONE
    [DllImport ("__Internal")]
#else
    [DllImport("AudioPlugin3DTIToolkit")]
#endif
	private static extern bool Set3DTISpatializerFloat(int parameterID, float value);


#if UNITY_IPHONE
    [DllImport ("__Internal")]
#else
    [DllImport("AudioPlugin3DTIToolkit")]
#endif
	private static extern bool Get3DTISpatializerFloat(int parameterID, out float value);


	/// Test if a Spatializer instance has been created. This can only be done by adding the SpatializerCore 
	/// effect to a mixer. Currently only one instance is supported
#if UNITY_IPHONE
    [DllImport ("__Internal")]
#else
	[DllImport("AudioPlugin3DTIToolkit")]
#endif
	private static extern bool Is3DTISpatializerCreated();


    private void Awake()
    {
        // Check a missized array hasn't been saved from a previous version
        if (spatializerParameters.Length != NumParameters)
        {
            int originalLength = spatializerParameters.Length;
            Array.Resize(ref spatializerParameters, NumParameters);
            if (spatializerParameters.Length > originalLength)
            {
                for (int i = originalLength; i<spatializerParameters.Length; i++)
                {
                    spatializerParameters[i] = ((SpatializerParameter)i).GetAttribute<SpatializerParameterAttribute>().defaultValue;
                }
            }
        }
    }

    /// <summary>

    /// Automatic setup of Toolkit Core (as read from custom GUI in Unity Inspector)
    /// </summary>
    void Start()
    {
        if (!Is3DTISpatializerCreated())
        {
            Debug.LogError("Cannot start 3DTI Spatializer as no instance has been created. Please ensure the SpatializerCore plugin has been added to a mixer in the scene.");
            return;
        }

        for (int i = 0; i < NumParameters; i++)
        {
            if (!Set3DTISpatializerFloat(i, spatializerParameters[i]))
            {
                Debug.LogError($"Failed to set 3DTI parameter {i}.", this);
            }
        }


        if (!GetSampleRate(out TSampleRateEnum sr))
        {
            Debug.LogError($"Unsupported sample rate for 3DTI Spatializer {AudioSettings.outputSampleRate}. Supported values are 44100, 48000 and 96000.");
        }
        else
        {
            Debug.Assert(0 <= (int)sr && (int)sr < 3);
            foreach (SpatializerBinaryRole role in Enum.GetValues(typeof(SpatializerBinaryRole)))
            {
                string resourcePath = binaryPaths(role)[(int)sr];
                SetBinaryPath(role, sr, resourcePath);

            }
        }
    }

    // --- Spatializer Core parameters

    public bool SetFloatParameter(SpatializerParameter parameter, float value, AudioSource source = null)
    {
		if (source != null)
		{
			if (!parameter.GetAttribute<SpatializerParameterAttribute>().isSourceParameter)
            {
				Debug.LogError($"Cannot set spatialization parameter {parameter} on a single AudioSource. Call this method with source==null to set this parameter on the Spatializer Core.", this);
				return false;
            }
			else
			{
				if (!source.SetSpatializerFloat((int)parameter, value))
				{
                    Debug.LogError($"Failed to set spatialization parameter {parameter} for AudioSource {source} on 3DTI Spatializer plugin.", this);
                    return false;
                }
				if (!source.GetSpatializerFloat((int)parameter, out float finalValue))
                {
                    Debug.LogError($"Failed to retrieve value of parameter {parameter} for AudioSource {source} from 3DTI Spatializer plugin after setting it.", this);
					return false;
                }
				else if (finalValue != value)
                {
					Debug.LogWarning($"Value for parameter {parameter} on source {source} was requested to be set to {value} but 3DTI Spatializer plugin corrected this value to {finalValue}");
                }
            }

		}
		else
		{
			if (!Set3DTISpatializerFloat((int)parameter, value))
			{
				Debug.LogError($"Failed to set parameter {parameter} on 3DTI Spatializer plugin.", this);
				return false;
			}
			if (!Get3DTISpatializerFloat((int)parameter, out spatializerParameters[(int)parameter]))
			{
				Debug.LogError($"Failed to retrieve value of parameter {parameter} from 3DTI Spatializer plugin after setting it.", this);
				return false;
			}
		}
		return true;
    }

	public bool GetFloatParameter(SpatializerParameter parameter, out float value, AudioSource source=null)
    {
		if (source != null)
        {
            if (!parameter.GetAttribute<SpatializerParameterAttribute>().isSourceParameter)
            {
                Debug.LogError($"Cannot get spatialization parameter {parameter} for a single AudioSource as it is not a per-source parameter. Call this method with source==null to retrieve this parameter's value on the Spatializer Core.", this);
				value = parameter.GetAttribute<SpatializerParameterAttribute>().defaultValue;
                return false;
            }
            else
            {
                if (!source.GetSpatializerFloat((int)parameter, out value))
                {
                    Debug.LogError($"Failed to retrieve value of parameter {parameter} for AudioSource {source} from 3DTI Spatializer plugin.", this);
                    return false;
                }
            }
        }
		if (!Get3DTISpatializerFloat((int)parameter, out value))
        {
            Debug.LogError($"Failed to retrieve parameter {parameter} from 3DTI Spatializer plugin.", this);
			return false;
        }
		return true;
    }

    // Throws exception on failure
    public float GetFloatParameter(SpatializerParameter parameter, AudioSource source=null)
    {
        if (!GetFloatParameter(parameter, out float value, source))
        {
			throw new Exception($"Failed to retrieve parameter {parameter} from 3DTI Spatializer plugin.");
        }
        return value;
    }

	public T GetParameter<T>(SpatializerParameter parameter, AudioSource source = null)
    {
		SpatializerParameterAttribute attributes = parameter.GetAttribute<SpatializerParameterAttribute>();
		Debug.Assert(typeof(T) == attributes.type);
		float f = GetFloatParameter(parameter, source);
		if (typeof(T).IsEnum)
        {
			return (T) Enum.ToObject(typeof(SpatializationMode), (Int32)f);
            //return (T)Convert.ChangeType((int)f, typeof(T));
        }
		else
        {
            return (T)Convert.ChangeType(f, typeof(T));
        }
    }

	public bool SetParameter<T>(SpatializerParameter parameter, T value, AudioSource source = null)
    {
        SpatializerParameterAttribute attributes = parameter.GetAttribute<SpatializerParameterAttribute>();
        Debug.Assert(typeof(T) == attributes.type);
		return SetFloatParameter(parameter, Convert.ToSingle(value), source);
    }


    public string GetBinaryPath(SpatializerBinaryRole role, TSampleRateEnum sampleRate)
    {
        Debug.Assert(Enum.IsDefined(typeof(SpatializerBinaryRole), role));
        Debug.Assert(Enum.IsDefined(typeof(TSampleRateEnum), sampleRate));

        return binaryPaths(role)[(int)sampleRate];
    }

    /// <summary>
    /// Sets the path of a binary file for the spatializer. If the path is not an empty string, and the target sample rate matches the current sample rate, then it requests the plugin to load the file.
    /// </summary>
    /// <param name="role"></param>
    /// <param name="sampleRate">Sample rate the binary is intended to be used at.</param>
    /// <param name="path">Leave empty to load no binary for this role at this sample rate.</param>
    /// <returns>True if the file successfully loaded</returns>
    public bool SetBinaryPath(SpatializerBinaryRole role, TSampleRateEnum sampleRate, string path)
    {
        Debug.Assert(Enum.IsDefined(typeof(SpatializerBinaryRole), role));
        Debug.Assert(Enum.IsDefined(typeof(TSampleRateEnum), sampleRate));
        binaryPaths(role)[(int)sampleRate] = path;
        if (path.Length > 0 && GetSampleRate(out TSampleRateEnum currentSampleRate) && currentSampleRate == sampleRate)
        {
            // TODO: This extra save is for the sake of android which can't read the normal filesystem. But it might be possible to send the data directly as an array.
            if (!(SaveResourceAsBinary(path, out string newPath) && Load3DTISpatializerBinary((int)role, newPath)))
            {
                Debug.LogError($"Failed to load Spatializer binary for {role} at sample rate {sampleRate}.");
                return false;
            }
        }
        return true;
    }


	/// <summary>
	/// Load one file from resources and save it as a binary file (for Android)
	/// </summary>    
	private bool SaveResourceAsBinary(string originalName, out string newFilename)
	{
        // remove .bytes extension
        if (originalName.EndsWith(".bytes"))
        {
			originalName = originalName.Substring(0, originalName.Length - ".bytes".Length);
        }

        // Setup name for new file
        newFilename = Application.persistentDataPath + "/" + originalName;

		// Load as asset from resources 
		TextAsset txtAsset = Resources.Load(originalName) as TextAsset;
		if (txtAsset == null)
		{
			Debug.LogError($"Could not load 3DTI resource {originalName}", this);
			return false;  // Could not load asset from resources
		}

        // Transform asset into stream and then into byte array
        MemoryStream streamData = new MemoryStream(txtAsset.bytes);
		byte[] dataArray = streamData.ToArray();


		// Write binary data to binary file        
		Directory.CreateDirectory(Path.GetDirectoryName(newFilename));
		using (BinaryWriter writer = new BinaryWriter(File.Open(newFilename, FileMode.Create)))
		{
			writer.Write(dataArray);
		}

		return true;
	}

 

    public bool GetSampleRate(out TSampleRateEnum sampleRate)
    {

        switch (AudioSettings.outputSampleRate)
        {
            case 44100:
                sampleRate =  TSampleRateEnum.K44;
                return true;
            case 48000:
                sampleRate = TSampleRateEnum.K48;
                return true; 
            case 96000:
                sampleRate = TSampleRateEnum.K96;
                return true;
            default:
                Debug.LogError("Sampling rates different than 44.1, 48 or 96kHz are not supported." + Environment.NewLine + "Go to Edit -> Project Settings -> Audio and set System Sample Rate to a valid value.");
                Debug.Break();
                Debug.developerConsoleVisible = true;
#if (UNITY_EDITOR)
                UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                sampleRate = TSampleRateEnum.K44;
                return false;
        }
    }


}
