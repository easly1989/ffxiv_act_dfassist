#pragma once

#include "wintoastlib.h"

#define DLLEXPORT extern "C" __declspec(dllexport)

using namespace WinToastLib;

typedef void(*ToastEventCallback)(int messageCode);

enum Results {
	ToastClicked,					// user clicked on the toast
	ToastDismissed,					// user dismissed the toast
	ToastTimeOut,					// toast timed out
	ToastHided,						// application hid the toast
	ToastNotActivated,				// toast was not activated
	ToastFailed,					// toast failed
	SystemNotSupported,				// system does not support toasts
	UnhandledOption,				// unhandled option
	MultipleTextNotSupported,		// multiple texts were provided
	InitializationFailure,			// toast notification manager initialization failure
	ToastNotLaunched,				// toast could not be launched,
	AwaitingInteractions			// when the toast is shown, awaiting for user interaction, timeone or an eventual error
};

DLLEXPORT void CreateToast_Text01(const wchar_t* appName,
	const wchar_t* appUserModelID,
	const wchar_t* toastMessage,
	ToastEventCallback eventCallback,
	const wchar_t* attribution = nullptr,
	int duration = 0,
	int audioFile = 0, 
	int audioOption = 0);

DLLEXPORT void CreateToast_Text02(const wchar_t* appName,
	const wchar_t* appUserModelID,
	const wchar_t* toastTitle,
	const wchar_t* toastMessage,
	ToastEventCallback eventCallback,
	const wchar_t* attribution = nullptr,
	bool wrapFirstLine = true,
	int duration = 0,
	int audioFile = 0, 
	int audioOption = 0);

DLLEXPORT void CreateToast_Text03(const wchar_t* appName,
	const wchar_t* appUserModelID,
	const wchar_t* toastTitle,
	const wchar_t* toastMessage,
	const wchar_t* toastAdditionalMessage,
	ToastEventCallback eventCallback,
	const wchar_t* attribution = nullptr,
	int duration = 0,
	int audioFile = 0, 
	int audioOption = 0);

DLLEXPORT void CreateToast_ImageAndText01(const wchar_t* appName,
	const wchar_t* appUserModelID,
	const wchar_t* toastMessage,
	const wchar_t* toastImagePath,
	ToastEventCallback eventCallback,
	const wchar_t* attribution = nullptr,
	int duration = 0,
	int audioFile = 0, 
	int audioOption = 0);

DLLEXPORT void CreateToast_ImageAndText02(const wchar_t* appName,
	const wchar_t* appUserModelID,
	const wchar_t* toastTitle,
	const wchar_t* toastMessage,
	const wchar_t* toastImagePath,
	ToastEventCallback eventCallback,
	const wchar_t* attribution = nullptr,
	bool wrapFirstLine = true,
	int duration = 0,
	int audioFile = 0, 
	int audioOption = 0);

DLLEXPORT void CreateToast_ImageAndText03(const wchar_t* appName,
	const wchar_t* appUserModelID,
	const wchar_t* toastTitle,
	const wchar_t* toastMessage,
	const wchar_t* toastAdditionalMessage,
	const wchar_t* toastImagePath,
	ToastEventCallback eventCallback,
	const wchar_t* attribution = nullptr,
	int duration = 0,
	int audioFile = 0, 
	int audioOption = 0);

// internal call, to generate every toast
void CreateToast(const wchar_t* appName,
	const wchar_t* appUserModelID,
	WinToastTemplate::WinToastTemplateType toastTemplate,
	ToastEventCallback eventCallback,
	const wchar_t* toastTitle = nullptr,
	const wchar_t* toastMessage = nullptr,
	const wchar_t* toastAdditionlMessage = nullptr,
	const wchar_t* toastImagePath = nullptr,
	const wchar_t* attribution = nullptr,
	WinToastTemplate::Duration duration = WinToastTemplate::Duration::System,
	WinToastTemplate::AudioSystemFile audioFile = WinToastTemplate::AudioSystemFile::DefaultSound,
	WinToastTemplate::AudioOption audioOption = WinToastTemplate::AudioOption::Default,
	INT64 expiration = -1,
	bool createShortcut = true);