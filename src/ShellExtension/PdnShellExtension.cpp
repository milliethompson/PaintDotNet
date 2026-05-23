/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

#pragma warning (disable: 4530)
#include "PdnShellExtension.h"
#include <shlwapi.h>
#pragma comment(lib, "shlwapi.lib")
#include <gdiplus.h>
#pragma comment(lib, "gdiplus.lib")
#include <atlenc.h>
#include "PdnGuid.h"
#include "PdnShell.h"
#include <windows.h>
#include "MemoryStream.h"

using namespace Gdiplus;

CPdnShellExtension::CPdnShellExtension()
    : m_lRefCount(1),
      m_bstrFileName(NULL)
{
    m_size.cx = -1;
    m_size.cy = -1;
    InterlockedIncrement(&g_lRefCount);
}

CPdnShellExtension::~CPdnShellExtension()
{
    InterlockedDecrement(&g_lRefCount);
}

DWORD CPdnShellExtension::AddRef()
{
    TraceEnter();
    TraceLeave();
    return (DWORD)InterlockedIncrement(&m_lRefCount);
}

DWORD CPdnShellExtension::Release()
{
    TraceEnter();
    DWORD dwRefCount = (DWORD)InterlockedDecrement(&m_lRefCount);

    if (0 == dwRefCount)
    {
        delete this;
    }

    TraceLeave();
    return dwRefCount;
}

STDMETHODIMP CPdnShellExtension::QueryInterface(REFIID iid, void **ppvObject)
{
    HRESULT hr = S_OK;
    TraceEnter();
    TraceOut("riid=%S", GuidToString(iid));

    if (SUCCEEDED(hr))
    {
        if (NULL == ppvObject)
        {
            hr = E_INVALIDARG;
        }
    }

    if (SUCCEEDED(hr))
    {
        *ppvObject = NULL;

        if (IsEqualCLSID(iid, IID_IUnknown))
        {
            *ppvObject = this;
        }
        else if (IsEqualCLSID(iid, IID_IPersistFile))
        {
            *ppvObject = (IPersistFile *)this;
        }
        else if (IsEqualCLSID(iid, IID_IExtractImage))
        {
            *ppvObject = (IExtractImage *)this;
        }
        else if (IsEqualCLSID(iid, CLSID_PdnShellExtension))
        {
            *ppvObject = this;
        }
    }

    if (SUCCEEDED(hr))
    {
        if (NULL != *ppvObject)
        {
            (*(IUnknown **)ppvObject)->AddRef();
            hr = S_OK;
        }
        else
        {
            hr = E_NOINTERFACE;
        }
    }

    TraceLeaveHr(hr);
    return hr;
}


