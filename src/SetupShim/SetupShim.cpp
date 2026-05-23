/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

#define _WIN32_WINNT 0x0501
#include <tchar.h>
#include <windows.h>
#include <strsafe.h>

// .NET FX detection code from: http://blogs.msdn.com/astebner/archive/2004/09/18/231253.aspx
//              which links to: http://astebner.sts.winisp.net/Tools/detectFX.cpp.txt

const TCHAR *g_szNetfx11RegKeyName = _T("Software\\Microsoft\\NET Framework Setup\\NDP\\v1.1.4322");
const TCHAR *g_szNetfx20RegKeyName = _T("Software\\Microsoft\\NET Framework Setup\\NDP\\v2.0.50727");
const TCHAR *g_szNetfxRegValueName = _T("Install");
const TCHAR *g_szNetfxSPxRegValueName = _T("SP");

const TCHAR *g_szMessageBoxTitle = _T("Paint.NET");

const TCHAR *g_szNetfx20ButNoNetfx11Text =
    _T("[English] Setup has detected that the .NET Framework 2.0 is installed. "
                 "However, Paint.NET requires the .NET Framework 1.1. "
                 "Note that both of these may be installed at the same time without loss "
                 "of functionality, and that they are designed to operate together in this manner. "
                 "A future release of Paint.NET, planned for mid-January 2006, will have support "
                 "for the .NET Framework 2.0, as well as full 64-bit support.\n"
                 "\n"
                 "Click OK to go to Microsoft's webpage where you may download and install the .NET Framework 1.1.\n"
       "\n"
       "[Deutsch] Auf ihrem Computer ist das .NET Framework 2.0 installiert. "
	   "Paint.NET benötigt jedoch das .NET Framework 1.1. "
	   "Bitte beachten sie, dass beide Versionen ohne Beeinträchtigung der Funktionalität "
	   "gleichzeitig auf einem Computer installiert sein können und dies sogar ausdrücklich vorgesehen ist. "
	   "Die für Mitte Januar 2006 geplante Version von Paint.NET wird schließlich sowohl auf "
	   "dem .NET Framework 2.0 lauffähig sein als auch 64-bit Plattformen unterstützen.\n"
	   "\n"
	   "Klicken sie bitte auf \"OK\" um auf die Microsoft Website weitergeleitet zu werden, "
	   "von der sie das .NET Framework 1.1 downloaden und installieren können.");

const TCHAR *g_szNetfx11NotFoundText = 
    _T("[English] Paint.NET requires that the .NET Framework 1.1 is installed. Click OK to go to Microsoft's webpage where you may download and install this.\n"
       "\n"
       "[Deutsch] Paint.NET setzt die Installation des .NET Frameworks 1.1 voraus. Klicken sie auf OK um die Microsoft Homepage zu öffnen und das Framework zu downloaden.");

const TCHAR *g_szNetfxInstallFailureTextFormat =
    _T("[English] Installation of the .NET Framework failed with the following error code: %d\n"
       "\n"
       "[Deutsch] Die Installation des .NET Frameworks ist mit folgendem Fehlercode fehlgeschlagen: %d");

// This isn't the most user-friendly way to get the system restarted.
// However, this should not affect many people as .NET generally doesn't
// require a restart after installation.
const TCHAR *g_szNetfxInstallRebootRequired =
    _T("[English] You must now manually restart your computer before continuing with the installation of Paint.NET.\n"
       "\n"
       "[Deutsch] Sie müssen ihren Computer nun neu starten bevor sie mit der Installation von Paint.NET fortfahren können.");

const TCHAR *g_szNetfx11DownloadUrl = _T("http://go.microsoft.com/fwlink/?LinkId=17153");
const TCHAR *g_szNetfxInstallerFileName = _T("dotnetfx.exe");
const TCHAR *g_szNetfxSP1InstallerFileName = _T("NDP1.1sp1-KB867460-X86.exe");
const TCHAR *g_szNetfxSP12k3x86InstallerFileName = _T("WindowsServer2003-KB867460-x86-ENU.EXE");

