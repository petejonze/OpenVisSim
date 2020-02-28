#ifndef FASTNOISE_SIMD_C_H
#define FASTNOISE_SIMD_C_H

typedef void* POINTER;

#if defined(_MSC_VER)
    #define EXPORT __declspec(dllexport)
#elif defined(__GNUC__)
    //  GCC
    #define EXPORT __attribute__((visibility("default")))
#else
    //  do nothing and hope for the best?
    #define EXPORT
#endif


#ifdef __cplusplus
extern "C" {
#endif

	EXPORT int GetSIMDLevel();
	EXPORT void SetSIMDLevel(int);
	EXPORT POINTER NewFastNoiseSIMD(int);
	EXPORT void NativeFree(POINTER);

	EXPORT void NativeSetSeed(POINTER, int);
	EXPORT int NativeGetSeed(POINTER);
	EXPORT void NativeSetFrequency(POINTER, float);
	EXPORT void NativeSetNoiseType(POINTER, int);
	EXPORT void NativeSetAxisScales(POINTER, float, float, float);

	EXPORT void NativeSetFractalOctaves(POINTER, int);
	EXPORT void NativeSetFractalLacunarity(POINTER, float);
	EXPORT void NativeSetFractalGain(POINTER, float);
	EXPORT void NativeSetFractalType(POINTER, int);

	EXPORT void NativeSetCellularDistanceFunction(POINTER, int);
	EXPORT void NativeSetCellularReturnType(POINTER, int);
	EXPORT void NativeSetCellularNoiseLookupType(POINTER, int);
	EXPORT void NativeSetCellularNoiseLookupFrequency(POINTER, float);

	EXPORT void NativeSetPerturbType(POINTER, int);
	EXPORT void NativeSetPerturbFrequency(POINTER, float);
	EXPORT void NativeSetPerturbAmp(POINTER, float);

	EXPORT void NativeSetPerturbFractalOctaves(POINTER, int);
	EXPORT void NativeSetPerturbFractalLacunarity(POINTER, float);
	EXPORT void NativeSetPerturbFractalGain(POINTER, float);

	EXPORT void NativeFillNoiseSet(POINTER, float*, int, int, int, int, int, int, float);
	EXPORT void NativeFillSampledNoiseSet(POINTER, float*, int, int, int, int, int, int, int);

	EXPORT void NativeFillNoiseSetVector(POINTER, float*, POINTER, float, float, float);
	EXPORT void NativeFillSampledNoiseSetVector(POINTER, float*, POINTER, float, float, float);

	EXPORT POINTER NewVectorSet(float*, int, int, int ,int ,int);
	EXPORT void NativeFreeVectorSet(POINTER);

#ifdef __cplusplus
}
#endif
#endif
