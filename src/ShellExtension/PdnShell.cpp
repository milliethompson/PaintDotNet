/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

#pragma warning (disable: 4530)
#include <windows.h>
#include <comcat.h>
#include "PdnShell.h"
#include "ClassFactory.h"

#pragma data_seg(".text")
#define INITGUID
#include <initguid.h>
#include <shlguid.h>
#include "PdnGuid.h"
#pragma data_seg()

#include <iostream>
using namespace std;

HINSTANCE g_hInstance;
volatile LONG g_lRefCount;

#ifndef NDEBUG
void TraceOut(const char *szFormat, ...)
{
    va_list marker;
    va_start(marker, szFormat);

    char buffer[2048];
    _vsnprintf(buffer, sizeof(buffer), szFormat, marker);

    OutputDebugString(buffer);
}
#endif

const WCHAR *GuidToString(GUID guid)
{
    static WCHAR szGuid[128];
    StringFromGUID2(guid, szGuid, 128);
    return szGuid;
}

extern "C" BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD dwReasonForCall, LPVOID lpvReserved)
{
    TraceEnter();

    switch (dwReasonForCall)
    {
        case DLL_PROCESS_ATTACH:
            g_hInstance = hInstance;
            break;

        case DLL_THREAD_ATTACH:
            break;

        case DLL_THREAD_DETACH:
            break;

        case DLL_PROCESS_DETACH:
            g_hInstance = NULL;
            break;
    }

    TraceLeave();
    return TRUE;
}

STDAPI DllCanUnloadNow()
{
    HRESULT hr;

    TraceEnter();

    if (0 == g_lRefCount)
    {
        hr = S_OK;
    }
    else
    {
        hr = S_FALSE;
    }

    TraceLeaveHr(hr);
    return hr;
}

STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID *ppv)
{
    TraceEnter();
    CClassFactory *pFactory = NULL;
    HRESULT hr = S_OK;

    if (SUCCEEDED(hr))
    {
        if (NULL == ppv)
        {
            hr = E_INVALIDARG;
        }
    }

    if (SUCCEEDED(hr))
    {
        if (!IsEqualCLSID(rclsid, CLSID_PdnShellExtension))
        {
            return CLASS_E_CLASSNOTAVAILABLE;
        }
    }

    if (SUCCEEDED(hr))
    {
        pFactory = new CClassFactory(rclsid);

        if (NULL == pFactory)
        {
            return E_OUTOFMEMORY;
        }
    }

    if (SUCCEEDED(hr))
    {
        hr = pFactory->QueryInterface(riid, ppv);
        pFactory->Release();
    }

    TraceLeaveHr(hr);
    return hr;
}

HRESULT SetRegistryStringValue(HKEY hRootKey, LPCWSTR wszSubKeyName, LPCWSTR wszValueName, LPCWSTR wszStringValue)
{
    HRESULT hr = S_OK;
    HKEY hKey = NULL;
    LONG lResult;

    if (SUCCEEDED(hr))
    {
        lResult = RegCreateKeyExW(hRootKey, wszSubKeyName, 0, NULL, REG_OPTION_NON_VOLATILE, KEY_WRITE, NULL, &hKey, NULL);
        if (lResult != ERROR_SUCCESS)
        {
            hr = HRESULT_FROM_WIN32(lResult);
        }
    }

    if (SUCCEEDED(hr))
    {
        lResult = RegSetValueExW(hKey, wszValueName, 0, REG_SZ, (const BYTE *)wszStringValue, 1 + (DWORD)wcslen(wszStringValue));
        if (lResult != ERROR_SUCCESS)
        {
            hr = HRESULT_FROM_WIN32(lResult);
        }
    }

    if (NULL != hKey)
    {
        RegCloseKey(hKey);
    }

    return hr;
}