const TCHAR *g_szPdnInstallerFileName = _T("SetupFrontEnd.exe");

BOOL RegistryGetValue(HKEY hk, const TCHAR* pszKey, const TCHAR* pszValue, 
                      DWORD dwType, LPBYTE data, DWORD dwSize)
{
    HKEY hkOpened;

    // Try to open the key
    if (RegOpenKeyEx(hk, pszKey, 0, KEY_READ, &hkOpened) != ERROR_SUCCESS)
    {
        return false;
    }

    // If the key was opened, try to retrieve the value
    if (RegQueryValueEx(hkOpened, pszValue, 0, &dwType, (LPBYTE)data, &dwSize) != ERROR_SUCCESS)
    {
        RegCloseKey(hkOpened);
        return false;
    }
    
    // Clean up
    RegCloseKey(hkOpened);

    return true;
}

bool IsNetfx11Installed()
{
    bool bRetValue = false;
    DWORD dwRegValue = 0;

    if (RegistryGetValue(HKEY_LOCAL_MACHINE, g_szNetfx11RegKeyName, g_szNetfxRegValueName, NULL, (LPBYTE)&dwRegValue, sizeof(DWORD)))
    {
        if (1 == dwRegValue)
        {
            bRetValue = true;
        }
    }

    return bRetValue;
}

bool IsNetfx20Installed()
{
    bool bRetValue = false;
    DWORD dwRegValue = 0;

    if (RegistryGetValue(HKEY_LOCAL_MACHINE, g_szNetfx20RegKeyName, g_szNetfxRegValueName, NULL, (LPBYTE)&dwRegValue, sizeof(DWORD)))
    {
        if (1 == dwRegValue)
        {
            bRetValue = true;
        }
    }

    return bRetValue;
}

bool FileExists(const TCHAR* pszFileName)
{
    return INVALID_FILE_ATTRIBUTES != GetFileAttributes(pszFileName);
}

// Returns 0xffffffff on failure, 0 if bWait is false and the operation was successful,
// or the exit code of the process launched if bWait is true and the operation was successful
DWORD OurShellExecute(const TCHAR* pszFileName, const TCHAR* pszParameters, bool bWait)
{
    DWORD dwRetVal = 0;
    SHELLEXECUTEINFO sei;

    ZeroMemory(&sei, sizeof(sei));
    sei.cbSize = sizeof(sei);
    sei.fMask = bWait ? SEE_MASK_NOCLOSEPROCESS : 0;
    sei.lpVerb = _T("open");
    sei.lpFile = pszFileName;
    sei.lpParameters = pszParameters;
    sei.nShow = SW_SHOWNORMAL;

    BOOL bResult = ShellExecuteEx(&sei);

    if (!bResult)
    {
        dwRetVal = 0xffffffff;
    }
    else
    {
        if (!bWait)
        {
            dwRetVal = 0;
        }
        else 
        {
            if (WAIT_OBJECT_0 == WaitForSingleObject(sei.hProcess, INFINITE))
            {
                DWORD dwExitCode = 0;
                BOOL bResult2 = GetExitCodeProcess(sei.hProcess, &dwExitCode);

                if (bResult2)
                {
                    dwRetVal = dwExitCode;
                }
                else
                {
                    dwRetVal = 0xffffffff;
                }
            }
            else
            {
                dwRetVal = 0xffffffff;
            }

            CloseHandle(sei.hProcess);
            sei.hProcess = NULL;
        }
    }

    return dwRetVal;
}

typedef BOOL (* IsWow64ProcessFnPtr)(HANDLE hProcess, PBOOL Wow64Process);

BOOL IsWindowsX64(void)
{
    HMODULE hKernel32 = LoadLibrary(_T("kernel32.dll"));

    IsWow64ProcessFnPtr iwpfp = NULL;
    if (NULL != hKernel32)
    {
        iwpfp = (IsWow64ProcessFnPtr)GetProcAddress(hKernel32, _T("IsWow64Process"));
    }

    BOOL bIsWow64 = FALSE;
    if (NULL != iwpfp)
    {
        BOOL bResult = iwpfp(GetCurrentProcess(), &bIsWow64);

        if (!bResult)
        {
            bIsWow64 = FALSE;
        }
    }

    if (NULL != hKernel32)
    {
        FreeLibrary(hKernel32);
        hKernel32 = NULL;
    }

    return bIsWow64;
}

