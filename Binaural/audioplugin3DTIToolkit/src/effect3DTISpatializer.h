#pragma once
#include "AudioPluginUtil.h"
#include <BinauralSpatializer/Core.h>
#include <Common/DynamicCompressorStereo.h>


namespace Spatializer3DTI
    {
    
    enum
    {
        PARAM_HRTF_FILE_STRING, //0
        PARAM_HEAD_RADIUS,
        PARAM_SCALE_FACTOR,
        PARAM_SOURCE_ID,    // DEBUG
        PARAM_CUSTOM_ITD,
        PARAM_HRTF_INTERPOLATION, // 5
        PARAM_MOD_FARLPF,
        PARAM_MOD_DISTATT,
        PARAM_MOD_NEAR_FIELD_ILD,
        PARAM_MOD_HRTF,
        PARAM_MAG_ANECHATT, // 10
        PARAM_MAG_SOUNDSPEED,
        PARAM_NEAR_FIELD_ILD_FILE_STRING,
        PARAM_DEBUG_LOG,
        
        // HA directionality
        PARAM_HA_DIRECTIONALITY_EXTEND_LEFT,
        PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT, // 15
        PARAM_HA_DIRECTIONALITY_ON_LEFT,
        PARAM_HA_DIRECTIONALITY_ON_RIGHT,
        
        // Limiter
        PARAM_LIMITER_SET_ON,
        PARAM_LIMITER_GET_COMPRESSION,
        
        // INITIALIZATION CHECK
        PARAM_IS_CORE_READY, // 20
        
        // HRTF resampling step
        PARAM_HRTF_STEP,
        
        // High Performance and None modes
        PARAM_HIGH_PERFORMANCE_ILD_FILE_STRING,
        PARAM_SPATIALIZATION_MODE,
        PARAM_BUFFER_SIZE,
        PARAM_SAMPLE_RATE,
        PARAM_BUFFER_SIZE_CORE,
        PARAM_SAMPLE_RATE_CORE,
        
        
        P_NUM
    };
    
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state);
    
    class GlobalState
    {
    public:
        std::shared_ptr<Binaural::CListener> listener;
        Binaural::CCore core;
        bool coreReady;
        bool loadedHRTF;                // New
        bool loadedNearFieldILD;        // New
        bool loadedHighPerformanceILD;    // New
        int spatializationMode;            // New
        float parameters[P_NUM];
        
        // STRING SERIALIZER
        char* strHRTFpath;
        bool strHRTFserializing;
        int strHRTFcount;
        int strHRTFlength;
        char* strNearFieldILDpath;
        bool strNearFieldILDserializing;
        int strNearFieldILDcount;
        int strNearFieldILDlength;
        char* strHighPerformanceILDpath;
        bool strHighPerformanceILDserializing;
        int strHighPerformanceILDcount;
        int strHighPerformanceILDlength;
        
        // Limiter
        Common::CDynamicCompressorStereo limiter;
        
        // DEBUG LOG
        bool debugLog = false;
        
        // MUTEX
        std::mutex spatializerMutex;
        
        
    protected:
        // Init parameters. Core is not ready until we load the HRTF. ILD will be disabled, so we don't need to worry yet
        GlobalState(int sampleRate, int dspBufferSize);
        
        // Instance is only constructed
        friend UNITY_AUDIODSP_RESULT CreateCallback(UnityAudioEffectState* state);
    };
    
    struct EffectData
    {
        int sourceID;    // DEBUG
        std::shared_ptr<Binaural::CSingleSourceDSP> audioSource;
        //        int bufferSize;
        //    int sampleRate;
    };
    
    
    int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition);
    int LoadHRTFBinaryString(const std::basic_string<uint8_t>& hrtfData, std::shared_ptr<Binaural::CListener> listener);
    
    
    bool IsCoreReady();
    void UpdateCoreIsReady();
    
//    template <class T>
//    void WriteLog(UnityAudioEffectState* state, string logtext, const T& value);
//
//    // Defined in the cpp file (it's normal to define templates in the header but doesn't matter here as it's only used in that file anyway)
//    template <class T>
//    void WriteLog(string logtext, const T& value, int sourceID=-1);
}

