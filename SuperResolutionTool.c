#define UNICODE
#define _UNICODE

#include <windows.h>
#include <commctrl.h>
#include <stdio.h>
#include <stdlib.h>
#include <shlwapi.h>
#include <strsafe.h>
#include <time.h>
#include <dbghelp.h>

#pragma comment(lib, "comctl32.lib")
#pragma comment(lib, "shlwapi.lib")
#pragma comment(lib, "dbghelp.lib")

#pragma comment(linker, "/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")

// Global Variables
HINSTANCE hInst;
HWND hMainWnd;
HWND hMonitorComboBox;
HWND hScaleTrackbar;
HWND hNewResolutionLabel;
HWND hDesktopResolutionLabel;
HWND hActiveResolutionLabel;
HWND hMagnificationLabel;
HWND hDisplaySettingsButton;
HWND hRebootButton;
HWND hBackupButton;
HWND hApplyButton;
HWND hAboutButton;

WCHAR selectedMonitor[256] = L"";
int selectedMonitorIndex = 0;
int magnification = 150; // Default magnification
int originalDesktopWidth = 0;
int originalDesktopHeight = 0;
int activeSignalWidth = 0;
int activeSignalHeight = 0;
int newDesktopWidth = 0;
int newDesktopHeight = 0;

// Function Prototypes
LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow);
BOOL RegisterWindowClass(HINSTANCE hInstance);
void PopulateMonitorList();
void UpdateResolutionInfo();
void CalculateNewResolution();
void BackupRegistry();
BOOL ApplyResolutionChanges();
void ShowAboutDialog();
void DebugPrint(const wchar_t *format, ...);
void CenterWindow(HWND hwnd);
void PositionButtons(HWND hwnd);
BOOL FileExists(LPCTSTR szPath);
void RestartComputer();
void OpenDisplaySettings();
void DisplayErrorMessage(const wchar_t* message, const wchar_t* caption);

