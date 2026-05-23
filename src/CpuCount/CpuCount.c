// CpuCount.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "CpuCount.h"

#pragma warning(disable : 4100)
BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	    case DLL_PROCESS_ATTACH:
	    case DLL_THREAD_ATTACH:
	    case DLL_THREAD_DETACH:
	    case DLL_PROCESS_DETACH:
    		break;
	}

    return TRUE;
}

extern unsigned char CPUCount(unsigned char *LogicalNum, unsigned char *PhysicalNum);

CPUCOUNT_API int GetPhysicalCpuCount(void)
{
    unsigned char logical;
    unsigned char physical;
    unsigned char ht;

    ht = CPUCount(&logical, &physical);
    return physical;
}


CPUCOUNT_API int GetLogicalPerPhysicalCpuCount(void)
{
    unsigned char logical;
    unsigned char physical;
    unsigned char ht;

    ht = CPUCount(&logical, &physical);
    return logical;
}

CPUCOUNT_API int IsHTSupported(void)
{
    unsigned char logical;
    unsigned char physical;
    unsigned char ht;

    ht = CPUCount(&logical, &physical);
    return ht ? 1 : 0;
}