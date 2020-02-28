#include "FastNoiseSIMD_C.h"
#include "FastNoiseSIMD.h"
#include <cstring>

#define L_2_FNP(l) reinterpret_cast<FastNoiseSIMD*>(l)
#define L_2_VSP(l) reinterpret_cast<FastNoiseVectorSet*>(l)

int GetSIMDLevel()
{
	return FastNoiseSIMD::GetSIMDLevel();
}

void SetSIMDLevel(int level)
{
	FastNoiseSIMD::SetSIMDLevel(level);
}

POINTER NewFastNoiseSIMD(int seed)
{
	return reinterpret_cast<POINTER>(FastNoiseSIMD::NewFastNoiseSIMD(seed));
}

void NativeFree(POINTER p)
{
	delete L_2_FNP(p);
}

void NativeSetSeed(POINTER p, int seed)
{
	L_2_FNP(p)->SetSeed(seed);
}

int NativeGetSeed(POINTER p)
{
	return L_2_FNP(p)->GetSeed();
}

void NativeSetFrequency(POINTER p, float freq)
{
	L_2_FNP(p)->SetFrequency(freq);
}

void NativeSetNoiseType(POINTER p, int noiseType)
{
	L_2_FNP(p)->SetNoiseType(static_cast<FastNoiseSIMD::NoiseType>(noiseType));
}

void NativeSetAxisScales(POINTER p, float xScale, float yScale, float zScale)
{
	L_2_FNP(p)->SetAxisScales(xScale, yScale, zScale);
}

void NativeSetFractalOctaves(POINTER p, int octaves)
{
	L_2_FNP(p)->SetFractalOctaves(static_cast<unsigned int>(octaves));
}

void NativeSetFractalLacunarity(POINTER p, float lacunarity)
{
	L_2_FNP(p)->SetFractalLacunarity(lacunarity);
}

void NativeSetFractalGain(POINTER p, float gain)
{
	L_2_FNP(p)->SetFractalGain(gain);
}

void NativeSetFractalType(POINTER p, int fractalType)
{
	L_2_FNP(p)->SetFractalType(static_cast<FastNoiseSIMD::FractalType>(fractalType));
}

void NativeSetCellularDistanceFunction(POINTER p, int cellularDistanceFunction)
{
	L_2_FNP(p)->SetCellularDistanceFunction(static_cast<FastNoiseSIMD::CellularDistanceFunction>(cellularDistanceFunction));
}

void NativeSetCellularReturnType(POINTER p, int cellularReturnType)
{
	L_2_FNP(p)->SetCellularReturnType(static_cast<FastNoiseSIMD::CellularReturnType>(cellularReturnType));
}

void NativeSetCellularNoiseLookupType(POINTER p, int noiseType)
{
	L_2_FNP(p)->SetCellularNoiseLookupType(static_cast<FastNoiseSIMD::NoiseType>(noiseType));
}

void NativeSetCellularNoiseLookupFrequency(POINTER p, float freq)
{
	L_2_FNP(p)->SetCellularNoiseLookupFrequency(freq);
}

void NativeSetPerturbType(POINTER p, int perturbType)
{
	L_2_FNP(p)->SetPerturbType(static_cast<FastNoiseSIMD::PerturbType>(perturbType));
}

void NativeSetPerturbFrequency(POINTER p, float perturbFreq)
{
	L_2_FNP(p)->SetPerturbFrequency(perturbFreq);
}

void NativeSetPerturbAmp(POINTER p, float perturbAmp)
{
	L_2_FNP(p)->SetPerturbAmp(perturbAmp);
}

void NativeSetPerturbFractalOctaves(POINTER p, int perturbOctaves)
{
	L_2_FNP(p)->SetPerturbFractalOctaves(perturbOctaves);
}

void NativeSetPerturbFractalLacunarity(POINTER p, float perturbFractalLacunarity)
{
	L_2_FNP(p)->SetPerturbFractalLacunarity(perturbFractalLacunarity);
}

void NativeSetPerturbFractalGain(POINTER p, float perturbFractalGain)
{
	L_2_FNP(p)->SetPerturbFractalGain(perturbFractalGain);
}

void NativeFillNoiseSet(POINTER p, float* noiseSet, int xStart, int yStart, int zStart, int xSize, int ySize, int zSize, float scaleModifier)
{
	L_2_FNP(p)->FillNoiseSet(noiseSet, xStart, yStart, zStart, xSize, ySize, zSize, scaleModifier);
}

void NativeFillSampledNoiseSet(POINTER p, float* noiseSet, int xStart, int yStart, int zStart, int xSize, int ySize, int zSize, int sampleScale)
{
	L_2_FNP(p)->FillSampledNoiseSet(noiseSet, xStart, yStart, zStart, xSize, ySize, zSize, sampleScale);
}

void NativeFillNoiseSetVector(POINTER p, float* noiseSet, POINTER pVectorSet, float xOffset, float yOffset, float zOffset)
{
	L_2_FNP(p)->FillNoiseSet(noiseSet, L_2_VSP(pVectorSet), xOffset, yOffset, zOffset);
}

void NativeFillSampledNoiseSetVector(POINTER p, float* noiseSet, POINTER pVectorSet, float xOffset, float yOffset, float zOffset)
{
	L_2_FNP(p)->FillSampledNoiseSet(noiseSet, L_2_VSP(pVectorSet), xOffset, yOffset, zOffset);
}

POINTER NewVectorSet(float* vectorSetArray, int arraySize, int samplingScale, int sampleSizeX, int sampleSizeY, int sampleSizeZ)
{
	FastNoiseVectorSet* vectorSet = new FastNoiseVectorSet();

	float* dataCopy = new float[arraySize];
	memcpy(dataCopy, vectorSetArray, arraySize * sizeof(float));

	vectorSet->size = arraySize / 3;
	vectorSet->sampleScale = static_cast<int>(samplingScale);
	vectorSet->sampleSizeX = sampleSizeX;
	vectorSet->sampleSizeY = sampleSizeY;
	vectorSet->sampleSizeZ = sampleSizeZ;
	vectorSet->xSet = dataCopy;
	vectorSet->ySet = vectorSet->xSet + vectorSet->size;
	vectorSet->zSet = vectorSet->ySet + vectorSet->size;

	return reinterpret_cast<POINTER>(vectorSet);
}

void NativeFreeVectorSet(POINTER p)
{
	delete L_2_VSP(p);
}
