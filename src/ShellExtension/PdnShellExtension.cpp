/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
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

    if (NULL == ppvObject)
    {
        return E_INVALIDARG;
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

HRESULT DoGdiplusStartup(Status *pStatusResult, ULONG_PTR *pToken, GdiplusStartupInput *gdiplusStartupInput)
{
    // An exception may be thrown because we delay-load gdiplus.dll and
    // this is not installed on Win2K systems. Even if .NET is installed,
    // gdiplus.dll is not located in the system directory and thus is not
    // locatable by the loader.
	// This was moved into a separate function because of compiler error
	// C2712: "cannot use __try in functions that require object unwinding"

	Status status = Ok;
	HRESULT hr = S_OK;

	if (NULL == pStatusResult)
	{
		hr = E_INVALIDARG;
	}

	if (SUCCEEDED(hr))
	{
		__try
		{
			status = GdiplusStartup(pToken, gdiplusStartupInput, NULL);
		}

		__except (EXCEPTION_EXECUTE_HANDLER)
		{
			hr = E_FAIL;
			status = Win32Error;
		}
	}

	if (SUCCEEDED(hr))
	{
		*pStatusResult = status;
	}

	return hr;
}


// ReadFile does not guarantee that it will read all the bytes that you ask for.
// It may decide to read fewer bytes for whatever reason. This function is a 
// wrapper around ReadFile that loops until all the bytes you have asked for
// are read, or there was an error, or the end of file was reached. EOF is
// considered an error condition; when you ask for N bytes with this function
// you either get all N bytes, or an error.
static HRESULT ReadFileComplete(HANDLE hFile, LPVOID lpBuffer, DWORD nNumberOfBytesToRead)
{
    HRESULT hr = S_OK;

    while (SUCCEEDED(hr) && nNumberOfBytesToRead > 0)
    {
        DWORD dwBytesRead = 0;

        BOOL bResult = ReadFile(hFile, lpBuffer, nNumberOfBytesToRead, &dwBytesRead, NULL);

        if (!bResult)
        {
            DWORD dwError = GetLastError();
            hr = HRESULT_FROM_WIN32(dwError);
        }
        else if (bResult && 0 == dwBytesRead)
        {
            hr = HRESULT_FROM_WIN32(ERROR_HANDLE_EOF);
        }
        else
        {
            lpBuffer = (void *)((BYTE *)lpBuffer + dwBytesRead);
            nNumberOfBytesToRead -= dwBytesRead;
        }
    }
    
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
    BYTE bMagic[4];
    ZeroMemory(bMagic, sizeof(bMagic));

    if (SUCCEEDED(hr))
    {
        hr = ReadFileComplete(hFile, (LPVOID)bMagic, sizeof(bMagic));
    }

    if (SUCCEEDED(hr))
    {
        if ('P' == bMagic[0] &&
            'D' == bMagic[1] &&
            'N' == bMagic[2] &&
            '3' == bMagic[3])
        {
            bPdn3File = TRUE;
        }
    }
    else
    {
        TraceOut("ReadFile(1) failed, hr=0x%x", hr);
    }

    if (SUCCEEDED(hr) && bPdn3File)
    {
        TraceOut("we have a pdn3 file");

        TraceOut("Read + decode length");

        int iLength = -1;
        BYTE bLength[3];
        ZeroMemory(bLength, sizeof(bLength));

        if (SUCCEEDED(hr))
        {
            hr = ReadFileComplete(hFile, (LPVOID)bLength, sizeof(bLength));
        }

        if (SUCCEEDED(hr))
        {
            iLength = bLength[0] + (bLength[1] << 8) + (bLength[2] << 16);
        }
        else
        {
            TraceOut("ReadFile(2) failed, hr=0x%x", hr);
        }

        TraceOut("Allocate buffer");
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

        TraceOut("Read N bytes");
        if (SUCCEEDED(hr))
        {
            hr = ReadFileComplete(hFile, (LPVOID)pbHeaderBytes, iLength);
        }

        if (FAILED(hr))
        {
            TraceOut("ReadFile(3) failed, hr=0x%x", hr);
        }

        TraceOut("Convert to UTF8 string");
        CHAR *szHeader = (CHAR *)pbHeaderBytes;

        TraceOut("Search for \"<thumb\"");
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

        TraceOut("Search for \"gif=\"");
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

        TraceOut("Search for \"");
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

        TraceOut("Stomp out the portion of the string that is the GIF in base64 format");
        CHAR *szGifBase64 = NULL;
        int iGifBase64Len = -1;
        if (SUCCEEDED(hr))
        {
            szGifBase64 = szHeader + iGifTagIndex + strlen(szGifTag);
            szHeader[iQuoteEndIndex] = '\0';
            iGifBase64Len = (int)strlen(szGifBase64);
        }

        TraceOut("Get required length of byte[] array for base64->byte[] conversion");
        int nGifBytes = -1;
        if (SUCCEEDED(hr))
        {
            nGifBytes = Base64DecodeGetRequiredLength(iGifBase64Len);
        }

        TraceOut("Allocate byte buffer for base64->byte[] conversion");
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

        TraceOut("Convert from base64 to byte[]");
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

        TraceOut("Wrap a memory stream around it");
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

        TraceOut("Startup GDI+");
        ULONG_PTR pGdiToken = NULL;

        if (SUCCEEDED(hr))
        {
            GdiplusStartupInput gdiplusStartupInput;
            Status status;
            
			hr = DoGdiplusStartup(&status, &pGdiToken, &gdiplusStartupInput);

            if (status != Ok)
            {
                hr = E_FAIL;
                pGdiToken = NULL;
                TraceOut("GdiplusStartup failed");
            }
        }

        TraceOut("Load GIF");
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

        TraceOut("Get HBITMAP from it");
        HBITMAP hBitmap = NULL;
        if (SUCCEEDED(hr))
        {
            UINT width = pBitmap->GetWidth();
            UINT height = pBitmap->GetHeight();
            PixelFormat pf = pBitmap->GetPixelFormat();

            TraceOut("HACK: if we don't do this, we get a Win32Error when we do GetHBITMAP ...");
            TraceOut("       ... which is odd because this call returns Win32Error.");
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

        TraceOut("Give bitmap to the caller!");
        if (SUCCEEDED(hr))
        {
            *phBmpImage = hBitmap;
        }

        TraceOut("Cleanup");
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
        wcscpy_s(pszPathBuffer, cchMax, m_bstrFileName);

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