// Entry Point
int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, PWSTR pCmdLine, int nCmdShow) {
    InitCommonControls();
    if (!RegisterWindowClass(hInstance)) return 1;
    if (!InitInstance(hInstance, nCmdShow)) return 1;
    MSG msg;
    while (GetMessage(&msg, NULL, 0, 0)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
    return (int)msg.wParam;
}

BOOL RegisterWindowClass(HINSTANCE hInstance) {
    WNDCLASS wc = { 0 };
    wc.lpfnWndProc = WindowProc;
    wc.hInstance = hInstance;
    wc.hCursor = LoadCursor(NULL, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_BTNFACE + 1);
    wc.lpszClassName = L"SuperResolutionToolClass";
    return RegisterClass(&wc) != 0;
}

BOOL InitInstance(HINSTANCE hInstance, int nCmdShow) {
    hInst = hInstance;
    hMainWnd = CreateWindowEx(0, L"SuperResolutionToolClass", L"Super Resolution Tool v1.0",
        WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX,
        CW_USEDEFAULT, CW_USEDEFAULT, 480, 340, NULL, NULL, hInstance, NULL);
    if (!hMainWnd) return FALSE;
    CenterWindow(hMainWnd);
    ShowWindow(hMainWnd, nCmdShow);
    return TRUE;
}

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
    switch (uMsg) {
    case WM_CREATE: {
        CreateWindow(L"STATIC", L"Global \"Super Resolution, Super Display Zoom\"", WS_VISIBLE | WS_CHILD | SS_LEFT, 20, 10, 440, 20, hwnd, NULL, hInst, NULL);
        CreateWindow(L"STATIC", L"Select Monitor:", WS_VISIBLE | WS_CHILD | SS_LEFT, 20, 40, 100, 20, hwnd, NULL, hInst, NULL);
        hMonitorComboBox = CreateWindow(L"COMBOBOX", L"", WS_VISIBLE | WS_CHILD | CBS_DROPDOWNLIST | WS_VSCROLL, 130, 40, 310, 200, hwnd, NULL, hInst, NULL);
        CreateWindow(L"STATIC", L"Magnification:", WS_VISIBLE | WS_CHILD | SS_LEFT, 20, 70, 100, 20, hwnd, NULL, hInst, NULL);
        hScaleTrackbar = CreateWindow(TRACKBAR_CLASS, L"", WS_VISIBLE | WS_CHILD | TBS_AUTOTICKS | TBS_HORZ, 130, 70, 250, 30, hwnd, NULL, hInst, NULL);
        hMagnificationLabel = CreateWindow(L"STATIC", L"150%", WS_VISIBLE | WS_CHILD | SS_LEFT, 390, 70, 50, 20, hwnd, NULL, hInst, NULL);
        CreateWindow(L"STATIC", L"New Desktop Res:", WS_VISIBLE | WS_CHILD | SS_LEFT, 20, 110, 120, 20, hwnd, NULL, hInst, NULL);
        hNewResolutionLabel = CreateWindow(L"STATIC", L"", WS_VISIBLE | WS_CHILD | SS_LEFT, 150, 110, 200, 20, hwnd, NULL, hInst, NULL);
        CreateWindow(L"STATIC", L"Current Desktop Res:", WS_VISIBLE | WS_CHILD | SS_LEFT, 20, 140, 130, 20, hwnd, NULL, hInst, NULL);
        hDesktopResolutionLabel = CreateWindow(L"STATIC", L"", WS_VISIBLE | WS_CHILD | SS_LEFT, 150, 140, 200, 20, hwnd, NULL, hInst, NULL);
        CreateWindow(L"STATIC", L"Active Signal Res:", WS_VISIBLE | WS_CHILD | SS_LEFT, 20, 170, 130, 20, hwnd, NULL, hInst, NULL);
        hActiveResolutionLabel = CreateWindow(L"STATIC", L"", WS_VISIBLE | WS_CHILD | SS_LEFT, 150, 170, 200, 20, hwnd, NULL, hInst, NULL);

        // Create buttons, but don't set their positions yet
        hDisplaySettingsButton = CreateWindow(L"BUTTON", L"Display Settings", WS_VISIBLE | WS_CHILD | BS_PUSHBUTTON, 0, 0, 140, 30, hwnd, (HMENU)1, hInst, NULL);
        hRebootButton = CreateWindow(L"BUTTON", L"Reboot", WS_VISIBLE | WS_CHILD | BS_PUSHBUTTON, 0, 0, 120, 30, hwnd, (HMENU)2, hInst, NULL);
        hBackupButton = CreateWindow(L"BUTTON", L"Backup", WS_VISIBLE | WS_CHILD | BS_PUSHBUTTON, 0, 0, 120, 30, hwnd, (HMENU)3, hInst, NULL);
        hApplyButton = CreateWindow(L"BUTTON", L"Apply", WS_VISIBLE | WS_CHILD | BS_PUSHBUTTON, 0, 0, 120, 30, hwnd, (HMENU)4, hInst, NULL);
        hAboutButton = CreateWindow(L"BUTTON", L"About and Help", WS_VISIBLE | WS_CHILD | BS_PUSHBUTTON, 0, 0, 140, 30, hwnd, (HMENU)5, hInst, NULL);

        SendMessage(hScaleTrackbar, TBM_SETRANGE, (WPARAM)TRUE, (LPARAM)MAKELONG(100, 350));
        SendMessage(hScaleTrackbar, TBM_SETPOS, (WPARAM)TRUE, (LPARAM)magnification);
        SendMessage(hScaleTrackbar, TBM_SETTICFREQ, (WPARAM)10, (LPARAM)0);
        PopulateMonitorList();
        UpdateResolutionInfo();
        CalculateNewResolution();
        PositionButtons(hwnd); // Call PositionButtons once in WM_CREATE
        break;
    }
    case WM_COMMAND: {
        int wmId = LOWORD(wParam);
        switch (wmId) {
            case 1: OpenDisplaySettings(); break;
            case 2: RestartComputer();    break;
            case 3: BackupRegistry();     break;
            case 4:
                if (ApplyResolutionChanges()) {
                    int msgboxID = MessageBox(hwnd, L"Registry updated. Restart your computer for changes to take effect.\nRestart now?", L"Success", MB_YESNO | MB_ICONINFORMATION);
                    if (msgboxID == IDYES) RestartComputer();
                }
                break;
            case 5: ShowAboutDialog(); break;
        }
        if (HIWORD(wParam) == CBN_SELCHANGE && (HWND)lParam == hMonitorComboBox) {
            selectedMonitorIndex = (int)SendMessage(hMonitorComboBox, CB_GETCURSEL, 0, 0);
            if (selectedMonitorIndex != CB_ERR) {
                SendMessage(hMonitorComboBox, CB_GETLBTEXT, selectedMonitorIndex, (LPARAM)selectedMonitor);
                DebugPrint(L"Selected Monitor: %s\n", selectedMonitor);
                UpdateResolutionInfo();
                CalculateNewResolution();
            }
        }
        break;
    }
    case WM_HSCROLL: {
        if ((HWND)lParam == hScaleTrackbar) {
            magnification = (int)SendMessage(hScaleTrackbar, TBM_GETPOS, 0, 0);
            magnification = (magnification / 10) * 10;
            SendMessage(hScaleTrackbar, TBM_SETPOS, (WPARAM)TRUE, (LPARAM)magnification);
            WCHAR magnificationStr[20];
            StringCchPrintf(magnificationStr, 20, L"%d%%", magnification);
            SetWindowText(hMagnificationLabel, magnificationStr);
            CalculateNewResolution();
        }
        break;
    }
    case WM_SIZE: // Reposition buttons when the window size changes
        PositionButtons(hwnd);
        break;

    case WM_DESTROY:
        PostQuitMessage(0);
        break;
    default:
        return DefWindowProc(hwnd, uMsg, wParam, lParam);
    }
    return 0;
}

