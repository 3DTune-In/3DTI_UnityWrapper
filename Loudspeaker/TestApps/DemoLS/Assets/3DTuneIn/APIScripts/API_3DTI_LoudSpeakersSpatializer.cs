﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class API_3DTI_LoudSpeakersSpatializer : MonoBehaviour {

    // SOURCE:
    int lastSourceID = 0;                       // Internal use for debug log

    // CONFIGURATION PRESETS:
    public enum T_LoudSpeakerConfigurationPreset { LS_PRESET_CUBE=0, LS_PRESET_OCTAHEDRON=1, LS_PRESET_2DSQUARE=2 };
    public T_LoudSpeakerConfigurationPreset  speakersConfigurationPreset = T_LoudSpeakerConfigurationPreset.LS_PRESET_CUBE;

    // ADVANCED:
    public float scaleFactor = 1.0f;            // Used by Inspector
    public bool modFarLPF = true;               // Used by Inspector
    public bool modDistAtt = true;              // Used by Inspector
    public float magAnechoicAttenuation = -6.0f;    // Used by Inspector    
    public float magSoundSpeed = 343.0f;            // Used by Inspector
    public bool debugLog = false;                   // Used by Inspector
    public float structureSide = 1.0f;  // Used by Inspector    

    //public float structureYaw   = 0.0f;
    //public float structurePitch = 0.0f;
    public List<Vector3> speakerPositions;  // Used by Inspector
    public List<Vector3> speakerOffsets;    // Used by Inspector
    public List<float> speakerWeights;      // Used by Inspector
    int numberOfSpeakers = 0;    

    // Definition of spatializer plugin commands   
    int SET_SCALE_FACTOR    = 0;
    int SET_SOURCE_ID       = 1;    
    int SET_MOD_FARLPF      = 2;
    int SET_MOD_DISTATT     = 3;    
    int SET_MAG_ANECHATT    = 4;
    int SET_MAG_SOUNDSPEED  = 5;    
    int SET_DEBUG_LOG       = 6;
    int SET_SAVE_SPEAKERS_CONFIG = 7;
    int SET_SPEAKER_1_X =8;
    int SET_SPEAKER_2_X = 9;
    int SET_SPEAKER_3_X = 10;
    int SET_SPEAKER_4_X = 11;
    int SET_SPEAKER_5_X = 12;
    int SET_SPEAKER_6_X = 13;
    int SET_SPEAKER_7_X = 14;
    int SET_SPEAKER_8_X = 15;
    int SET_SPEAKER_1_Y = 16;
    int SET_SPEAKER_2_Y = 17;
    int SET_SPEAKER_3_Y = 18;
    int SET_SPEAKER_4_Y = 19;
    int SET_SPEAKER_5_Y = 20;
    int SET_SPEAKER_6_Y = 21;
    int SET_SPEAKER_7_Y = 22;
    int SET_SPEAKER_8_Y = 23;
    int SET_SPEAKER_1_Z = 24;
    int SET_SPEAKER_2_Z = 25;
    int SET_SPEAKER_3_Z = 26;
    int SET_SPEAKER_4_Z = 27;
    int SET_SPEAKER_5_Z = 28;
    int SET_SPEAKER_6_Z = 29;
    int SET_SPEAKER_7_Z = 30;
    int SET_SPEAKER_8_Z = 31;
    int SET_SPEAKER_1_W = 32;
    int SET_SPEAKER_2_W = 33;
    int SET_SPEAKER_3_W = 34;
    int SET_SPEAKER_4_W = 35;
    int SET_SPEAKER_5_W = 36;
    int SET_SPEAKER_6_W = 37;
    int SET_SPEAKER_7_W = 38;
    int SET_SPEAKER_8_W = 39;
    int GET_MINIMUM_DISTANCE = 40;

    // Hack for modifying one single AudioSource (TO DO: fix this)
    bool selectSource = false;
    AudioSource selectedSource;

    // This is needed from Unity 2017
    bool isInitialized = false;

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Automatic setup of Toolkit Core (as read from custom GUI in Unity Inspector)
    /// </summary>
    void Start()
    {
        //StartLoudSpeakersSpatializer();
    }

    void Update()
    {
        if (!isInitialized)
        {
            if (StartLoudSpeakersSpatializer())
                isInitialized = true;
        }
    }

    /////////////////////////////////////////////////////////////////////
    // GLOBAL METHODS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Sends all configuration to all spatialized sources. 
    /// Use it each time you activate an audio source or activate its "spatialize" attribute. 
    /// </summary>
    public bool StartLoudSpeakersSpatializer(AudioSource source = null)
    {
        // Select only one AudioSource
        if (source != null)
        {
            selectSource = true;
            selectedSource = source;
        }

        // Debug log:
        if (!SendWriteDebugLog(debugLog)) return false;

        // Global setup:
        if (!SetScaleFactor(scaleFactor)) return false;
        if (!SendSourceIDs()) return false;

        // Setup modules enabler:
        if (!SetupModulesEnabler()) return false;

        // Setup speakers configuration (position)
        //if (!SetupSpeakersConfiguration(structureSide)) return false;
        if (!SetSpeakersConfigurationPreset(speakersConfigurationPreset)) return false;

        // Go back to default state, affecting all sources
        selectSource = false;

        return true;
    }


    /////////////////////////////////////////////////////////////////////
    // SOURCE API METHODS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set one speakers configuration preset 
    /// </summary>
    /// <returns></returns>
    public bool SetSpeakersConfigurationPreset(T_LoudSpeakerConfigurationPreset preset)
    {
        if ((preset != T_LoudSpeakerConfigurationPreset.LS_PRESET_2DSQUARE) &&
            (preset != T_LoudSpeakerConfigurationPreset.LS_PRESET_CUBE) &&
            (preset != T_LoudSpeakerConfigurationPreset.LS_PRESET_OCTAHEDRON))
            return false;

        speakersConfigurationPreset = preset;
        speakerPositions.Clear();
        speakerOffsets.Clear();
        speakerWeights.Clear();
        
        switch (preset)
        {
            case T_LoudSpeakerConfigurationPreset.LS_PRESET_CUBE:
                numberOfSpeakers = 8;
                break;

            //case T_LoudSpeakerConfigurationPreset.LS_PRESET_DOME:
            //    numberOfSpeakers = 8;
            //    break;

            case T_LoudSpeakerConfigurationPreset.LS_PRESET_OCTAHEDRON:
                numberOfSpeakers = 6;
                break;

            case T_LoudSpeakerConfigurationPreset.LS_PRESET_2DSQUARE:
                numberOfSpeakers = 4;
                break;

            default:
                return false;                
        }        

        // Create positions, offsets and weights for each speaker
        for (int i=0; i < numberOfSpeakers; i++)
        {
            speakerPositions.Add(Vector3.zero);
            speakerOffsets.Add(Vector3.zero);
            speakerWeights.Add(0.0f);
        }

        // Calculate speaker positions and weights and send configuration to toolkit
        CalculateSpeakerPositions();
        CalculateSpeakerWeights();
        if (!SendLoudSpeakersConfiguration()) return false;
        return true;
    }

    /// <summary>
    /// Get number of speakers of currently set speakers configuration preset
    /// </summary>
    /// <returns></returns>
    public int GetNumberOfSpeakers()
    {
        return numberOfSpeakers;
    }    

    /// <summary>
    /// Set size of one side of the speakers configuration structure, in meters
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public bool SetStructureSide(float side)
    {
        structureSide = side;
        CalculateSpeakerPositions();
        return SendLoudSpeakersConfiguration();
    }

    /// <summary>
    /// Get minimum distance from any source to listener (maximum distance of all speakers in the configuration)
    /// </summary>
    /// <returns></returns>
    public float GetMinimumDistanceToListener()
    {
        float returnValue;

        List<AudioSource> sources = GetAllSpatializedSources();
        if (sources.Count == 0)
            return -1.0f;

        if (!sources[0].GetSpatializerFloat(GET_MINIMUM_DISTANCE, out returnValue))
            return -1.0f;
        else
            return returnValue;
    }

    /// <summary>
    /// Get position of one speaker, including offset
    /// </summary>
    /// <param name="speakerID"></param>
    /// <returns></returns>
    public Vector3 GetSpeakerPosition(int speakerID)
    {
        return new Vector3(speakerPositions[speakerID].x + speakerOffsets[speakerID].x,
                            speakerPositions[speakerID].y + speakerOffsets[speakerID].y,
                            speakerPositions[speakerID].z + speakerOffsets[speakerID].z);
    }

    /// <summary>
    /// Set offset for one speaker
    /// </summary>
    /// <param name="speakerID"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public bool SetSpeakerOffset(int speakerID, Vector3 offset)
    {
        if ((speakerID < numberOfSpeakers) && (speakerID >= 0))
        {
            speakerOffsets[speakerID] = offset;
            return SendLoudSpeakersConfiguration();
        }
        else
            return false;
    }

    /////////////////////////////////////////////////////////////////////
    // ADVANCED API METHODS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Send configuration of all speakers
    /// </summary>
    public bool SendLoudSpeakersConfiguration()
    {
        // Speaker 1     
        if (numberOfSpeakers > 0)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_1_X, GetSpeakerPosition(0).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_1_Y, GetSpeakerPosition(0).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_1_Z, GetSpeakerPosition(0).z)) return false;            
        }

        // Speaker 2
        if (numberOfSpeakers > 1)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_2_X, GetSpeakerPosition(1).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_2_Y, GetSpeakerPosition(1).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_2_Z, GetSpeakerPosition(1).z)) return false;            
        }

        // Speaker 3
        if (numberOfSpeakers > 2)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_3_X, GetSpeakerPosition(2).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_3_Y, GetSpeakerPosition(2).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_3_Z, GetSpeakerPosition(2).z)) return false;            
        }

        // Speaker 4
        if (numberOfSpeakers > 3)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_4_X, GetSpeakerPosition(3).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_4_Y, GetSpeakerPosition(3).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_4_Z, GetSpeakerPosition(3).z)) return false;            
        }

        // Speaker 5
        if (numberOfSpeakers > 4)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_5_X, GetSpeakerPosition(4).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_5_Y, GetSpeakerPosition(4).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_5_Z, GetSpeakerPosition(4).z)) return false;            
        }

        // Speaker 6
        if (numberOfSpeakers > 5)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_6_X, GetSpeakerPosition(5).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_6_Y, GetSpeakerPosition(5).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_6_Z, GetSpeakerPosition(5).z)) return false;            
        }

        // Speaker 7
        if (numberOfSpeakers > 6)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_7_X, GetSpeakerPosition(6).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_7_Y, GetSpeakerPosition(6).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_7_Z, GetSpeakerPosition(6).z)) return false;            
        }

        // Speaker 8
        if (numberOfSpeakers > 7)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_8_X, GetSpeakerPosition(7).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_8_Y, GetSpeakerPosition(7).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_8_Z, GetSpeakerPosition(7).z)) return false;            
        }

        // Send command for ending setup
        if (!SendCommandForAllSources(SET_SAVE_SPEAKERS_CONFIG, 1.0f)) return false;

        // Send weights 
        if (numberOfSpeakers > 0) if (!SendCommandForAllSources(SET_SPEAKER_1_W, speakerWeights[0])) return false;
        if (numberOfSpeakers > 1) if (!SendCommandForAllSources(SET_SPEAKER_2_W, speakerWeights[1])) return false;
        if (numberOfSpeakers > 2) if (!SendCommandForAllSources(SET_SPEAKER_3_W, speakerWeights[2])) return false;
        if (numberOfSpeakers > 3) if (!SendCommandForAllSources(SET_SPEAKER_4_W, speakerWeights[3])) return false;
        if (numberOfSpeakers > 4) if (!SendCommandForAllSources(SET_SPEAKER_5_W, speakerWeights[4])) return false;
        if (numberOfSpeakers > 5) if (!SendCommandForAllSources(SET_SPEAKER_6_W, speakerWeights[5])) return false;
        if (numberOfSpeakers > 6) if (!SendCommandForAllSources(SET_SPEAKER_7_W, speakerWeights[6])) return false;
        if (numberOfSpeakers > 7) if (!SendCommandForAllSources(SET_SPEAKER_8_W, speakerWeights[7])) return false;
        
        return true;
    }

    /// <summary>
    /// Calculate positions for all speakers, but do not send them to the plugin yet
    /// </summary>
    public void CalculateSpeakerPositions()
    {
        switch (speakersConfigurationPreset)
        {
            case T_LoudSpeakerConfigurationPreset.LS_PRESET_CUBE:

                // Front Left Down speaker                
                speakerPositions[2] = new Vector3(-0.5f * structureSide, -0.5f * structureSide, 0.5f * structureSide) + speakerOffsets[0];

                // Front Right Down speaker
                speakerPositions[3] = new Vector3(0.5f * structureSide, -0.5f * structureSide, 0.5f * structureSide) + speakerOffsets[1];

                // Rear Left Down speaker
                speakerPositions[6] = new Vector3(-0.5f * structureSide, -0.5f * structureSide, -0.5f * structureSide) + speakerOffsets[2];

                // Rear Right Down speaker
                speakerPositions[7] = new Vector3(0.5f * structureSide, -0.5f * structureSide, -0.5f * structureSide) + speakerOffsets[3];

                // Front Left Up speaker
                speakerPositions[0] = new Vector3(-0.5f * structureSide, 0.5f * structureSide, 0.5f * structureSide) + speakerOffsets[4];

                // Front Right Up speaker
                speakerPositions[1] = new Vector3(0.5f * structureSide, 0.5f * structureSide, 0.5f * structureSide) + speakerOffsets[5];

                // Rear Left Up speaker
                speakerPositions[4] = new Vector3(-0.5f * structureSide, 0.5f * structureSide, -0.5f * structureSide) + speakerOffsets[6];

                // Rear Right Up speaker
                speakerPositions[5] = new Vector3(0.5f * structureSide, 0.5f * structureSide, -0.5f * structureSide) + speakerOffsets[7];

                break;

            //case T_LoudSpeakerConfigurationPreset.LS_PRESET_DOME:                

            //    // Front speaker
            //    speakerPositions[0] =  new Vector3(0.0f, 0.0f, structureSide) + speakerOffsets[0];

            //    // Left speaker
            //    speakerPositions[1] = new Vector3(-structureSide, 0.0f, 0.0f) + speakerOffsets[1];

            //    // Rear speaker
            //    speakerPositions[2] =  new Vector3(0.0f, 0.0f, -structureSide) + speakerOffsets[2];

            //    // Right speaker
            //    speakerPositions[3] =  new Vector3(structureSide, 0.0f, 0.0f) + speakerOffsets[3];

            //    // Front Left Up speaker
            //    speakerPositions[4] =  new Vector3(-0.5f * structureSide, 0.5f * structureSide, 0.5f * structureSide) + speakerOffsets[4];

            //    // Front Right Up speaker
            //    speakerPositions[5] =  new Vector3(0.5f * structureSide, 0.5f * structureSide, 0.5f * structureSide) + speakerOffsets[5];

            //    // Rear Left Up speaker
            //    speakerPositions[6] =  new Vector3(-0.5f * structureSide, 0.5f * structureSide, -0.5f * structureSide) + speakerOffsets[6];

            //    // Rear Right Up speaker
            //    speakerPositions[7] =  new Vector3(0.5f * structureSide, 0.5f * structureSide, -0.5f * structureSide) + speakerOffsets[7];

            //    break;

            case T_LoudSpeakerConfigurationPreset.LS_PRESET_OCTAHEDRON:

                // Front speaker
                speakerPositions[0] = new Vector3(0.0f, 0.0f, structureSide) + speakerOffsets[0];

                // Left speaker
                speakerPositions[1] = new Vector3(-structureSide, 0.0f, 0.0f) + speakerOffsets[1];

                // Rear speaker
                speakerPositions[2] = new Vector3(0.0f, 0.0f, -structureSide) + speakerOffsets[2];

                // Right speaker
                speakerPositions[3] = new Vector3(structureSide, 0.0f, 0.0f) + speakerOffsets[3];

                // Zenith speaker
                speakerPositions[4] = new Vector3(0.0f, structureSide, 0.0f) + speakerOffsets[4];

                // Nadir speaker
                speakerPositions[5] = new Vector3(0.0f, -structureSide, 0.0f) + speakerOffsets[5];

                break;

            case T_LoudSpeakerConfigurationPreset.LS_PRESET_2DSQUARE:

                // Front speaker
                speakerPositions[0] = new Vector3(0.0f, 0.0f, structureSide) + speakerOffsets[0];

                // Left speaker
                speakerPositions[1] = new Vector3(-structureSide, 0.0f, 0.0f) + speakerOffsets[1];

                // Rear speaker
                speakerPositions[2] = new Vector3(0.0f, 0.0f, -structureSide) + speakerOffsets[2];

                // Right speaker
                speakerPositions[3] = new Vector3(structureSide, 0.0f, 0.0f) + speakerOffsets[3];

                break;

        }
    }

    /// <summary>
    /// Calculate weights for all speakers, but do not send them to the plugin yet
    /// </summary>
    public void CalculateSpeakerWeights()
    {
        // First approach; regular configurations only
        for (int i = 0; i < numberOfSpeakers; i++)
        {
            speakerWeights[i] = 1.0f / numberOfSpeakers;
        }
    }

    /// <summary>
    /// Set ID for all sources, for internal use of the wrapper
    /// </summary>
    public bool SendSourceIDs()
    {
        if (!selectSource)
        {
            List<AudioSource> audioSources = GetAllSpatializedSources();
            foreach (AudioSource source in audioSources)
            {
                if (!source.SetSpatializerFloat(SET_SOURCE_ID, (float)++lastSourceID)) return false;
            }
            return true;
        }
        else
            return selectedSource.SetSpatializerFloat(SET_SOURCE_ID, (float)++lastSourceID);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set scale factor. Allows the toolkit to work with big-scale or small-scale scenarios
    /// </summary>
    public bool SetScaleFactor(float scale)
    {
        scaleFactor = scale;
        return SendCommandForAllSources(SET_SCALE_FACTOR, scale);
    }

    /// <summary>
    ///  Setup modules enabler, allowing to switch on/off core features
    /// </summary>
    public bool SetupModulesEnabler()
    {
        if (!SetModFarLPF(modFarLPF)) return false;
        if (!SetModDistanceAttenuation(modDistAtt)) return false;       
        return true;
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off far distance LPF
    /// </summary>        
    public bool SetModFarLPF(bool _enable)
    {
        modFarLPF = _enable;        
        return SendCommandForAllSources(SET_MOD_FARLPF, Bool2Float(_enable));
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off distance attenuation
    /// </summary>        
    public bool SetModDistanceAttenuation(bool _enable)
    {
        modDistAtt = _enable;
        return SendCommandForAllSources(SET_MOD_DISTATT, Bool2Float(_enable));
    }

    /////////////////////////////////////////////////////////////////////
  
    /// <summary>
    /// Set magnitude Anechoic Attenuation
    /// </summary>    
    public bool SetMagnitudeAnechoicAttenuation(float value)
    {
        magAnechoicAttenuation = value;
        return SendCommandForAllSources(SET_MAG_ANECHATT, value);
    }

    /// <summary>
    /// Set magnitude Sound Speed
    /// </summary>    
    public bool SetMagnitudeSoundSpeed(float value)
    {
        magSoundSpeed = value;
        return SendCommandForAllSources(SET_MAG_SOUNDSPEED, value);
    }

    /////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////
    // AUXILIARY FUNCTIONS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Send command to plugin to switch on/off write to Debug Log file
    /// </summary>
    public bool SendWriteDebugLog(bool _enable)
    {
        debugLog = _enable;
        return SendCommandForAllSources(SET_DEBUG_LOG, Bool2Float(_enable));
    }

    /// <summary>
    /// Send command to the DLL, for each registered source
    /// </summary>
    public bool SendCommandForAllSources(int command, float value)
    {
        if (!selectSource)
        {
            List<AudioSource> audioSources = GetAllSpatializedSources();
            foreach (AudioSource source in audioSources)
            {
                if (!source.SetSpatializerFloat(command, value))
                    return false;
            }
            return true;
        }
        else
            return selectedSource.SetSpatializerFloat(command, value);
    }

    /// <summary>
    /// Returns a list with all audio sources with the Spatialized toggle checked
    /// </summary>
    public List<AudioSource> GetAllSpatializedSources()
    {
        //GameObject[] audioSources = GameObject.FindGameObjectsWithTag("AudioSource");

        List<AudioSource> spatializedSources = new List<AudioSource>();

        AudioSource[] audioSources = UnityEngine.Object.FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source.spatialize)
                spatializedSources.Add(source);
        }

        return spatializedSources;
    }

    /// <summary>
    /// Auxiliary function
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    float Bool2Float(bool v)
    {
        if (v)
            return 1.0f;
        else
            return 0.0f;
    }
}