BOOL IsWindows2k3(void)
{
    OSVERSIONINFO ovi;
    ZeroMemory(&ovi, sizeof(ovi));
    ovi.dwOSVersionInfoSize = sizeof(ovi);
    BOOL bResult = ::GetVersionEx(&ovi);

    if (!bResult)
    {
        return FALSE;
    }

    return (5 == ovi.dwMajorVersion && 2 == ovi.dwMinorVersion);
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
{
    int nReturnVal;

    // Stage 1: Figure out the situation with the installation of .NET
    if (IsNetfx11Installed())
    {
        // .NET's already installed, our job is done!
        nReturnVal = 0;
    }
    else
    {
        // .NET 1.1 is not installed.
        if (FileExists(g_szNetfxInstallerFileName))
        {
            // .NET installer is present, so let's try and install it
            DWORD dwResult = OurShellExecute(g_szNetfxInstallerFileName, NULL, true);

            switch (dwResult)
            {
                // Success
                case 0:    
                    nReturnVal = 0;

                    // Now we should try to install .NET 1.1 SP1.
                    // However, Windows Server 2003 (x86) requires a different redist, so we
                    // must check for that and act appropriately.
                    BOOL bIsWin2k3;
                    bIsWin2k3 = IsWindows2k3();

                    BOOL bIsX64;
                    bIsX64 = IsWindowsX64();

                    const TCHAR *szSP1InstallerName;
                    const TCHAR *szArgs;

                    if (bIsWin2k3 && !bIsX64)
                    {
                        szSP1InstallerName = g_szNetfxSP12k3x86InstallerFileName;
                        szArgs = _T("/passive");
                    }
                    else
                    {
                        szSP1InstallerName = g_szNetfxSP1InstallerFileName;
                        szArgs = _T("/I");
                    }

                    if (FileExists(szSP1InstallerName))
                    {
                        DWORD dwResult = OurShellExecute(szSP1InstallerName, szArgs, true);

                        if (0 != dwResult)
                        {
                            nReturnVal = 1;
                        }
                    }

                    break;

                // Reboot required
                case 8192: 
                    MessageBox(NULL, g_szNetfxInstallRebootRequired, g_szMessageBoxTitle, MB_OK | MB_ICONINFORMATION);
                    nReturnVal = 1;
                    break;

                // Other. Interpreted as failure.
                default:   
                    {
                        TCHAR szErrorText[1024];
                        HRESULT hr = StringCchPrintf(
                            szErrorText, 
                            sizeof(szErrorText) / sizeof(szErrorText[0]), 
                            g_szNetfxInstallFailureTextFormat,
                            dwResult,
                            dwResult);

                        if (SUCCEEDED(hr))
                        {
                            MessageBox(NULL, szErrorText, g_szMessageBoxTitle, MB_OK | MB_ICONERROR);
                        }
                    }

                    nReturnVal = 1;
                    break;
            }
        }
        else
        {
            const TCHAR* szText = g_szNetfx11NotFoundText;

            if (IsNetfx20Installed())
            {
                szText = g_szNetfx20ButNoNetfx11Text;
            }

            // .NET not installed, and the installer isn't around. Ask them to go to a website in order to install it.
            int nResult = MessageBox(NULL, szText, g_szMessageBoxTitle, MB_OKCANCEL | MB_ICONERROR);

            if (IDOK == nResult)
            {
                OurShellExecute(g_szNetfx11DownloadUrl, NULL, false);
            }

            nReturnVal = 1;
        }
    }

    // Stage 2: Launch our setup app!
    if (0 == nReturnVal)
    {
        OurShellExecute(g_szPdnInstallerFileName, GetCommandLine(), true);
    }

    return nReturnVal;
}