void PopulateMonitorList() {
    HKEY hKey;
    LSTATUS status = RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers\\Configuration", 0, KEY_READ, &hKey);

    if (status != ERROR_SUCCESS) {
        DisplayErrorMessage(L"Failed to open registry key: HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers\\Configuration", L"Error");
        DebugPrint(L"RegOpenKeyEx failed with error %d\n", status);
        return;
    }

    DWORD index = 0;
    WCHAR subkeyName[256];
    DWORD subkeyNameLength = 256;

    while (RegEnumKeyEx(hKey, index, subkeyName, &subkeyNameLength, NULL, NULL, NULL, NULL) == ERROR_SUCCESS) {
        SendMessage(hMonitorComboBox, CB_ADDSTRING, 0, (LPARAM)subkeyName);
        if(index == 0)
        {
           StringCchCopy(selectedMonitor, 256, subkeyName);
        }
        index++;
        subkeyNameLength = 256;
    }

    RegCloseKey(hKey);
    SendMessage(hMonitorComboBox, CB_SETCURSEL, 0, 0);
}

void UpdateResolutionInfo() {
    if (wcslen(selectedMonitor) == 0) {
         DebugPrint(L"No monitor selected.\n");
        return;
    }

    HKEY hKey;
    WCHAR regPath[512];

    StringCchPrintf(regPath, 512, L"SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers\\Configuration\\%s\\00\\00", selectedMonitor);
    DebugPrint(L"Reading PrimSurfSize from: %s\n", regPath);

    if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, regPath, 0, KEY_READ, &hKey) == ERROR_SUCCESS) {
        DWORD dataSize = sizeof(DWORD);

        if (RegQueryValueEx(hKey, L"PrimSurfSize.cx", NULL, NULL, (LPBYTE)&originalDesktopWidth, &dataSize) != ERROR_SUCCESS) {
              DebugPrint(L"Failed to read PrimSurfSize.cx\n");
        }

        if (RegQueryValueEx(hKey, L"PrimSurfSize.cy", NULL, NULL, (LPBYTE)&originalDesktopHeight, &dataSize) != ERROR_SUCCESS) {
             DebugPrint(L"Failed to read PrimSurfSize.cy\n");
        }
        RegCloseKey(hKey);
        DebugPrint(L"PrimSurfSize.cx: %d, PrimSurfSize.cy: %d\n", originalDesktopWidth, originalDesktopHeight);

    }
    else{
        DebugPrint(L"Failed to open registry key: %s\n", regPath);
    }

     StringCchPrintf(regPath, 512, L"SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers\\Configuration\\%s\\00\\00", selectedMonitor);
       DebugPrint(L"Reading ActiveSize from: %s\n", regPath);

    if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, regPath, 0, KEY_READ, &hKey) == ERROR_SUCCESS) {
        DWORD dataSize = sizeof(DWORD);
        if (RegQueryValueEx(hKey, L"ActiveSize.cx", NULL, NULL, (LPBYTE)&activeSignalWidth, &dataSize) != ERROR_SUCCESS)
        {
             DebugPrint(L"Failed to read ActiveSize.cx\n");
        }
        if (RegQueryValueEx(hKey, L"ActiveSize.cy", NULL, NULL, (LPBYTE)&activeSignalHeight, &dataSize) != ERROR_SUCCESS){
              DebugPrint(L"Failed to read ActiveSize.cy\n");
        }
        RegCloseKey(hKey);
         DebugPrint(L"ActiveSize.cx: %d, ActiveSize.cy: %d\n", activeSignalWidth, activeSignalHeight);
    }
      else{
        DebugPrint(L"Failed to open registry key: %s\n", regPath);
    }

    WCHAR resolutionText[256];
    StringCchPrintf(resolutionText, 256, L"%d x %d", originalDesktopWidth, originalDesktopHeight);
    SetWindowText(hDesktopResolutionLabel, resolutionText);

    StringCchPrintf(resolutionText, 256, L"%d x %d", activeSignalWidth, activeSignalHeight);
    SetWindowText(hActiveResolutionLabel, resolutionText);
}

