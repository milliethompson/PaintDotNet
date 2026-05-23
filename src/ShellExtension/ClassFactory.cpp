/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

#pragma warning (disable: 4530)
#include "ClassFactory.h"
#include "PdnShell.h"
#include "PdnGuid.h"
#include "PdnShellExtension.h"

// This is included so as to avoid using the C standard library
// This reduces our DLL size and/or reduces our implicit import size
int __cdecl memcmp(const void *b1, const void *b2, size_t length)
{
    char *c1 = (char *)b1;
    char *c2 = (char *)b2;

    while (length > 0)
    {
        if (*c1 > *c2)
        {
            return 1;
        }
        else
        if (*c1 < *c2)
        {
            return -1;
        }

        c1++;
        c2++;
        length--;
    }

    return 0;
}


CClassFactory::CClassFactory(CLSID clsid)
{
    Constructor(clsid);
    return;
}


void CClassFactory::Constructor(CLSID clsid)
{
    m_clsidObject = clsid;
    m_lRefCount = 1;
    InterlockedIncrement(&g_lRefCount);
    return;
}


CClassFactory::~CClassFactory(void)
{
    Destructor();
    return;
}


void CClassFactory::Destructor(void)
{
    InterlockedDecrement (&g_lRefCount);
    return;
}


STDMETHODIMP CClassFactory::QueryInterface(REFIID riid, LPVOID *ppReturn)
{
    TraceEnter();
    TraceOut("riid=%S", GuidToString(riid));
    *ppReturn = NULL;

    if (IsEqualCLSID (riid, IID_IUnknown))
    {
        *ppReturn = this;
    }
    else
    if (IsEqualCLSID (riid, IID_IClassFactory))
    {
        *ppReturn = (IClassFactory *) this;
    }

    if (*ppReturn != NULL)
    {
        (*(LPUNKNOWN *)ppReturn)->AddRef ();
        TraceLeaveHr(S_OK);
        return S_OK;
    }

    TraceLeaveHr(E_NOINTERFACE);
    return E_NOINTERFACE;
}


STDMETHODIMP_(DWORD) CClassFactory::AddRef ()
{
    return InterlockedIncrement(&m_lRefCount);
}


STDMETHODIMP_(DWORD) CClassFactory::Release ()
{
    DWORD dwNewRC = InterlockedDecrement(&m_lRefCount);

    if (0 == dwNewRC)
    {
        delete this;
        return 0;
    }
    else
    {
    return dwNewRC;
    }
}


STDMETHODIMP CClassFactory::CreateInstance(IUnknown *pUnkOuter, REFIID riid, void **ppvObject)
{
    void *pvResult = NULL;
    HRESULT hr = S_OK;

    TraceEnter();
    TraceOut("riid=%S", GuidToString(riid));

    if (SUCCEEDED(hr))
    {
        if (NULL == ppvObject)
        {
            hr = E_INVALIDARG;
        }
    }

    if (SUCCEEDED(hr))
    {
        if (pUnkOuter != NULL)
        {
            hr = CLASS_E_NOAGGREGATION;
        }
    }

    CPdnShellExtension *pShellExtension = NULL;
    if (SUCCEEDED(hr))
    {
        pShellExtension = new CPdnShellExtension();

        if (NULL == pShellExtension)
        {
            hr = E_OUTOFMEMORY;
        }
    }

    if (SUCCEEDED(hr))
    {
        hr = pShellExtension->QueryInterface(riid, ppvObject);
        pShellExtension->Release ();
    }

    TraceLeaveHr(hr);
    return hr;
}


STDMETHODIMP CClassFactory::LockServer (BOOL fLock)
{
    return E_NOTIMPL;
}