STDMETHODIMP CPdnShellExtension::GetClassID(CLSID *pClassID)
{
    HRESULT hr = E_NOTIMPL;
    TraceEnter();
    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CPdnShellExtension::GetCurFile(LPOLESTR *ppszFileName)
{
    HRESULT hr = E_NOTIMPL;
    TraceEnter();
    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CPdnShellExtension::IsDirty()
{
    HRESULT hr = E_NOTIMPL;
    TraceEnter();
    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CPdnShellExtension::Load(LPCOLESTR pszFileName, 
                                      DWORD dwMode)
{
    HRESULT hr = S_OK;
    TraceEnter();

    TraceOut("filename=%S", pszFileName);
    TraceOut("mode=%u", dwMode);

    if (SUCCEEDED(hr))
    {
        if (NULL == pszFileName)
        {
            hr = E_INVALIDARG;
        }
    }

    if (SUCCEEDED(hr))
    {
        SysFreeString(m_bstrFileName);
        m_bstrFileName = SysAllocString(pszFileName);

        if (NULL == m_bstrFileName)
        {
            return E_OUTOFMEMORY;
        }
        else
        {
            return S_OK;
        }
    }

    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CPdnShellExtension::Save(LPCOLESTR pszFileName, 
                                      BOOL fRemember)
{
    HRESULT hr = E_NOTIMPL;
    TraceEnter();

    TraceOut("fileName=%S", pszFileName);

    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CPdnShellExtension::SaveCompleted(LPCOLESTR pszFileName)
{
    HRESULT hr = E_NOTIMPL;
    TraceEnter();
    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CPdnShellExtension::Extract(HBITMAP *phBmpImage)
{
    HRESULT hr = S_OK;
    DWORD dwError = ERROR_SUCCESS;
    BOOL bResult = TRUE;
    TraceEnter();

    // Open file
    HANDLE hFile = INVALID_HANDLE_VALUE;
    if (SUCCEEDED(hr))
    {
        LPCWSTR lpFileName = (LPCWSTR)m_bstrFileName;
        DWORD dwDesiredAccess = GENERIC_READ;
        DWORD dwShareMode = FILE_SHARE_READ;
        LPSECURITY_ATTRIBUTES lpSecurityAttributes = NULL;
        DWORD dwCreationDisposition = OPEN_EXISTING;
        DWORD dwFlagsAndAttributes = FILE_FLAG_SEQUENTIAL_SCAN;
        HANDLE hTemplateFile = NULL;

        hFile = CreateFileW(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

        if (INVALID_HANDLE_VALUE == hFile)
        {
            dwError = GetLastError();
            hr = HRESULT_FROM_WIN32(dwError);
            TraceOut("CreateFile failed, hr=0x%x", hr);
        }
    }

    // Read magic numbers
    BOOL bPdn3File = FALSE;
    if (SUCCEEDED(hr))
    {
        BYTE bMagic[4];
        ZeroMemory(bMagic, sizeof(bMagic));
        DWORD dwBytesRead = 0;

        bResult = ReadFile(hFile, (LPVOID)bMagic, sizeof(bMagic), &dwBytesRead, NULL);

        if (!bResult)
        {
            dwError = GetLastError();
            hr = HRESULT_FROM_WIN32(dwError);
            TraceOut("ReadFile(1) failed, hr=0x%x", hr);
        }
        else 
        {
            if (dwBytesRead == sizeof(bMagic) &&
                bMagic[0] == 'P' &&
                bMagic[1] == 'D' &&
                bMagic[2] == 'N' &&
                bMagic[3] == '3')
            {
                bPdn3File = TRUE;
            }
        }
    }

    if (SUCCEEDED(hr) && bPdn3File)
    {
        TraceOut("we have a pdn3 file");

        // Read + decode length
        int iLength = -1;
        if (SUCCEEDED(hr))
        {
            BYTE bLength[3];
            DWORD dwBytesRead = 0;

            bResult = ReadFile(hFile, (LPVOID)bLength, sizeof(bLength), &dwBytesRead, NULL);

            if (!bResult)
            {
                dwError = GetLastError();
                hr = HRESULT_FROM_WIN32(dwError);
                TraceOut("ReadFile(2) failed, hr=0x%x", hr);
            }
            else
            {
                iLength = bLength[0] + (bLength[1] << 8) + (bLength[2] << 16);
            }
        }

        // Allocate buffer
        BYTE *pbHeaderBytes = NULL;
        if (SUCCEEDED(hr))
        {
            pbHeaderBytes = new BYTE[1 + iLength];

            if (NULL == pbHeaderBytes)
            {
                hr = E_OUTOFMEMORY;
                TraceOut("pbHeaderBytes alloc failed");
            }
            else
            {
                ZeroMemory(pbHeaderBytes, 1 + iLength);
            }
        }

        // Read N bytes
        if (SUCCEEDED(hr))
        {
            DWORD dwBytesRead = 0;

            bResult = ReadFile(hFile, (LPVOID)pbHeaderBytes, iLength, &dwBytesRead, NULL);

            if (!bResult)
            {
                dwError = GetLastError();
                hr = HRESULT_FROM_WIN32(dwError);
                TraceOut("ReadFile(3) failed, hr=0x%x", hr);
            }
            else
            {
                if (iLength != dwBytesRead)
                {
                    TraceOut("expected %d bytes, but got %u bytes", iLength, dwBytesRead);
                    hr = E_UNEXPECTED;
                }
            }
        }

        // Convert to UTF8 string
        CHAR *szHeader = (CHAR *)pbHeaderBytes;

        // Search for "<thumb"
        const CHAR *szThumbTag = "<thumb ";
        __int64 iThumbTagIndex = -1;
        if (SUCCEEDED(hr))
        {
            CHAR *szFoundHere = strstr(szHeader, szThumbTag);

            if (NULL == szFoundHere)
            {
                TraceOut("Did not find opening tag, \"%s\"", szThumbTag);
                hr = E_UNEXPECTED;
            }
            else
            {
                iThumbTagIndex = szFoundHere - szHeader;
            }
        }

        // Search for "gif=\""
        const char *szGifTag = "gif=\"";
        __int64 iGifTagIndex = -1;
        if (SUCCEEDED(hr))
        {
            CHAR *szFoundHere = strstr(szHeader + iThumbTagIndex + strlen(szThumbTag), szGifTag);

            if (NULL == szFoundHere)
            {
                TraceOut("Did not find gif tag, \"%s\"", szGifTag);
                hr = E_UNEXPECTED;
            }
            else
            {
                iGifTagIndex = szFoundHere - szHeader;
            }
        }

        // Search for "\""
        const char *szQuoteEnd = "\"";
        __int64 iQuoteEndIndex = -1;
        if (SUCCEEDED(hr))
        {
            CHAR *szFoundHere = strstr(szHeader + iGifTagIndex + strlen(szGifTag), szQuoteEnd);

            if (NULL == szFoundHere)
            {
                TraceOut("Did not find closing quote, \"%s\"", szQuoteEnd);
                hr = E_UNEXPECTED;
            }
            else
            {
                iQuoteEndIndex = szFoundHere - szHeader;
            }
        }

        // Stomp out the portion of the string that is the GIF in base64 format
        CHAR *szGifBase64 = NULL;
        int iGifBase64Len = -1;
        if (SUCCEEDED(hr))
        {
            szGifBase64 = szHeader + iGifTagIndex + strlen(szGifTag);
            szHeader[iQuoteEndIndex] = '\0';
            iGifBase64Len = (int)strlen(szGifBase64);
        }

        // Get required length of byte[] array for base64->byte[] conversion
        int nGifBytes = -1;
        if (SUCCEEDED(hr))
        {
            nGifBytes = Base64DecodeGetRequiredLength(iGifBase64Len);
        }

        // Allocate byte buffer for base64->byte[] conversion
        BYTE *pbGifBytes = NULL;
        if (SUCCEEDED(hr))
        {
            pbGifBytes = new BYTE[nGifBytes];

            if (NULL == pbGifBytes)
            {
                hr = E_OUTOFMEMORY;
                TraceOut("pbGifBytes alloc failed");
            }
            else
            {
                ZeroMemory(pbGifBytes, nGifBytes);
            }
        }

        // Convert from base64 to byte[]
        int iGifLen = -1;
        if (SUCCEEDED(hr))
        {
            int nDestLen = iGifBase64Len;

            bResult = Base64Decode(szGifBase64, iGifBase64Len, pbGifBytes, &nDestLen);

            if (!bResult)
            {
                TraceOut("Base64Decode failed");
                hr = E_FAIL;
            }
            else
            {
                iGifLen = nDestLen;
            }
        }

        // Wrap a memory stream around it
        CMemoryStream *pMemoryStream = NULL;
        if (SUCCEEDED(hr))
        {
            pMemoryStream = new CMemoryStream(pbGifBytes, iGifLen + 1);

            if (NULL == pMemoryStream)
            {
                TraceOut("pMemoryStream alloc failed");
                hr = E_OUTOFMEMORY;
            }
        }

        // Startup GDI+
        ULONG_PTR pGdiToken = NULL;
        if (SUCCEEDED(hr))
        {
            GdiplusStartupInput gdiplusStartupInput;
            Status status = GdiplusStartup(&pGdiToken, &gdiplusStartupInput, NULL);

            if (status != Ok)
            {
                hr = E_FAIL;
                pGdiToken = NULL;
                TraceOut("GdiplusStartup failed");
            }
        }

        // Load GIF
        Bitmap *pBitmap = NULL;        
        if (SUCCEEDED(hr))
        {
            pBitmap = Bitmap::FromStream((IStream *)pMemoryStream);

            if (NULL == pBitmap)
            {
                hr = E_FAIL;
                TraceOut("Bitmap::FromStream returned NULL");
            }
        }

        // Get HBITMAP from it
        HBITMAP hBitmap = NULL;
        if (SUCCEEDED(hr))
        {
            UINT width = pBitmap->GetWidth();
            UINT height = pBitmap->GetHeight();
            PixelFormat pf = pBitmap->GetPixelFormat();

            // HACK: if we don't do this, we get a Win32Error when we do GetHBITMAP ...
            //       ... which is odd because this call returns Win32Error.
            Color color;
            Status status1 = pBitmap->GetPixel(width - 1, height - 1, &color);

            Status status = pBitmap->GetHBITMAP(Color(0), &hBitmap);

            if (Ok != status)
            {
                if (Win32Error == status)
                {
                    dwError = GetLastError();
                    hr = HRESULT_FROM_WIN32(dwError);
                    TraceOut("pBitmap->GetHBITMAP failed, hr=0x%x", hr);
                }
                else
                {
                    hr = E_FAIL;
                }
            }
        }

        // Give bitmap to the caller!
        if (SUCCEEDED(hr))
        {
            *phBmpImage = hBitmap;
        }

        // Cleanup
        if (NULL != pBitmap)
        {
            delete pBitmap;
            pBitmap = NULL;
        }

        if (NULL != pGdiToken)
        {
            GdiplusShutdown(pGdiToken);
            pGdiToken = NULL;
        }

        if (NULL != pMemoryStream)
        {
            pMemoryStream->Release();
            pMemoryStream = NULL;
        }

        if (NULL != pbHeaderBytes)
        {
            delete [] pbHeaderBytes;
            pbHeaderBytes = NULL;
        }

        if (NULL != pbGifBytes)
        {
            delete [] pbGifBytes;
            pbGifBytes = NULL;
        }
    }

    if (!bPdn3File || FAILED(hr))
    {
        // Give generic PDN icon of some sort
        hr = E_FAIL;
    }    

    // Cleanup
    if (INVALID_HANDLE_VALUE != hFile)
    {
        CloseHandle(hFile);
        hFile = INVALID_HANDLE_VALUE;
    }

    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CPdnShellExtension::GetLocation(LPWSTR pszPathBuffer, 
                                             DWORD cchMax, 
                                             DWORD *pdwPriority, 
                                             const SIZE *prgSize, 
                                             DWORD dwRecClrDepth, 
                                             DWORD *pdwFlags)
{
    HRESULT hr = S_OK;
    TraceEnter();

    TraceOut("pszPathBuffer=%S", pszPathBuffer);
    TraceOut("cchMax=%u", cchMax);

    if (SUCCEEDED(hr))
    {
        if (NULL == pszPathBuffer)
        {
            TraceOut("pszPathBuffer is NULL");
            hr = E_INVALIDARG;
        }
    }

    if (SUCCEEDED(hr))
    {
        if (NULL == pdwPriority)
        {
            TraceOut("pdwPriority is NULL");
            hr = E_INVALIDARG;
        }
    }

    if (SUCCEEDED(hr))
    {
        if (NULL == pdwFlags)
        {
            TraceOut("pdwFlags is NULL");
            hr = E_INVALIDARG;
        }
    }

    if (SUCCEEDED(hr))
    {
        if (NULL == prgSize)
        {
            TraceOut("prgSize is NULL");
            hr = E_INVALIDARG;
        }
    }

    TraceOut("prgSize=%d x %d", prgSize->cx, prgSize->cy);

    if (SUCCEEDED(hr))
    {
        wcscpy(pszPathBuffer, m_bstrFileName);

        *pdwPriority = 1;

        if (*pdwFlags & IEIFLAG_ASPECT)
        {
            m_size = *prgSize;
        }
        else
        {
            m_size.cx = -1;
            m_size.cy = -1;
        }

        *pdwFlags |= IEIFLAG_CACHE;

        if (*pdwFlags & IEIFLAG_ASYNC)
        {
            hr = E_PENDING;
        }
        else
        {
            hr = NOERROR;
        }
    }

    TraceLeaveHr(hr);
    return hr;
}