void CalculateNewResolution() {
    if(activeSignalWidth == 0 || activeSignalHeight == 0) {
        return;
    }

    newDesktopWidth = (int)((double)activeSignalWidth * magnification / 100.0 + 0.5);
    newDesktopHeight = (int)((double)activeSignalHeight * magnification / 100.0 + 0.5);

     WCHAR resolutionText[256];
    StringCchPrintf(resolutionText, 256, L"%d x %d", newDesktopWidth, newDesktopHeight);
    SetWindowText(hNewResolutionLabel, resolutionText);
    DebugPrint(L"New resolution calculated: %d x %d\n", newDesktopWidth, newDesktopHeight);
}
void BackupRegistry() {
    WCHAR filename[MAX_PATH];
    time_t t = time(NULL);
    struct tm tm;
    localtime_s(&tm, &t);

    wcsftime(filename, sizeof(filename) / sizeof(WCHAR), L"backup_%Y%m%d_%H%M%S.reg", &tm);

    WCHAR currentDir[MAX_PATH];
    if (GetCurrentDirectory(MAX_PATH, currentDir) == 0) {
        DisplayErrorMessage(L"Failed to get current directory.", L"Error");
        DebugPrint(L"Failed to get current directory. Error Code: %lu\n", GetLastError());
        return;
    }

    WCHAR fullPath[MAX_PATH];
    StringCchPrintf(fullPath, MAX_PATH, L"%s\\%s", currentDir, filename);
    DebugPrint(L"Backup Full Path: %s\n", fullPath);

    WCHAR command[1024];
    StringCchPrintf(command, 1024, L"reg export \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers\\Configuration\" \"%s\" /y", fullPath);

    STARTUPINFOW si;
    PROCESS_INFORMATION pi;

    ZeroMemory(&si, sizeof(si));
    si.cb = sizeof(si);
    ZeroMemory(&pi, sizeof(pi));

    if (CreateProcessW(NULL, command, NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &si, &pi)) {
        WaitForSingleObject(pi.hProcess, INFINITE);
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);

        WCHAR message[MAX_PATH + 100];
        StringCchPrintf(message, MAX_PATH + 100, L"Registry backup created successfully:\n%s", fullPath);
        MessageBox(hMainWnd, message, L"Backup Success", MB_OK | MB_ICONINFORMATION);
        DebugPrint(L"Registry backup created successfully.\n");

    } else {
        DWORD errorCode = GetLastError();
        WCHAR errorMsg[256];
        StringCchPrintf(errorMsg, 256, L"Failed to create backup. Error code: %lu", errorCode);
        MessageBox(hMainWnd, errorMsg, L"Backup Error", MB_ICONERROR | MB_OK);
        DebugPrint(L"Failed to create backup. Error code: %lu\n", errorCode);
    }
}
BOOL ApplyResolutionChanges() {
     if (wcslen(selectedMonitor) == 0) {
        MessageBox(hMainWnd, L"Please select a monitor.", L"Error", MB_OK | MB_ICONERROR);
        return FALSE;
    }

    WCHAR confirmMessage[1024];
    StringCchPrintf(confirmMessage, 1024,
        L"You are about to change the resolution for the following monitor:\n\n"
        L"Monitor: %s\n"
        L"Current Desktop Resolution: %d x %d\n"
        L"New Desktop Resolution: %d x %d\n"
        L"Active Signal Resolution: %d x %d (unchanged)\n\n"
        L"WARNING: Changing display settings can potentially cause display issues.  Ensure you have a backup.\n\n"
        L"Do you want to continue?",
        selectedMonitor, originalDesktopWidth, originalDesktopHeight, newDesktopWidth, newDesktopHeight, activeSignalWidth, activeSignalHeight);

    int result = MessageBox(hMainWnd, confirmMessage, L"Confirm Resolution Change", MB_YESNO | MB_ICONWARNING);

    if (result != IDYES) {
        return FALSE;
    }

    HKEY hKey;
    WCHAR regPath[512];
    LSTATUS status;

    StringCchPrintf(regPath, 512, L"SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers\\Configuration\\%s\\00\\00", selectedMonitor);
    DebugPrint(L"Writing to registry: %s\n", regPath);

    status = RegOpenKeyEx(HKEY_LOCAL_MACHINE, regPath, 0, KEY_WRITE, &hKey);
    if (status != ERROR_SUCCESS) {
         WCHAR errorMessage[256];
         StringCchPrintf(errorMessage, 256, L"Failed to open registry key: %s, Error code: %lu", regPath, GetLastError());
         DisplayErrorMessage(errorMessage, L"Error");
        DebugPrint(L"Failed to open registry key: %s, Error code: %lu\n", regPath, GetLastError());
        return FALSE;
    }

    status = RegSetValueEx(hKey, L"PrimSurfSize.cx", 0, REG_DWORD, (const BYTE*)&newDesktopWidth, sizeof(DWORD));
    if (status != ERROR_SUCCESS) {
         WCHAR errorMessage[256];
         StringCchPrintf(errorMessage, 256, L"Failed to write PrimSurfSize.cx to: %s. Error Code: %lu", regPath, GetLastError());
        RegCloseKey(hKey);
        DisplayErrorMessage(errorMessage, L"Error");
        DebugPrint(L"Failed to set PrimSurfSize.cx, Error code: %lu\n", GetLastError());
        return FALSE;
    }

    status = RegSetValueEx(hKey, L"PrimSurfSize.cy", 0, REG_DWORD, (const BYTE*)&newDesktopHeight, sizeof(DWORD));
    if (status != ERROR_SUCCESS) {
         WCHAR errorMessage[256];
         StringCchPrintf(errorMessage, 256, L"Failed to write PrimSurfSize.cy to: %s. Error Code: %lu", regPath, GetLastError());
        RegCloseKey(hKey);
        DisplayErrorMessage(errorMessage, L"Error");
         DebugPrint(L"Failed to set PrimSurfSize.cy, Error code: %lu\n", GetLastError());
        return FALSE;
    }
    RegCloseKey(hKey);

      StringCchPrintf(regPath, 512, L"SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers\\Configuration\\%s\\00", selectedMonitor);
       DebugPrint(L"Writing to registry: %s\n", regPath);
    status = RegOpenKeyEx(HKEY_LOCAL_MACHINE, regPath, 0, KEY_WRITE, &hKey);
      if (status != ERROR_SUCCESS) {
        WCHAR errorMessage[256];
         StringCchPrintf(errorMessage, 256, L"Failed to open registry key: %s, Error code: %lu", regPath, GetLastError());
         DisplayErrorMessage(errorMessage, L"Error");
        DebugPrint(L"Failed to open registry key: %s, Error code: %lu\n", regPath, GetLastError());
        return FALSE;
    }
    status = RegSetValueEx(hKey, L"PrimSurfSize.cx", 0, REG_DWORD, (const BYTE*)&newDesktopWidth, sizeof(DWORD));
    if (status != ERROR_SUCCESS) {
          WCHAR errorMessage[256];
        StringCchPrintf(errorMessage, 256, L"Failed to write PrimSurfSize.cx to: %s.  Error Code: %lu", regPath, GetLastError());
        RegCloseKey(hKey);
        DisplayErrorMessage(errorMessage, L"Error");
        DebugPrint(L"Failed to set PrimSurfSize.cx, Error code: %lu\n", GetLastError());
        return FALSE;
    }
    status = RegSetValueEx(hKey, L"PrimSurfSize.cy", 0, REG_DWORD, (const BYTE*)&newDesktopHeight, sizeof(DWORD));

    if (status != ERROR_SUCCESS) {
        WCHAR errorMessage[256];
        StringCchPrintf(errorMessage, 256, L"Failed to write PrimSurfSize.cy to: %s.  Error Code: %lu", regPath, GetLastError());
        RegCloseKey(hKey);
        DisplayErrorMessage(errorMessage, L"Error");
        DebugPrint(L"Failed to set PrimSurfSize.cy, Error code: %lu\n", GetLastError());
        return FALSE;
    }
    RegCloseKey(hKey);

    return TRUE;
}

