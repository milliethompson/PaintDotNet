// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the CPUCOUNT_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// CPUCOUNT_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef CPUCOUNT_EXPORTS
#define CPUCOUNT_API __declspec(dllexport)
#else
#define CPUCOUNT_API __declspec(dllimport)
#endif

#ifdef __cpluplus
extern "C"
{
#endif
    CPUCOUNT_API int GetPhysicalCpuCount(void);
    CPUCOUNT_API int GetLogicalPerPhysicalCpuCount(void);
    CPUCOUNT_API int IsHTSupported(void);
#ifdef __cplusplus
}
#endif
