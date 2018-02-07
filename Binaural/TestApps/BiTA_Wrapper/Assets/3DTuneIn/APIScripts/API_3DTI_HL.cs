﻿/**
*** API for 3D-Tune-In Toolkit HL Simulation Unity Wrapper ***
*
* version beta 1.0
* Created on: November 2016
* 
* Author: 3DI-DIANA Research Group / University of Malaga / Spain
* Contact: areyes@uma.es
* 
* Project: 3DTI (3D-games for TUNing and lEarnINg about hearing aids)
* Module: 3DTI Toolkit Unity Wrapper
**/

using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;   // For ReadOnlyCollection
using API_3DTI_Common;

public class API_3DTI_HL : MonoBehaviour
{
    // Global variables
    public AudioMixer hlMixer;  // Drag&drop here the HAHL_3DTI_Mixer

    // Public constant and type definitions 
    public static readonly ReadOnlyCollection<float> AUDIOMETRY_PRESET_MILD = new ReadOnlyCollection<float>(new[] { 7f, 7f, 12f, 15f, 22f, 25f, 25f, 25f, 25f });
    public static readonly ReadOnlyCollection<float> AUDIOMETRY_PRESET_MODERATE = new ReadOnlyCollection<float>(new[] { 22f, 22f, 27f, 30f, 37f, 40f, 40f, 40f, 40f });
    public static readonly ReadOnlyCollection<float> AUDIOMETRY_PRESET_SEVERE = new ReadOnlyCollection<float>(new[] { 47f, 47f, 52f, 55f, 62f, 65f, 65f, 65f, 65f });
    public static readonly ReadOnlyCollection<float> AUDIOMETRY_PRESET_NORMAL = new ReadOnlyCollection<float>(new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f });        
    public enum T_HLBand { HZ_62 =0, HZ_125 =1, HZ_250 =2, HZ_500 = 3, HZ_1K = 4, HZ_2K = 5, HZ_4K = 6, HZ_8K =7, HZ_16K =8 };
    public const int NUM_HL_BANDS = 9;
    public enum T_HLTemporalDistortionBandUpperLimit { HZ_UL_200 =0, HZ_UL_400 =1, HZ_UL_800 =2, HZ_UL_1600 =3, HZ_UL_3200 =4, HZ_UL_6400 =5, HZ_UL_WRONG =-1 };

    // Internal constants
    const float DEFAULT_CALIBRATION = 100.0f;
    const float DEFAULT_ATTACK = 20.0f;
    const float DEFAULT_RELEASE = 100.0f;
    const float DEFAULT_TA_BAND = 1600.0f;
    const float DEFAULT_TA_POWER = 0.0f;
    const float DEFAULT_TA_CUTOFF = 500.0f;
    const int DEFAULT_FS_SIZE = 1;
    const float DEFAULT_FS_HZ = 0.0f;

    // Internal parameters for consistency with GUI
    [HideInInspector]
    public bool GLOBAL_LEFT_ON = false;                         // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool GLOBAL_RIGHT_ON = false;                        // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_CALIBRATION = DEFAULT_CALIBRATION;       // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float [] PARAM_AUDIOMETRY_LEFT = new float[9] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };  // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float [] PARAM_AUDIOMETRY_RIGHT = new float[9] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }; // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_LEFT_ATTACK = DEFAULT_ATTACK;            // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_LEFT_RELEASE = DEFAULT_RELEASE;          // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_RIGHT_ATTACK = DEFAULT_ATTACK;           // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_RIGHT_RELEASE = DEFAULT_RELEASE;         // For internal use, DO NOT USE IT DIRECTLY

    [HideInInspector]
    public bool MBE_LEFT_ON = true;                         // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool MBE_RIGHT_ON = true;                        // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool TA_LEFT_ON = false;                         // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool TA_RIGHT_ON = false;                        // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_LEFT_TA_BAND = DEFAULT_TA_BAND;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_RIGHT_TA_BAND = DEFAULT_TA_BAND;     // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_LEFT_TA_POWER = DEFAULT_TA_POWER;    // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_RIGHT_TA_POWER = DEFAULT_TA_POWER;   // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_LEFT_TA_CUTOFF = DEFAULT_TA_CUTOFF;  // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_RIGHT_TA_CUTOFF = DEFAULT_TA_CUTOFF; // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_TA_LRSYNC = 0.0f;                    // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool PARAM_TA_LRSYNC_ON = false;                 // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool PARAM_LEFT_TA_POSTLPF = true;                    // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool PARAM_RIGHT_TA_POSTLPF = true;                    // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool FS_LEFT_ON = false;                         // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool FS_RIGHT_ON = false;                        // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public int PARAM_LEFT_FS_DOWN_SIZE = DEFAULT_FS_SIZE;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public int PARAM_LEFT_FS_UP_SIZE = DEFAULT_FS_SIZE;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public int PARAM_RIGHT_FS_DOWN_SIZE = DEFAULT_FS_SIZE;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public int PARAM_RIGHT_FS_UP_SIZE = DEFAULT_FS_SIZE;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_LEFT_FS_DOWN_HZ = DEFAULT_FS_HZ;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_LEFT_FS_UP_HZ = DEFAULT_FS_HZ;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_RIGHT_FS_DOWN_HZ = DEFAULT_FS_HZ;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_RIGHT_FS_UP_HZ = DEFAULT_FS_HZ;      // For internal use, DO NOT USE IT DIRECTLY

    ///////////////////////////////////////
    // BASIC API
    ///////////////////////////////////////
    
    /// <summary>
    /// Set audiometry preset for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="preset ({AUDIOGRAM_PRESET_MILD, AUDIOGRAM_PRESET_MODERATE, AUDIOGRAM_PRESET_SEVERE, AUDIOGRAM_PRESET_NORMAL})"></param>
    /// <returns></returns>    
    public bool SetAudiometryPreset(T_ear ear, ReadOnlyCollection<float> presetAudiometry)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetAudiometryPreset(T_ear.LEFT, presetAudiometry)) return false;
            return SetAudiometryPreset(T_ear.RIGHT, presetAudiometry);
        }

        List<float> hearingLevels = new List<float>(presetAudiometry); 
        return SetAudiometry(ear, hearingLevels);
    }

    /// <summary>
    /// Enable hearing loss for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool EnableHearingLoss(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!EnableHearingLoss(T_ear.LEFT)) return false;
            return EnableHearingLoss(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            GLOBAL_LEFT_ON = true;
            paramName += "Process_LeftOn";
        }
        else
        {
            GLOBAL_RIGHT_ON = true;
            paramName += "Process_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(true));
    }

    /// <summary>
    /// Disable hearing loss for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool DisableHearingLoss(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!DisableHearingLoss(T_ear.LEFT)) return false;
            return DisableHearingLoss(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            GLOBAL_LEFT_ON = false;
            paramName += "Process_LeftOn";
        }
        else
        {
            GLOBAL_RIGHT_ON = false;
            paramName += "Process_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(false));
    }

    ///////////////////////////////////////
    // NON-LINEAR ATTENUATION (MULTIBAND EXPANDER)
    ///////////////////////////////////////

    /// <summary>
    /// Set all hearing loss levels (full audiometry) for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="hearingLevels (dBHL[])"></param>
    /// <returns></returns>
    public bool SetAudiometry(T_ear ear, List<float> hearingLevels)
    {
        for (int b=0; b < hearingLevels.Count; b++)
        {
            if (!SetHearingLevel(ear, b, hearingLevels[b])) return false;
        }

        return true;
    }

    /// <summary>
    /// Set hearing loss level for one band in one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="band"></param>
    /// <param name="hearingLevel (dBHL)"></param>
    /// <returns></returns>
    public bool SetHearingLevel(T_ear ear, int band, float hearingLevel)
    {
        // Check size
        if ((band < 0) || (band > NUM_HL_BANDS))
            return false;

        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetHearingLevel(T_ear.LEFT, band, hearingLevel)) return false;
            return SetHearingLevel(T_ear.RIGHT, band, hearingLevel);
        }
        
        // Set internal variables and build parameter string
        string paramName = "HL3DTI_HL_Band_" + band.ToString() + "_";
        if (ear == T_ear.LEFT)
        {
            PARAM_AUDIOMETRY_LEFT[band] = hearingLevel;
            paramName += "Left";
        }
        else
        {
            PARAM_AUDIOMETRY_RIGHT[band] = hearingLevel;
            paramName += "Right";
        }

        // Send command
        return hlMixer.SetFloat(paramName, hearingLevel);
    }

    /// <summary>
    /// Set attack of all bands envelope detectors for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="attack (ms)"></param>
    /// <returns></returns>
    public bool SetAttackForAllBands(T_ear ear, float attack)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetAttackForAllBands(T_ear.LEFT, attack)) return false;
            return SetAttackForAllBands(T_ear.RIGHT, attack);
        }
        if (ear == T_ear.LEFT)
        {
            if (!hlMixer.SetFloat("HL3DTI_Attack_Left", attack)) return false;
            PARAM_LEFT_ATTACK = attack;
        }
        if (ear == T_ear.RIGHT)
        {
            if (!hlMixer.SetFloat("HL3DTI_Attack_Right", attack)) return false;
            PARAM_RIGHT_ATTACK = attack;
        }
        return true;
    }

    /// <summary>
    /// Set release of all bands envelope detectors for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="release (ms)"></param>
    /// <returns></returns>
    public bool SetReleaseForAllBands(T_ear ear, float release)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetReleaseForAllBands(T_ear.LEFT, release)) return false;
            return SetReleaseForAllBands(T_ear.RIGHT, release);
        }

        if (ear == T_ear.LEFT)
        {
            if (!hlMixer.SetFloat("HL3DTI_Release_Left", release)) return false;
            PARAM_LEFT_RELEASE = release;
        }
        if (ear == T_ear.RIGHT)
        {
            if (!hlMixer.SetFloat("HL3DTI_Release_Right", release)) return false;
            PARAM_RIGHT_RELEASE = release;
        }
        return true;
    }

    /// <summary>
    /// Set calibration to allow conversion between dBSPL and dBHL to dBFS (internally used by the hearing loss simulator)
    /// </summary>
    /// <param name="dBSPL_for_0dBFS (how many dBSPL are measured with 0dBFS)"></param>
    /// <returns></returns>
    public bool SetCalibration(float dBSPL_for_0dBFS)
    {
        PARAM_CALIBRATION = dBSPL_for_0dBFS;
        return hlMixer.SetFloat("HL3DTI_Calibration", dBSPL_for_0dBFS);
    }

    /// <summary>
    /// Enable non-linear attenuation (multiband expander) for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool EnableNonLinearAttenuation(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!EnableNonLinearAttenuation(T_ear.LEFT)) return false;
            return EnableNonLinearAttenuation(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            MBE_LEFT_ON = true;
            paramName += "MBE_LeftOn";
        }
        else
        {
            MBE_RIGHT_ON = true;
            paramName += "MBE_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(true));
    }

    /// <summary>
    /// Disable non-linear attenuation (multiband expander) for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool DisableNonLinearAttenuation(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!DisableNonLinearAttenuation(T_ear.LEFT)) return false;
            return DisableNonLinearAttenuation(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            MBE_LEFT_ON = false;
            paramName += "MBE_LeftOn";
        }
        else
        {
            MBE_RIGHT_ON = false;
            paramName += "MBE_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(false));
    }

    ///////////////////////////////////////
    // TEMPORAL DISTORTION (ASYNCHRONY) SIMULATION
    ///////////////////////////////////////

    /// <summary>
    /// Enable Temporal distortion simulation for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool EnableTemporalDistortionSimulation(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!EnableTemporalDistortionSimulation(T_ear.LEFT)) return false;
            return EnableTemporalDistortionSimulation(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            TA_LEFT_ON = true;
            paramName += "TA_LeftOn";
        }
        else
        {
            TA_RIGHT_ON = true;
            paramName += "TA_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(true));
    }

    /// <summary>
    /// Disable Temporal distortion simulation for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool DisableTemporalDistortionSimulation(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!DisableTemporalDistortionSimulation(T_ear.LEFT)) return false;
            return DisableTemporalDistortionSimulation(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            TA_LEFT_ON = false;
            paramName += "TA_LeftOn";
        }
        else
        {
            TA_RIGHT_ON = false;
            paramName += "TA_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(false));
    }

    /// <summary>
    /// Disable left-right synchronicity in Temporal distortion simulation
    /// </summary>    
    /// <returns></returns>
    public bool EnableTemporalDistortionLeftRightSynchronicity()
    {
        PARAM_TA_LRSYNC_ON = true;
        return hlMixer.SetFloat("HL3DTI_TA_LRSync_On", CommonFunctions.Bool2Float(true));
    }

    /// <summary>
    /// Disable left-right synchronicity in Temporal distortion simulation
    /// </summary>    
    /// <returns></returns>
    public bool DisableTemporalDistortionLeftRightSynchronicity()
    {
        PARAM_TA_LRSYNC_ON = false;
        return hlMixer.SetFloat("HL3DTI_TA_LRSync_On", CommonFunctions.Bool2Float(false));
    }
    
    /// <summary>
    /// Set left-right synchronicity amount (if left-right synchronicity was previously enabled)
    /// </summary>
    /// <param name="leftRightSynchronicity"></param>
    /// <returns></returns>
    public bool SetTemporalDistortionLeftRightSynchronicity(float leftRightSynchronicity)
    {        
        if ((leftRightSynchronicity < 0.0f) || (leftRightSynchronicity > 1.0f))
            return false;

        PARAM_TA_LRSYNC = leftRightSynchronicity;
        return hlMixer.SetFloat("HL3DTI_TA_LRSync", leftRightSynchronicity);
    }

    /// <summary>
    /// Set band upper limit for one ear in Temporal distortion simulator
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="bandUpperLimit (Hz)"></param>
    /// <returns></returns>
    public bool SetTemporalDistortionBandUpperLimit(T_ear ear, T_HLTemporalDistortionBandUpperLimit bandUpperLimit)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetTemporalDistortionBandUpperLimit(T_ear.LEFT, bandUpperLimit)) return false;
            return SetTemporalDistortionBandUpperLimit(T_ear.RIGHT, bandUpperLimit);
        }

        // Get actual value of band upper limit in Hz
        float bandUpperLimitHz = FromBandUpperLimitEnumToFloat(bandUpperLimit);

        // And set
        if (bandUpperLimitHz != 0.0f)
        {
            if (ear == T_ear.LEFT)
            {
                PARAM_LEFT_TA_BAND = bandUpperLimitHz;
                return hlMixer.SetFloat("HL3DTI_TA_Band_Left", bandUpperLimitHz);
            }
            if (ear == T_ear.RIGHT)
            {
                PARAM_RIGHT_TA_BAND = bandUpperLimitHz;
                return hlMixer.SetFloat("HL3DTI_TA_Band_Right", bandUpperLimitHz);
            }
        }        
        return false;
    }

    /// <summary>
    /// Set white noise power for one ear in Temporal distortion simulator
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="whiteNoisePower (ms)"></param>
    /// <returns></returns>
    public bool SetTemporalDistortionWhiteNoisePower(T_ear ear, float whiteNoisePower)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetTemporalDistortionWhiteNoisePower(T_ear.LEFT, whiteNoisePower)) return false;
            return SetTemporalDistortionWhiteNoisePower(T_ear.RIGHT, whiteNoisePower);
        }

        if (ear == T_ear.LEFT)
        {
            PARAM_LEFT_TA_POWER = whiteNoisePower;
            return hlMixer.SetFloat("HL3DTI_TA_Noise_Power_Left", whiteNoisePower);
        }
        if (ear == T_ear.RIGHT)
        {
            PARAM_RIGHT_TA_POWER = whiteNoisePower;
            return hlMixer.SetFloat("HL3DTI_TA_Noise_Power_Right", whiteNoisePower);
        }
        return false;
    }

    /// <summary>
    /// Set autocorrelation filter cutoff frequency for one ear in Temporal distortion simulator
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="cutoff (Hz)"></param>
    /// <returns></returns>
    public bool SetTemporalDistortionAutocorrelationFilterCutoff(T_ear ear, float cutoff)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetTemporalDistortionAutocorrelationFilterCutoff(T_ear.LEFT, cutoff)) return false;
            return SetTemporalDistortionAutocorrelationFilterCutoff(T_ear.RIGHT, cutoff);
        }

        if (ear == T_ear.LEFT)
        {
            PARAM_LEFT_TA_CUTOFF = cutoff;
            return hlMixer.SetFloat("HL3DTI_TA_Noise_LPF_Left", cutoff);
        }
        if (ear == T_ear.RIGHT)
        {
            PARAM_RIGHT_TA_CUTOFF = cutoff;
            return hlMixer.SetFloat("HL3DTI_TA_Noise_LPF_Right", cutoff);
        }
        return false;
    }

    /// <summary>
    /// Get one float value of Hz from a T_HLTemporalDistortionBandUpperLimit enum value
    /// </summary>
    /// <param name="bandLimit"></param>
    /// <returns></returns>
    public float FromBandUpperLimitEnumToFloat(T_HLTemporalDistortionBandUpperLimit bandLimit)
    {        
        switch (bandLimit)
        {
            case T_HLTemporalDistortionBandUpperLimit.HZ_UL_200:
                return 200.0f;                
            case T_HLTemporalDistortionBandUpperLimit.HZ_UL_400:
                return 400.0f;                
            case T_HLTemporalDistortionBandUpperLimit.HZ_UL_800:
                return 800.0f;                
            case T_HLTemporalDistortionBandUpperLimit.HZ_UL_1600:
                return 1600.0f;              
            case T_HLTemporalDistortionBandUpperLimit.HZ_UL_3200:
                return 3200.0f;                
            case T_HLTemporalDistortionBandUpperLimit.HZ_UL_6400:
                return 6400.0f;                
            default:
                return 0.0f;
        }
    }

    /// <summary>
    /// Get one T_HLTemporalDistortionBandUpperLimit enum value from a float value in Hz
    /// </summary>
    /// <param name="bandLimitHz"></param>
    /// <returns></returns>
    public T_HLTemporalDistortionBandUpperLimit FromFloatToBandUpperLimitEnum(float bandLimitHz)
    {
        if (Mathf.Abs(bandLimitHz - 200.0f) < 0.01)
            return T_HLTemporalDistortionBandUpperLimit.HZ_UL_200;
        if (Mathf.Abs(bandLimitHz - 400.0f) < 0.01)
            return T_HLTemporalDistortionBandUpperLimit.HZ_UL_400;
        if (Mathf.Abs(bandLimitHz - 800.0f) < 0.01)
            return T_HLTemporalDistortionBandUpperLimit.HZ_UL_800;
        if (Mathf.Abs(bandLimitHz - 1600.0f) < 0.01)
            return T_HLTemporalDistortionBandUpperLimit.HZ_UL_1600;
        if (Mathf.Abs(bandLimitHz - 3200.0f) < 0.01)
            return T_HLTemporalDistortionBandUpperLimit.HZ_UL_3200;
        if (Mathf.Abs(bandLimitHz - 6400.0f) < 0.01)
            return T_HLTemporalDistortionBandUpperLimit.HZ_UL_6400;

        return T_HLTemporalDistortionBandUpperLimit.HZ_UL_WRONG;
    }

    ///// <summary>
    ///// Get the zero and one autocorrelation coefficients of the jitter noise source for Temporal distortion in one ear.
    ///// The coefficient one is normalized with respect to coefficient zero.
    ///// </summary>
    ///// <param name="ear"></param>
    ///// <param name="coef0"></param>
    ///// <param name="coef1"></param>
    ///// <returns></returns>
    //public bool GetAutocorrelationCoefficients(T_ear ear, out float coef0, out float coef1)
    //{
    //    coef0 = 0.0f;
    //    coef1 = 0.0f;        
    //    if (ear == T_ear.LEFT)
    //    {
    //        if (!hlMixer.GetFloat("HL3DTI_TA_Autocor0_Get_Left", out coef0)) return false;
    //        if (!hlMixer.GetFloat("HL3DTI_TA_Autocor1_Get_Left", out coef1)) return false;
    //        coef1 = coef1 / coef0;
    //        return true;
    //    }
    //    if (ear == T_ear.RIGHT)
    //    {
    //        if (!hlMixer.GetFloat("HL3DTI_TA_Autocor0_Get_Right", out coef0)) return false;
    //        if (!hlMixer.GetFloat("HL3DTI_TA_Autocor1_Get_Right", out coef1)) return false;
    //        coef1 = coef1 / coef0;
    //        return true;
    //    }
    //    return false;
    //}

    ///////////////////////////////////////
    // FREQUENCY SMEARING SIMULATION
    ///////////////////////////////////////

    /// <summary>
    /// Enable frequency smearing simulation for one or both ears
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool EnableFrequencySmearingSimulation(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!EnableFrequencySmearingSimulation(T_ear.LEFT)) return false;
            return EnableFrequencySmearingSimulation(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            TA_LEFT_ON = true;
            paramName += "FS_LeftOn";
        }
        else
        {
            TA_RIGHT_ON = true;
            paramName += "FS_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(true));
    }

    /// <summary>
    /// Disable frequency smearing simulation for one or both ears
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool DisableFrequencySmearingSimulation(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!DisableFrequencySmearingSimulation(T_ear.LEFT)) return false;
            return DisableFrequencySmearingSimulation(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            TA_LEFT_ON = false;
            paramName += "FS_LeftOn";
        }
        else
        {
            TA_RIGHT_ON = false;
            paramName += "FS_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(false));
    }

    /// <summary>
    /// Set buffer size for downward section of smearing window
    /// </summary>    
    /// <param name="ear"></param>
    /// <param name="downSize"></param>
    /// <returns></returns>
    public bool SetFrequencySmearingDownwardBufferSize(T_ear ear, int downSize)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetFrequencySmearingDownwardBufferSize(T_ear.LEFT, downSize)) return false;
            return SetFrequencySmearingDownwardBufferSize(T_ear.RIGHT, downSize);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_FS_Size_Down_";
        if (ear == T_ear.LEFT)
        {
            PARAM_LEFT_FS_DOWN_SIZE = downSize;
            paramName += "Left";
        }
        if (ear == T_ear.RIGHT)
        {
            PARAM_RIGHT_FS_DOWN_SIZE = downSize;
            paramName += "Right";
        }

        // Send command
        return hlMixer.SetFloat(paramName, (float)downSize);
    }

    /// <summary>
    /// Set buffer size for upward section of smearing window
    /// </summary>    
    /// <param name="ear"></param>
    /// <param name="upSize"></param>
    /// <returns></returns>
    public bool SetFrequencySmearingUpwardBufferSize(T_ear ear, int upSize)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetFrequencySmearingUpwardBufferSize(T_ear.LEFT, upSize)) return false;
            return SetFrequencySmearingUpwardBufferSize(T_ear.RIGHT, upSize);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_FS_Size_Up_";
        if (ear == T_ear.LEFT)
        {
            PARAM_LEFT_FS_UP_SIZE = upSize;
            paramName += "Left";
        }
        if (ear == T_ear.RIGHT)
        {
            PARAM_RIGHT_FS_UP_SIZE = upSize;
            paramName += "Right";
        }

        // Send command
        return hlMixer.SetFloat(paramName, (float)upSize);
    }

    /// <summary>
    /// Set smearing amount (in Hz) for downward section of smearing window
    /// </summary>    
    /// <param name="ear"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool SetFrequencySmearingDownwardAmount_Hz(T_ear ear, float amount)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetFrequencySmearingDownwardAmount_Hz(T_ear.LEFT, amount)) return false;
            return SetFrequencySmearingDownwardAmount_Hz(T_ear.RIGHT, amount);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_FS_Hz_Down_";
        if (ear == T_ear.LEFT)
        {
            PARAM_LEFT_FS_DOWN_HZ = amount;
            paramName += "Left";
        }
        if (ear == T_ear.RIGHT)
        {
            PARAM_RIGHT_FS_DOWN_HZ = amount;
            paramName += "Right";
        }

        // Send command
        return hlMixer.SetFloat(paramName, amount);
    }

    /// <summary>
    /// Set smearing amount (in Hz) for upward section of smearing window
    /// </summary>    
    /// <param name="ear"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool SetFrequencySmearingUpwardAmount_Hz(T_ear ear, float amount)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetFrequencySmearingUpwardAmount_Hz(T_ear.LEFT, amount)) return false;
            return SetFrequencySmearingUpwardAmount_Hz(T_ear.RIGHT, amount);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_FS_Hz_Up_";
        if (ear == T_ear.LEFT)
        {
            PARAM_LEFT_FS_UP_HZ = amount;
            paramName += "Left";
        }
        if (ear == T_ear.RIGHT)
        {
            PARAM_RIGHT_FS_UP_HZ = amount;
            paramName += "Right";
        }

        // Send command
        return hlMixer.SetFloat(paramName, amount);
    }
}