void ShowAboutDialog() {
     MessageBox(hMainWnd,
        L"Super Resolution Tool\n"
        L"Version: v1.0\n"
        L"Author: CXT\n"
        L"Website: https://www.cxthhhhh.com\n\n"
        L"Global \"super resolution, super display scaling\", similar to NVIDIA DLSS, AMD FSR, Intel XeSS and Microsoft DirectSR.\n"
        L"Increases the content displayed on the screen while keeping the resolution of the output signal unchanged.\n\n"
        L"How to use:\n"
        L"1. Run SuperResolutionTool.exe as administrator.\n"
        L"2. Select the monitor you want to modify from the dropdown list.\n"
        L"3. Adjust the magnification using the trackbar. The new desktop resolution will be displayed.\n"
        L"4. (Highly Recommended) Click the 'Backup' button to save the current registry settings.\n"
        L"5. Click the 'Apply' button. A confirmation dialog will appear. Review the changes and click 'Yes' to proceed.\n"
        L"6. Restart your computer.  You can also click 'Reboot'.\n"
        L"7. After restarting, go to Windows Settings -> System -> Display, select the modified monitor, and choose the new resolution from the 'Display resolution' dropdown.\n"
        L"   You can also click 'Display Settings' to quickly open it.\n\n"
        L"WARNING: Modifying display settings can potentially cause display issues. Always back up your registry before applying changes.",
        L"About & Help",
        MB_OK | MB_ICONINFORMATION);
}

void DebugPrint(const wchar_t *format, ...) {
    va_list args;
    va_start(args, format);
    WCHAR buffer[1024];
    vswprintf_s(buffer, sizeof(buffer) / sizeof(buffer[0]), format, args);
    OutputDebugStringW(buffer);
    va_end(args);
}

void CenterWindow(HWND hwnd) {
    RECT rect;
    GetWindowRect(hwnd, &rect);
    int screenWidth = GetSystemMetrics(SM_CXSCREEN);
    int screenHeight = GetSystemMetrics(SM_CYSCREEN);
    int windowWidth = rect.right - rect.left;
    int windowHeight = rect.bottom - rect.top;
    int x = (screenWidth - windowWidth) / 2;
    int y = (screenHeight - windowHeight) / 2;
    SetWindowPos(hwnd, NULL, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
}

// Function to position the buttons
void PositionButtons(HWND hwnd) {
    RECT rcClient;
    GetClientRect(hwnd, &rcClient);
    int buttonSpacing = 10; // Spacing between buttons
    int yPosRow1 = 210;
    int yPosRow2 = 250;

    // First row of buttons
    int totalWidthRow1 = 140 + buttonSpacing + 120; // Display Settings + spacing + Reboot
    int xStartRow1 = (rcClient.right - totalWidthRow1) / 2;
    SetWindowPos(hDisplaySettingsButton, NULL, xStartRow1, yPosRow1, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
    SetWindowPos(hRebootButton, NULL, xStartRow1 + 140 + buttonSpacing, yPosRow1, 0, 0, SWP_NOSIZE | SWP_NOZORDER);

    // Second row of buttons
    int totalWidthRow2 = 120 + buttonSpacing + 120 + buttonSpacing + 140; // Backup + spacing + Apply + spacing + About
    int xStartRow2 = (rcClient.right - totalWidthRow2) / 2;
    SetWindowPos(hBackupButton, NULL, xStartRow2, yPosRow2, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
    SetWindowPos(hApplyButton, NULL, xStartRow2 + 120 + buttonSpacing, yPosRow2, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
    SetWindowPos(hAboutButton, NULL, xStartRow2 + 120 + buttonSpacing + 120 + buttonSpacing, yPosRow2, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
}

BOOL FileExists(LPCTSTR szPath) {
    DWORD dwAttrib = GetFileAttributes(szPath);
    return (dwAttrib != INVALID_FILE_ATTRIBUTES && !(dwAttrib & FILE_ATTRIBUTE_DIRECTORY));
}

void RestartComputer() {
    int msgboxID = MessageBox(hMainWnd, L"Are you sure you want to restart your computer?", L"Confirm Restart", MB_YESNO | MB_ICONQUESTION);

    if (msgboxID == IDYES) {
        HANDLE hToken;
        TOKEN_PRIVILEGES tkp;

        if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken)) {
            DisplayErrorMessage(L"OpenProcessToken failed.", L"Error");
            DebugPrint(L"OpenProcessToken failed. Error: %lu\n", GetLastError());
            return;
        }

        LookupPrivilegeValue(NULL, SE_SHUTDOWN_NAME, &tkp.Privileges[0].Luid);
        tkp.PrivilegeCount = 1;
        tkp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

        AdjustTokenPrivileges(hToken, FALSE, &tkp, 0, (PTOKEN_PRIVILEGES)NULL, 0);
        if (GetLastError() != ERROR_SUCCESS) {
             DisplayErrorMessage(L"AdjustTokenPrivileges failed.", L"Error");
             DebugPrint(L"AdjustTokenPrivileges failed. Error: %lu\n", GetLastError());
             return;
        }

        if (!ExitWindowsEx(EWX_REBOOT | EWX_FORCE, SHTDN_REASON_MAJOR_OPERATINGSYSTEM | SHTDN_REASON_MINOR_RECONFIG)) {
            DisplayErrorMessage(L"ExitWindowsEx failed.", L"Error");
              DebugPrint(L"ExitWindowsEx failed. Error: %lu\n", GetLastError());
            return;
        }
        CloseHandle(hToken);
    }
}

void OpenDisplaySettings() {
    HINSTANCE result = ShellExecute(NULL, L"open", L"ms-settings:display", NULL, NULL, SW_SHOWNORMAL);
    if ((intptr_t)result <= 32) {
        DisplayErrorMessage(L"Failed to open Display Settings.", L"Error");
         DebugPrint(L"Failed to open Display Settings.  ShellExecute returned: %p\n", result);
    }
}

void DisplayErrorMessage(const wchar_t* message, const wchar_t* caption) {
	MessageBox(NULL, message, caption, MB_OK | MB_ICONERROR);
}